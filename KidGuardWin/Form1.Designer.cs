namespace KidGuardWin;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

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
        this.txtDomain = new System.Windows.Forms.TextBox();
        this.btnBlock = new System.Windows.Forms.Button();
        this.btnUnblock = new System.Windows.Forms.Button();
        this.lstBlocked = new System.Windows.Forms.ListBox();
        this.lblStatus = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // txtDomain
        // 
        this.txtDomain.Location = new System.Drawing.Point(12, 12);
        this.txtDomain.Name = "txtDomain";
        this.txtDomain.PlaceholderText = "nhập domain (ví dụ: tiktok.com)";
        this.txtDomain.Size = new System.Drawing.Size(356, 23);
        this.txtDomain.TabIndex = 0;
        // 
        // btnBlock
        // 
        this.btnBlock.Location = new System.Drawing.Point(374, 12);
        this.btnBlock.Name = "btnBlock";
        this.btnBlock.Size = new System.Drawing.Size(90, 23);
        this.btnBlock.TabIndex = 1;
        this.btnBlock.Text = "Block";
        this.btnBlock.UseVisualStyleBackColor = true;
        this.btnBlock.Click += new System.EventHandler(this.btnBlock_Click);
        // 
        // btnUnblock
        // 
        this.btnUnblock.Location = new System.Drawing.Point(470, 12);
        this.btnUnblock.Name = "btnUnblock";
        this.btnUnblock.Size = new System.Drawing.Size(90, 23);
        this.btnUnblock.TabIndex = 2;
        this.btnUnblock.Text = "Unblock";
        this.btnUnblock.UseVisualStyleBackColor = true;
        this.btnUnblock.Click += new System.EventHandler(this.btnUnblock_Click);
        // 
        // lstBlocked
        // 
        this.lstBlocked.FormattingEnabled = true;
        this.lstBlocked.ItemHeight = 15;
        this.lstBlocked.Location = new System.Drawing.Point(12, 50);
        this.lstBlocked.Name = "lstBlocked";
        this.lstBlocked.Size = new System.Drawing.Size(548, 274);
        this.lstBlocked.TabIndex = 3;
        // 
        // lblStatus
        // 
        this.lblStatus.AutoSize = true;
        this.lblStatus.Location = new System.Drawing.Point(12, 337);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(0, 15);
        this.lblStatus.TabIndex = 4;
        // 
        // Form1
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(574, 364);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.lstBlocked);
        this.Controls.Add(this.btnUnblock);
        this.Controls.Add(this.btnBlock);
        this.Controls.Add(this.txtDomain);
        this.Name = "Form1";
        this.Text = "KidGuard Win – Website Blocker";
        this.Load += new System.EventHandler(this.Form1_Load);
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.TextBox txtDomain;
    private System.Windows.Forms.Button btnBlock;
    private System.Windows.Forms.Button btnUnblock;
    private System.Windows.Forms.ListBox lstBlocked;
    private System.Windows.Forms.Label lblStatus;
}
