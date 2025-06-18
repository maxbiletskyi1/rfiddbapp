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
            btnHistorique = new Button();
            SuspendLayout();
            // 
            // checkBtn
            // 
            checkBtn.Location = new Point(0, 0);
            checkBtn.Name = "checkBtn";
            checkBtn.Size = new Size(75, 23);
            checkBtn.TabIndex = 7;
            // 
            // label1
            // 
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(101, 23);
            label1.TabIndex = 6;
            // 
            // messageLbl
            // 
            messageLbl.AutoSize = true;
            messageLbl.Font = new Font("Segoe UI", 24F);
            messageLbl.Location = new Point(328, 173);
            messageLbl.MaximumSize = new Size(1000, 0);
            messageLbl.Name = "messageLbl";
            messageLbl.Size = new Size(0, 54);
            messageLbl.TabIndex = 5;
            // 
            // btnHistorique
            // 
            btnHistorique.Location = new Point(12, 12);
            btnHistorique.Name = "btnHistorique";
            btnHistorique.Size = new Size(118, 41);
            btnHistorique.TabIndex = 8;
            btnHistorique.Text = "Historique";
            btnHistorique.UseVisualStyleBackColor = true;
            btnHistorique.Click += btnHistorique_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1481, 469);
            Controls.Add(btnHistorique);
            Controls.Add(messageLbl);
            Controls.Add(label1);
            Controls.Add(checkBtn);
            MaximumSize = new Size(1500, 516);
            MinimumSize = new Size(1000, 516);
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
        private Button btnHistorique;
    }
}
