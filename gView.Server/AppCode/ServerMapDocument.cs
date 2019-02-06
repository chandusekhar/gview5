﻿using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Data.Relations;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace gView.Server.AppCode
{
    public class ServerMapDocument : IMapDocument, IMapDocumentModules, IPersistable
    {
        private List<IMap> _maps = new List<IMap>();
        // ToDo: ThreadSafe
        private Dictionary<IMap, ModulesPersists> _mapModules = new Dictionary<IMap, ModulesPersists>();
        private ITableRelations _tableRelations;

        public ServerMapDocument()
        {
            _tableRelations = new TableRelations(this);
        }

        #region IMapDocument Member

        public event LayerAddedEvent LayerAdded;
        public event LayerRemovedEvent LayerRemoved;
        public event MapAddedEvent MapAdded;
        public event MapDeletedEvent MapDeleted;
        public event MapScaleChangedEvent MapScaleChanged;
        public event AfterSetFocusMapEvent AfterSetFocusMap;

        public List<IMap> Maps
        {
            get { return _maps; }
        }

        public IMap FocusMap
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public bool AddMap(IMap map)
        {
            if (InternetMapServer.Instance == null) return false;
            if (map == null || _maps.Contains(map)) return true;

            _maps.Add(map);
            if (MapAdded != null) MapAdded(map);
            return true;
        }

        public bool RemoveMap(IMap map)
        {
            if (map == null || !_maps.Contains(map)) return false;

            _maps.Remove(map);
            if (MapDeleted != null) MapDeleted(map);
            return true;
        }

        public IMap this[string mapName]
        {
            get
            {
                foreach (IMap map in _maps)
                {
                    if (map.Name == mapName)
                        return map;
                }
                return null;
            }
        }

        public IMap this[gView.Framework.Data.IDatasetElement layer]
        {
            get
            {
                foreach (IMap map in _maps)
                {
                    foreach (IDatasetElement element in map.MapElements)
                    {
                        if (element == layer)
                            return map;
                    }
                }
                return null;
            }
        }

        public IApplication Application
        {
            get { return null; }
        }

        public ITableRelations TableRelations
        {
            get { return _tableRelations; }
        }

        #endregion

        public bool LoadMapDocument(string path)
        {
            XmlStream stream = new XmlStream("");
            if (stream.ReadStream(path))
            {
                while (_maps.Count > 0)
                {
                    this.RemoveMap((IMap)_maps[0]);
                }

                stream.Load("MapDocument", null, this);
                return true;
            }
            return false;
        }

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return "";
            }
        }

        public void Load(IPersistStream stream)
        {
            while (_maps.Count > 0)
            {
                this.RemoveMap((IMap)_maps[0]);
            }

            IMap map;
            while ((map = (IMap)stream.Load("IMap", null, new gView.Framework.Carto.Map())) != null)
            {
                this.AddMap(map);

                var modules = new ModulesPersists(map);
                stream.Load("Moduls", null, modules);
                _mapModules.Add(map, modules);
            }
        }

        public void Save(IPersistStream stream)
        {
            foreach (IMap map in _maps)
            {
                stream.Save("IMap", map);

                if (_mapModules.ContainsKey(map))
                {
                    stream.Save("Moduls", _mapModules[map]);
                }

            }
        }

        #endregion

        #region IMapDocumentModules

        public IEnumerable<IMapApplicationModule> GetMapModules(IMap map)
        {
            if(_mapModules.ContainsKey(map))
            {
                return _mapModules[map].Modules;
            }

            return new IMapApplicationModule[0];
        }

        #endregion
    }

    internal class ModulesPersists : IPersistable
    {
        private IMap _map;
        private List<IMapApplicationModule> _modules = new List<IMapApplicationModule>();

        public ModulesPersists(IMap map)
        {
            _map = map;
        }

        public IEnumerable<IMapApplicationModule> Modules => _modules;

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            if (_map == null) return;

            while (true)
            {
                ModulePersist module = stream.Load("Module", null, new ModulePersist()) as ModulePersist;
                if (module == null)
                    break;

                if (module.Module != null)
                    _modules.Add(module.Module);
            }
        }

        public void Save(IPersistStream stream)
        {
            if (_map == null) return;

            PlugInManager pluginManager = new PlugInManager();
            foreach(IMapApplicationModule module in _modules)
            {
                if (module is IPersistable)
                {
                    stream.Save("Module", new ModulePersist(module));
                }
            }
        }

        #endregion
    }

    internal class ModulePersist : IPersistable
    {
        private IMapApplicationModule _module = null;

        public ModulePersist(IMapApplicationModule module)
        {
            _module = module;
        }
        public ModulePersist()
        {
        }

        public IMapApplicationModule Module => _module;

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            try
            {
                Guid guid = new Guid(stream.Load("GUID") as string);
                _module = (IMapApplicationModule)PlugInManager.Create(guid);

                if (!(_module is IPersistable)) return;
                ((IPersistable)_module).Load(stream);
            }
            catch { }
        }

        public void Save(IPersistStream stream)
        {
            if (_module == null || !(_module is IPersistable)) return;

            stream.Save("GUID", PlugInManager.PlugInID(_module).ToString());
            ((IPersistable)_module).Save(stream);
        }

        #endregion
    }
}