using System.Collections.Generic;

namespace Ullikummi.Data
{
    public interface IConfigurable
    {
        IDictionary<string, string> Properties { get; }
    }
}
