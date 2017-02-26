using System.Collections.Generic;

namespace Ullikummi.CodeGeneration.Objective.Roslyn.Code
{
    internal class CodeFile
    {
        public IList<string> Usings { get; set; }
        public string Namespace { get; set; }
        public IList<Type> Types { get; set; }

        public CodeFile()
        {
            Usings = new List<string>();
            Types = new List<Type>();
        }
    }
}
