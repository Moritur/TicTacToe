namespace TicTac.Gameplay
{
    /// <summary>Result of an attempt to set a symbol on a grid.</summary>
    public enum MoveResult
    {
        /// <summary>Symbol was successfully set.</summary>
        Success,
        /// <summary>Another symbol is already placed in that spot.</summary>
        Blocked,
        /// <summary>Symbol was successfully set and it created a winning formation.</summary>
        Victory,
        /// <summary>Symbol was successfully set and it filled the last empty field on the grid without creating a winning formation.</summary>
        Tie
    }
}
