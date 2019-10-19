namespace ChatClient
{
    partial class Form1
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
            this.messagehistory = new System.Windows.Forms.TextBox();
            this.message = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.portBox = new System.Windows.Forms.TextBox();
            this.nickBox = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // messagehistory
            // 
            this.messagehistory.Location = new System.Drawing.Point(126, 169);
            this.messagehistory.Multiline = true;
            this.messagehistory.Name = "messagehistory";
            this.messagehistory.Size = new System.Drawing.Size(928, 456);
            this.messagehistory.TabIndex = 0;
            this.messagehistory.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // message
            // 
            this.message.Location = new System.Drawing.Point(126, 715);
            this.message.Multiline = true;
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(690, 70);
            this.message.TabIndex = 1;
            this.message.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(881, 715);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(173, 70);
            this.sendButton.TabIndex = 2;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // portBox
            // 
            this.portBox.Location = new System.Drawing.Point(126, 75);
            this.portBox.Name = "portBox";
            this.portBox.Size = new System.Drawing.Size(288, 26);
            this.portBox.TabIndex = 3;
            this.portBox.Text = "Port";
            this.portBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // nickBox
            // 
            this.nickBox.Location = new System.Drawing.Point(528, 75);
            this.nickBox.Name = "nickBox";
            this.nickBox.Size = new System.Drawing.Size(288, 26);
            this.nickBox.TabIndex = 4;
            this.nickBox.Text = "Nickname";
            this.nickBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(881, 53);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(173, 70);
            this.connectButton.TabIndex = 5;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 908);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.nickBox);
            this.Controls.Add(this.portBox);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.message);
            this.Controls.Add(this.messagehistory);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox messagehistory;
        private System.Windows.Forms.TextBox message;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.TextBox portBox;
        private System.Windows.Forms.TextBox nickBox;
        private System.Windows.Forms.Button connectButton;
    }
}

