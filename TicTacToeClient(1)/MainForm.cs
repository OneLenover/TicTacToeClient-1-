using Grpc.Net.Client;
using TicTacToe.App.Protos;
using Consul;
using System.Net.Http;

namespace TicTacToeClient_1_
{
    public partial class MainForm : Form
    {
        private GameService.GameServiceClient? _client;
        private string? _gameId;
        private string _playerId = ""; 
        private SmallBoardUC[] _boards = new SmallBoardUC[9];
        private System.Windows.Forms.Timer _timer = new();

        public MainForm()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            InitializeComponent();
            for (int i = 0; i < 9; i++) {
                _boards[i] = new SmallBoardUC(i);
                int idx = i;
                _boards[i].OnCellClick += (cIdx) => MakeMove(idx, cIdx);
                flowLayoutPanel1.Controls.Add(_boards[i]);
            }
            _timer.Interval = 1500;
            _timer.Tick += async (s, e) => await RefreshState();
        }

        private async Task<bool> EnsureClient() {
            if (_client != null) return true;
            try {
                using var consul = new ConsulClient(c => c.Address = new Uri("http://localhost:8500"));
                var services = await consul.Health.Service("tictactoe-service", null, true);
                if (services.Response.Length > 0) {
                    var s = services.Response[0].Service;
                    var httpHandler = new HttpClientHandler();
                    httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    var channel = GrpcChannel.ForAddress($"https://{s.Address}:{s.Port}", new GrpcChannelOptions { HttpHandler = httpHandler });
                    _client = new GameService.GameServiceClient(channel);
                    return true;
                }
            } catch { }
            lblStatus.Text = "Поиск сервера...";
            return false;
        }

        private void OnRulesClick()
        {
            string rulesText = @"Ultimate Крестики-Нолики 3x3

Правила игры:
1. Игровое поле состоит из 9 досок 3x3
2. Первый ход делается в любую клетку
3. Следующий ход определяется предыдущим:
   - Играется в доске, соответствующей позиции
   последнего хода (пример: игрок X сходил в
   некое поле в левую нижнюю клетку, тогда
   игрок O после этого сможет сходить только
   в левом нижнем поле и тд)
   - Если доска окажется полностью занятой или
   захваченной одним из игроков, ход возможен
   в любое свободное место
4. Игрок захватывает доску победив по классическим
   правилам крестиков-ноликов
5. Побеждает игрок, первым захвативший 3 доски
   по классическим правилам (3 доски по вертикали,
   горизонтали или диагонали)

Версия: 2.0 (2025)";

            MessageBox.Show(rulesText, "Правила игры", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void OnCheckPlayer() {
            if (string.IsNullOrWhiteSpace(txtLogin.Text)) return;
            if (!await EnsureClient()) return;

            _playerId = txtLogin.Text.Trim();
            try {
                var res = await _client!.CheckSessionAsync(new CheckRequest { PlayerId = _playerId });
                if (res.Exists) {
                    StartGameSession(_playerId, res.GameId);
                } else {
                    loginPanel.Visible = false;
                    roomPanel.Visible = true;
                }
            } catch { lblStatus.Text = "Ошибка связи"; _client = null; }
        }

        private void OnCreateRoom() {
            string rid = new Random().Next(1000, 9999).ToString();
            StartGameSession(_playerId, rid);
        }

        private void OnJoinRoom() {
            if (txtRoomId.Text.Length == 4) StartGameSession(_playerId, txtRoomId.Text);
        }

        private async void StartGameSession(string nick, string rid) {
            if (!await EnsureClient()) return;
            _gameId = rid;
            
            try {
                var r = await _client!.CreateGameAsync(new CreateRequest { PlayerId = $"{nick}|{rid}" });
                
                roomPanel.Visible = false;
                loginPanel.Visible = false;
                headerPanel.Visible = true;
                flowLayoutPanel1.Visible = true;
                
                UpdateUI(r);
                _timer.Start();
            } catch { _client = null; }
        }

        private async void OnResetGame() {
            lblStatus.Text = "Сброс игры...";
            if (!await EnsureClient() || _gameId == null) return;
            try {
                var r = await _client!.ResetGameAsync(new StateRequest { GameId = _gameId, PlayerId = _playerId });
                UpdateUI(r);
            } catch { _client = null; }
        }

        private async void OnExitRoom() {
            if (_gameId != null) {
                try {
                    if (await EnsureClient()) {
                        await _client!.ExitGameAsync(new ExitRequest { GameId = _gameId, PlayerId = _playerId });
                    }
                } catch { }
            }
            _timer.Stop();
            _gameId = null;
            _client = null;
            flowLayoutPanel1.Visible = false;
            headerPanel.Visible = false;
            loginPanel.Visible = true;
            roomPanel.Visible = false;
            lblStatus.BackColor = Color.SkyBlue;
            lblStatus.Text = "Введите логин";
        }

        private async void MakeMove(int bIdx, int cIdx) {
            if (!await EnsureClient() || _gameId == null) return;
            try {
                var r = await _client!.MakeMoveAsync(new MoveRequest {
                    GameId = _gameId, PlayerId = _playerId,
                    BoardX = bIdx % 3, BoardY = bIdx / 3, CellX = cIdx % 3, CellY = cIdx / 3
                });
                if (!string.IsNullOrEmpty(r.Error)) MessageBox.Show(r.Error);
                UpdateUI(r);
            } catch { _client = null; }
        }

        private async Task RefreshState() {
            if (_gameId == null) return;
            if (!await EnsureClient()) return;
            try {
                var r = await _client!.GetStateAsync(new StateRequest { GameId = _gameId, PlayerId = _playerId });
                UpdateUI(r);
            } catch { _client = null; }
        }

        private void UpdateUI(GameResponse r) {
            if (this.InvokeRequired) { this.BeginInvoke(new Action(() => UpdateUI(r))); return; }

            lblInfo.Text = $"ВЫ: {_playerId}  |  КОМНАТА: #{r.GameId}\nИГРОКИ: X({r.PlayerX}) vs O({r.PlayerO})";
            
            btnNewGame.Visible = (r.Status != "Playing");

            bool isMyTurn = (r.CurrentPlayerId == _playerId) && (r.Status == "Playing");
            
            if (r.Status == "Playing") {
                lblStatus.Text = isMyTurn ? "ВАШ ХОД!" : $"Ожидание: {r.CurrentPlayerId}";
                lblStatus.BackColor = Color.SkyBlue;
            } else if (r.Status == "Draw") {
                lblStatus.Text = "ИГРА ЗАВЕРШЕНА: НИЧЬЯ";
                lblStatus.BackColor = Color.LightGray;
            } else {
                bool iAmX = (r.PlayerX == _playerId);
                bool iAmO = (r.PlayerO == _playerId);
                bool xWon = (r.Status == "X_Won");
                bool oWon = (r.Status == "O_Won");

                bool iWon = (iAmX && xWon) || (iAmO && oWon);
                bool opponentWon = (iAmX && oWon) || (iAmO && xWon);

                if (iWon) {
                    lblStatus.Text = "ПОЗДРАВЛЯЕМ! ВЫ ПОБЕДИЛИ!";
                    lblStatus.BackColor = Color.LightGreen;
                } else if (opponentWon) {
                    lblStatus.Text = "ИГРА ОКОНЧЕНА. ВЫ ПРОИГРАЛИ.";
                    lblStatus.BackColor = Color.LightCoral;
                } else {
                    lblStatus.Text = "ИГРА ЗАВЕРШЕНА: " + r.Status;
                    lblStatus.BackColor = Color.LightYellow;
                }
            }

            for (int i = 0; i < 9; i++) {
                string sub = GetSubBoardString(r.FullBoard, i);
                bool active = (r.ActiveBoardX == -1) || (r.ActiveBoardX == i % 3 && r.ActiveBoardY == i / 3);
                _boards[i].UpdateBoard(sub, r.SmallBoardWinners[i], isMyTurn, active);
                _boards[i].SetHighlight(active && isMyTurn && r.Status == "Playing");
            }
        }

        private string GetSubBoardString(string full, int bIdx) {
            if (string.IsNullOrEmpty(full) || full.Length < 81) return ".........";
            char[] sub = new char[9];
            int bX = bIdx % 3, bY = bIdx / 3;
            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 3; x++)
                    sub[y * 3 + x] = full[((bY * 3 + y) * 9 + (bX * 3 + x))];
            return new string(sub);
        }
    }
}