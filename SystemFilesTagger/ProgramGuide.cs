using FileTagDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystemFilesTagger {
    public partial class ProgramGuide : Form {
        public ProgramGuide() {
            InitializeComponent();
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
