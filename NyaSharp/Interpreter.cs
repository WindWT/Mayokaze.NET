using System;
using System.Collections.Generic;
using System.Text;

namespace NyaSharp
{
    class Interpreter
    {
        private NyaParser parser;
        public Interpreter() {
            parser = new NyaParser();
        }

        public dynamic eval(string code) {
            return parser.DoParse(code).eval(RuntimeFactory.Runtime);
        }
    }
}
