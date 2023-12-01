using System;

namespace PlantUMLCodeGeneratorGUI
{
    class KnownBugCapturedException : Exception
    {
        public readonly int BugId;
        public KnownBugCapturedException(int bugId, string explanation = "") : base(explanation)
        {
            BugId = bugId;
        }
    }
}