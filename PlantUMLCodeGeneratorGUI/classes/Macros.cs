using System;
using System.Collections.Generic;

namespace PlantUMLCodeGeneratorGUI.classes
{
    static class Macros
    {
        public static void Process(String content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            frmLoadingDialog.UpdateProgressText("Replacing macros...");

            var indexOfMacro = 0;
            var macroContents = new List<string>();
            while ((indexOfMacro = content.IndexOf("#define", indexOfMacro + 1, StringComparison.Ordinal)) != -1)
            {
                var indexOfEndOfMacro = indexOfMacro + 6;

                while (true)
                {
                    var indexOfEndOfLine = content.IndexOf("\n", indexOfEndOfMacro, StringComparison.Ordinal);
                    var indexOfEndOfBSlash = content.IndexOf("\\", indexOfEndOfMacro, StringComparison.Ordinal);

                    if (indexOfEndOfLine == -1)
                    {
                        macroContents.Add(content.Substring(indexOfMacro, content.Length - indexOfMacro - 1).Replace("\\", ""));
                        indexOfMacro = content.Length;
                        break;
                    }

                    if (indexOfEndOfBSlash == -1 || indexOfEndOfBSlash > indexOfEndOfLine)
                    {
                        macroContents.Add(content.Substring(indexOfMacro, indexOfEndOfLine - indexOfMacro).Replace("\\", ""));
                        indexOfMacro = indexOfEndOfLine;
                        break;
                    }

                    indexOfEndOfMacro = indexOfEndOfLine + 1;
                }
            }

            foreach (var macroContent in macroContents)
            {
                
            }
        }
    }
}