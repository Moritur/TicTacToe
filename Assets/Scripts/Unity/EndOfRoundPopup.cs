using System;
using UnityEngine;
using UnityEngine.UI;

namespace TicTac.Unity
{
    /// <summary>Popup that appears when a round of the game is finished.</summary>
    public class EndOfRoundPopup : MonoBehaviour
    {
        [Header("UI Images")]
        [SerializeField, Tooltip("Image representing the symbol that won.")]
        Image symbol;
        [SerializeField, Tooltip("Title declaring the result of the round.")]
        Image title;
        [SerializeField, Tooltip("Details displayed on the panel.")]
        Image details;

        [Header("Buttons")]
        [SerializeField, Tooltip("Opens main menu.")]
        Button homeButton;
        [SerializeField, Tooltip("Starts another round.")]
        Button restartButton;

        [Header("Title Sprites")]
        [SerializeField, Tooltip("Title displayed on victory.")]
        Sprite vicotryTitle;
        [SerializeField, Tooltip("Title displayed on defeat.")]
        Sprite defeatTitle;
        [SerializeField, Tooltip("Title displayed on draw.")]
        Sprite drawTitle;

        [Header("Detail Sprites")]
        [SerializeField, Tooltip("Details displayed on victory.")]
        Sprite victoryDetails;
        [SerializeField, Tooltip("Details displayed on draw.")]
        Sprite drawDetails;
        [SerializeField, Tooltip("Details displayed on defeat.")]
        Sprite defeatDetails;

        [Space]
        [SerializeField, Tooltip("Game object disabled when there is no symbol to display.")]
        GameObject symbolRoot;

        /// <summary>Invoked when <see cref="homeButton"/> is pressed.</summary>
        Action goToMainMenu;
        /// <summary>Invoked when <see cref="restartButton"/> is pressed.</summary>
        Action restartRound;

        void Awake()
        {
            homeButton.onClick.AddListener(GoToMainMenu);
            restartButton.onClick.AddListener(RestartRound);
        }

        public void ShowVictory(Sprite symbol, Action goToMainMenu, Action restartRound) => Show(symbol, goToMainMenu, restartRound, vicotryTitle, victoryDetails);
        public void ShowDefeat(Action goToMainMenu, Action restartRound)                 => Show(null,   goToMainMenu, restartRound, defeatTitle,  defeatDetails);
        public void ShowDraw(Action goToMainMenu, Action restartRound)                   => Show(null,   goToMainMenu, restartRound, drawTitle,    drawDetails);

        void Show(Sprite symbol, Action goToMainMenu, Action restartRound, Sprite title, Sprite details)
        {
            symbolRoot.SetActive(symbol != null); //Hide symbol UI if there is no symbol to show

            this.symbol.sprite  = symbol;
            this.goToMainMenu   = goToMainMenu;
            this.restartRound   = restartRound;
            this.title.sprite   = title;
            this.details.sprite = details;

            gameObject.SetActive(true);
        }

        void GoToMainMenu()
        {
            gameObject.SetActive(false);
            goToMainMenu();
        }

        void RestartRound()
        {
            gameObject.SetActive(false);
            restartRound();
        }
    }
}
