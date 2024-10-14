using System;
using System.Collections;
using System.IO;
using UnityEngine;
using TicTac.Gameplay;

namespace TicTac.Unity
{
    /// <summary>Manages loading and unloading of skin asset bundles and exposes currently loaded skin to other types that need it.</summary>
    public class SkinManager
    {
        /// <summary>Path to the directory where skin asset bundles are stored.</summary>
        static readonly string SkinAssetBundleDirectory = Application.streamingAssetsPath;

        //Addressable names of assets in the asset bundle used to load assets without knowing names of the files used to create them
        public static readonly string AssetNameX = "spriteX";
        public static readonly string AssetNameO = "spriteO";
        public static readonly string AssetNameBackground = "spriteBackground";

        /// <summary>Name of the currently loaded skin.</summary>
        public string SkinName { get; private set; }
        /// <summary>Sprite representing <see cref="Symbol.X"/>.</summary>
        public Sprite SpriteX { get; private set; }
        /// <summary>Sprite representing <see cref="Symbol.O"/>.</summary>
        public Sprite SpriteO { get; private set; }
        /// <summary>Background sprite.</summary>
        public Sprite Background { get; private set; }

        /// <summary>Asset bundle from which the current skin was loaded.</summary>
        AssetBundle skinAssetBundle;

        /// <summary>
        /// Private empty constructor.
        /// Other types can create instance of this class using <see cref="CreateCoroutine(string, Action{SkinManager})"/>.
        /// </summary>
        private SkinManager() { }

        /// <summary>
        /// Coroutine that creates new instance of <see cref="SkinManager"/> with the given skin already loaded.
        /// Crated instance is returned as argument in a call to <paramref name="callback"/>.
        /// When creation of new instance fails, null is passed instead.
        /// </summary>
        /// <param name="newSkinName">Name of the skin asset bundle to load.</param>
        /// <param name="callback">Called when corutine is finished.</param>
        public static IEnumerator CreateCoroutine(string newSkinName, Action<SkinManager> callback)
        {
            var instance = new SkinManager(); //Create empty instance
            yield return instance.TryLoadSkinCoroutine(newSkinName, OnLoaded); //Load the requested skin

            void OnLoaded(bool success) => callback(success ? instance : null); //If loading the skin failed, return null instead
        }

        /// <summary>Coroutine that loads new skin, unloads previous skin and calls <paramref name="callback"/> when finished.</summary>
        /// <param name="newSkinName">Name of the skin asset bundle to load.</param>
        /// <param name="callback">Called once loading is finished. Passed argument is true on success and false on failure.</param>
        /// <remarks>If requested skin is already loaded <paramref name="callback"/> will be called instantly.</remarks>
        public IEnumerator TryLoadSkinCoroutine(string newSkinName, Action<bool> callback)
        {
            newSkinName = newSkinName.TrimEnd(' ');

            if (SkinName == newSkinName) { callback(true); yield break; } //This skin is already loaded

            var request = AssetBundle.LoadFromFileAsync(Path.Combine(SkinAssetBundleDirectory, newSkinName));
            
            yield return request;

            var bundle = request.assetBundle;

            if (bundle == null) { callback(false); yield break; } //Failed to load bundle with that name

            callback(TryLoadSkinFromBundle(bundle, newSkinName));
        }

        bool TryLoadSkinFromBundle(AssetBundle newBundle, string newSkinName)
        {
            try //This method should never throw
            {
                var newX  = newBundle.LoadAsset<Sprite>(AssetNameX);
                var newO  = newBundle.LoadAsset<Sprite>(AssetNameO);
                var newBg = newBundle.LoadAsset<Sprite>(AssetNameBackground);

                //Check if all assets were loaded successfully
                if (newX == null || newO == null || newBg == null) return false;

                //Update current skin
                SkinName = newSkinName;
                SpriteX = newX;
                SpriteO = newO;
                Background = newBg;

                //Start unloading previous bundle, but don't wait for it to finish
                if (skinAssetBundle != null) skinAssetBundle.UnloadAsync(true);

                skinAssetBundle = newBundle;
            }
            catch (Exception e) //Log exception to add context to the failure and then return false
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }
    }
}
