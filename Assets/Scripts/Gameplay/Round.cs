#nullable enable
using System;
using System.Collections.Generic;
using TicTac.Gameplay.Time;

namespace TicTac.Gameplay
{
    /// <summary>Represents a round of the game where two players play until one of them wins or tie is reached.</summary>
    public class Round
    {
        /// <summary>Invoked when round is finished.</summary>
        /// <param name="winner">The winner of this round or null if it ended in a draw.</param>
        public delegate void RoundFinishedHandler(Player? winner);

        //TO DO: Use dependency injection
        static readonly Random random = new Random();

        /// <summary>List of all valid moves found in this turn if hint was requested.</summary>
        /// <remarks>Reuse same list for each hint to avoid allocations.</remarks>
        readonly List<(int x, int y)> validMoves = new List<(int x, int y)>(GameGrid.GridFieldCount);

        /// <inheritdoc cref="RoundFinishedHandler"/>
        readonly RoundFinishedHandler roundFinished;

        public readonly GameGrid grid;
        public readonly GameMode mode;

        /// <summary>Can <see cref="Undo"/> be called during this turn?</summary>
        public bool IsUndoAvailable => mode != GameMode.PlayerVsPlayer;
        /// <summary>Can <see cref="GetHint"/> be called during this turn?</summary>
        public bool IsHintAvailable => mode != GameMode.PlayerVsPlayer;
        /// <summary>Can <see cref="Reset"/> be called during this turn?</summary>
        public bool IsResetAvailable => true;

        /// <summary>Number of seconds a player has to complete their turn.</summary>
        public float TimePerTurn { get; private set; }
        /// <summary>Number of seconds remaining before player whose turn it is will run out of time.</summary>
        public float RemainingTimeInTurn => turnTimedOutAction.TimeRemaining;

        /// <summary>Player represented by <see cref="Symbol.X"/></summary>
        Player playerX;
        /// <summary>Player represented by <see cref="Symbol.O"/></summary>
        Player playerO;

        /// <summary>Action that will get invoked if player doesn't make their move before they run of of time.</summary>
        IScheduledAction turnTimedOutAction;

        /// <summary>
        /// True when it's currently turn of <see cref="playerX"/>.
        /// False when it's currently turn of <see cref="playerO"/>.
        /// </summary>
        bool isPlayerXsTurn;

        /// <summary>The player on whose move we are currently waiting.</summary>
        Player CurrentPlayer => isPlayerXsTurn ? playerX : playerO;
        /// <summary>The player who will be moving in the next turn.</summary>
        Player NextPlayer => isPlayerXsTurn ? playerO : playerX;

        public Round(GameMode mode, GameGrid grid, RoundFinishedHandler roundFinished, ITimeSource time, float timePerTurn)
        {
            this.mode = mode;
            this.grid = grid;
            this.roundFinished = roundFinished;
            this.TimePerTurn = timePerTurn;
            this.turnTimedOutAction = time.Schedule(OnTurnTimedOut, timePerTurn);

            (playerX, playerO) = CreatePlayers(mode);

            StartFirstTurn();
        }

        /// <summary>Creates players that match the mode.</summary>
        (Player x, Player o) CreatePlayers(GameMode mode)
        {
            Player x;
            Player o;
            switch (mode)
            {
                case GameMode.PlayerVsPlayer: //In PvP it doesn't matter which player is which
                    x = new Human(Symbol.X, grid, OnPlayerTurnFinished);
                    o = new Human(Symbol.O, grid, OnPlayerTurnFinished);
                    break;
                case GameMode.PlayerVsEasyAI:
                case GameMode.PlayerVsMediumAI:
                    if (random.NextDouble() < 0.5) //Randomly decide which player gets which symbol
                    {
                        x = new Human(Symbol.X, grid, OnPlayerTurnFinished);
                        o = CreateAiPlayer(mode, Symbol.O);
                    }
                    else
                    {
                        x = CreateAiPlayer(mode, Symbol.X);
                        o = new Human(Symbol.O, grid, OnPlayerTurnFinished);
                    }
                    break;
                default:
                    throw new ArgumentException($"Unexpected game mode: {mode}.", nameof(mode));
            }

            return (x, o);
        }

        /// <summary>Creates AI player matching the given game mode and assigns it the given symbol.</summary>
        AI CreateAiPlayer(GameMode mode, Symbol symbol) => mode switch
        {
            GameMode.PlayerVsEasyAI   => new EasyAI(symbol, grid, OnPlayerTurnFinished),
            GameMode.PlayerVsMediumAI => new MediumAI(symbol, grid, OnPlayerTurnFinished),
            _ => throw new ArgumentException($"Unexpected game mode: {mode}.", nameof(mode))
        };

        /// <summary>Starts this turn from the beginning with the same settings.</summary>
        public void Reset()
        {
            if (!IsResetAvailable) throw new InvalidOperationException($"{nameof(IsResetAvailable)}:{IsResetAvailable}");

            //Create new players, so they get randomly assigned X and O if necessary
            (playerX, playerO) = CreatePlayers(mode);

            //Clear the grid and start the game again
            grid.Reset();

            StartFirstTurn();
        }

        /// <summary>Initiaies first turn of the round.</summary>
        void StartFirstTurn()
        {
            isPlayerXsTurn = true;        //Player X always starts
            turnTimedOutAction.Restart(); //Start counting turn time limit
            
            //Inform the player that their turn has started if they can receive this kind of information
            (CurrentPlayer as AI)?.MakeMove();
        }

        /// <summary>Undoes last move made by both players.</summary>
        public void Undo()
        {
            if (!IsUndoAvailable) throw new InvalidOperationException($"{nameof(IsUndoAvailable)}:{IsUndoAvailable}");

            grid.Undo(2); //Undo last two moves, so it's still the same player's turn
        }

        /// <summary>A hint that suggests a valid move for the current player.</summary>
        /// <returns>Current player's symbol and coordinates of a suggested valid move for them.</returns>
        public (Symbol symbol, int x, int y) GetHint()
        {
            if (!IsHintAvailable) throw new InvalidOperationException($"{nameof(IsHintAvailable)}:{IsHintAvailable}");

            int x, y; //Coordinates of the suggested move

            //At the time of writing this happens to be the same logic as that used by MediumAI, but it servers a different purpose
            //and it should be possible to edit that code independently, so it's not a violation of DRY
            //1. Win if possible
            //2. Block the other player from winning in their turn if they could do that
            //3. Make a random valid move
            if (!grid.TryGetWinningMove(CurrentPlayer.symbol, out x, out y) && !grid.TryGetWinningMove(NextPlayer.symbol, out x, out y))
            {
                validMoves.Clear();
                grid.GetAllValidMoves(validMoves);

                if (validMoves.Count <= 0) throw new InvalidOperationException("There are no valid moves.");

                (x, y) = validMoves[random.Next(validMoves.Count)];
            }

            return (CurrentPlayer.symbol, x, y);
        }

        /// <summary>Forwards grid input to player whose turn it currently is.</summary>
        public void ForwardGridInput(int x, int y) => (CurrentPlayer as Human)?.ReceiveInput(x, y); //Only forward input if this player can receive it

        /// <summary>Called when current turn ends due to player not making any move within the time limit.</summary>
        void OnTurnTimedOut() => FinishRound(NextPlayer); //If current player runs out of time the other player wins

        /// <summary>Called by players when they finish their turns.</summary>
        void OnPlayerTurnFinished(Player player, MoveResult result)
        {
            switch (result)
            {
                case MoveResult.Success:
                    isPlayerXsTurn = !isPlayerXsTurn;
                    turnTimedOutAction.Restart();
                    (CurrentPlayer as AI)?.MakeMove();
                    break;
                case MoveResult.Victory:
                    FinishRound(player);
                    break;
                case MoveResult.Tie:
                    FinishRound(null);
                    break;
                default:
                    throw new ArgumentException($"Unexpected result: {result}.", nameof(result));
            }
        }

        /// <summary>Finishes current round with victory of one of the players or a draw.</summary>
        /// <param name="winner">Winner of this round or null in case of a draw.</param>
        void FinishRound(Player? winner)
        {
            turnTimedOutAction.Cancel(); //No more turns, no need to keep track of time
            roundFinished(winner);
        }
    }
}
