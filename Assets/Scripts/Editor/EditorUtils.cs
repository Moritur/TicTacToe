using System;
using UnityEditor;
using UnityEngine;

namespace TicTac.Editor
{
    /// <summary>General utility methods for editor scripts.</summary>
    public static class EditorUtils
    {
        /// <summary>Make a field to receive a <see cref="Sprite"/> and update <paramref name="current"/>.</summary>
        /// <param name="label">Label displayed in UI.</param>
        /// <param name="current">Current value of the field.</param>
        public static void SpriteField(string label, ref Sprite current) => current = EditorGUILayout.ObjectField(label, current, typeof(Sprite), false) as Sprite;

        /// <summary>Returns an array with valid paths to all assets or throws an exception if path to any of them can't be found.</summary>
        /// <returns>An array with valid paths to all assets in the same order as <paramref name="assets"/>.</returns>
        public static string[] GetAssetPaths(params UnityEngine.Object[] assets)
        {
            var result = new string[assets.Length];

            for (int i = 0; i < assets.Length; i++)
            {
                result[i] = GetAssetPath(assets[i]);
            }

            return result;
        }

        /// <summary>Returns valid path to an asset or throws an exception if path can't be found.</summary>
        public static string GetAssetPath(UnityEngine.Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);

            if (string.IsNullOrEmpty(path)) throw new ArgumentException($"Failed to get path for asset {asset}.", nameof(asset));

            return path;
        }
    }
}
