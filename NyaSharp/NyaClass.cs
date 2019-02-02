using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NyaSharp
{
    class NyaClass : NyaObject
    {
        public Dictionary<string, dynamic> runtime_methods { get; }
        public Dictionary<dynamic, NyaProc> class_attrs { get; }
        public NyaObject base_class { get; set; }

        public NyaClass(NyaClass @base) {
            base_class = @base;

            //To solve chicken-egg problem
            NyaClass runtime_class = null;
            if (RuntimeFactory.Runtime != null) {
                runtime_class = (NyaClass) RuntimeFactory.Runtime["Class"];
                if (base_class == null) {
                    base_class= (NyaClass)RuntimeFactory.Runtime["Class"];
                }
            }
            else {
                runtime_class = null;
            }

            //Csharp do not allow init parent after child
            base.Init(runtime_class);
        }

        public NyaClass() : this(null) {

        }

        public dynamic Lookup(string method_name) {
            if (runtime_methods.ContainsKey(method_name)) {
                return runtime_methods[method_name];
            }
            if (base_class != RuntimeFactory.Runtime["Class"]) {
                return ((NyaClass)base_class).Lookup(method_name);
            }
            return null;
        }

        public NyaObject New() {
            var attrs = class_attrs.ToDictionary(entry => entry.Key,
                entry => entry.Value);
            return new NyaObject(this, attrs);
        }

        public NyaObject NewWithValue(dynamic value) {
            var attrs = class_attrs.ToDictionary(entry => entry.Key,
                entry => entry.Value);
            return new NyaObject(this, attrs, value);
        }
    }
}
