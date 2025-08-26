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
    private System.Windows.Forms.TextBox txtAllowedQuiet = null!;
    private System.Windows.Forms.Label lblAllowedQuiet = null!;
    private System.Windows.Forms.DateTimePicker dtStart = null!;
    private System.Windows.Forms.DateTimePicker dtEnd = null!;
    private System.Windows.Forms.Label lblQuiet = null!;
    private System.Windows.Forms.NumericUpDown numRetention = null!;
    private System.Windows.Forms.Label lblRetention = null!;
    private System.Windows.Forms.Label lblCloseWarn = null!;
    private System.Windows.Forms.NumericUpDown numCloseWarn = null!;
    private System.Windows.Forms.Label lblMaxSize = null!;
    private System.Windows.Forms.NumericUpDown numMaxSize = null!;
    private System.Windows.Forms.Label lblAdditionalQuiet = null!;
    private System.Windows.Forms.TextBox txtAdditionalQuiet = null!;

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
        this.txtAllowedQuiet = new System.Windows.Forms.TextBox();
        this.lblAllowedQuiet = new System.Windows.Forms.Label();
        this.dtStart = new System.Windows.Forms.DateTimePicker();
        this.dtEnd = new System.Windows.Forms.DateTimePicker();
        this.lblQuiet = new System.Windows.Forms.Label();
        this.numRetention = new System.Windows.Forms.NumericUpDown();
        this.lblRetention = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.numRetention)).BeginInit();
        this.lblCloseWarn = new System.Windows.Forms.Label();
        this.numCloseWarn = new System.Windows.Forms.NumericUpDown();
        this.lblMaxSize = new System.Windows.Forms.Label();
        this.numMaxSize = new System.Windows.Forms.NumericUpDown();
        ((System.ComponentModel.ISupportInitialize)(this.numCloseWarn)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numMaxSize)).BeginInit();
        this.lblAdditionalQuiet = new System.Windows.Forms.Label();
        this.txtAdditionalQuiet = new System.Windows.Forms.TextBox();
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
        // lblAllowedQuiet
        this.lblAllowedQuiet.AutoSize = true;
        this.lblAllowedQuiet.Location = new System.Drawing.Point(22, 210);
        this.lblAllowedQuiet.Name = "lblAllowedQuiet";
        this.lblAllowedQuiet.Size = new System.Drawing.Size(300, 15);
        this.lblAllowedQuiet.Text = "Allowed processes during Quiet Hours (one per line):";
        // txtAllowedQuiet
        this.txtAllowedQuiet.Location = new System.Drawing.Point(22, 228);
        this.txtAllowedQuiet.Multiline = true;
        this.txtAllowedQuiet.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtAllowedQuiet.Size = new System.Drawing.Size(376, 80);
        // lblQuiet
        this.lblQuiet.AutoSize = true;
        this.lblQuiet.Location = new System.Drawing.Point(22, 316);
        this.lblQuiet.Name = "lblQuiet";
        this.lblQuiet.Size = new System.Drawing.Size(126, 15);
        this.lblQuiet.Text = "Quiet hours (local time):";
        // dtStart
        this.dtStart.Format = System.Windows.Forms.DateTimePickerFormat.Time;
        this.dtStart.ShowUpDown = true;
        this.dtStart.Location = new System.Drawing.Point(22, 336);
        this.dtStart.Width = 100;
        // dtEnd
        this.dtEnd.Format = System.Windows.Forms.DateTimePickerFormat.Time;
        this.dtEnd.ShowUpDown = true;
        this.dtEnd.Location = new System.Drawing.Point(132, 336);
        this.dtEnd.Width = 100;
        // lblRetention
        this.lblRetention.AutoSize = true;
        this.lblRetention.Location = new System.Drawing.Point(22, 368);
        this.lblRetention.Name = "lblRetention";
        this.lblRetention.Size = new System.Drawing.Size(187, 15);
        this.lblRetention.Text = "Log retention (days, >=1, default 14):";
        // numRetention
        this.numRetention.Minimum = 1;
        this.numRetention.Maximum = 365;
        this.numRetention.Value = 14;
        this.numRetention.Location = new System.Drawing.Point(215, 366);
        this.numRetention.Width = 60;
        // lblCloseWarn
        this.lblCloseWarn.AutoSize = true;
        this.lblCloseWarn.Location = new System.Drawing.Point(22, 398);
        this.lblCloseWarn.Name = "lblCloseWarn";
        this.lblCloseWarn.Size = new System.Drawing.Size(172, 15);
        this.lblCloseWarn.Text = "Block close warning (seconds):";
        // numCloseWarn
        this.numCloseWarn.Minimum = 0;
        this.numCloseWarn.Maximum = 300;
        this.numCloseWarn.Value = 10;
        this.numCloseWarn.Location = new System.Drawing.Point(215, 396);
        this.numCloseWarn.Width = 60;
        // lblMaxSize
        this.lblMaxSize.AutoSize = true;
        this.lblMaxSize.Location = new System.Drawing.Point(22, 428);
        this.lblMaxSize.Name = "lblMaxSize";
        this.lblMaxSize.Size = new System.Drawing.Size(101, 15);
        this.lblMaxSize.Text = "Log max size (MB):";
        // numMaxSize
        this.numMaxSize.Minimum = 0;
        this.numMaxSize.Maximum = 10000;
        this.numMaxSize.Value = 200;
        this.numMaxSize.Location = new System.Drawing.Point(215, 426);
        this.numMaxSize.Width = 80;
        // btnSave
        this.btnSave.Location = new System.Drawing.Point(22, 466);
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
        // lblAdditionalQuiet
        this.lblAdditionalQuiet.AutoSize = true;
        this.lblAdditionalQuiet.Location = new System.Drawing.Point(22, 458);
        this.lblAdditionalQuiet.Name = "lblAdditionalQuiet";
        this.lblAdditionalQuiet.Size = new System.Drawing.Size(243, 15);
        this.lblAdditionalQuiet.Text = "Additional quiet windows (HH:mm-HH:mm, per line):";
        // txtAdditionalQuiet
        this.txtAdditionalQuiet.Location = new System.Drawing.Point(22, 476);
        this.txtAdditionalQuiet.Multiline = true;
        this.txtAdditionalQuiet.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtAdditionalQuiet.Size = new System.Drawing.Size(376, 80);
        // lblPath
        this.lblPath.AutoSize = true;
        this.lblPath.Location = new System.Drawing.Point(22, 566);
        this.lblPath.Name = "lblPath";
        this.lblPath.Size = new System.Drawing.Size(86, 15);
        this.lblPath.Text = "Config path: -";
        // SettingsForm
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(480, 600);
        this.Controls.Add(this.lblPath);
        this.Controls.Add(this.txtAdditionalQuiet);
        this.Controls.Add(this.lblAdditionalQuiet);
        this.Controls.Add(this.txtAllowedQuiet);
        this.Controls.Add(this.lblAllowedQuiet);
        this.Controls.Add(this.numMaxSize);
        this.Controls.Add(this.lblMaxSize);
        this.Controls.Add(this.numCloseWarn);
        this.Controls.Add(this.lblCloseWarn);
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
        ((System.ComponentModel.ISupportInitialize)(this.numCloseWarn)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numMaxSize)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
