using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sprache;

namespace SukimoeSharp
{
    class Scheme
    {
        private Dictionary<string, dynamic> Env = new Dictionary<string, dynamic>();

        enum NodeType
        {
            ValNode,
            ListNode
        }

        private class Node
        {
            public NodeType type { get; private set; }
            public string value;
            public List<Node> array;

            public Node(NodeType t, string v = "", List<Node> a = null) {
                type = t;
                value = v;
                array = a ?? new List<Node>();
            }
            public dynamic eval(Dictionary<string, dynamic> env) {
                switch (type) {
                    case NodeType.ValNode: {
                            return Convert.ToInt32(value);
                        }
                    case NodeType.ListNode: {
                            switch (value) {
                                case "+": return array[0].eval(env) + array[1].eval(env);
                                case "-": return array[0].eval(env) - array[1].eval(env);
                                case "*": return array[0].eval(env) * array[1].eval(env);
                                case "/": return array[0].eval(env) / array[1].eval(env);
                                case "define": {
                                        if (array[1].type == NodeType.ValNode) {
                                            env[array[0].eval(env)] = array[1];
                                        }
                                        else {
                                            env[array[0].eval(env)] = array[1].eval(env);
                                        }
                                        break;
                                    }
                                case "lambda": //delta
                                {
                                        return new Closure() { f = array[1], p = array[0].eval(env), ctx = env };
                                    }
                                case "print": {
                                        Console.WriteLine(array[0].eval(env));
                                        break;
                                    }
                                case "": {
                                        //if (string.IsNullOrEmpty(array[0].value) == false) {
                                        //    var key = array[0].value;
                                        //    if (env.ContainsKey(key) && env[key] is Closure) {
                                        //        Closure val = env[key];
                                        //        //env = env.Concat(val.ctx);
                                        //        foreach (var kv in val.ctx) {
                                        //            env.Add(kv.Key, kv.Value);
                                        //        }
                                        //        env[val.p] = array[0];
                                        //        return val.f.eval(env);
                                        //    }
                                        //}
                                        dynamic result = null;
                                        array.ForEach(o => result = o.eval(env));
                                        return result;
                                    }
                                default: {
                                        if (env.ContainsKey(value)) {
                                            if (env[value] is Closure) {
                                                Closure val = env[value];
                                                //env = env.Concat(val.ctx);
                                                var capture = new Dictionary<string, dynamic>();
                                                foreach (var kv in env) {
                                                    capture.Add(kv.Key, kv.Value);
                                                }
                                                foreach (var kv in val.ctx) {
                                                    if (capture.ContainsKey(kv.Key)) {
                                                        capture[kv.Key] = kv.Value;
                                                    }
                                                    else {
                                                        capture.Add(kv.Key, kv.Value);
                                                    }
                                                }
                                                capture[val.p] = array[0];
                                                return val.f.eval(capture);
                                            }
                                            return env[value].eval(env);
                                        }
                                        return value;
                                    }

                            }
                            break;
                        }
                }

                return 0;
            }
        }

        struct Closure
        {
            public Node f;
            public dynamic p;
            public Dictionary<string, dynamic> ctx;
        }

        private Parser<Node> program;

        public Scheme() {
            Parser<Node> list = null;
            var integer = Parse.Number.Select(o => new Node(NodeType.ValNode) { value = o });
            var op = Parse.Char(c => "+-*/".Contains(c), "only +-*/");
            var id = Parse.Regex(new Regex(@"[^\s\(\)\[\]]+"))
                //Parse.Identifier(
                //Parse.CharExcept(c => new Regex(@"[\s\(\)\[\]]").IsMatch(c.ToString()), "not empty nor ()[]"),
                //Parse.CharExcept(c => new Regex(@"[\s\(\)\[\]]").IsMatch(c.ToString()), "not empty nor ()[]"))
                .Select(o =>
                    new Node(NodeType.ListNode, o));
            var value = integer.Or(Parse.Ref(() => list)).Or(id);
            var calc = from lc in Parse.Char('(')
                       from o in op.Token()
                       from value1 in value.Token()
                       from value2 in value.Token()
                       from rc in Parse.Char(')')
                       select new Node(NodeType.ListNode, o.ToString(), new List<Node>() { value1, value2 });
            var lambda = from lc in Parse.Char('(')
                         from word in Parse.String("lambda").Token()
                         from value1 in id.Token()
                         from value2 in Parse.Ref(() => list)
                         from rc in Parse.Char(')')
                         select new Node(NodeType.ListNode, string.Join("", word), new List<Node>() { value1, value2 });
            var invoke = from lc in Parse.Char('(')
                         from value1 in id.Token()
                         from value2 in value
                         from rc in Parse.Char(')')
                         select new Node(NodeType.ListNode, value1.value, new List<Node>() { value2 });
            var display = from lc in Parse.Char('(')
                          from word in Parse.String("print").Token()
                          from value2 in Parse.Ref(() => list)
                          from rc in Parse.Char(')')
                          select new Node(NodeType.ListNode, string.Join("", word), new List<Node>() { value2 });
            var define = from lc in Parse.Char('(')
                         from word in Parse.String("define").Token()
                         from value1 in id.Token()
                         from value2 in Parse.Ref(() => list)
                         from rc in Parse.Char(')')
                         select new Node(NodeType.ListNode, string.Join("", word), new List<Node>() { value1, value2 });
            list = calc.Or(display).Or(invoke).Or(lambda).Or(define).Or(integer).Or(id);

            program = list.Token().XMany().End().Select(o => new Node(NodeType.ListNode, "", o.ToList()));
        }

        public void Run(string source) {
            var result = program.Parse(source);
            result.eval(Env);
        }

        static readonly Parser<string> Identifier = Parse.Letter.Once()
            .SelectMany(first => Parse.LetterOrDigit.XOr(Parse.Char('-')).XOr(Parse.Char('_')).Many(),
                (first, rest) => new string(first.Concat(rest).ToArray()));
    }
}
