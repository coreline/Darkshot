using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Collections.Generic;
using static Darkshot.Gdi32.Gdi32;
using System.Runtime.InteropServices;

namespace Darkshot
{
    public static class Filters
    {
        public static Bitmap Gaussian(this Bitmap bitmap, float radius)
        {
            if (bitmap.Width < 3 || bitmap.Height < 3)
                return bitmap;

            var result = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);
            var channels = bitmap.GetChannelsCount();
            for (var channel = 0; channel < channels; channel++)
            {
                var matrix = bitmap.GetMatrix(channel);
                var blurred = Gaussian(matrix, radius);
                result.SetMatrix(blurred, channel);
            }

            return result;
        }

        static int GetChannelsCount(this Bitmap bmp)
        {
            var rect = new Rectangle(Point.Empty, bmp.Size);
            var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            var channels = bitmapData.Stride / bmp.Width;
            bmp.UnlockBits(bitmapData);
            return channels;
        }

        static float[,] GetMatrix(this Bitmap bmp, int channel)
        {
            //var rect = new Rectangle(Point.Empty, bmp.Size);
            //int width = rect.Width;
            //int height = rect.Height;
            //var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            //var stride = bitmapData.Stride;
            //var channels = bitmapData.Stride / bmp.Width;
            //var ptr = (byte*)bitmapData.Scan0;
            //var matrix = new float[width, height];

            //fixed (float* pmatrix = matrix)
            //{
            //    for (var y = 0; y < height; y++)
            //    {
            //        var row = ptr + stride * y;
            //        for (var x = 0; x < width; x++)
            //        {
            //            var p = row + x * channels;
            //            pmatrix[x * height + y + channel] = p[channel];
            //        }
            //    }
            //}

            //bmp.UnlockBits(bitmapData);

            //return matrix;

            var rect = new Rectangle(Point.Empty, bmp.Size);
            int width = rect.Width;
            int height = rect.Height;
            var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            var stride = bitmapData.Stride;
            var channels = bitmapData.Stride / bmp.Width;
            var bytes = new byte[stride * height];
            Marshal.Copy(bitmapData.Scan0, bytes, 0, bytes.Length);
            bmp.UnlockBits(bitmapData);

            var matrix = new float[width, height];

            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                    matrix[x, y] = bytes[stride * y + x * channels + channel];

            return matrix;
        }

        static void SetMatrix(this Bitmap bmp, float[,] matrix, int channel)
        {
            //var rect = new Rectangle(Point.Empty, bmp.Size);
            //int width = rect.Width;
            //int height = rect.Height;
            //var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            //var stride = bitmapData.Stride;
            //var channels = bitmapData.Stride / bmp.Width;
            //var ptr = (byte*)bitmapData.Scan0;

            //fixed (float* pmatrix = matrix)
            //{
            //    for (var y = 0; y < height; y++)
            //    {
            //        var row = ptr + stride * y;
            //        for (var x = 0; x < width; x++)
            //        {
            //            var p = row + x * channels + channel;
            //            var mval = pmatrix[x * height + y];
            //            *p = (byte)((mval > 255) ? 255 : (mval < 0 ? 0 : mval));
            //        }
            //    }
            //}

            //bmp.UnlockBits(bitmapData);

            var rect = new Rectangle(Point.Empty, bmp.Size);
            int width = rect.Width;
            int height = rect.Height;
            var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            var stride = bitmapData.Stride;
            var channels = bitmapData.Stride / bmp.Width;
            var bytes = new byte[stride * height];

            Marshal.Copy(bitmapData.Scan0, bytes, 0, bytes.Length);

            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var val = matrix[x, y];
                    bytes[stride * y + x * channels + channel] = (byte)((val > 255) ? 255 : (val < 0 ? 0 : val));
                }

            Marshal.Copy(bytes, 0, bitmapData.Scan0, bytes.Length);

            bmp.UnlockBits(bitmapData);
        }

        /// <summary>
        /// Рекурсивный фильтр Гаусса
        /// </summary>
        /// <param name="matrix">Матрица мсходного изображения</param>
        /// <param name="sigma">Параметр размытия "сигма"</param>
        /// <returns>Результирующая матрица</returns>
        static unsafe float[,] Gaussian(float[,] matrix, float sigma)
        {
            #region Declare variables

            const int MIN_WIDTH = 3;
            const int MIN_HEIGTH = 3;
            const float MIN_SIGMA = 0.25f;

            int w, h;
            float q, b0, b1, b2, b3, b;
            float[,] result;

            #endregion

            #region Checking for correct input variables

            if (sigma < MIN_SIGMA)
                throw new Exception("The sigma can't be below " + MIN_SIGMA);
            if (matrix == null)
                throw new Exception("The matrix must be initialized");

            w = matrix.GetLength(0);
            h = matrix.GetLength(1);

            if (w < MIN_WIDTH || h < MIN_HEIGTH)
                throw new Exception("Size of matrix can't be below [" + MIN_WIDTH + ", " + MIN_HEIGTH + "]");

            #endregion

            #region Initialize variables

            q = (float)((sigma <= 2.5)
                             ? (3.97156 - 4.14554 * Math.Sqrt(1.0 - 0.26891 * Math.Sqrt(sigma)))
                             : (-0.9633 + 0.98711 * Math.Sqrt(sigma)));

            b0 = (float)(1.57825 + 2.44413 * q + 1.4281 * q * q + 0.422205 * q * q * q);
            b1 = (float)(2.44413 * q + 2.85619 * q * q + 1.26661 * q * q * q);
            b2 = (float)(-1.4281 * q * q - 1.26661 * q * q * q);
            b3 = (float)(0.422205 * q * q * q);
            b = (float)(1.0 - ((b1 + b2 + b3) / b0));

            result = new float[w, h];

            #endregion

            #region Apply filter by horizontal

            for (int y = 0; y < h; y++)
            {
                fixed (float* r = result, m = matrix)
                {
                    r[y] = m[y] * (b + (b1 + b2 + b3) / b0);
                    r[h + y] = b * m[h + y] + r[y] * (b1 + b2 + b3) / b0;
                    r[2 * h + y] = b * m[2 * h + y] + (b1 * r[h + y] + (b2 + b3) * r[y]) / b0;

                    for (int x = 3; x < w; x++)
                    {
                        r[x * h + y] = b * m[x * h + y] + (b1 * r[(x - 1) * h + y] + b2 * r[(x - 2) * h + y] + b3 * r[(x - 3) * h + y]) / b0;
                    }

                    r[(w - 1) * h + y] = r[(w - 1) * h + y] * (b + (b1 + b2 + b3) / b0);
                    r[(w - 2) * h + y] = b * r[(w - 2) * h + y] + r[(w - 1) * h + y] * (b1 + b2 + b3) / b0;
                    r[(w - 3) * h + y] = b * r[(w - 3) * h + y] + (b1 * r[(w - 2) * h + y] + (b2 + b3) * r[(w - 1) * h + y]) / b0;

                    for (int x = w - 4; x >= 0; x--)
                    {
                        r[x * h + y] = b * r[x * h + y] + (b1 * r[(x + 1) * h + y] + b2 * r[(x + 2) * h + y] + b3 * r[(x + 3) * h + y]) / b0;
                    }
                }
            }


            #endregion

            #region Apply filter by vertical

            for (int x = 0; x < w; x++)
            {
                fixed (float* r = result)
                {
                    float* row = r + x * h;
                    row[0] = row[0] * (b + (b1 + b2 + b3) / b0);
                    row[1] = b * row[1] + row[0] * (b1 + b2 + b3) / b0;
                    row[2] = b * row[2] + (b1 * row[1] + (b2 + b3) * row[0]) / b0;

                    for (int y = 3; y < h; y++)
                    {
                        row[y] = b * row[y] + (b1 * row[y - 1] + b2 * row[y - 2] + b3 * row[y - 3]) / b0;
                    }

                    row[h - 1] = row[h - 1] * (b + (b1 + b2 + b3) / b0);
                    row[h - 2] = b * row[h - 2] + row[h - 1] * (b1 + b2 + b3) / b0;
                    row[h - 3] = b * row[h - 3] + (b1 * row[h - 2] + (b2 + b3) * row[h - 1]) / b0;

                    for (int y = h - 4; y >= 0; y--)
                    {
                        row[y] = b * row[y] + (b1 * row[y + 1] + b2 * row[y + 2] + b3 * row[y + 3]) / b0;
                    }
                }
            }
            #endregion

            return result;
        }


    }
}
