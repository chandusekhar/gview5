﻿using gView.Framework.Reflection;
using gView.GraphicsEngine;
using gView.Symbology.Framework.Symbology.UI.Rules;
using System.ComponentModel;

namespace gView.Framework.Symbology
{
    public class LegendItemWidthWhithOutlineSymbol
        : LegendItem
    {
        [Browsable(true)]
        [DisplayName("Symbol")]
        [Category("Outline Symbol")]
        [PropertyDescription(AllowNull = true,
                             DefaultInitializaionType = typeof(SimpleLineSymbol))]
        public ISymbol OutlineSymbol { get; set; }

        [Browsable(true)]
        [DisplayName("Smoothingmode")]
        [Category("Outline Symbol")]
        [PropertyDescription(BrowsableRule = typeof(OutlineSymbolBrowsableRule))]
        public SymbolSmoothing SmoothingMode
        {
            get => OutlineSymbol switch
            {
                Symbol symbol => symbol.Smoothingmode,
                SymbolCollection symbolCollection => symbolCollection.SymbolSmothingMode,
                _ => SymbolSmoothing.None,
            };

            set
            {
                if (OutlineSymbol is Symbol)
                {
                    ((Symbol)OutlineSymbol).Smoothingmode = value;
                }
                else if (OutlineSymbol is SymbolCollection)
                {
                    ((SymbolCollection)OutlineSymbol).SymbolSmothingMode = value;
                }
            }
        }

        [Browsable(true)]
        [DisplayName("Color")]
        [Category("Outline Symbol")]
        [PropertyDescription(BrowsableRule = typeof(OutlineSymbolBrowsableRule))]
        public ArgbColor OutlineColor
        {
            get => this switch
            {
                IPenColor penColor => penColor.PenColor,
                _ => ArgbColor.Transparent
            };
            set 
            {
                if(this is IPenColor penColor) penColor.PenColor = value;
            }
        }

        [Browsable(true)]
        [DisplayName("DashStyle")]
        [Category("Outline Symbol")]
        [PropertyDescription(BrowsableRule = typeof(OutlineSymbolBrowsableRule))]
        public LineDashStyle OutlineDashStyle
        {
            get => this switch
            {
                IPenDashStyle penDashStyle => penDashStyle.PenDashStyle,
                _ => LineDashStyle.Solid
            };
            set
            {
                if (this is IPenDashStyle penDashStyle) 
                    penDashStyle.PenDashStyle = value;
            }
        }

        [Browsable(true)]
        [Category("Outline Symbol")]
        [DisplayName("Width")]
        [PropertyDescription(BrowsableRule = typeof(OutlineSymbolBrowsableRule))]
        public float OutlineWidth
        {
            get => this switch
            {
                IPenWidth penWidth => penWidth.PenWidth,
                _ => 0f
            };
            set
            {
                if (this is IPenWidth penWidth)
                    penWidth.PenWidth = value;
            }
        }
    }
}
