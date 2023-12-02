using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PlantUMLCodeGeneratorGUI.classes;

namespace PlantUMLCodeGeneratorGUI
{
    static class Processor
    {
        public enum ScopeCharacterType
        {
            Parentheses = '(',
            AngleBrackets = '<',
            Braces = '{'
        }

        public static string Process(string fileContent, Settings settings, out Namespace defaultNamespace)
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

        private static string Cleanup(String content)
        {
            var regexEnumClass = new Regex("enum[ \t]+class ");
            return regexEnumClass.Replace(content, "enum ");
        }

        public static string GetScopedContent(String completeContent, ref int offset, ScopeCharacterType scopeCharacterType = ScopeCharacterType.Braces)
        {
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
                var indexOfOpeningBracket = methodContent.IndexOf(startCharacterType, indexToCheckFrom, StringComparison.Ordinal);
                var indexOfClosingBracket = methodContent.IndexOf(endCharacterType, indexToCheckFrom, StringComparison.Ordinal);
                var tmpIndexToCheckFrom = indexToCheckFrom;

                if (indexOfOpeningBracket > indexOfClosingBracket || indexOfOpeningBracket == -1)
                {
                    indexToCheckFrom = indexOfClosingBracket + 1;
                    numberOfClosingBrackets--;
                }
                else
                {
                    indexToCheckFrom = indexOfOpeningBracket + 1;
                    numberOfClosingBrackets++;
                }

                if (indexToCheckFrom < tmpIndexToCheckFrom)
                    throw new KnownBugCapturedException(3, "If a detected scope hasn't been properly closed (reason yet to be determined) it can cause the logic to fail. In theory the code shouldn't compile.");
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

            var containedTypes = new List<Class>();
            var containedTypesStr = type;
            containedTypesStr = containedTypesStr.Substring(indexOfOpeningAngledBracket + 1);
            containedTypesStr = containedTypesStr.Substring(0, indexOfClosingAngledBracket - indexOfOpeningAngledBracket - 1);

            var containedTypeStrings = containedTypesStr.Split(',').Select(i => i.Trim());
            foreach (var containedTypeStr in containedTypeStrings)
            {
                var currentType = containedTypeStr;
                var indexOfAngleBracket = containedTypeStr.IndexOf("<", StringComparison.Ordinal);
                if (indexOfAngleBracket != -1)
                {
                    containedTypes.AddRange(GetContainedTypes(currentType, ownerClass));
                    currentType = containedTypeStr.Substring(0, indexOfAngleBracket);
                }

                var foundType = GetTypeAsClass(currentType, ownerClass, false);
                if (foundType != null)
                {
                    containedTypes.Add(foundType);
                }
            }

            return containedTypes;
        }
    }
}