#nullable enable
using System;
using System.Collections.Generic;

namespace TicTac.Gameplay
{
    /// <summary>Simple AI player that makes random moves.</summary>
    public class EasyAI : AI
    {
        //TO DO: Use dependency injection
        static readonly Random Random = new Random();

        /// <summary>List of all valid moves found in this turn.</summary>
        /// <remarks>Reuse same list on each turn to avoid allocations.</remarks>
        readonly List<(int x, int y)> validMoves = new List<(int x, int y)>(GameGrid.GridFieldCount);

        public EasyAI(Symbol symbol, GameGrid grid, Action<Player, MoveResult> turnFinished) : base(symbol, grid, turnFinished) { }

        public override void MakeMove()
        {
            //Find all possible moves in this turn
            validMoves.Clear();
            grid.GetAllValidMoves(validMoves);

            if (validMoves.Count <= 0) throw new InvalidOperationException("There are no valid moves.");

            //Make a random valid move
            (var x, var y) = validMoves[Random.Next(validMoves.Count)];
            var result = grid.TrySetSymbol(symbol, x, y);

            this.turnFinished(this, result);
        }
    }
}
