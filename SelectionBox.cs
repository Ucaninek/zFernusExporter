using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace zFernusExporter
{
    public partial class SelectionBox : Form
    {
        public string SelectedItem { get; private set; }
        public List<string> items { get; set; }

        public SelectionBox()
        {
            InitializeComponent();
        }

        private void B_OK_Click(object sender, EventArgs e)
        {
            string? output = CB_Selection.SelectedItem.ToString();
            if (CB_Selection.SelectedIndex == -1 || output == null)
            {
                MessageBox.Show("No item selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SelectedItem = output;
            this.Close();
        }

        private void SelectionBox_Load(object sender, EventArgs e)
        {
            if(items == null)
            {
                MessageBox.Show("No items to display", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            foreach (string item in items)
            {
                CB_Selection.Items.Add(Path.GetFileName(item));
            }
        }
    }
}
