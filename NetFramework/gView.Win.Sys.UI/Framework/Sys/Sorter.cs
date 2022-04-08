﻿using System.Collections.Generic;

namespace gView.Framework.Sys.UI
{
    public class ExplorerObjectCompareByName : IComparer<gView.Framework.UI.IExplorerObject>
    {
        #region IComparer<IExplorerObject> Member

        public int Compare(gView.Framework.UI.IExplorerObject x, gView.Framework.UI.IExplorerObject y)
        {
            return string.Compare(x.Name.ToLower(), y.Name.ToLower());
        }

        #endregion
    }
}
