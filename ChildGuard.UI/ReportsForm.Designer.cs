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
    private System.Windows.Forms.Button btnExportChart = null!;
    private System.Windows.Forms.DateTimePicker dtpTo = null!;
    private System.Windows.Forms.Label lblTo = null!;
    private System.Windows.Forms.CheckBox chkTimeFilter = null!;
    private System.Windows.Forms.Label lblTimeFrom = null!;
    private System.Windows.Forms.DateTimePicker dtpTimeFrom = null!;
    private System.Windows.Forms.Label lblTimeTo = null!;
    private System.Windows.Forms.DateTimePicker dtpTimeTo = null!;
    private System.Windows.Forms.Panel pnlTrend = null!;
    private System.Windows.Forms.Button btnExportTrendCsv = null!;
    private System.Windows.Forms.Button btnExportTrendChart = null!;

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
        this.btnExportChart = new System.Windows.Forms.Button();
        this.dtpTo = new System.Windows.Forms.DateTimePicker();
        this.lblTo = new System.Windows.Forms.Label();
        this.chkTimeFilter = new System.Windows.Forms.CheckBox();
        this.lblTimeFrom = new System.Windows.Forms.Label();
        this.dtpTimeFrom = new System.Windows.Forms.DateTimePicker();
        this.lblTimeTo = new System.Windows.Forms.Label();
        this.dtpTimeTo = new System.Windows.Forms.DateTimePicker();
        this.btnExportTrendCsv = new System.Windows.Forms.Button();
        this.btnExportTrendChart = new System.Windows.Forms.Button();
        this.pnlTrend = new System.Windows.Forms.Panel();
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
        // btnExportChart
        this.btnExportChart.Location = new System.Drawing.Point(680, 44);
        this.btnExportChart.Name = "btnExportChart";
        this.btnExportChart.Size = new System.Drawing.Size(96, 23);
        this.btnExportChart.Text = "Export Chart";
        this.btnExportChart.UseVisualStyleBackColor = true;
        this.btnExportChart.Click += new System.EventHandler(this.btnExportChart_Click);
        // lblTo (date to)
        this.lblTo.AutoSize = true;
        this.lblTo.Location = new System.Drawing.Point(16, 76);
        this.lblTo.Name = "lblTo";
        this.lblTo.Size = new System.Drawing.Size(21, 15);
        this.lblTo.Text = "To";
        // dtpTo (date to)
        this.dtpTo.Location = new System.Drawing.Point(44, 72);
        this.dtpTo.Name = "dtpTo";
        this.dtpTo.Size = new System.Drawing.Size(200, 23);
        // chkTimeFilter
        this.chkTimeFilter.AutoSize = true;
        this.chkTimeFilter.Location = new System.Drawing.Point(264, 74);
        this.chkTimeFilter.Name = "chkTimeFilter";
        this.chkTimeFilter.Size = new System.Drawing.Size(96, 19);
        this.chkTimeFilter.Text = "Time filter";
        this.chkTimeFilter.UseVisualStyleBackColor = true;
        // lblTimeFrom
        this.lblTimeFrom.AutoSize = true;
        this.lblTimeFrom.Location = new System.Drawing.Point(16, 100);
        this.lblTimeFrom.Name = "lblTimeFrom";
        this.lblTimeFrom.Size = new System.Drawing.Size(37, 15);
        this.lblTimeFrom.Text = "From";
        // dtpTimeFrom
        this.dtpTimeFrom.Format = System.Windows.Forms.DateTimePickerFormat.Time;
        this.dtpTimeFrom.ShowUpDown = true;
        this.dtpTimeFrom.Location = new System.Drawing.Point(60, 96);
        this.dtpTimeFrom.Name = "dtpTimeFrom";
        this.dtpTimeFrom.Size = new System.Drawing.Size(120, 23);
        // lblTimeTo
        this.lblTimeTo.AutoSize = true;
        this.lblTimeTo.Location = new System.Drawing.Point(188, 100);
        this.lblTimeTo.Name = "lblTimeTo";
        this.lblTimeTo.Size = new System.Drawing.Size(21, 15);
        this.lblTimeTo.Text = "To";
        // dtpTimeTo
        this.dtpTimeTo.Format = System.Windows.Forms.DateTimePickerFormat.Time;
        this.dtpTimeTo.ShowUpDown = true;
        this.dtpTimeTo.Location = new System.Drawing.Point(214, 96);
        this.dtpTimeTo.Name = "dtpTimeTo";
        this.dtpTimeTo.Size = new System.Drawing.Size(120, 23);
        // btnExportTrendCsv
        this.btnExportTrendCsv.Location = new System.Drawing.Point(542, 72);
        this.btnExportTrendCsv.Name = "btnExportTrendCsv";
        this.btnExportTrendCsv.Size = new System.Drawing.Size(130, 23);
        this.btnExportTrendCsv.Text = "Export Trend CSV";
        this.btnExportTrendCsv.UseVisualStyleBackColor = true;
        this.btnExportTrendCsv.Click += new System.EventHandler(this.btnExportTrendCsv_Click);
        // btnExportTrendChart
        this.btnExportTrendChart.Location = new System.Drawing.Point(680, 72);
        this.btnExportTrendChart.Name = "btnExportTrendChart";
        this.btnExportTrendChart.Size = new System.Drawing.Size(96, 23);
        this.btnExportTrendChart.Text = "Trend Chart";
        this.btnExportTrendChart.UseVisualStyleBackColor = true;
        this.btnExportTrendChart.Click += new System.EventHandler(this.btnExportTrendChart_Click);
        // pnlTrend
        this.pnlTrend.Location = new System.Drawing.Point(16, 626);
        this.pnlTrend.Name = "pnlTrend";
        this.pnlTrend.Size = new System.Drawing.Size(760, 120);
        this.pnlTrend.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlTrend_Paint);
        // Form
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 840);
        this.Controls.Add(this.pnlTrend);
        this.Controls.Add(this.btnExportTrendChart);
        this.Controls.Add(this.btnExportTrendCsv);
        this.Controls.Add(this.dtpTimeTo);
        this.Controls.Add(this.lblTimeTo);
        this.Controls.Add(this.dtpTimeFrom);
        this.Controls.Add(this.lblTimeFrom);
        this.Controls.Add(this.chkTimeFilter);
        this.Controls.Add(this.dtpTo);
        this.Controls.Add(this.lblTo);
        this.Controls.Add(this.btnExportChart);
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
