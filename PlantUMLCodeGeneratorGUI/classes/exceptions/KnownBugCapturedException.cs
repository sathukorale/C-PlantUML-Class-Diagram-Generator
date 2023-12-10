using System;

namespace PlantUMLCodeGeneratorGUI.classes.exceptions
{
    class KnownBugCapturedException : Exception
    {
        public readonly int BugId;
        public KnownBugCapturedException(int bugId, string explanation = "") : base(explanation)
        {
            BugId = bugId;
        }
        public KnownBugCapturedException(Exception innerEx, int bugId, string explanation = "") : base(explanation, innerEx)
        {
            BugId = bugId;
        }
    }
}