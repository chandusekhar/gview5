﻿namespace gView.Plugins.MapTools.Dialogs
{
    partial class FormImportRenderers
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormImportRenderers));
            this.chkFeatureRenderer = new System.Windows.Forms.CheckBox();
            this.chkLabelRenderer = new System.Windows.Forms.CheckBox();
            this.chkSelectionRenderer = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkFeatureRenderer
            // 
            this.chkFeatureRenderer.AccessibleDescription = null;
            this.chkFeatureRenderer.AccessibleName = null;
            resources.ApplyResources(this.chkFeatureRenderer, "chkFeatureRenderer");
            this.chkFeatureRenderer.BackgroundImage = null;
            this.chkFeatureRenderer.Checked = true;
            this.chkFeatureRenderer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkFeatureRenderer.Font = null;
            this.chkFeatureRenderer.Name = "chkFeatureRenderer";
            this.chkFeatureRenderer.UseVisualStyleBackColor = true;
            // 
            // chkLabelRenderer
            // 
            this.chkLabelRenderer.AccessibleDescription = null;
            this.chkLabelRenderer.AccessibleName = null;
            resources.ApplyResources(this.chkLabelRenderer, "chkLabelRenderer");
            this.chkLabelRenderer.BackgroundImage = null;
            this.chkLabelRenderer.Checked = true;
            this.chkLabelRenderer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLabelRenderer.Font = null;
            this.chkLabelRenderer.Name = "chkLabelRenderer";
            this.chkLabelRenderer.UseVisualStyleBackColor = true;
            // 
            // chkSelectionRenderer
            // 
            this.chkSelectionRenderer.AccessibleDescription = null;
            this.chkSelectionRenderer.AccessibleName = null;
            resources.ApplyResources(this.chkSelectionRenderer, "chkSelectionRenderer");
            this.chkSelectionRenderer.BackgroundImage = null;
            this.chkSelectionRenderer.Checked = true;
            this.chkSelectionRenderer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSelectionRenderer.Font = null;
            this.chkSelectionRenderer.Name = "chkSelectionRenderer";
            this.chkSelectionRenderer.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.AccessibleDescription = null;
            this.btnOK.AccessibleName = null;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.BackgroundImage = null;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Font = null;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleDescription = null;
            this.btnCancel.AccessibleName = null;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.BackgroundImage = null;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = null;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // FormImportRenderers
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.chkSelectionRenderer);
            this.Controls.Add(this.chkLabelRenderer);
            this.Controls.Add(this.chkFeatureRenderer);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.Name = "FormImportRenderers";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkFeatureRenderer;
        private System.Windows.Forms.CheckBox chkLabelRenderer;
        private System.Windows.Forms.CheckBox chkSelectionRenderer;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}