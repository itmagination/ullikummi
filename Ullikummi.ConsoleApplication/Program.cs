using System;
using System.IO;

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

            Console.WriteLine("Ullikummi!");

        }
    }
}
