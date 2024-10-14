using System.Collections;
using TicTac.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace TicTac.Unity
{
    public partial class GridUI
    {
        //TO DO: Decouple this class from GridUI
        /// <summary>Helper class for managing state of hint displayed on grid.</summary>
        /// <remarks>This class helps keep that feature separate from the rest of the <see cref="GridUI"/> code.</remarks>
        class Hint
        {
            readonly GridUI gridUI;

            Coroutine coroutine;
            /// <summary>Last image that was used to display a hint.</summary>
            Image image;
            /// <summary>Sprite displayed by <see cref="image"/> before it was replaced by the hint.</summary>
            Sprite sprite;
            /// <summary>Color of <see cref="image"/> before it was changed for the hint.</summary>
            Color color;

            static WaitForSeconds waitForHint = new WaitForSeconds(1.5f); //Create once and reuse

            public Hint(GridUI gridUI) => this.gridUI = gridUI;

            /// <summary>Shows the given symbol as a hint on the given coordinates.</summary>
            /// <remarks>Hint will disappear on its own after a set amout of time.</remarks>
            public void Show(Symbol symbol, int x, int y)
            {
                Hide(); //Make sure there is only one hint visible at a time
                coroutine = gridUI.StartCoroutine(ShowHintCoroutine(symbol, x, y));
            }

            /// <summary>Hides currently displayed hint if there is any.</summary>
            public void Hide()
            {
                if (coroutine == null) return;

                gridUI.StopCoroutine(coroutine);
                HideInternal();
            }

            /// <summary>Hides currently displayed hint if there is any and sets <see cref="coroutine"/> to null,
            /// but doesn't stop any active coroutines.</summary>
            void HideInternal()
            {
                image.sprite = sprite;
                image.color = color;

                coroutine = null;
            }

            IEnumerator ShowHintCoroutine(Symbol symbol, int x, int y)
            {
                //Get the image component and save its current status, so it can be restored later
                image = gridUI.fields[gridUI.CoordinatesToIndex(x, y)].image;
                sprite = image.sprite;
                color = image.color;

                //Display the hint
                image.sprite = gridUI.GetSymbolSprite(symbol);
                image.color = new Color(1, 1, 1, 0.5f);

                yield return waitForHint; //Wait before hiding it

                HideInternal();
            }
        }
    }
}
