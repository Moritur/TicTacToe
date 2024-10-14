using System;
using System.Linq;
using TicTac.Gameplay;
using TicTac.Gameplay.Time;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTac.Unity
{
    /// <summary>Controls presentation of data and player interaction in main menu.</summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Main Menu UI elements")]
        [SerializeField, Tooltip("Background image visible in main menu and other parts of the game.")]
        Image background;
        [SerializeField, Tooltip("Dropdown for selecting game mode.")]
        TMP_Dropdown gameModeDropdown;
        [SerializeField, Tooltip("Button that starts new game round.")]
        Button startButton;
        [SerializeField, Tooltip("Input field player can use to type in the name of a skin to load.")]
        TMP_InputField skinNameInput;
        [SerializeField, Tooltip("Button that applies new skin.")]
        Button reskinButton;

        [Space]
        [SerializeField, Tooltip("UI active when game roud is in progress.")]
        RoundUI roundUI;

        [Space]
        [SerializeField, Tooltip("List of available game modes that can be started from main menu.")]
        GameModeData[] gameModes;

        /// <summary>Used to load game skins and access sprites stored in them.</summary>
        SkinManager skinManager;

        ITimeSource time;

        GameMode SelectedGameMode => gameModes[gameModeDropdown.value].mode;
        string SelectedSkinName => skinNameInput.text;

        void Awake()
        {
            //Set up dropdown with available game modes
            gameModeDropdown.ClearOptions();
            gameModeDropdown.AddOptions(gameModes.Select(data => data.displayName).ToList());

            //Subscribe to button presses
            startButton.onClick.AddListener(StartGameRound);
            reskinButton.onClick.AddListener(ApplyNewSkin);
            
            //Allow triggering reskin by submitting its name from input field (enter key on keboard)
            skinNameInput.onSubmit.AddListener(_ => reskinButton.onClick.Invoke());

            //Make sure round UI is not visible at the start of the game
            roundUI.gameObject.SetActive(false);
        }

        /// <summary>Initializes and enables main menu UI. Before that it should be disabled.</summary>
        public void Initialze(SkinManager skinManager, ITimeSource time)
        {
            Debug.Log("Initializing main menu.");
            
            this.time = time;
            this.skinManager = skinManager;
            RefreshSkin();

            //Make sure background color is not modified when skin is loaded
            //It can use a custom color for when skin is loading
            background.color = Color.white;
            background.gameObject.SetActive(true); //Make sure background is active
            gameObject.SetActive(true);
        }

        /// <summary>Applies current skin to all affected UI elements of main menu.</summary>
        void RefreshSkin() => background.sprite = skinManager.Background;

        /// <summary>Starts new round of the game with current settings.</summary>
        void StartGameRound()
        {
            Debug.Log($"Starting new game round in mode {SelectedGameMode}.");

            roundUI.StartGameRound(SelectedGameMode, skinManager.SpriteX, skinManager.SpriteO, OnGameplayFinished, time);

            gameObject.SetActive(false); //Hide main menu
        }

        /// <summary>Called when round or rounds are finished and user wants to go back to main menu.</summary>
        void OnGameplayFinished()
        {
            Debug.Log($"Round finished - opening main menu.");
            gameObject.SetActive(true);
            roundUI.gameObject.SetActive(false);
        }

        /// <summary>Loads and switches to new skin.</summary>
        void ApplyNewSkin()
        {
            reskinButton.interactable = false;
            StartCoroutine(skinManager.TryLoadSkinCoroutine(SelectedSkinName, OnNewSkinLoaded));
        }

        void OnNewSkinLoaded(bool success)
        {
            reskinButton.interactable = true;

            if (!success)
            {
                Debug.LogError($"Failed to load new skin. Check previous error messages for details.");
                return;
            }

            Debug.Log($"Loaded new skin '{skinManager.SkinName}'.");
            
            RefreshSkin();
        }

        /// <summary>Represents a game mode that can be started from main menu and contains information about how it should be presented in UI.</summary>
        [Serializable] struct GameModeData
        {
            public GameMode mode;
            [Tooltip("Name displayed to players in UI.")]
            public string displayName;
        }
    }
}
