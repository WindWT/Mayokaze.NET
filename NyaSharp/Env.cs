using System;
using System.Collections.Generic;
using System.Text;

namespace NyaSharp
{
    class Env
    {
        public NyaObject current_self { get; }
        public NyaClass current_class { get; }
        public Dictionary<string, dynamic> locals { get; set; }

        private Dictionary<string, NyaObject> constants = new Dictionary<string, NyaObject>();

        public Env(NyaObject currentSelf) : this(currentSelf, currentSelf.runtime_class) {

        }
        public Env(NyaObject currentSelf, NyaClass currentClass) {
            locals = new Dictionary<string, dynamic>();
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
