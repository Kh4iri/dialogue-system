# Dialogue System

A node based dialogue system using Unity's Graph Toolkit. This package allows you to create dialogue trees for your games, with support for variables and conditions.

Tested Unity version: **6000.3.9f1 (6.3 LTS)**

### Dependencies:

- Graph Toolkit (com.unity.graphtoolkit). Front-end interface for the node-based dialogue system.
- Input System (com.unity.inputsystem). Should be installed by default. Used in NarrativeBox dialogue preset UI.
- Serialization (com.unity.serialization). Used for serializing dialogue data.
- EditorAttributes ([Asset Store](https://assetstore.unity.com/packages/tools/gui/editorattributes-269285) | [Github](https://github.com/v0lt13/EditorAttributes)). Used for styling the inspector.
- Serialized Dictionary ([Asset Store](https://assetstore.unity.com/packages/tools/utilities/serialized-dictionary-243052) | [Github](https://github.com/ayellowpaper/SerializedDictionary)). Used for managing dialogue variables in the inspector.
- SerializeReferenceExtensions ([Github](https://github.com/mackysoft/Unity-SerializeReferenceExtensions)). Used for choosing variable types in the inspector.
- DOTween ([Asset Store](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)). Used for animating dialogue UI elements (only used in Narrative Box preset for now).

## Dialogue View Presets

### 1. Narrative Box

Story-focused layout, featuring large character portraits and stacked choice menus. Built entirely with UI Toolkit and integrated with the Input System.


