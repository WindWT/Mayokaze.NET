using System;
using System.IO;
using Sprache;

namespace SukimoeSharp
{
    class Program
    {
        static void Main(string[] args) {
            if (args.Length <= 0 || string.IsNullOrEmpty(args[0])) {
                Console.WriteLine("need a scheme file name");
                return;
            }

            if (File.Exists(args[0]) == false) {
                Console.WriteLine("INPUT NOT FOUND");
                return;
            }

            var input = File.ReadAllText(args[0]);
            //Ghostree.DebugRun(input);
            new Scheme().Run(input);

            Console.ReadLine();
        }
    }
}
