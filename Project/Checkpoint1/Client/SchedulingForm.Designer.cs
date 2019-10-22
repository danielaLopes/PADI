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
            this.listButton = new System.Windows.Forms.Button();
            this.createButton = new System.Windows.Forms.Button();
            this.list = new System.Windows.Forms.TextBox();
            this.topic = new System.Windows.Forms.TextBox();
            this.minAttendees = new System.Windows.Forms.TextBox();
            this.slots = new System.Windows.Forms.TextBox();
            this.attendees = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // usernameLabel
            // 
            this.usernameLabel.AutoSize = true;
            this.usernameLabel.Location = new System.Drawing.Point(35, 38);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(0, 17);
            this.usernameLabel.TabIndex = 0;
            // 
            // listButton
            // 
            this.listButton.Location = new System.Drawing.Point(163, 375);
            this.listButton.Name = "listButton";
            this.listButton.Size = new System.Drawing.Size(141, 48);
            this.listButton.TabIndex = 5;
            this.listButton.Text = "List";
            this.listButton.UseVisualStyleBackColor = true;
            this.listButton.Click += new System.EventHandler(this.listButton_Click);
            // 
            // createButton
            // 
            this.createButton.Location = new System.Drawing.Point(402, 375);
            this.createButton.Name = "createButton";
            this.createButton.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.createButton.Size = new System.Drawing.Size(141, 47);
            this.createButton.TabIndex = 6;
            this.createButton.Text = "Create";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.createButton_Click);
            // 
            // list
            // 
            this.list.Location = new System.Drawing.Point(28, 160);
            this.list.Multiline = true;
            this.list.Name = "list";
            this.list.Size = new System.Drawing.Size(276, 199);
            this.list.TabIndex = 7;
            // 
            // topic
            // 
            this.topic.Location = new System.Drawing.Point(341, 160);
            this.topic.Name = "topic";
            this.topic.Size = new System.Drawing.Size(202, 22);
            this.topic.TabIndex = 8;
            this.topic.Text = "Topic";
            this.topic.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // minAttendees
            // 
            this.minAttendees.Location = new System.Drawing.Point(341, 199);
            this.minAttendees.Name = "minAttendees";
            this.minAttendees.Size = new System.Drawing.Size(202, 22);
            this.minAttendees.TabIndex = 9;
            this.minAttendees.Text = "Min Attendees";
            this.minAttendees.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // slots
            // 
            this.slots.Location = new System.Drawing.Point(341, 236);
            this.slots.Multiline = true;
            this.slots.Name = "slots";
            this.slots.Size = new System.Drawing.Size(202, 56);
            this.slots.TabIndex = 10;
            this.slots.Text = "Slots";
            this.slots.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // attendees
            // 
            this.attendees.Location = new System.Drawing.Point(341, 310);
            this.attendees.Multiline = true;
            this.attendees.Name = "attendees";
            this.attendees.Size = new System.Drawing.Size(202, 49);
            this.attendees.TabIndex = 11;
            this.attendees.Text = "Attendees";
            this.attendees.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // SchedulingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.attendees);
            this.Controls.Add(this.slots);
            this.Controls.Add(this.minAttendees);
            this.Controls.Add(this.topic);
            this.Controls.Add(this.list);
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.listButton);
            this.Controls.Add(this.usernameLabel);
            this.Name = "SchedulingForm";
            this.Text = "MSDAD - Meeting Scheduler";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.Button listButton;
        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.TextBox list;
        private System.Windows.Forms.TextBox topic;
        private System.Windows.Forms.TextBox minAttendees;
        private System.Windows.Forms.TextBox slots;
        private System.Windows.Forms.TextBox attendees;
    }
}