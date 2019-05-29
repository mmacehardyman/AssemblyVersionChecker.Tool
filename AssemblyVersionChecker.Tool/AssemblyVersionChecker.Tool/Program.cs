using AssemblyVersionChecker.Tool.Models.Application;
using CommandLine;
using System;


namespace AssemblyVersionChecker.Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(new Main().Run);

            Console.WriteLine("Done. Press any key to continue.");
            Console.ResetColor();
            Console.ReadKey();
        }
    }
}