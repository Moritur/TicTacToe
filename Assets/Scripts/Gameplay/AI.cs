#nullable enable
using System;

namespace TicTac.Gameplay
{
    /// <summary>Player controlled by AI.</summary>
    public abstract class AI : Player
    {
        protected AI(Symbol symbol, GameGrid grid, Action<Player, MoveResult> turnFinished) : base(symbol, grid, turnFinished) { }

        /// <summary>Initiaies AI move and calls <see cref="Player.turnFinished"/> when done.</summary>
        public abstract void MakeMove();
    }
}
