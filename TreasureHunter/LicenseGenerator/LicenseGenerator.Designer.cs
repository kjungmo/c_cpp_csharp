
namespace LicenseGenerator
{
	partial class LicenseGenerator
	{
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnGenerate = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.tbLicense = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// btnGenerate
			// 
			this.btnGenerate.Location = new System.Drawing.Point(199, 73);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new System.Drawing.Size(149, 23);
			this.btnGenerate.TabIndex = 0;
			this.btnGenerate.Text = "Generate";
			this.btnGenerate.UseVisualStyleBackColor = true;
			this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(27, 27);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(76, 12);
			this.label4.TabIndex = 3;
			this.label4.Text = "License Key";
			// 
			// tbLicense
			// 
			this.tbLicense.Location = new System.Drawing.Point(131, 24);
			this.tbLicense.Name = "tbLicense";
			this.tbLicense.Size = new System.Drawing.Size(430, 21);
			this.tbLicense.TabIndex = 4;
			// 
			// LicenseGenerator
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(579, 124);
			this.Controls.Add(this.tbLicense);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.btnGenerate);
			this.Name = "LicenseGenerator";
			this.Text = "CogAplex License Generator";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnGenerate;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbLicense;
	}
}

