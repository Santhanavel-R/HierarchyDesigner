using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HierarchyDesigner
{
    public class HierarchyComponentPopup : EditorWindow
    {
        private static HierarchyComponentPopup instance;
        private GameObject targetGameObject;
        private Rect targetRowRect;
        private float fadeAlpha = 0f;
        private double lastTime;
        private float scale = 0.98f;

        private class ScriptItem
        {
            public string Name;
            public Texture2D Icon;
            public int Count;
        }

        private class UnityComponentItem
        {
            public string Name;
            public Texture2D Icon;
            public int Count;
        }

        private List<ScriptItem> customScripts = new List<ScriptItem>();
        private List<UnityComponentItem> unityComponents = new List<UnityComponentItem>();
        private int totalComponentCount = 0;

        private const float Width = 260f;
        private const float MaxHeight = 400f;

        // Custom Styles Colors (matching mockup)
        private static readonly Color BgColor = new Color(0.145f, 0.145f, 0.15f, 1f); // #252526
        private static readonly Color HeaderBgColor = new Color(0.18f, 0.18f, 0.19f, 1f); // Slightly lighter
        private static readonly Color BorderColor = new Color(0.25f, 0.25f, 0.26f, 1f); // 1px subtle border
        private static readonly Color DividerColor = new Color(1f, 1f, 1f, 0.08f); // 8% opacity divider

        private static Texture2D cardTexture;
        private static Texture2D headerTexture;
        private static Rect targetRowScreenRect;
        private static Vector2 mouseScreenPosition;

        public static void SetMouseScreenPosition(Vector2 screenPos)
        {
            mouseScreenPosition = screenPos;
        }

        public static void ShowPopup(GameObject go, Rect rowRect)
        {
            if (go == null) return;

            if (instance == null)
            {
                instance = CreateInstance<HierarchyComponentPopup>();
                instance.wantsMouseMove = true;
                instance.lastTime = EditorApplication.timeSinceStartup;
                instance.fadeAlpha = 0f;
                instance.scale = 0.98f;
                instance.ShowPopup();
            }

            // Only rebuild cache when gameobject selection changes
            if (instance.targetGameObject != go)
            {
                instance.targetGameObject = go;
                instance.RebuildComponentCache();
            }

            instance.targetRowRect = rowRect;

            // Resolve target row rect in screen space
            Vector2 rowScreenPos = GUIUtility.GUIToScreenPoint(new Vector2(rowRect.x, rowRect.y));
            targetRowScreenRect = new Rect(rowScreenPos.x, rowScreenPos.y, rowRect.width, rowRect.height);

            instance.PositionWindow();
        }

        private void RebuildComponentCache()
        {
            customScripts.Clear();
            unityComponents.Clear();
            totalComponentCount = 0;

            if (targetGameObject == null) return;

            var comps = targetGameObject.GetComponents<Component>();

            var scriptDict = new Dictionary<Type, ScriptItem>();
            var unityDict = new Dictionary<Type, UnityComponentItem>();

            foreach (var comp in comps)
            {
                if (comp == null || comp is Transform) continue;

                totalComponentCount++;
                Type t = comp.GetType();
                string ns = t.Namespace ?? "";

                bool isUnityComponent = ns.StartsWith("UnityEngine") || ns.StartsWith("UnityEditor") || ns.StartsWith("Unity");
                bool isCustomScript = comp is MonoBehaviour && !isUnityComponent;

                if (isCustomScript)
                {
                    if (scriptDict.TryGetValue(t, out var item))
                    {
                        item.Count++;
                    }
                    else
                    {
                        Texture2D icon = EditorGUIUtility.ObjectContent(null, t).image as Texture2D;
                        scriptDict[t] = new ScriptItem
                        {
                            Name = SplitPascalCase(t.Name),
                            Icon = icon,
                            Count = 1
                        };
                    }
                }
                else
                {
                    if (unityDict.TryGetValue(t, out var item))
                    {
                        item.Count++;
                    }
                    else
                    {
                        Texture2D icon = EditorGUIUtility.ObjectContent(null, t).image as Texture2D;
                        unityDict[t] = new UnityComponentItem
                        {
                            Name = SplitPascalCase(t.Name),
                            Icon = icon,
                            Count = 1
                        };
                    }
                }
            }

            customScripts.AddRange(scriptDict.Values);
            unityComponents.AddRange(unityDict.Values);
        }

        private void PositionWindow()
        {
            if (targetGameObject == null) return;

            Vector2 screenPos = GUIUtility.GUIToScreenPoint(new Vector2(targetRowRect.xMax + 12f, targetRowRect.y));
            float height = CalculateHeight();

            // Screen boundaries check
            Rect screenBounds = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
            if (screenPos.y + height > screenBounds.height - 40f)
            {
                screenPos.y = screenBounds.height - height - 40f;
            }
            if (screenPos.x + Width > screenBounds.width - 20f)
            {
                screenPos.x = GUIUtility.GUIToScreenPoint(new Vector2(targetRowRect.xMin - Width - 12f, targetRowRect.y)).x;
            }

            position = new Rect(screenPos.x, screenPos.y, Width, height);
            Repaint();
        }

        private float CalculateHeight()
        {
            if (totalComponentCount == 0)
            {
                return 130f; // Minimal height for empty state
            }

            float height = 40f; // Header

            if (customScripts.Count > 0)
            {
                height += 24f; // Custom Scripts Label
                height += (customScripts.Count * 22f) + 12f; // Box container and items
                height += 8f; // Spacing
            }

            if (unityComponents.Count > 0)
            {
                if (customScripts.Count > 0)
                {
                    height += 8f; // Divider spacing
                }
                height += 22f; // Unity Components label
                
                // Wrap horizontal items layout
                int itemsPerRow = Mathf.FloorToInt((Width - 24f) / 36f);
                int rows = Mathf.CeilToInt((float)unityComponents.Count / Mathf.Max(1, itemsPerRow));
                height += (rows * 24f) + 12f;
            }

            return Mathf.Min(height + 12f, MaxHeight);
        }

        private void Update()
        {
            double delta = EditorApplication.timeSinceStartup - lastTime;
            lastTime = EditorApplication.timeSinceStartup;

            // Fade in animation (120ms)
            if (fadeAlpha < 1f)
            {
                fadeAlpha = Mathf.Min(1f, fadeAlpha + (float)(delta / 0.12));
                scale = Mathf.Min(1f, scale + (float)(delta / 0.12) * 0.02f);
                Repaint();
            }

            // Close if mouse is no longer over the hierarchy or the popup
            var mouseOver = mouseOverWindow;
            if (mouseOver == null || (mouseOver != this && !mouseOver.GetType().Name.Contains("Hierarchy")))
            {
                Close();
                return;
            }

            if (mouseOver != this)
            {
                Rect bufferPopupRect = new Rect(position.x - 12f, position.y - 12f, position.width + 24f, position.height + 24f);
                if (!targetRowScreenRect.Contains(mouseScreenPosition) && !bufferPopupRect.Contains(mouseScreenPosition))
                {
                    Close();
                }
            }
        }

        private void OnGUI()
        {
            if (targetGameObject == null)
            {
                Close();
                return;
            }

            Color guiColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, fadeAlpha);

            // Card background 9-slice
            GUIStyle cardStyle = new GUIStyle();
            cardStyle.normal.background = GetOrCreateCardTexture();
            cardStyle.border = new RectOffset(8, 8, 8, 8);

            GUILayout.BeginArea(new Rect(0, 0, position.width, position.height), cardStyle);
            
            // Header: Name and Total Added Components count
            DrawHeader();

            if (totalComponentCount == 0)
            {
                DrawEmptyState();
            }
            else
            {
                GUILayout.BeginVertical();
                GUILayout.Space(6f);

                // Section 1: Custom Scripts list
                if (customScripts.Count > 0)
                {
                    DrawScriptsSection();
                }

                // Divider line if both groups are present
                if (customScripts.Count > 0 && unityComponents.Count > 0)
                {
                    DrawSectionDivider();
                }

                // Section 2: Unity Built-in Components icons
                if (unityComponents.Count > 0)
                {
                    DrawUnityComponentsSection();
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndArea();
            GUI.color = guiColor;
        }

        private void DrawHeader()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 40f);
            EditorGUI.DrawRect(rect, HeaderBgColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), BorderColor);

            // GameObject Icon
            Texture2D goIcon = EditorGUIUtility.ObjectContent(targetGameObject, typeof(GameObject)).image as Texture2D;
            if (goIcon != null)
            {
                GUI.DrawTexture(new Rect(rect.x + 12f, rect.y + 11f, 18f, 18f), goIcon);
            }

            // GameObject Name
            GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(rect.x + 34f, rect.y + 10f, Width - 100f, 20f), targetGameObject.name, nameStyle);

            // X Added pill badge
            GUIStyle badgeStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
            };
            Rect badgeRect = new Rect(rect.xMax - 66f, rect.y + 11f, 54f, 18f);

            GUIStyle capsuleStyle = new GUIStyle();
            capsuleStyle.normal.background = GetOrCreateHeaderTexture();
            capsuleStyle.border = new RectOffset(6, 6, 6, 6);
            GUI.Box(badgeRect, "", capsuleStyle);
            
            GUI.Label(badgeRect, $"{totalComponentCount} Added", badgeStyle);
        }

        private void DrawScriptsSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            GUIStyle sectionLabelStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.72f, 0.61f, 1f) } // Scripts purple
            };
            GUILayout.Label("📄 Custom Scripts", sectionLabelStyle);
            GUILayout.EndHorizontal();

            // Box container wrapper
            Rect containerRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(4f);

            foreach (var item in customScripts)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(6f);
                
                // Script Icon
                if (item.Icon != null)
                {
                    GUILayout.Label(item.Icon, GUILayout.Width(16f), GUILayout.Height(16f));
                }
                
                // Name
                GUIStyle nameStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
                };
                GUILayout.Label(item.Name, nameStyle);

                // Multiplier badge if count > 1
                if (item.Count > 1)
                {
                    GUILayout.FlexibleSpace();
                    GUIStyle multiplierStyle = new GUIStyle(EditorStyles.miniBoldLabel)
                    {
                        normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
                    };
                    GUILayout.Label($"x{item.Count}", multiplierStyle);
                    GUILayout.Space(6f);
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }

            GUILayout.Space(2f);
            EditorGUILayout.EndVertical();
            GUILayout.Space(4f);
        }

        private void DrawSectionDivider()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 8f);
            EditorGUI.DrawRect(new Rect(rect.x + 12f, rect.y + 3f, rect.width - 24f, 1f), DividerColor);
        }

        private void DrawUnityComponentsSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            GUIStyle sectionLabelStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.65f, 0.65f, 0.65f) }
            };
            GUILayout.Label("Unity Components", sectionLabelStyle);
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);

            // Flow layout for icons side-by-side
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);
            
            float currentX = 12f;
            float maxRowWidth = Width - 24f;
            float itemWidth = 20f;
            
            foreach (var item in unityComponents)
            {
                // If count > 1 we need extra width to draw the multiplier text "x2"
                float currentItemWidth = item.Count > 1 ? 38f : itemWidth;

                if (currentX + currentItemWidth > maxRowWidth)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(12f);
                    currentX = 12f;
                }

                Rect itemRect = GUILayoutUtility.GetRect(currentItemWidth, 20f);
                
                // Draw Icon
                if (item.Icon != null)
                {
                    GUI.DrawTexture(new Rect(itemRect.x, itemRect.y + 2f, 16f, 16f), item.Icon);
                }
                
                // Draw multiplier text
                if (item.Count > 1)
                {
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        fontSize = 9,
                        normal = { textColor = new Color(0.65f, 0.65f, 0.65f) }
                    };
                    GUI.Label(new Rect(itemRect.x + 18f, itemRect.y + 2f, 20f, 16f), $"x{item.Count}", labelStyle);
                }

                // Native hover tooltip containing the component type name
                GUI.Label(new Rect(itemRect.x, itemRect.y, currentItemWidth, 20f), new GUIContent("", item.Name));

                currentX += currentItemWidth + 6f;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);
        }

        private void DrawEmptyState()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(16f);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.55f, 0.55f, 0.55f) }
            };
            GUIStyle bodyStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.42f, 0.42f, 0.42f) }
            };

            GUILayout.Label("📂  No Added Components", titleStyle);
            GUILayout.Label("This GameObject only contains a Transform component.", bodyStyle);

            GUILayout.EndVertical();
        }

        private static string SplitPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return System.Text.RegularExpressions.Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        }

        private void OnEnable()
        {
            instance = this;
        }

        private static Texture2D GetOrCreateCardTexture()
        {
            if (cardTexture != null) return cardTexture;
            
            int width = 16;
            int height = 16;
            cardTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            int r = 8;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isCorner = false;
                    float dx = 0, dy = 0;
                    if (x < r && y < r) { isCorner = true; dx = x - r; dy = y - r; }
                    else if (x < r && y >= height - r) { isCorner = true; dx = x - r; dy = y - (height - r); }
                    else if (x >= width - r && y < r) { isCorner = true; dx = x - (width - r); dy = y - r; }
                    else if (x >= width - r && y >= height - r) { isCorner = true; dx = x - (width - r); dy = y - (height - r); }

                    if (isCorner)
                    {
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        if (d > r)
                        {
                            cardTexture.SetPixel(x, y, Color.clear);
                        }
                        else if (d > r - 1)
                        {
                            cardTexture.SetPixel(x, y, Color.Lerp(BgColor, BorderColor, 0.5f));
                        }
                        else if (d > r - 2)
                        {
                            cardTexture.SetPixel(x, y, BorderColor);
                        }
                        else
                        {
                            cardTexture.SetPixel(x, y, BgColor);
                        }
                    }
                    else
                    {
                        bool isBorder = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                        cardTexture.SetPixel(x, y, isBorder ? BorderColor : BgColor);
                    }
                }
            }
            cardTexture.Apply();
            return cardTexture;
        }

        private static Texture2D GetOrCreateHeaderTexture()
        {
            if (headerTexture != null) return headerTexture;
            
            int width = 12;
            int height = 12;
            headerTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            int r = 6;
            Color bg = new Color(0.25f, 0.25f, 0.26f, 1f); // badge background
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isCorner = false;
                    float dx = 0, dy = 0;
                    if (x < r && y < r) { isCorner = true; dx = x - r; dy = y - r; }
                    else if (x < r && y >= height - r) { isCorner = true; dx = x - r; dy = y - (height - r); }
                    else if (x >= width - r && y < r) { isCorner = true; dx = x - (width - r); dy = y - r; }
                    else if (x >= width - r && y >= height - r) { isCorner = true; dx = x - (width - r); dy = y - (height - r); }

                    if (isCorner)
                    {
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        headerTexture.SetPixel(x, y, d > r ? Color.clear : bg);
                    }
                    else
                    {
                        headerTexture.SetPixel(x, y, bg);
                    }
                }
            }
            headerTexture.Apply();
            return headerTexture;
        }

        private void OnDestroy()
        {
            instance = null;
        }
    }
}
