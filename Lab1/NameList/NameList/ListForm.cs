using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NameList
{
    public partial class ListForm : Form
    {

        public ListForm(List<String> names)
        {
            InitializeComponent();

            foreach(String name in names)
            {
                textBox1.Text += name + " ";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
