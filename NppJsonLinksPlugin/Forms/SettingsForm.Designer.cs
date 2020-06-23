using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NppJsonLinksPlugin.PluginInfrastructure;

namespace NppJsonLinksPlugin.Forms
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.defaultMappingPathTextbox = new System.Windows.Forms.TextBox();
            this.soundEnabledComboBox = new System.Windows.Forms.ComboBox();
            this.highlightingEnabledComboBox = new System.Windows.Forms.ComboBox();
            this.loggerModeComboBox = new System.Windows.Forms.ComboBox();
            this.loggerPathTextbox = new System.Windows.Forms.TextBox();
            this.settingsJsonUriTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.jumpToLineDelayTextbox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.resetDefaultsButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // defaultMappingPathTextbox
            // 
            this.defaultMappingPathTextbox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.defaultMappingPathTextbox.Location = new System.Drawing.Point(230, 138);
            this.defaultMappingPathTextbox.Margin = new System.Windows.Forms.Padding(0);
            this.defaultMappingPathTextbox.Name = "defaultMappingPathTextbox";
            this.defaultMappingPathTextbox.Size = new System.Drawing.Size(630, 22);
            this.defaultMappingPathTextbox.TabIndex = 4;
            // 
            // soundEnabledComboBox
            // 
            this.soundEnabledComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.soundEnabledComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.soundEnabledComboBox.FormattingEnabled = true;
            this.soundEnabledComboBox.Items.AddRange(new object[] {"false", "true"});
            this.soundEnabledComboBox.Location = new System.Drawing.Point(230, 200);
            this.soundEnabledComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.soundEnabledComboBox.Name = "soundEnabledComboBox";
            this.soundEnabledComboBox.Size = new System.Drawing.Size(630, 24);
            this.soundEnabledComboBox.TabIndex = 7;
            // 
            // highlightingEnabledComboBox
            // 
            this.highlightingEnabledComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.highlightingEnabledComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.highlightingEnabledComboBox.FormattingEnabled = true;
            this.highlightingEnabledComboBox.Items.AddRange(new object[] {"false", "true"});
            this.highlightingEnabledComboBox.Location = new System.Drawing.Point(230, 167);
            this.highlightingEnabledComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.highlightingEnabledComboBox.Name = "highlightingEnabledComboBox";
            this.highlightingEnabledComboBox.Size = new System.Drawing.Size(630, 24);
            this.highlightingEnabledComboBox.TabIndex = 5;
            // 
            // loggerModeComboBox
            // 
            this.loggerModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.loggerModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.loggerModeComboBox.FormattingEnabled = true;
            this.loggerModeComboBox.Items.AddRange(new object[] {"ENABLED", "ONLY_ERRORS", "DISABLED"});
            this.loggerModeComboBox.Location = new System.Drawing.Point(230, 87);
            this.loggerModeComboBox.Margin = new System.Windows.Forms.Padding(0);
            this.loggerModeComboBox.Name = "loggerModeComboBox";
            this.loggerModeComboBox.Size = new System.Drawing.Size(630, 24);
            this.loggerModeComboBox.TabIndex = 3;
            // 
            // loggerPathTextbox
            // 
            this.loggerPathTextbox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.loggerPathTextbox.Location = new System.Drawing.Point(230, 58);
            this.loggerPathTextbox.Margin = new System.Windows.Forms.Padding(0);
            this.loggerPathTextbox.Name = "loggerPathTextbox";
            this.loggerPathTextbox.Size = new System.Drawing.Size(630, 22);
            this.loggerPathTextbox.TabIndex = 2;
            // 
            // settingsJsonUriTextbox
            // 
            this.settingsJsonUriTextbox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsJsonUriTextbox.Location = new System.Drawing.Point(230, 10);
            this.settingsJsonUriTextbox.Margin = new System.Windows.Forms.Padding(0);
            this.settingsJsonUriTextbox.Name = "settingsJsonUriTextbox";
            this.settingsJsonUriTextbox.Size = new System.Drawing.Size(630, 22);
            this.settingsJsonUriTextbox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(230, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "URI of settings.json";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BackColor = System.Drawing.SystemColors.Control;
            this.label2.Location = new System.Drawing.Point(0, 48);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(230, 32);
            this.label2.TabIndex = 2;
            this.label2.Text = "Logger path:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.BackColor = System.Drawing.SystemColors.Control;
            this.label3.Location = new System.Drawing.Point(0, 80);
            this.label3.Margin = new System.Windows.Forms.Padding(0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(230, 32);
            this.label3.TabIndex = 4;
            this.label3.Text = "Logger mode:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.SystemColors.Control;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(0, 160);
            this.label4.Margin = new System.Windows.Forms.Padding(0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(230, 32);
            this.label4.TabIndex = 6;
            this.label4.Text = "Highlighting enabled:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.BackColor = System.Drawing.SystemColors.Control;
            this.label6.Location = new System.Drawing.Point(0, 192);
            this.label6.Margin = new System.Windows.Forms.Padding(0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(230, 32);
            this.label6.TabIndex = 10;
            this.label6.Text = "Sound enabled:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.BackColor = System.Drawing.SystemColors.Control;
            this.label7.Location = new System.Drawing.Point(0, 128);
            this.label7.Margin = new System.Windows.Forms.Padding(0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(230, 32);
            this.label7.TabIndex = 13;
            this.label7.Text = "Default mapping path prefix:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 230F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 630F));
            this.tableLayoutPanel1.Controls.Add(this.jumpToLineDelayTextbox, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 11);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.settingsJsonUriTextbox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.loggerPathTextbox, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.loggerModeComboBox, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.soundEnabledComboBox, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.defaultMappingPathTextbox, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.highlightingEnabledComboBox, 1, 7);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(20, 16);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 12;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(860, 350);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // jumpToLineDelayTextbox
            // 
            this.jumpToLineDelayTextbox.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.jumpToLineDelayTextbox.Location = new System.Drawing.Point(230, 234);
            this.jumpToLineDelayTextbox.Margin = new System.Windows.Forms.Padding(0);
            this.jumpToLineDelayTextbox.Name = "jumpToLineDelayTextbox";
            this.jumpToLineDelayTextbox.Size = new System.Drawing.Size(630, 22);
            this.jumpToLineDelayTextbox.TabIndex = 8;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.BackColor = System.Drawing.SystemColors.Control;
            this.label8.Location = new System.Drawing.Point(0, 224);
            this.label8.Margin = new System.Windows.Forms.Padding(0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(230, 32);
            this.label8.TabIndex = 14;
            this.label8.Text = "Jump to line delay:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.ColumnCount = 5;
            this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
            this.panel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.panel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.panel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.panel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.panel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.panel1.Controls.Add(this.cancelButton, 1, 2);
            this.panel1.Controls.Add(this.saveButton, 3, 2);
            this.panel1.Controls.Add(this.resetDefaultsButton, 3, 0);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 275);
            this.panel1.Name = "panel1";
            this.panel1.RowCount = 4;
            this.panel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.panel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.panel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.panel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panel1.Size = new System.Drawing.Size(854, 104);
            this.panel1.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cancelButton.Location = new System.Drawing.Point(128, 40);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(0);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(256, 24);
            this.cancelButton.TabIndex = 10;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.saveButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveButton.Location = new System.Drawing.Point(469, 40);
            this.saveButton.Margin = new System.Windows.Forms.Padding(0);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(256, 24);
            this.saveButton.TabIndex = 11;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // resetDefaultsButton
            // 
            this.panel1.SetColumnSpan(this.resetDefaultsButton, 2);
            this.resetDefaultsButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resetDefaultsButton.Location = new System.Drawing.Point(469, 0);
            this.resetDefaultsButton.Margin = new System.Windows.Forms.Padding(0);
            this.resetDefaultsButton.Name = "resetDefaultsButton";
            this.resetDefaultsButton.Size = new System.Drawing.Size(385, 24);
            this.resetDefaultsButton.TabIndex = 9;
            this.resetDefaultsButton.Text = "Reset to default";
            this.resetDefaultsButton.UseVisualStyleBackColor = true;
            this.resetDefaultsButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 382);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SettingsForm";
            this.Padding = new System.Windows.Forms.Padding(20, 16, 20, 16);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SettingsForm";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox defaultMappingPathTextbox;
        private System.Windows.Forms.ComboBox highlightingEnabledComboBox;
        private System.Windows.Forms.TextBox jumpToLineDelayTextbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox loggerModeComboBox;
        private System.Windows.Forms.TextBox loggerPathTextbox;
        private System.Windows.Forms.TableLayoutPanel panel1;
        private System.Windows.Forms.Button resetDefaultsButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.TextBox settingsJsonUriTextbox;
        private System.Windows.Forms.ComboBox soundEnabledComboBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

        #endregion
    }
}