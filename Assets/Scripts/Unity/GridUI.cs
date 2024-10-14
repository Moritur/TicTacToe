using System;
using TicTac.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace TicTac.Unity
{
    /// <summary>Presentation and interaction for <see cref="GameGrid"/>.</summary>
    public partial class GridUI : MonoBehaviour
    {
        /// <inheritdoc cref="GameGrid.GridFieldCount"/>
        const int fieldCount = GameGrid.GridFieldCount;
        /// <inheritdoc cref="GameGrid.GridSize"/>
        const int gridSize = GameGrid.GridSize;

        [SerializeField, Tooltip("Sprite used to represent an empty grid field.")]
        Sprite emptyField;

        [SerializeField, Tooltip("Images that will display symbols set in grid fields." +
            "Order is starting at the top left and then moving right until end of the row is reached, then repeating for the next row.")]
        GridField[] fields;

        /// <summary>Sprite representing <see cref="Symbol.X"/></summary>
        Sprite spriteX;
        /// <summary>Sprite representing <see cref="Symbol.O"/></summary>
        Sprite spriteO;
        /// <summary>Round that is currently in progress.</summary>
        Round round;
        /// <summary>Helper for displayeing hints when requested by a user.</summary>
        Hint hint;

        void Reset() => fields = new GridField[fieldCount]; //Number of fields should never change

        void OnValidate()
        {
            //Number of fields should never change
            if (fields == null) fields = new GridField[fieldCount];

            if (fields.Length != fieldCount)
            {
                Debug.LogError($"Numer of fields in grid UI should always be {fieldCount}.");
                Array.Resize(ref fields, fieldCount);
            }
        }

        void Awake()
        {
            //Set up input listeners in UI elements representing grid fields
            for (int i = 0; i < fields.Length; i++)
            {
                (var x, var y) = IndexToCoordinates(i);
                fields[i].button.onClick.AddListener(() => OnFieldInputReceived(x, y));
            }
        }

        /// <summary>Converts field index to (x, y) grid coordinates.</summary>
        (int x, int y) IndexToCoordinates(int index)
        {
            var y = index / gridSize;
            var x = index - (y * gridSize);

            return (x, y);
        }

        /// <summary>Converts grid coordinates to index.</summary>
        int CoordinatesToIndex(int x, int y) => (y * gridSize) + x;

        /// <summary>Sets up input and skin, then enabled this UI.</summary>
        /// <param name="spriteX">Sprite representing <see cref="Symbol.X"/>.</param>
        /// <param name="spriteO">Sprite representing <see cref="Symbol.O"/>.</param>
        public void SetUp(Sprite spriteX, Sprite spriteO, Round round)
        {
            hint ??= new(this); //Create hint helper if it wans't created before

            round?.grid.RemoveFieldChangedListener(UpdateGridField); //If there was a previous round remove subscription from its grid

            this.spriteX = spriteX;
            this.spriteO = spriteO;
            this.round = round;

            //Update all grid fields to match the grid
            for (int x = 0; x < gridSize; x++)
            {
                for(int y = 0; y < gridSize; y++)
                {
                    UpdateGridField(x, y);
                }
            }

            round.grid.AddFieldChangedListener(UpdateGridField);

            gameObject.SetActive(true);
        }

        /// <inheritdoc cref="Hint.Show(Symbol, int, int)"/>
        public void ShowHint(Symbol symbol, int x, int y) => hint.Show(symbol, x, y);

        /// <summary>Called when one of the fields is pressed.</summary>
        void OnFieldInputReceived(int x, int y) => round.ForwardGridInput(x, y);

        /// <summary>Updates visual representation of the grid field at given coordinates to match its symbol.</summary>
        void UpdateGridField(int x, int y)
        {
            hint.Hide(); //Hide hint if grid state has changed

            var symbol = round.grid.GetSymbol(x, y);
            var image = fields[CoordinatesToIndex(x, y)].image;
            
            //Update sprite to match the symbol
            image.sprite = GetSymbolSprite(symbol);
            image.color = Color.white;
        }

        Sprite GetSymbolSprite(Symbol symbol) => symbol switch
        {
            Symbol.Empty => emptyField,
            Symbol.X     => spriteX,
            Symbol.O     => spriteO,
            _ => throw new Exception($"Unexpected symbol {symbol}.")
        };

        /// <summary>Serialized references to grid field components in UI set in editor.</summary>
        [Serializable] struct GridField
        {
            public Image image;
            public Button button;
        }
    }
}
