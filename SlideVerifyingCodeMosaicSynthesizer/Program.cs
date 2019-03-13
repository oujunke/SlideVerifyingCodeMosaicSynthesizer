using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlideVerifyingCodeMosaicSynthesizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var imagePath = "../../Image";
            for (int nindex = 1; nindex < 8; nindex++)
            {
                var bt1 = (Image.FromFile($"{imagePath}/t{nindex}.jpg") as Bitmap);
                var image1 = bt1.Compress(new Size(bt1.Width / 4, bt1.Height / 4));//压缩图片，节约卷积时间
                image1.Save($"{imagePath}/s{nindex}.jpg");
                var bt2 = Image.FromFile($"{imagePath}/t{nindex}_2.png") as Bitmap;
                var image2 = bt2.Compress(new Size(bt2.Width / 4, bt2.Height / 4));//压缩图片，节约卷积时间
                image2.Save($"{imagePath}/s{nindex}_2.jpg");
                ShapeConvolutional2D convolutional2D = new ShapeConvolutional2D(image1);
                convolutional2D.GenerateKernel(image2);
                var creslut = convolutional2D.Convolution(out int mx, out int my);
                Bitmap bitmap = new Bitmap(bt1);
                Graphics graphics = Graphics.FromImage(bitmap);
                graphics.DrawImage(bt2, mx * 4, my * 4, convolutional2D.KernelRect.Multiply(4).ToRectangle(), GraphicsUnit.Pixel);
                bitmap.Save($"{imagePath}/h{nindex}.jpg");
            }
        }
    }
}
