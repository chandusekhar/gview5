﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Core.Framework.system
{
    public interface IKeyVault
    {
        Task<string> SecretAsync(string uri);

        string Secret(string uri);
    }
}
