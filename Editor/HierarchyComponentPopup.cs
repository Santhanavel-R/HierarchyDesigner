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

        private string searchQuery = "";
        
        // Cache collapsed states per category to persist during hover
        private static readonly Dictionary<string, bool> CategoryCollapsedStates = new Dictionary<string, bool>()
        {
            { "Scripts", false },
            { "Rendering", false },
            { "Physics", false },
            { "Audio", false },
            { "Animation", false },
            { "UI", false },
            { "Navigation", false },
            { "AI", false },
            { "Lighting", false },
            { "Miscellaneous", false }
        };

        // Cached Data structures
        private class CachedComponent
        {
            public Component Component;
            public string CleanName;
            public Texture2D Icon;
        }

        private class CategoryGroup
        {
            public string Name;
            public string IconStr;
            public Color TitleColor;
            public List<CachedComponent> Items = new List<CachedComponent>();
            public List<CachedComponent> FilteredItems = new List<CachedComponent>();
        }

        private List<CategoryGroup> cachedCategories = new List<CategoryGroup>();
        private int totalComponentCount = 0;
        private int totalFilteredCount = 0;

        private const float Width = 340f;
        private const float MaxHeight = 450f;

        // Custom Styles Colors (matching mockup)
        private static readonly Color BgColor = new Color(0.145f, 0.145f, 0.15f, 1f); // #252526
        private static readonly Color HeaderBgColor = new Color(0.18f, 0.18f, 0.19f, 1f); // Slightly lighter
        private static readonly Color BorderColor = new Color(0.25f, 0.25f, 0.26f, 1f); // 1px subtle border
        private static readonly Color DividerColor = new Color(1f, 1f, 1f, 0.08f); // 8% opacity divider

        // Categories Details
        private static readonly Dictionary<string, (string Icon, Color Color)> CategoryDetails = new Dictionary<string, (string, Color)>()
        {
            { "Scripts", ("#", new Color(0.72f, 0.61f, 1f)) },
            { "Rendering", ("📷", new Color(0.31f, 0.85f, 1f)) },
            { "Physics", ("⚙", new Color(0.48f, 0.88f, 0.48f)) },
            { "Audio", ("🔊", new Color(0.22f, 0.91f, 0.91f)) },
            { "Animation", ("🧩", new Color(1f, 0.64f, 0f)) },
            { "UI", ("🎨", new Color(1f, 0.41f, 0.7f)) },
            { "Navigation", ("🗺", new Color(1f, 0.84f, 0f)) },
            { "AI", ("🧠", new Color(1f, 0.4f, 0.4f)) },
            { "Lighting", ("💡", new Color(1f, 0.92f, 0.5f)) },
            { "Miscellaneous", ("📦", new Color(0.7f, 0.7f, 0.7f)) }
        };

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

            // Resolve target row rect in screen space (since GUI context is active here)
            Vector2 rowScreenPos = GUIUtility.GUIToScreenPoint(new Vector2(rowRect.x, rowRect.y));
            targetRowScreenRect = new Rect(rowScreenPos.x, rowScreenPos.y, rowRect.width, rowRect.height);

            instance.PositionWindow();
        }

        private void RebuildComponentCache()
        {
            cachedCategories.Clear();
            totalComponentCount = 0;

            if (targetGameObject == null) return;

            var comps = targetGameObject.GetComponents<Component>();
            var groupDict = new Dictionary<string, CategoryGroup>();

            foreach (var detail in CategoryDetails)
            {
                groupDict[detail.Key] = new CategoryGroup
                {
                    Name = detail.Key,
                    IconStr = detail.Value.Icon,
                    TitleColor = detail.Value.Color
                };
            }

            foreach (var comp in comps)
            {
                if (comp == null || comp is Transform || comp is HierarchyHeader) continue;

                totalComponentCount++;
                string rawName = comp.GetType().Name;
                string cleanName = SplitPascalCase(rawName);

                Texture2D icon = EditorGUIUtility.ObjectContent(null, comp.GetType()).image as Texture2D;

                CachedComponent cachedComp = new CachedComponent
                {
                    Component = comp,
                    CleanName = cleanName,
                    Icon = icon
                };

                // Map to categories
                string category = CategorizeComponent(comp);
                groupDict[category].Items.Add(cachedComp);
            }

            foreach (var group in groupDict.Values)
            {
                if (group.Items.Count > 0)
                {
                    cachedCategories.Add(group);
                }
            }

            FilterComponents();
        }

        private string CategorizeComponent(Component comp)
        {
            Type t = comp.GetType();
            string ns = t.Namespace ?? "";

            bool isUnityComponent = ns.StartsWith("UnityEngine") || ns.StartsWith("UnityEditor") || ns.StartsWith("Unity");
            if (comp is MonoBehaviour && !isUnityComponent)
            {
                return "Scripts";
            }

            if (t.Name.Contains("Camera") || t.Name.Contains("Renderer") || t.Name.Contains("CanvasRenderer") || t.Name.Contains("MeshFilter") || comp is Skybox || comp is Projector)
            {
                return "Rendering";
            }
            if (comp is Collider || comp is Collider2D || comp is Rigidbody || comp is Rigidbody2D || t.Name.Contains("Joint") || comp is ConstantForce)
            {
                return "Physics";
            }
            if (comp is AudioSource || comp is AudioListener || t.Name.Contains("Audio"))
            {
                return "Audio";
            }
            if (comp is Animator || comp is Animation || t.Name.Contains("Playable"))
            {
                return "Animation";
            }
            if (t.Name.Contains("Canvas") || t.Name.Contains("Graphic") || t.Name.Contains("Button") || t.Name.Contains("Text") || t.Name.Contains("Image") || t.Name.Contains("Layout") || t.Name.Contains("EventSystem"))
            {
                return "UI";
            }
            if (t.Name.Contains("NavMesh") || t.Name.Contains("OffMeshLink"))
            {
                return "Navigation";
            }
            if (t.Name.Contains("Agent") || t.Name.Contains("Sensor") || t.Name.Contains("Decision") || t.Name.Contains("Behaviour"))
            {
                return "AI";
            }
            if (comp is Light || comp is ReflectionProbe || comp is LightProbeGroup || t.Name.Contains("Light"))
            {
                return "Lighting";
            }

            return "Miscellaneous";
        }

        private void FilterComponents()
        {
            totalFilteredCount = 0;
            string filter = searchQuery.Trim().ToLower();

            foreach (var group in cachedCategories)
            {
                group.FilteredItems.Clear();
                foreach (var item in group.Items)
                {
                    if (string.IsNullOrEmpty(filter) || item.CleanName.ToLower().Contains(filter))
                    {
                        group.FilteredItems.Add(item);
                        totalFilteredCount++;
                    }
                }
            }
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
            float height = 48f; // Header

            // Search Bar Height
            height += 32f;

            if (totalComponentCount == 0)
            {
                return 180f; // Empty state height
            }

            foreach (var group in cachedCategories)
            {
                if (group.FilteredItems.Count == 0) continue;

                height += 28f; // Category Header
                
                CategoryCollapsedStates.TryGetValue(group.Name, out bool collapsed);
                if (!collapsed)
                {
                    height += group.FilteredItems.Count * 22f; // Items
                }
                height += 8f; // Spacing
            }

            // Interactive grid footer height
            height += 84f;

            return Mathf.Min(height, MaxHeight);
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
            
            // Header row with Name and Active Status
            DrawHeader();

            // Search Bar
            DrawSearchBar();

            if (totalComponentCount == 0)
            {
                DrawEmptyState();
            }
            else
            {
                // Component Categories List
                float contentHeight = position.height - 48f - 32f - 84f; // subtracting header, search, and grid footer
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(contentHeight));
                GUILayout.BeginVertical();
                GUILayout.Space(4);

                foreach (var group in cachedCategories)
                {
                    if (group.FilteredItems.Count == 0) continue;
                    DrawCategoryGroup(group);
                }

                GUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }

            // Interactive Grid Footer (Tag, Layer, Static, Active controls)
            DrawGridFooter();

            GUILayout.EndArea();
            GUI.color = guiColor;
        }

        private void DrawHeader()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 48f);
            
            // Draw lighter header background
            EditorGUI.DrawRect(rect, HeaderBgColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), BorderColor);

            // Icon box
            Texture2D goIcon = EditorGUIUtility.ObjectContent(targetGameObject, typeof(GameObject)).image as Texture2D;
            if (goIcon != null)
            {
                GUI.DrawTexture(new Rect(rect.x + 12f, rect.y + 14f, 20f, 20f), goIcon);
            }

            // Title label
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(rect.x + 38f, rect.y + 6f, 200f, 20f), targetGameObject.name, titleStyle);

            // Subtitle status (Static & Active labels)
            GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                richText = true,
                normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
            };
            string staticStr = targetGameObject.isStatic ? "<color=#5bc266>Static</color>" : "Static";
            string activeStr = targetGameObject.activeSelf ? "<color=#5bc266>Active</color>" : "<color=#e05e5e>Inactive</color>";
            GUI.Label(new Rect(rect.x + 38f, rect.y + 24f, 200f, 16f), $"{staticStr}  •  {activeStr}", subtitleStyle);

            // Total badge
            GUIStyle badgeStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                normal = { textColor = Color.white }
            };
            Rect badgeRect = new Rect(rect.xMax - 32f, rect.y + 16f, 20f, 17f);
            
            GUIStyle capsuleStyle = new GUIStyle();
            capsuleStyle.normal.background = GetOrCreateHeaderTexture();
            capsuleStyle.border = new RectOffset(6, 6, 6, 6);
            GUI.Box(badgeRect, "", capsuleStyle);
            
            GUI.Label(badgeRect, totalComponentCount.ToString(), badgeStyle);
        }

        private void DrawSearchBar()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 32f);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), DividerColor);

            GUIStyle searchStyle = new GUIStyle(EditorStyles.textField)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(12, 12, 6, 6)
            };

            GUILayout.BeginArea(new Rect(rect.x + 12f, rect.y + 5f, rect.width - 24f, 22f));
            EditorGUI.BeginChangeCheck();
            
            GUI.SetNextControlName("SearchField");
            searchQuery = EditorGUILayout.TextField("", searchQuery, searchStyle);
            
            if (EditorGUI.EndChangeCheck())
            {
                FilterComponents();
            }
            GUILayout.EndArea();

            // Search placeholder text
            if (string.IsNullOrEmpty(searchQuery))
            {
                GUIStyle placeholderStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontSize = 10,
                    normal = { textColor = new Color(0.45f, 0.45f, 0.45f) }
                };
                GUI.Label(new Rect(rect.x + 18f, rect.y + 7f, 200f, 18f), "🔍 Search components...", placeholderStyle);
            }
        }

        private void DrawCategoryGroup(CategoryGroup group)
        {
            CategoryCollapsedStates.TryGetValue(group.Name, out bool collapsed);

            Rect rect = GUILayoutUtility.GetRect(0f, 24f);

            // Hover row highlight
            if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.04f));
            }

            // Click header to toggle collapse
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && Event.current.button == 0)
            {
                collapsed = !collapsed;
                CategoryCollapsedStates[group.Name] = collapsed;
                Event.current.Use();
            }

            // Toggle arrow symbol
            string arrow = collapsed ? "▶" : "▼";
            GUIStyle arrowStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
            };
            GUI.Label(new Rect(rect.x + 8f, rect.y + 4f, 16f, 16f), arrow, arrowStyle);

            // Category title (with specific theme colored text)
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                normal = { textColor = group.TitleColor }
            };
            GUI.Label(new Rect(rect.x + 24f, rect.y + 4f, 150f, 16f), $"{group.IconStr}  {group.Name}", titleStyle);

            // Group count badge
            GUIStyle countStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
            };
            Rect badgeRect = new Rect(rect.xMax - 32f, rect.y + 4f, 18f, 15f);
            
            GUIStyle capsuleStyle = new GUIStyle();
            capsuleStyle.normal.background = GetOrCreateHeaderTexture();
            capsuleStyle.border = new RectOffset(6, 6, 6, 6);
            GUI.Box(badgeRect, "", capsuleStyle);
            GUI.Label(badgeRect, group.FilteredItems.Count.ToString(), countStyle);

            // Draw children items if expanded
            if (!collapsed)
            {
                foreach (var item in group.FilteredItems)
                {
                    DrawComponentRow(item);
                }
            }
            GUILayout.Space(6);
        }

        private void DrawComponentRow(CachedComponent item)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 22f);

            // Row highlight hover effect
            bool isHovered = rect.Contains(Event.current.mousePosition);
            if (Event.current.type == EventType.Repaint && isHovered)
            {
                EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.05f));
            }

            // Context Menu activation
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.button == 1) // Right Click
                {
                    ShowComponentContextMenu(item);
                    Event.current.Use();
                }
            }

            // Component Icon (Unity official icon)
            if (item.Icon != null)
            {
                GUI.DrawTexture(new Rect(rect.x + 28f, rect.y + 3f, 16f, 16f), item.Icon);
            }

            // Component Clean text
            GUIStyle itemStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                normal = { textColor = isHovered ? Color.white : new Color(0.85f, 0.85f, 0.85f) }
            };
            GUI.Label(new Rect(rect.x + 48f, rect.y + 3f, Width - 80f, 16f), item.CleanName, itemStyle);

            // Options Context Menu button on Hover
            if (isHovered)
            {
                Rect contextBtnRect = new Rect(rect.xMax - 26f, rect.y + 2f, 18f, 18f);
                GUIStyle contextStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 0, 0)
                };
                if (GUI.Button(contextBtnRect, "⋮", contextStyle))
                {
                    ShowComponentContextMenu(item);
                }
            }
        }

        private void ShowComponentContextMenu(CachedComponent item)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Ping GameObject"), false, () => EditorGUIUtility.PingObject(targetGameObject));
            menu.AddItem(new GUIContent("Highlight In Inspector"), false, () => {
                Selection.activeGameObject = targetGameObject;
                Highlighter.Highlight("Inspector", item.Component.GetType().Name);
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Copy Component Name"), false, () => EditorGUIUtility.systemCopyBuffer = item.CleanName);
            menu.AddItem(new GUIContent("Copy Component Type"), false, () => EditorGUIUtility.systemCopyBuffer = item.Component.GetType().FullName);
            menu.ShowAsContext();
        }

        private void DrawEmptyState()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(24f);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
            };
            GUIStyle bodyStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.45f, 0.45f, 0.45f) }
            };

            GUILayout.Label("📂  No Components Found", titleStyle);
            GUILayout.Label("This GameObject only contains a Transform component.", bodyStyle);

            GUILayout.EndVertical();
        }

        private void DrawGridFooter()
        {
            Rect rect = new Rect(0f, position.height - 84f, position.width, 84f);
            
            // Separator lines
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), BorderColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 1f, rect.width, rect.height - 1f), HeaderBgColor);

            // Draw grid dividers
            float midX = rect.x + rect.width * 0.5f;
            float midY = rect.y + 44f;
            EditorGUI.DrawRect(new Rect(midX, rect.y + 4f, 1f, 76f), DividerColor);
            EditorGUI.DrawRect(new Rect(rect.x + 8f, midY, rect.width - 16f, 1f), DividerColor);

            // Grid cell coordinates
            Rect cellTag = new Rect(rect.x + 12f, rect.y + 6f, midX - 20f, 32f);
            Rect cellLayer = new Rect(midX + 12f, rect.y + 6f, midX - 20f, 32f);
            Rect cellStatic = new Rect(rect.x + 12f, midY + 6f, midX - 20f, 32f);
            Rect cellActive = new Rect(midX + 12f, midY + 6f, midX - 20f, 32f);

            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                normal = { textColor = new Color(0.55f, 0.55f, 0.55f) }
            };

            // Cell 1: Tag Selector
            GUI.Label(new Rect(cellTag.x, cellTag.y, 80f, 12f), "🏷️ Tag", labelStyle);
            EditorGUI.BeginChangeCheck();
            string newTag = EditorGUI.TagField(new Rect(cellTag.x, cellTag.y + 12f, cellTag.width - 10f, 16f), targetGameObject.tag);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetGameObject, "Change Tag");
                targetGameObject.tag = newTag;
            }

            // Cell 2: Layer Selector
            GUI.Label(new Rect(cellLayer.x, cellLayer.y, 80f, 12f), "📍 Layer", labelStyle);
            EditorGUI.BeginChangeCheck();
            int newLayer = EditorGUI.LayerField(new Rect(cellLayer.x, cellLayer.y + 12f, cellLayer.width - 10f, 16f), targetGameObject.layer);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetGameObject, "Change Layer");
                targetGameObject.layer = newLayer;
            }

            // Cell 3: Static Selector
            GUI.Label(new Rect(cellStatic.x, cellStatic.y, 80f, 12f), "🌍 Static", labelStyle);
            EditorGUI.BeginChangeCheck();
            bool newStatic = EditorGUI.Toggle(new Rect(cellStatic.x, cellStatic.y + 12f, 16f, 16f), targetGameObject.isStatic);
            GUIStyle toggleLabelStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10 };
            GUI.Label(new Rect(cellStatic.x + 20f, cellStatic.y + 12f, 80f, 16f), newStatic ? "Yes" : "No", toggleLabelStyle);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetGameObject, "Change Static");
                targetGameObject.isStatic = newStatic;
            }

            // Cell 4: Active State Selector
            GUI.Label(new Rect(cellActive.x, cellActive.y, 80f, 12f), "⚡ Active", labelStyle);
            EditorGUI.BeginChangeCheck();
            bool newActive = EditorGUI.Toggle(new Rect(cellActive.x, cellActive.y + 12f, 16f, 16f), targetGameObject.activeSelf);
            GUI.Label(new Rect(cellActive.x + 20f, cellActive.y + 12f, 80f, 16f), newActive ? "Yes" : "No", toggleLabelStyle);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(targetGameObject, "Change Active");
                targetGameObject.SetActive(newActive);
            }
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
