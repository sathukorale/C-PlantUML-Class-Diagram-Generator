using System.Collections.Generic;

namespace PlantUMLCodeGeneratorGUI.classes
{
    public class Scope
    {
        public string Name;
        public readonly List<Class> Parents;
        public readonly List<Member> Members;
        public readonly List<Method> Methods;
        public readonly List<Class> NestedClasses;

        public Scope(string name)
        {
            Name = name;
            Parents = new List<Class>();
            Members = new List<Member>();
            Methods = new List<Method>();
            NestedClasses = new List<Class>();
        }
    }
}