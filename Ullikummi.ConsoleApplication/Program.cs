using System;
using System.IO;
using Ullikummi.CodeGeneration.Objective.Roslyn;

namespace Ullikummi
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var data = File.ReadAllText(args[0]);

            var dataReader = new DataReader.DataReader();

            var graph = dataReader.ReadData(data);

            var csharpCodeGenerator = new CSharpCodeGenerator();
            var visualBasicCodeGenerator = new VisualBasicCodeGenerator();

            Console.WriteLine();
            Console.WriteLine(csharpCodeGenerator.GenerateCode(graph));
            //Console.WriteLine();
            //Console.WriteLine(visualBasicCodeGenerator.GenerateCode(graph));
        }
    }
}
