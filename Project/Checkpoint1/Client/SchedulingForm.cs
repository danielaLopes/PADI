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
        private CClient client;
        public SchedulingForm(CClient client)
        {
            InitializeComponent();

            this.client = client;
        }

        private void listButton_Click(object sender, EventArgs e)
        {
            client.List();
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            client.Create(topic.Text, Int32.Parse(minAttendees.Text), slots.Text, attendees.Text);
        }
    }
}
