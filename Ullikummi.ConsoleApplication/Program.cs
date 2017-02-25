using System;
using System.IO;
using Ullikummi.CodeGeneration;

namespace Ullikummi
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Ullikummi!");

            var data = File.ReadAllText(args[0]);

            var dataReader = new DataReader.DataReader();

            var graph = dataReader.ReadData(data);

            var csharpCodeGenerator = new CSharpCodeGenerator();

            Console.WriteLine("Ullikummi!");

            Console.WriteLine(csharpCodeGenerator.GenerateCode(graph));
        }
    }
}
