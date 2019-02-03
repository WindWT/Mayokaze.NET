using System.Collections.Generic;
using System.Linq;

namespace NyaSharp
{
    class NyaProc
    {
        private List<string> @params;
        private dynamic body;
        private Dictionary<string, dynamic> closure;
        public NyaProc(List<string> @p, dynamic @b, dynamic @c) {
            @params = p;
            body = b;
            closure = c;
        }

        public dynamic Call(NyaObject env, List<NyaObject> arguments) {
            //It's a method call
            var e = new Env(env);

            for (var i = 0; i < @params.Count; i++) {
                var param = @params[i];
                if (e.locals.ContainsKey(param)) {
                    e.locals[param] = arguments[i];
                }
                else {
                    e.locals.Add(param, arguments[i]);
                }
            }
            return body.eval(e);
        }

        public dynamic Call(Env env, List<NyaObject> arguments) {
            //It's a lambda call
            env.locals = env.locals.Concat(closure)
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, g => g.First().Value);

            for (var i = 0; i < @params.Count; i++) {
                var param = @params[i];
                if (env.locals.ContainsKey(param)) {
                    env.locals[param] = arguments[i];
                }
                else {
                    env.locals.Add(param, arguments[i]);
                }
            }
            return body.eval(env);
        }
    }
}