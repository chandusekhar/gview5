using System;
using System.Xml;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.system;
using gView.Framework.Symbology;

namespace gView.Framework.Symbology.UI.Controls
{
	/// <summary>
	/// Zusammenfassung f�r FormSymbol.
	/// </summary>
	public class SymbolControl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbSymbolTypes;
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Panel panelProperties;
        private System.Windows.Forms.Splitter splitter1;
		private SymbolCollectionComposer symbolCollectionComposer;
		private ISymbol _symbol=null;
        private TextSymbolAlignment _txtSymbolAlignment = TextSymbolAlignment.Center;

        public SymbolControl()
        {
            InitializeComponent();
        }
        public SymbolControl(ISymbol symbol)
		{
			if(symbol!=null) 
			{
				_symbol=(ISymbol)symbol.Clone();
                if (_symbol is ILabel)
                {
                    _txtSymbolAlignment = ((ILabel)_symbol).TextSymbolAlignment;
                    ((ILabel)_symbol).TextSymbolAlignment = TextSymbolAlignment.Center;
                }
			}

			InitializeComponent();
		}


		public ISymbol Symbol 
		{
			get 
			{
                ISymbol symbol = symbolCollectionComposer.Symbol;
                if (symbol is ILabel)
                {
                    ((ILabel)symbol).TextSymbolAlignment = _txtSymbolAlignment;
                }
                return symbol;
            }
            set
            {
                if (value != null)
                {
                    _symbol = (ISymbol)value.Clone();
                    if (_symbol is ILabel)
                    {
                        _txtSymbolAlignment = ((ILabel)_symbol).TextSymbolAlignment;
                        ((ILabel)_symbol).TextSymbolAlignment = TextSymbolAlignment.Center;
                    }
                }
            }
		}
		/// <summary>
		/// Die verwendeten Ressourcen bereinigen.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Vom Windows Form-Designer generierter Code
		/// <summary>
		/// Erforderliche Methode f�r die Designerunterst�tzung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor ge�ndert werden.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SymbolControl));
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmbSymbolTypes = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelProperties = new System.Windows.Forms.Panel();
            this.symbolCollectionComposer = new gView.Framework.Symbology.UI.Controls.SymbolCollectionComposer();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.cmbSymbolTypes);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Name = "panel1";
            // 
            // cmbSymbolTypes
            // 
            resources.ApplyResources(this.cmbSymbolTypes, "cmbSymbolTypes");
            this.cmbSymbolTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSymbolTypes.Name = "cmbSymbolTypes";
            this.cmbSymbolTypes.SelectedIndexChanged += new System.EventHandler(this.cmbSymbolTypes_SelectedIndexChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // panelProperties
            // 
            resources.ApplyResources(this.panelProperties, "panelProperties");
            this.panelProperties.Name = "panelProperties";
            // 
            // symbolCollectionComposer
            // 
            resources.ApplyResources(this.symbolCollectionComposer, "symbolCollectionComposer");
            this.symbolCollectionComposer.Name = "symbolCollectionComposer";
            this.symbolCollectionComposer.Symbol = null;
            this.symbolCollectionComposer.SelectedSymbolChanged += new gView.Framework.Symbology.UI.Controls.SymbolCollectionComposer.SelectedSymbolChangedEvent(this.symbolCollectionComposer_SelectedSymbolChanged);
            // 
            // splitter1
            // 
            resources.ApplyResources(this.splitter1, "splitter1");
            this.splitter1.Name = "splitter1";
            this.splitter1.TabStop = false;
            // 
            // SymbolControl
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.panelProperties);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.symbolCollectionComposer);
            this.Name = "SymbolControl";
            this.Load += new System.EventHandler(this.FormSymbol_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void FormSymbol_Load(object sender, System.EventArgs e)
		{
			if(_symbol==null) return;

            if (_symbol is ITextSymbol)
            {
                ((ITextSymbol)_symbol).Text = "Label";
            }

			symbolCollectionComposer.AddSymbol(_symbol);
			symbolCollectionComposer.Init();
			//_symbol=(ISymbol)((SymbolCollectionItem)((SymbolCollection)symbolCollectionComposer.Symbol).Symbols[0]).Symbol;
			
			MakeGUI();
		}

		private void MakeGUI() 
		{
			cmbSymbolTypes.Items.Clear();

			if(PlugInManager.IsPlugin(_symbol)) 
			{
				PlugInManager compManager=new PlugInManager();

				foreach(var symbolType in compManager.GetPlugins(Plugins.Type.ISymbol)) 
				{
					ISymbol symbol=compManager.CreateInstance<ISymbol>(symbolType);
                    if (symbol is SymbolCollection) continue;

                    if(_symbol.GetType().Equals(symbol.GetType()))
					    symbol=_symbol;

					if(_symbol is IPointSymbol && symbol is IPointSymbol) 
					{
						cmbSymbolTypes.Items.Add(new SymbolItem(symbol));
					}
					if(_symbol is ILineSymbol && symbol is ILineSymbol) 
					{ 
						cmbSymbolTypes.Items.Add(new SymbolItem(symbol));
					}
					if(_symbol is IFillSymbol && symbol is IFillSymbol) 
					{
						cmbSymbolTypes.Items.Add(new SymbolItem(symbol));
					}
                    if (_symbol is ITextSymbol && symbol is ITextSymbol)
                    {
                        ((ITextSymbol)symbol).Text = "Label";
                        cmbSymbolTypes.Items.Add(new SymbolItem(symbol));
                    }
                    if (_symbol.GetType().Equals(symbol.GetType()) &&
                        cmbSymbolTypes.Items.Count > 0)
                    {
                        cmbSymbolTypes.SelectedItem = cmbSymbolTypes.Items[cmbSymbolTypes.Items.Count - 1];
                    }
				}
			}
		}
		private void cmbSymbolTypes_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			panelProperties.Controls.Clear();

			if(cmbSymbolTypes.SelectedItem==null) return;

			ISymbol symbol=((SymbolItem)cmbSymbolTypes.SelectedItem).Symbol;

			if(symbol is IPropertyPage) 
			{
                Control control = ((IPropertyPage)symbol).PropertyPage(symbol) as Control;
                if (control != null)
                {
                    control.Dock = DockStyle.Fill;
                    if (control.Parent is IPropertyPageUI)
                    {
                        ((IPropertyPageUI)control.Parent).PropertyChanged += new PropertyChangedEvent(PropertyChanged);
                    }
                    else if (control is IPropertyPageUI)
                    {
                        ((IPropertyPageUI)control).PropertyChanged += new PropertyChangedEvent(PropertyChanged);
                    }
                    panelProperties.Controls.Add(control);
                }
			}

			symbolCollectionComposer.ReplaceSelectedSymbol(symbol);
		}

		private void PropertyChanged(object propertyObject) 
		{
			symbolCollectionComposer.Refresh();
		}

		private void symbolCollectionComposer_SelectedSymbolChanged(gView.Framework.Symbology.ISymbol symbol)
		{
			if(symbol==null) return;
			_symbol=symbol;
			MakeGUI();
        }

        #region Classes
        internal class SymbolItem
        {
            private ISymbol _sym;

            public SymbolItem(ISymbol sym)
            {
                _sym = sym;
            }

            public override string ToString()
            {
                return _sym.Name;
            }

            public ISymbol Symbol
            {
                get { return _sym; }
            }
        }
        #endregion
    }
}
