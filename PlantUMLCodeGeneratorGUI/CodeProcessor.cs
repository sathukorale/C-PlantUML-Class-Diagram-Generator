using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PlantUMLCodeGeneratorGUI.classes;
using PlantUMLCodeGeneratorGUI.classes.exceptions;

namespace PlantUMLCodeGeneratorGUI
{
    public static class CodeProcessor
    {
        public enum ScopeCharacterType
        {
            Parentheses = '(',
            AngleBrackets = '<',
            Braces = '{'
        }

        public static string Process(string fileContent, Settings settings, out Namespace defaultNamespace)
        {
            try
            {
                Namespace.ResetDefaultNamespace();

                Macros.Process(fileContent);
                fileContent = RemoveComments(fileContent);
                fileContent = Cleanup(fileContent);

                Namespace.DefaultNamespace.Set(fileContent);

                var stringifiedContent = Namespace.DefaultNamespace.ToString(settings);

                defaultNamespace = Namespace.DefaultNamespace;

                return stringifiedContent.ClassContent + Environment.NewLine + Environment.NewLine + stringifiedContent.ClassConnectivityContent;
            }
            catch (ScopeNotClosedException ex)
            {
                throw new KnownBugCapturedException(ex, 3, "If a detected scope hasn't been properly closed (reason yet to be determined) it can cause the logic to fail. In theory the code shouldn't compile.");
            }
        }

        private static string Cleanup(String content)
        {
            var regexEnumClass = new Regex("enum[ \t]+class ");
            return regexEnumClass.Replace(content, "enum ");
        }

        public static string GetScopedContent(string completeContent, ref int offset, ScopeCharacterType scopeCharacterType = ScopeCharacterType.Braces)
        {
            if (completeContent.Trim().Length == 0) return completeContent;

            var startCharacterType = "";
            var endCharacterType = "";

            if (scopeCharacterType == ScopeCharacterType.Braces)
            {
                startCharacterType = "{";
                endCharacterType = "}";
            }
            else if (scopeCharacterType == ScopeCharacterType.AngleBrackets)
            {
                startCharacterType = "<";
                endCharacterType = ">";
            }
            else if (scopeCharacterType == ScopeCharacterType.Parentheses)
            {
                startCharacterType = "(";
                endCharacterType = ")";
            }

            var methodContent = completeContent.Substring(offset);
            var indexofStartCharacterType = (methodContent.IndexOf(startCharacterType, StringComparison.Ordinal) + 1);

            offset += indexofStartCharacterType;
            methodContent = methodContent.Substring(indexofStartCharacterType);

            var numberOfClosingBrackets = 1;
            var indexToCheckFrom = 0;

            while (numberOfClosingBrackets > 0)
            {
                var indexOfClosingBracket = methodContent.IndexOf(endCharacterType, indexToCheckFrom, StringComparison.Ordinal);
                if (indexOfClosingBracket == -1) // since numberOfClosingBrackets > 0
                    throw new ScopeNotClosedException();

                var indexOfOpeningBracket = methodContent.IndexOf(startCharacterType, indexToCheckFrom, StringComparison.Ordinal);
                var tmpIndexToCheckFrom = indexToCheckFrom;

                if (indexOfOpeningBracket > indexOfClosingBracket || indexOfOpeningBracket == -1)
                {
                    indexToCheckFrom = indexOfClosingBracket + 1;
                    numberOfClosingBrackets--;
                }
                else if (indexOfClosingBracket > indexOfOpeningBracket)
                {
                    indexToCheckFrom = indexOfOpeningBracket + 1;
                    numberOfClosingBrackets++;
                }

                if (indexToCheckFrom < tmpIndexToCheckFrom)
                    throw new ScopeNotClosedException();
            }

            offset += (indexToCheckFrom);
            var scopeContent = methodContent.Substring(0, indexToCheckFrom - 1);

            return scopeContent;
        }

        public static string RemoveComments(string completeContent)
        {
            frmLoadingDialog.UpdateProgressText("Cleaning-up comments...");
            var lastIndex = 0;

            while (true)
            {
                var indexOfMultiLineComment = completeContent.IndexOf("/*", lastIndex, StringComparison.Ordinal);
                var indexOfOneLineComment = completeContent.IndexOf("//", lastIndex, StringComparison.Ordinal);

                // Checking whether there are any more comments.
                if (indexOfOneLineComment == -1 && indexOfMultiLineComment == -1) break;

                if ((indexOfMultiLineComment < indexOfOneLineComment || indexOfOneLineComment == -1) && indexOfMultiLineComment > 0)
                {
                    var indexOfMultiLineCommentClosing = completeContent.IndexOf("*/", indexOfMultiLineComment, StringComparison.Ordinal);

                    completeContent = completeContent.Substring(0, indexOfMultiLineComment) + completeContent.Substring(indexOfMultiLineCommentClosing + 2);
                    lastIndex = indexOfMultiLineComment;
                }
                else
                {
                    var indexOfCrlf = completeContent.IndexOf("\r\n", indexOfOneLineComment, StringComparison.Ordinal);
                    var indexofLf = completeContent.IndexOf("\n", indexOfOneLineComment, StringComparison.Ordinal);
                    var indexOfNext = completeContent.Length;

                    if (indexofLf != -1 && indexOfCrlf != -1)
                    {
                        indexOfNext = Math.Min(indexofLf + 1, indexOfCrlf + 2);
                    }
                    else if (indexofLf != -1 ^ indexOfCrlf - 1 == 0)
                    {
                        indexOfNext = Math.Max(indexofLf + 1, indexOfCrlf + 2);
                    }

                    completeContent = completeContent.Substring(0, indexOfOneLineComment) + completeContent.Substring(indexOfNext);
                    lastIndex = indexOfOneLineComment;
                }
            }
            
            return completeContent;
        }
          
        public static Class GetTypeAsClass(string type, Class ownerClass, bool justCheck = true)
        {
            var tmpType = type;
            if (tmpType.Contains("<") && tmpType.Contains(">"))
            {
                tmpType = tmpType.Substring(0, tmpType.IndexOf("<", StringComparison.Ordinal)).Trim();
            }

            tmpType = tmpType.Replace("*", "").Replace("&", "").Replace("const ", "");
            if (tmpType.Contains("::"))
            {
                return Class.GetClass(tmpType, justCheck);
            }

            return Class.GetClass(ownerClass.ParentNamespace, tmpType, justCheck) ?? Class.GetClass(Namespace.DefaultNamespace, tmpType, justCheck);
        }

        public static List<Class> GetContainedTypes(string type, Class ownerClass)
        {
            var indexOfOpeningAngledBracket = type.IndexOf("<", StringComparison.Ordinal);
            var indexOfClosingAngledBracket = type.LastIndexOf(">", StringComparison.Ordinal);
            
            if (indexOfOpeningAngledBracket == -1 || indexOfClosingAngledBracket == -1)
            {
                return new List<Class>();
            }

            var containedTypeNames = FindContainedTypes(type).SelectMany(i => i.GetAllTypes());
            var containedTypes = new List<Class>();

            foreach (var containedTypeName in containedTypeNames)
            {
                var containedType = GetTypeAsClass(containedTypeName, ownerClass, false);
                containedTypes.Add(containedType);
            }

            return containedTypes;
        }

        // To grab all the different types in type string. For example, the following:
        //
        // int, std::unique_ptr<Something>
        // int, std::string
        // int, Something<int, int, std::string>
        // int, Something<int, int, Something<int, int, std::string>>
        // Something<int, int, std::string>, int
        // Something<int, int, Something<int, int, std::string>>, int
        // Something<int, int, std::string>, Something<int, int, std::string>
        // Something<int, int, Something<int, int, std::string>>, Something<int, int, Something<int, int, std::string>>
        // 
        public static List<GenericType> FindContainedTypes(string typeStr)
        {
            var containedTypes = new List<GenericType>();
            var offset = 0;

            while (offset < typeStr.Length)
            {
                var indexOfComma = typeStr.IndexOf(",", offset, StringComparison.Ordinal);
                var indexOfOpeningAngledBracket = typeStr.IndexOf("<", offset, StringComparison.Ordinal);

                if (indexOfComma == -1 && indexOfOpeningAngledBracket == -1)
                {
                    var typeName = typeStr.Substring(offset).Trim();
                    if (IsValidTypeName(typeName)) containedTypes.Add(new GenericType(typeName));
                    break;
                }

                if ((indexOfComma < indexOfOpeningAngledBracket || indexOfOpeningAngledBracket == -1) && indexOfComma != -1)
                {
                    var typeName = typeStr.Substring(offset, indexOfComma - offset).Trim();
                    if (IsValidTypeName(typeName)) containedTypes.Add(new GenericType(typeName));
                    offset = indexOfComma + 1;
                }
                else if (indexOfOpeningAngledBracket < indexOfComma || indexOfComma == -1)
                {
                    var typeName = typeStr.Substring(offset, indexOfOpeningAngledBracket - offset).Trim();
                    var scopedContent = GetScopedContent(typeStr, ref indexOfOpeningAngledBracket, ScopeCharacterType.AngleBrackets);
                    var typesInScope = FindContainedTypes(scopedContent);

                    if (IsValidTypeName(typeName)) containedTypes.Add(new TemplateType(typeName, typesInScope));

                    offset = indexOfOpeningAngledBracket + 1;
                }
            }

            return containedTypes;
        }

        private static bool IsValidTypeName(string type)
        {
            type = type.Trim();
            return type.Length != 0 && RegExs.validClassName.IsMatch(type);
        }

        public class GenericType
        {
            public readonly string Type;

            public GenericType(string type)
            {
                Type = type;
            }

            public virtual List<string> GetAllTypes()
            {
                return new List<string>() { Type };
            }
        }

        public class TemplateType : GenericType
        {
            public readonly List<GenericType> TemplateTypes;
            public TemplateType(string type, List<GenericType> templateTypes) : base(type)
            {
                TemplateTypes = templateTypes;
            }

            public override List<string> GetAllTypes()
            {
                var types = base.GetAllTypes();
                foreach (var templateType in TemplateTypes)
                {
                    types.AddRange(templateType.GetAllTypes());
                }

                return types;
            }
        }
    }
}