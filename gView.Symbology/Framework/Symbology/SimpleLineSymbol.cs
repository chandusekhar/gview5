﻿using gView.Framework.Carto;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Symbology.UI;
using gView.Framework.system;
using gView.Framework.UI;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace gView.Framework.Symbology
{
    [gView.Framework.system.RegisterPlugIn("91CC3F6F-0EC5-42b7-AA34-9C89803118E7")]
    public sealed class SimpleLineSymbol : Symbol, ILineSymbol, IPropertyPage, IPenColor, IPenWidth, IPenDashStyle
    {
        private Pen _pen;
        private Color _color;

        public SimpleLineSymbol()
        {
            _color = Color.Black;
            _pen = new Pen(_color, 1);
            _pen.LineJoin = LineJoin.Round;
        }

        private SimpleLineSymbol(Color color, float width)
        {
            _color = color;
            _pen = new Pen(_color, width);
            _pen.LineJoin = LineJoin.Round;
        }

        ~SimpleLineSymbol()
        {
            this.Release();
        }

        public override string ToString()
        {
            return this.Name;
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.DashStyleTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
        [UseDashStylePicker()]
        public DashStyle DashStyle
        {
            get
            {
                return _pen.DashStyle;
            }
            set
            {
                _pen.DashStyle = value;
            }
        }

        public System.Drawing.Drawing2D.LineCap LineStartCap
        {
            get
            {
                return _pen.StartCap;
            }
            set
            {
                _pen.StartCap = value;
            }
        }

        public System.Drawing.Drawing2D.LineCap LineEndCap
        {
            get
            {
                return _pen.EndCap;
            }
            set
            {
                _pen.EndCap = value;
            }
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.PenWidthTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [UseWidthPicker()]
        public float Width
        {
            get
            {
                return _pen.Width;
            }
            set
            {
                if (_pen == null)
                {
                    _pen = new Pen(_color, value);
                }
                else
                {
                    _pen.Width = value;
                }
            }
        }

        [Browsable(true)]
        //[Editor(typeof(gView.Framework.UI.ColorTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
        [UseColorPicker()]
        public System.Drawing.Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _pen.Color = value;
                _color = value;
            }
        }

        #region ILineSymbol Member

        public void DrawPath(IDisplay display, System.Drawing.Drawing2D.GraphicsPath path)
        {
            if (path != null)
            {
                display.GraphicsContext.SmoothingMode = (SmoothingMode)this.Smoothingmode;
                display.GraphicsContext.DrawPath(_pen, path);
                display.GraphicsContext.SmoothingMode = SmoothingMode.None;
            }
        }

        #endregion

        #region ISymbol Member

        public void Draw(IDisplay display, IGeometry geometry)
        {
            // Wenn DashStyle nicht Solid (und Antialiasing) soll Geometry erst geclippt werden,
            // da es sonst zu extrem Zeitaufwendigen Graphikopertation kommt...

            if (this.DashStyle != DashStyle.Solid &&
                this.Smoothingmode != SymbolSmoothing.None)
            {
                IEnvelope dispEnvelope =
                    display.DisplayTransformation != null ?
                    new Envelope(display.DisplayTransformation.TransformedBounds(display)) :
                    new Envelope(display.Envelope);

                //dispEnvelope.Raise(75);
                geometry = gView.Framework.SpatialAlgorithms.Clip.PerformClip(dispEnvelope, geometry);
                if (geometry == null)
                {
                    return;
                }

                //GraphicsPath gp2 = DisplayOperations.Geometry2GraphicsPath(display, dispEnvelope);
                //if (gp2 != null)
                //{
                //    this.DrawPath(display, gp2);
                //    gp2.Dispose(); gp2 = null;
                //}
            }

            GraphicsPath gp = DisplayOperations.Geometry2GraphicsPath(display, geometry);
            if (gp != null)
            {
                if (this.LineStartCap == System.Drawing.Drawing2D.LineCap.ArrowAnchor ||
                    this.LineEndCap == System.Drawing.Drawing2D.LineCap.ArrowAnchor)
                {
                    //
                    // bei LineCap Arrow (Pfeil...) kann es bei sehr kurzen Linen
                    // zu einer Out of Memory Exception kommen...
                    //
                    try
                    {
                        this.DrawPath(display, gp);
                    }
                    catch
                    {
                        LineCap sCap = this.LineStartCap;
                        LineCap eCap = this.LineEndCap;
                        this.LineStartCap = (sCap == System.Drawing.Drawing2D.LineCap.ArrowAnchor) ? System.Drawing.Drawing2D.LineCap.Triangle : sCap;
                        this.LineEndCap = (eCap == System.Drawing.Drawing2D.LineCap.ArrowAnchor) ? System.Drawing.Drawing2D.LineCap.Triangle : eCap;

                        this.DrawPath(display, gp);

                        this.LineStartCap = sCap;
                        this.LineEndCap = eCap;
                    }
                }
                else
                {
                    this.DrawPath(display, gp);
                }
                gp.Dispose(); gp = null;
            }
        }

        public void Release()
        {
            if (_pen != null)
            {
                _pen.Dispose();
                _pen = null;
            }
        }


        [Browsable(false)]
        public string Name
        {
            get
            {
                return "Line Symbol";
            }
        }

        #endregion

        #region IPropertyPage Member

        public object PropertyPageObject()
        {
            return null;
        }

        public object PropertyPage(object initObject)
        {
            string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Win.Symbology.UI.dll");

            IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Symbology.UI.PropertyForm_SimpleLineSymbol") as IPropertyPanel;
            if (p != null)
            {
                return p.PropertyPanel(this);
            }

            return null;
        }

        #endregion

        #region IPersistable Member

        [Browsable(false)]
        public string PersistID
        {
            get
            {
                return null;
            }
        }

        public void Load(IPersistStream stream)
        {
            base.Load(stream);

            this.Color = Color.FromArgb((int)stream.Load("color", Color.Black.ToArgb()));
            this.Width = (float)stream.Load("width", (float)1);
            this.DashStyle = (DashStyle)stream.Load("dashstyle", DashStyle.Solid);

            this.LineStartCap = (LineCap)stream.Load("linescap", LineCap.Flat);
            this.LineEndCap = (LineCap)stream.Load("lineecap", LineCap.Flat);

            int cap_old = (int)stream.Load("linecap", -1);
            if (cap_old >= 0)
            {
                this.LineStartCap = this.LineEndCap = (LineCap)cap_old;
            }

            this.MaxPenWidth = (float)stream.Load("maxwidth", 0f);
            this.MinPenWidth = (float)stream.Load("minwidth", 0f);
            this.PenWidthUnit = (DrawingUnit)stream.Load("widthunit", DrawingUnit.Pixel);
        }

        public void Save(IPersistStream stream)
        {
            base.Save(stream);

            stream.Save("color", this.Color.ToArgb());
            stream.Save("width", this.Width);
            stream.Save("dashstyle", (int)this.DashStyle);
            stream.Save("linescap", (int)this.LineStartCap);
            stream.Save("lineecap", (int)this.LineEndCap);

            stream.Save("maxwidth", (float)this.MaxPenWidth);
            stream.Save("minwidth", (float)this.MinPenWidth);
            stream.Save("widthunit", (int)this.PenWidthUnit);
        }

        #endregion

        #region IClone2

        public object Clone(CloneOptions options)
        {
            var display = options?.Display;

            if (display == null)
            {
                return Clone();
            }

            float fac = 1;
            if (_widthUnit == DrawingUnit.Pixel)
            {
                if (display.refScale > 1)
                {
                    fac = (float)(display.refScale / display.mapScale);
                    fac = options.RefScaleFactor(fac);
                }

                if (display.dpi != 96.0)
                {
                    fac *= (float)(display.dpi / 96.0);
                }
            }
            else if (_widthUnit != DrawingUnit.Pixel && _pen.Width > 0)
            {
                float pix = (float)(display.mapScale / (display.dpi / 0.0254));
                if (pix == 0f)
                    fac = 0;
                else
                    fac = 1f / pix;
            }

            SimpleLineSymbol clone = new SimpleLineSymbol(_color, ReferenceScaleHelper.PenWidth(_pen.Width * fac, this, display));
            clone.DashStyle = this.DashStyle;
            clone.LineStartCap = this.LineStartCap;
            clone.LineEndCap = this.LineEndCap;
            clone.Smoothingmode = this.Smoothingmode;
            clone.LegendLabel = _legendLabel;

            clone.PenWidthUnit = _widthUnit;

            return clone;
        }

        #endregion

        #region IPenColor Member

        [Browsable(false)]
        public Color PenColor
        {
            get
            {
                return Color;
            }
            set
            {
                Color = value;
            }
        }

        #endregion

        #region IPenWidth Member

        [Browsable(false)]
        public float PenWidth
        {
            get
            {
                return this.Width;
            }
            set
            {
                this.Width = value;
            }
        }

        private float _maxWidth, _minWidth;

        [Browsable(true)]
        [Category("Reference Scaling")]
        [UseWidthPicker()]
        public float MaxPenWidth
        {
            get
            {
                return _maxWidth;
            }
            set
            {
                _maxWidth = value;
            }
        }

        [Browsable(true)]
        [Category("Reference Scaling")]
        [UseWidthPicker()]
        public float MinPenWidth
        {
            get
            {
                return _minWidth;
            }
            set
            {
                _minWidth = value;
            }
        }

        private DrawingUnit _widthUnit;

        [Browsable(true)]
        [DisplayName("Width Unit")]
        [Category("Unit")]
        public DrawingUnit PenWidthUnit
        {
            get { return _widthUnit; }
            set { _widthUnit = value; }
        }

        #endregion

        #region IPenDashStyle Member

        [Browsable(false)]
        public DashStyle PenDashStyle
        {
            get
            {
                return this.DashStyle;
            }
            set
            {
                this.DashStyle = value;
            }
        }

        #endregion

        #region ISymbol Member

        [Browsable(false)]
        public SymbolSmoothing SymbolSmothingMode
        {
            set { this.Smoothingmode = value; }
        }

        public bool RequireClone()
        {
            return _widthUnit != DrawingUnit.Pixel;
        }

        #endregion
    }
}
