#nullable enable
using System;
using System.Collections.Generic;

namespace TicTac.Gameplay
{
    /// <summary>AI player that always takes the opportunity to win in the current turn or block the other player from winning in their turn.</summary>
    public class MediumAI : AI
    {
        //TO DO: Use dependency injection
        static readonly Random Random = new Random();

        /// <summary>List of all valid moves found in this turn.</summary>
        /// <remarks>Reuse same list on each turn to avoid allocations.</remarks>
        readonly List<(int x, int y)> validMoves = new List<(int x, int y)>(GameGrid.GridFieldCount);
        
        /// <summary>Symbol representing the other player.</summary>
        readonly Symbol oppositeSymbol;

        public MediumAI(Symbol symbol, GameGrid grid, Action<Player, MoveResult> turnFinished) : base(symbol, grid, turnFinished)
        {
            oppositeSymbol = symbol switch
            {
                Symbol.X => Symbol.O,
                Symbol.O => Symbol.X,
                _ => throw new ArgumentException($"Unexpected symbol: {symbol}", nameof(symbol))
            };
        }

        public override void MakeMove()
        {
            int x, y; //Coordinates of the move this player will make in this turn

            //1. Win if possible
            //2. Block the other player from winning in their turn if they could do that
            //3. Make a random valid move
            if (!grid.TryGetWinningMove(symbol, out x, out y) && !grid.TryGetWinningMove(oppositeSymbol, out x, out y))
            {
                validMoves.Clear();
                grid.GetAllValidMoves(validMoves);

                if (validMoves.Count <= 0) throw new InvalidOperationException("There are no valid moves.");

                (x, y) = validMoves[Random.Next(validMoves.Count)];
            }

            var result = grid.TrySetSymbol(symbol, x, y);

            this.turnFinished(this, result);
        }
    }
}
