
namespace LicenceKeyGenerator
{
    partial class LicenseKeyGenerator
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
            this.mbSerialNumber = new System.Windows.Forms.Label();
            this.macAddress = new System.Windows.Forms.Label();
            this.diskdriveSerialNumber = new System.Windows.Forms.Label();
            this.tbMb = new System.Windows.Forms.TextBox();
            this.tbMac = new System.Windows.Forms.TextBox();
            this.tbDiskdrive = new System.Windows.Forms.TextBox();
            this.encrypted = new System.Windows.Forms.Label();
            this.tbLicense = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mbSerialNumber
            // 
            this.mbSerialNumber.AutoSize = true;
            this.mbSerialNumber.Location = new System.Drawing.Point(33, 32);
            this.mbSerialNumber.Name = "mbSerialNumber";
            this.mbSerialNumber.Size = new System.Drawing.Size(103, 12);
            this.mbSerialNumber.TabIndex = 0;
            this.mbSerialNumber.Text = "Motherboard S/N";
            // 
            // macAddress
            // 
            this.macAddress.AutoSize = true;
            this.macAddress.Location = new System.Drawing.Point(52, 71);
            this.macAddress.Name = "macAddress";
            this.macAddress.Size = new System.Drawing.Size(84, 12);
            this.macAddress.TabIndex = 1;
            this.macAddress.Text = "MAC Address";
            // 
            // diskdriveSerialNumber
            // 
            this.diskdriveSerialNumber.AutoSize = true;
            this.diskdriveSerialNumber.Location = new System.Drawing.Point(80, 122);
            this.diskdriveSerialNumber.Name = "diskdriveSerialNumber";
            this.diskdriveSerialNumber.Size = new System.Drawing.Size(56, 12);
            this.diskdriveSerialNumber.TabIndex = 2;
            this.diskdriveSerialNumber.Text = "HDD S/N";
            // 
            // tbMb
            // 
            this.tbMb.Location = new System.Drawing.Point(154, 29);
            this.tbMb.Name = "tbMb";
            this.tbMb.ReadOnly = true;
            this.tbMb.Size = new System.Drawing.Size(172, 21);
            this.tbMb.TabIndex = 3;
            // 
            // tbMac
            // 
            this.tbMac.Location = new System.Drawing.Point(154, 68);
            this.tbMac.Multiline = true;
            this.tbMac.Name = "tbMac";
            this.tbMac.ReadOnly = true;
            this.tbMac.Size = new System.Drawing.Size(172, 25);
            this.tbMac.TabIndex = 4;
            // 
            // tbDiskdrive
            // 
            this.tbDiskdrive.Location = new System.Drawing.Point(154, 113);
            this.tbDiskdrive.Multiline = true;
            this.tbDiskdrive.Name = "tbDiskdrive";
            this.tbDiskdrive.ReadOnly = true;
            this.tbDiskdrive.Size = new System.Drawing.Size(172, 64);
            this.tbDiskdrive.TabIndex = 5;
            // 
            // encrypted
            // 
            this.encrypted.AutoSize = true;
            this.encrypted.Location = new System.Drawing.Point(60, 220);
            this.encrypted.Name = "encrypted";
            this.encrypted.Size = new System.Drawing.Size(76, 12);
            this.encrypted.TabIndex = 6;
            this.encrypted.Text = "License Key";
            // 
            // tbLicense
            // 
            this.tbLicense.Location = new System.Drawing.Point(154, 220);
            this.tbLicense.Multiline = true;
            this.tbLicense.Name = "tbLicense";
            this.tbLicense.ReadOnly = true;
            this.tbLicense.Size = new System.Drawing.Size(172, 47);
            this.tbLicense.TabIndex = 7;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(251, 191);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Generate";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // LicenseKeyGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 294);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tbLicense);
            this.Controls.Add(this.encrypted);
            this.Controls.Add(this.tbDiskdrive);
            this.Controls.Add(this.tbMac);
            this.Controls.Add(this.tbMb);
            this.Controls.Add(this.diskdriveSerialNumber);
            this.Controls.Add(this.macAddress);
            this.Controls.Add(this.mbSerialNumber);
            this.Name = "LicenseKeyGenerator";
            this.Text = "License Key Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label mbSerialNumber;
        private System.Windows.Forms.Label macAddress;
        private System.Windows.Forms.Label diskdriveSerialNumber;
        private System.Windows.Forms.TextBox tbMb;
        private System.Windows.Forms.TextBox tbMac;
        private System.Windows.Forms.TextBox tbDiskdrive;
        private System.Windows.Forms.Label encrypted;
        private System.Windows.Forms.TextBox tbLicense;
        private System.Windows.Forms.Button button1;
    }
}

