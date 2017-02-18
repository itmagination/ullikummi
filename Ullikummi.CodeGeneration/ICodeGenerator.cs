using Ullikummi.Data;

namespace Ullikummi.CodeGeneration
{
    public interface ICodeGenerator
    {
        string GenerateCode(Graph graph);
    }
}
