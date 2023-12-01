using System;

namespace PlantUMLCodeGeneratorGUI
{
    internal class KnownBugCapturedException : Exception
    {
        public readonly int BugId;
        public KnownBugCapturedException(int bugId, string explanation = "") : base(explanation)
        {
            BugId = bugId;
        }
    }
}