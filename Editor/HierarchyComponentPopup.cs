using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HierarchyDesigner.Runtime;

namespace HierarchyDesigner
{
    public class HierarchyComponentPopup : EditorWindow
    {
        private static HierarchyComponentPopup instance;
        private GameObject targetGameObject;
        private Rect targetRowRect;
        private Vector2 scrollPosition;
        private float fadeAlpha = 0f;
        private double lastTime;
        private float scale = 0.98f;

        private const float Width = 250f;
        private const float MaxHeight = 350f;

        // Custom Styles Colors (matching mockup)
        private static readonly Color BgColor = new Color(0.145f, 0.145f, 0.15f, 1f); // #252526
        private static readonly Color HeaderBgColor = new Color(0.18f, 0.18f, 0.19f, 1f); // Slightly lighter
        private static readonly Color BorderColor = new Color(0.25f, 0.25f, 0.26f, 1f); // 1px subtle border
        private static readonly Color DividerColor = new Color(1f, 1f, 1f, 0.12f); // 12% opacity divider

        // Categories & Accents
        private class CategoryInfo
        {
            public string Name;
            public string Icon;
            public Color AccentColor;
            public List<string> Components = new List<string>();
        }

        private static Texture2D cardTexture;
        private static Texture2D headerTexture;

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

            instance.targetGameObject = go;
            instance.targetRowRect = rowRect;
            instance.PositionWindow();
        }

        private void PositionWindow()
        {
            if (targetGameObject == null) return;

            // Get screen space cursor position
            Vector2 mouseScreenPos = Event.current != null ? GUIUtility.GUIToScreenPoint(Event.current.mousePosition) : Vector2.zero;
            Vector2 screenPos = GUIUtility.GUIToScreenPoint(new Vector2(targetRowRect.xMax + 12f, targetRowRect.y));

            // Calculate height dynamically
            float height = CalculateHeight();

            // Screen boundaries check
            Rect screenBounds = new Rect(0, 0, Screen.currentResolution.width, Screen.currentResolution.height);
            if (screenPos.y + height > screenBounds.height - 40f)
            {
                screenPos.y = screenBounds.height - height - 40f;
            }
            if (screenPos.x + Width > screenBounds.width - 20f)
            {
                // Reposition to the left of the row if it exceeds screen width
                screenPos.x = GUIUtility.GUIToScreenPoint(new Vector2(targetRowRect.xMin - Width - 12f, targetRowRect.y)).x;
            }

            position = new Rect(screenPos.x, screenPos.y, Width, height);
            Repaint();
        }

        private float CalculateHeight()
        {
            float height = 44f; // Header
            
            var categories = GetCategorizedComponents();
            if (categories.Count > 0)
            {
                foreach (var cat in categories)
                {
                    height += 24f; // Category Header
                    height += cat.Components.Count * 18f; // Items
                    height += 12f; // Divider and spacing
                }
            }

            // Metadata footer
            height += 48f;

            return Mathf.Min(height, MaxHeight);
        }

        private List<CategoryInfo> GetCategorizedComponents()
        {
            var categories = new List<CategoryInfo>();
            if (targetGameObject == null) return categories;

            var comps = targetGameObject.GetComponents<Component>();

            // Setup Categories
            var scripts = new CategoryInfo { Name = "Scripts", Icon = "📄", AccentColor = new Color(0.72f, 0.61f, 1f) };
            var rendering = new CategoryInfo { Name = "Rendering", Icon = "📷", AccentColor = new Color(0.31f, 0.85f, 1f) };
            var physics = new CategoryInfo { Name = "Physics", Icon = "⚙", AccentColor = new Color(0.32f, 0.61f, 1f) };
            var lighting = new CategoryInfo { Name = "Lighting", Icon = "💡", AccentColor = new Color(1f, 0.84f, 0f) };
            var audio = new CategoryInfo { Name = "Audio", Icon = "🔊", AccentColor = new Color(1f, 0.4f, 0.4f) };
            var xr = new CategoryInfo { Name = "XR", Icon = "🎮", AccentColor = new Color(0.22f, 0.91f, 0.22f) };
            var anim = new CategoryInfo { Name = "Animation", Icon = "🧩", AccentColor = new Color(1f, 0.54f, 0f) };

            foreach (var comp in comps)
            {
                if (comp == null || comp is Transform || comp is HierarchyHeader) continue;

                string rawName = comp.GetType().Name;
                string cleanName = SplitPascalCase(rawName);

                bool isUnityComponent = comp.GetType().Namespace != null && 
                                       (comp.GetType().Namespace.StartsWith("UnityEngine") || 
                                        comp.GetType().Namespace.StartsWith("UnityEditor") ||
                                        comp.GetType().Namespace.StartsWith("Unity"));

                bool isCustomScript = comp is MonoBehaviour && !isUnityComponent;

                if (isCustomScript)
                {
                    scripts.Components.Add(cleanName);
                }
                else
                {
                    // Map Unity components
                    Type t = comp.GetType();
                    if (t.Name.Contains("Camera") || t.Name.Contains("Renderer") || t.Name.Contains("Canvas"))
                    {
                        rendering.Components.Add(cleanName);
                    }
                    else if (comp is Collider || comp is Collider2D || comp is Rigidbody || comp is Rigidbody2D || t.Name.Contains("Joint"))
                    {
                        physics.Components.Add(cleanName);
                    }
                    else if (comp is Light || comp is ReflectionProbe || t.Name.Contains("Light"))
                    {
                        lighting.Components.Add(cleanName);
                    }
                    else if (comp is AudioSource || comp is AudioListener || t.Name.Contains("Audio"))
                    {
                        audio.Components.Add(cleanName);
                    }
                    else if (t.Name.Contains("XR") || t.Name.Contains("XROrigin") || t.Name.Contains("Interactor"))
                    {
                        xr.Components.Add(cleanName);
                    }
                    else if (comp is Animator || comp is Animation)
                    {
                        anim.Components.Add(cleanName);
                    }
                    else
                    {
                        // Fallback to rendering or physics based on class
                        physics.Components.Add(cleanName);
                    }
                }
            }

            if (scripts.Components.Count > 0) categories.Add(scripts);
            if (rendering.Components.Count > 0) categories.Add(rendering);
            if (physics.Components.Count > 0) categories.Add(physics);
            if (lighting.Components.Count > 0) categories.Add(lighting);
            if (audio.Components.Count > 0) categories.Add(audio);
            if (xr.Components.Count > 0) categories.Add(xr);
            if (anim.Components.Count > 0) categories.Add(anim);

            return categories;
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

            // Check if mouse has exited the popup and target row
            Vector2 mousePos = Event.current != null ? Event.current.mousePosition : Vector2.zero;
            Vector2 screenMousePos = GUIUtility.GUIToScreenPoint(mousePos);

            if (targetGameObject == null || !position.Contains(screenMousePos))
            {
                // Convert target row rect to screen space
                Vector2 rowScreenPos = GUIUtility.GUIToScreenPoint(new Vector2(targetRowRect.x, targetRowRect.y));
                Rect rowScreenRect = new Rect(rowScreenPos.x, rowScreenPos.y, targetRowRect.width, targetRowRect.height);

                // Add 10px buffer to prevent instant close during micro movements
                Rect bufferPopupRect = new Rect(position.x - 10f, position.y - 10f, position.width + 20f, position.height + 20f);

                if (!rowScreenRect.Contains(screenMousePos) && !bufferPopupRect.Contains(screenMousePos))
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

            // Apply fade alpha and scale translation matrix
            Color guiColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, fadeAlpha);

            // Outer Card Rounded Background (9-slice)
            GUIStyle cardStyle = new GUIStyle();
            cardStyle.normal.background = GetOrCreateCardTexture();
            cardStyle.border = new RectOffset(8, 8, 8, 8);

            GUILayout.BeginArea(new Rect(0, 0, position.width, position.height), cardStyle);
            
            // Header Bar
            DrawHeader();

            // Scrollable Content
            var categories = GetCategorizedComponents();
            float contentHeight = position.height - 44f - 48f; // height minus header and footer
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(contentHeight));
            GUILayout.BeginVertical();
            GUILayout.Space(6);

            for (int i = 0; i < categories.Count; i++)
            {
                DrawCategory(categories[i]);
                if (i < categories.Count - 1)
                {
                    DrawCategoryDivider();
                }
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            // Footer (Metadata cards)
            DrawFooter();

            GUILayout.EndArea();

            GUI.color = guiColor;
        }

        private void DrawHeader()
        {
            Rect headerRect = GUILayoutUtility.GetRect(0f, 40f);
            
            // Lighter header background
            EditorGUI.DrawRect(headerRect, HeaderBgColor);
            
            // Bottom header border
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.yMax - 1f, headerRect.width, 1f), BorderColor);

            // Left icon & label
            GUIStyle headerLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };
            
            GUI.Label(new Rect(headerRect.x + 12f, headerRect.y, 150f, headerRect.height), "📦  Components", headerLabelStyle);

            // Right count badge
            int totalCount = targetGameObject.GetComponents<Component>().Length - 1; // subtract transform
            GUIStyle badgeStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                normal = { textColor = Color.white }
            };

            Rect badgeRect = new Rect(headerRect.xMax - 32f, headerRect.y + 11f, 20f, 18f);
            
            // Rounded count badge background
            GUIStyle capsuleStyle = new GUIStyle();
            capsuleStyle.normal.background = GetOrCreateHeaderTexture();
            capsuleStyle.border = new RectOffset(6, 6, 6, 6);
            GUI.Box(badgeRect, "", capsuleStyle);
            
            GUI.Label(badgeRect, totalCount.ToString(), badgeStyle);
        }

        private void DrawCategory(CategoryInfo cat)
        {
            GUILayout.Space(4);
            
            // Category Header Row
            GUILayout.BeginHorizontal();
            GUILayout.Space(12f);

            GUIStyle catIconStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft
            };
            GUILayout.Label(cat.Icon, catIconStyle, GUILayout.Width(18f));

            GUIStyle catNameStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = cat.AccentColor }
            };
            GUILayout.Label(cat.Name, catNameStyle);

            GUILayout.FlexibleSpace();

            // Category Count badge
            GUIStyle badgeStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
            };
            
            Rect badgeRect = GUILayoutUtility.GetRect(18f, 16f);
            badgeRect.y += 2f;
            
            GUIStyle capsuleStyle = new GUIStyle();
            capsuleStyle.normal.background = GetOrCreateHeaderTexture();
            capsuleStyle.border = new RectOffset(6, 6, 6, 6);
            GUI.Box(badgeRect, "", capsuleStyle);
            
            GUI.Label(badgeRect, cat.Components.Count.ToString(), badgeStyle);
            GUILayout.Space(12f);
            GUILayout.EndHorizontal();

            // Items list
            GUIStyle itemStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f, 1f) }
            };

            foreach (var compName in cat.Components)
            {
                // Hover effect highlight check
                Rect itemRect = GUILayoutUtility.GetRect(0f, 18f);
                if (Event.current.type == EventType.Repaint && itemRect.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(itemRect, new Color(1f, 1f, 1f, 0.05f));
                }

                // Dot accent color matches category accent color
                string bulletText = $"   <color=#{ColorUtility.ToHtmlStringRGBA(cat.AccentColor)}>•</color>  {compName}";
                GUIStyle richItemStyle = new GUIStyle(itemStyle) { richText = true };
                GUI.Label(new Rect(itemRect.x + 8f, itemRect.y, itemRect.width - 8f, itemRect.height), bulletText, richItemStyle);
            }
            GUILayout.Space(4);
        }

        private void DrawCategoryDivider()
        {
            Rect dividerRect = GUILayoutUtility.GetRect(0f, 8f);
            EditorGUI.DrawRect(new Rect(dividerRect.x + 12f, dividerRect.y + 3f, dividerRect.width - 24f, 1f), DividerColor);
        }

        private void DrawFooter()
        {
            Rect footerRect = new Rect(0f, position.height - 48f, position.width, 48f);
            
            // Draw separator line
            EditorGUI.DrawRect(new Rect(footerRect.x, footerRect.y, footerRect.width, 1f), BorderColor);
            EditorGUI.DrawRect(new Rect(footerRect.x, footerRect.y + 1f, footerRect.width, footerRect.height - 1f), HeaderBgColor);

            // Display Tag, Layer, and States
            string tag = targetGameObject.tag;
            string layer = LayerMask.LayerToName(targetGameObject.layer);
            bool isActive = targetGameObject.activeSelf;
            bool isStatic = targetGameObject.isStatic;

            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };

            float y = footerRect.y + 6f;
            GUI.Label(new Rect(12f, y, 110f, 16f), $"🏷️ Tag: {tag}", labelStyle);
            GUI.Label(new Rect(130f, y, 110f, 16f), $"📍 Layer: {layer}", labelStyle);

            float yBottom = y + 16f;
            GUI.Label(new Rect(12f, yBottom, 110f, 16f), isActive ? "⚡ Active: Yes" : "⚡ Active: No", labelStyle);
            GUI.Label(new Rect(130f, yBottom, 110f, 16f), isStatic ? "🌍 Static: Yes" : "🌍 Static: No", labelStyle);
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
            Color bg = new Color(0.25f, 0.25f, 0.26f, 1f); // pill badge background
            
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
