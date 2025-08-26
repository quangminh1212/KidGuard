namespace ChildGuard.UI;

partial class ReportsForm
{
    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.DateTimePicker dtp = null!;
    private System.Windows.Forms.ComboBox cmbType = null!;
    private System.Windows.Forms.Button btnLoad = null!;
    private System.Windows.Forms.DataGridView grid = null!;
    private System.Windows.Forms.Label lblSummary = null!;
    private System.Windows.Forms.Button btnExport = null!;
    private System.Windows.Forms.Panel pnlChart = null!;
    private System.Windows.Forms.Label lblProcFilter = null!;
    private System.Windows.Forms.TextBox txtProcFilter = null!;
    private System.Windows.Forms.CheckBox chkByHour = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.dtp = new System.Windows.Forms.DateTimePicker();
        this.cmbType = new System.Windows.Forms.ComboBox();
        this.btnLoad = new System.Windows.Forms.Button();
        this.grid = new System.Windows.Forms.DataGridView();
        ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
        this.lblSummary = new System.Windows.Forms.Label();
        this.btnExport = new System.Windows.Forms.Button();
        this.pnlChart = new System.Windows.Forms.Panel();
        this.lblProcFilter = new System.Windows.Forms.Label();
        this.txtProcFilter = new System.Windows.Forms.TextBox();
        this.chkByHour = new System.Windows.Forms.CheckBox();
        this.SuspendLayout();
        // dtp
        this.dtp.Location = new System.Drawing.Point(16, 16);
        this.dtp.Name = "dtp";
        this.dtp.Size = new System.Drawing.Size(200, 23);
        // cmbType
        this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbType.Location = new System.Drawing.Point(224, 16);
        this.cmbType.Name = "cmbType";
        this.cmbType.Size = new System.Drawing.Size(160, 23);
        // btnLoad
        this.btnLoad.Location = new System.Drawing.Point(392, 16);
        this.btnLoad.Name = "btnLoad";
        this.btnLoad.Size = new System.Drawing.Size(80, 23);
        this.btnLoad.Text = "Load";
        this.btnLoad.UseVisualStyleBackColor = true;
        this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
        // grid
        this.grid.AllowUserToAddRows = false;
        this.grid.AllowUserToDeleteRows = false;
        this.grid.ReadOnly = true;
        this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.grid.Location = new System.Drawing.Point(16, 56);
        this.grid.Name = "grid";
        this.grid.Size = new System.Drawing.Size(760, 380);
        this.grid.Columns.Add("Timestamp", "Timestamp");
        this.grid.Columns.Add("Type", "Type");
        this.grid.Columns.Add("Data", "Data");
        // lblSummary
        this.lblSummary.AutoSize = true;
        this.lblSummary.Location = new System.Drawing.Point(16, 444);
        this.lblSummary.Name = "lblSummary";
        this.lblSummary.Size = new System.Drawing.Size(88, 15);
        this.lblSummary.Text = "Summary: none";
        // btnExport
        this.btnExport.Location = new System.Drawing.Point(680, 16);
        this.btnExport.Name = "btnExport";
        this.btnExport.Size = new System.Drawing.Size(96, 23);
        this.btnExport.Text = "Export CSV";
        this.btnExport.UseVisualStyleBackColor = true;
        this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
        // pnlChart
        this.pnlChart.Location = new System.Drawing.Point(16, 494);
        this.pnlChart.Name = "pnlChart";
        this.pnlChart.Size = new System.Drawing.Size(760, 120);
        this.pnlChart.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlChart_Paint);
        // lblProcFilter
        this.lblProcFilter.AutoSize = true;
        this.lblProcFilter.Location = new System.Drawing.Point(16, 44);
        this.lblProcFilter.Name = "lblProcFilter";
        this.lblProcFilter.Size = new System.Drawing.Size(76, 15);
        this.lblProcFilter.Text = "Process filter";
        // txtProcFilter
        this.txtProcFilter.Location = new System.Drawing.Point(98, 42);
        this.txtProcFilter.Name = "txtProcFilter";
        this.txtProcFilter.Size = new System.Drawing.Size(286, 23);
        // chkByHour
        this.chkByHour.AutoSize = true;
        this.chkByHour.Location = new System.Drawing.Point(392, 44);
        this.chkByHour.Name = "chkByHour";
        this.chkByHour.Size = new System.Drawing.Size(86, 19);
        this.chkByHour.Text = "Group by hour";
        this.chkByHour.UseVisualStyleBackColor = true;
        // Form
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 630);
        this.Controls.Add(this.chkByHour);
        this.Controls.Add(this.txtProcFilter);
        this.Controls.Add(this.lblProcFilter);
        this.Controls.Add(this.pnlChart);
        this.Controls.Add(this.lblSummary);
        this.Controls.Add(this.grid);
        this.Controls.Add(this.btnExport);
        this.Controls.Add(this.btnLoad);
        this.Controls.Add(this.cmbType);
        this.Controls.Add(this.dtp);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
        this.Name = "ReportsForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Reports";
        this.Load += new System.EventHandler(this.ReportsForm_Load);
        ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
        this.ResumeLayout(false);
    }
}
