using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PlantUMLCodeGeneratorGUI.classes
{
    class Namespace
    {
        public string Name;
        public Namespace ParentNamespace;
        public List<Namespace> ChildNamespaces;
        public Dictionary<string, Namespace> Namespaces;        
        public Dictionary<string, Class> Classes;

        public string FullName
        {
            get
            {
                var parentNamespace = this;
                var fullName = "";
                while (parentNamespace != null)
                {
                    fullName = parentNamespace.Name + (fullName.Length == 0 ? "" : "::") + fullName;
                    parentNamespace = parentNamespace.ParentNamespace;
                }

                return fullName;
            }
        }

        private Namespace(Namespace parentNamespace, string name)
        {
            Name = name;
            ParentNamespace = parentNamespace;

            if (ParentNamespace != null) ParentNamespace.AddChild(this);

            ChildNamespaces = new List<Namespace>();
            Namespaces = new Dictionary<string, Namespace>();
            Classes = new Dictionary<string, Class>();
        }

        private void AddChild(Namespace childNamespace)
        {
            ChildNamespaces.Add(childNamespace);
        }

        public void Set(string namespaceContent)
        {
            frmLoadingDialog.UpdateProgressText("Processing Class : " + FullName);
            var remainingContent = "";

            var matches = RegExs.namespaceMatch.Matches(namespaceContent);
            var lastMatchOffset = 0;

            foreach (Match match in matches)
            {
                var matchIndex = match.Index;
                if (matchIndex < lastMatchOffset) continue;

                remainingContent += namespaceContent.Substring(lastMatchOffset, match.Index - lastMatchOffset);

                var innerNamespaceContent = Processor.GetScopedContent(namespaceContent, ref matchIndex);
                var namespaceName = match.Groups[3].Value;
                lastMatchOffset = match.Index + match.Length + innerNamespaceContent.Length + 1;

                var namespaceObj = namespaceName.Contains("::") ? GetNamespace(namespaceName) : GetNamespace(this, namespaceName);
                namespaceObj.Set(innerNamespaceContent);

                if (Namespaces.ContainsKey(namespaceObj.Name) == false) Namespaces.Add(namespaceObj.Name, namespaceObj);
            }

            remainingContent += namespaceContent.Substring(lastMatchOffset, namespaceContent.Length - lastMatchOffset);
            
            remainingContent = Class.ProcessScopedContent(this, remainingContent);
        }

        public StringifiedContent ToString(Settings settings)
        {
            var namespaceContent = "";
            var classConnectivityContent = "";

            namespaceContent += "package " + Name + " {" + Environment.NewLine;

            var keys = Namespaces.Keys.ToArray();
            foreach (var key in keys)
            {
                var ns = Namespaces[key];
                var stringifiedContent = ns.ToString(settings);

                namespaceContent += "\t" + (stringifiedContent.ClassContent.Trim()).Replace(Environment.NewLine, Environment.NewLine + "\t").Trim();
                namespaceContent += Environment.NewLine;
                namespaceContent += Environment.NewLine;

                if (stringifiedContent.ClassConnectivityContent.Any())
                {
                    classConnectivityContent += stringifiedContent.ClassConnectivityContent;
                    classConnectivityContent += Environment.NewLine;
                    classConnectivityContent += Environment.NewLine;
                }
            }

            namespaceContent = namespaceContent.Trim();
            namespaceContent += Environment.NewLine;

            if (Classes.Any() && Namespaces.Any())
                namespaceContent += Environment.NewLine;

            for (int a = 0; a < Classes.Count; a++)
            {
                var cs = Classes[Classes.Keys.ToArray()[a]];
                var stringifiedContent = cs.ToString(settings);

                namespaceContent += "\t" + (stringifiedContent.ClassContent.Trim()).Replace(Environment.NewLine, Environment.NewLine + "\t").Trim();
                namespaceContent += Environment.NewLine;
                namespaceContent += Environment.NewLine;

                if (stringifiedContent.ClassConnectivityContent.Any())
                {
                    classConnectivityContent += stringifiedContent.ClassConnectivityContent;
                    classConnectivityContent += Environment.NewLine;
                    classConnectivityContent += Environment.NewLine;
                }
            }

            namespaceContent = namespaceContent.Trim();
            namespaceContent += Environment.NewLine;
            namespaceContent += "}";

            return new StringifiedContent(namespaceContent.Trim(), classConnectivityContent.Trim());
        }

        public static Namespace GetNamespace(Namespace parentNamespace, string namespaceName)
        {
            return GetNamespace(parentNamespace.FullName + "::" + namespaceName);
        }

        public static Namespace GetNamespace(string fullNameIncludingNamespace, bool justCheck = false)
        {
            var parentNamespace = DefaultNamespace;
            var namespaceNames = fullNameIncludingNamespace.Split(new []{"::"}, StringSplitOptions.None);
            foreach (var namespaceName in namespaceNames)
            {
                if (namespaceName == DefaultNamespace.Name) continue;

                Namespace foundNamespace;
                parentNamespace.Namespaces.TryGetValue(namespaceName, out foundNamespace);
                if (foundNamespace  == null)
                {
                    if (justCheck)
                    {
                        return null;
                    }

                    foundNamespace = new Namespace(parentNamespace, namespaceName);
                    parentNamespace.Namespaces.Add(namespaceName, foundNamespace);
                }

                parentNamespace = foundNamespace;
            }

            return parentNamespace;
        }

        public static void ResetDefaultNamespace()
        {
            DefaultNamespace = new Namespace(null, "DefaultNamespace");
        }

        public static Namespace DefaultNamespace = new Namespace(null, "DefaultNamespace");
    }
}