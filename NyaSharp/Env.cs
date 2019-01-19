using System;
using System.Collections.Generic;
using System.Text;

namespace NyaSharp
{
    class Env
    {
        public NyaObject current_self { get; }
        public NyaClass current_class { get; }
        public Dictionary<dynamic, dynamic> locals { get; set; }

        private Dictionary<string, dynamic> constants = new Dictionary<string, dynamic>();

        public Env(NyaObject currentSelf) : this(currentSelf, currentSelf.runtime_class) {

        }
        public Env(NyaObject currentSelf, NyaClass currentClass) {
            locals = new Dictionary<dynamic, dynamic>();
            current_self = currentSelf;
            current_class = currentClass;
        }

        public NyaObject this[string name]
        {
            get => constants[name];
            set
            {
                if (constants.ContainsKey(name)) { constants[name] = value; }
                else {
                    constants.Add(name, value);
                }
            }
        }
    }
}
