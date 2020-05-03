using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NppPluginForHC.PluginInfrastructure;

namespace NppPluginForHC
{
    internal static class Main
    {
        internal const string PluginName = "NppPluginForHC";
        static string iniFilePath = null;
        static bool someSetting = false;
        static frmMyDlg frmMyDlg = null;
        static int idMyDlg = -1;
        static Bitmap tbBmp = NppPluginForHC.Properties.Resources.star;
        static Bitmap tbBmp_tbTab = NppPluginForHC.Properties.Resources.star_bmp;
        static Icon tbIcon = null;

        private static readonly EventHandler OnLeftMouseClick = delegate
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                GoToDefinition();
            }
        };

        public static void OnNotification(ScNotification notification)
        {
            switch (notification.Header.Code)
            {
                case (uint) NppMsg.NPPN_READY:
                    MouseHook.Start();
                    break;
                case (uint) SciMsg.SCN_FOCUSIN:
                    MouseHook.RegisterListener(OnLeftMouseClick);
                    break;
                case (uint) SciMsg.SCN_FOCUSOUT:
                    MouseHook.CleanListeners();
                    break;
            }
        }

        internal static void OnShutdown()
        {
            MouseHook.CleanListeners();
            MouseHook.Stop();
            // Win32.WritePrivateProfileString("SomeSection", "SomeKey", someSetting ? "1" : "0", iniFilePath);
        }

        private static void GetVersion()
        {
            MessageBox.Show("Version: 0.0.1");
        }

        internal static void CommandMenuInit()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath)) Directory.CreateDirectory(iniFilePath);
            iniFilePath = Path.Combine(iniFilePath, PluginName + ".ini");
            someSetting = (Win32.GetPrivateProfileInt("SomeSection", "SomeKey", 0, iniFilePath) != 0);

            PluginBase.SetCommand(0, "Version", GetVersion, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(1, "MyDockableDialog", myDockableDialog);
            idMyDlg = 1;
            PluginBase.SetCommand(2, "GoToDefinition", GoToDefinition, new ShortcutKey(true, false, true, Keys.Enter));
        }


        internal static void myDockableDialog()
        {
            if (frmMyDlg == null)
            {
                frmMyDlg = new frmMyDlg();

                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(tbBmp_tbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = frmMyDlg.Handle;
                _nppTbData.pszName = "My dockable dialog";
                _nppTbData.dlgID = idMyDlg;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint) tbIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            }
            else
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_DMMSHOW, 0, frmMyDlg.Handle);
            }
        }

        private static void GoToDefinition()
        {
            StringBuilder sbWord = new StringBuilder(4096);

            if (Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTWORD, 4096, sbWord) != IntPtr.Zero)
            {
                // NppMsg.NPPM_GETCURRENTDOCINDEX - документа (tab pane)
                int currentLine = Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_GETCURRENTLINE, 0, 0).ToInt32();
                int currentColumn = Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_GETCURRENTCOLUMN, 0, 0).ToInt32();

                String word = sbWord.ToString();
                if (sbWord.Length > 0)
                {
                    MessageBox.Show("word: " + word);
                    return;
                }
            }

            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GRABFOCUS, 0, 0);
            System.Media.SystemSounds.Hand.Play();
        }

        #region " Layout Base "

        internal static void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = tbBmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint) NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }

        #endregion " Layout Base "

        #region " Logging "

        internal static void ErrorOut(Exception ex)
        {
            TRACE(ex.Message);
            try

            {
                using (TextWriter w = new StreamWriter("C:\\NppPluginForHC.ERROR.txt", true))
                {
                    w.WriteLine(string.Format(
                        "\n{0}-{1}-{2} {3}-{4}-{5}:\n" +
                        "====================",
                        DateTime.Now.Year,
                        DateTime.Now.Month.ToString("00"),
                        DateTime.Now.Day.ToString("00"),
                        DateTime.Now.Hour.ToString("00"),
                        DateTime.Now.Minute.ToString("00"),
                        DateTime.Now.Second.ToString("00")));
                    w.WriteLine(ex.Message);
                    w.WriteLine(ex.StackTrace);
                }

                MessageBox.Show("Owing to unfortunate circumstances an error with the following message occured:\n\n"
                                + "\"" + ex.Message + "\"\n\n"
                                + "Hence a logfile has been written to the NppPluginForHC folder.\n"
                                + "Please post its content in the forum, if you think it's worth being fixed.\n"
                                + "Sorry for the inconvenience.",
                    "NppPluginForHC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while attempting to write error logfile:\n" + e.Message,
                    "NppPluginForHC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [Conditional("TRACE_ALL")]
        internal static void TRACE(string msg)
        {
            try
            {
                if (string.IsNullOrEmpty(TRACEPATH))
                {
                    TRACEPATH = Environment.ExpandEnvironmentVariables("C:\\NppPluginForHC.TRACE.txt");
                    TRACEPID = Process.GetCurrentProcess().Id.ToString("X04");
                }

                using (TextWriter w = new StreamWriter(TRACEPATH, true))
                {
                    StackTrace stackTrace = new StackTrace();
                    MethodBase methodBase = stackTrace.GetFrame(1).GetMethod();
                    w.WriteLine(string.Format("{0}:{1:000} <{2}> {3}[{4}] {5}",
                        DateTime.Now.ToLongTimeString(), DateTime.Now.Millisecond, TRACEPID,
                        new String(' ', (stackTrace.FrameCount - 1) * 3), methodBase.Name, msg));
                }
            }
            catch (Exception ex)
            {
                if (!TRACEERRORSHOWN)
                {
                    TRACEERRORSHOWN = true;
                    MessageBox.Show("Error while attempting to write trace file:\n" + ex.Message,
                        "NppPluginForHC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        static string TRACEPATH, TRACEPID;
        static bool TRACEERRORSHOWN;

        #endregion Logging
    }
}