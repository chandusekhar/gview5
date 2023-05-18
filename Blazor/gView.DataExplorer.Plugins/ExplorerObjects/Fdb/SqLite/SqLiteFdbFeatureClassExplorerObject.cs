﻿using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.DataSources.Fdb;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Data;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Events;
using gView.Framework.Geometry;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Fdb.SqLite;

[RegisterPlugIn("16DB07EC-5C30-4C2E-85AC-B49A44188B1A")]
public class SqLiteFdbFeatureClassExplorerObject : ExplorerObjectCls, 
                                                   IExplorerSimpleObject, 
                                                   ISerializableExplorerObject, 
                                                   IExplorerObjectDeletable
                                                   /*, IExplorerObjectContextMenu*/, 
                                                   IExplorerObjectRenamable, 
                                                   IExplorerObjectCreatable
{
    private string _filename = "", _dsname = "", _fcname = "", _type = "";
    private string _icon = "";
    private IFeatureClass? _fc = null;
    private IRasterClass? _rc = null;
    private SqLiteFdbDatasetExplorerObject? _parent = null;
    private bool _isNetwork = false;

    public SqLiteFdbFeatureClassExplorerObject() : 
        base(null, typeof(FeatureClass), 1) 
    { 
    }
    public SqLiteFdbFeatureClassExplorerObject(SqLiteFdbDatasetExplorerObject parent, string filename, string dsname, IDatasetElement element)
        : base(parent, typeof(FeatureClass), 1)
    {
        if (element == null)
        {
            return;
        }

        _parent = parent;
        _filename = filename;
        _dsname = dsname;
        _fcname = element.Title;

        string typePrefix = String.Empty;
        bool isLinked = false;
        if (element.Class is LinkedFeatureClass)
        {
            typePrefix = "Linked ";
            isLinked = true;
        }

        if (element.Class is IRasterCatalogClass)
        {
            _icon = "webgis:tiles";
            _type = typePrefix + "Raster Catalog Layer";
            _rc = (IRasterClass)element.Class;
        }
        else if (element.Class is IRasterClass)
        {
            _icon = "webgis:tiles";
            _type = typePrefix + "Raster Layer";
            _rc = (IRasterClass)element.Class;
        }
        else if (element.Class is IFeatureClass)
        {
            _fc = (IFeatureClass)element.Class;
            switch (_fc.GeometryType)
            {
                case GeometryType.Envelope:
                case GeometryType.Polygon:
                    if (isLinked)
                    {
                        _icon = "webgis:shape-polygon";
                    }
                    else
                    {
                        _icon = "webgis:shape-polygon";
                    }

                    _type = typePrefix + "Polygon Featureclass";
                    break;
                case GeometryType.Multipoint:
                case GeometryType.Point:
                    if (isLinked)
                    {
                        _icon = "basic:dot-filled";
                    }
                    else
                    {
                        _icon = "basic:dot-filled";
                    }

                    _type = typePrefix + "Point Featureclass";
                    break;
                case GeometryType.Polyline:
                    if (isLinked)
                    {
                        _icon = "webgis:shape-polyline";
                    }
                    else
                    {
                        _icon = "webgis:shape-polyline";
                    }

                    _type = typePrefix + "Polyline Featureclass";
                    break;
                case GeometryType.Network:
                    _icon = "webgis:construct-edge-intersect";
                    _type = "Networkclass";
                    _isNetwork = true;
                    break;
            }
        }

        //if (!_isNetwork)
        //{
        //    _contextItems = new ToolStripItem[1];
        //    _contextItems[0] = new ToolStripMenuItem("Tasks");

        //    //_contextItems = new ToolStripItem[1];
        //    //_contextItems[0] = new ToolStripMenuItem("Rebuild Spatial Index...");
        //    //_contextItems[0].Click += new EventHandler(RebuildSpatialIndex_Click);
        //    ToolStripMenuItem item = new ToolStripMenuItem("Shrink Spatial Index...");
        //    item.Click += new EventHandler(ShrinkSpatialIndex_Click);
        //    ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
        //    item = new ToolStripMenuItem("Spatial Index Definition...");
        //    item.Click += new EventHandler(SpatialIndexDef_Click);
        //    ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
        //    item = new ToolStripMenuItem("Repair Spatial Index...");
        //    item.Click += new EventHandler(RepairSpatialIndex_Click);
        //    ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
        //    ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(new ToolStripSeparator());
        //    item = new ToolStripMenuItem("Truncate");
        //    item.Click += new EventHandler(Truncate_Click);
        //    ((ToolStripMenuItem)_contextItems[0]).DropDownItems.Add(item);
        //}
    }

    #region IExplorerObject Members

    public string Name
    {
        get { return _fcname; }
    }

    public string FullName
    {
        get
        {
            return _filename + ((_filename != "") ? @"\" : "") + _dsname + ((_dsname != "") ? @"\" : "") + _fcname;
        }
    }
    public string Type
    {
        get { return String.IsNullOrEmpty(_type) ? "Feature Class" : _type; }
    }

    public string Icon => _icon;

    public void Dispose()
    {
        if (_fc != null)
        {
            _fc = null;
        }
        if (_rc != null)
        {
            _rc = null;
        }
    }
    public Task<object?> GetInstanceAsync()
    {
        if (_fc != null)
        {
            return Task.FromResult<object?>(_fc);
        }

        if (_rc != null)
        {
            return Task.FromResult<object?>(_rc);
        }

        return Task.FromResult<object?>(null);
    }
    #endregion

    #region ISerializableExplorerObject Member

    async public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
    {
        if (cache.Contains(FullName))
        {
            return cache[FullName];
        }

        FullName = FullName.Replace("/", @"\");
        int lastIndex = FullName.LastIndexOf(@"\");
        if (lastIndex == -1)
        {
            return null;
        }

        string dsName = FullName.Substring(0, lastIndex);
        string fcName = FullName.Substring(lastIndex + 1, FullName.Length - lastIndex - 1);

        SqLiteFdbDatasetExplorerObject? dsObject = new SqLiteFdbDatasetExplorerObject();
        dsObject = await dsObject.CreateInstanceByFullName(dsName, cache) as SqLiteFdbDatasetExplorerObject;
        if (dsObject == null || await dsObject.ChildObjects() == null)
        {
            return null;
        }

        foreach (IExplorerObject exObject in await dsObject.ChildObjects())
        {
            if (exObject.Name == fcName)
            {
                cache.Append(exObject);
                return exObject;
            }
        }
        return null;
    }

    #endregion

    //#region IExplorerObjectContextMenu Member

    //public ToolStripItem[] ContextMenuItems
    //{
    //    get
    //    {
    //        return _contextItems;
    //    }
    //}

    //#endregion

    void ShrinkSpatialIndex_Click(object sender, EventArgs e)
    {
        if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
        {
            throw new Exception("Can't rebuild index...\nUncorrect feature class !!!");
        }

        List<IClass> classes = new List<IClass>();
        classes.Add(_fc);

        //SpatialIndexShrinker rebuilder = new SpatialIndexShrinker();
        //rebuilder.RebuildIndices(classes);
    }

    async void SpatialIndexDef_Click(object sender, EventArgs e)
    {
        if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
        {
            throw new Exception("Can't show spatial index definition...\nUncorrect feature class !!!");
        }

        //FormRebuildSpatialIndexDef dlg = await FormRebuildSpatialIndexDef.Create((SQLiteFDB)_fc.Dataset.Database, _fc);
        //if (dlg.ShowDialog() == DialogResult.OK)
        //{
        //}
    }

    void RepairSpatialIndex_Click(object sender, EventArgs e)
    {
        if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
        {
            throw new Exception("Can't show spatial index definition...\nUncorrect feature class !!!");
        }

        //FormRepairSpatialIndexProgress dlg = new FormRepairSpatialIndexProgress((SQLiteFDB)_fc.Dataset.Database, _fc);
        //if (dlg.ShowDialog() == DialogResult.OK)
        //{
        //}
    }

    void Truncate_Click(object sender, EventArgs e)
    {
        if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
        {
            throw new Exception("Can't rebuild index...\nUncorrect feature class !!!");
        }

        ((SQLiteFDB)_fc.Dataset.Database).TruncateTable(_fc.Name);
    }

    #region IExplorerObjectDeletable Member

    public event ExplorerObjectDeletedEvent? ExplorerObjectDeleted = null;

    async public Task<bool> DeleteExplorerObject(ExplorerObjectEventArgs e)
    {
        if (_parent == null)
        {
            return false;
        }

        if (await _parent.DeleteFeatureClass(_fcname))
        {
            if (ExplorerObjectDeleted != null)
            {
                ExplorerObjectDeleted(this);
            }

            return true;
        }
        return false;
    }

    #endregion

    #region IExplorerObjectRenamable Member

    public event ExplorerObjectRenamedEvent? ExplorerObjectRenamed;

    async public Task<bool> RenameExplorerObject(string newName)
    {
        if (_fc == null || _fc.Dataset == null || !(_fc.Dataset.Database is SQLiteFDB))
        {
            throw new Exception("Can't rename featureclass...\nUncorrect feature class !!!");
        }

        if (!await ((SQLiteFDB)_fc.Dataset.Database).RenameFeatureClass(this.Name, newName))
        {
            throw new Exception("Can't rename featureclass...\n" + ((SQLiteFDB)_fc.Dataset.Database).LastErrorMessage);
        }

        _fcname = newName;

        if (ExplorerObjectRenamed != null)
        {
            ExplorerObjectRenamed(this);
        }

        return true;
    }

    #endregion

    #region IExplorerObjectCreatable Member

    public bool CanCreate(IExplorerObject parentExObject)
    {
        if (parentExObject is SqLiteFdbDatasetExplorerObject &&
            !((SqLiteFdbDatasetExplorerObject)parentExObject).IsImageDataset)
        {
            return true;
        }

        return false;
    }

    async public Task<IExplorerObject?> CreateExplorerObjectAsync(IExplorerApplicationScope scope, IExplorerObject parentExObject)
    {
        if (!CanCreate(parentExObject))
        {
            return null;
        }

        var instance = await parentExObject.GetInstanceAsync();
        if (!(instance is IFeatureDataset) || !(((IDataset)instance).Database is SQLiteFDB))
        {
            return null;
        }
        SQLiteFDB? fdb = ((IDataset)instance).Database as SQLiteFDB;

        // ToDo:
        //FormNewFeatureclass dlg = await FormNewFeatureclass.Create(instance as IFeatureDataset);
        //if (dlg.ShowDialog() != DialogResult.OK)
        //{
        //    return null;
        //}

        //IGeometryDef gDef = dlg.GeometryDef;

        //int FCID = await fdb.CreateFeatureClass(
        //    parentExObject.Name,
        //    dlg.FeatureclassName,
        //    gDef,
        //    dlg.Fields);

        //if (FCID < 0)
        //{
        //    throw new Exception("ERROR: " + fdb.LastErrorMessage);
        //}

        //ISpatialIndexDef sIndexDef = await fdb.SpatialIndexDef(parentExObject.Name);
        //await fdb.SetSpatialIndexBounds(dlg.FeatureclassName, "BinaryTree2", dlg.SpatialIndexExtents, 0.55, 200, dlg.SpatialIndexLevels);

        //IDatasetElement element = await ((IFeatureDataset)instance).Element(dlg.FeatureclassName);
        //return new SqLiteFdbFeatureClassExplorerObject(
        //    parentExObject as SqLiteFdbDatasetExplorerObject,
        //    _filename,
        //    parentExObject.Name,
        //    element);

        return null;
    }

    #endregion
}