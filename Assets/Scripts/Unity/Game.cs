using TicTac.Unity.Time;
using UnityEngine;

namespace TicTac.Unity
{
    /// <summary>Initializes all core elements of the game on startup and opens main menu.</summary>
    public class Game : MonoBehaviour
    {
        [SerializeField, Tooltip("Name of the skin asset bundle to be loaded when the game starts.")]
        string defaultSkinName;
        [SerializeField, Tooltip("Main menu that will be enabled when all core components of the game are loaded.")]
        MainMenuUI mainMenuUI;

        void Awake()
        {
            Debug.Log("Loading core game elements.");
            mainMenuUI.gameObject.SetActive(false); //Main menu UI should stay disabled until skin manager is created
            StartCoroutine(SkinManager.CreateCoroutine(defaultSkinName, OnSkinManagerCreated));
        }

        /// <summary>Called when <see cref="SkinManager"/> is created and ready to be used.</summary>
        /// <param name="skinManager"><see cref="SkinManager"/> or null if its creation failed.</param>
        void OnSkinManagerCreated(SkinManager skinManager)
        {
            if (skinManager == null)
            {
                Debug.LogError("Failed to create skin manager. Main menu can't be loaded and game will shut down.");
                Application.Quit(); //If we are not in editor, quit the game so player doesn't waste time waiting for nothing
                return;
            }

            //Time source will use this component to start coroutines so it must not be destoryed and stay enabled
            mainMenuUI.Initialze(skinManager, new ScaledUnityTimeSource(this));
        }

        //Time source will use this component to start coroutines so it must not be destoryed and stay enabled
        void OnDisable() => Debug.Log($"{nameof(Game)} component was disabled. This is ok if the game is shutting down, but otherwise some features migth become broken.");
    }
}
