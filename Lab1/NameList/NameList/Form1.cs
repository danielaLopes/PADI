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
    public partial class Form1 : Form
    {
        private List<String> _names;

        public Form1()
        {
            InitializeComponent();
            _names = new List<String>();
        }

        private void Add_Name_Click(object sender, EventArgs e)
        {
           NameForm nameForm = new NameForm(_names);
           nameForm.ShowDialog(); // Shows the form
        }

        private void List_Names_Click(object sender, EventArgs e)
        {
            ListForm listForm = new ListForm(_names);
            listForm.ShowDialog(); // Shows the form
        }

        private void Clear_Names_Click(object sender, EventArgs e)
        {
            _names.Clear();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
