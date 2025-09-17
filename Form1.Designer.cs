namespace KidGuard;

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
    private System.Windows.Forms.TextBox domainTextBox;
    private System.Windows.Forms.Button blockButton;
    private System.Windows.Forms.Button unblockButton;
    private System.Windows.Forms.Label statusLabel;

    private void InitializeComponent()
    {
        this.domainTextBox = new System.Windows.Forms.TextBox();
        this.blockButton = new System.Windows.Forms.Button();
        this.unblockButton = new System.Windows.Forms.Button();
        this.statusLabel = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // domainTextBox
        // 
        this.domainTextBox.Location = new System.Drawing.Point(20, 20);
        this.domainTextBox.Name = "domainTextBox";
        this.domainTextBox.PlaceholderText = "nhập domain, ví dụ: tiktok.com";
        this.domainTextBox.Size = new System.Drawing.Size(360, 23);
        this.domainTextBox.TabIndex = 0;
        // 
        // blockButton
        // 
        this.blockButton.Location = new System.Drawing.Point(20, 60);
        this.blockButton.Name = "blockButton";
        this.blockButton.Size = new System.Drawing.Size(100, 30);
        this.blockButton.TabIndex = 1;
        this.blockButton.Text = "Block";
        this.blockButton.UseVisualStyleBackColor = true;
        this.blockButton.Click += new System.EventHandler(this.blockButton_Click);
        // 
        // unblockButton
        // 
        this.unblockButton.Location = new System.Drawing.Point(140, 60);
        this.unblockButton.Name = "unblockButton";
        this.unblockButton.Size = new System.Drawing.Size(100, 30);
        this.unblockButton.TabIndex = 2;
        this.unblockButton.Text = "Unblock";
        this.unblockButton.UseVisualStyleBackColor = true;
        this.unblockButton.Click += new System.EventHandler(this.unblockButton_Click);
        // 
        // statusLabel
        // 
        this.statusLabel.AutoSize = true;
        this.statusLabel.Location = new System.Drawing.Point(20, 110);
        this.statusLabel.Name = "statusLabel";
        this.statusLabel.Size = new System.Drawing.Size(125, 15);
        this.statusLabel.TabIndex = 3;
        this.statusLabel.Text = "Trạng thái: Chưa làm";
        // 
        // Form1
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(400, 150);
        this.Controls.Add(this.statusLabel);
        this.Controls.Add(this.unblockButton);
        this.Controls.Add(this.blockButton);
        this.Controls.Add(this.domainTextBox);
        this.Name = "Form1";
        this.Text = "KidGuard - Site Blocker";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion
}
