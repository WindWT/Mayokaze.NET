using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NyaSharp
{
    class NyaObject : ICloneable
    {
        public NyaClass runtime_class { get; set; }
        public Dictionary<dynamic, NyaProc> obj_attrs { get; set; }
        public dynamic ruby_value { get; set; }

        public NyaObject(NyaClass runtimeClass, Dictionary<dynamic, NyaProc> attrs, dynamic rubyValue) {
            Init(runtimeClass, attrs, rubyValue);
        }

        public NyaObject(NyaClass runtimeClass)
        : this(runtimeClass, new Dictionary<dynamic, NyaProc>(), null) {

        }
        public NyaObject(NyaClass runtimeClass, Dictionary<dynamic, NyaProc> attrs)
            : this(runtimeClass, attrs, null) {

        }

        public NyaObject() {

        }

        protected void Init(NyaClass runtimeClass, Dictionary<dynamic, NyaProc> attrs, dynamic rubyValue) {
            runtime_class = runtimeClass;
            obj_attrs = attrs;
            ruby_value = rubyValue;
        }

        protected void Init(NyaClass runtimeClass) {
            Init(runtimeClass, new Dictionary<dynamic, NyaProc>(), null);
        }

        public dynamic this[dynamic index]
        {
            get => obj_attrs[index];
            set => obj_attrs[index] = value;
        }

        public dynamic Call(string method, Env env, List<NyaObject> arguments = null) {
            if (arguments == null) {
                arguments = new List<NyaObject>();
            }

            var proc = obj_attrs[method];
            if (proc != null) {
                return proc.Call(this, arguments);
            }
            proc = runtime_class.Lookup(method);
            if (proc != null) {
                return proc.Call(this, arguments);
            }
            proc = env.locals[method];
            if (proc != null) {
                return proc.Call(this, arguments);
            }

            proc = ((NyaClass) RuntimeFactory.Runtime["Object"]).Lookup(method);
            if (proc != null) {
                return proc.Call(this, arguments);
            }

            throw new Exception($"Method not found: {method}");
        }

        public object Clone() {
            var r = (NyaClass)runtime_class.Clone();
            var oa = obj_attrs.ToDictionary(entry => entry.Key,
                entry => entry.Value);
            var rv = ruby_value;    //not copied, may cause bug?
            return new NyaObject(r, oa, rv);
        }
    }
}
