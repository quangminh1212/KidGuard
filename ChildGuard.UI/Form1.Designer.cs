namespace ChildGuard.UI;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Label lblKeys = null!;
    private System.Windows.Forms.Label lblMouse = null!;
    private System.Windows.Forms.Button btnStart = null!;
    private System.Windows.Forms.Button btnStop = null!;
    private System.Windows.Forms.CheckBox chkEnableInput = null!;
    private System.Windows.Forms.Timer uiTimer = null!;
    private System.Windows.Forms.MenuStrip menuStrip1 = null!;
    private System.Windows.Forms.ToolStripMenuItem mnuSettings = null!;
    private System.Windows.Forms.ToolStripMenuItem mnuReports = null!;
    private System.Windows.Forms.ToolStripMenuItem mnuPolicy = null!;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.lblKeys = new System.Windows.Forms.Label();
        this.lblMouse = new System.Windows.Forms.Label();
        this.btnStart = new System.Windows.Forms.Button();
        this.btnStop = new System.Windows.Forms.Button();
        this.chkEnableInput = new System.Windows.Forms.CheckBox();
        this.uiTimer = new System.Windows.Forms.Timer(this.components);
        this.menuStrip1 = new System.Windows.Forms.MenuStrip();
        this.mnuSettings = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuReports = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuPolicy = new System.Windows.Forms.ToolStripMenuItem();
        this.menuStrip1.SuspendLayout();
        this.SuspendLayout();
        // 
        // lblKeys
        // 
        this.lblKeys.AutoSize = true;
        this.lblKeys.Location = new System.Drawing.Point(24, 24);
        this.lblKeys.Name = "lblKeys";
        this.lblKeys.Size = new System.Drawing.Size(122, 15);
        this.lblKeys.TabIndex = 0;
        this.lblKeys.Text = "KeyPress Count: 0";
        // 
        // lblMouse
        // 
        this.lblMouse.AutoSize = true;
        this.lblMouse.Location = new System.Drawing.Point(24, 54);
        this.lblMouse.Name = "lblMouse";
        this.lblMouse.Size = new System.Drawing.Size(139, 15);
        this.lblMouse.TabIndex = 1;
        this.lblMouse.Text = "Mouse Event Count: 0";
        // 
        // chkEnableInput
        // 
        this.chkEnableInput.AutoSize = true;
        this.chkEnableInput.Location = new System.Drawing.Point(24, 92);
        this.chkEnableInput.Name = "chkEnableInput";
        this.chkEnableInput.Size = new System.Drawing.Size(187, 19);
        this.chkEnableInput.TabIndex = 2;
        this.chkEnableInput.Text = "Enable input monitoring (LL)";
        this.chkEnableInput.UseVisualStyleBackColor = true;
        // 
        // btnStart
        // 
        this.btnStart.Location = new System.Drawing.Point(24, 127);
        this.btnStart.Name = "btnStart";
        this.btnStart.Size = new System.Drawing.Size(94, 29);
        this.btnStart.TabIndex = 3;
        this.btnStart.Text = "Start";
        this.btnStart.UseVisualStyleBackColor = true;
        this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
        // 
        // btnStop
        // 
        this.btnStop.Location = new System.Drawing.Point(132, 127);
        this.btnStop.Name = "btnStop";
        this.btnStop.Size = new System.Drawing.Size(94, 29);
        this.btnStop.TabIndex = 4;
        this.btnStop.Text = "Stop";
        this.btnStop.UseVisualStyleBackColor = true;
        this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
        // 
        // uiTimer
        // 
        this.uiTimer.Interval = 500;
        this.uiTimer.Tick += new System.EventHandler(this.uiTimer_Tick);
        // 
        // menuStrip1
        //
        this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.mnuSettings, this.mnuReports, this.mnuPolicy });
        this.menuStrip1.Location = new System.Drawing.Point(0, 0);
        this.menuStrip1.Name = "menuStrip1";
        this.menuStrip1.Size = new System.Drawing.Size(340, 24);
        this.menuStrip1.TabIndex = 5;
        this.menuStrip1.Text = "menuStrip1";
        // mnuSettings
        this.mnuSettings.Name = "mnuSettings";
        this.mnuSettings.Size = new System.Drawing.Size(61, 20);
        this.mnuSettings.Text = "Settings";
        this.mnuSettings.Click += new System.EventHandler(this.mnuSettings_Click);
        // mnuReports
        this.mnuReports.Name = "mnuReports";
        this.mnuReports.Size = new System.Drawing.Size(59, 20);
        this.mnuReports.Text = "Reports";
        this.mnuReports.Click += new System.EventHandler(this.mnuReports_Click);
        // mnuPolicy
        this.mnuPolicy.Name = "mnuPolicy";
        this.mnuPolicy.Size = new System.Drawing.Size(86, 20);
        this.mnuPolicy.Text = "Policy Editor";
        this.mnuPolicy.Click += new System.EventHandler(this.mnuPolicy_Click);
        // Form1
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(520, 300);
        this.MinimumSize = new System.Drawing.Size(520, 300);
        this.Controls.Add(this.menuStrip1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MainMenuStrip = this.menuStrip1;
        this.MaximizeBox = false;
        this.Name = "Form1";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "ChildGuard UI";
        this.menuStrip1.ResumeLayout(false);
        this.menuStrip1.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion
}
