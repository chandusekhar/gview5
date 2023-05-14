﻿using gView.DataExplorer.Plugins.ExplorerObjects.Base;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerObjects;

public class ComputerObject : ExplorerParentObject, IExplorerObject
{
    public ComputerObject()
        : base(null, null, 0)
    {
    }

    #region IExplorerObject Member

    public string Name
    {
        get { return "Computer"; }
    }

    public string FullName
    {
        get { return ""; }
    }

    public string? Type
    {
        get { return ""; }
    }

    public string Icon => "basic:monitor";

    public Task<object?> GetInstanceAsync()
    {
        return Task.FromResult<object?>(null);
    }

    #endregion

    #region ISerializableExplorerObject Member

    public Task<IExplorerObject?> CreateInstanceByFullName(string FullName, ISerializableExplorerObjectCache cache)
    {
        if (FullName == String.Empty)
        {
            return Task.FromResult<IExplorerObject?>(new ComputerObject());
        }

        return Task.FromResult<IExplorerObject?>(null);
    }

    #endregion

    #region IExplorerParentObject Member

    async public override Task<bool> Refresh()
    {
        await base.Refresh();

        string[] drives = System.IO.Directory.GetLogicalDrives();

        foreach (string drive in drives)
        {
            System.IO.DriveInfo info = new System.IO.DriveInfo(drive);

            DriveObject exObject = new DriveObject(this, info.Name.Replace("\\", ""), (uint)info.DriveType);
            base.AddChildObject(exObject);
        }

        ConfigConnections configStream = new ConfigConnections("directories");
        Dictionary<string, string> networkDirectories = configStream.Connections;
        if (networkDirectories != null)
        {
            foreach (string dir in networkDirectories.Keys)
            {
                MappedDriveObject exObject = new MappedDriveObject(this, networkDirectories[dir]);
                base.AddChildObject(exObject);
            }
        }

        PlugInManager compMan = new PlugInManager();

        foreach (var exObjectType in compMan.GetPlugins(Framework.system.Plugins.Type.IExplorerObject))
        {
            var exObject = compMan.CreateInstance<IExplorerObject>(exObjectType);

            if (!(exObject is IExplorerGroupObject))
            {
                continue;
            }

            base.AddChildObject(exObject);
        }

        return true;
    }

    #endregion
}