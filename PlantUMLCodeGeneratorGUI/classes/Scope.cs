using System.Collections.Generic;

namespace PlantUMLCodeGeneratorGUI
{
    class Scope
    {
        public string Name;
        public List<Class> Parents { get; set; }
        public List<Member> Members { get; set; }
        public List<Method> Methods { get; set; }

        public Scope(string name)
        {
            Name = name;
            Parents = new List<Class>();
            Members = new List<Member>();
            Methods = new List<Method>();
        }
    }
}