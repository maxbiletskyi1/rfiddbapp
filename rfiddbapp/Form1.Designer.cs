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
            nameLbl = new Label();
            idLbl = new Label();
            nameInput = new TextBox();
            idInput = new TextBox();
            label1 = new Label();
            messageLbl = new Label();
            SuspendLayout();
            // 
            // checkBtn
            // 
            checkBtn.AutoSize = true;
            checkBtn.Location = new Point(789, 563);
            checkBtn.Name = "checkBtn";
            checkBtn.Size = new Size(94, 30);
            checkBtn.TabIndex = 0;
            checkBtn.Text = "Verifier";
            checkBtn.UseVisualStyleBackColor = true;
            checkBtn.Click += checkBtn_Click;
            // 
            // nameLbl
            // 
            nameLbl.AutoSize = true;
            nameLbl.Location = new Point(236, 162);
            nameLbl.Name = "nameLbl";
            nameLbl.Size = new Size(49, 20);
            nameLbl.TabIndex = 1;
            nameLbl.Text = "Name";
            // 
            // idLbl
            // 
            idLbl.AutoSize = true;
            idLbl.Location = new Point(236, 229);
            idLbl.Name = "idLbl";
            idLbl.Size = new Size(24, 20);
            idLbl.TabIndex = 2;
            idLbl.Text = "ID";
            // 
            // nameInput
            // 
            nameInput.Location = new Point(340, 155);
            nameInput.Name = "nameInput";
            nameInput.Size = new Size(125, 27);
            nameInput.TabIndex = 3;
            // 
            // idInput
            // 
            idInput.Location = new Point(340, 226);
            idInput.Name = "idInput";
            idInput.Size = new Size(125, 27);
            idInput.TabIndex = 4;
            // 
            // label1
            // 
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(100, 23);
            label1.TabIndex = 0;
            // 
            // messageLbl
            // 
            messageLbl.AutoSize = true;
            messageLbl.Location = new Point(552, 183);
            messageLbl.Name = "messageLbl";
            messageLbl.Size = new Size(0, 20);
            messageLbl.TabIndex = 5;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(922, 632);
            Controls.Add(messageLbl);
            Controls.Add(label1);
            Controls.Add(idInput);
            Controls.Add(nameInput);
            Controls.Add(idLbl);
            Controls.Add(nameLbl);
            Controls.Add(checkBtn);
            Name = "Form1";
            Text = "Vigichantier";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button checkBtn;
        private Label nameLbl;
        private Label idLbl;
        private TextBox nameInput;
        private TextBox idInput;
        private Label label1;
        private Label messageLbl;
    }
}
