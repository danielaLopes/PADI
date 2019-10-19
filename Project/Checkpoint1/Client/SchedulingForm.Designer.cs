namespace Client
{
    partial class SchedulingForm
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
            this.usernameLabel = new System.Windows.Forms.Label();
            this.portLabel = new System.Windows.Forms.Label();
            this.username = new System.Windows.Forms.TextBox();
            this.port = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.listButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Location = new System.Drawing.Point(35, 38);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(77, 17);
            this.usernameLabel.TabIndex = 0;
            this.usernameLabel.Text = "Username:";
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(35, 71);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(38, 17);
            this.portLabel.TabIndex = 1;
            this.portLabel.Text = "Port:";
            // 
            // username
            // 
            this.username.Location = new System.Drawing.Point(133, 33);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(185, 22);
            this.username.TabIndex = 2;
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(133, 68);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(185, 22);
            this.port.TabIndex = 3;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(224, 108);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(94, 43);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // listButton
            // 
            this.listButton.Location = new System.Drawing.Point(570, 38);
            this.listButton.Name = "listButton";
            this.listButton.Size = new System.Drawing.Size(141, 48);
            this.listButton.TabIndex = 5;
            this.listButton.Text = "List";
            this.listButton.UseVisualStyleBackColor = true;
            this.listButton.Click += new System.EventHandler(this.listButton_Click);
            // 
            // SchedulingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.listButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.port);
            this.Controls.Add(this.username);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.usernameLabel);
            this.Name = "SchedulingForm";
            this.Text = "MSDAD - Meeting Scheduler";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox username;
        private System.Windows.Forms.TextBox port;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button listButton;
    }
}