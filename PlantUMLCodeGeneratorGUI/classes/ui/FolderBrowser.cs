using System;
using System.Runtime.InteropServices;
using System.Text;

namespace PlantUMLCodeGeneratorGUI.classes.ui
{
    public class FolderBrowser
    {
        private delegate int BrowseCallbackProc(IntPtr hwnd, uint uMsg, IntPtr lParam, IntPtr lpData);

        [DllImport("shell32.dll")]
        static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

        [StructLayout(LayoutKind.Sequential)]
        struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName;
            public string lpszTitle;
            public uint ulFlags;
            public BrowseCallbackProc lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        private static int OnBrowseEvent(IntPtr hWnd, uint uMsg, IntPtr lParam, IntPtr lpData)
        {
            if (uMsg == 0x0001) // BFFM_INITIALIZED
            {
                SendMessage(hWnd, 0x0007, 0, lpData); // BFFM_SETSELECTION
            }
            return 0;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, IntPtr lParam);

        public static string ShowDialog(string prompt, string initialPath)
        {
            var bi = new BROWSEINFO
            {
                ulFlags = 0x40 | 0x10,
                lpszTitle = prompt,
                lpfn = OnBrowseEvent,
                lParam = Marshal.StringToHGlobalAuto(initialPath)
            };

            var pidl = SHBrowseForFolder(ref bi);
            Marshal.FreeHGlobal(bi.lParam);

            if (pidl == IntPtr.Zero) return null;

            var path = new StringBuilder(260);
            return SHGetPathFromIDList(pidl, path) ? path.ToString() : null;
        }
    }

}
