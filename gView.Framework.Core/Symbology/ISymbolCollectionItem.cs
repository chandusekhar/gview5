﻿namespace gView.Framework.Core.Symbology
{
    public interface ISymbolCollectionItem
    {
        bool Visible { get; set; }
        ISymbol Symbol { get; }
    }
}