using System.Linq;
using TicTac.Unity;
using UnityEditor;
using UnityEngine;

namespace TicTac.Editor
{
    /// <summary>Editor window for creating skin asset bundles.</summary>
    public class SkinEditor : EditorWindow
    {
        [MenuItem("Window/TicTac/Skin Editor")]
        static void ShowWindow() => GetWindow<SkinEditor>();

        /// <summary>Path to the directory where skin asset bundles are stored.</summary>
        static readonly string outputDirectory = "Assets/StreamingAssets/";

        //Fields set from UI
        string skinName;
        Sprite spriteX;
        Sprite spriteO;
        Sprite background;

        /// <summary>True when all values are valid and skin asset bundle can be built.</summary>
        bool AreAllValuesValid => 
            !string.IsNullOrWhiteSpace(skinName) &&
            spriteX    != null    &&
            spriteO    != null    &&
            background != null    &&
            background != spriteO && //Same asset can't be used twice in one asset bundle
            background != spriteX && 
            spriteX    != spriteO;

        void OnGUI()
        {
            skinName = EditorGUILayout.TextField("Skin Name", skinName);
            EditorUtils.SpriteField("X",          ref spriteX);
            EditorUtils.SpriteField("O",          ref spriteO);
            EditorUtils.SpriteField("Background", ref background);

            if (GUILayout.Button("Clear")) ClearAllFields();

            //Disable build button until all values are set
            GUI.enabled = AreAllValuesValid;
            if (GUILayout.Button("Build")) BuildSkinAssetBundle();
            GUI.enabled = true;
        }

        void ClearAllFields()
        {
            spriteX = spriteO = background = null;
            skinName = null;
        }

        void BuildSkinAssetBundle()
        {
            var bundleBuild = new AssetBundleBuild();
            bundleBuild.assetBundleName = skinName;
            bundleBuild.assetNames = EditorUtils.GetAssetPaths(spriteX, spriteO, background);
            
            //Set unversal aliases used to load assets instead of file names
            bundleBuild.addressableNames = new string[] { SkinManager.AssetNameX, SkinManager.AssetNameO, SkinManager.AssetNameBackground };

            //Build asset bundle using LZMA compression, because all assets will always be loaded at the same time
            //If this ever causes loading bundles to take too long, try different compression settings
            //TO DO: Build for all supported platforms with different paths and update runtime loading to include that
            BuildPipeline.BuildAssetBundles(outputDirectory, new[] { bundleBuild }, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

            Debug.Log($"Skin asset bundle '{skinName}' created in '{outputDirectory}'.");

            //Delete manifest asset bundle that was created as part of this build process, because it only includes the asset bundle that was just built
            //It won't contain any asset bundles thet were built before, so it's useless and misleading
            //It's optional and we don't use it, so it can be safely deleted
            //If it's ever needed in the future, we will have to start building all asset bundles together or manually update it, so it will contain all previously built bundles
            string manifestAssetBundlePath = outputDirectory + outputDirectory.TrimEnd('/').Split('/').Last();
            AssetDatabase.DeleteAsset(manifestAssetBundlePath);
            AssetDatabase.DeleteAsset(manifestAssetBundlePath + ".manifest");
        }
    }
}
