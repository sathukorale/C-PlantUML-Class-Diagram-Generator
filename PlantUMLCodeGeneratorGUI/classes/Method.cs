using System.Collections.Generic;
using System.Linq;

namespace PlantUMLCodeGeneratorGUI.classes
{
    class Method
    {
        public bool IsConstructor { get; internal set; }
        public bool IsVirtual { get; internal set; }
        public bool IsOverriding { get; internal set; }
        public bool IsConst { get; internal set; }
        public bool IsPureVirtual { get; internal set; }
        public bool IsOperatorOverload { get; internal set; }
        public Member ReturnType { get; internal set; }
        public string Name { get; internal set; }
        public List<Member> Arguments { get; internal set; }
        public Class Owner { get; internal set; }
        public bool IsDestructor { get; set; }

        public Method()
        {
            Arguments = new List<Member>();
        }

        public override string ToString()
        {
            var argumentStr = Arguments.Aggregate(string.Empty, (a, b) => a.ToString() + ", " + b.ToString());
            if (argumentStr.Length > 0)
            {
                argumentStr = argumentStr.Substring(1).Trim();
            }

            return Name + "( " + argumentStr + (argumentStr.Length == 0 ? "" : " ") + ((ReturnType == null || string.IsNullOrEmpty(ReturnType.Type)) ? ")" : (") : " + ReturnType.Type));
        }
    }
}