using System.Linq;

namespace PlantUMLCodeGeneratorGUI
{
    class ArrayMember : Member
    {
        // Using string because the lengths might refer to other variables, etc
        public string[] ArrayLengths { get; internal set; }

        public override string ToString()
        {
            return Name + " : " + Type + string.Concat(Enumerable.Repeat("[]", ArrayLengths.Length));
        }
    }
}