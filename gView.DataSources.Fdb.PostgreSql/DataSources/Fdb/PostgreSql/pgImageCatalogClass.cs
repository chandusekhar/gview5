﻿using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using gView.Framework.system;
using gView.Framework.IO;
using gView.Framework.Geometry;
using System.IO;
using System.Data;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading.Tasks;

namespace gView.DataSources.Fdb.PostgreSql
{
    public class pgImageCatalogClass : IRasterCatalogClass, IParentRasterLayer, IFileSystemDependent, IFileWatchingDirectories, IRefreshable, IPointIdentify, IMulitPointIdentify, IGridClass, IMultiGridIdentify, IPersistable
    {
        internal enum ImageSpaceType { Database, FileSystem, Invalid }

        private IPolygon _polygon = null;
        internal ISpatialReference _sRef = null;
        private InterpolationMethod _interpolation = InterpolationMethod.Fast;
        internal IFeatureClass _fc;
        public pgFDB _fdb;
        internal string _dsname = String.Empty, _imageSpace = String.Empty;
        internal ImageSpaceType _imageSpaceType = ImageSpaceType.Database;
        internal IRasterDataset _dataset = null;
        internal PlugInManager _compMan = new PlugInManager();

        public IRasterClass RasterClass { get { return null; } }

        internal pgImageCatalogClass()
        {
        }

        public pgImageCatalogClass(IRasterDataset dataset, pgFDB fdb, IFeatureClass polygonsFC, ISpatialReference sRef, string imageSpace)
        {
            _dataset = dataset;
            _dsname = dataset.DatasetName;
            _fdb = fdb;
            _fc = polygonsFC;
            calcPolygon(polygonsFC.Envelope);
            _sRef = sRef;
            _imageSpace = (imageSpace == null) ? String.Empty : imageSpace;

            if (_imageSpace != String.Empty && imageSpace.ToLower() != "database")
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(_imageSpace);
                    if (di.Exists)
                        _imageSpaceType = ImageSpaceType.FileSystem;
                    else
                        _imageSpaceType = ImageSpaceType.Invalid;
                }
                catch
                {
                    _imageSpaceType = ImageSpaceType.Invalid;
                }
            }
        }

        private void calcPolygon(IEnvelope env)
        {
            if (env == null) return;
            _polygon = new Polygon();
            Ring ring = new Ring();
            ring.AddPoint(new Point(env.minx, env.miny));
            ring.AddPoint(new Point(env.maxx, env.miny));
            ring.AddPoint(new Point(env.maxx, env.maxy));
            ring.AddPoint(new Point(env.minx, env.maxy));
            _polygon.AddRing(ring);
        }

        async public Task<ICursor> ImageList()
        {
            try
            {
                if (_fc == null) return null;

                QueryFilter filter = new QueryFilter();
                filter.AddField("PATH");
                filter.AddField("LAST_MODIFIED");
                filter.AddField("PATH2");
                filter.AddField("LAST_MODIFIED2");
                filter.AddField("MANAGED");

                return await _fc.Search(filter);
            }
            catch
            {
                return null;
            }
        }

        public IFeature this[string filename]
        {
            get
            {
                IFeatureCursor cursor = null;
                try
                {
                    if (_fc == null) return null;

                    QueryFilter filter = new QueryFilter();
                    filter.AddField("*");
                    filter.WhereClause = "\"PATH\"='" + filename + "'";
                    cursor = _fc.Search(filter) as IFeatureCursor;

                    if (cursor == null) return null;
                    return cursor.NextFeature();
                }
                catch
                {
                    return null;
                }
                finally
                {
                    if (cursor != null) cursor.Dispose();
                }
            }
        }

        #region IRasterClass Members

        public gView.Framework.Geometry.IPolygon Polygon
        {
            get
            {
                return _polygon;
            }
        }

        public void BeginPaint(gView.Framework.Carto.IDisplay display, ICancelTracker cancelTracker)
        {

        }

        public void EndPaint(ICancelTracker cancelTracker)
        {

        }

        public System.Drawing.Color GetPixel(double X, double Y)
        {
            return System.Drawing.Color.Transparent;
        }

        public System.Drawing.Bitmap Bitmap
        {
            get { return null; }
        }

        public double oX
        {
            get { return 0.0; }
        }

        public double oY
        {
            get { return 0.0; }
        }

        public double dx1
        {
            get { return 0.0; }
        }

        public double dx2
        {
            get { return 0.0; }
        }

        public double dy1
        {
            get { return 0.0; }
        }

        public double dy2
        {
            get { return 0.0; }
        }

        public gView.Framework.Geometry.ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
            set
            {
                _sRef = value;
            }
        }

        public IRasterDataset Dataset
        {
            get { return _dataset; }
        }
        #endregion

        #region IParentLayer Members

        public InterpolationMethod InterpolationMethod
        {
            get
            {
                return _interpolation;
            }
            set
            {
                _interpolation = value;
            }
        }

        public IRasterLayerCursor ChildLayers(gView.Framework.Carto.IDisplay display, string filterClause)
        {
            if (_fc == null || display == null || _fdb == null)
                return new SimpleRasterlayerCursor(new List<IRasterLayer>());

            double dpm = Math.Max(display.GraphicsContext.DpiX, display.GraphicsContext.DpiY) / 0.0254;
            double pix = display.mapScale / dpm;/*display.dpm;*/  // [m]

            IEnvelope dispEnvelope = display.DisplayTransformation.TransformedBounds(display); //display.Envelope;
            if (display.GeometricTransformer != null)
            {
                dispEnvelope = (IEnvelope)((IGeometry)display.GeometricTransformer.InvTransform2D(dispEnvelope)).Envelope;
            }

            SpatialFilter filter = new SpatialFilter();
            filter.Geometry = dispEnvelope;
            filter.SubFields = "*";
            filter.WhereClause = filterClause;
            filter.SpatialRelation = spatialRelation.SpatialRelationIntersects;

            return new RasterLayerCursor(this, _fc.Search(filter) as IFeatureCursor,
                dispEnvelope, pix);
        }

        #endregion

        #region IFileSystemDependent Members

        public bool FileChanged(string filename)
        {
            return false;
        }

        public bool FileDeleted(string filename)
        {
            return false;
        }

        #endregion

        #region IFileWatchingDirectories Members

        private List<String> _directories = new List<string>();
        public List<DirectoryInfo> Directories
        {
            get
            {
                List<DirectoryInfo> dis = new List<DirectoryInfo>();
                foreach (string directory in _directories)
                {
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo(directory);
                        dis.Add(di);
                    }
                    catch { }
                }
                return dis;
            }
        }

        public void AddDirectory(DirectoryInfo di)
        {
            if (_directories.Contains(di.FullName.ToLower())) return;
            _directories.Add(di.FullName.ToLower());
        }

        public void RemoveDirectory(DirectoryInfo di)
        {
            if (!_directories.Contains(di.FullName.ToLower())) return;
            _directories.Remove(di.FullName.ToLower());
        }

        #endregion

        #region IClass Member

        public string Name
        {
            get { return _dsname; }
        }

        public string Aliasname
        {
            get { return _dsname; }
        }

        IDataset IClass.Dataset
        {
            get { return _dataset; }
        }

        #endregion

        #region IFeatureClass Member

        public string ShapeFieldName
        {
            get
            {
                if (_fc == null) return String.Empty;
                return _fc.ShapeFieldName;
            }
        }

        public IEnvelope Envelope
        {
            get
            {
                if (_fc == null) return null;
                return _fc.Envelope;
            }
        }

        public int CountFeatures
        {
            get
            {
                if (_fc == null) return 0;
                return _fc.CountFeatures;
            }
        }

        public IFeatureCursor GetFeatures(IQueryFilter filter)
        {
            if (_fc == null) return null;
            return _fc.GetFeatures(filter);
        }

        #endregion

        #region ITableClass Member

        public ICursor Search(IQueryFilter filter)
        {
            if (_fc == null) return null;
            return _fc.Search(filter);
        }

        public ISelectionSet Select(IQueryFilter filter)
        {
            if (_fc == null) return null;
            return _fc.Select(filter);
        }

        public IFields Fields
        {
            get
            {
                if (_fc == null) return null;
                return _fc.Fields;
            }
        }

        public IField FindField(string name)
        {
            if (_fc == null) return null;
            return _fc.FindField(name);
        }

        public string IDFieldName
        {
            get
            {
                if (_fc == null) return String.Empty;
                return _fc.IDFieldName;
            }
        }

        #endregion

        #region IGeometryDef Member

        public bool HasZ
        {
            get
            {
                if (_fc == null) return false;
                return _fc.HasZ; ;
            }
        }

        public bool HasM
        {
            get
            {
                if (_fc == null) return false;
                return _fc.HasM;
            }
        }

        public geometryType GeometryType
        {
            get
            {
                if (_fc == null) return geometryType.Unknown;
                return _fc.GeometryType;
            }
        }

        //public GeometryFieldType GeometryFieldType
        //{
        //    get
        //    {
        //        if (_fc == null) return GeometryFieldType.Default;
        //        return _fc.GeometryFieldType;
        //    }
        //}
        #endregion

        #region IRefreshable Member

        public void RefreshFrom(object obj)
        {
            if (!(obj is pgImageCatalogClass)) return;

            pgImageCatalogClass ic = (pgImageCatalogClass)obj;
            if (ic.Name != this.Name) return;

            _polygon = ic._polygon.Clone() as IPolygon;
            if (_fc is IRefreshable)
                ((IRefreshable)_fc).RefreshFrom(ic._fc);
        }

        #endregion

        #region IPointIdentify Member

        public ICursor PointQuery(gView.Framework.Carto.IDisplay display, IPoint point, ISpatialReference sRef, IUserData userdata)
        {
            PointCollection pColl = new PointCollection();
            pColl.AddPoint(point);

            return MultiPointQuery(display, pColl, sRef, userdata);
        }

        #endregion

        #region IMulitPointIdentify Member

        public ICursor MultiPointQuery(gView.Framework.Carto.IDisplay dispaly, IPointCollection points, ISpatialReference sRef, IUserData userdata)
        {
            IMultiPoint mPoint = new MultiPoint(points);
            List<IRasterLayer> layers = QueryChildLayers(mPoint, String.Empty);

            if (layers == null || layers.Count == 0) return null;
            List<IRow> cursorRows = new List<IRow>();

            for (int i = 0; i < mPoint.PointCount; i++)
            {
                IPoint point = mPoint[i];
                foreach (IRasterLayer layer in layers)
                {
                    if (layer == null ||
                        !(layer.Class is IRasterClass) ||
                        !(layer.Class is IPointIdentify)) continue;

                    if (gView.Framework.SpatialAlgorithms.Algorithm.Jordan(
                        ((IRasterClass)layer.Class).Polygon,
                        point.X, point.Y))
                    {
                        using (ICursor cursor = ((IPointIdentify)layer.Class).PointQuery(dispaly, point, sRef, userdata))
                        {
                            if (cursor is IRowCursor)
                            {
                                IRow row;
                                while ((row = ((IRowCursor)cursor).NextRow) != null)
                                {
                                    row.Fields.Add(new FieldValue("x", point.X));
                                    row.Fields.Add(new FieldValue("y", point.Y));
                                    cursorRows.Add(row);
                                }
                            }
                        }
                    }
                }
            }

            return new SimpleRowCursor(cursorRows);
        }

        #endregion

        #region IMultiGridIdentify

        public float[] MultiGridQuery(gView.Framework.Carto.IDisplay display, IPoint[] Points, double dx, double dy, ISpatialReference sRef, IUserData userdata)
        {
            if (Points == null || Points.Length != 3)
                return null;
            PointCollection pColl = new PointCollection();
            pColl.AddPoint(Points[0]);
            pColl.AddPoint(Points[1]);
            pColl.AddPoint(Points[2]);

            double d1x = Points[1].X - Points[0].X, d1y = Points[1].Y - Points[0].Y;
            double d2x = Points[2].X - Points[0].X, d2y = Points[2].Y - Points[0].Y;
            double len1 = Math.Sqrt(d1x * d1x + d1y * d1y);
            double len2 = Math.Sqrt(d2x * d2x + d2y * d2y);
            int stepX = (int)Math.Round(len1 / dx);
            int stepY = (int)Math.Round(len2 / dy);

            d1x /= stepX; d1y /= stepX;
            d2x /= stepY; d2y /= stepY;

            List<float> vals = new List<float>();
            vals.Add((float)(stepX + 1));
            vals.Add((float)(stepY + 1));

            List<IRasterLayer> layers = QueryChildLayers(pColl.Envelope, String.Empty);
            try
            {
                for (int y = 0; y <= stepY; y++)
                {
                    for (int x = 0; x <= stepX; x++)
                    {
                        Point p = new Point(Points[0].X + d1x * x + d2x * y,
                                            Points[0].Y + d1y * x + d2y * y);

                        bool found = false;
                        foreach (IRasterLayer layer in layers)
                        {
                            if (layer == null ||
                                !(layer.Class is IRasterClass) ||
                                !(layer.Class is IGridIdentify)) continue;

                            if (gView.Framework.SpatialAlgorithms.Algorithm.Jordan(((IRasterClass)layer.Class).Polygon, p.X, p.Y))
                            {
                                ((IGridIdentify)layer.Class).InitGridQuery();
                                float val = ((IGridIdentify)layer.Class).GridQuery(display, p, sRef);
                                vals.Add(val);
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            vals.Add(float.MinValue);
                    }
                }
            }
            finally
            {
                foreach (IRasterLayer layer in layers)
                {
                    if (layer.Class is IGridIdentify)
                        ((IGridIdentify)layer.Class).ReleaseGridQuery();
                }
            }


            return vals.ToArray();
        }

        #endregion

        #region IGridClass Member

        private GridColorClass[] _colorClasses = null;
        private bool _useHillShade = true;
        private double[] _hillShadeVector = new double[] { -1.0, 1.0, 1.0 };
        private bool _useNoDataValue = false;
        private double _noDataValue = 0.0;

        public GridRenderMethode ImplementsRenderMethods
        {
            get
            {
                return GridRenderMethode.Colors | GridRenderMethode.HillShade | GridRenderMethode.NullValue;
            }
        }

        public GridColorClass[] ColorClasses
        {
            get
            {
                return _colorClasses;
            }
            set
            {
                _colorClasses = value;
            }
        }

        public bool UseHillShade
        {
            get
            {
                return _useHillShade;
            }
            set
            {
                _useHillShade = value;
            }
        }

        public double[] HillShadeVector
        {
            get
            {
                return _hillShadeVector;
            }
            set
            {
                if (value != null && value.Length == 3)
                    _hillShadeVector = value;
            }
        }

        public double MinValue
        {
            get { return -1000.0; }
        }

        public double MaxValue
        {
            get { return 1000.0; }
        }

        public double IgnoreDataValue
        {
            get
            {
                return _noDataValue;
            }
            set
            {
                _noDataValue = value;
            }
        }

        public bool UseIgnoreDataValue
        {
            get
            {
                return _useNoDataValue;
            }
            set
            {
                _useNoDataValue = value;
            }
        }

        bool _renderRawGridValues = false;
        public bool RenderRawGridValues
        {
            get { return _renderRawGridValues; }
            set { _renderRawGridValues = value; }
        }
        #endregion

        public List<IRasterLayer> QueryChildLayers(IGeometry geometry, string filterClause)
        {
            List<IRasterLayer> childlayers = new List<IRasterLayer>();

            if (_fc == null || _fdb == null) return childlayers;

            SpatialFilter filter = new SpatialFilter();
            filter.Geometry = geometry;
            filter.SubFields = "*";
            filter.WhereClause = filterClause;
            filter.SpatialRelation = spatialRelation.SpatialRelationIntersects;

            using (IRasterLayerCursor cursor = new RasterLayerCursor(
                this, _fc.Search(filter) as IFeatureCursor))
            {
                IRasterLayer layer;
                while ((layer = cursor.NextRasterLayer) != null)
                {
                    childlayers.Add(layer);
                }
            }
            return childlayers;
        }

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _colorClasses = null;
            List<GridColorClass> classes = new List<GridColorClass>();
            GridColorClass cc;
            while ((cc = (GridColorClass)stream.Load("GridClass", null, new GridColorClass(0, 0, System.Drawing.Color.White))) != null)
            {
                classes.Add(cc);
            }
            if (classes.Count > 0)
                _colorClasses = classes.ToArray();

            _useHillShade = (bool)stream.Load("UseHillShade", true);
            _hillShadeVector[0] = (double)stream.Load("HillShadeDx", 0.0);
            _hillShadeVector[1] = (double)stream.Load("HillShadeDy", 0.0);
            _hillShadeVector[2] = (double)stream.Load("HillShadeDz", 0.0);
            _useNoDataValue = (bool)stream.Load("UseIgnoreData", 0);
            _noDataValue = (double)stream.Load("IgnoreData", 0.0);
            _renderRawGridValues = (bool)stream.Load("RenderRawGridValues", false);
        }

        public void Save(IPersistStream stream)
        {
            if (_colorClasses != null)
            {
                foreach (GridColorClass cc in _colorClasses)
                {
                    stream.Save("GridClass", cc);
                }
            }

            stream.Save("UseHillShade", _useHillShade);
            stream.Save("HillShadeDx", _hillShadeVector[0]);
            stream.Save("HillShadeDy", _hillShadeVector[1]);
            stream.Save("HillShadeDz", _hillShadeVector[2]);
            stream.Save("UseIgnoreData", _useNoDataValue);
            stream.Save("IgnoreData", _noDataValue);
            stream.Save("RenderRawGridValues", _renderRawGridValues);
        }

        #endregion
    }

    internal class RasterLayerCursor : IRasterLayerCursor
    {
        private IFeatureCursor _cursor;
        private pgImageCatalogClass _layer;
        private IEnvelope _dispEnvelope = null;
        private double _pix = 1.0;

        public RasterLayerCursor(pgImageCatalogClass layer, IFeatureCursor cursor)
        {
            _layer = layer;
            _cursor = cursor;
        }
        public RasterLayerCursor(pgImageCatalogClass layer, IFeatureCursor cursor, IEnvelope dispEnvelope, double pix)
            : this(layer, cursor)
        {
            _dispEnvelope = dispEnvelope;
            _pix = pix;
        }

        #region IRasterLayerCursor Member

        public IRasterLayer NextRasterLayer
        {
            get
            {
                if (_cursor == null || _layer == null) return null;

                try
                {
                    while (true)
                    {
                        IFeature feature = _cursor.NextFeature;
                        if (feature == null) return null;

                        IRasterLayer rLayer = null;

                        double cell = Math.Max((double)feature["CELLX"], (double)feature["CELLY"]);
                        int levels = (int)feature["LEVELS"];

                        if (!(bool)feature["MANAGED"])
                        {
                            if (feature["RF_PROVIDER"] == null || feature["RF_PROVIDER"] == DBNull.Value)
                            {
                                gView.DataSources.Raster.File.RasterFileDataset rDataset = new gView.DataSources.Raster.File.RasterFileDataset();
                                rLayer = rDataset.AddRasterFile((string)feature["PATH"], feature.Shape as IPolygon);
                            }
                            else
                            {
                                IRasterFileDataset rDataset = _layer._compMan.CreateInstance(new Guid(feature["RF_PROVIDER"].ToString())) as IRasterFileDataset;
                                if (rDataset == null) continue;
                                rLayer = rDataset.AddRasterFile((string)feature["PATH"], feature.Shape as IPolygon);
                            }
                            if (rLayer != null && rLayer.RasterClass != null)
                            {
                                rLayer.InterpolationMethod = _layer.InterpolationMethod;
                                if (rLayer.RasterClass.SpatialReference == null) rLayer.RasterClass.SpatialReference = _layer._sRef;
                            }
                        }

                        if (rLayer != null)
                        {
                            if (rLayer.Class is IGridClass)
                            {
                                IGridClass gridClass = (IGridClass)rLayer.Class;
                                gridClass.ColorClasses = _layer.ColorClasses;
                                gridClass.UseHillShade = _layer.UseHillShade;
                                gridClass.HillShadeVector = _layer.HillShadeVector;
                                gridClass.UseIgnoreDataValue = _layer.UseIgnoreDataValue;
                                gridClass.IgnoreDataValue = _layer.IgnoreDataValue;
                                gridClass.RenderRawGridValues = _layer.RenderRawGridValues;
                            }

                            return rLayer;
                        }
                    }
                }
                catch
                {
                }
                return null;
            }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            if (_cursor != null)
                _cursor.Dispose();
            _cursor = null;
        }

        #endregion
    }

    internal class pgImageDatasetImageClass : IRasterClass
    {
        pgFDB _fdb;
        private int _ID;
        private Polygon _polygon;
        private ISpatialReference _sRef = null;
        private string _dsname = "";
        private double _X, _Y;
        private double _dx_X, _dx_Y, _dy_X, _dy_Y;
        int _iWidth, _iHeight;

        public pgImageDatasetImageClass(pgFDB fdb, string dsname, int ID, Polygon polygon)
        {
            _fdb = fdb;
            _ID = ID;
            _polygon = polygon;
            _dsname = dsname;
        }

        #region IRasterLayer Members

        public IPolygon Polygon
        {
            get { return _polygon; }
        }

        System.Drawing.Bitmap _bm = null;
        public void BeginPaint(gView.Framework.Carto.IDisplay display, ICancelTracker cancelTracker)
        {
            if (_fdb == null) return;
            try
            {
                DataTable tab = _fdb._conn.Select("IMAGE,X,Y,dx1,dx2,dy1,dy2", _dsname + "_IMAGE_DATA", "ID=" + _ID);
                if (tab == null) return;
                if (tab.Rows.Count != 1) return;
                DataRow row = tab.Rows[0];

                _bm = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(new MemoryStream((byte[])row["IMG"]));
                _X = (double)tab.Rows[0]["X"];
                _Y = (double)tab.Rows[0]["Y"];
                _dx_X = (double)tab.Rows[0]["dx1"];
                _dx_Y = (double)tab.Rows[0]["dx2"];
                _dy_X = (double)tab.Rows[0]["dy1"];
                _dy_Y = (double)tab.Rows[0]["dy2"];
                _iWidth = _bm.Width;
                _iHeight = _bm.Height;
            }
            catch
            {
                EndPaint(cancelTracker);
            }
        }

        public void EndPaint(ICancelTracker cancelTracker)
        {
            if (_bm != null)
            {
                _bm.Dispose();
                _bm = null;
            }
        }

        public System.Drawing.Color GetPixel(double X, double Y)
        {
            return System.Drawing.Color.Transparent;
        }

        public System.Drawing.Bitmap Bitmap
        {
            get { return _bm; }
        }

        public double oX { get { return _X; } }
        public double oY { get { return _Y; } }
        public double dx1 { get { return _dx_X; } }
        public double dx2 { get { return _dx_Y; } }
        public double dy1 { get { return _dy_X; } }
        public double dy2 { get { return _dy_Y; } }

        public ISpatialReference SpatialReference
        {
            get
            {
                return _sRef;
            }
            set
            {
                _sRef = value;
            }
        }

        InterpolationMethod _interpolation = InterpolationMethod.Fast;
        public InterpolationMethod InterpolationMethod
        {
            get
            {
                return _interpolation;
            }
            set
            {
                _interpolation = value;
            }
        }

        public IRasterDataset Dataset
        {
            get { return null; }
        }
        #endregion

        #region IClass Member

        public string Name
        {
            get { return "image"; }
        }

        public string Aliasname
        {
            get { return Name; }
        }

        IDataset IClass.Dataset
        {
            get { return null; }
        }

        #endregion
    }
}
