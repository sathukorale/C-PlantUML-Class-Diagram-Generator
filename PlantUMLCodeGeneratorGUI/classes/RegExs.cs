using System.Text.RegularExpressions;

namespace PlantUMLCodeGeneratorGUI
{
    static class RegExs
    { 
        public static Regex namespaceMatch = new Regex(@"(namespace)([ \t]+)([a-zA-Z0-9_:]+)([ \t\r\n]*{)");
        public static Regex classMatch = new Regex(@"([ \t]*)((class|struct)[ \t]+)([A-Z_0-9]+[ \t]+)?([a-zA-Z0-9_]+)(([ \t\r\n]*{)|(([ \t]+:)([ \t]*[a-zA-Z0-9_,<>:\* ]+([ \t\r\n]*))*{))");
        public static Regex methodMatch = new Regex(@"(([ \t]+)([a-zA-Z0-9_:,<>\*]+))+([ \t]*\()((([ \t]*)(([a-zA-Z0-9_:]+[&*]?[ \t]+[a-zA-Z0-9_]+)(,[ \t]*[a-zA-Z0-9_:]+[&*]?[ \t]+[a-zA-Z0-9_]+))|)(\)[ \t\r\n]*([{;=]|override|const)))");
        public static Regex multiSpaces = new Regex("[ ]{2,}", RegexOptions.None);
        public static Regex templateTypes = new Regex(@"(<)([a-zA-Z0-9_:\* ,]+)(>)");
    }
}