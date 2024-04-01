using FileTagDB;
using System.Runtime.InteropServices;

namespace SystemFilesTagger {
    public partial class ProgramGuide : Form {
        public ProgramGuide() {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        [DllImport("User32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            const int WM_NCPAINT = 0x85;
            if (m.Msg == WM_NCPAINT) {
                FileAndTagsManager.UseImmersiveDarkMode(m.HWnd, true);
            }
        }
    }
}
