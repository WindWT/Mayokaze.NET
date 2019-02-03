using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sprache;

namespace NyaSharp
{
    class NyaParser
    {
        class Program : List<dynamic>
        {

        }

        private Parser<NumberNode> NUM = Parse.Regex(new Regex(@"\d+(\.\d+)?(e[\+\-]?\d+)?", RegexOptions.IgnoreCase))
            .Select(n => new NumberNode(Convert.ToDecimal(n)));
        private Parser<BoolNode> BOOL = Parse.String("true").Text().Select(o => new BoolNode(true))
            .Or(Parse.String("false").Text().Select(o => new BoolNode(false)))
            .Or(Parse.String("nil").Text().Select(o => new BoolNode(null)))
            .Or(Parse.String("null").Text().Select(o => new BoolNode(null)));
        private Parser<string> SPACE = Parse.Regex(@"\A\s*", "space");

        private Parser<string> ID = Parse.Regex(@"\A[a-z]\w*", "id").Text();
        private Parser<string> CONST = Parse.Regex(@"\A[A-Z]\w*", "const").Text();
        private Parser<string> ATTR = Parse.Regex(@"\A@\w*", "attr").Text();
        private Parser<string> STRING = Parse.Regex(@"\A""(.*?)""", "string").Text();

        private Parser<char> BRA = Parse.Char('{').Token();
        private Parser<char> KET = Parse.Char('}').Token();
        private Parser<char> OPA = Parse.Char('(').Token();
        private Parser<char> CPA = Parse.Char(')').Token();
        private Parser<char> CALL_OP = Parse.Char('.').Token();

        private Parser<string> MUL_OP = Parse.Regex(@"[\*\/%]", "mul operator").Token();
        private Parser<string> ADD_OP = Parse.Regex(@"[\+\-]", "add operator").Token();
        private Parser<string> UN_OP = Parse.Regex(@"(\+\+|\-\-|!)", "unary operator").Token();
        private Parser<string> BOOL_OP = Parse.Regex(@"(&&|\|\|)", "bool operator").Token();
        private Parser<string> COMP_OP = Parse.Regex(@"[(\<=|\>=|==|!=|\<|\>)]", "compare operator").Token();

        private Parser<string> CLASS = Parse.String("class").Text().Token().Named("class");
        private Parser<string> DEF = Parse.String("def").Text().Token().Named("def");
        private Parser<string> IF = Parse.String("if").Text().Token().Named("if");
        private Parser<string> ELSE = Parse.String("else").Text().Token().Named("else");
        private Parser<string> WHILE = Parse.String("while").Text().Token().Named("while");
        private Parser<string> UNLESS = Parse.String("unless").Text().Token().Named("unless");

        private Parser<Nodes> parser;

        public NyaParser() {
            Parser<IOption<CallNode>> call2 = null;
            Parser<INode> expr = null;

            var terminator = (from a in Parse.Char(';').Token()
                              from b in SPACE
                              select new EmptyNode()).Named("terminator");
            var @var = ATTR.Select(n => (dynamic)new GetAttrNode(n))
                .Or(ID)
                .Or(CONST.Select(n => (dynamic)new GetConstantNode(n))).Named("var");
            var atom = NUM
                .Or((Parser<dynamic>)STRING)
                .Or(BOOL)
                .Or(@var)
                .Or(Parse.Ref(() => expr).Contained(OPA, CPA)).Named("atom");
            var alist = Parse.Ref(() => expr).DelimitedBy(Parse.Char(',').Token()).Optional().Contained(OPA, CPA)
                .Named("alist");

            var gslot = BOOL
                .Or((Parser<dynamic>)
                    from a1 in atom
                    from l in Parse.Char('[').Token()
                    from a2 in atom
                    from r in Parse.Char(']').Token()
                    select new CallNode((a1 is string ? new CallNode(null, a1, new Nodes()) : (CallNode)a1),
                        "get_slot", new Nodes() { a2 })).Named("gslot");

            var sslot = BOOL
                .Or((Parser<INode>)
                    from a1 in atom
                    from l in Parse.Char('[').Token()
                    from a2 in atom
                    from r in Parse.Char(']').Token()
                    from equal in Parse.Char('=').Token()
                    from d in Parse.Ref(() => expr)
                    select new CallNode((a1 is string ? new CallNode(null, a1, new Nodes()) : (CallNode)a1),
                        "set_slot", new Nodes() { a2, d })).Named("sslot");

            var call = gslot
                .Or(ID.SelectMany(a => alist, (a, b) => new { a, b })
                    .SelectMany(@t => Parse.Ref(() => call2), (@t, c) =>
                    {
                        if (c.IsDefined) {
                            var r = c.Get();
                            while (r.receiver != null) {
                                r = (CallNode)r.receiver;
                            }
                            r.receiver = new CallNode(null, @t.a, new Nodes(@t.b.GetOrDefault()));
                            return c.Get();
                        }
                        else {
                            return new CallNode(null, @t.a, new Nodes(@t.b.GetOrDefault()));
                        }
                    }))
                .Or(var.SelectMany(a => Parse.Ref(() => call2), (a, b) =>
                {
                    if (b.IsDefined) {
                        var r = b.Get();
                        while (r.receiver != null) {
                            r = (CallNode)r.receiver;
                        }
                        r.receiver = a;
                        return b.Get();
                    }
                    else {
                        return a;
                    }
                })).Or(atom).Named("call");

            call2 = CALL_OP.SelectMany(a => ID, (a, b) => new { a, b })
                  .SelectMany(@t => alist, (ab, c) => new { ab, c })
                  .SelectMany<dynamic, dynamic, CallNode>(@t => Parse.Ref(() => call2), (abc, d) =>
                  {
                      if (d.IsDefined) {
                          var r = d.Get();
                          if (r != null) {
                              while (r.receiver != null) {
                                  r = r.receiver;
                              }

                              r.receiver = new CallNode(null, abc.ab.b, abc.c);
                              return d;
                          }
                          else {
                              return new CallNode(null, abc.ab.b, abc.c);
                          }
                      }

                      return null;
                  }).Optional().Named("call2");

            var unary = (from a in call
                         from b in UN_OP
                         select new CallNode(a, b, new Nodes()))
                .Or(from a in UN_OP
                    from b in call
                    select new CallNode(b, a, new Nodes()))
                .Or(call).Named("unary");

            var term = unary.SelectMany(@t => (from op in MUL_OP
                                               from p in unary
                                               select new { op, p }).XMany(),
                (p, ps) =>
                {
                    INode result = p;
                    foreach (var opp in ps) {
                        result = new CallNode(result, opp.op, new Nodes() { opp.p });
                    }

                    return result;
                }).Named("term");

            var binary = term.SelectMany(@t => (from op in ADD_OP
                                                from p in term
                                                select new { op, p }).XMany(),
                (p, ps) =>
                {
                    INode result = p;
                    foreach (var opp in ps) {
                        result = new CallNode(result, opp.op, new Nodes() { opp.p });
                    }

                    return result;
                }).Named("binary");

            var comp = binary.SelectMany(@t => (from op in COMP_OP
                                                from p in binary
                                                select new { op, p }).XMany(),
                (p, ps) =>
                {
                    INode result = p;
                    foreach (var opp in ps) {
                        result = new CallNode(result, opp.op, new Nodes() { opp.p });
                    }

                    return result;
                }).Named("comp");

            var @bool = comp.SelectMany(@t => (from op in BOOL_OP
                                               from p in comp
                                               select new { op, p }).XMany(),
                (p, ps) =>
                {
                    INode result = p;
                    foreach (var opp in ps) {
                        result = new CallNode(result, opp.op, new Nodes() { opp.p });
                    }

                    return result;
                }).Named("bool");

            var assign = ((Parser<INode>)from a in ID
                                         from b in Parse.Char('=').Token()
                                         from c in Parse.Ref(() => expr)
                                         select new SetLocalNode(a, c))
                .Or(from a in CONST
                    from b in Parse.Char('=').Token()
                    from c in Parse.Ref(() => expr)
                    select new SetConstantNode(a, c))
                .Or(from a in ATTR
                    from b in Parse.Char('=').Token()
                    from c in Parse.Ref(() => expr)
                    select new SetAttrNode(a, c))
                .Or(sslot).Named("assign");

            var plist = ID.DelimitedBy(Parse.Char(',').Token()).Optional().Contained(OPA, CPA)
                .Named("plist");

            Parser<Nodes> block = null;

            var define = (from a in DEF
                          from b in ID
                          from c in plist
                          from d in Parse.Ref(() => block)
                          select new DefNode(b, c.GetOrDefault().ToList(), d)).Named("define");

            var clazz = (from a in CLASS
                         from b in CONST
                         from c in Parse.Ref(() => block)
                         select new ClassNode(b, c, null))
                .Or(from a in CLASS
                    from b in CONST
                    from c in Parse.String("<<").Text().Token()
                    from d in CONST
                    from e in Parse.Ref(() => block)
                    select new ClassNode(b, e, d)).Named("clazz");

            var lambda = (from a in Parse.Char('\\').Token()
                          from b in plist
                          from c in Parse.Ref(() => block)
                          select new LambdaNode(b, c)).Named("lambda");

            expr = assign.Or(@bool).Or(lambda).Named("expr");
            //expr = ((Parser<INode>)@bool).Or(lambda).Named("expr");

            var ifs = (from a in IF
                       from b in expr
                       from c in Parse.Ref(() => block)
                       from d in ELSE
                       from e in Parse.Ref(() => block)
                       select new IfNode(b, c, e))
                .Or(from a in IF
                    from b in expr
                    from c in Parse.Ref(() => block)
                    select new IfNode(b, c, null)).Named("ifs");

            var whiles = (from a in WHILE
                          from b in expr
                          from c in Parse.Ref(() => block)
                          select new WhileNode(b, c)).Named("whiles");

            var unlesss = (from a in expr
                           from b in UNLESS
                           from c in expr
                           select new UnlessNode(c, a)).Named("unlesss");

            Parser<INode> stmt = null;

            block = (from a in BRA
                     from b in Parse.Ref(() => stmt).XMany()
                     from c in KET
                     select new Nodes(b)).Named("block");

            stmt = (from a in expr
                    from b in terminator
                    select a).Or(ifs)
                .Or(whiles)
                .Or(unlesss)
                .Or(define)
                .Or(clazz)
                .Or(terminator).Named("stmt");

            var program = (from a in SPACE
                           from b in Parse.Ref(() => stmt).XMany()
                           select new Nodes(b)).Named("program");

            parser = program.End().Named("parser");

        }

        public Nodes DoParse(string source) {
            var result = parser.TryParse(source);
            return result.Value;
        }
    }
}
