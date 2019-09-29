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
    public partial class NameForm : Form
    {
        private List<String> _names;

        public NameForm(List<String> names)
        {
            InitializeComponent();
            _names = names;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void NameForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            _names.Add(textBox1.Text);
            this.Close();
        }
    }
}
