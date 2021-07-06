﻿using gView.GraphicsEngine.Abstraction;
using gView.GraphicsEngine.GdiPlus.Extensions;
using System;
using System.IO;

namespace gView.GraphicsEngine.GdiPlus
{
    public class GdiBitmapEncoding : IBitmapEncoding
    {
        public string EngineName => "GdiPlus";

        public void Encode(IBitmap bitmap, string filename, ImageFormat format, int quality = 0)
        {
            if (bitmap == null)
            {
                throw new ArgumentException("Bitmap is NULL");
            }

            if (bitmap.EngineElement is System.Drawing.Bitmap)
            {
                ((System.Drawing.Bitmap)bitmap.EngineElement).Save(filename, format.ToImageFormat());
            }
            else
            {
                BitmapPixelData pixelData = null;
                try
                {
                    pixelData = bitmap.LockBitmapPixelData(BitmapLockMode.ReadWrite, bitmap.PixelFormat);
                    using (var bm = new System.Drawing.Bitmap(
                            pixelData.Width,
                            pixelData.Height,
                            pixelData.Stride,
                            pixelData.PixelFormat.ToGdiPixelFormat(),
                            pixelData.Scan0))
                    {
                        bm.Save(filename, format.ToImageFormat());
                    }
                }
                finally
                {
                    if (pixelData != null)
                    {
                        bitmap.UnlockBitmapPixelData(pixelData);
                    }
                }
            }
        }

        public void Encode(IBitmap bitmap, Stream stream, ImageFormat format, int quality = 0)
        {
            if (bitmap == null)
            {
                throw new ArgumentException("Bitmap is NULL");
            }

            if (bitmap.EngineElement is System.Drawing.Bitmap)
            {
                ((System.Drawing.Bitmap)bitmap.EngineElement).Save(stream, format.ToImageFormat());
            }
            else
            {
                BitmapPixelData pixelData = null;
                try
                {
                    pixelData = bitmap.LockBitmapPixelData(BitmapLockMode.ReadWrite, bitmap.PixelFormat);
                    using (var bm = new System.Drawing.Bitmap(
                            pixelData.Width,
                            pixelData.Height,
                            pixelData.Stride,
                            pixelData.PixelFormat.ToGdiPixelFormat(),
                            pixelData.Scan0))
                    {
                        bm.Save(stream, format.ToImageFormat());
                    }
                }
                finally
                {
                    if (pixelData != null)
                    {
                        bitmap.UnlockBitmapPixelData(pixelData);
                    }
                }
            }
        }
    }
}
