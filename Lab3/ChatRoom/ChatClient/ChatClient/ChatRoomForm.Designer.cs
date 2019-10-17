namespace ChatClient
{
    partial class ChatRoomForm
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
            this.portText = new System.Windows.Forms.TextBox();
            this.portLabel = new System.Windows.Forms.Label();
            this.messageLabel = new System.Windows.Forms.Label();
            this.conversationLabel = new System.Windows.Forms.Label();
            this.messageText = new System.Windows.Forms.TextBox();
            this.conversationText = new System.Windows.Forms.TextBox();
            this.nicknameLabel = new System.Windows.Forms.Label();
            this.nicknameText = new System.Windows.Forms.TextBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.sendButton = new System.Windows.Forms.Button();
            this.newClientLabel = new System.Windows.Forms.Label();
            this.chatRoomLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // portText
            // 
            this.portText.Location = new System.Drawing.Point(154, 50);
            this.portText.Name = "portText";
            this.portText.Size = new System.Drawing.Size(102, 22);
            this.portText.TabIndex = 0;
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(47, 53);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(37, 17);
            this.portLabel.TabIndex = 1;
            this.portLabel.Text = "port:";
            // 
            // messageLabel
            // 
            this.messageLabel.Location = new System.Drawing.Point(47, 190);
            this.messageLabel.Name = "messageLabel";
            this.messageLabel.Size = new System.Drawing.Size(69, 17);
            this.messageLabel.TabIndex = 2;
            this.messageLabel.Text = "message:";
            // 
            // conversationLabel
            // 
            this.conversationLabel.AutoSize = true;
            this.conversationLabel.Location = new System.Drawing.Point(47, 252);
            this.conversationLabel.Name = "conversationLabel";
            this.conversationLabel.Size = new System.Drawing.Size(93, 17);
            this.conversationLabel.TabIndex = 3;
            this.conversationLabel.Text = "conversation:";
            // 
            // messageText
            // 
            this.messageText.Location = new System.Drawing.Point(154, 190);
            this.messageText.Name = "messageText";
            this.messageText.Size = new System.Drawing.Size(370, 22);
            this.messageText.TabIndex = 4;
            // 
            // conversationText
            // 
            this.conversationText.Location = new System.Drawing.Point(154, 252);
            this.conversationText.Name = "conversationText";
            this.conversationText.Size = new System.Drawing.Size(370, 22);
            this.conversationText.TabIndex = 5;
            // 
            // nicknameLabel
            // 
            this.nicknameLabel.AutoSize = true;
            this.nicknameLabel.Location = new System.Drawing.Point(47, 86);
            this.nicknameLabel.Name = "nicknameLabel";
            this.nicknameLabel.Size = new System.Drawing.Size(72, 17);
            this.nicknameLabel.TabIndex = 6;
            this.nicknameLabel.Text = "nickname:";
            // 
            // nicknameText
            // 
            this.nicknameText.Location = new System.Drawing.Point(154, 83);
            this.nicknameText.Name = "nicknameText";
            this.nicknameText.Size = new System.Drawing.Size(177, 22);
            this.nicknameText.TabIndex = 7;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(583, 53);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(101, 43);
            this.connectButton.TabIndex = 8;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(583, 190);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(101, 46);
            this.sendButton.TabIndex = 9;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // newClientLabel
            // 
            this.newClientLabel.AutoSize = true;
            this.newClientLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.newClientLabel.Location = new System.Drawing.Point(45, 9);
            this.newClientLabel.Name = "newClientLabel";
            this.newClientLabel.Size = new System.Drawing.Size(106, 25);
            this.newClientLabel.TabIndex = 10;
            this.newClientLabel.Text = "New Client";
            // 
            // chatRoomLabel
            // 
            this.chatRoomLabel.AutoSize = true;
            this.chatRoomLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.chatRoomLabel.Location = new System.Drawing.Point(45, 151);
            this.chatRoomLabel.Name = "chatRoomLabel";
            this.chatRoomLabel.Size = new System.Drawing.Size(110, 25);
            this.chatRoomLabel.TabIndex = 11;
            this.chatRoomLabel.Text = "Chat Room";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 124);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(743, 17);
            this.label1.TabIndex = 12;
            this.label1.Text = "---------------------------------------------------------------------------------" +
    "------------------------------------------------------------------";
            // 
            // ChatRoomForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(749, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chatRoomLabel);
            this.Controls.Add(this.newClientLabel);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.nicknameText);
            this.Controls.Add(this.nicknameLabel);
            this.Controls.Add(this.conversationText);
            this.Controls.Add(this.messageText);
            this.Controls.Add(this.conversationLabel);
            this.Controls.Add(this.messageLabel);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.portText);
            this.Name = "ChatRoomForm";
            this.Text = "Chat Room Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox portText;
        private System.Windows.Forms.Label messageLabel;
        private System.Windows.Forms.TextBox messageText;
        private System.Windows.Forms.Label conversationLabel;
        private System.Windows.Forms.TextBox conversationText;
        private System.Windows.Forms.Label nicknameLabel;
        private System.Windows.Forms.TextBox nicknameText;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Label newClientLabel;
        private System.Windows.Forms.Label chatRoomLabel;
        private System.Windows.Forms.Label label1;
    }
}

