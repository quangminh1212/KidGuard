namespace ChildGuard.UI;

partial class SettingsForm
{
    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.CheckBox chkInput = null!;
    private System.Windows.Forms.CheckBox chkActiveWindow = null!;
    private System.Windows.Forms.Button btnSave = null!;
    private System.Windows.Forms.Button btnCancel = null!;
    private System.Windows.Forms.Label lblPath = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.chkInput = new System.Windows.Forms.CheckBox();
        this.chkActiveWindow = new System.Windows.Forms.CheckBox();
        this.btnSave = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.lblPath = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // chkInput
        this.chkInput.AutoSize = true;
        this.chkInput.Location = new System.Drawing.Point(22, 22);
        this.chkInput.Name = "chkInput";
        this.chkInput.Size = new System.Drawing.Size(214, 19);
        this.chkInput.Text = "Enable input monitoring (keyboard/mouse)";
        this.chkInput.UseVisualStyleBackColor = true;
        // chkActiveWindow
        this.chkActiveWindow.AutoSize = true;
        this.chkActiveWindow.Location = new System.Drawing.Point(22, 52);
        this.chkActiveWindow.Name = "chkActiveWindow";
        this.chkActiveWindow.Size = new System.Drawing.Size(156, 19);
        this.chkActiveWindow.Text = "Enable active window log";
        this.chkActiveWindow.UseVisualStyleBackColor = true;
        // btnSave
        this.btnSave.Location = new System.Drawing.Point(22, 98);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(84, 27);
        this.btnSave.Text = "Save";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
        // btnCancel
        this.btnCancel.Location = new System.Drawing.Point(122, 98);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(84, 27);
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        // lblPath
        this.lblPath.AutoSize = true;
        this.lblPath.Location = new System.Drawing.Point(22, 140);
        this.lblPath.Name = "lblPath";
        this.lblPath.Size = new System.Drawing.Size(86, 15);
        this.lblPath.Text = "Config path: -";
        // SettingsForm
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(420, 180);
        this.Controls.Add(this.lblPath);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.chkActiveWindow);
        this.Controls.Add(this.chkInput);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SettingsForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Settings";
        this.Load += new System.EventHandler(this.SettingsForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
