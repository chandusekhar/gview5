﻿using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.Framework.DataExplorer.Abstraction;
using gView.Interoperability.GeoServices.Dataset;
using System;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects.Web.GeoServices;

[gView.Framework.system.RegisterPlugIn("5133CFA1-AA5E-47FC-990A-462772158CA5")]
public class GeoServicesServiceLayerExplorerObject : ExplorerObjectCls,
                                                     IExplorerSimpleObject
{
    new private IExplorerObject? _parent;
    private readonly GeoServicesFeatureClass? _fc;

    public GeoServicesServiceLayerExplorerObject()
        : base(null, typeof(GeoServicesFeatureClass), 1)
    {
    }

    public GeoServicesServiceLayerExplorerObject(IExplorerObject parent, GeoServicesFeatureClass featureClass)
        : base(parent, typeof(GeoServicesFeatureClass), 1)
    {
        _parent = parent;
        _fc = featureClass;
    }

    public string Name => _fc != null ? _fc.Name : string.Empty;

    public string FullName
    {
        get
        {
            if (_parent == null || _fc == null)
            {
                return "";
            }

            return _parent.FullName +
                $@"\{_fc.ID}";
        }
    }

    public string Type => "Service layer";

    public string Icon => "basic:code-c";

    async public Task<IExplorerObject?> CreateInstanceByFullName(string fullName, ISerializableExplorerObjectCache? cache)
    {
        if (cache != null && cache.Contains(fullName))
        {
            return cache[fullName];
        }

        fullName = fullName.Replace("/", @"\");
        int pos = fullName.LastIndexOf(@"\");

        if (pos < 0)
        {
            return null;
        }

        string layerId = fullName.Substring(pos + 1);
        string parentFullName = fullName.Substring(0, pos);

        var parentObject =
            (IExplorerParentObject)(await new GeoServicesFolderExplorerObject(null, String.Empty, String.Empty).CreateInstanceByFullName(parentFullName, null)) ??
            (IExplorerParentObject)(await new GeoServicesServiceExplorerObject(null, String.Empty, String.Empty, String.Empty).CreateInstanceByFullName(parentFullName, null));

        if (parentObject != null)
        {
            foreach (var child in await parentObject.ChildObjects())
            {
                if (child.FullName == fullName)
                {
                    cache?.Append(child);
                    return child;
                }
            }
        }

        return null;
    }

    public void Dispose()
    {

    }

    public Task<object?> GetInstanceAsync() => Task.FromResult<object?>(_fc);
}
