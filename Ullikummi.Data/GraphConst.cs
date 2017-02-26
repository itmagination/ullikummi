namespace Ullikummi.Data
{
    public static class GraphConst
    {
        public class Metadata
        {
            public const string Name = "name";
            public const string Namespace = "namespace";
            public const string Using = "using";
            public const string Accessibility = "accessibility";

            public const string UsingNamespaceSeparator = "&";
        }

        public class ConnectionProperties
        {
            public const string Name = "name";
            public const string Parameters = "params";

            public const string ValuesSeparator = ";";
            public const string ComplexValueSeparator = "#";
        }
    }
}
