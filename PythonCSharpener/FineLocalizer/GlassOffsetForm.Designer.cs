
namespace FineLocalizer
{
    partial class GlassOffsetForm_
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
            this.tbOffsetTx = new System.Windows.Forms.TextBox();
            this.label73 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbCarType = new System.Windows.Forms.ComboBox();
            this.label74 = new System.Windows.Forms.Label();
            this.lblCarType_ = new System.Windows.Forms.Label();
            this.tbOffsetTy = new System.Windows.Forms.TextBox();
            this.btnApply_ = new System.Windows.Forms.Button();
            this.btnCancel_ = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lblCarName_ = new System.Windows.Forms.Label();
            this.tbCarName = new System.Windows.Forms.TextBox();
            this.pictureBoxGlassOffset = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGlassOffset)).BeginInit();
            this.SuspendLayout();
            // 
            // tbOffsetTx
            // 
            this.tbOffsetTx.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.tbOffsetTx.Location = new System.Drawing.Point(425, 130);
            this.tbOffsetTx.Name = "tbOffsetTx";
            this.tbOffsetTx.Size = new System.Drawing.Size(126, 25);
            this.tbOffsetTx.TabIndex = 5;
            // 
            // label73
            // 
            this.label73.AutoSize = true;
            this.label73.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.label73.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label73.Location = new System.Drawing.Point(393, 133);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(24, 18);
            this.label73.TabIndex = 4;
            this.label73.Text = "Tx";
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.cbCarType);
            this.panel1.Location = new System.Drawing.Point(391, 39);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(161, 25);
            this.panel1.TabIndex = 1;
            // 
            // cbCarType
            // 
            this.cbCarType.BackColor = System.Drawing.Color.White;
            this.cbCarType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbCarType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCarType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbCarType.Font = new System.Drawing.Font("굴림", 11.25F);
            this.cbCarType.FormattingEnabled = true;
            this.cbCarType.Location = new System.Drawing.Point(0, 0);
            this.cbCarType.Margin = new System.Windows.Forms.Padding(0);
            this.cbCarType.Name = "cbCarType";
            this.cbCarType.Size = new System.Drawing.Size(159, 23);
            this.cbCarType.TabIndex = 0;
            this.cbCarType.SelectedIndexChanged += new System.EventHandler(this.cbCarType__SelectedIndexChanged);
            // 
            // label74
            // 
            this.label74.AutoSize = true;
            this.label74.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.label74.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label74.Location = new System.Drawing.Point(393, 163);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(24, 18);
            this.label74.TabIndex = 6;
            this.label74.Text = "Ty";
            // 
            // lblCarType_
            // 
            this.lblCarType_.AutoSize = true;
            this.lblCarType_.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.lblCarType_.Location = new System.Drawing.Point(393, 16);
            this.lblCarType_.Name = "lblCarType_";
            this.lblCarType_.Size = new System.Drawing.Size(64, 18);
            this.lblCarType_.TabIndex = 0;
            this.lblCarType_.Text = "CarType";
            // 
            // tbOffsetTy
            // 
            this.tbOffsetTy.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.tbOffsetTy.Location = new System.Drawing.Point(425, 160);
            this.tbOffsetTy.Name = "tbOffsetTy";
            this.tbOffsetTy.Size = new System.Drawing.Size(126, 25);
            this.tbOffsetTy.TabIndex = 7;
            // 
            // btnApply_
            // 
            this.btnApply_.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnApply_.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnApply_.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowFrame;
            this.btnApply_.FlatAppearance.BorderSize = 0;
            this.btnApply_.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Tomato;
            this.btnApply_.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Tomato;
            this.btnApply_.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply_.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.btnApply_.ForeColor = System.Drawing.Color.White;
            this.btnApply_.Location = new System.Drawing.Point(0, 0);
            this.btnApply_.Name = "btnApply_";
            this.btnApply_.Size = new System.Drawing.Size(80, 35);
            this.btnApply_.TabIndex = 0;
            this.btnApply_.Text = "Apply";
            this.btnApply_.UseVisualStyleBackColor = false;
            this.btnApply_.Click += new System.EventHandler(this.btnApply__Click);
            // 
            // btnCancel_
            // 
            this.btnCancel_.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnCancel_.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCancel_.FlatAppearance.BorderSize = 0;
            this.btnCancel_.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Tomato;
            this.btnCancel_.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Tomato;
            this.btnCancel_.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel_.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel_.ForeColor = System.Drawing.Color.White;
            this.btnCancel_.Location = new System.Drawing.Point(0, 0);
            this.btnCancel_.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnCancel_.Name = "btnCancel_";
            this.btnCancel_.Size = new System.Drawing.Size(77, 35);
            this.btnCancel_.TabIndex = 0;
            this.btnCancel_.Text = "Cancel";
            this.btnCancel_.UseVisualStyleBackColor = false;
            this.btnCancel_.Click += new System.EventHandler(this.btnCancel__Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(391, 204);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnApply_);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnCancel_);
            this.splitContainer1.Size = new System.Drawing.Size(161, 35);
            this.splitContainer1.SplitterDistance = 80;
            this.splitContainer1.TabIndex = 8;
            this.splitContainer1.TabStop = false;
            // 
            // lblCarName_
            // 
            this.lblCarName_.AutoSize = true;
            this.lblCarName_.Font = new System.Drawing.Font("Consolas", 11.25F);
            this.lblCarName_.Location = new System.Drawing.Point(393, 71);
            this.lblCarName_.Name = "lblCarName_";
            this.lblCarName_.Size = new System.Drawing.Size(64, 18);
            this.lblCarName_.TabIndex = 2;
            this.lblCarName_.Text = "CarName";
            // 
            // tbCarName
            // 
            this.tbCarName.BackColor = System.Drawing.Color.White;
            this.tbCarName.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbCarName.Location = new System.Drawing.Point(391, 93);
            this.tbCarName.Name = "tbCarName";
            this.tbCarName.ReadOnly = true;
            this.tbCarName.Size = new System.Drawing.Size(160, 26);
            this.tbCarName.TabIndex = 3;
            // 
            // pictureBoxGlassOffset
            // 
            this.pictureBoxGlassOffset.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxGlassOffset.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBoxGlassOffset.ImageLocation = "./Resources/glassoffset.png";
            this.pictureBoxGlassOffset.Location = new System.Drawing.Point(18, 32);
            this.pictureBoxGlassOffset.Name = "pictureBoxGlassOffset";
            this.pictureBoxGlassOffset.Padding = new System.Windows.Forms.Padding(3);
            this.pictureBoxGlassOffset.Size = new System.Drawing.Size(356, 213);
            this.pictureBoxGlassOffset.TabIndex = 9;
            this.pictureBoxGlassOffset.TabStop = false;
            // 
            // GlassOffsetForm_
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(592, 302);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBoxGlassOffset);
            this.Controls.Add(this.tbCarName);
            this.Controls.Add(this.lblCarName_);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.tbOffsetTx);
            this.Controls.Add(this.label73);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label74);
            this.Controls.Add(this.lblCarType_);
            this.Controls.Add(this.tbOffsetTy);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "GlassOffsetForm_";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GlassOffsetForm";
            this.panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxGlassOffset)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbOffsetTx;
        private System.Windows.Forms.Label label73;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cbCarType;
        private System.Windows.Forms.Label label74;
        private System.Windows.Forms.Label lblCarType_;
        private System.Windows.Forms.TextBox tbOffsetTy;
        private System.Windows.Forms.Button btnApply_;
        private System.Windows.Forms.Button btnCancel_;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblCarName_;
        private System.Windows.Forms.TextBox tbCarName;
        private System.Windows.Forms.PictureBox pictureBoxGlassOffset;
    }
}