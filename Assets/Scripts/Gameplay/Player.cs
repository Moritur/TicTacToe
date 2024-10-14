#nullable enable
using System;

namespace TicTac.Gameplay
{
    /// <summary>Decides what move to make in their turn.</summary>
    public abstract class Player
    {
        /// <summary>Symbol representing this player's moves on the grid.</summary>
        public readonly Symbol symbol;
        /// <summary>Grid on which this player moves.</summary>
        protected readonly GameGrid grid;
        /// <summary>Invoked when this player finishes their turn.</summary>
        protected readonly Action<Player, MoveResult> turnFinished;

        protected Player(Symbol symbol, GameGrid grid, Action<Player, MoveResult> turnFinished)
        {
            if (symbol == Symbol.Empty) throw new ArgumentException("Player can't be assigned empty symbol.", nameof(symbol));

            this.symbol = symbol;
            this.grid = grid;
            this.turnFinished = turnFinished;
        }
    }
}
