﻿using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.system;
using System;
using System.Threading.Tasks;

namespace gView.Framework.Carto.LayerRenderers
{
    public sealed class RenderRasterLayer
    {
        private Map _map;
        private IRasterLayer _layer;
        private ICancelTracker _cancelTracker;
        private InterpolationMethod _interpolMethod = InterpolationMethod.Fast;
        private float _transparency = 0.0f;
        private System.Drawing.Color _transColor = System.Drawing.Color.Transparent;
        static private IRasterLayer _lastRasterLayer = null;

        public RenderRasterLayer(Map map, IRasterLayer layer, IRasterLayer rLayer, ICancelTracker cancelTracker)
        {
            _map = map;
            _layer = layer;
            _cancelTracker = cancelTracker;
            if (rLayer != null)
            {
                _interpolMethod = rLayer.InterpolationMethod;
                _transparency = rLayer.Transparency;
                _transColor = rLayer.TransparentColor;
            }
        }

        // Thread
        async public Task Render()
        {
            try
            {
                if (_layer == null || _map == null || _cancelTracker == null)
                {
                    return;
                }

                if (_layer.RasterClass.Polygon == null)
                {
                    return;
                }

                IEnvelope env = _layer.RasterClass.Polygon.Envelope;
                double minx = env.minx, miny = env.miny, maxx = env.maxx, maxy = env.maxy;
                _map.World2Image(ref minx, ref miny);
                _map.World2Image(ref maxx, ref maxy);
                int iWidth = 0, iHeight = 0;
                int min_x = Math.Max(0, (int)Math.Min(minx, maxx) - 1);
                int min_y = Math.Max(0, (int)Math.Min(miny, maxy) - 1);
                int max_x = Math.Min(iWidth = _map.iWidth, (int)Math.Max(minx, maxx) + 1);
                int max_y = Math.Min(iHeight = _map.iHeight, (int)Math.Max(miny, maxy) + 1);

                // _lastRasterLayer bei ImageServer vermeiden,
                // weil sonst das Bild gelöscht wird bevor es gezeichnet wurde
                // nicht Threadsicher!!!!!!!
                /*
                if (_lastRasterLayer != null && _lastRasterLayer != _layer)
                {
                    _lastRasterLayer.EndPaint();
                }
                */

                await _layer.RasterClass.BeginPaint(_map.Display, _cancelTracker);
                if (_layer.RasterClass.Bitmap == null)
                {
                    return;
                }

                //System.Windows.Forms.MessageBox.Show("begin");

                double W = (_map.Envelope.maxx - _map.Envelope.minx);
                double H = (_map.Envelope.maxy - _map.Envelope.miny);
                double MinX = _map.Envelope.minx;
                double MinY = _map.Envelope.miny;

                //_lastRasterLayer = _layer;

                System.Drawing.Graphics gr = _map.Display.GraphicsContext;
                if (gr == null)
                {
                    return;
                }

                gr.InterpolationMode = (System.Drawing.Drawing2D.InterpolationMode)_interpolMethod;

                // Transformation berechnen
                System.Drawing.RectangleF rect;
                switch (gr.InterpolationMode)
                {
                    case System.Drawing.Drawing2D.InterpolationMode.Bilinear:
                    case System.Drawing.Drawing2D.InterpolationMode.Bicubic:
                        rect = new System.Drawing.RectangleF(0, 0, _layer.RasterClass.Bitmap.Width - 1f, _layer.RasterClass.Bitmap.Height - 1f);
                        break;
                    case System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor:
                        rect = new System.Drawing.RectangleF(-0.5f, -0.5f, _layer.RasterClass.Bitmap.Width, _layer.RasterClass.Bitmap.Height);
                        //rect = new System.Drawing.RectangleF(0f, 0f, _layer.RasterClass.Bitmap.Width, _layer.RasterClass.Bitmap.Height);
                        break;
                    default:
                        rect = new System.Drawing.RectangleF(0, 0, _layer.RasterClass.Bitmap.Width, _layer.RasterClass.Bitmap.Height);
                        break;
                }
                System.Drawing.PointF[] points = new System.Drawing.PointF[3];

                if (_layer.RasterClass is IRasterClass2)
                {
                    IPoint p1 = ((IRasterClass2)_layer.RasterClass).PicPoint1;
                    IPoint p2 = ((IRasterClass2)_layer.RasterClass).PicPoint2;
                    IPoint p3 = ((IRasterClass2)_layer.RasterClass).PicPoint3;
                    if (_map.Display.GeometricTransformer != null)
                    {
                        p1 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p1);
                        p2 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p2);
                        p3 = (IPoint)_map.Display.GeometricTransformer.Transform2D(p3);
                    }

                    double X = p1.X, Y = p1.Y;
                    _map.Display.World2Image(ref X, ref Y);
                    points[0] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));

                    X = p2.X; Y = p2.Y;
                    _map.Display.World2Image(ref X, ref Y);
                    points[1] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));

                    X = p3.X; Y = p3.Y;
                    _map.Display.World2Image(ref X, ref Y);
                    points[2] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));

                    RoundGraphicPixelPoints(points);
                }
                else
                {
                    double X1 = _layer.RasterClass.oX - _layer.RasterClass.dx1 / 2.0 - _layer.RasterClass.dy1 / 2.0;
                    double Y1 = _layer.RasterClass.oY - _layer.RasterClass.dx2 / 2.0 - _layer.RasterClass.dy2 / 2.0;
                    double X = X1;
                    double Y = Y1;
                    if (_map.Display.GeometricTransformer != null)
                    {
                        IPoint p = (IPoint)_map.Display.GeometricTransformer.Transform2D(new Point(X, Y));
                        X = p.X; Y = p.Y;
                    }
                    _map.Display.World2Image(ref X, ref Y);
                    points[0] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));
                    X = X1 + (_layer.RasterClass.Bitmap.Width) * _layer.RasterClass.dx1;
                    Y = Y1 + (_layer.RasterClass.Bitmap.Width) * _layer.RasterClass.dx2;
                    if (_map.Display.GeometricTransformer != null)
                    {
                        IPoint p = (IPoint)_map.Display.GeometricTransformer.Transform2D(new Point(X, Y));
                        X = p.X; Y = p.Y;
                    }
                    _map.Display.World2Image(ref X, ref Y);
                    points[1] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));
                    X = X1 + (_layer.RasterClass.Bitmap.Height) * _layer.RasterClass.dy1;
                    Y = Y1 + (_layer.RasterClass.Bitmap.Height) * _layer.RasterClass.dy2;
                    if (_map.Display.GeometricTransformer != null)
                    {
                        IPoint p = (IPoint)_map.Display.GeometricTransformer.Transform2D(new Point(X, Y));
                        X = p.X; Y = p.Y;
                    }
                    _map.Display.World2Image(ref X, ref Y);
                    points[2] = new System.Drawing.PointF(ToPixelFloat(X), ToPixelFloat(Y));
                }

                if (_transColor.ToArgb() != System.Drawing.Color.Transparent.ToArgb())
                {
                    try
                    {
                        // kann OutOfMemoryException auslösen...
                        _layer.RasterClass.Bitmap.MakeTransparent(_transColor);
                    }
                    catch (Exception ex)
                    {
                        if (_map is IServiceMap && ((IServiceMap)_map).MapServer != null)
                        {
                            await ((IServiceMap)_map).MapServer.LogAsync(
                                ((IServiceMap)_map).Name,
                                "RenderRasterLayerThread: " + ((_layer != null) ? _layer.Title : String.Empty),
                                loggingMethod.error,
                                ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                        }
                        if (_map != null)
                        {
                            if (_map != null)
                            {
                                _map.AddException(new Exception("RenderRasterLayerThread: " + ((_layer != null) ? _layer.Title : String.Empty) + "\n" + ex.Message, ex));
                            }
                        }
                    }
                }

                //var comQual = gr.CompositingQuality;
                //gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                float opaque = 1.0f - _transparency;
                if (opaque > 0f && opaque < 1f)
                {
                    float[][] ptsArray ={
                            new float[] {1, 0, 0, 0, 0},
                            new float[] {0, 1, 0, 0, 0},
                            new float[] {0, 0, 1, 0, 0},
                            new float[] {0, 0, 0, opaque, 0},
                            new float[] {0, 0, 0, 0, 1}};

                    System.Drawing.Imaging.ColorMatrix clrMatrix = new System.Drawing.Imaging.ColorMatrix(ptsArray);
                    System.Drawing.Imaging.ImageAttributes imgAttributes = new System.Drawing.Imaging.ImageAttributes();
                    imgAttributes.SetColorMatrix(clrMatrix,
                                                 System.Drawing.Imaging.ColorMatrixFlag.Default,
                                                 System.Drawing.Imaging.ColorAdjustType.Bitmap);

                    gr.DrawImage(_layer.RasterClass.Bitmap, points, rect, System.Drawing.GraphicsUnit.Pixel, imgAttributes);
                }
                else
                {
                    gr.DrawImage(_layer.RasterClass.Bitmap, points, rect, System.Drawing.GraphicsUnit.Pixel);
                    //using (System.Drawing.Bitmap bm__ = new System.Drawing.Bitmap(100, 100))
                    //using (System.Drawing.Graphics gr__ = System.Drawing.Graphics.FromImage(bm__))
                    //{
                    //    gr__.DrawImage(_layer.RasterClass.Bitmap, new System.Drawing.Point(0, 0),);
                    //}
                }
            }
            catch (Exception ex)
            {
                if (_map is IServiceMap && ((IServiceMap)_map).MapServer != null)
                {
                    await ((IServiceMap)_map).MapServer.LogAsync(
                        ((IServiceMap)_map).Name,
                        "RenderRasterLayerThread:" + ((_layer != null) ? _layer.Title : String.Empty), loggingMethod.error,
                        ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                }
                if (_map != null)
                {
                    if (_map != null)
                    {
                        _map.AddException(new Exception("RenderRasterLayerThread: " + ((_layer != null) ? _layer.Title : String.Empty) + "\n" + ex.Message, ex));
                    }
                }
            }
            finally
            {
                _layer.RasterClass.EndPaint(_cancelTracker);
            }
        }

        /*
        public void Render2()
        {
            if (_layer == null || _map == null || _cancelTracker == null) return;
            if (_layer.Polygon == null) return;

            IEnvelope env = _layer.Polygon.Envelope;
            double minx = env.minx, miny = env.miny, maxx = env.maxx, maxy = env.maxy;
            _map.World2Image(ref minx, ref miny);
            _map.World2Image(ref maxx, ref maxy);
            int iWidth = 0, iHeight = 0;
            int min_x = Math.Max(0, (int)Math.Min(minx, maxx) - 1);
            int min_y = Math.Max(0, (int)Math.Min(miny, maxy) - 1);
            int max_x = Math.Min(iWidth = _map.iWidth, (int)Math.Max(minx, maxx) + 1);
            int max_y = Math.Min(iHeight = _map.iHeight, (int)Math.Max(miny, maxy) + 1);

            if (_lastRasterLayer != null && _lastRasterLayer != _layer)
            {
                _lastRasterLayer.EndPaint();
            }
            _layer.BeginPaint();
            //System.Windows.Forms.MessageBox.Show("begin");

            double W = (_map.Envelope.maxx - _map.Envelope.minx);
            double H = (_map.Envelope.maxy - _map.Envelope.miny);
            double MinX = _map.Envelope.minx;
            double MinY = _map.Envelope.miny;

            
            _lastRasterLayer = _layer;

            System.Drawing.Graphics gr = _map.Display.GraphicsContext;
            // Transformation berechnen
            System.Drawing.RectangleF rect = new System.Drawing.Rectangle(
                0, 0, _layer.Bitmap.Width, _layer.Bitmap.Height);
            System.Drawing.PointF[] points = new System.Drawing.PointF[3];

            double X = _layer.oX;
            double Y = _layer.oY;
            _map.Display.World2Image(ref X, ref Y);
            points[0] = new System.Drawing.PointF((float)X, (float)Y);
            X = _layer.oX + rect.Width * _layer.dx1;
            Y = _layer.oY + rect.Width * _layer.dx2;
            _map.Display.World2Image(ref X, ref Y);
            points[1] = new System.Drawing.PointF((float)X, (float)Y);
            X = _layer.oX + rect.Height * _layer.dy1;
            Y = _layer.oY + rect.Height * _layer.dy2;
            _map.Display.World2Image(ref X, ref Y);
            points[2] = new System.Drawing.PointF((float)X, (float)Y);

            gr.ResetTransform();
            gr.Transform = new System.Drawing.Drawing2D.Matrix(rect, points);
            gr.DrawImage(
                _layer.Bitmap, rect, rect, System.Drawing.GraphicsUnit.Pixel);

            gr.ResetTransform();

            //_layer.EndPaint();
        }
    */

        private float ToPixelFloat(double d)
        {
            return (float)d;
            //return (float)Math.Round(d, 2);
        }

        private void RoundGraphicPixelPoints(System.Drawing.PointF[] points)
        {
            float espsi = .1f;
            if (points.Length == 3)
            {
                points[0].X -= espsi;
                points[0].Y -= espsi;

                points[1].X += espsi;
                points[1].Y -= espsi;

                points[2].X -= espsi;
                points[2].Y += espsi;
            }
        }
    }
}
