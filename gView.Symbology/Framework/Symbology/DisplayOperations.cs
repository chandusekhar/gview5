using gView.Framework.Carto;
using gView.Framework.Geometry;
using System;
using System.Drawing.Drawing2D;

namespace gView.Framework.Symbology
{
    internal class DisplayOperations
    {
        public static System.Drawing.Drawing2D.GraphicsPath Geometry2GraphicsPath(IDisplay display, IGeometry geometry)
        {
            try
            {
                if (geometry is IPolygon)
                {
                    return ConvertPolygon(display, (IPolygon)geometry);
                }
                else if (geometry is IPolyline)
                {
                    return ConvertPolyline(display, (IPolyline)geometry);
                }
                else if (geometry is IEnvelope)
                {
                    return ConvertEnvelope(display, (IEnvelope)geometry);
                }
            }
            catch
            {
            }
            return null;
        }

        private static System.Drawing.Drawing2D.GraphicsPath ConvertPolygon(IDisplay display, IPolygon polygon)
        {
            //if (polygon == null || polygon.RingCount == 0)
            //    return null;

            GraphicsPath gp = new GraphicsPath();

            for (int r = 0; r < polygon.RingCount; r++)
            {
                bool first = true;
                int count = 0;
                IRing ring = polygon[r];
                int pCount = ring.PointCount;

                //double o_x = -1e10, o_y = -1e10;
                float o_x = float.MinValue, o_y = float.MinValue;
                gp.StartFigure();
                for (int p = 0; p < pCount; p++)
                {
                    IPoint point = ring[p];
                    double x = point.X, y = point.Y;

                    display.World2Image(ref x, ref y);

                    //
                    // Auf 0.1 Pixel runden, sonst kann es bei fast
                    // horizontalen (vertikalen) Linien zu Fehlern kommen
                    // -> Eine h�lfte (beim Bruch) wird nicht mehr gezeichnet
                    //
                    x = Math.Round(x, 1);
                    y = Math.Round(y, 1);

                    if (!((float)o_x == (float)x &&
                        (float)o_y == (float)y))
                    {
                        if (!first)
                        {
                            gp.AddLine(
                                (float)o_x,
                                (float)o_y,
                                (float)x,
                                (float)y);
                            count++;
                        }
                        else
                        {
                            first = false;
                        }
                    }
                    o_x = (float)x;
                    o_y = (float)y;
                }
                if (count > 0)
                {
                    gp.CloseFigure();
                }
            }

            return gp;
        }
        private static System.Drawing.Drawing2D.GraphicsPath ConvertEnvelope(IDisplay display, IEnvelope envelope)
        {
            GraphicsPath gp = new GraphicsPath();

            double minx = envelope.minx, miny = envelope.miny;
            double maxx = envelope.maxx, maxy = envelope.maxy;
            display.World2Image(ref minx, ref miny);
            display.World2Image(ref maxx, ref maxy);

            gp.StartFigure();
            gp.AddLine((float)minx, (float)miny, (float)maxx, (float)miny);
            gp.AddLine((float)maxx, (float)miny, (float)maxx, (float)maxy);
            gp.AddLine((float)maxx, (float)maxy, (float)minx, (float)maxy);
            gp.CloseFigure();

            return gp;
        }
        private static System.Drawing.Drawing2D.GraphicsPath ConvertPolyline(IDisplay display, IPolyline polyline)
        {
            //if (polyline == null || polyline.PathCount == 0)
            //    return null;

            GraphicsPath gp = new GraphicsPath();

            for (int r = 0; r < polyline.PathCount; r++)
            {
                bool first = true;
                int count = 0;
                IPath path = polyline[r];
                int pCount = path.PointCount;

                //double o_x = -1e10, o_y = -1e10;
                float o_x = float.MinValue, o_y = float.MinValue;
                gp.StartFigure();
                for (int p = 0; p < pCount; p++)
                {
                    IPoint point = path[p];
                    double x = point.X, y = point.Y;
                    display.World2Image(ref x, ref y);

                    //
                    // Auf 0.1 Pixel runden, sonst kann es bei fast
                    // horizontalen (vertikalen) Linien zu Fehlern kommen
                    // -> Eine h�lfte (beim Bruch) wird nicht mehr gezeichnet
                    //
                    x = Math.Round(x, 1);
                    y = Math.Round(y, 1);

                    if (!((float)o_x == (float)x &&
                          (float)o_y == (float)y))
                    {
                        if (!first)
                        {
                            gp.AddLine(
                                (float)o_x,
                                (float)o_y,
                                (float)x,
                                (float)y);
                            count++;
                        }
                        else
                        {
                            first = false;
                        }
                    }
                    o_x = (float)x;
                    o_y = (float)y;
                }
                /*
                if(count>0) 
                { 
                    gp.CloseFigure();
                }
                */
            }

            return gp;
        }
    }
}
