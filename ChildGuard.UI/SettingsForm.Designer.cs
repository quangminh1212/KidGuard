namespace ChildGuard.UI;

partial class SettingsForm
{
    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.CheckBox chkInput = null!;
    private System.Windows.Forms.CheckBox chkActiveWindow = null!;
    private System.Windows.Forms.Button btnSave = null!;
    private System.Windows.Forms.Button btnCancel = null!;
    private System.Windows.Forms.Label lblPath = null!;
    private System.Windows.Forms.TextBox txtBlocked = null!;
    private System.Windows.Forms.Label lblBlocked = null!;
    private System.Windows.Forms.DateTimePicker dtStart = null!;
    private System.Windows.Forms.DateTimePicker dtEnd = null!;
    private System.Windows.Forms.Label lblQuiet = null!;
    private System.Windows.Forms.NumericUpDown numRetention = null!;
    private System.Windows.Forms.Label lblRetention = null!;

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
        this.txtBlocked = new System.Windows.Forms.TextBox();
        this.lblBlocked = new System.Windows.Forms.Label();
        this.dtStart = new System.Windows.Forms.DateTimePicker();
        this.dtEnd = new System.Windows.Forms.DateTimePicker();
        this.lblQuiet = new System.Windows.Forms.Label();
        this.numRetention = new System.Windows.Forms.NumericUpDown();
        this.lblRetention = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.numRetention)).BeginInit();
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
        // lblBlocked
        this.lblBlocked.AutoSize = true;
        this.lblBlocked.Location = new System.Drawing.Point(22, 82);
        this.lblBlocked.Name = "lblBlocked";
        this.lblBlocked.Size = new System.Drawing.Size(184, 15);
        this.lblBlocked.Text = "Blocked processes (one per line):";
        // txtBlocked
        this.txtBlocked.Location = new System.Drawing.Point(22, 100);
        this.txtBlocked.Multiline = true;
        this.txtBlocked.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtBlocked.Size = new System.Drawing.Size(376, 100);
        // lblQuiet
        this.lblQuiet.AutoSize = true;
        this.lblQuiet.Location = new System.Drawing.Point(22, 210);
        this.lblQuiet.Name = "lblQuiet";
        this.lblQuiet.Size = new System.Drawing.Size(126, 15);
        this.lblQuiet.Text = "Quiet hours (local time):";
        // dtStart
        this.dtStart.Format = System.Windows.Forms.DateTimePickerFormat.Time;
        this.dtStart.ShowUpDown = true;
        this.dtStart.Location = new System.Drawing.Point(22, 230);
        this.dtStart.Width = 100;
        // dtEnd
        this.dtEnd.Format = System.Windows.Forms.DateTimePickerFormat.Time;
        this.dtEnd.ShowUpDown = true;
        this.dtEnd.Location = new System.Drawing.Point(132, 230);
        this.dtEnd.Width = 100;
        // lblRetention
        this.lblRetention.AutoSize = true;
        this.lblRetention.Location = new System.Drawing.Point(22, 262);
        this.lblRetention.Name = "lblRetention";
        this.lblRetention.Size = new System.Drawing.Size(187, 15);
        this.lblRetention.Text = "Log retention (days, >=1, default 14):";
        // numRetention
        this.numRetention.Minimum = 1;
        this.numRetention.Maximum = 365;
        this.numRetention.Value = 14;
        this.numRetention.Location = new System.Drawing.Point(215, 260);
        this.numRetention.Width = 60;
        // btnSave
        this.btnSave.Location = new System.Drawing.Point(22, 300);
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
        this.lblPath.Location = new System.Drawing.Point(22, 340);
        this.lblPath.Name = "lblPath";
        this.lblPath.Size = new System.Drawing.Size(86, 15);
        this.lblPath.Text = "Config path: -";
        // SettingsForm
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(430, 380);
        this.Controls.Add(this.lblPath);
        this.Controls.Add(this.numRetention);
        this.Controls.Add(this.lblRetention);
        this.Controls.Add(this.dtEnd);
        this.Controls.Add(this.dtStart);
        this.Controls.Add(this.lblQuiet);
        this.Controls.Add(this.txtBlocked);
        this.Controls.Add(this.lblBlocked);
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
        ((System.ComponentModel.ISupportInitialize)(this.numRetention)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
