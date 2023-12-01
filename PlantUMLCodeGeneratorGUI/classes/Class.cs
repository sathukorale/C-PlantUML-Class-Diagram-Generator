using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PlantUMLCodeGeneratorGUI
{
    class Class
    {
        public Namespace ParentNamespace;
        public string Name { get; internal set; }
        public string FullName { get { return ParentNamespace.FullName + "::" + Name; } }

        public Scope PublicScope { get; internal set; }
        public Scope ProtectedScope { get; internal set; }
        public Scope PrivateScope { get; internal set; }

        public bool IsInterface { get; internal set; }

        private Class(Namespace parentNamespace, string name)
        {
            ParentNamespace = parentNamespace;
            Name = name;

            PublicScope = new Scope("public");
            ProtectedScope = new Scope("protected");
            PrivateScope = new Scope("private");
        }

        public static Class GetClass(Namespace namespaceObj, string className, bool justCheck = false)
        {
            Class foundClass = null;
            if (namespaceObj.Classes.TryGetValue(className, out foundClass) == false)
            {
                if (justCheck)
                {
                    foreach (var childNamespace in namespaceObj.ChildNamespaces)
                    {
                        foundClass = GetClass(childNamespace, className, true);
                        if (foundClass != null) break;
                    }
                }
                else
                {
                    foundClass = new Class(namespaceObj, className);
                    namespaceObj.Classes.Add(foundClass.Name, foundClass);
                }
            }

            return foundClass;
        }


        public static Class GetClass(string fullName, bool justCheck = false)
        {
            var namespaceName = "";
            var className = "";

            if (fullName.Contains("<"))
            {
                var lastIndexOfColons = fullName.LastIndexOf("::", StringComparison.Ordinal);
                var lastIndexOfTemplateStart = fullName.LastIndexOf("<", StringComparison.Ordinal);
                var section = fullName.Substring(0, lastIndexOfTemplateStart);

                if (lastIndexOfColons < lastIndexOfTemplateStart)
                {
                    var index = section.LastIndexOf("::", StringComparison.Ordinal);

                    namespaceName = section.Substring(0, index);
                    className = section.Substring(index + 2);
                }
                else
                {
                    namespaceName = "";
                    className = section;
                }
            }
            else
            {
                namespaceName = fullName.Substring(0, fullName.LastIndexOf("::", StringComparison.Ordinal));
                className = fullName.Substring(fullName.LastIndexOf("::", StringComparison.Ordinal) + 2);
            }

            return GetClass(namespaceName, className, justCheck);
        }

        public static Class GetClass(string namespaceName, string className, bool justCheck = false)
        {
            var namespaceObj = Namespace.GetNamespace(namespaceName, justCheck);
            if (justCheck && namespaceObj == null)
            {
                return null;
            }

            return GetClass(namespaceObj, className, justCheck);
        }

        private List<string> GetScopedSegments(string classContent, Scope[] scopes)
        {
            var segments = new List<string>();
            var lastIndexChecked = 0;

            while (true)
            {
                var matchingIndices = scopes.Select(i => new Regex(i.Name + "[ \t]*:").Match(classContent, lastIndexChecked + 1)).Where(i => i.Success).ToArray();
                if (matchingIndices.Any() == false) break;

                var orderedScopes = matchingIndices.OrderBy(i => i.Index);
                var scopeEnd = orderedScopes.First();

                segments.Add(classContent.Substring(lastIndexChecked, scopeEnd.Index - lastIndexChecked));

                lastIndexChecked = scopeEnd.Index;
            }

            segments.Add(classContent.Substring(lastIndexChecked, classContent.Length - lastIndexChecked - 1).Trim());

            return segments.Where(i => i.Length > 0).ToList();
        }

        public void Set(string classContent)
        {
            frmLoadingDialog.UpdateProgressText("Processing Class : " + ParentNamespace.FullName + Name);

            var scopes = new [] { PublicScope, PrivateScope, ProtectedScope };
            var segments = GetScopedSegments(classContent, scopes);

            var scopeData = new Dictionary<string, string>();
            foreach (var segment in segments)
            {
                var scopeName = "private";
                var matchingScopes = scopes.Where(i => segment.StartsWith(i.Name)).ToArray();
                if (matchingScopes.Any())
                {
                    scopeName = matchingScopes.First().Name;
                }

                if (scopeData.ContainsKey(scopeName) == false)
                    scopeData.Add(scopeName, "");

                scopeData[scopeName] += Environment.NewLine + segment.Substring(matchingScopes.Any() ? (segment.IndexOf(":", StringComparison.Ordinal) + 1) : 0);
            }

            foreach (var scope in scopes)
            {
                var scopeContent = "";
                if (scopeData.TryGetValue(scope.Name, out scopeContent) == false) continue;
                
                var visitedIndex = 0;

                while (visitedIndex < scopeContent.Length - 1)
                {
                    var indexOfCurlyBracket = scopeContent.IndexOf("{", visitedIndex + 1, StringComparison.Ordinal);
                    var indexOfSemiColon = scopeContent.IndexOf(";", visitedIndex + 1, StringComparison.Ordinal);
                    var indexOfAssignment = scopeContent.IndexOf("=", visitedIndex + 1, StringComparison.Ordinal);

                    var orderedIndices = new[] {indexOfSemiColon, indexOfCurlyBracket}.Where(i => i != -1).ToArray();
                    if (orderedIndices.Any() == false) break;

                    var nextToVisitIndex = orderedIndices.OrderBy(i => i).First();
                    if (nextToVisitIndex == -1) break;

                    var methodContent = scopeContent.Substring(visitedIndex, nextToVisitIndex - visitedIndex).Trim();

                    if (nextToVisitIndex == indexOfCurlyBracket)
                    {
                        var offset = indexOfCurlyBracket;
                        var scopedContent = Processor.GetScopedContent(scopeContent, ref offset);
                        visitedIndex = offset;

                        if (methodContent.StartsWith("struct"))
                        {
                            var indexOfInnerSemiColon = scopeContent.IndexOf(";", visitedIndex, StringComparison.Ordinal) + 1;
                            
                            if (indexOfInnerSemiColon != -1)
                                visitedIndex = indexOfInnerSemiColon;
                        }
                    }
                    else
                    {
                        visitedIndex = nextToVisitIndex + 1;
                    }

                    methodContent = RegExs.multiSpaces.Replace(methodContent.Replace("\r\n", "").Replace("\n", "").Replace("\t", " "), " ");

                    var argStartBracket = methodContent.IndexOf("(", StringComparison.Ordinal);
                    var argEndBracket = methodContent.IndexOf(")", StringComparison.Ordinal);

                    if (argStartBracket != -1 && argEndBracket != -1) // This is a method
                    {
                        if (methodContent.Contains(" operator()") || methodContent.Contains(" operator ()"))
                        {
                            argStartBracket = methodContent.IndexOf("(", argEndBracket + 1, StringComparison.Ordinal);
                            argEndBracket = methodContent.IndexOf(")", argEndBracket + 1, StringComparison.Ordinal);
                        }

                        var indexOfColon = methodContent.IndexOf(":", argEndBracket, StringComparison.Ordinal);
                        if (indexOfColon != -1 && indexOfColon < nextToVisitIndex)
                        {
                            methodContent = methodContent.Substring(0, indexOfColon).Trim();
                        }

                        var isOperatorOverload = methodContent.Contains(" operator");
                        var methodNameAndStuff = methodContent.Substring(0, argStartBracket).Replace("virtual", "").Replace("inline", "").Replace("static", "").Replace("~ ", "~").Replace("operator", "").Trim();
                        var indexOfSpace = methodNameAndStuff.LastIndexOf(" ", StringComparison.Ordinal);
                        var methodName = methodNameAndStuff.Substring(indexOfSpace + 1).Trim();
                        var methodReturn = indexOfSpace != -1 ? methodNameAndStuff.Substring(0, indexOfSpace).Trim() : null;
                        var arguments = methodContent.Substring(argStartBracket + 1, argEndBracket - argStartBracket - 1).Split(',');

                        var method = new Method
                        {
                            Name = methodName,
                            ReturnType = string.IsNullOrEmpty(methodReturn) ? null : new Member() { Type = methodReturn },
                            IsConstructor = methodName == Name,
                            IsDestructor = methodName == "~" + Name,
                            IsVirtual = methodContent.StartsWith("virtual "),
                            IsConst = methodContent.EndsWith("const"),
                            IsOverriding = methodContent.EndsWith("override"),
                            IsPureVirtual = methodContent.Replace(" ", "").EndsWith("=0"),
                            IsOperatorOverload = isOperatorOverload,
                            Owner = this
                        };

                        if (method.ReturnType == null && (method.IsConstructor == false && method.IsDestructor == false)) continue;

                        foreach (string tmpArgument in arguments)
                        {
                            var argument = tmpArgument;

                            if (string.IsNullOrEmpty(argument)) continue;

                            if (argument.Contains("=")) argument = argument.Substring(0, argument.IndexOf("=", StringComparison.Ordinal)).Trim();

                            var hasASpace    = argument.Contains(' ');
                            var argumentName = hasASpace ? argument.Split(' ').Last() : "";
                            var argumentType = hasASpace ? argument.Substring(0, argument.LastIndexOf(" ", StringComparison.Ordinal)).Trim() : argument;

                            method.Arguments.Add(new Member { Name = argumentName, Type = argumentType, OwnerClass = this, OwnerMethod = method });
                        }

                        scope.Methods.Add(method);
                    }
                    else if (argEndBracket == -1 && argStartBracket == -1) // This might be a variable
                    {
                        if (indexOfAssignment != -1) // Most likely someone is initializing this variable.
                            nextToVisitIndex = indexOfSemiColon;

                        if (nextToVisitIndex == indexOfSemiColon) // This is definitely a variable
                        {
                            try
                            {
                                var memberContent = methodContent.Trim();
                                if (memberContent.Length == 0) continue;

                                if (memberContent.StartsWith("friend class") || memberContent.StartsWith("typedef")) continue;

                                memberContent = memberContent.Split('=').First().Trim();
                                var indexOfLastSpace = memberContent.LastIndexOf(" ", StringComparison.Ordinal);
                                var memberType = memberContent.Substring(0, indexOfLastSpace).Trim();
                                var memberName = memberContent.Substring(indexOfLastSpace + 1).Trim();

                                var regexArray = new Regex("(\\[\\d+\\])+$");
                                var match = regexArray.Match(memberName);

                                if (match.Success)
                                {
                                    memberName = memberName.Substring(0, match.Index).Trim();

                                    var regexArraySegment = new Regex("\\][ \t]*\\[");
                                    var arrayLengths = regexArraySegment.Replace(match.Value, ";").Replace("[", "").Replace("]", "").Split(';').Select(i => int.Parse(i.Trim())).ToArray();

                                    scope.Members.Add(new ArrayMember { Name = memberName, Type = memberType, OwnerClass = this, ArrayLengths = arrayLengths });
                                }
                                else
                                {
                                    scope.Members.Add(new Member { Name = memberName, Type = memberType, OwnerClass = this });
                                }

                                nextToVisitIndex = indexOfSemiColon;
                                if (indexOfAssignment != -1) visitedIndex = nextToVisitIndex + 1;
                            }
                            catch { }
                        }
                    }
                }
            }

            IsInterface = scopes.Any(i => i.Methods.Any(ii => ii.IsPureVirtual));
        }

        public StringifiedContent ToString(Settings settings)
        {
            var scopes = new List<Scope> { PublicScope };

            if (settings.IncludeProtectedMethodsAndMembers) scopes.Add(ProtectedScope);
            if (settings.IncludePrivateMethodsAndMembers)   scopes.Add(PrivateScope);

            string classContent = "";
            string classRelationshipsContent = "";

            classContent += (IsInterface ? "interface " : "class ") + Name + " {" + Environment.NewLine;

            foreach (var scope in scopes)
            {
                var prefix = ((scope == PublicScope) ? "+" : (scope == ProtectedScope ? "#" : "-"));

                foreach (var scopeMember in scope.Members)
                {
                    classContent += "\t" + prefix + scopeMember + Environment.NewLine;
                }

                foreach (var scopeMethod in scope.Methods.Where(i => (settings.IncludeOverridenMethods || i.IsOverriding == false) && i.IsOperatorOverload == false))
                {
                    classContent += "\t" + prefix + scopeMethod + Environment.NewLine;
                }
            }

            classContent += "}" + Environment.NewLine;

            if (scopes.Any()) 
                classContent += Environment.NewLine;

            foreach (var scope in scopes)
            {
                foreach (var scopeParent in scope.Parents)
                {
                    if (scopeParent.Name.Contains("<"))
                    {
                        var containerParentType = GetClass(ParentNamespace, scopeParent.Name.Substring(0, scopeParent.Name.IndexOf("<", StringComparison.Ordinal)));
                        var containedParentTypes = Processor.GetContainedTypes(scopeParent.Name, this);
                        foreach (var parentType in containedParentTypes)
                        {
                            classRelationshipsContent += Name + " ..|> " + parentType.Name + Environment.NewLine;
                        }

                        classRelationshipsContent += Name + " ..|> " + containerParentType.Name + Environment.NewLine; 
                    }
                    else
                    {
                        classRelationshipsContent += Name + " ..|> " + scopeParent.Name + Environment.NewLine; 
                    }
                }

                foreach (var referredType in scope.Members.Select(i => i.GetTypeAsClass()).Where(i => i != null).GroupBy(i => i))
                {
                    if (referredType.Any() == false) continue;

                    classRelationshipsContent += Name + " \"1\" *-- \"" + referredType.Count() + "\" " + referredType.Key.Name + Environment.NewLine;
                }

                foreach (var referredType in scope.Members.Select(i => i.GetContainedTypes()).SelectMany(i => i).Where(i => i != null).GroupBy(i => i))
                {
                    if (referredType.Any() == false) continue;

                    classRelationshipsContent += Name + " \"1\" -- \"" + referredType.Count() + "\" " + referredType.Key.Name + Environment.NewLine;
                }
            }

            return new StringifiedContent(classContent.Trim(), classRelationshipsContent.Trim());
        }
    }

    class Enum
    {
    }
}
