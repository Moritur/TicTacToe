#nullable enable
using System;

namespace TicTac.Gameplay
{
    /// <summary>Player controlled by a user.</summary>
    public class Human : Player
    {   
        public Human(Symbol symbol, GameGrid grid, Action<Player, MoveResult> turnFinished) : base(symbol, grid, turnFinished) { }

        /// <summary>Receives user input.</summary>
        public void ReceiveInput(int x, int y)
        {
            var result = grid.TrySetSymbol(symbol, x, y);

            if (result == MoveResult.Blocked) return; //Keep waiting for valid move input

            turnFinished(this, result);
        }
    }
}
