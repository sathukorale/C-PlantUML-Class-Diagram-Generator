using System.Text.RegularExpressions;

namespace PlantUMLCodeGeneratorGUI.classes
{
    static class RegExs
    { 
        public static Regex namespaceMatch = new Regex(@"(namespace)([ \t]+)([a-zA-Z0-9_:]*)([ \t\r\n]*{)");
        public static Regex classMatch = new Regex(@"([ \t]*)((class|struct|union)[ \t]+)([A-Z_0-9]+[ \t]+)?([a-zA-Z0-9_]+)(([ \t\r\n]*{)|(([ \t]+:)([ \t]*[a-zA-Z0-9_,<>:\* ]+([ \t\r\n]*))*{))");
        public static Regex methodMatch = new Regex(@"(([ \t]+)([a-zA-Z0-9_:,<>\*]+))+([ \t]*\()((([ \t]*)(([a-zA-Z0-9_:]+[&*]?[ \t]+[a-zA-Z0-9_]+)(,[ \t]*[a-zA-Z0-9_:]+[&*]?[ \t]+[a-zA-Z0-9_]+))|)(\)[ \t\r\n]*([{;=]|override|const)))");
        public static Regex multiSpaces = new Regex("[ ]{2,}", RegexOptions.None);
        public static Regex templateTypes = new Regex(@"(<)([a-zA-Z0-9_:\* ,]+)(>)");
        public static Regex bitFieldDeclaration = new Regex("[ \t]*:[ \t]*\\d+[ \t]*;?$");
        public static Regex regexArrayField = new Regex("([ \\t]*\\[[ \\t]*[0-9a-zA-Z_ \\t\\*\\-\\+\\%\\/]*[ \\t]*\\][ \\t]*)+");
        public static Regex regexTemplateStart = new Regex("^template[ \t]*<.*");
        public static Regex typedefStructMatch = new Regex(@"([ \t]*)(typedef)([ \t]+)(class|struct|union)([ \t\r\n]*{)");
        public static Regex validClassName = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]+");
    }
}