namespace ChildGuard.UI;

partial class PolicyEditorForm
{
    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.TextBox txtJson = null!;
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
        this.txtJson = new System.Windows.Forms.TextBox();
        this.btnSave = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.lblPath = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // txtJson
        this.txtJson.Location = new System.Drawing.Point(16, 16);
        this.txtJson.Multiline = true;
        this.txtJson.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        this.txtJson.AcceptsReturn = true;
        this.txtJson.AcceptsTab = true;
        this.txtJson.WordWrap = false;
        this.txtJson.Size = new System.Drawing.Size(600, 320);
        // btnSave
        this.btnSave.Location = new System.Drawing.Point(16, 344);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(84, 27);
        this.btnSave.Text = "Save";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
        // btnCancel
        this.btnCancel.Location = new System.Drawing.Point(116, 344);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(84, 27);
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        // lblPath
        this.lblPath.AutoSize = true;
        this.lblPath.Location = new System.Drawing.Point(16, 380);
        this.lblPath.Name = "lblPath";
        this.lblPath.Size = new System.Drawing.Size(94, 15);
        this.lblPath.Text = "Config path: -";
        // Form
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(700, 450);
        this.MinimumSize = new System.Drawing.Size(640, 430);
        this.Controls.Add(this.lblPath);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.txtJson);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Policy Editor";
        this.Load += new System.EventHandler(this.PolicyEditorForm_Load);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}

