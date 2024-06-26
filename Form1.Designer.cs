namespace zFernusExporter
{
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
            components = new System.ComponentModel.Container();
            TB_Logs = new TextBox();
            GB_Logs = new GroupBox();
            B_Start = new Button();
            L_ProcessStatus = new Label();
            linkLabel1 = new LinkLabel();
            T_ProcessTimer = new System.Windows.Forms.Timer(components);
            GB_Logs.SuspendLayout();
            SuspendLayout();
            // 
            // TB_Logs
            // 
            TB_Logs.Dock = DockStyle.Fill;
            TB_Logs.Location = new Point(3, 19);
            TB_Logs.Multiline = true;
            TB_Logs.Name = "TB_Logs";
            TB_Logs.ReadOnly = true;
            TB_Logs.Size = new Size(398, 428);
            TB_Logs.TabIndex = 0;
            // 
            // GB_Logs
            // 
            GB_Logs.Controls.Add(TB_Logs);
            GB_Logs.Dock = DockStyle.Right;
            GB_Logs.Location = new Point(396, 0);
            GB_Logs.Name = "GB_Logs";
            GB_Logs.Size = new Size(404, 450);
            GB_Logs.TabIndex = 1;
            GB_Logs.TabStop = false;
            GB_Logs.Text = "Loglar";
            // 
            // B_Start
            // 
            B_Start.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            B_Start.Location = new Point(12, 378);
            B_Start.Name = "B_Start";
            B_Start.Size = new Size(378, 45);
            B_Start.TabIndex = 2;
            B_Start.Text = "PDF Olustur";
            B_Start.UseVisualStyleBackColor = true;
            B_Start.Click += B_Start_Click;
            // 
            // L_ProcessStatus
            // 
            L_ProcessStatus.Font = new Font("Segoe UI", 24F, FontStyle.Regular, GraphicsUnit.Point);
            L_ProcessStatus.Location = new Point(12, 19);
            L_ProcessStatus.Name = "L_ProcessStatus";
            L_ProcessStatus.Size = new Size(378, 356);
            L_ProcessStatus.TabIndex = 3;
            L_ProcessStatus.Text = "PDF Yapmak istediginiz Z-Kitabinizi bir kere acip kapamaniz yeterlidir.";
            L_ProcessStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new Point(12, 426);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(65, 15);
            linkLabel1.TabIndex = 4;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "zemi.space";
            // 
            // T_ProcessTimer
            // 
            T_ProcessTimer.Enabled = true;
            T_ProcessTimer.Interval = 1000;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(linkLabel1);
            Controls.Add(L_ProcessStatus);
            Controls.Add(B_Start);
            Controls.Add(GB_Logs);
            Name = "Form1";
            Text = "Form1";
            GB_Logs.ResumeLayout(false);
            GB_Logs.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox TB_Logs;
        private GroupBox GB_Logs;
        private Button B_Start;
        private Label L_ProcessStatus;
        private LinkLabel linkLabel1;
        private System.Windows.Forms.Timer T_ProcessTimer;
    }
}
