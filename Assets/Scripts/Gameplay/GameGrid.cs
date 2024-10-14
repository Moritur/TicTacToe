using System;
using System.Collections.Generic;

#nullable enable
namespace TicTac.Gameplay
{
    /// <summary>3x3 Tic-tac-toe grid with fields that are either X, O or empty.</summary>
    public class GameGrid
    {
        /// <summary>Length of the grid side.</summary>
        public const int GridSize = 3;
        /// <summary>Total number of fields in the gird.</summary>
        public const int GridFieldCount = GridSize * GridSize;
        /// <summary>Upper boundary for coordinates.</summary>
        public const int MaxCoordinate = GridSize - 1;

        /// <summary>Values of all fields of the grid.</summary>
        /// <remarks>Specific fields are accessed using the convention of axis names [X,Y].</remarks>
        readonly Symbol[,] fields = new Symbol[GridSize, GridSize];

        /// <summary>History of all moves that set values of this grid's fields.</summary>
        /// <remarks>
        /// Moves are stored as coordinates without symbols, because grid symbols can't be changed once set.
        /// Moves are added to history in the order in which they are performed.
        /// </remarks>
        Stack<(int x, int y)> moveHistory = new Stack<(int x, int y)>(GridFieldCount);

        /// <summary>Raised when field value changes. Arguments are X and Y coordinate of th field that was modified.</summary>
        event Action<int, int>? FieldChanged;

        /// <summary>Registers handler for event that is raised when field value changes. Arguments are X and Y coordinate of th field that was modified.</summary>
        public void AddFieldChangedListener(Action<int, int>? handler)    => FieldChanged += handler;
        /// <summary>Removes handler added with <see cref="AddFieldChangedListener(Action{int, int}?)"/>.</summary>
        public void RemoveFieldChangedListener(Action<int, int>? handler) => FieldChanged -= handler;

        /// <summary>Attempts to set <paramref name="symbol"/> at the given position on the grid.</summary>
        /// <param name="x">Grid field index along X axis in range [0,2].</param>
        /// <param name="y">Grid field index along Y axis in range [0,2].</param>
        /// <returns>Result of the attempt.</returns>
        public MoveResult TrySetSymbol(Symbol symbol, int x, int y)
        {
            if (IsIndexOutOfRange(x)) throw new ArgumentOutOfRangeException(nameof(x), x, "Argument must be in range [0,2].");
            if (IsIndexOutOfRange(y)) throw new ArgumentOutOfRangeException(nameof(y), y, "Argument must be in range [0,2].");

            //Can't set symbol in a grid field that is not empty
            if (fields[x,y] != Symbol.Empty) return MoveResult.Blocked;

            moveHistory.Push((x, y)); //Save this move in history
            SetSymbolInternal(symbol, x, y);

            if (IsWinningFormation(x, y))            return MoveResult.Victory; //This move formed a winning formation
            if (moveHistory.Count >= GridFieldCount) return MoveResult.Tie;     //If there are no more empty fields it's a tie

            return MoveResult.Success;
        }

        /// <summary>Sets symbol without changing <see cref="moveHistory"/> and raises <see cref="FieldChanged"/>.</summary>
        void SetSymbolInternal(Symbol symbol, int x, int y)
        {
            fields[x, y] = symbol;
            FieldChanged?.Invoke(x, y);
        }

        /// <summary>Returns current symbol at the given grid field.</summary>
        /// <param name="x">Grid field index along X axis in range [0,2].</param>
        /// <param name="y">Grid field index along Y axis in range [0,2].</param>
        public Symbol GetSymbol(int x, int y) => fields[x, y];

        /// <summary>Adds all valid moves to the given list.</summary>
        /// <remarks>Consider reusing same list for multiple queries to save allocations.</remarks>
        public void GetAllValidMoves(List<(int x, int y)> validMoves)
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (GetSymbol(x, y) == Symbol.Empty) validMoves.Add((x, y));
                }
            }
        }

        /// <summary>Clears the grid by setting all the fields to empty.</summary>
        public void Reset()
        {
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    SetSymbolInternal(Symbol.Empty, x, y);
                }
            }

            moveHistory.Clear();
        }

        /// <summary>Undoes last <paramref name="n"/> moves or no moves, if <paramref name="n"/> is greater than the number of moves made.</summary>
        public void Undo(int n)
        {
            if (n > moveHistory.Count) return;

            for (int i = 0; i < n; i++) Undo();
        }

        /// <summary>Undoes last move if any moves were made.</summary>
        public void Undo()
        {
            if (moveHistory.TryPop(out var lastMove))
                SetSymbolInternal(Symbol.Empty, lastMove.x, lastMove.y);
        }

        /// <summary>Returns true if <paramref name="i"/> is not in the acceptable range for X and Y coordinates for this grid, which is [0,2].</summary>
        bool IsIndexOutOfRange(int i) => (i > MaxCoordinate || i < 0);

        /// <summary>Returns true if symbol at the given position is a part of a winning formation.</summary>
        /// <remarks>Winning formation is the same symbol three times in a row, column or diagonally.</remarks>
        bool IsWinningFormation(int x, int y)
        {
            if (moveHistory.Count < GridSize) return false; //Not enough symbols on the board to create a winning formation

            var symbol = fields[x, y];

            if (symbol == Symbol.Empty) return false; //Empty sylbol can't win

            //TO DO: Merge this algorithm with TryGetFinishingMove
            //Start by checking all neighbours
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                var currentX = x + xOffset;

                if (IsIndexOutOfRange(currentX)) continue;

                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    //Check if we are in range and if neighbour has the same value
                    var currentY = y + yOffset;
                    if (yOffset == 0 && xOffset == 0)         continue; //Don't check the same grid field twice
                    if (IsIndexOutOfRange(currentY))          continue;
                    if (symbol != fields[currentX, currentY]) continue;
                    
                    //Move further in the same direction to check if the final symbol is there
                    var nextX = currentX + xOffset;
                    var nextY = currentY + yOffset;

                    if (!IsIndexOutOfRange(nextX) && !IsIndexOutOfRange(nextY) && symbol == fields[nextX, nextY])
                        return true;

                    //Check on the opposite side of the original location, in case symbol was placed in the middle of the combination
                    nextX = x - xOffset;
                    nextY = y - yOffset;

                    if (!IsIndexOutOfRange(nextX) && !IsIndexOutOfRange(nextY) && symbol == fields[nextX, nextY])
                        return true;
                }
            }

            return false; //Return false if no combination was found
        }

        /// <summary>Finds winning move for the given symbol if such move exists.</summary>
        /// <param name="moveX">X coordinate of teh winning move or -1 if no move was found.</param>
        /// <param name="moveX">Y coordinate of teh winning move or -1 if no move was found.</param>
        /// <returns>False if there is no winning move, otherwise true.</returns>
        /// <remarks>When there is more than one winning move the first move that is found is returned.</remarks>
        public bool TryGetWinningMove(Symbol symbol, out int moveX, out int moveY)
        {
            //Not enough symbols on the board to create a winning formation in one move
            if (moveHistory.Count < GridSize - 1)
            {
                moveX = moveY = -1;
                return false;
            }

            //Check all grid fields that have the right symbol
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    //If the symbol matches, check if winning formation can be build from there
                    if (fields[x,y] == symbol && TryGetFinishingMove(x, y, out moveX, out moveY))
                        return true;
                }
            }

            //No winning move was found
            moveX = moveY = -1;
            return false;
        }

        /// <summary>Finds a move that finishes a winning formation started at the given coordinates, if there is an unfinished winning formation there.</summary>
        /// <param name="moveX">X coordinate of teh winning move or -1 if no move was found.</param>
        /// <param name="moveX">Y coordinate of teh winning move or -1 if no move was found.</param>
        /// <returns>False if no move was found.</returns>
        /// <remarks>When there is more than one winning move the first move that is found is returned.</remarks>
        bool TryGetFinishingMove(int x, int y, out int moveX, out int moveY)
        {
            moveX = moveY = -1;

            var symbol = fields[x, y];

            if (symbol == Symbol.Empty) return false; //Empty symbol can't win

            //TO DO: Merge this algorithm with IsWinningFormation
            //Start by checking all neighbours
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                var currentX = x + xOffset;

                if (IsIndexOutOfRange(currentX)) continue;

                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    //Check if we are in range and if neighbour has the same value
                    var currentY = y + yOffset;
                    if (yOffset == 0 && xOffset == 0)         continue; //Don't check the same grid field twice
                    if (IsIndexOutOfRange(currentY))          continue;

                    //Move further in the same direction to check if the final symbol can be placed there
                    var nextX = currentX + xOffset;
                    var nextY = currentY + yOffset;

                    //Check further ahead for same symbol in case this is a gap and the winning move is to fill it
                    if (fields[currentX, currentY] == Symbol.Empty)
                    {
                        if (!IsIndexOutOfRange(nextX) && !IsIndexOutOfRange(nextY) && fields[nextX, nextY] == symbol)
                        {
                            moveX = currentX;
                            moveY = currentY;
                            return true;
                        }
                    }

                    //If this is another symbol and gap was already excluded, there can be no winning formation in that direction
                    if (fields[currentX, currentY] != symbol) continue;

                    //If all symbols so far match, check if the final grid field is empty, so a winning formation can be created by placing the right symbol there
                    if (!IsIndexOutOfRange(nextX) && !IsIndexOutOfRange(nextY) && fields[nextX, nextY] == Symbol.Empty)
                    {
                        moveX = nextX;
                        moveY = nextY;
                        return true;
                    }

                    //Check on the opposite side of the original location, in case search started in the middle of a potential combination
                    nextX = x - xOffset;
                    nextY = y - yOffset;

                    if (!IsIndexOutOfRange(nextX) && !IsIndexOutOfRange(nextY) && fields[nextX, nextY] == Symbol.Empty)
                    {
                        moveX = nextX;
                        moveY = nextY;
                        return true;
                    }
                }
            }

            return false; //Return false if no move was found
        }
    }
}
