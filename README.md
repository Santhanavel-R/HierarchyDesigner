# Hierarchy Designer

Hierarchy Designer is a premium, feature-rich editor extension for Unity designed to visually organize, customize, and declutter your Hierarchy window. It provides tools to structure scenes, highlight key components, and navigate complex trees efficiently with zero impact on runtime performance.

---

## Complete Feature Directory

### 1. Hierarchy Group Headers
Create distinct functional sections directly inside the Unity Hierarchy window to organize large scenes.
* **Pre-configured Presets**: Includes templates for typical game components: `🎮 XR`, `🖥 UI`, `🔊 AUDIO`, `🌍 ENVIRONMENT`, `🎯 INTERACTABLES`, `✨ EFFECTS`, `⚙ Managers`, and `🐞 DEBUG`.
* **Custom Styling**: Define background colors, text layouts, and separator details individually for each header.
* **Line Separators**: Select from different line drawing styles (Solid, Dotted, Dashed, etc.) and custom colors to border headers.

### 2. Nesting Guide Lines & Rainbow Palettes
Trace parent-child connections in deeply nested hierarchy trees easily with guide lines.
* **Rainbow Nesting**: Automatically color-codes guide lines based on their tree depth.
* **Curated Themes**: Select from multiple preset rainbow palettes:
  * **Default**: Classic vibrant spectrum.
  * **Pastel**: Soft, professional, low-contrast HSL colors.
  * **Neon**: High-contrast, bright neon lines for dark editor layouts.
  * **Warm**: Sunset spectrum (Reds, Oranges, Yellows).
  * **Cool**: Oceanic spectrum (Blues, Greens, Cyans).
  * **Monochrome**: Slate and grayscale lines.
* **Nesting Guide Line Color**: Toggle off Rainbow nesting to apply a single static connection color.
* **Opacity Controls**: Customize connection line strength using a slider to perfectly match your Editor theme.

### 3. GameObject Row Borders
Draw visual outline borders around selected or hovered GameObjects to improve visibility.
* Render borders around the full width of hierarchy items.
* Custom color picker to select the outline color.
* Custom opacity slider to fade or sharpen the outlines.

### 4. Child Count Badges
Quickly see the count of nested children inside parent GameObjects at a glance.
* **Selectable Badge Styles**:
  * **Classic**: Gray brackets `[12]`
  * **Bubble**: Gray parentheses `(12)`
  * **Diamond**: Cyan diamond indicator `◆12`
  * **Hexagon**: Orange hexagon indicator `⬢12`
  * **Notification**: Blue notification dot `●12`
  * **Dot**: Red dot indicator `◉12`
  * **Tag**: Sleek green tag pointer box outline.
  * **None**: Bare number count `12`
* **Automated Color Nesting**: Badge numbers automatically inherit the nesting guide line color corresponding to the GameObject's depth, keeping a unified visual scheme.
* **Predefined Badge Colors**: Brackets, dots, tags, and symbols use premium, fixed colors matching standard IDE layouts.

### 5. Floating "Added Components" Hover Panel
Inspect components and metadata directly inside the Hierarchy without selecting GameObjects or opening the Inspector.
* **Instant Hover Trigger**: Appears instantly when hovering the cursor anywhere on a GameObject row.
* **Custom Scripts Box**: Lists all attached custom MonoBehaviours with the official C# script icon and readable PascalCase auto-splitting (e.g. `ModuleFlowController` -> `Module Flow Controller`).
* **Unity Built-in Row**: Displays native Unity components as a side-by-side row of official icons (`📷 🧊 🎨 📐 ⚡`).
* **Icon Tooltips**: Hovering any Unity component icon displays a tooltip indicating the component's name (e.g., `Camera`).
* **Grouped Duplicates**: Duplicate components are grouped together and marked with a multiplier badge (e.g. `x3`) to keep lists short.
* **Auto-Sizing Height & Width**: The panel width is locked to a compact `260f` and the height dynamically wraps around the contents.
* **Smart Dismiss**: Closes instantly when the mouse leaves the row bounds or the popup area.

### 6. Centralized Layout Database & Themes
Manage all your styles and visual parameters from a central place.
* Configured using a single `HierarchyDatabase` ScriptableObject.
* Includes pre-installed built-in themes and support for custom user-created editor themes.
* Clear database configurations with full **Reset** defaults.

### 7. Unified Editor Configuration Window
Manage all settings easily through a clean, unified config panel.
* Open the panel via **Tools > Hierarchy Designer**.
* Organizes configurations into **three clean bordered groups (helpBoxes)**:
  * **Separation Settings**: Toggle and customize separator lines.
  * **Feature Toggles**: Toggle and style Child Counts, Component Icons, and GameObject Row Borders.
  * **Nesting Settings**: Configure Nesting Lines, Rainbow Palettes, and opacity.

---

## Installation

### Via Git URL
1. Open Unity.
2. Open **Window > Package Manager**.
3. Click the **`+`** icon -> **Add package from git URL...**
4. Paste the URL:
   ```
   https://github.com/Santhanavel-R/HierarchyDesigner.git
   ```
5. Click **Add**.

### Via Local Disk (For Active Development)
1. Clone or download this repository locally.
2. Open Unity's **Package Manager**.
3. Click **`+` -> Add package from disk...**
4. Choose the [package.json](package.json) file in your local directory.
