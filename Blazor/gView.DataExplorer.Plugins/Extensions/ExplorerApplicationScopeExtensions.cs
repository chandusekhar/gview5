﻿using gView.DataExplorer.Plugins.Services;
using gView.Framework.DataExplorer.Abstraction;
using System;

namespace gView.DataExplorer.Plugins.Extensions;

static public class ExplorerApplicationScopeExtensions
{
    static public ExplorerApplicationScopeService ToScopeService(this IExplorerApplicationScope appScope)
        => appScope is ExplorerApplicationScopeService ?
            (ExplorerApplicationScopeService)appScope :
            throw new Exception("AppScope is not an Service. Appliation Service not registered correctly");
}