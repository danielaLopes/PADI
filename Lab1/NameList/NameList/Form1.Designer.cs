namespace NameList
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
            this.Add_Name = new System.Windows.Forms.Button();
            this.List_Names = new System.Windows.Forms.Button();
            this.Clear_Names = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Add_Name
            // 
            this.Add_Name.Location = new System.Drawing.Point(80, 93);
            this.Add_Name.Name = "Add_Name";
            this.Add_Name.Size = new System.Drawing.Size(151, 62);
            this.Add_Name.TabIndex = 0;
            this.Add_Name.Text = "Add name";
            this.Add_Name.UseVisualStyleBackColor = true;
            this.Add_Name.Click += new System.EventHandler(this.Add_Name_Click);
            // 
            // List_Names
            // 
            this.List_Names.Location = new System.Drawing.Point(80, 177);
            this.List_Names.Name = "List_Names";
            this.List_Names.Size = new System.Drawing.Size(151, 61);
            this.List_Names.TabIndex = 1;
            this.List_Names.Text = "List names";
            this.List_Names.UseVisualStyleBackColor = true;
            this.List_Names.Click += new System.EventHandler(this.List_Names_Click);
            // 
            // Clear_Names
            // 
            this.Clear_Names.Location = new System.Drawing.Point(80, 259);
            this.Clear_Names.Name = "Clear_Names";
            this.Clear_Names.Size = new System.Drawing.Size(151, 52);
            this.Clear_Names.TabIndex = 2;
            this.Clear_Names.Text = "Clear Names";
            this.Clear_Names.UseVisualStyleBackColor = true;
            this.Clear_Names.Click += new System.EventHandler(this.Clear_Names_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(76, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(163, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Manage Name\'s List";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(316, 368);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Clear_Names);
            this.Controls.Add(this.List_Names);
            this.Controls.Add(this.Add_Name);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Add_Name;
        private System.Windows.Forms.Button List_Names;
        private System.Windows.Forms.Button Clear_Names;
        private System.Windows.Forms.Label label1;
    }
}

