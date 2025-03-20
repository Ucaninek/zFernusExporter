namespace zFernusExporter
{
    partial class SelectionBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            CB_Selection = new ComboBox();
            B_OK = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Dock = DockStyle.Top;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Padding = new Padding(5);
            label1.Size = new Size(481, 91);
            label1.TabIndex = 0;
            label1.Text = "bilgisayarinizda birden fazla kitap tespit edildi. lutfen pdf yapmak istediginiz kitabin\r\n adini asagidan secin.";
            // 
            // CB_Selection
            // 
            CB_Selection.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            CB_Selection.FormattingEnabled = true;
            CB_Selection.Location = new Point(12, 94);
            CB_Selection.Name = "CB_Selection";
            CB_Selection.Size = new Size(377, 29);
            CB_Selection.TabIndex = 1;
            // 
            // B_OK
            // 
            B_OK.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            B_OK.Location = new Point(395, 94);
            B_OK.Name = "B_OK";
            B_OK.Size = new Size(74, 29);
            B_OK.TabIndex = 2;
            B_OK.Text = "Tamam";
            B_OK.UseVisualStyleBackColor = true;
            B_OK.Click += B_OK_Click;
            // 
            // SelectionBox
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(481, 131);
            Controls.Add(B_OK);
            Controls.Add(CB_Selection);
            Controls.Add(label1);
            MaximizeBox = false;
            MaximumSize = new Size(497, 170);
            MinimizeBox = false;
            MinimumSize = new Size(497, 170);
            Name = "SelectionBox";
            Text = "SelectionBox";
            Load += SelectionBox_Load;
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private ComboBox CB_Selection;
        private Button B_OK;
    }
}