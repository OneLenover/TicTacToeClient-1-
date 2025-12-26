namespace TicTacToeClient_1_
{
    public partial class MainForm : Form
    {
        bool isPlayerXTurn = true;
        Button[] boardButtons;
        int player1Score = 0;
        int player2Score = 0;
        public MainForm()
        {
            InitializeComponent();

            boardButtons = new Button[] { button1, button2, button3, button4, button5, button6, button7, button8, button9 };
            playerTurn.Text = "Игрок 1";
            foreach (var btn in boardButtons)
            {
                btn.Click += OnCellClick;
            }
        }

        private void OnCellClick(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;

            if (!string.IsNullOrEmpty(clickedButton.Text)) return;

            MakeMove(clickedButton);
        }

        private void MakeMove(Button button)
        {
            button.Text = isPlayerXTurn ? "X" : "O";

            if (CheckWin())
            {
                UpdateScore();
                MessageBox.Show($"Игрок {(isPlayerXTurn ? "1 (X)" : "2 (O)")} победил!");
                ResetBoard();
            }
            else if (boardButtons.All(b => b.Text != ""))
            {
                MessageBox.Show("Ничья!");
                ResetBoard();
            }
            else
            {
                isPlayerXTurn = !isPlayerXTurn;
                playerTurn.Text = isPlayerXTurn ? "Игрок 1" : "Игрок 2";
            }
        }

        private bool CheckWin()
        {
            int[][] winners = new int[][]
            {
                new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 },    // Горизонтали
                new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 },    // Вертикали
                new[] { 0, 4, 8 }, new[] { 2, 4, 6 }    // Диагонали
            };

            foreach (var w in winners)
            {
                if (boardButtons[w[0]].Text != "" &&
                    boardButtons[w[0]].Text == boardButtons[w[1]].Text &&
                    boardButtons[w[0]].Text == boardButtons[w[2]].Text)
                {
                    HighlightWinner(w);
                    return true;
                }
            }
            return false;
        }

        private void HighlightWinner(int[] indices)
        {
            foreach (int i in indices)
            {
                boardButtons[i].BackColor = Color.LightGreen;
            }
        }

        private void ResetBoard()
        {
            foreach (var btn in boardButtons)
            {
                btn.Text = "";
                btn.BackColor = Color.FromKnownColor(KnownColor.Control);
                btn.UseVisualStyleBackColor = true;
            }
            isPlayerXTurn = true;
        }

        private void UpdateScore()
        {
            if (isPlayerXTurn) player1Score++; else player2Score++;
            player1score.Text = player1Score.ToString();
            player2score.Text = player2Score.ToString();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
