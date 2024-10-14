using System;
using TicTac.Gameplay;
using TicTac.Gameplay.Time;
using UnityEngine;
using UnityEngine.UI;

namespace TicTac.Unity
{
    public class RoundUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField, Tooltip("Restarts current game round.")]
        Button restartButton;
        [SerializeField, Tooltip("Displays hint for next possible move.")]
        Button hintButton;
        [SerializeField, Tooltip("Reverts last move of both players.")]
        Button undoButton;

        [Header("UI Elements")]
        [SerializeField, Tooltip("Controls display of the game grid.")]
        GridUI gridUI;
        [SerializeField, Tooltip("Popup displayed when round ends.")]
        EndOfRoundPopup endOfRoundPopup;
        [SerializeField, Tooltip("Inner part of progress bar showing the remaining time in current turn.")]
        Image timeBar;

        [Header("Configuration")]
        [SerializeField, Range(1, 30), Tooltip("Number of seconds player has to complete their turn.")]
        float timePerTurn;

        Round round;
        /// <summary>Invoked when there are no more rounds to be played.</summary>
        Action onGameplayFinished;
        /// <summary>Sprite representing <see cref="Symbol.X"/>.</summary>
        Sprite spriteX;
        /// <summary>Sprite representing <see cref="Symbol.O"/>.</summary>
        Sprite spriteO;

        void Awake()
        {
            restartButton.onClick.AddListener(Restart);
            hintButton.onClick.AddListener(Hint);
            undoButton.onClick.AddListener(Undo);
        }

        /// <param name="spriteX">Sprite representing <see cref="Symbol.X"/>.</param>
        /// <param name="spriteO">Sprite representing <see cref="Symbol.O"/>.</param>
        public void StartGameRound(GameMode mode, Sprite spriteX, Sprite spriteO, Action onGameplayFinished, ITimeSource time)
        {
            this.spriteO = spriteO;
            this.spriteX = spriteX;
            this.onGameplayFinished = onGameplayFinished;
            round = new Round(mode, new GameGrid(), OnRoundFinished, time, timePerTurn);

            gridUI.SetUp(spriteX, spriteO, round);

            restartButton.gameObject.SetActive(round.IsResetAvailable);
            hintButton.gameObject.SetActive(round.IsHintAvailable);
            undoButton.gameObject.SetActive(round.IsUndoAvailable);

            gameObject.SetActive(true);
        }

        void Update() => timeBar.fillAmount = round.RemainingTimeInTurn / round.TimePerTurn;

        /// <inheritdoc cref="Round.RoundFinishedHandler"/>
        void OnRoundFinished(Player winner)
        {
            if (winner == null) //No winner means a draw
            {
                endOfRoundPopup.ShowDraw(onGameplayFinished, Restart);
            }
            else
            {
                var symbol = winner.symbol == Symbol.X ? spriteX : spriteO; //Sprite with symbol of the winner

                if (round.mode == GameMode.PlayerVsPlayer) endOfRoundPopup.ShowVictory(symbol, onGameplayFinished, Restart); //In PvP always show victory, but include symbol of the winner to make it clear who won
                else if (winner is AI)                     endOfRoundPopup.ShowDefeat(onGameplayFinished, Restart);          //If AI won then show defeat popup to the player
                else                                       endOfRoundPopup.ShowVictory(null, onGameplayFinished, Restart);   //If player won against AI show victory, but don't include player's symbol, since it's not necassary
            }

            gameObject.SetActive(false); //Hide round UI while popup is open
        }

        /// <inheritdoc cref="Round.Reset"/>
        void Restart()
        {
            gameObject.SetActive(true);
            round.Reset();
        }

        /// <inheritdoc cref="GridUI.ShowHint(Symbol, int, int)"/>
        void Hint()
        {
            (var symbol, var x, var y) = round.GetHint();
            gridUI.ShowHint(symbol, x , y);
        }

        /// <inheritdoc cref="Round.Undo"/>
        void Undo() => round.Undo();
    }
}
