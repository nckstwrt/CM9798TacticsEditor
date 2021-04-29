
namespace CM9798TacticsEditor
{
    partial class TacticsEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TacticsEditor));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.textBoxExeFile = new System.Windows.Forms.TextBox();
            this.textBoxTacticName = new System.Windows.Forms.TextBox();
            this.buttonSave = new System.Windows.Forms.Button();
            this.textBoxShortCode = new System.Windows.Forms.TextBox();
            this.textBoxTacticsHex = new System.Windows.Forms.TextBox();
            this.buttonReset = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonBrowse);
            this.groupBox1.Controls.Add(this.textBoxExeFile);
            this.groupBox1.Location = new System.Drawing.Point(6, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(300, 53);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "CM2E16.EXE (CM 97/98 v2.93 Only)";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(221, 17);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = "Browse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // textBoxExeFile
            // 
            this.textBoxExeFile.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBoxExeFile.Location = new System.Drawing.Point(6, 19);
            this.textBoxExeFile.Name = "textBoxExeFile";
            this.textBoxExeFile.Size = new System.Drawing.Size(209, 20);
            this.textBoxExeFile.TabIndex = 0;
            // 
            // textBoxTacticName
            // 
            this.textBoxTacticName.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBoxTacticName.Location = new System.Drawing.Point(118, 67);
            this.textBoxTacticName.Name = "textBoxTacticName";
            this.textBoxTacticName.Size = new System.Drawing.Size(103, 20);
            this.textBoxTacticName.TabIndex = 1;
            this.textBoxTacticName.Text = "Diamond Formation";
            this.textBoxTacticName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxTacticName.TextChanged += new System.EventHandler(this.textBoxTacticName_TextChanged);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(236, 529);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // textBoxShortCode
            // 
            this.textBoxShortCode.BackColor = System.Drawing.SystemColors.ControlDark;
            this.textBoxShortCode.Location = new System.Drawing.Point(231, 67);
            this.textBoxShortCode.Name = "textBoxShortCode";
            this.textBoxShortCode.ReadOnly = true;
            this.textBoxShortCode.Size = new System.Drawing.Size(71, 20);
            this.textBoxShortCode.TabIndex = 3;
            this.textBoxShortCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxTacticsHex
            // 
            this.textBoxTacticsHex.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBoxTacticsHex.Location = new System.Drawing.Point(6, 529);
            this.textBoxTacticsHex.Multiline = true;
            this.textBoxTacticsHex.Name = "textBoxTacticsHex";
            this.textBoxTacticsHex.Size = new System.Drawing.Size(224, 48);
            this.textBoxTacticsHex.TabIndex = 4;
            this.textBoxTacticsHex.TextChanged += new System.EventHandler(this.textBoxTacticsHex_TextChanged);
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(236, 554);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(75, 23);
            this.buttonReset.TabIndex = 5;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // TacticsEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(314, 584);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.textBoxTacticsHex);
            this.Controls.Add(this.textBoxShortCode);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.textBoxTacticName);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "TacticsEditor";
            this.Text = "CM9798 Tactics Editor";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.TacticsEditor_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TacticsEditor_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TacticsEditor_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TacticsEditor_MouseUp);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox textBoxExeFile;
        private System.Windows.Forms.TextBox textBoxTacticName;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.TextBox textBoxShortCode;
        private System.Windows.Forms.TextBox textBoxTacticsHex;
        private System.Windows.Forms.Button buttonReset;
    }
}

