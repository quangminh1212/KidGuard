namespace ChildGuard.Agent;

partial class CountdownForm
{
    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.Label lblMsg = null!;
    private System.Windows.Forms.Label lblCountdown = null!;
    private System.Windows.Forms.Button btnCloseNow = null!;
    private System.Windows.Forms.Button btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.lblMsg = new System.Windows.Forms.Label();
        this.lblCountdown = new System.Windows.Forms.Label();
        this.btnCloseNow = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // lblMsg
        this.lblMsg.AutoSize = true;
        this.lblMsg.Location = new System.Drawing.Point(16, 16);
        this.lblMsg.Name = "lblMsg";
        this.lblMsg.Size = new System.Drawing.Size(220, 15);
        this.lblMsg.Text = "Ứng dụng sẽ bị đóng theo cấu hình.";
        // lblCountdown
        this.lblCountdown.AutoSize = true;
        this.lblCountdown.Location = new System.Drawing.Point(16, 44);
        this.lblCountdown.Name = "lblCountdown";
        this.lblCountdown.Size = new System.Drawing.Size(92, 15);
        this.lblCountdown.Text = "Đóng sau: 10s";
        // btnCloseNow
        this.btnCloseNow.Location = new System.Drawing.Point(16, 72);
        this.btnCloseNow.Name = "btnCloseNow";
        this.btnCloseNow.Size = new System.Drawing.Size(100, 27);
        this.btnCloseNow.Text = "Close Now";
        this.btnCloseNow.UseVisualStyleBackColor = true;
        this.btnCloseNow.Click += new System.EventHandler(this.btnCloseNow_Click);
        // btnCancel
        this.btnCancel.Location = new System.Drawing.Point(128, 72);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(100, 27);
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        // Form
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(320, 115);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnCloseNow);
        this.Controls.Add(this.lblCountdown);
        this.Controls.Add(this.lblMsg);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "CountdownForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "ChildGuard";
        this.Load += new System.EventHandler(this.CountdownForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}

