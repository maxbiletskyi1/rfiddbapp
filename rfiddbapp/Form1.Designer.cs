namespace rfiddbapp
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
            checkBtn = new Button();
            label1 = new Label();
            messageLbl = new Label();
            SuspendLayout();
            // 
            // messageLbl
            // 
            messageLbl.AutoSize = true;
            messageLbl.Font = new Font("Segoe UI", 24F);
            messageLbl.Location = new Point(328, 174);
            messageLbl.Name = "messageLbl";
            messageLbl.Size = new Size(0, 54);
            messageLbl.TabIndex = 5;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1307, 472);
            Controls.Add(messageLbl);
            Controls.Add(label1);
            Controls.Add(checkBtn);
            Name = "Form1";
            Text = "Vigichantier";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button checkBtn;
        private Label label1;
        private Label messageLbl;
    }
}
