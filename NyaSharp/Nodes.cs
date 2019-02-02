using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NyaSharp
{
    interface INode
    {
        dynamic eval(Env env);
    }

    class Nodes : List<INode>
    {
        public Nodes() {

        }
        public Nodes(IEnumerable<INode> ns) {
            foreach (var node in ns) {
                this.Append(node);
            }
        }
        public dynamic eval(Env env) {
            dynamic return_value = null;
            this.ForEach(node => { return_value = node.eval(env); });
            return return_value || RuntimeFactory.Runtime["nil"];
        }
    }

    class EmptyNode : INode
    {
        public dynamic eval(Env env) {
            return null;
        }
    }

    class NumberNode : INode
    {
        private dynamic value;

        public NumberNode(dynamic v) {
            value = v;
        }
        public dynamic eval(Env env) {
            return ((NyaClass)RuntimeFactory.Runtime["Number"]).NewWithValue(value);
        }
    }

    class StringNode : INode
    {
        private dynamic value;

        public StringNode(dynamic v) {
            value = v;
        }
        public dynamic eval(Env env) {
            return ((NyaClass)RuntimeFactory.Runtime["String"]).NewWithValue(value);
        }
    }

    class BoolNode : INode
    {
        private dynamic value;

        public BoolNode(dynamic v) {
            value = v;
        }
        public dynamic eval(Env env) {
            return ((NyaClass)RuntimeFactory.Runtime["BoolClass"]).NewWithValue(value);
        }
    }

    class CallNode : INode
    {
        public CallNode receiver;
        private dynamic method;
        private Nodes arguments;  //todo

        public CallNode(CallNode r, dynamic m, Nodes a) {
            receiver = r;
            method = m;
            arguments = a;
        }
        public dynamic eval(Env env) {
            if (receiver == null && env.locals[method] != null && arguments.Any() == false) {
                return env.locals[method];
            }
            else {
                var value = env.current_self;
                if (receiver != null) {
                    value = receiver.eval(env);
                }

                var eval_arguments = arguments.Select(arg => (NyaObject)arg.eval(env)).ToList();
                return value.Call(method, env, eval_arguments);  //Lambda call needs the env,while method call needs the receiver information
            }
        }
    }

    class GetAttrNode : INode
    {
        private dynamic name;

        public GetAttrNode(dynamic n) {
            name = n;
        }
        public dynamic eval(Env env) {
            if (env.current_self is NyaClass) {
                env.current_class.class_attrs[name] = RuntimeFactory.Runtime["nil"];
                return env.current_class.class_attrs[name];
            }
            else {
                return env.current_self[name];
            }
        }
    }

    class SetAttrNode : INode
    {
        private dynamic name;
        private dynamic value;

        public SetAttrNode(dynamic n, dynamic v) {
            name = n;
            value = v;
        }
        public dynamic eval(Env env) {
            if (env.current_self is NyaClass) {
                env.current_class.class_attrs[name] = value.eval(env);
                return env.current_class.class_attrs[name];
            }
            else {
                env.current_self[name] = value.eval(env);
                return env.current_self[name];
            }
        }
    }

    class GetConstantNode : INode
    {
        private string name;

        public GetConstantNode(string n) {
            name = n;
        }
        public dynamic eval(Env env) {
            return env[name];
        }
    }

    class SetConstantNode : INode
    {
        private string name;
        private INode value;

        public SetConstantNode(string n, INode v) {
            name = n;
            value = v;
        }
        public dynamic eval(Env env) {
            env[name] = value.eval(env);
            return env[name];
        }
    }

    class SetLocalNode : INode
    {
        private string name;
        private INode value;

        public SetLocalNode(string n, INode v) {
            name = n;
            value = v;
        }
        public dynamic eval(Env env) {
            env.locals[name] = value.eval(env);
            return env.locals[name];
        }
    }

    class DefNode : INode
    {
        private string name;
        private dynamic @params;
        private dynamic body;

        public DefNode(string n, dynamic p, dynamic b) {
            name = n;
            @params = p;
            body = b;
        }
        public dynamic eval(Env env) {
            var method = new NyaProc(@params, body, null);
            env.current_class.runtime_methods[name] = method;
            return env.current_class.runtime_methods[name];
        }
    }

    class LambdaNode : INode
    {
        private dynamic @params;
        private dynamic body;

        public LambdaNode(dynamic p, dynamic b) {
            @params = p;
            body = b;
        }
        public dynamic eval(Env env) {
            return new NyaProc(@params, body, env.locals);
        }
    }

    class ClassNode : INode
    {
        private string name;
        private dynamic body;
        private dynamic @base;

        public ClassNode(dynamic n, dynamic bo, dynamic ba) {
            name = n;
            body = bo;
            @base = ba;
        }
        public dynamic eval(Env env) {
            //class reopen
            NyaClass nya_class = (NyaClass)env[name];
            if (nya_class != null) {
                if (@base != null && env[@base] != null) {
                    nya_class = new NyaClass(env[@base]);
                }
                else {
                    nya_class = new NyaClass();
                }
                env[name] = nya_class;
            }
            //Evaluate the body of class, everything defined inside the class will have the class env
            var class_env = new Env(nya_class, nya_class);
            body.eval(class_env);
            return nya_class;
        }
    }

    class IfNode : INode
    {
        private dynamic condition;
        private dynamic body;
        private dynamic else_body;

        public IfNode(dynamic c, dynamic b, dynamic eb) {
            condition = c;
            body = b;
            else_body = eb;
        }
        public dynamic eval(Env env) {
            if (condition.eval(env).ruby_value) {
                return body.eval(env);
            }
            else {
                if (else_body != null) { return else_body.eval(env); }
                else { return null; }
            }
        }
    }

    class WhileNode : INode
    {
        private dynamic condition;
        private dynamic body;

        public WhileNode(dynamic c, dynamic b) {
            condition = c;
            body = b;
        }
        public dynamic eval(Env env) {
            dynamic value = null;
            while (condition.eval(env).ruby_value) {
                value = body.eval(env);
            }
            return value;
        }
    }

    class UnlessNode : INode
    {
        private dynamic condition;
        private dynamic body;

        public UnlessNode(dynamic c, dynamic b) {
            condition = c;
            body = b;
        }
        public dynamic eval(Env env) {
            //csharp has no unless
            if (condition.eval(env).ruby_value == false) {
                return body.eval(env);
            }
            return null;
        }
    }
}
