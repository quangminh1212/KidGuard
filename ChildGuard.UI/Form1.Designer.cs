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
        // Form1
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(340, 190);
        this.Controls.Add(this.btnStop);
        this.Controls.Add(this.btnStart);
        this.Controls.Add(this.chkEnableInput);
        this.Controls.Add(this.lblMouse);
        this.Controls.Add(this.lblKeys);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "Form1";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "ChildGuard UI";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion
}
