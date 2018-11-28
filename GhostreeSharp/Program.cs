using System;
using System.IO;

namespace Ghostree
{
    class Program
    {
        static void Main(string[] args) {
            if (args.Length <= 0 || string.IsNullOrEmpty(args[0])) {
                Console.WriteLine("NO INPUT");
                return;
            }

            if (File.Exists(args[0]) == false) {
                Console.WriteLine("INPUT NOT FOUND");
                return;
            }

            var input = File.ReadAllBytes(args[0]);
            //Ghostree.DebugRun(input);
            Ghostree.Run(input);
        }
    }
}
