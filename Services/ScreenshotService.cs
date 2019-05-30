using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using GI.Screenshot;


namespace Monitor.Services
{
    public class ScreenshotService
    {
        public Image TakeScreenshot(IntPtr windowHandle)
        {
            if (windowHandle != IntPtr.Zero)
            {
                var sc = new ScreenCapture();
                Rectangle r = GetWindowRectangle(windowHandle);

                var image = BitmapFromSource(CaptureRegion(new Rect(r.X, r.Y, r.Width, r.Height)));
//                image.Save(@"c:\temp\aa.png");
                return image;
            }
            else
            {
                return null;
            }
        }

        public static BitmapSource CaptureRegion(Rect rect)
        {
            using (var bitmap = new Bitmap((int) rect.Width, (int) rect.Height,
                PixelFormat.Format32bppArgb))
            {
                var graphics = Graphics.FromImage(bitmap);

                graphics.CopyFromScreen((int) rect.X, (int) rect.Y, 0, 0,
                    new System.Drawing.Size((int) rect.Size.Width, (int) rect.Size.Height),
                    CopyPixelOperation.SourceCopy);

                return bitmap.ToBitmapSource();
            }
        }

        private System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }

            return bitmap;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);


        public static Rectangle GetWindowRectangle(IntPtr Handle)
        {
            RECT area = new RECT();
            GetWindowRect(Handle, out area);

            //if (isDialogWindow(Handle))
            //{
            //    area.Left -= 5;
            //    area.Right += 5;
            //    area.Top -= 5;
            //    area.Bottom += 5;
            //}

            return new Rectangle(area.Left, area.Top, area.Width, area.Height);
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left_, int top_, int right_, int bottom_)
            {
                Left = left_;
                Top = top_;
                Right = right_;
                Bottom = bottom_;
            }

            public int Height
            {
                get { return Bottom - Top; }
            }

            public int Width
            {
                get { return Right - Left; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
            }

            // Handy method for converting to a System.Drawing.Rectangle
            public Rectangle ToRectangle()
            {
                return Rectangle.FromLTRB(Left, Top, Right, Bottom);
            }

            public static RECT FromRectangle(Rectangle rectangle)
            {
                return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
            }

            public override int GetHashCode()
            {
                return Left ^ ((Top << 13) | (Top >> 0x13))
                            ^ ((Width << 0x1a) | (Width >> 6))
                            ^ ((Height << 7) | (Height >> 0x19));
            }

            #region Operator overloads

            public static implicit operator Rectangle(RECT rect)
            {
                return rect.ToRectangle();
            }

            public static implicit operator RECT(Rectangle rect)
            {
                return FromRectangle(rect);
            }

            #endregion
        }
    }
}