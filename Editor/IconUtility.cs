using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HierarchyDesigner.Editor
{
    /// <summary>
    /// Utility class containing helper methods for working with built-in Unity editor icons.
    /// </summary>
    public static class IconUtility
    {
        #region Constants & Fields

        /// <summary>
        /// A list of standard Unity built-in icons that are safe to use across editor versions.
        /// </summary>
        private static readonly List<string> BuiltInIcons = new List<string>
        {
            string.Empty,
            "d_Folder Icon",
            "d_GameManager Icon",
            "d_Settings Icon",
            "d_AudioSource Icon",
            "d_Camera Icon",
            "d_Canvas Icon",
            "d_DirectionalLight Icon",
            "d_EventSystem Icon",
            "d_MeshRenderer Icon",
            "d_ParticleSystem Icon",
            "d_PreMatCube",
            "d_SceneAsset Icon",
            "d_Shader Icon",
            "d_Terrain Icon",
            "d_Transform Icon",
            "cs Script Icon",
            "d_GridAxisIcon",
            "d_Favorite",
            "d_Filter",
            "d_Info",
            "d_Warning",
            "d_Error",
            "d_Settings",
            "d_Help"
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the list of standard built-in Unity icon names.
        /// </summary>
        public static IReadOnlyList<string> GetBuiltInIconNames()
        {
            return BuiltInIcons;
        }

        /// <summary>
        /// Retrieves the Texture2D associated with a built-in Unity icon name.
        /// </summary>
        /// <param name="iconName">The built-in icon name.</param>
        /// <returns>The Texture2D if found, otherwise null.</returns>
        public static Texture2D GetIconTexture(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                return null;
            }

            GUIContent iconContent = EditorGUIUtility.IconContent(iconName);
            if (iconContent != null && iconContent.image != null)
            {
                return iconContent.image as Texture2D;
            }

            return null;
        }

        #endregion
    }
}
