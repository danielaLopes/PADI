using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class SchedulingForm : Form
    {
        CClient client;
        public SchedulingForm()
        {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine(Int32.Parse(port.Text));
            client = new CClient(username.Text, Int32.Parse(port.Text));

            // disable users from registering more than once on the same client app
            username.ReadOnly = true;
            port.ReadOnly = true;
            connectButton.Enabled = false;
        }

        private void listButton_Click(object sender, EventArgs e)
        {
            client.List();
        }
    }
}
