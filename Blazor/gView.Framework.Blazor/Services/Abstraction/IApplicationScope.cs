﻿using gView.Framework.Blazor.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Blazor.Services.Abstraction;

public interface IApplicationScope : IApplicationBusyHandling, IDisposable
{
    Task<T?> ShowModalDialog<T>(Type razorComponent,
                                string title,
                                T? model = default(T));

    Task<T?> ShowKnownDialog<T>(KnownDialogs dialog,
                                             string? title = null,
                                             T? model = default);

    void SetClipboardItem(ClipboardItem item);
    Type? GetClipboardItemType();
    IEnumerable<T> GetClipboardElements<T>();
}
