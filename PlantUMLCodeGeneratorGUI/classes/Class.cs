using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PlantUMLCodeGeneratorGUI.classes.utils;

namespace PlantUMLCodeGeneratorGUI.classes
{
    class Class
    {
        public Namespace ParentNamespace;
        public string Name { get; internal set; }
        public string FullName => ParentNamespace.FullName + "::" + Name;

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
            Class foundClass;
            if (namespaceObj.Classes.TryGetValue(className, out foundClass)) return foundClass;

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

            var correctedSegments = new List<string>();
            var concatenatedSegment = "";

            // This is a hack for the time being. Why we need this is described below
            foreach (var segment in segments)
            {
                concatenatedSegment += segment;

                // Just checking whether we have captured inner classes partially, because inner classes
                // themselves have public/private/protected sections
                if (concatenatedSegment.Count(c => c == '{') != concatenatedSegment.Count(c => c == '}')) continue;

                correctedSegments.Add(concatenatedSegment);
                concatenatedSegment = "";
            }

            return correctedSegments.Where(i => i.Length > 0).ToList();
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

            foreach (var key in scopeData.Keys.ToArray())
            {
                var identifiedClasses = new List<Class>();
                scopeData[key] = IdentifyInnerClasses(scopeData[key], ref identifiedClasses);

                scopes.First(i => i.Name == key).NestedClasses.AddRange(identifiedClasses);
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

                        if (methodContent.StartsWith("struct") || methodContent.StartsWith("union"))
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

                            var hasASpace = argument.Contains(' ');
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

                        if (nextToVisitIndex != indexOfSemiColon) continue; // This is very likely a variable

                        try
                        {
                            var memberContent = methodContent.Trim();
                            if (memberContent.Length == 0) continue;

                            var ignorablePrefixes = new [] { "friend class ", "typedef ", "using ", "struct ", "class ", "union " /* forward declarations */ };
                            if (ignorablePrefixes.Any(i => memberContent.StartsWith(i))) continue;

                            // In c++ we can define/declare/initialize variable like this
                            //
                            // int a;
                            // int a, b, c;
                            // int a = 10, b = 20, c = 30;
                            // int a = 10, b, c = 30;
                            // int a{10}, b{20}, c{30};
                            // int a(10), b(20), c(30);
                            // int a = {}, b = {}, c = {};
                            // auto a = 10, b = 20, c = 30;
                            // int a[] = {1, 2, 3};
                            // 
                            // We don't care about the values, just about the type and the variable names
                            //
                            // Including the above situation we can have members like this
                            // int a = 10;
                            // int a[3];
                            // int a[] = {1, 2, 3};
                            // int a[2][2];
                            // Something a;
                            // Something a[];
                            // Something<int> a;
                            // Something <int> a;
                            // long long a; // There can even be type modified like this for certain types
                            // unsigned long long a;
                            // 
                            // So we are going to lookup the first , after the >, so that we will get the first
                            // member in a series of member declarations like "Something a, b, c" or Something<int> a, b, c
                            // and from out of that we are going to remove the array part if any. So for ex:
                            // Something<int> a[1][2], b, c => Something<int> a; Then its the item after the last space, which
                            // is the member name, everything before this is the member type

                            var indexOfLastTemplateEnd = Math.Max(0, memberContent.IndexOf(">", StringComparison.Ordinal));
                            var indexOfFirstComma = memberContent.IndexOf(",", indexOfLastTemplateEnd, StringComparison.Ordinal);
                            if (indexOfFirstComma == -1) indexOfFirstComma = memberContent.Length;

                            var firstMember = memberContent.Substring(0, indexOfFirstComma).Trim();
                            var indexOfFirstEqSign = firstMember.LastIndexOf("=", StringComparison.Ordinal);
                            if (indexOfFirstEqSign == -1) indexOfFirstEqSign = firstMember.Length;

                            firstMember = firstMember.Substring(0, indexOfFirstEqSign).Trim();
                            firstMember = RegExs.bitFieldDeclaration.Replace(firstMember, "");

                            var indexOfLastSpace = firstMember.LastIndexOf(" ", StringComparison.Ordinal);
                            var memberType = firstMember.Substring(0, indexOfLastSpace).Trim();
                            memberContent = firstMember.Substring(indexOfLastSpace + 1).Trim();

                            var memberContentSegments = memberContent.Split(',');
                            foreach (var memberContentSegment in memberContentSegments)
                            {
                                // We are removing the : \d+; in bit fields
                                var modifiedMemberContentSegment = RegExs.bitFieldDeclaration.Replace(memberContentSegment, "");
                                var memberName = modifiedMemberContentSegment.Split('=').First().Trim();
                                var match = RegExs.regexArrayField.Match(memberName);

                                // So there can be variables/members in situations like bit fields where we
                                // don't have to mention variable/member names. We are going to ignore these fields
                                if (memberName.Length == 0) continue;

                                if (match.Success)
                                {
                                    memberName = memberName.Substring(0, match.Index).Trim();

                                    var regexArraySegment = new Regex("\\][ \t]*\\[");
                                    var arrayLengths = regexArraySegment.Replace(match.Value, ";").Replace("[", "").Replace("]", "").Split(';').Select(i => i.Trim()).ToArray();

                                    scope.Members.Add(new ArrayMember { Name = memberName, Type = memberType, OwnerClass = this, ArrayLengths = arrayLengths });
                                }
                                else
                                {
                                    scope.Members.Add(new Member { Name = memberName, Type = memberType, OwnerClass = this });
                                }
                            }

                            nextToVisitIndex = indexOfSemiColon;
                            if (indexOfAssignment != -1) visitedIndex = nextToVisitIndex + 1;
                        }
                        catch
                        {
                            // ignored
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

                foreach (var nestedClass in scope.NestedClasses)
                {
                    classRelationshipsContent += Name + " +-- " + nestedClass.Name + " : [ " + scope.Name + " ] Inner Class";
                }
            }

            return new StringifiedContent(classContent.Trim(), classRelationshipsContent.Trim());
        }

        private string IdentifyInnerClasses(string content, ref List<Class> innerClasses)
        {
            var remainingContent = "";
            var offset = 0;

            var classMatches = RegExs.classMatch.Matches(content);
            var previouslyCheckedSegments = new List<Segment>();
            
            foreach (Match classMatch in classMatches)
            {
                if (classMatch.Index < offset) continue;

                // This means this class match is inside another class
                if (previouslyCheckedSegments.Any(i => Utilities.IsInnerMatch(i, classMatch))) continue;

                remainingContent += content.Substring(offset, classMatch.Index - offset);

                var classEndOffset = classMatch.Index;
                var classContent = Processor.GetScopedContent(content, ref classEndOffset);
                var classObj = Class.GetClass(this.ParentNamespace, classMatch.Groups[5].Value);

                innerClasses.Add(classObj);

                // This is where our class potentially ends
                offset = classEndOffset;

                // We don't want the inner classes to be processed again
                previouslyCheckedSegments.Add(new Segment(classMatch.Index, classMatch.Index - classEndOffset));

                if (string.IsNullOrWhiteSpace(classContent) == false) classObj.Set(classContent);

                // Checking whether this class inherits others
                var parentStr = classMatch.Groups[8].Value;
                if (string.IsNullOrEmpty(parentStr)) continue;

                parentStr = parentStr.Replace("::", "[--]").Replace(":", "").Replace("[--]", "::").Replace("{", "").Trim();

                var parentNames = GetParents(parentStr);
                foreach (var parent in parentNames)
                {
                    var isPrivate = false;
                    var isPublic = false;

                    if (parent.Contains("private ")) isPrivate = true;
                    else if (parent.Contains("public ")) isPublic = true;
                    else if (parent.Contains("protected ") == false) isPrivate = true;

                    var parentName = parent.Replace("private", "").Replace("public", "").Replace("protected", "").Trim().Replace(" ", "").Replace("\t", "");

                    var indexOfColon = parentName.IndexOf("::", StringComparison.Ordinal);
                    var indexOfTemplateStart = parentName.IndexOf("<", StringComparison.Ordinal);

                    // Checking whether this is a situation similar to 'namespace::ClassName<OtherClassName>'
                    // and not this 'ClassName<namespace::OtherClassName>' and shortening this logic would
                    // drastically reduce readability
                    var hasNamespace = false;
                    if (indexOfColon != -1)
                        if (indexOfTemplateStart != -1)
                            hasNamespace = (indexOfColon < indexOfTemplateStart);
                        else
                            hasNamespace = true;

                    var parentClassObj = hasNamespace ? Class.GetClass(parentName) : Class.GetClass(this.ParentNamespace, parentName);
                    (isPrivate ? classObj.PrivateScope : (isPublic ? classObj.PublicScope : classObj.ProtectedScope)).Parents.Add(parentClassObj);
                }
            }

            remainingContent += content.Substring(offset);
            return remainingContent;
        }

        public static string ProcessScopedContent(Namespace parentNamespace, string fullContent)
        {
            var remainingContent = "";
            var offset = 0;

            var classMatches = RegExs.classMatch.Matches(fullContent);
            var previouslyCheckedSegments = new List<Segment>();

            foreach (Match classMatch in classMatches)
            {
                if (classMatch.Index < offset) continue;

                // This means this class match is inside another class
                if (previouslyCheckedSegments.Any(i => Utilities.IsInnerMatch(i, classMatch))) continue;

                remainingContent += fullContent.Substring(offset, classMatch.Index - offset);

                var matchOffset = classMatch.Index;
                var classContent = Processor.GetScopedContent(fullContent, ref matchOffset);
                var classObj = Class.GetClass(parentNamespace, classMatch.Groups[5].Value);

                // This is where our class potentially ends
                offset = matchOffset;
                classObj.Set(classContent);
                previouslyCheckedSegments.Add(new Segment(classMatch.Index, classMatch.Index - offset));
            }

            remainingContent += fullContent.Substring(offset);
            return remainingContent;
        }

        private static string[] GetParents(string parentString)
        {
            var regexMultipleSpaces = new Regex("[ \t]+");
            parentString = regexMultipleSpaces.Replace(parentString.Trim().Replace("\r", "").Replace("\n", ""), " ");

            var modifiedParentString = "";
            var lastVisitedIndex = 0;
            var matches = RegExs.templateTypes.Matches(parentString).OfType<Match>().ToArray();

            foreach (var match in matches)
            {
                modifiedParentString += parentString.Substring(lastVisitedIndex, match.Index - lastVisitedIndex);
                modifiedParentString += match.Value.Replace(",", "[COMMA]");
                lastVisitedIndex = (match.Index + match.Length);
            }

            if (lastVisitedIndex < parentString.Length - 1)
            {
                modifiedParentString += parentString.Substring(lastVisitedIndex, parentString.Length - lastVisitedIndex);
            }

            return modifiedParentString.Split(',').Select(i => i.Trim().Replace("[COMMA]", ", ")).ToArray();
        }
    }

    class Enum
    {
    }
}
