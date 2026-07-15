# Hierarchy Designer

A visual hierarchy separator/header generator and renderer for Unity Editor. Perfect for breaking down complex scenes into structured, easily scannable sections without introducing runtime overhead or complex layouts.

## Features

- **Custom Section Headers**: Create visual dividers inside Unity's Hierarchy window.
- **Built-in Icon Selector**: Leverage Unity's editor icons library with live previews.
- **Harmony Color Palette Support**: Customize individual header backgrounds to fit different themes or categories.
- **Database Serialization**: Layout definitions are saved inside a lightweight ScriptableObject asset (`HierarchyLayout.asset`).
- **Complete Undo/Redo**: Creation, modification, reordering, and deletion support Unity's built-in `Undo` system.
- **Optimized Rendering**: Fast dictionary lookup caches ensure zero overhead and high framerates inside the editor hierarchy.

## Project Structure

```
HierarchyDesigner/
├── Runtime/
│   ├── HierarchyHeader.cs (MonoBehaviour component attached to GameObjects)
│   ├── HierarchyHeaderData.cs (Serializable data structure for header configuration)
│   └── HierarchyDatabase.cs (ScriptableObject representing the asset database)
├── Editor/
│   ├── HierarchyDesignerWindow.cs (Main ReorderableList GUI window interface)
│   ├── HierarchyDrawer.cs (Intercepts hierarchy rendering and draws custom headers)
│   ├── HierarchyCreator.cs (Handles GameObject lifecycle management inside the scene)
│   ├── HierarchyStyles.cs (Style tokens, alignments, margins, and sizes)
│   ├── IconUtility.cs (Built-in Unity editor icon resolution helpers)
│   ├── HierarchyUtility.cs (General workspace helpers)
│   └── HierarchyDatabaseEditor.cs (Custom inspector editor for the ScriptableObject database)
└── Resources/
    └── README.md (This user guide)
```

## Quick Start

1. Open the tool from **Tools > Hierarchy Designer** in the top menu bar.
2. Click **+ Add Section** to create a new header entry.
3. Configure:
   - **Header Name**: Visual label (e.g. `🎮 XR`, `🌍 Environment`, `🖥 UI`).
   - **Icon**: Dropdown selector showing a list of built-in Unity editor icons.
   - **Color**: Click the color wheel to set the background strip color.
4. Use **Move Up** / **Move Down** to modify the relative sorting order of the section headers.
5. Click:
   - **Create Headers** to spawn the visual header GameObjects in your active scene.
   - **Update Headers** to sync scene GameObject names and ordering changes.
   - **Delete Headers** to safely remove generated headers from the active scene.
