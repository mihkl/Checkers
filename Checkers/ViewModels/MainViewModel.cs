using Checkers.Models;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;

using System.Threading;
using System.Threading.Tasks;

namespace Checkers.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ReactiveCommand<CheckersPiece?, Unit> SelectPiece { get; }
    public ObservableCollection<CheckersSquare> Board { get; set; }
    public ObservableCollection<CheckersPiece?> Pieces { get; set; }
    public ObservableCollection<(int, int)> CurrentValidMoves { get; set; }
    public ObservableCollection<CheckersPiece?> BlackPieces { get; set; }
    public ObservableCollection<CheckersPiece?> RedPieces { get; set; }
    public bool IsRedTurn { get; set; } = true;
    public bool IsBlackTurn => !IsRedTurn;
    private bool isGameOver = false;
    private string? gameOverMessage = null;
    private NetworkManager _networkManager;

    public CheckersPiece? SelectedPiece { get; set; }

    public bool IsGameOver {
        get => isGameOver;
        set => this.RaiseAndSetIfChanged(ref isGameOver, value);
    }

    public string? GameOverMessage {
        get => gameOverMessage;
        set => this.RaiseAndSetIfChanged(ref gameOverMessage, value);
    }
    public bool IsPlayer1 { get; private set; }

    public MainViewModel(string[] args) {

        _networkManager = new NetworkManager();
        _networkManager.OnMessageReceived += HandleNetworkMessage;
        _networkManager.OnServerConnected += () => IsPlayer1 = true;
        _networkManager.OnClientConnected += () => IsPlayer1 = false;


        Board = [];
        Pieces = [];
        CurrentValidMoves = [];
        BlackPieces = [];
        RedPieces = [];
        SelectPiece = ReactiveCommand.Create<CheckersPiece?>(TrySelectPiece);

        InitializeBoard();
        InitializePieces();

        if (args[1] == "server") {
            StartServer();
        } else if (args[1] == "client") {
            StartClient();
        }

        Task.Run(() =>
        {
            while (true) {
                _networkManager.PollEvents();
                Thread.Sleep(15);
            }
        });
    }

    private void StartServer() {
        _networkManager.StartServer(9050);
    }

    private void StartClient() {
        _networkManager.StartClient("127.0.0.1", 9050);
    }

    private void HandleNetworkMessage(string message) {
        var move = JsonConvert.DeserializeObject<MoveMessage>(message);
        ApplyMove(move);
    }

    private void ApplyMove(MoveMessage move) {
        var selectedPiece = Pieces[move.FromIndex];
        var piece = Pieces[move.ToIndex];
        selectedPiece.Move(selectedPiece, piece, Pieces);

        UpdateValidMoves(CurrentValidMoves);
        UpdatePieces();
        IsRedTurn = !IsRedTurn;
    }

    private void MakeMove(CheckersPiece piece) {

        var fromIndex = Pieces.IndexOf(SelectedPiece);
        var toIndex = Pieces.IndexOf(piece);
        var moveMessage = new MoveMessage { FromIndex = fromIndex, ToIndex = toIndex };
        var serializedMove = JsonConvert.SerializeObject(moveMessage);

        var didCapture = SelectedPiece?.Move(SelectedPiece, piece, Pieces);

        _networkManager.SendMessage(serializedMove);

        if (didCapture is null or false) {
            CurrentValidMoves = [];
            SelectedPiece = null;
            IsRedTurn = !IsRedTurn;
        } else {
            CurrentValidMoves = AddPossibleMoves(true);
            if (CurrentValidMoves.Count == 0) {
                SelectedPiece = null;
                IsRedTurn = !IsRedTurn;
            }
        }
        UpdateValidMoves(CurrentValidMoves);
    }
    private void TrySelectPiece(CheckersPiece? piece) {
        if (piece is null) return;

        if (SelectedPiece is null && piece is EmptyPiece) {
            return;
        }
        if (SelectedPiece is null || SelectedPiece?.Color == piece.Color) {
            if (piece?.Color == PieceColor.Black && IsRedTurn) return;
            if (piece?.Color == PieceColor.Red && IsBlackTurn) return;
            SelectedPiece = piece;
            CurrentValidMoves = AddPossibleMoves();
            return;
        }
        if (SelectedPiece == piece) {
            SelectedPiece = null;
            CurrentValidMoves = [];
            return;
        }
        if (CurrentValidMoves.Contains((piece.Row, piece.Column))) {
            MakeMove(piece);
            UpdatePieces();
        }
    }

    private void UpdatePieces() {
        BlackPieces.Clear();
        RedPieces.Clear();
        foreach (var piece in Pieces) {
            if (piece is not EmptyPiece && piece.Color is PieceColor.Black) {
                BlackPieces.Add(piece);
            }
            else if (piece is not EmptyPiece && piece.Color is PieceColor.Red) {
                RedPieces.Add(piece);
            }
        }
        CheckGameOver();
    }

    private void CheckGameOver() {
        if (BlackPieces.Count == 0) {
            IsGameOver = true;
            GameOverMessage = "Red Wins!";
        }
        else if (RedPieces.Count == 0) {
            IsGameOver = true;
            GameOverMessage = "Black Wins!";
        }
    }

    private void UpdateValidMoves(ObservableCollection<(int, int)> currentValidMoves) {
        foreach (var square in Board) {
            square.UpdateValidMoves(currentValidMoves);
        }
    }

    private ObservableCollection<(int, int)> AddPossibleMoves(bool isAfterCapture = false) {
        if (SelectedPiece is null) return [];
        var moves = SelectedPiece.GetPossibleMoves(Pieces, isAfterCapture);
        var validMoves = new ObservableCollection<(int, int)>();
        foreach (var move in moves) {
            if (move.Item1 >= 0 && move.Item1 < 8 && move.Item2 >= 0 && move.Item2 < 8) {
                validMoves.Add(move);
            }
        }
        UpdateValidMoves(validMoves);
        return validMoves;
    }

    private void InitializeBoard() {
        for (var i = 0; i < 8; i++) {
            for (var j = 0; j < 8; j++) {
                var square = new CheckersSquare {
                    Row = i,
                    Column = j
                };
                Board.Add(square);
            }
        }
    }

    private void InitializePieces() {
        for (var i = 0; i < 8; i++) {
            for (var j = 0; j < 8; j++) {
                var index = i * 8 + j;
                if ((i + j) % 2 == 0) {
                    if (i < 3) {
                        var piece = new CheckersPiece(index, PieceColor.Black);
                        BlackPieces.Add(piece);
                        Pieces.Add(piece);
                    }
                    else if (i > 4) {
                        var piece = new CheckersPiece(index, PieceColor.Red);
                        RedPieces.Add(piece);
                        Pieces.Add(piece);
                    }
                    else {
                        Pieces.Add(new EmptyPiece(index));
                    }
                }
                else {
                    Pieces.Add(new EmptyPiece(index));
                }
            }
        }
    }
}
