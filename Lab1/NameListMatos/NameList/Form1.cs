using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NameList; 

namespace NameList
{
    public partial class Form1 : Form
    {
        private NameList nameList;

        public Form1()
        {
            this.nameList = new NameList();
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var newName = textToAdd.Text;
            Console.WriteLine("add button clicked: {0}", newName);
            this.nameList.addName(newName);
            this.textToAdd.Text = ""; 
            this.textToDisplay.Text = this.nameList.getNames();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("clear button clicked");
            this.nameList.clearNames();
            this.textToDisplay.Text = this.nameList.getNames();
        }
    }
}
