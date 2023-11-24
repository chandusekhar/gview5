using gView.Framework.Carto.Rendering.UI;
using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.Symbology;
using gView.Framework.system;
using gView.Framework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace gView.Framework.Carto.Rendering
{
    public enum LegendGroupCartographicMethod { Simple = 0, LegendOrdering = 1, LegendAndSymbolOrdering = 2 }

    [gView.Framework.system.RegisterPlugIn("C7A92674-0120-4f3d-BC03-F1210136B5C6")]
    public class ValueMapRenderer : Cloner, IFeatureRenderer, IPropertyPage, ILegendGroup
    {
        private string _valueField = String.Empty;
        private Dictionary<string, ISymbol> _symbolTable = new Dictionary<string, ISymbol>();
        //private ISymbol _defaultSymbol = null;
        private GeometryType _geometryType = GeometryType.Unknown;
        private SymbolRotation _symbolRotation;
        private bool _useRefscale = true;
        private LegendGroupCartographicMethod _cartoMethod = LegendGroupCartographicMethod.Simple;
        private Dictionary<string, List<IFeature>> _features = null;

        public ValueMapRenderer()
        {
            _symbolRotation = new SymbolRotation();
        }

        public void Dispose()
        {
            foreach (string key in _symbolTable.Keys)
            {
                ISymbol symbol = _symbolTable[key];
                if (symbol == null)
                {
                    continue;
                }

                symbol.Release();
            }
            _symbolTable.Clear();
            //if (_defaultSymbol != null)
            //{
            //    _defaultSymbol.Release();
            //    _defaultSymbol = null;
            //}
        }

        public string ValueField
        {
            get { return _valueField; }
            set
            {
                if (_valueField == value)
                {
                    return;
                }

                /*
                foreach(string key in _symbolTable.Keys) 
                {
                    ISymbol sym=(ISymbol)_symbolTable[key];
                    sym.Release();
                }
                _symbolTable.Clear();
                */
                _valueField = value;
            }
        }

        public ISymbol DefaultSymbol
        {
            get { return this[null]; }
            set
            {
                this[null] = value;
                if (value is ILegendItem)
                {
                    if (value.LegendLabel == "")
                    {
                        value.LegendLabel = "all other values";
                    }
                }
            }
        }

        public GeometryType GeometryType
        {
            get { return _geometryType; }
            set { _geometryType = value; }
        }

        public ISymbol this[string key]
        {
            get
            {
                if (key == null)
                {
                    key = "__gview_all_other_values__";
                }

                if (!_symbolTable.ContainsKey(key))
                {
                    return null;
                }

                return _symbolTable[key];
            }
            set
            {
                if (key == null)
                {
                    key = "__gview_all_other_values__";
                }

                ISymbol symbol;
                if (value == null)
                {
                    symbol = RendererFunctions.CreateStandardSymbol(_geometryType, 150, 3, 7);
                }
                else
                {
                    if (!(value is ISymbol))
                    {
                        return;
                    }

                    symbol = value;
                }

                if (!_symbolTable.ContainsKey(key))
                {
                    _symbolTable.Add(key, symbol);
                }
                else
                {
                    _symbolTable[key].Release();
                    _symbolTable[key] = symbol;
                }
                if (symbol is ILegendItem)
                {
                    if (String.IsNullOrEmpty(symbol.LegendLabel))
                    {
                        symbol.LegendLabel =
                        (key == "__gview_all_other_values__") ? "all other values" : key;
                    }
                }
            }
        }

        public ICollection Keys
        {
            get
            {
                return _symbolTable.Keys;
            }
        }

        public void RemoveSymbol(string key)
        {
            ISymbol symbol = _symbolTable[key];
            if (symbol == null)
            {
                return;
            }

            symbol.Release();
            _symbolTable.Remove(key);
        }

        public SymbolRotation SymbolRotation
        {
            get { return _symbolRotation; }
            set
            {
                if (value == null)
                {
                    _symbolRotation.RotationFieldName = "";
                }
                else
                {
                    _symbolRotation = value;
                }
            }
        }

        public LegendGroupCartographicMethod CartoMethod
        {
            get { return _cartoMethod; }
            set { _cartoMethod = value; }
        }

        public void ReorderLegendItems(string[] keys)
        {
            if (keys == null)
            {
                return;
            }

            Dictionary<string, ISymbol> orderedSymbolTable = new Dictionary<string, ISymbol>();
            foreach (string key in keys)
            {
                if (_symbolTable.ContainsKey(key))
                {
                    orderedSymbolTable.Add(key, _symbolTable[key]);
                }
            }

            foreach (string key in _symbolTable.Keys)
            {
                if (!orderedSymbolTable.ContainsKey(key))
                {
                    orderedSymbolTable.Add(key, _symbolTable[key]);
                }
            }

            _symbolTable = orderedSymbolTable;
        }

        #region IFeatureRenderer Member

        public bool CanRender(IFeatureLayer layer, IMap map)
        {
            if (layer == null)
            {
                return false;
            }

            if (layer.FeatureClass == null)
            {
                return false;
            }
            /*
if (layer.FeatureClass.GeometryType == geometryType.Unknown ||
   layer.FeatureClass.GeometryType == geometryType.Network) return false;
* */
            if (layer.LayerGeometryType == GeometryType.Unknown ||
                layer.LayerGeometryType == GeometryType.Network)
            {
                return false;
            }

            return true;
        }

        public bool HasEffect(IFeatureLayer layer, IMap map)
        {
            return true;
        }

        public bool UseReferenceScale
        {
            get { return _useRefscale; }
            set { _useRefscale = value; }
        }

        public string Name
        {
            get
            {
                return "Value Map Renderer";
            }
        }

        public void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter)
        {
            if (layer.FeatureClass == null)
            {
                return;
            }

            if (layer.FeatureClass.FindField(_valueField) != null)
            {
                filter.AddField(_valueField);
            }

            if (layer.FeatureClass.FindField(_symbolRotation.RotationFieldName) != null)
            {
                filter.AddField(_symbolRotation.RotationFieldName);
            }
        }

        public string Category
        {
            get
            {
                return "Categories";
            }
        }

        /*
		public void Draw(IDisplay disp, IFeatureCursor fCursor, gView.Framework.Carto.Rendering.DrawPhase drawPhase, ICancelTracker cancelTracker)
		{
			IFeature feature;
			
			try 
			{
				ISymbol _symbol=null;
				string key="";
				while((feature=fCursor.NextFeature)!=null) 
				{
					string k=((FieldValue)feature.Fields[0]).Value.ToString();
					if(k!=key) 
					{
						key=k;
						_symbol=(ISymbol)_symbolTable[key];
					}
			
					if(cancelTracker!=null) 
						if(!cancelTracker.Continue) 
							return;

					disp.Draw((_symbol==null) ? _defaultSymbol : _symbol,feature.Shape);

                    if (_labelRenderer != null) _labelRenderer.Draw(disp, feature, drawPhase, cancelTracker);
				}
			} 
			catch(Exception ex)
			{
				string msg=ex.Message;
			}
		}
         * */
        public void Draw(IDisplay disp, IFeature feature)
        {
            object o = feature[_valueField];
            string k = (o != null) ? o.ToString() : null; //feature[_valueField].ToString(); //((FieldValue)feature.Fields[0]).Value.ToString();

            if (_cartoMethod == LegendGroupCartographicMethod.Simple)
            {
                ISymbol _symbol = null;
                if (k != null && _symbolTable.ContainsKey(k))
                {
                    _symbol = _symbolTable[k];
                }

                _symbol = (_symbol == null) ? this[null] : _symbol;
                if (_symbolRotation.RotationFieldName != "")
                {
                    if (_symbol is ISymbolRotation)
                    {
                        object rot = feature[_symbolRotation.RotationFieldName];
                        if (rot != null && rot != DBNull.Value)
                        {
                            ((ISymbolRotation)_symbol).Rotation = (float)_symbolRotation.Convert2DEGAritmetic(Convert.ToDouble(rot));
                        }
                        else
                        {
                            ((ISymbolRotation)_symbol).Rotation = 0;
                        }
                    }
                }
                if (_symbol != null)
                {
                    disp.Draw(_symbol, feature.Shape);
                }
            }
            else
            {
                if (_features == null)
                {
                    _features = new Dictionary<string, List<IFeature>>();
                }

                if (k == null || (k != null && !_symbolTable.ContainsKey(k)))
                {
                    k = "__gview_all_other_values__";
                }

                if (!_symbolTable.ContainsKey(k))
                {
                    return;
                }

                List<IFeature> fList = null;
                if (_features.ContainsKey(k))
                {
                    fList = _features[k];
                }
                else
                {
                    fList = new List<IFeature>();
                    _features.Add(k, fList);
                }

                fList.Add(feature);
            }
        }

        public void StartDrawing(IDisplay display)
        {

        }

        public void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker)
        {
            if (cancelTracker == null)
            {
                cancelTracker = new CancelTracker();
            }

            if (_features != null)
            {
                if (_cartoMethod == LegendGroupCartographicMethod.LegendAndSymbolOrdering && cancelTracker.Continue)
                {
                    int symbolIndex = 0;

                    List<string> keys = new List<string>();
                    foreach (string key in _symbolTable.Keys)
                    {
                        keys.Insert(0, key);
                    }

                    while (true)
                    {
                        bool loop = false;
                        foreach (string key in keys)
                        {
                            if (!_features.ContainsKey(key) || _features[key] == null)
                            {
                                continue;
                            }

                            if (!cancelTracker.Continue)
                            {
                                break;
                            }

                            ISymbol symbol = _symbolTable.ContainsKey(key) ? _symbolTable[key] : null;
                            if (symbol == null)
                            {
                                continue;
                            }

                            if (symbol is ISymbolCollection)
                            {
                                ISymbolCollection symbolCol = (ISymbolCollection)symbol;
                                if (symbolIndex >= symbolCol.Symbols.Count)
                                {
                                    continue;
                                }

                                if (symbolCol.Symbols.Count > symbolIndex + 1)
                                {
                                    loop = true;
                                }

                                ISymbolCollectionItem symbolItem = symbolCol.Symbols[symbolIndex];
                                if (symbolItem.Visible == false || symbolItem.Symbol == null)
                                {
                                    continue;
                                }

                                symbol = symbolItem.Symbol;
                            }
                            else if (symbolIndex > 0)
                            {
                                continue;
                            }

                            List<IFeature> features = _features[key];
                            bool isRotatable = symbol is ISymbolRotation;

                            if (!cancelTracker.Continue)
                            {
                                break;
                            }

                            int counter = 0;
                            foreach (IFeature feature in features)
                            {
                                if (isRotatable && !String.IsNullOrEmpty(_symbolRotation.RotationFieldName))
                                {
                                    object rot = feature[_symbolRotation.RotationFieldName];
                                    if (rot != null && rot != DBNull.Value)
                                    {
                                        ((ISymbolRotation)symbol).Rotation = (float)_symbolRotation.Convert2DEGAritmetic(Convert.ToDouble(rot));
                                    }
                                    else
                                    {
                                        ((ISymbolRotation)symbol).Rotation = 0;
                                    }
                                }

                                disp.Draw(symbol, feature.Shape);

                                counter++;
                                if (counter % 100 == 0 && !cancelTracker.Continue)
                                {
                                    break;
                                }
                            }
                        }

                        if (!loop)
                        {
                            break;
                        }

                        symbolIndex++;
                    }
                }
                else if (_cartoMethod == LegendGroupCartographicMethod.LegendOrdering && cancelTracker.Continue)
                {
                    List<string> keys = new List<string>();
                    foreach (string key in _symbolTable.Keys)
                    {
                        keys.Insert(0, key);
                    }

                    foreach (string key in keys)
                    {
                        if (!_features.ContainsKey(key) || _features[key] == null)
                        {
                            continue;
                        }

                        if (!cancelTracker.Continue)
                        {
                            break;
                        }

                        ISymbol symbol = _symbolTable.ContainsKey(key) ? _symbolTable[key] : null;
                        if (symbol == null)
                        {
                            continue;
                        }

                        List<IFeature> features = _features[key];
                        bool isRotatable = symbol is ISymbolRotation;

                        int counter = 0;
                        foreach (IFeature feature in features)
                        {
                            if (isRotatable && !String.IsNullOrEmpty(_symbolRotation.RotationFieldName))
                            {
                                object rot = feature[_symbolRotation.RotationFieldName];
                                if (rot != null && rot != DBNull.Value)
                                {
                                    ((ISymbolRotation)symbol).Rotation = (float)_symbolRotation.Convert2DEGAritmetic(Convert.ToDouble(rot));
                                }
                                else
                                {
                                    ((ISymbolRotation)symbol).Rotation = 0;
                                }
                            }
                            disp.Draw(symbol, feature.Shape);

                            counter++;
                            if (counter % 100 == 0 && !cancelTracker.Continue)
                            {
                                break;
                            }
                        }
                    }
                }

                _features.Clear();
                _features = null;
            }
        }
        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _valueField = (string)stream.Load("field", "");
            // Kompatibilit�t zu �teren Projekten
            ISymbol defSymbol = (ISymbol)stream.Load("default", null);
            if (defSymbol != null)
            {
                this[null] = defSymbol;
            }

            _cartoMethod = (LegendGroupCartographicMethod)stream.Load("CartographicMethod", (int)LegendGroupCartographicMethod.Simple);

            ValueMapRendererSymbol sym;
            while ((sym = (ValueMapRendererSymbol)stream.Load("ValueMapSymbol", null, new ValueMapRendererSymbol())) != null)
            {
                this[sym._key] = sym._symbol;
            }
            _symbolRotation = (SymbolRotation)stream.Load("SymbolRotation", _symbolRotation, _symbolRotation);
        }

        public void Save(IPersistStream stream)
        {
            stream.Save("field", _valueField);
            //stream.Save("default", _defaultSymbol);
            stream.Save("CartographicMethod", (int)_cartoMethod);

            foreach (string key in _symbolTable.Keys)
            {
                ValueMapRendererSymbol sym = new ValueMapRendererSymbol(key, _symbolTable[key]);
                stream.Save("ValueMapSymbol", sym);
            }
            if (_symbolRotation.RotationFieldName != "")
            {
                stream.Save("SymbolRotation", _symbolRotation);
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
            if (initObject is IFeatureLayer)
            {
                IFeatureLayer layer = (IFeatureLayer)initObject;
                if (layer.FeatureClass == null)
                {
                    return null;
                }

                if (_symbolTable.Count == 0)
                {
                    this[null] = RendererFunctions.CreateStandardSymbol(layer.LayerGeometryType/*layer.FeatureClass.GeometryType*/);
                }

                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                Assembly uiAssembly = Assembly.LoadFrom(appPath + @"/gView.Win.Carto.Rendering.UI.dll");

                IPropertyPanel p = uiAssembly.CreateInstance("gView.Framework.Carto.Rendering.UI.PropertyPage_ValueMapRenderer") as IPropertyPanel;
                if (p != null)
                {
                    return p.PropertyPanel(this, (IFeatureLayer)initObject);
                }
            }

            return null;
        }

        #endregion

        #region ILegendGroup Members

        public int LegendItemCount
        {
            get { return 1 + _symbolTable.Count; }
        }

        public ILegendItem LegendItem(int index)
        {
            if (index < 0)
            {
                return null;
            }
            //if (index == 0) return (ILegendItem)_defaultSymbol;
            if (index <= _symbolTable.Count)
            {
                int count = 0;
                foreach (object lItem in _symbolTable.Values)
                {
                    if (count == index && lItem is ILegendItem)
                    {
                        return (LegendItem)lItem;
                    }

                    count++;
                }
            }
            return null;
        }

        public void SetSymbol(ILegendItem item, ISymbol symbol)
        {
            if (item == symbol)
            {
                return;
            }

            //if (item == _defaultSymbol)
            //{
            //    _defaultSymbol.Release();
            //    _defaultSymbol = symbol;
            //}
            //else
            {
                foreach (string key in _symbolTable.Keys)
                {
                    if (!(_symbolTable[key] is ILegendItem))
                    {
                        continue;
                    }

                    if (_symbolTable[key] == item)
                    {
                        if (symbol is ILegendItem)
                        {
                            symbol.LegendLabel = item.LegendLabel;
                        }

                        _symbolTable[key] = symbol;
                        return;
                    }
                }
            }
        }
        #endregion

        #region IClone2
        public object Clone(CloneOptions options)
        {
            ValueMapRenderer renderer = new ValueMapRenderer();
            renderer._valueField = _valueField;
            //if (_defaultSymbol != null)
            //    renderer._defaultSymbol = (ISymbol)_defaultSymbol.Clone(_useRefscale ? display : null);
            foreach (string key in _symbolTable.Keys)
            {
                ISymbol symbol = _symbolTable[key];
                if (symbol != null)
                {
                    symbol = (ISymbol)symbol.Clone(_useRefscale ? options : null);
                }

                renderer._symbolTable.Add(key, symbol);
            }
            renderer._geometryType = _geometryType;
            renderer._symbolRotation = (SymbolRotation)_symbolRotation.Clone();
            renderer._cartoMethod = _cartoMethod;
            return renderer;
        }
        public void Release()
        {
            Dispose();
        }
        #endregion

        #region IRenderer Member

        public List<ISymbol> Symbols
        {
            get
            {
                return new List<ISymbol>(_symbolTable.Values);
            }
        }

        public bool Combine(IRenderer renderer)
        {
            return false;
        }

        #endregion

        #region IRenderRequiresClone

        public bool RequireClone()
        {
            if (_symbolTable != null)
            {
                foreach (var symbol in _symbolTable.Values)
                {
                    if (symbol != null && symbol.RequireClone())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }

    internal class ValueMapRendererSymbol : IPersistable
    {
        public string _key;
        public ISymbol _symbol;

        public ValueMapRendererSymbol()
        {
        }
        public ValueMapRendererSymbol(string key, ISymbol symbol)
        {
            _key = key;
            _symbol = symbol;
        }

        #region IPersistable Member

        public string PersistID
        {
            get
            {
                return null;
            }
        }

        public void Load(IPersistStream stream)
        {
            _key = (string)stream.Load("key");
            if (_key == "__gview_all_other_values__")
            {
                _key = null;
            }

            _symbol = (ISymbol)stream.Load("symbol");
        }

        public void Save(IPersistStream stream)
        {
            if (_key == null)
            {
                stream.Save("key", "__gview_all_other_values__");
            }
            else
            {
                stream.Save("key", _key);
            }

            stream.Save("symbol", _symbol);
        }

        #endregion
    }
}
