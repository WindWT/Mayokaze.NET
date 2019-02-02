using System;
using System.IO;
using System.Linq;

namespace NyaSharp
{
    class Program
    {
        static void Main(string[] args) {

            var interpreter = new Interpreter();

            if (args.Any()) {
                interpreter.eval(File.ReadAllText(args[0]));
            }
            else {
                Console.WriteLine("Nya nya nyaruko: ");
                while (true) {
                    Console.Write(">> ");
                    var line=Console.ReadLine();
                    var value = interpreter.eval(line);
                    Console.WriteLine(value.ruby_value.inspect);
                }
            }
        }
    }
}
