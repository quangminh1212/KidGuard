namespace ChildGuard.Agent;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.NotifyIcon notifyIcon = null!;
    private System.Windows.Forms.ContextMenuStrip trayMenu = null!;
    private System.Windows.Forms.ToolStripMenuItem mnuStart = null!;
    private System.Windows.Forms.ToolStripMenuItem mnuStop = null!;
    private System.Windows.Forms.ToolStripMenuItem mnuExit = null!;
    private System.Windows.Forms.Timer activeWindowTimer = null!;

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
        this.trayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.mnuStart = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuStop = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
        this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
        this.activeWindowTimer = new System.Windows.Forms.Timer(this.components);
        this.trayMenu.SuspendLayout();
        this.SuspendLayout();
        // 
        // trayMenu
        // 
        this.trayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuStart,
            this.mnuStop,
            this.mnuExit});
        this.trayMenu.Name = "trayMenu";
        this.trayMenu.Size = new System.Drawing.Size(159, 70);
        // 
        // mnuStart
        // 
        this.mnuStart.Name = "mnuStart";
        this.mnuStart.Size = new System.Drawing.Size(158, 22);
        this.mnuStart.Text = "Start Monitoring";
        this.mnuStart.Click += new System.EventHandler(this.mnuStart_Click);
        // 
        // mnuStop
        // 
        this.mnuStop.Name = "mnuStop";
        this.mnuStop.Size = new System.Drawing.Size(158, 22);
        this.mnuStop.Text = "Stop Monitoring";
        this.mnuStop.Click += new System.EventHandler(this.mnuStop_Click);
        // 
        // mnuExit
        // 
        this.mnuExit.Name = "mnuExit";
        this.mnuExit.Size = new System.Drawing.Size(158, 22);
        this.mnuExit.Text = "Exit";
        this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
        // 
        // notifyIcon
        // 
        this.notifyIcon.ContextMenuStrip = this.trayMenu;
        this.notifyIcon.Text = "ChildGuard Agent";
        this.notifyIcon.Visible = true;
        this.notifyIcon.Icon = System.Drawing.SystemIcons.Shield;
        // 
        // activeWindowTimer
        // 
        this.activeWindowTimer.Interval = 1000;
        this.activeWindowTimer.Tick += new System.EventHandler(this.activeWindowTimer_Tick);
        // 
        // Form1
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(240, 100);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "Form1";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "ChildGuard Agent";
        this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
        this.Load += new System.EventHandler(this.Form1_Load);
        this.trayMenu.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    #endregion
}
