using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NyaSharp
{
    class NyaObject : ICloneable
    {
        public NyaClass runtime_class { get; set; }
        public Dictionary<dynamic, dynamic> obj_attrs { get; set; }
        public dynamic ruby_value { get; set; }

        public NyaObject(NyaClass runtimeClass, Dictionary<dynamic, dynamic> attrs, dynamic rubyValue) {
            Init(runtimeClass, attrs, rubyValue);
        }

        public NyaObject(NyaClass runtimeClass)
        : this(runtimeClass, new Dictionary<dynamic, dynamic>(), null) {

        }
        public NyaObject(NyaClass runtimeClass, Dictionary<dynamic, dynamic> attrs)
            : this(runtimeClass, attrs, null) {

        }

        public NyaObject() {

        }

        protected void Init(NyaClass runtimeClass, Dictionary<dynamic, dynamic> attrs, dynamic rubyValue) {
            runtime_class = runtimeClass;
            obj_attrs = attrs;
            ruby_value = rubyValue;
        }

        protected void Init(NyaClass runtimeClass) {
            Init(runtimeClass, new Dictionary<dynamic, dynamic>(), null);
        }

        public dynamic this[dynamic index]
        {
            get => obj_attrs[index];
            set => obj_attrs[index] = value;
        }

        public void Call(dynamic method, dynamic env, dynamic arguments = null) {
            throw new NotImplementedException();
        }

        public object Clone() {
            var r = (NyaClass)runtime_class.Clone();
            var oa=obj_attrs.ToDictionary(entry => entry.Key,
                entry => entry.Value);
            var rv = ruby_value;    //not copied, may cause bug?
            return new NyaObject(r, oa, rv);
        }
    }
}
