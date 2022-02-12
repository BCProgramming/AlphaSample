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

namespace sampleLayered
{
    public partial class layeredsample : Form
    {
        const byte AC_SRC_ALPHA = 1;
        const int ULW_ALPHA = 2;
        const int WM_NCHITTEST = 0x84;
        const int HTCAPTION = 2;
        const int WS_EX_LAYERED = 0x00080000;
        [StructLayout(LayoutKind.Sequential)]
        private struct _Size
        {
            internal int cx;
            internal int cy;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct _Point
        {
            internal int x;
            internal int y;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;

            public BLENDFUNCTION(byte op, byte flags, byte alpha, byte format)
            {
                BlendOp = op;
                BlendFlags = flags;
                SourceConstantAlpha = alpha;
                AlphaFormat = format;
            }
        }
        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst,
   ref _Point pptDst, ref _Size psize, IntPtr hdcSrc, ref _Point pptSrc, uint crKey,
   [In] ref BLENDFUNCTION pblend, uint dwFlags);
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC([In] IntPtr hdc);

        public layeredsample(Bitmap useBitmap)
        {
            TopMost = true;
            ShowInTaskbar = false;
            this.Size = useBitmap.Size;
            StartPosition = FormStartPosition.CenterScreen;
            this.Show();
            SetAlphaBitmap(useBitmap);
            this.BackColor = Color.Red;
            Click += Layeredsample_Click;
            //InitializeComponent();
        }

        private void Layeredsample_Click(object sender, EventArgs e)
        {
            Close();
        }

      
        private void SetAlphaBitmap(Bitmap src)
        {
            IntPtr ScreenDC = GetDC(IntPtr.Zero);
            IntPtr mDC = CreateCompatibleDC(ScreenDC);
            IntPtr hBmp = IntPtr.Zero, hOldBmp = IntPtr.Zero;
            try
            {
                hBmp = src.GetHbitmap(Color.FromArgb(0));
                hOldBmp = SelectObject(mDC, hBmp);
                _Size s = new _Size() { cx = src.Width, cy = src.Height };
                _Point srcLocation = new _Point() { x = 0, y = 0 };
                _Point newLoc = new _Point() { x = Left, y = Top };
                BLENDFUNCTION blend = new BLENDFUNCTION();
                blend.BlendOp = 0;
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = 255;
                blend.AlphaFormat = AC_SRC_ALPHA;
                UpdateLayeredWindow(Handle, ScreenDC, ref newLoc, ref s, mDC, ref srcLocation, 0, ref blend, ULW_ALPHA);
            }
            finally
            {
                if(hBmp!=IntPtr.Zero)
                {
                    SelectObject(mDC, hOldBmp);
                    DeleteObject(hBmp);
                    DeleteDC(mDC);
                }
            }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                var resultparams = base.CreateParams;
                resultparams.ExStyle |= WS_EX_LAYERED;
                return resultparams;
            }
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)HTCAPTION;
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}
