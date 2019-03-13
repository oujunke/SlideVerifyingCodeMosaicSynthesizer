using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
namespace SlideVerifyingCodeMosaicSynthesizer
{
    public class ShapeConvolutional2D
    {
        public Point[] Left;
        public Point[] Top;
        public Point[] Right;
        public Point[] Bottom;
        public int ANum = 40;
        public int[,] Input;
        public Rect KernelRect;
        public Size InputSize;
        private int strides = 1;
        public ShapeConvolutional2D()
        {

        }
        /// <summary>
        /// 生成简单形状卷积器
        /// </summary>
        /// <param name="bitmap">输入图片</param>
        /// <param name="isGray">是否已灰度化</param>
        public ShapeConvolutional2D(Bitmap bitmap, bool isGray = false) : this()
        {
            if (isGray)
            {
                Input = GrayBitmapToNumArray(bitmap);
            }
            else
            {
                Input = BitmapToNumArray(bitmap);
            }
            InputSize = new Size(Input.GetLength(0), Input.GetLength(1));
        }
        /// <summary>
        /// 生成简单形状卷积器
        /// </summary>
        /// <param name="bitmap">输入背景图片</param>
        /// <param name="Kernel">输入标签图片</param>
        /// <param name="isGray">背景图片是否灰度化</param>
        public ShapeConvolutional2D(Bitmap bitmap,Bitmap Kernel, bool isGray = false) : this(bitmap,isGray)
        {
            GenerateKernel(Kernel);
        }
        /// <summary>
        /// 灰度照片转成灰度像素数组
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public int[,] GrayBitmapToNumArray(Bitmap bitmap)
        {
            var reslut = new int[bitmap.Width, bitmap.Height];
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    reslut[x, y] = bitmap.GetPixel(x, y).R;
                }
            }
            return reslut;
        }
        /// <summary>
        /// 照片转成灰度像素数组
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public int[,] BitmapToNumArray(Bitmap bitmap)
        {
            return GrayBitmapToNumArray(bitmap.Gray());
        }
        private bool AddPL(Bitmap bitmap, List<Point> pl, int y, int x, ref Rect rectangle)
        {
            var c = bitmap.GetPixel(x, y);
            if (c.A > ANum)//&& c.GetBrightness()> 0.001
            {
                if (x < rectangle.Left)
                    rectangle.Left = x;
                if (y < rectangle.Top)
                    rectangle.Top = y;
                if (x > rectangle.Right)
                    rectangle.Right = x;
                if (y > rectangle.Bottom)
                    rectangle.Bottom = y;
                pl.Add(new Point(x, y));
                return true;
            }
            return false;
        }
        /// <summary>
        /// 生成卷积内核
        /// </summary>
        /// <param name="bitmap"></param>
        public void GenerateKernel(Bitmap bitmap)
        {
            List<Point> pl = new List<Point>();
            KernelRect = new Rect(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (AddPL(bitmap, pl, y, x, ref KernelRect))
                        break;
                }
            }
            Left = pl.ToArray();
            pl.Clear();
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    if (AddPL(bitmap, pl, y, x, ref KernelRect))
                        break;
                }
            }
            Top = pl.ToArray();
            pl.Clear();
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = bitmap.Height - 1; y >= 0; y--)
                {
                    if (AddPL(bitmap, pl, y, x, ref KernelRect))
                        break;
                }
            }
            Bottom = pl.ToArray();
            pl.Clear();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = bitmap.Width - 1; x >= 0; x--)
                {
                    if (AddPL(bitmap, pl, y, x, ref KernelRect))
                        break;
                }
            }
            Right = pl.ToArray();
            var topSpaceNum = 1;
            Left = Left.Select(p =>
            {
                p.X = p.X - KernelRect.Left;
                p.Y = p.Y - KernelRect.Top;
                return p;
            }).Take(Left.Length - topSpaceNum).ToArray();
            Right = Right.Select(p =>
            {
                p.X = p.X - KernelRect.Left;
                p.Y = p.Y - KernelRect.Top;
                return p;
            }).Take(Right.Length - topSpaceNum).ToArray();
            Top = Top.Select(p =>
             {
                 p.X = p.X - KernelRect.Left;
                 p.Y = p.Y - KernelRect.Top;
                 return p;
             }).ToArray();
            Bottom = Bottom.Select(p =>
             {
                 p.X = p.X - KernelRect.Left;
                 p.Y = p.Y - KernelRect.Top - topSpaceNum;
                 return p;
             }).ToArray();
        }
        /// <summary>
        /// 卷积图像
        /// </summary>
        /// <param name="mx"></param>
        /// <param name="my"></param>
        /// <returns></returns>
        public int[,] Convolution()
        {
            return Convolution(out int mx, out int my);
        }
        /// <summary>
        /// 卷积图像
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="pt"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public int[,] Convolution(out int mx, out int my)
        {
            int ey = KernelRect.Height;
            int mn = int.MinValue;
            mx = 0;
            my = 0;
            var w = InputSize.Width - KernelRect.Width - strides * 2;
            var h = InputSize.Height - KernelRect.Height - strides * 2;
            var res = new int[w, h];
            int num = 0;
            for (int y = strides; y < h; y++)
            {
                for (int x = strides; x < w; x++)
                {
                    num = ConvolutionValue(x, y);
                    if (num > mn)
                    {
                        mn = num;
                        mx = x;
                        my = y;
                    }
                    res[x - strides, y - strides] = num;
                }
            }
            return res;
        }
        /// <summary>
        /// 根据像素点，获取卷积值
        /// </summary>
        /// <param name="chart"></param>
        /// <returns></returns>
        public int ConvolutionValue(int x, int y)
        {
            int num = 0;
            foreach (var p in Left)
            {
                num += GetNum(Input[x + p.X, y + p.Y], Input[x + p.X + 1, y + p.Y]);
            }
            foreach (var p in Right)
            {
                num += GetNum(Input[x + p.X, y + p.Y], Input[x + p.X - 1, y + p.Y]);
            }
            foreach (var p in Top)
            {
                num += GetNum(Input[x + p.X, y + p.Y], Input[x + p.X, y + p.Y + 1]);
            }
            foreach (var p in Bottom)
            {
                num += GetNum(Input[x + p.X, y + p.Y], Input[x + p.X, y + p.Y - 1]);
            }
            return num;
        }
        /// <summary>
        /// 输出数组结构和数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ts"></param>
        /// <param name="path"></param>
        public static void OutputTwoDimensionArray<T>(T[,] ts,string path )
        {
            var sw =new System.IO.StreamWriter(path,false);
            var w = ts.GetLength(0);
            var h = ts.GetLength(1);
            for(int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    sw.Write($"{ts[x,y]}\t");
                }
                sw.WriteLine("");
            }
            sw.Close();
        }
        /// <summary>
        /// 卷积操作记分器(可根据情况微调)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int GetNum(int x, int y)
        {
            return (x > y*2?2:(x>y ? 1 : -1));
        }

        public class Rect
        {
            public int Right;
            public int Top;
            public int Left;
            public int Bottom;
            public int Height { set => Bottom = Top + value; get => Bottom - Top; }
            public int Width { set => Right = Left + value; get => Right - Left; }
            public Rect(int x, int y, int x2, int y2)
            {
                Top = y;
                Bottom = y2;
                Left = x;
                Right = x2;
            }
            public Rect()
            {

            }
            public bool IsEmpty()
            {
                return Right == 0 && Top == 0 && Left == 0 && Bottom == 0;
            }
            public Rectangle ToRectangle()
            {
                return new Rectangle(Left,Top,Width,Height);
            }
            public Rect Multiply(int num)
            {
                return new Rect(Left* num, Top* num,Right* num,Bottom*num);
            }
        }
    }
}
