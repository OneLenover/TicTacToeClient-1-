namespace TicTacToeClient_1_
{
    partial class MainForm
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
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            button8 = new Button();
            button9 = new Button();
            label1 = new Label();
            playerTurn = new Label();
            label3 = new Label();
            label4 = new Label();
            player1score = new Label();
            player2score = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(90, 90);
            button1.TabIndex = 0;
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(108, 12);
            button2.Name = "button2";
            button2.Size = new Size(90, 90);
            button2.TabIndex = 1;
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(204, 12);
            button3.Name = "button3";
            button3.Size = new Size(90, 90);
            button3.TabIndex = 2;
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(204, 108);
            button4.Name = "button4";
            button4.Size = new Size(90, 90);
            button4.TabIndex = 5;
            button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.Location = new Point(108, 108);
            button5.Name = "button5";
            button5.Size = new Size(90, 90);
            button5.TabIndex = 4;
            button5.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            button6.Location = new Point(12, 108);
            button6.Name = "button6";
            button6.Size = new Size(90, 90);
            button6.TabIndex = 3;
            button6.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            button7.Location = new Point(204, 204);
            button7.Name = "button7";
            button7.Size = new Size(90, 90);
            button7.TabIndex = 8;
            button7.UseVisualStyleBackColor = true;
            // 
            // button8
            // 
            button8.Location = new Point(108, 204);
            button8.Name = "button8";
            button8.Size = new Size(90, 90);
            button8.TabIndex = 7;
            button8.UseVisualStyleBackColor = true;
            // 
            // button9
            // 
            button9.Location = new Point(12, 204);
            button9.Name = "button9";
            button9.Size = new Size(90, 90);
            button9.TabIndex = 6;
            button9.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.BackColor = Color.SkyBlue;
            label1.Font = new Font("Segoe UI", 16F);
            label1.Location = new Point(300, 12);
            label1.Name = "label1";
            label1.Size = new Size(150, 30);
            label1.TabIndex = 9;
            label1.Text = "Ход";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.Click += label1_Click;
            // 
            // playerTurn
            // 
            playerTurn.BackColor = Color.White;
            playerTurn.Location = new Point(300, 42);
            playerTurn.Name = "playerTurn";
            playerTurn.Size = new Size(150, 60);
            playerTurn.TabIndex = 10;
            playerTurn.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.BackColor = Color.LightGreen;
            label3.Location = new Point(300, 108);
            label3.Name = "label3";
            label3.Size = new Size(75, 30);
            label3.TabIndex = 11;
            label3.Text = "Игрок 1";
            label3.TextAlign = ContentAlignment.TopCenter;
            // 
            // label4
            // 
            label4.BackColor = Color.LightGreen;
            label4.Location = new Point(375, 108);
            label4.Name = "label4";
            label4.Size = new Size(75, 30);
            label4.TabIndex = 12;
            label4.Text = "Игрок 2";
            label4.TextAlign = ContentAlignment.TopCenter;
            // 
            // player1score
            // 
            player1score.BackColor = Color.White;
            player1score.Location = new Point(300, 138);
            player1score.Name = "player1score";
            player1score.Size = new Size(75, 60);
            player1score.TabIndex = 13;
            player1score.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // player2score
            // 
            player2score.BackColor = Color.White;
            player2score.Location = new Point(375, 138);
            player2score.Name = "player2score";
            player2score.Size = new Size(75, 60);
            player2score.TabIndex = 14;
            player2score.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.AntiqueWhite;
            ClientSize = new Size(484, 311);
            Controls.Add(player2score);
            Controls.Add(player1score);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(playerTurn);
            Controls.Add(label1);
            Controls.Add(button7);
            Controls.Add(button8);
            Controls.Add(button9);
            Controls.Add(button4);
            Controls.Add(button5);
            Controls.Add(button6);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "MainForm";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;
        private Button button9;
        private Label label1;
        private Label playerTurn;
        private Label label3;
        private Label label4;
        private Label player1score;
        private Label player2score;
    }
}
