using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TicTac.Gameplay;
using TicTac.Gameplay.Utils;

namespace TicTac.Tests.Gameplay
{
    public class GameGridTests
    {
        /// <summary>All possible valid coordinates for 3x3 grid.</summary>
        static (int x, int y)[] validCoordinates = new (int x, int y)[9]
        {
            (0, 0),(1, 0),(2, 0),
            (0, 1),(1, 1),(2, 1),
            (0, 2),(1, 2),(2, 2)
        };

        /// <summary>Different sets of valid coordinates that can be used for testing.</summary>
        static IEnumerable<(int x, int y)[]> TestCoordinates()
        {
            yield return validCoordinates; //All

            foreach (var c in validCoordinates) //Just one
            {
                yield return c.AsEnumerable().ToArray();
            }

            for (int i = 0; i < validCoordinates.Length; i++)
            {
                var coordinates = validCoordinates.SkipAt(i); //All but one

                yield return coordinates.ToArray();

                //Skip some more to make holes in the grid
                for (int j = 0; j < (7 - i); j++)
                {
                    coordinates = coordinates.SkipAt(i);
                    yield return coordinates.ToArray();
                }
            }
        }

        /// <summary>Wraps all values form <see cref="TestCoordinates"/> into <see cref="TestCaseData"/>, so NUnit can properly recognize array as test case argument.</summary>
        static IEnumerable<TestCaseData> TestCoordinatesTestCases() => TestCoordinates().Select(c => new TestCaseData(arg: c));

        /// <summary>Returns X for even numbers or 0 and O for odd numbers.</summary>
        /// <remarks>The goal of this method is to make tests utilize both symbols in a deterministic manner.</remarks>
        Symbol GetSymbolForIndex(int i) => (i % 2) > 0 ? Symbol.O : Symbol.X; //Alternate between symbols

        static IEnumerable<TestCaseData> UndoTestCases()
        {
            foreach (var c in WithUndoCount(1)) { yield return c; }
            foreach (var c in WithUndoCount(2)) { yield return c; }
            foreach (var c in WithUndoCount(6)) { yield return c; }
            foreach (var c in WithUndoCount(9)) { yield return c; }

            IEnumerable<TestCaseData> WithUndoCount(int count)
            {
                foreach (var coordinates in TestCoordinates().Where(c => c.Length >= count))
                {
                    yield return new TestCaseData(coordinates, count);
                }
            }
        }

        [TestCaseSource(nameof(UndoTestCases))]
        public void Undo((int x, int y)[] fieldsToSet, int undoCount)
        {
            var grid = new GameGrid();
            
            //Set all requested fields
            for (int i = 0; i < fieldsToSet.Length; i++)
            {
                var field = fieldsToSet[i];
                var symbol = GetSymbolForIndex(i);
                var result = grid.TrySetSymbol(symbol, field.x, field.y);

                Assert.AreNotEqual(MoveResult.Blocked, result);
            }

            grid.Undo(undoCount);
            var lastUndoneIndex = fieldsToSet.Length - undoCount;

            //Check if all fields are in expected state
            for (int i = 0; i < fieldsToSet.Length; i++)
            {
                var field = fieldsToSet[i];
                var symbol = grid.GetSymbol(field.x, field.y);
                var expectedSymbol = i >= lastUndoneIndex ? Symbol.Empty : GetSymbolForIndex(i);

                Assert.AreEqual(expectedSymbol, symbol);
            }
        }

        [TestCaseSource(nameof(TestCoordinatesTestCases))]
        public void SetGetAndReset((int x, int y)[] fieldsToSet)
        {
            var grid = new GameGrid();

            for (int i = 0; i < fieldsToSet.Length; i++)
            {
                var field = fieldsToSet[i];

                var result = grid.TrySetSymbol(GetSymbolForIndex(i), field.x, field.y);
                Assert.AreNotEqual(MoveResult.Blocked, result, $"Failed to set field {field} that should be empty.");

                var symbol = grid.GetSymbol(field.x, field.y);
                Assert.AreEqual(GetSymbolForIndex(i), symbol, $"Field {field} has different value than the one it was set to.");

                result = grid.TrySetSymbol(GetSymbolForIndex(i + 1), field.x, field.y);
                Assert.AreEqual(MoveResult.Blocked, result, $"Field {field} was already set, but attempt to set it again was not blocked.");
            }

            //Test getting list of all valid moves (can be used by hints and AI)
            var expectedValidMoves = validCoordinates.Length - fieldsToSet.Length;
            var validMoves = new List<(int x, int y)>(expectedValidMoves);
            grid.GetAllValidMoves(validMoves);
            Assert.AreEqual(expectedValidMoves, validMoves.Count, "Invalid number of valid moves.");
            var intersectCount = fieldsToSet.Intersect(validMoves).Count();
            Assert.AreEqual(0, intersectCount, "Field that was already set should not be considered a valid move.");

            //Make sure all fields are empty after reset
            grid.Reset();
            for (int i = 0; i < validCoordinates.Length; i++)
            {
                var field = validCoordinates[i];
                var symbol = grid.GetSymbol(field.x, field.y);
                Assert.AreEqual(Symbol.Empty, symbol, $"Field {field} is not empty after grid was reset.");
            }
        }

        /// <summary>All 8 existing winning formations.</summary>
        static (int x, int y)[][] winningFormations = new (int x, int y)[8][]
        {
            new []{ (0, 0),(1, 0),(2, 0) }, //Upper row
            new []{ (0, 1),(1, 1),(2, 1) }, //Middle row
            new []{ (0, 2),(1, 2),(2, 2) }, //Bottom row
            new []{ (0, 0),(0, 1),(0, 2) }, //Left column
            new []{ (1, 0),(1, 1),(1, 2) }, //Middle column
            new []{ (2, 0),(2, 1),(2, 2) }, //Rigth column
            new []{ (0, 0),(1, 1),(2, 2) }, //Diagonal 1
            new []{ (2, 0),(1, 1),(0, 2) }, //Diagonal 2
        };

        /// <summary>Some formations that are not winning.</summary>
        static (int x, int y)[][] notWinningFormations = new (int x, int y)[][]
        {
            new []{ (0, 0),(1, 1),(2, 0) },
            new []{ (0, 1),(1, 2),(2, 1) },
            new []{ (0, 2),(1, 2),(2, 1) },
            new []{ (2, 0),(0, 1),(0, 2) },
            new []{ (1, 0),(1, 1),(0, 1) },
            new []{ (1, 2),(2, 1),(2, 2) },
            new []{ (0, 0),(0, 1),(2, 2) },
            new []{ (2, 0),(2, 1),(1, 2) },
        };

        //TO DO: add all 16 possible draw formations, since 16 is still a small number
        /// <summary>Some formations that lead to a draw.</summary>
        static Symbol[][,] drawFormations = new Symbol[][,]
        {
            new Symbol[,]
            {
                { Symbol.O, Symbol.X, Symbol.X },
                { Symbol.X, Symbol.O, Symbol.O },
                { Symbol.O, Symbol.X, Symbol.X },
            },
            new Symbol[,]
            {
                { Symbol.X, Symbol.X, Symbol.O },
                { Symbol.O, Symbol.O, Symbol.X },
                { Symbol.X, Symbol.O, Symbol.X },
            },
            new Symbol[,]
            {
                { Symbol.O, Symbol.X, Symbol.O },
                { Symbol.X, Symbol.O, Symbol.X },
                { Symbol.X, Symbol.O, Symbol.X },
            },
            new Symbol[,]
            {
                { Symbol.X, Symbol.O, Symbol.O },
                { Symbol.O, Symbol.X, Symbol.X },
                { Symbol.X, Symbol.X, Symbol.O },
            },
        };

        static IEnumerable<TestCaseData> WinTestCases()
        {
            foreach (var formation in winningFormations)
            {
                yield return new TestCaseData(formation, Symbol.X);
                yield return new TestCaseData(formation, Symbol.O);
            }
        }

        [TestCaseSource(nameof(drawFormations))]
        public void Draw(Symbol[,] formation)
        {
            GameGrid grid = new GameGrid();

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    var result = grid.TrySetSymbol(formation[x, y], x, y);

                    //All moves should just succeed and the last one should result in a tie
                    if (x == 2 && y == 2) Assert.AreEqual(MoveResult.Tie, result);
                    else                  Assert.AreEqual(MoveResult.Success, result);
                }
            }
        }

        //There are no separate tests for losing, because one player's defeat is just another player's victory
        [TestCaseSource(nameof(WinTestCases))]
        public void Win((int x, int y)[] winningFormation, Symbol winner)
        {
            var grid = new GameGrid();

            var loser = winner == Symbol.X ? Symbol.O : Symbol.X;
            //Pick some loosing moves for the opposite symbol
            //Skip different number for each test case to improve the spread of test data
            var losingFormation = validCoordinates.Except(winningFormation).Skip(winningFormation[0].x).Take(2).ToList();

            //Execute all moves except for the last one
            for (int i = 0; i < winningFormation.Length - 1; i++)
            {
                //Winner moves first
                (var x, var y) = winningFormation[i];
                var result = grid.TrySetSymbol(winner, x, y);
                Assert.AreEqual(MoveResult.Success, result);

                //Then looser
                (x, y) = losingFormation[i];
                result = grid.TrySetSymbol(loser, x, y);
                Assert.AreEqual(MoveResult.Success, result);
            }

            var winningMove = winningFormation.Last(); //Final move should win

            //Check if grid can correctly predict that move (can be used by hints and AI)
            var foundWinningMove = grid.TryGetWinningMove(winner, out var winX, out var winY);
            Assert.IsTrue(foundWinningMove, $"Winning move exists at {winningMove}, but was not found by grid.");
            Assert.AreEqual(winningMove, (winX, winY), "Winning move found by grid is different from the actual winning move.");

            //Making winning move should result in victory
            var victory = grid.TrySetSymbol(winner, winningMove.x, winningMove.y);
            Assert.AreEqual(MoveResult.Victory, victory, $"Finishing winning combination at {winningMove} did not result in victory.");
        }
    }
}
