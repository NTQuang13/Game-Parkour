namespace UnityServer_selfmake_
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            richTextBox_notification = new RichTextBox();
            textBox_PORT = new TextBox();
            label_PORT = new Label();
            button_LISTEN = new Button();
            textBox_UDPreceiver = new TextBox();
            SuspendLayout();
            // 
            // richTextBox_notification
            // 
            richTextBox_notification.Location = new Point(120, 81);
            richTextBox_notification.Name = "richTextBox_notification";
            richTextBox_notification.Size = new Size(503, 163);
            richTextBox_notification.TabIndex = 0;
            richTextBox_notification.Text = "";
            // 
            // textBox_PORT
            // 
            textBox_PORT.Location = new Point(270, 34);
            textBox_PORT.Name = "textBox_PORT";
            textBox_PORT.Size = new Size(100, 23);
            textBox_PORT.TabIndex = 1;
            textBox_PORT.Text = "10000";
            // 
            // label_PORT
            // 
            label_PORT.AutoSize = true;
            label_PORT.Location = new Point(209, 37);
            label_PORT.Name = "label_PORT";
            label_PORT.Size = new Size(35, 15);
            label_PORT.TabIndex = 2;
            label_PORT.Text = "PORT";
            // 
            // button_LISTEN
            // 
            button_LISTEN.Location = new Point(414, 34);
            button_LISTEN.Name = "button_LISTEN";
            button_LISTEN.Size = new Size(75, 23);
            button_LISTEN.TabIndex = 3;
            button_LISTEN.Text = "LISTEN";
            button_LISTEN.UseVisualStyleBackColor = true;
            button_LISTEN.Click += button_LISTEN_Click;
            // 
            // textBox_UDPreceiver
            // 
            textBox_UDPreceiver.Location = new Point(120, 279);
            textBox_UDPreceiver.Name = "textBox_UDPreceiver";
            textBox_UDPreceiver.Size = new Size(503, 23);
            textBox_UDPreceiver.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBox_UDPreceiver);
            Controls.Add(button_LISTEN);
            Controls.Add(label_PORT);
            Controls.Add(textBox_PORT);
            Controls.Add(richTextBox_notification);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox richTextBox_notification;
        private TextBox textBox_PORT;
        private Label label_PORT;
        private Button button_LISTEN;
        private TextBox textBox_UDPreceiver;
    }
}
