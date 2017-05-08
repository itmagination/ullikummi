using System;
using System.IO;
using Ullikummi.CodeGeneration.Objective.Roslyn;

namespace Ullikummi
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("usage: Ullikummi.ConsoleApplication dataFile language");
                return;
            }

            var data = File.ReadAllText(args[0]);

            var dataReader = new DataReader.DataReader();

            var graph = dataReader.ReadData(data);

            var language = args[1];

            if(language.Equals("cs"))
            {
                var csharpCodeGenerator = new CSharpCodeGenerator();
                Console.WriteLine(csharpCodeGenerator.GenerateCode(graph));
                return;
            }
            if(language.Equals("vb"))
            {
                var visualBasicCodeGenerator = new VisualBasicCodeGenerator();
                Console.WriteLine(visualBasicCodeGenerator.GenerateCode(graph));
                return;
            }
        }
    }
}
