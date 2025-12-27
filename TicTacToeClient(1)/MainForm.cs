using Consul;
using Grpc.Core;
using Grpc.Net.Client;
using System.Text;
using TicTacToe.App.Protos;

namespace TicTacToeClient_1_
{
    public partial class MainForm : Form
    {
        private Button[] boardButtons;
        private GameService.GameServiceClient? _client;
        private GrpcChannel? _channel;
        private readonly string _gameId = "room_1";
        private readonly string _myPlayerId;
        private CancellationTokenSource _cts = new();

        public MainForm()
        {
            InitializeComponent();
            boardButtons = new Button[] { button1, button2, button3, button4, button5, button6, button7, button8, button9 };
            _myPlayerId = "User_" + Guid.NewGuid().ToString().Substring(0, 4);

            foreach (var btn in boardButtons) btn.Click += OnCellClick;
            if (Controls.ContainsKey("newRoundButton"))
                newRoundButton.Click += async (s, e) => await TryResetRoundAsync();

            this.Load += (s, e) => { _ = ConnectionLoopAsync(); };
        }

        // Основной цикл: если связь упала, ищем нового лидера и переподключаемся
        private async Task ConnectionLoopAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    this.Invoke(() => HostName.Text = "Поиск лидера...");
                    string leaderUrl = await GetLeaderUrlFromConsul();

                    this.Invoke(() => HostName.Text = $"Сервер: {leaderUrl}");

                    _channel = GrpcChannel.ForAddress(leaderUrl);
                    _client = new GameService.GameServiceClient(_channel);

                    // Регистрируемся/проверяем сессию
                    var response = await _client.CreateGameAsync(new CreateRequest { PlayerId = _myPlayerId, GameId = _gameId });
                    this.Invoke(() => UpdateUI(response));

                    // Слушаем события. Если метод завершится (ошибка сервера), цикл начнется заново
                    await StartListeningAsync();
                }
                catch (Exception ex)
                {
                    this.Invoke(() => HostName.Text = "Связь потеряна. Ожидание...");
                    await Task.Delay(2000); // Пауза перед поиском нового лидера
                }
            }
        }

        private async Task StartListeningAsync()
        {
            if (_client == null) return;
            using var call = _client.SubscribeGameEvents(new StateRequest { GameId = _gameId }, cancellationToken: _cts.Token);

            try
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync(_cts.Token))
                {
                    this.Invoke(() => UpdateUI(response));
                }
            }
            catch (RpcException) { /* Сервер упал, выходим из метода для реконнекта */ }
        }

        private async Task<string> GetLeaderUrlFromConsul()
        {
            // Используем адрес Consul из окружения или дефолт
            string consulAddr = Environment.GetEnvironmentVariable("CONSUL_HTTP_ADDR") ?? "http://localhost:8500";
            using var consul = new ConsulClient(c => c.Address = new Uri(consulAddr));

            var leader = await consul.KV.Get("service/tic-tac-toe/leader");
            if (leader.Response == null) throw new Exception("Лидер не выбран");

            return Encoding.UTF8.GetString(leader.Response.Value);
        }

        private async void OnCellClick(object? sender, EventArgs e)
        {
            if (sender is not Button btn || _client == null) return;
            int index = Array.IndexOf(boardButtons, btn);
            int x = index / 3;
            int y = index % 3;

            try
            {
                await _client.MakeMoveAsync(new MoveRequest { GameId = _gameId, PlayerId = _myPlayerId, X = x, Y = y });
            }
            catch (RpcException ex)
            {
                MessageBox.Show(ex.Status.Detail, "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch { /* Ошибки сети обработает ConnectionLoop */ }
        }

        private void UpdateUI(GameResponse state)
        {
            if (state == null || this.IsDisposed) return;

            // Определяем символ
            string mySymbol = (state.PlayerXId == _myPlayerId) ? "X" : (state.PlayerOId == _myPlayerId ? "O" : "");
            PlayerName.Text = $"Вы: {mySymbol} ({_myPlayerId})";

            // Обновляем доску
            for (int i = 0; i < 9; i++)
            {
                char c = state.Board[i];
                boardButtons[i].Text = (c == '.') ? "" : c.ToString();
                boardButtons[i].BackColor = SystemColors.Control;
            }

            // Подсветка победы
            foreach (var idx in state.WinningLine)
                if (idx >= 0 && idx < 9) boardButtons[idx].BackColor = Color.LightGreen;

            // Логика блокировки кнопок
            bool isMyTurn = state.Status == "Playing" && state.CurrentPlayerId == _myPlayerId;
            for (int i = 0; i < 9; i++)
                boardButtons[i].Enabled = isMyTurn && state.Board[i] == '.';

            player1score.Text = state.PlayerXScore.ToString();
            player2score.Text = state.PlayerOScore.ToString();

            if (state.Status == "Waiting") playerTurn.Text = "Ожидание противника...";
            else if (state.Status == "Playing") playerTurn.Text = isMyTurn ? "ВАШ ХОД!" : "Ход противника...";
            else playerTurn.Text = "Раунд окончен";
        }

        private async Task TryResetRoundAsync()
        {
            if (_client == null) return;
            try { await _client.ResetRoundAsync(new StateRequest { GameId = _gameId }); }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts.Cancel();
            base.OnFormClosing(e);
        }

        private void label1_Click(object sender, EventArgs e) { }
    }
}