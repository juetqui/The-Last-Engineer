#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PrefabPainterWindow : EditorWindow
{
    [MenuItem("Tools/Prefab Painter")]
    public static void ShowWindow()
    {
        var w = GetWindow<PrefabPainterWindow>("Prefab Painter");
        w.minSize = new Vector2(420, 360);
    }

    private PrefabPainterSceneTool _tool;
    private PrefabPainterPalette _palette;
    private ListView _listView;
    private VisualElement _previewArea;
    private ObjectField _paletteField;
    private Slider _gridSlider;
    private EnumField _snapEnum;
    private Toggle _alignToggle, _randomYawToggle, _livePreviewToggle;
    private FloatField _yOffsetField;

    private void OnEnable()
    {
        _tool = new PrefabPainterSceneTool();
        SceneView.duringSceneGui += _tool.DuringSceneGUI;


        // UI Toolkit
        var root = rootVisualElement;
        root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/PrefabPainter/PrefabPainter.uss"));
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/PrefabPainter/PrefabPainter.uxml");
        uxml.CloneTree(root);

        _paletteField = root.Q<ObjectField>("paletteField");
        _listView = root.Q<ListView>("prefabList");
        _previewArea = root.Q<VisualElement>("previewArea");
        _gridSlider = root.Q<Slider>("gridSlider");
        _snapEnum = root.Q<EnumField>("snapEnum");
        _alignToggle = root.Q<Toggle>("alignToggle");
        _randomYawToggle = root.Q<Toggle>("randomYawToggle");
        _livePreviewToggle = root.Q<Toggle>("livePreviewToggle");
        _yOffsetField = root.Q<FloatField>("yOffsetField");


        _paletteField.objectType = typeof(PrefabPainterPalette);
        _paletteField.RegisterValueChangedCallback(evt => LoadPalette(evt.newValue as PrefabPainterPalette));


        root.Q<Button>("newPaletteBtn").clicked += CreatePalette;
        root.Q<Button>("clearLastBtn").clicked += () => _tool.ClearLastPlaced();


        _gridSlider.RegisterValueChangedCallback(_ => PushSettings());
        _snapEnum.RegisterValueChangedCallback(_ => PushSettings());
        _alignToggle.RegisterValueChangedCallback(_ => PushSettings());
        _randomYawToggle.RegisterValueChangedCallback(_ => PushSettings());
        _livePreviewToggle.RegisterValueChangedCallback(_ => PushSettings());
        _yOffsetField.RegisterValueChangedCallback(_ => PushSettings());


        BuildList(null);
        PushSettings();
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= _tool.DuringSceneGUI;
        _tool?.Dispose();
    }


    private void LoadPalette(PrefabPainterPalette p)
    {
        _palette = p;
        BuildList(_palette);
    }


    private void CreatePalette()
    {
        var path = EditorUtility.SaveFilePanelInProject("Create Palette", "NewPrefabPalette", "asset", "Choose location");
        if (string.IsNullOrEmpty(path)) return;
        var p = CreateInstance<PrefabPainterPalette>();
        AssetDatabase.CreateAsset(p, path);
        AssetDatabase.SaveAssets();
        _paletteField.value = p;
    }

    private void BuildList(PrefabPainterPalette palette)
    {
        // Datasource
        List<GameObject> data = palette ? palette.prefabs : new List<GameObject>();

        // ListView setup
        _listView.makeItem = () =>
        {
            var row = new VisualElement();
            row.AddToClassList("prefab-row");
            var img = new Image { scaleMode = ScaleMode.ScaleToFit };
            img.AddToClassList("prefab-thumb");
            var lbl = new Label();
            lbl.AddToClassList("prefab-label");
            row.Add(img);
            row.Add(lbl);
            return row;
        };

        _listView.bindItem = (elem, i) =>
        {
            var go = data[i];
            var img = elem.Q<Image>();
            var lbl = elem.Q<Label>();
            lbl.text = go ? go.name : "<none>";
            img.image = go ? AssetPreview.GetAssetPreview(go) ?? AssetPreview.GetMiniThumbnail(go) : null;
        };

        _listView.itemsSource = data;
        _listView.selectionType = SelectionType.Single;
        _listView.selectionChanged += objs =>
        {
            foreach (var o in objs)
            {
                var go = o as GameObject;
                _tool.SetCurrentPrefab(go);
                UpdatePreview(go);
                break;
            }
        };

        // Permitir drag & drop desde Project a la lista
        _listView.RegisterCallback<DragUpdatedEvent>(evt =>
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.StopPropagation();
        });
        _listView.RegisterCallback<DragPerformEvent>(evt =>
        {
            if (_palette == null)
            {
                EditorUtility.DisplayDialog("Prefab Painter", "Creá o asigná una Palette primero.", "OK");
                return;
            }
            foreach (var obj in DragAndDrop.objectReferences)
            {
                var go = obj as GameObject;
                if (go && PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab)
                    _palette.prefabs.Add(go);
            }
            EditorUtility.SetDirty(_palette);
            _listView.Rebuild();
        });
    }


    private void UpdatePreview(GameObject go)
    {
        _previewArea.Clear();
        if (!go) return;
        var img = new Image { image = AssetPreview.GetAssetPreview(go) ?? AssetPreview.GetMiniThumbnail(go) };
        img.AddToClassList("big-preview");
        _previewArea.Add(img);
    }


    private void PushSettings()
    {
        var s = new PrefabPainterSceneTool.Settings
        {
            gridSize = _gridSlider?.value ?? 1f,
            snapMode = _snapEnum != null ? (PrefabPainterSceneTool.SnapMode)_snapEnum.value : PrefabPainterSceneTool.SnapMode.Grid,
            alignToSurfaceNormal = _alignToggle?.value ?? false,
            randomYaw = _randomYawToggle?.value ?? false,
            yOffset = _yOffsetField?.value ?? 0f,
            livePreview = _livePreviewToggle?.value ?? true,
        };
        _tool?.SetSettings(s);
    }
}
#endif