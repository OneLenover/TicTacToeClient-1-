using Grpc.Core;
using Grpc.Net.Client;
using TicTacToe.App.Protos;

namespace TicTacToeClient_1_
{
    public partial class MainForm : Form
    {
        private readonly Button[] boardButtons;
        private readonly GameService.GameServiceClient _client;
        private readonly string _gameId = "room_1";
        private readonly string _myPlayerId;
        private readonly CancellationTokenSource _cts = new();

        public MainForm()
        {
            InitializeComponent();
            boardButtons = new Button[] { button1, button2, button3, button4, button5, button6, button7, button8, button9 };
            _myPlayerId = "User_" + Guid.NewGuid().ToString().Substring(0, 4);

            var channel = GrpcChannel.ForAddress("http://localhost:50051");
            _client = new GameService.GameServiceClient(channel);

            foreach (var btn in boardButtons) btn.Click += OnCellClick;

            /*
            if (Controls.ContainsKey("newRoundButton"))
            {
                newRoundButton.Click += async (s, e) => await TryResetRoundAsync();
            }
            */

            this.Load += async (s, e) => await ConnectToGameAsync();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Останавливаем прослушивание стримов
            _cts.Cancel();
            base.OnFormClosing(e);
        }

        private async Task ConnectToGameAsync()
        {
            try
            {
                var req = new CreateRequest { PlayerId = _myPlayerId, GameId = _gameId };
                var response = await _client.CreateGameAsync(req);
                UpdateUI(response);


                // Запуск фонового прослушивания (не ожидаем завершения)
                _ = StartListeningAsync();
            }
            catch (RpcException rex)
            {
                MessageBox.Show($"gRPC error: {rex.Status.Detail}", "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task StartListeningAsync()
        {
            // Подписываемся на стрим событий игры
            using var call = _client.SubscribeGameEvents(new StateRequest { GameId = _gameId }, cancellationToken: _cts.Token);


            try
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync(_cts.Token))
                {
                    if (this.IsDisposed) return;

                    // Обновляем UI в UI-потоке
                    this.Invoke(new MethodInvoker(() => UpdateUI(response)));
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex)
            {
                if (!_cts.IsCancellationRequested)
                    Console.WriteLine("Stream error: " + ex.Message);
            }
        }

        private async void OnCellClick(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            int index = Array.IndexOf(boardButtons, btn);
            if (index < 0 || index >= 9) return;


            int x = index / 3;
            int y = index % 3;

            try
            {
                var move = new MoveRequest
                {
                    GameId = _gameId,
                    PlayerId = _myPlayerId,
                    X = x,
                    Y = y
                };


                var resp = await _client.MakeMoveAsync(move);
                // Ответ придёт и в стрим, но можно сразу обновить UI из ответа метода
                UpdateUI(resp);
            }
            catch (RpcException ex)
            {
                // Показываем деталь ошибки, если сервер вернул её
                MessageBox.Show(ex.Status.Detail, "Ход невозможен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateUI(GameResponse state)
        {
            if (state == null) return;


            // 1) Обновляем клетки и цвета
            var board = state.Board ?? ".........";
            if (board.Length != 9) board = board.PadRight(9, '.').Substring(0, 9);


            for (int i = 0; i < 9; i++)
            {
                char c = board[i];
                boardButtons[i].Text = (c == '.') ? string.Empty : c.ToString();
                boardButtons[i].BackColor = SystemColors.Control;
            }


            // 2) Подсветка победной линии
            if (state.WinningLine != null && state.WinningLine.Count > 0)
            {
                foreach (var idx in state.WinningLine)
                {
                    if (idx >= 0 && idx < 9)
                        boardButtons[idx].BackColor = Color.LightGreen;
                }
            }


            // 3) Доступность кнопок (только если мой ход и клетка пустая)
            bool isMyTurn = state.Status == "Playing" && state.CurrentPlayerId == _myPlayerId;
            for (int i = 0; i < 9; i++)
            {
                boardButtons[i].Enabled = isMyTurn && string.IsNullOrEmpty(boardButtons[i].Text);
            }


            // 4) Обновляем счёт и статусы из состояния сервера
            try
            {
                player1score.Text = state.PlayerXScore.ToString();
                player2score.Text = state.PlayerOScore.ToString();
            }
            catch { /* Если лейблы не найдены — пропускаем */ }


            if (state.Status == "Waiting")
            {
                playerTurn.Text = "Ожидание второго игрока...";
            }
            else if (state.Status == "Playing")
            {
                playerTurn.Text = isMyTurn ? "ВАШ ХОД!" : "Ход противника...";
            }
            else if (state.Status == "Won" || state.Status == "Draw")
            {
                ProcessGameOver(state);
            }
        }

        private void ProcessGameOver(GameResponse state)
        {
            // Отключаем кнопки поля
            foreach (var b in boardButtons) b.Enabled = false;


            if (state.Status == "Won")
            {
                if (state.WinnerId == _myPlayerId)
                {
                    MessageBox.Show("ВЫ ПОБЕДИЛИ!", "Победа", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("ВЫ ПРОИГРАЛИ!", "Поражение", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (state.Status == "Draw")
            {
                MessageBox.Show("НИЧЬЯ!", "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            // Если в форме есть кнопка нового раунда — активируем её
            if (Controls.ContainsKey("newRoundButton"))
            {
                //newRoundButton.Enabled = true;
            }
            else
            {
                // Автоматически сбрасываем раунд через 1.2 секунды — удобно для быстрых игр; можно отключить
                _ = Task.Run(async () =>
                {
                    try { await Task.Delay(1200, _cts.Token); }
                    catch { return; }


                    if (_cts.IsCancellationRequested) return;
                    try
                    {
                        var resp = await _client.ResetRoundAsync(new StateRequest { GameId = _gameId });
                        this.Invoke(new MethodInvoker(() => UpdateUI(resp)));
                    }
                    catch { /* игнорируем ошибки сброса */ }
                });
            }
        }

        private async Task TryResetRoundAsync()
        {
            try
            {
                //newRoundButton.Enabled = false;
                var resp = await _client.ResetRoundAsync(new StateRequest { GameId = _gameId });
                UpdateUI(resp);
            }
            catch (RpcException rex)
            {
                MessageBox.Show(rex.Status.Detail, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e) { }
    }
}
