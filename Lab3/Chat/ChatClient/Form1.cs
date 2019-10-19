using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonLibrary;

namespace ChatClient
{
    delegate void MessageDelegate(string message);

    public partial class Form1 : Form
    {
        private TcpChannel channel;
        private CClientService client;
        private ICServerService s;

        public Form1()
        {
            InitializeComponent();
        }

        public void addMessages(string messages)
        {
            this.messagehistory.Text = messages;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                MessageDelegate messageDelegate = new MessageDelegate(this.addMessages);

                this.channel = new TcpChannel(Int32.Parse(portBox.Text));
                ChannelServices.RegisterChannel(this.channel, false);

                this.client = new CClientService(this, messageDelegate);
                RemotingServices.Marshal(this.client, "BCiWC", typeof(CClientService));

                this.s = (CServerService)Activator.GetObject(typeof(ICServerService), "tcp://localhost:1234/ChatServer");
                this.s.addUser(nickBox.Text, "tcp://localhost:" + portBox.Text + "/BCiWC");
            }
            catch (RemotingException)
            {
                Console.WriteLine("Something went wrong with connection");
            }

        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.s.sendMessage(nickBox.Text, message.Text);
            } 
            catch (RemotingException)
            {
                Console.WriteLine("Something went wrong with send message");
            }
        }
    }
}
