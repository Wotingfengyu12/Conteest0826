using DevExpress.DirectX.Common;
using DevExpress.Utils.Drawing;
using DevExpress.XtraPrinting.Native.LayoutAdjustment;
using DevExpress.XtraSplashScreen;
using DevExpress.XtraWaitForm;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CQC.ConTest
{
    public partial class Loading : WaitForm
    {
        #region 字段

        private event Action CancelEvent;

        #endregion
        public Loading()
        {
            InitializeComponent();
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 20;
        }

        #region Overrides

        public override void SetCaption(string caption)
        {
            base.SetCaption(caption);
            this.lbl_Caption.Text = caption;
        }
        public override void SetDescription(string description)
        {
            base.SetDescription(description);
            this.lbl_Description.Text = description;
        }
        public override void ProcessCommand(Enum cmd, object arg)
        {
            if (true == cmd.HasFlag(WaitFormCommand.EndExecuteEvent))
            {
                CancelEvent += arg as Action;
            }
            else if (true == cmd.HasFlag(WaitFormCommand.Loading))
            {
                //Rectangle bounds = (Rectangle)arg;
                //this.Bounds = bounds;

                //this.tableLayoutPanel1.Location = new Point(
                //    (bounds.Width - this.tableLayoutPanel1.Bounds.Width) / 2,
                //    (bounds.Height - this.tableLayoutPanel1.Bounds.Height) / 2);
            }
            else
            {
                base.ProcessCommand(cmd, arg);
            }
        }


        #endregion

        public enum WaitFormCommand
        {
            None,
            Loading,
            EndExecuteEvent,
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            if (null != CancelEvent)
            {
                CancelEvent();
            }
        }
    }

    class CustomOverlayPainter : OverlayWindowPainterBase
    {

        #region 字段

        private HOOK m_Hook = null;         // 鼠标事件钩子
        private int m_ProgressBarOffset = 0;        // 进度条偏移
        private Rectangle m_ButtonRectangle = new Rectangle(500, 500, 75, 30);

        #endregion

        #region 委托

        public delegate void CancelWaitingHandler();
        public event CancelWaitingHandler CancelWaitingEvent;

        #endregion

        #region 属性

        /// <summary>
        /// 显示文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// 执行取消操作状态，true取消，false未执行取消
        /// </summary>
        public bool HasCanceled { get; private set; }

        #endregion

        // Defines the string’s font.
        public readonly Font drawFont;

        public CustomOverlayPainter(Form form)
        {
            drawFont = new Font("Tahoma", 10);
            m_Hook = new HOOK();
            m_Hook.MouseClickEvent += MouseClick;
        }

        public CustomOverlayPainter(string str, Form form)
            : this(form)
        {
            Text = str;
        }

        #region 绘制

        protected override void Draw(OverlayWindowCustomDrawContext context)
        {
            m_Hook.StartHook(context.DrawArgs.ViewInfo.Owner.Handle);
            //The Handled event parameter should be set to true. 
            //to disable the default drawing algorithm. 
            context.Handled = true;
            //Provides access to the drawing surface. 
            GraphicsCache cache = context.DrawArgs.Cache;
            //Adjust the TextRenderingHint option
            //to improve the image quality.
            cache.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            //Overlapped control bounds. 
            Rectangle bounds = context.DrawArgs.Bounds;
            //Draws the default background. 
            context.DrawBackground();

            Rectangle backgroundRectangle = new Rectangle();
            backgroundRectangle.Height = 300;
            backgroundRectangle.Width = 670;
            backgroundRectangle.X = (bounds.Width - backgroundRectangle.Width) / 2;
            backgroundRectangle.Y = (bounds.Height - backgroundRectangle.Height) / 2;

            DrawImage(cache, backgroundRectangle);

            Brush drawBrush = Brushes.White;
            cache.Graphics.DrawString(this.Caption, new Font("Tahoma", 14, FontStyle.Bold), drawBrush, new RectangleF(
                new Point(backgroundRectangle.X + 10, backgroundRectangle.Bottom - 80 - 30 - 10),
                new SizeF(600, 40)), new StringFormat()
                {
                    LineAlignment = StringAlignment.Center
                });

            cache.Graphics.DrawString(this.Text, new Font("Tahoma", 9), drawBrush, new RectangleF(
                new Point(backgroundRectangle.X + 10, backgroundRectangle.Bottom - 40 - 30 - 10),
                new SizeF(600, 40)), new StringFormat()
                {
                    LineAlignment = StringAlignment.Center
                });

            m_ButtonRectangle = new Rectangle(backgroundRectangle.Right - 75 - 10, backgroundRectangle.Bottom - 30 - 10, 75, 30);
            DrawButton(cache, m_ButtonRectangle, "Cancel");

            DrawProgressBar(cache, new Rectangle(
                backgroundRectangle.X + 10, backgroundRectangle.Bottom - 30 - 10,
                backgroundRectangle.Width - 75 - 10 - 10 - 10, 30));
        }

        private static void DrawImage(GraphicsCache cache, Rectangle rectangle)
        {
            try
            {
                using (Image img = new Bitmap(@"Resources/SplashScreenNew.png"))
                {
                    cache.Graphics.DrawImageUnscaledAndClipped(img, rectangle);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DrawButton(GraphicsCache cache, Rectangle rectangle, string text)
        {
            try
            {
                cache.Graphics.FillPath(Brushes.White, GetRoundRectangle(rectangle));

                DrawString(cache, rectangle, text);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DrawString(GraphicsCache cache, Rectangle bounds, string text)
        {
            try
            {
                String drawString = text;
                Font drawFont = new Font("Tahoma", 9);
                //Get the system's black brush.
                Brush drawBrush = Brushes.Black;
                //Calculate the size of the message string.
                SizeF textSize = cache.CalcTextSize(drawString, drawFont);
                //A point that specifies the upper-left corner of the rectangle where the string will be drawn.
                PointF drawPoint = new PointF(
                    bounds.Left + bounds.Width / 2 - textSize.Width / 2,
                    bounds.Top + bounds.Height / 2 - textSize.Height / 2
                    );
                //Draw the string on the screen.
                cache.Graphics.DrawString(drawString, drawFont, drawBrush, drawPoint);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 绘制进度条
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="bounds"></param>
        private void DrawProgressBar(GraphicsCache cache, Rectangle bounds)
        {
            try
            {
                cache.Graphics.FillPath(Brushes.White, GetRoundRectangle(bounds)); //绘制圆角实心矩形

                for (int i = 0; i < 10; i++)
                {
                    Rectangle rectangle = new Rectangle();
                    rectangle.Y = bounds.Y + 2;
                    rectangle.Height = bounds.Height - 4;
                    rectangle.Width = 100;

                    m_ProgressBarOffset += 3;
                    rectangle.X = bounds.X + m_ProgressBarOffset;
                    if (true == rectangle.X > bounds.X + bounds.Width - rectangle.Width)
                    {
                        m_ProgressBarOffset = 0;
                        rectangle.X = bounds.X;
                    }

                    cache.Graphics.FillPath(Brushes.Green, GetRoundRectangle(rectangle)); //绘制圆角实心矩形
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>  
        /// 根据普通矩形得到圆角矩形的路径  
        /// </summary>  
        /// <param name="rectangle">原始矩形</param>  
        /// <param name="r">半径</param>  
        /// <returns>图形路径</returns>  
        private GraphicsPath GetRoundRectangle(Rectangle rectangle)
        {
            // 把圆角矩形分成八段直线、弧的组合，依次加到路径中  
            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(new Point(rectangle.X, rectangle.Y), new Point(rectangle.Right, rectangle.Y));
            gp.AddLine(new Point(rectangle.Right, rectangle.Y), new Point(rectangle.Right, rectangle.Bottom));
            gp.AddLine(new Point(rectangle.Right, rectangle.Bottom), new Point(rectangle.X, rectangle.Bottom));
            gp.AddLine(new Point(rectangle.X, rectangle.Bottom), new Point(rectangle.X, rectangle.Y));
            return gp;
        }

        #endregion

        #region 事件

        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (false == m_ButtonRectangle.Contains(new Point(e.X + 10, e.Y + 10)))
                {
                    return;
                }

                if (null != CancelWaitingEvent)
                {
                    CancelWaitingEvent();
                    HasCanceled = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
    //...

    internal class HOOK
    {
        #region 字段

        private int m_Hook = 0;
        private IntPtr? m_CurrentHandle = null;
        private HookProc m_MouseHookProcedure = null; // 不要定义在方法里面。执行几次后会被回收；切记！！！！

        #endregion

        #region 属性

        #endregion

        #region 委托

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        public delegate void MouseClickHandler(object sender, MouseEventArgs e);
        public event MouseClickHandler MouseClickEvent;

        #endregion

        #region 引用函数

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// GetForegroundWindow
        ///  　函数功能：该函数返回前台窗口（用户当前工作的窗口）。系统分配给产生前台窗口的线程一个稍高一点的优先级。
        ///  　函数原型：HWND GetForegroundWindow（VOID）
        ///  　参数：无。
        ///  　返回值：函数返回前台窗回的句柄。
        ///  　速查：Windows NT：3.1以上版本；Windows：95以上版本：Windows CE：1.0以上版本：头文件：Winuser.h；库文件：user32.lib。
        ///  　摘要：
        ///  　函数功能：该函数返回前台窗口（用户当前工作的窗口）。系统分配给产生前台窗口的线程一个稍高一点的优先级。
        ///  　函数原型：HWND GetForegroundWindow（VOID）
        ///  　参数：无。
        ///  　返回值：函数返回前台窗回的句柄。
        ///  　速查：Windows NT：3.1以上版本；Windows：95以上版本：Windows CE：1.0以上版本：头文件：Winuser.h；库文件：user32.lib。
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// GetWindowThreadProcessId这个函数来获得窗口所属进程ID和线程ID
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, int ID);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        #endregion

        #region 构造函数

        public HOOK()
        {
            m_MouseHookProcedure = new HookProc(HOOKProcReturn);
        }

        #endregion

        #region 方法

        /// <summary>
        /// 消息处理中心
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int HOOKProcReturn(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                //鼠标右键
                if (wParam.ToInt32() == 0xa1)
                {
                    Win32Api.MouseHookStruct MyMouseHookStruct = (Win32Api.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32Api.MouseHookStruct));
                    MouseButtons button = MouseButtons.Left;

                    Point point = new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);

                    var e = new MouseEventArgs(button, 1, point.X, point.Y, 0);

                    if (null != MouseClickEvent)
                    {
                        MouseClickEvent(this, e);
                    }

                }
            }
            //监听下一次事件
            return CallNextHookEx(m_Hook, nCode, wParam, lParam);
        }

        public bool StartHook(IntPtr intPtr)
        {
            if (null != m_CurrentHandle)
            {
                return true;
            }
            m_CurrentHandle = intPtr;
            // MouseHookProcedure = new HookProc(HOOK.HOOKProcReturn);
            //监听 事件 发送消息
            m_Hook = SetWindowsHookEx((int)HookType.WH_MOUSE, m_MouseHookProcedure, IntPtr.Zero, GetWindowThreadProcessId(m_CurrentHandle.Value, 0));
            if (m_Hook == 0)
            {
                return false;
            }
            return true;
        }

        public bool StopHook()
        {
            return UnhookWindowsHookEx(m_Hook);
        }

        #endregion

        #region 内部结构
        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public HOOK.POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// 钩子类型舰艇鼠标键盘等事件
        /// </summary>
        private enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,//局部 线程级别
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14 //全局
        }

        public class Win32Api
        {
            [StructLayout(LayoutKind.Sequential)]
            public class POINT
            {
                public int x;
                public int y;
            }
            [StructLayout(LayoutKind.Sequential)]
            public class MouseHookStruct
            {
                public POINT pt;
                public int hwnd;
                public int wHitTestCode;
                public int dwExtraInfo;
            }
            public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
            //安装钩子
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
            //卸载钩子
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern bool UnhookWindowsHookEx(int idHook);
            //调用下一个钩子
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);
        }

        #endregion

    }
}