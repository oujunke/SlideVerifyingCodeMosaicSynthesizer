using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SlideVerifyingCodeMosaicSynthesizer
{
    public static class ImageExtend
    {
        #region 压缩图片  
        /// <summary>  
        /// 压缩到指定尺寸  
        /// </summary>  
        /// <param name="oldfile">原文件</param>  
        /// <param name="newfile">新文件</param>  
        public static Bitmap Compress(this Bitmap bitmap, Size newSize)
        {
            try
            {
                ImageFormat thisFormat = bitmap.RawFormat;
                Bitmap outBmp = new Bitmap(newSize.Width, newSize.Height);
                Graphics g = Graphics.FromImage(outBmp);
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, new Rectangle(0, 0, newSize.Width, newSize.Height), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel);
                g.Dispose();
                EncoderParameters encoderParams = new EncoderParameters();
                long[] quality = new long[1];
                quality[0] = 100;
                EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                encoderParams.Param[0] = encoderParam;
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICI = null;
                for (int x = 0; x < arrayICI.Length; x++)
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICI = arrayICI[x]; //设置JPEG编码  
                        break;
                    }
                return outBmp;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region 图片灰度化  
        /// <summary>
        /// 灰度化  
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color Gray(Color c)
        {
            int rgb = Convert.ToInt32((double)(((0.3 * c.R) + (0.59 * c.G)) + (0.11 * c.B)));
            return Color.FromArgb(rgb, rgb, rgb);
        }
        /// <summary>
        /// 图片灰度化  
        /// </summary>
        /// <param name="mybm"></param>
        /// <returns></returns>
        public static Bitmap Gray(this Bitmap mybm)
        {
            Bitmap bm = new Bitmap(mybm.Width, mybm.Height);
            int x, y;
            for (x = 0; x < mybm.Width; x++)
            {
                for (y = 0; y < mybm.Height; y++)
                {
                    bm.SetPixel(x, y, Gray(mybm.GetPixel(x, y)));
                }
            }
            return bm;
        }
        #endregion
    }
}
