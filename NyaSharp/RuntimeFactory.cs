using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NyaSharp
{
    class RuntimeFactory
    {
        public static Env Runtime;

        static RuntimeFactory() {
            var nya_class = new NyaClass();
            nya_class.runtime_class = nya_class;
            nya_class.base_class = nya_class;
            var object_class = new NyaClass()
            {
                runtime_class = nya_class,
                base_class = nya_class
            };
            Runtime = new Env(object_class.New());

            Runtime["Class"] = nya_class;
            Runtime["Object"] = object_class;
            Runtime["Number"] = new NyaClass();
            Runtime["String"] = new NyaClass();
            Runtime["BoolClass"] = new NyaClass();

            Runtime["false"] = ((NyaClass)Runtime["BoolClass"]).NewWithValue(false);
            Runtime["true"] = ((NyaClass)Runtime["BoolClass"]).NewWithValue(true);
            Runtime["nil"] = ((NyaClass)Runtime["BoolClass"]).NewWithValue(null);
            Runtime["null"] = ((NyaClass)Runtime["BoolClass"]).NewWithValue(null);    //add for sharp
            Runtime["Kernel"] = ((NyaClass)Runtime["Object"]).New();

            ((NyaClass)Runtime["Class"]).runtime_methods["new"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) => receiver.New());

            ((NyaClass)Runtime["Object"]).runtime_methods["get_slot"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     return receiver[arguments[0].ruby_value];
                 });
            ((NyaClass)Runtime["Object"]).runtime_methods["set_slot"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     receiver[arguments[0].ruby_value] = arguments[1];
                     return receiver[arguments[0].ruby_value];
                 });
            ((NyaClass) Runtime["Object"]).runtime_methods["clone"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                {
                    return (NyaObject) receiver.Clone();
                });

            ((NyaClass)Runtime["Number"]).runtime_methods["+"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     var result = receiver.ruby_value + arguments.First().ruby_value;
                     return ((NyaClass)Runtime["Number"]).NewWithValue(result);
                 });
            ((NyaClass)Runtime["Number"]).runtime_methods["-"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     var result = receiver.ruby_value - arguments.First().ruby_value;
                     return ((NyaClass)Runtime["Number"]).NewWithValue(result);
                 });
            ((NyaClass)Runtime["Number"]).runtime_methods["*"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     var result = receiver.ruby_value * arguments.First().ruby_value;
                     return ((NyaClass)Runtime["Number"]).NewWithValue(result);
                 });
            ((NyaClass)Runtime["Number"]).runtime_methods["/"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     var result = receiver.ruby_value / arguments.First().ruby_value;
                     return ((NyaClass)Runtime["Number"]).NewWithValue(result);
                 });
            ((NyaClass)Runtime["Number"]).runtime_methods["%"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     var result = receiver.ruby_value % arguments.First().ruby_value;
                     return ((NyaClass)Runtime["Number"]).NewWithValue(result);
                 });
            ((NyaClass)Runtime["Number"]).runtime_methods[">"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     var result = receiver.ruby_value > arguments.First().ruby_value;
                     return result ? Runtime["true"] : Runtime["false"];
                 });
            ((NyaClass)Runtime["Number"]).runtime_methods["<"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     var result = receiver.ruby_value < arguments.First().ruby_value;
                     return result ? Runtime["true"] : Runtime["false"];
                 });
            ((NyaClass)Runtime["Number"]).runtime_methods["=="] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     var result = receiver.ruby_value == arguments.First().ruby_value;
                     return result ? Runtime["true"] : Runtime["false"];
                 });

            ((NyaClass)Runtime["BoolClass"]).runtime_methods["&&"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     var result = receiver.ruby_value && arguments.First().ruby_value;
                     return result ? Runtime["true"] : Runtime["false"];
                 });
            ((NyaClass)Runtime["BoolClass"]).runtime_methods["||"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     var result = receiver.ruby_value || arguments.First().ruby_value;
                     return result ? Runtime["true"] : Runtime["false"];
                 });

            ((NyaClass)Runtime["Object"]).runtime_methods["print"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                     {
                         Console.WriteLine(arguments.First().ruby_value);
                         return Runtime["null"];
                     });
            ((NyaClass)Runtime["Object"]).runtime_methods["read"] =
                new Func<NyaClass, IList<NyaObject>, NyaObject>((receiver, arguments) =>
                 {
                     var input = Console.ReadLine();
                     return ((NyaClass)Runtime["String"]).NewWithValue(input);
                 });
        }
    }
}
