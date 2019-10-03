using System;
using System.Threading;
using System.Windows.Forms;

namespace PlantUMLCodeGeneratorGUI
{
    public partial class frmLoadingDialog : Form
    {
        private static bool _cancel;
        private static bool _is_busy;
        private static string _message = "Please wait while the list is being populated";
        private static frmLoadingDialog _presentedDialog;

        private static object _current_execution_return_detail;

        public frmLoadingDialog()
        {
            InitializeComponent();
            prgLoadingProgress.Maximum = 100;
        }

        public static void ShowWindow()
        {
            ShowWindow("Please wait while the list is being populated");
        }

        public static void ShowWindow(string message, IWin32Window owner = null)
        {
            if (_presentedDialog == null || _presentedDialog.IsDisposed)
                _presentedDialog = new frmLoadingDialog();
            _message = message;
            _cancel = false;
            _presentedDialog.SetUp(false);
            if (owner == null)
            {
                _presentedDialog.Show();
            }
            else
            {
                _presentedDialog.ShowDialog(owner);
            }
        }

        public static object ShowWindow(Func<object, object> method, object param, string message, IWin32Window owner = null)
        {
            return ShowWindow(method, param, message, false, owner);
        }

        public static object ShowWindow(Func<object, object> method, object param, string message, bool showProgress, IWin32Window owner = null)
        {
            if (_is_busy)
                return null;

            if (_presentedDialog == null || _presentedDialog.IsDisposed)
                _presentedDialog = new frmLoadingDialog();

            _is_busy = true;
            _current_execution_return_detail = null;
            var t = new Thread(delegate(object o)
            {
                _current_execution_return_detail = method(o);
                _is_busy = false;
                try
                {
                    HideWindow();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            _message = message;
            _cancel = false;
            _presentedDialog.SetUp(showProgress);
            t.Start(param);
            if (_is_busy)
            {
                _presentedDialog.Hide();
                _presentedDialog.ShowDialog(owner);
            }
            return _current_execution_return_detail;
        }

        private void SetUp(bool showProgress)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object>(value =>
                {
                    lblMessage.Text = _message;
                    prgLoadingProgress.Value = 0;
                    prgLoadingProgress.Style = ProgressBarStyle.Marquee;
                }), showProgress);
            }
            else
            {
                lblMessage.Text = _message;
                prgLoadingProgress.Value = 0;
                prgLoadingProgress.Style = ProgressBarStyle.Marquee;
            }
        }

        private void UpdateProgress(int value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object>((objValue) => {
                    prgLoadingProgress.Style = ((int)objValue == 0) ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
                    prgLoadingProgress.Value = (int)objValue;
                    Application.DoEvents();
                }), value);
            }
            else
            {
                prgLoadingProgress.Style = (value == 0) ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
                prgLoadingProgress.Value = value;
                Application.DoEvents();
            }

        }

        private void UpatedText(string text)
        {
            if (string.IsNullOrWhiteSpace(text) == false)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action<object>((objValue) =>
                    {
                        lblMessage.Text = (string)objValue;
                    }), text);
                }
                else
                {
                    lblMessage.Text = text;
                }
            }

            Application.DoEvents();
        }

        public static void UpdateProgressPercentage(int value)
        { 
            if (_presentedDialog != null && _presentedDialog.IsDisposed == false)
            {
                if (_presentedDialog.InvokeRequired)
                {
                    _presentedDialog.BeginInvoke(new Action<object>(objectPassed => _presentedDialog.UpdateProgress((int)objectPassed)), value);
                }
                else
                {
                    _presentedDialog.UpdateProgress(value);
                }
            }
        }

        public static void UpdateProgressText(string text)
        {
            if (_presentedDialog != null && _presentedDialog.IsDisposed == false)
            {
                if (_presentedDialog.InvokeRequired)
                {
                    _presentedDialog.BeginInvoke(new Action<object>(objectPassed => _presentedDialog.UpatedText((string)objectPassed)), text);
                }
                else
                {
                    _presentedDialog.UpatedText(text);
                }
            }
        }

        public static bool GetStatus()
        {
            Application.DoEvents();
            return _cancel;
        }

        public static void HideWindow()
        {
            if (_presentedDialog == null || _presentedDialog.IsDisposed)
                return;

            if (_presentedDialog.InvokeRequired)
            {
                try
                {
                    _presentedDialog.BeginInvoke(new Action<object>(objectPassed => _presentedDialog.Close()), new object[] { null });
                }
                catch { }
            }
            else
            {
                _presentedDialog.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cancel = true;
            Close();
        }
    }
}
