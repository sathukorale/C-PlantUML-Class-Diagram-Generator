using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PlantUMLCodeGeneratorGUI
{
    class Utilities
    {
        public static List<Class> GetAllClasses(Namespace parentNamespace)
        {
            var classes = parentNamespace.Classes.Select(i => i.Value).ToList();

            foreach (var childNamespace in parentNamespace.ChildNamespaces)
            {
                classes.AddRange(GetAllClasses(childNamespace));
            }

            return classes;
        }

        public static List<Member> GetAllMembers(Class parentClass)
        {
            var members = new List<Member>();
            members.AddRange(parentClass.PublicScope.Members);
            members.AddRange(parentClass.ProtectedScope.Members);
            members.AddRange(parentClass.PrivateScope.Members);

            return members;
        }

        public static List<Method> GetAllMethods(Class parentClass)
        {
            var methods = new List<Method>();
            methods.AddRange(parentClass.PublicScope.Methods);
            methods.AddRange(parentClass.ProtectedScope.Methods);
            methods.AddRange(parentClass.PrivateScope.Methods);

            return methods;
        }

        public static bool IsContainer(string type)
        {
            var isContainerType = (type.StartsWith("std::list") || type.StartsWith("std::vector") || type.StartsWith("std::set") || type.StartsWith("std::queue") || type.StartsWith("std::dequeque"));
            return isContainerType;
        }
    }
}
