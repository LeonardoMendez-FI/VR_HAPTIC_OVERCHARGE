#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HUDGeneratorTool : EditorWindow
{
    // ── Paletas de color ────────────────────────────────
    enum ColorPalette { Cyan, Purple, Amber, MilitaryGreen }
    ColorPalette currentPalette = ColorPalette.Cyan;

    Color primary, accent, textLight, danger, panelBg, visorEdge;

    // ── Layout ──────────────────────────────────────────
    Vector2 referenceRes = new Vector2(1920, 1080);
    int structSegments = 10;
    int energyCells = 8;
    float margin = 28f;
    float panelPadding = 12f;

    // ── Opciones ────────────────────────────────────────
    bool generateActors = true;
    bool createUICamera = true;
    bool saveAsPrefab = false;

    // ── Referencias a Managers ──────────────────────────
    EnergyManager energyManager;
    StructManager structManager;
    MoveManager moveManager;
    AttackManager attackManager;

    GameObject generatedRoot;

    const string MENU_PATH = "Tools/HUD System/Generate HUD";
    const string PREFAB_PATH = "Assets/HUDSystem/Prefabs";
    const string PREFAB_NAME = "RoboticHUD.prefab";

    [MenuItem(MENU_PATH)]
    public static void OpenWindow() => GetWindow<HUDGeneratorTool>("HUD Generator");

    void OnGUI()
    {
        GUILayout.Label("⬡ ROBOTIC HUD GENERATOR", EditorStyles.boldLabel);
        GUILayout.Space(6);

        DrawPaletteSelector();
        DrawLayoutSettings();
        DrawManagerReferences();
        DrawOptions();
        GUILayout.Space(12);
        if (GUILayout.Button("▶ Generate HUD Hierarchy", GUILayout.Height(36)))
            GenerateHUD();
        if (generatedRoot != null && GUILayout.Button("💾 Save as Prefab"))
            SaveAsPrefab();
    }

    void DrawPaletteSelector()
    {
        currentPalette = (ColorPalette)EditorGUILayout.EnumPopup("Color Palette", currentPalette);
        ApplyPalette();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ColorField("Primary", primary);
        EditorGUILayout.ColorField("Accent", accent);
        EditorGUILayout.ColorField("Text", textLight);
        EditorGUILayout.ColorField("Danger", danger);
        EditorGUILayout.ColorField("Panel BG", panelBg);
        EditorGUILayout.ColorField("Visor Edge", visorEdge);
        EditorGUI.EndDisabledGroup();
    }

    void ApplyPalette()
    {
        switch (currentPalette)
        {
            case ColorPalette.Cyan:
                primary = new Color(0, 0.92f, 1, 1);
                accent = new Color(0.1f, 0.45f, 1, 1);
                textLight = new Color(0.92f, 0.97f, 1, 1);
                danger = new Color(1, 0.12f, 0, 1);
                panelBg = new Color(0, 0.08f, 0.18f, 0.6f);
                visorEdge = new Color(0, 0.92f, 1, 0.18f);
                break;
            case ColorPalette.Purple:
                primary = new Color(0.6f, 0.4f, 1, 1);
                accent = new Color(0.8f, 0.6f, 1, 1);
                textLight = new Color(0.95f, 0.9f, 1, 1);
                danger = new Color(1, 0.3f, 0, 1);
                panelBg = new Color(0.08f, 0.04f, 0.2f, 0.6f);
                visorEdge = new Color(0.6f, 0.4f, 1, 0.2f);
                break;
            case ColorPalette.Amber:
                primary = new Color(1, 0.75f, 0, 1);
                accent = new Color(1, 0.5f, 0, 1);
                textLight = new Color(1, 0.95f, 0.8f, 1);
                danger = new Color(1, 0.2f, 0, 1);
                panelBg = new Color(0.15f, 0.1f, 0, 0.6f);
                visorEdge = new Color(1, 0.75f, 0, 0.2f);
                break;
            case ColorPalette.MilitaryGreen:
                primary = new Color(0.2f, 0.8f, 0.2f, 1);
                accent = new Color(0.4f, 0.9f, 0.4f, 1);
                textLight = new Color(0.9f, 1, 0.9f, 1);
                danger = new Color(1, 0.4f, 0, 1);
                panelBg = new Color(0.02f, 0.12f, 0.02f, 0.6f);
                visorEdge = new Color(0.2f, 0.8f, 0.2f, 0.18f);
                break;
        }
    }

    void DrawLayoutSettings()
    {
        GUILayout.Label("Layout", EditorStyles.boldLabel);
        referenceRes = EditorGUILayout.Vector2Field("Reference Resolution", referenceRes);
        structSegments = EditorGUILayout.IntSlider("Structure Segments", structSegments, 4, 20);
        energyCells = EditorGUILayout.IntSlider("Energy Cells", energyCells, 4, 16);
        margin = EditorGUILayout.Slider("Margin", margin, 8, 60);
        panelPadding = EditorGUILayout.Slider("Panel Padding", panelPadding, 4, 24);
    }

    void DrawManagerReferences()
    {
        GUILayout.Label("Managers (for auto-wiring actors)", EditorStyles.boldLabel);
        energyManager = EditorGUILayout.ObjectField("Energy Manager", energyManager, typeof(EnergyManager), true) as EnergyManager;
        structManager = EditorGUILayout.ObjectField("Struct Manager", structManager, typeof(StructManager), true) as StructManager;
        moveManager = EditorGUILayout.ObjectField("Move Manager", moveManager, typeof(MoveManager), true) as MoveManager;
        attackManager = EditorGUILayout.ObjectField("Attack Manager", attackManager, typeof(AttackManager), true) as AttackManager;
    }

    void DrawOptions()
    {
        generateActors = EditorGUILayout.Toggle("Generate HUD Actors", generateActors);
        createUICamera = EditorGUILayout.Toggle("Create UI Camera", createUICamera);
        saveAsPrefab = EditorGUILayout.Toggle("Save as Prefab after generation", saveAsPrefab);
    }

    void GenerateHUD()
    {
        var existing = GameObject.Find("HUDRoot");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Replace HUD?", "A HUDRoot already exists. Replace it?", "Replace", "Cancel"))
                return;
            DestroyImmediate(existing);
        }

        Undo.SetCurrentGroupName("Generate Robotic HUD");
        int undoGroup = Undo.GetCurrentGroup();

        var root = BuildCanvas();
        generatedRoot = root;

        if (createUICamera) BuildUICamera(root);

        var visorFrame = BuildVisorFrame(root);
        var topArea = BuildTopArea(root);
        var levelTitle = BuildLevelTitle(topArea);
        var counters = BuildCounters(topArea);
        var bottomLeft = BuildBottomLeft(root);
        var structBar = BuildStructureBar(bottomLeft);
        var moveMode = BuildMovementMode(bottomLeft);
        var bottomCenter = BuildBottomCenter(root);
        var energyCellsObj = BuildEnergyCells(bottomCenter);
        var blCorner = BuildTelemetryCorner(root, true);
        var brCorner = BuildTelemetryCorner(root, false);

        // HUDRoot
        var hudRoot = root.AddComponent<HUDRoot>();
        hudRoot.visorFrame = visorFrame?.GetComponent<VisorFrameUI>();
        hudRoot.structureBar = structBar?.GetComponent<StructureBarUI>();
        hudRoot.energyCells = energyCellsObj?.GetComponent<EnergyCellsUI>();
        hudRoot.movementMode = moveMode?.GetComponent<MovementModeUI>();
        hudRoot.joystickTelemetry = blCorner?.GetComponentInChildren<JoystickTelemetryUI>();
        hudRoot.levelTitle = levelTitle?.GetComponent<LevelTitleUI>();
        hudRoot.counters = counters?.GetComponent<CountersUI>();

        // Actors
        if (generateActors)
        {
            if (energyManager != null && energyCellsObj != null)
            {
                var a = energyCellsObj.AddComponent<EnergyHUDActor>();
                a.energyCells = energyCellsObj.GetComponent<EnergyCellsUI>();
                a.manager = energyManager;
            }
            if (structManager != null && structBar != null)
            {
                var a = structBar.AddComponent<StructureHUDActor>();
                a.structureBar = structBar.GetComponent<StructureBarUI>();
                a.manager = structManager;
            }
            if (moveManager != null && moveMode != null)
            {
                var a = moveMode.AddComponent<MovementModeHUDActor>();
                a.movementModeUI = moveMode.GetComponent<MovementModeUI>();
                a.manager = moveManager;
            }
            if (moveManager != null && blCorner != null)
            {
                var a = blCorner.AddComponent<JoystickTelemetryHUDActor>();
                a.telemetryUI = blCorner.GetComponentInChildren<JoystickTelemetryUI>();
                a.manager = moveManager;
            }
            if (attackManager != null && counters != null)
            {
                var a = counters.AddComponent<CountersHUDActor>();
                a.countersUI = counters.GetComponent<CountersUI>();
                a.manager = attackManager;
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = root;

        if (saveAsPrefab) SaveAsPrefab();
        Debug.Log("[HUDGenerator] HUD generated successfully.");
    }

    // ── Builders ──────────────────────────────────────────
    GameObject BuildCanvas()
    {
        var go = new GameObject("HUDRoot");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.sortingOrder = 10;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceRes;
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        SetLayerRecursive(go, LayerMask.NameToLayer("UI"));
        StretchFull(go);
        return go;
    }

    void BuildUICamera(GameObject root)
    {
        var cam = Camera.main;
        var go = new GameObject("UI Camera");
        go.transform.SetParent(cam != null ? cam.transform : null);
        var c = go.AddComponent<Camera>();
        c.clearFlags = CameraClearFlags.Depth;
        c.cullingMask = 1 << LayerMask.NameToLayer("UI");
        c.depth = 10;
        root.GetComponent<Canvas>().worldCamera = c;
    }

    GameObject BuildVisorFrame(GameObject parent)
    {
        var go = CreateChild(parent, "VisorFrame");
        StretchFull(go);
        var img = go.AddComponent<Image>();
        img.color = visorEdge;
        img.raycastTarget = false;
        go.AddComponent<VisorFrameUI>();
        return go;
    }

    GameObject BuildTopArea(GameObject parent)
    {
        var go = CreateChild(parent, "TopArea");
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, 80);
        return go;
    }

    GameObject BuildLevelTitle(GameObject parent)
    {
        var go = CreateChild(parent, "LevelTitle");
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.2f, 0);
        rt.anchorMax = new Vector2(0.8f, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.AddComponent<CanvasGroup>();

        var lineL = CreateChild(go, "AccentLineLeft");
        SetRect(lineL, new Vector2(0, 0.45f), new Vector2(0.05f, 0.55f));
        lineL.AddComponent<Image>().color = primary;

        var lineR = CreateChild(go, "AccentLineRight");
        SetRect(lineR, new Vector2(0.95f, 0.45f), new Vector2(1, 0.55f));
        lineR.AddComponent<Image>().color = primary;

        var titleGO = CreateChild(go, "TitleText");
        SetRect(titleGO, new Vector2(0.08f, 0.3f), new Vector2(0.92f, 1));
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "LEVEL 1 — ROBOTICS LABORATORY";
        titleTMP.fontSize = 22;
        titleTMP.color = textLight;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.UpperCase | FontStyles.Bold;

        var subGO = CreateChild(go, "SubtitleText");
        SetRect(subGO, new Vector2(0.08f, 0), new Vector2(0.92f, 0.35f));
        var subTMP = subGO.AddComponent<TextMeshProUGUI>();
        subTMP.text = "SECTOR 01 / ENTRY POINT";
        subTMP.fontSize = 11;
        subTMP.color = primary;
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.fontStyle = FontStyles.UpperCase;

        var comp = go.AddComponent<LevelTitleUI>();
        var so = new SerializedObject(comp);
        SetSerializedRef(so, "titleText", titleTMP);
        SetSerializedRef(so, "subtitleText", subTMP);
        SetSerializedRef(so, "accentLineLeft", lineL.GetComponent<Image>());
        SetSerializedRef(so, "accentLineRight", lineR.GetComponent<Image>());
        so.ApplyModifiedPropertiesWithoutUndo();
        return go;
    }

    GameObject BuildCounters(GameObject parent)
    {
        var go = CreateChild(parent, "Counters");
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 0.5f);
        rt.sizeDelta = new Vector2(160, 0);
        rt.anchoredPosition = new Vector2(-margin, 0);

        var (elimLbl, elimVal) = BuildCounterRow(go, "EliminationRow", "ELIMINATED", 0);
        var (objLbl, objVal) = BuildCounterRow(go, "ObjectiveRow", "REMAINING", -38);

        var comp = go.AddComponent<CountersUI>();
        var so = new SerializedObject(comp);
        SetSerializedRef(so, "eliminationLabel", elimLbl);
        SetSerializedRef(so, "eliminationCount", elimVal);
        SetSerializedRef(so, "objectiveLabel", objLbl);
        SetSerializedRef(so, "objectiveCount", objVal);
        so.ApplyModifiedPropertiesWithoutUndo();
        return go;
    }

    (TMP_Text, TMP_Text) BuildCounterRow(GameObject parent, string name, string label, float y)
    {
        var row = CreateChild(parent, name);
        var rt = row.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.sizeDelta = new Vector2(0, 32);
        rt.anchoredPosition = new Vector2(0, y);

        row.AddComponent<Image>().color = panelBg;

        var lblGO = CreateChild(row, "Label");
        SetRect(lblGO, new Vector2(0.04f, 0), new Vector2(0.55f, 1));
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text = label;
        lbl.fontSize = 9;
        lbl.color = primary;
        lbl.fontStyle = FontStyles.UpperCase;

        var valGO = CreateChild(row, "Value");
        SetRect(valGO, new Vector2(0.55f, 0), new Vector2(0.97f, 1));
        var val = valGO.AddComponent<TextMeshProUGUI>();
        val.text = "000";
        val.fontSize = 16;
        val.color = textLight;
        val.alignment = TextAlignmentOptions.Right;
        val.fontStyle = FontStyles.Bold;

        return (lbl, val);
    }

    GameObject BuildBottomLeft(GameObject parent)
    {
        var go = CreateChild(parent, "BottomLeft");
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;
        rt.sizeDelta = new Vector2(280, 130);
        rt.anchoredPosition = new Vector2(margin, margin);
        return go;
    }

    GameObject BuildStructureBar(GameObject parent)
    {
        var go = CreateChild(parent, "StructureBar");
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        go.AddComponent<Image>().color = panelBg;

        var lblGO = CreateChild(go, "StructureLabel");
        SetRect(lblGO, new Vector2(0.02f, 0.6f), new Vector2(0.7f, 1));
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text = "STRUCTURE";
        lbl.fontSize = 9;
        lbl.color = primary;
        lbl.fontStyle = FontStyles.UpperCase;

        var valGO = CreateChild(go, "StructureValue");
        SetRect(valGO, new Vector2(0.7f, 0.6f), new Vector2(0.98f, 1));
        var val = valGO.AddComponent<TextMeshProUGUI>();
        val.text = "100%";
        val.fontSize = 11;
        val.color = textLight;
        val.alignment = TextAlignmentOptions.Right;

        var segs = CreateChild(go, "Segments");
        SetRect(segs, new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.58f));
        var segList = new List<Image>();
        float w = 1f / structSegments;
        for (int i = 0; i < structSegments; i++)
        {
            var s = CreateChild(segs, $"Seg_{i:D2}");
            SetRect(s, new Vector2(i * w + 0.004f, 0), new Vector2((i + 1) * w - 0.004f, 1));
            var img = s.AddComponent<Image>();
            img.color = primary;
            segList.Add(img);
        }

        var comp = go.AddComponent<StructureBarUI>();
        var so = new SerializedObject(comp);
        SetSerializedRef(so, "labelText", lbl);
        SetSerializedRef(so, "valueText", val);
        var segProp = so.FindProperty("segments");
        segProp.arraySize = segList.Count;
        for (int i = 0; i < segList.Count; i++)
            segProp.GetArrayElementAtIndex(i).objectReferenceValue = segList[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        return go;
    }

    GameObject BuildMovementMode(GameObject parent)
    {
        var go = CreateChild(parent, "MovementMode");
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.offsetMin = new Vector2(0, panelPadding);
        rt.offsetMax = new Vector2(0, -panelPadding);

        go.AddComponent<Image>().color = panelBg;

        var lblGO = CreateChild(go, "ModeLabel");
        SetRect(lblGO, new Vector2(0.22f, 0.1f), new Vector2(0.95f, 0.9f));
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text = "WALK MODE";
        lbl.fontSize = 13;
        lbl.color = textLight;
        lbl.fontStyle = FontStyles.Bold | FontStyles.UpperCase;

        var iconGO = CreateChild(go, "ModeIcon");
        SetRect(iconGO, new Vector2(0.02f, 0.1f), new Vector2(0.2f, 0.9f));
        var icon = iconGO.AddComponent<Image>();
        icon.color = primary;
        icon.preserveAspect = true;

        var comp = go.AddComponent<MovementModeUI>();
        var so = new SerializedObject(comp);
        SetSerializedRef(so, "modeLabel", lbl);
        SetSerializedRef(so, "modeIcon", icon);
        SetSerializedRef(so, "backgroundPanel", go.GetComponent<Image>());
        so.ApplyModifiedPropertiesWithoutUndo();
        return go;
    }

    GameObject BuildBottomCenter(GameObject parent)
    {
        var go = CreateChild(parent, "BottomCenter");
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(320, 90);
        rt.anchoredPosition = new Vector2(0, margin);
        return go;
    }

    GameObject BuildEnergyCells(GameObject parent)
    {
        var go = CreateChild(parent, "EnergySystem");
        StretchFull(go);
        go.AddComponent<Image>().color = panelBg;

        var lblGO = CreateChild(go, "EnergyLabel");
        SetRect(lblGO, new Vector2(0.02f, 0.6f), new Vector2(0.65f, 1));
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text = "ENERGY";
        lbl.fontSize = 9;
        lbl.color = primary;
        lbl.fontStyle = FontStyles.UpperCase;

        var pctGO = CreateChild(go, "EnergyPercent");
        SetRect(pctGO, new Vector2(0.65f, 0.6f), new Vector2(0.98f, 1));
        var pct = pctGO.AddComponent<TextMeshProUGUI>();
        pct.text = "100%";
        pct.fontSize = 11;
        pct.color = textLight;
        pct.alignment = TextAlignmentOptions.Right;

        var cellsContainer = CreateChild(go, "Cells");
        SetRect(cellsContainer, new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.58f));
        var cellsList = new List<Image>();
        float w = 1f / energyCells;
        for (int i = 0; i < energyCells; i++)
        {
            var cell = CreateChild(cellsContainer, $"Cell_{i:D2}");
            SetRect(cell, new Vector2(i * w + 0.006f, 0), new Vector2((i + 1) * w - 0.006f, 1));
            var img = cell.AddComponent<Image>();
            img.color = primary;
            cellsList.Add(img);
        }

        var comp = go.AddComponent<EnergyCellsUI>();
        var so = new SerializedObject(comp);
        SetSerializedRef(so, "labelText", lbl);
        SetSerializedRef(so, "percentText", pct);
        var cellProp = so.FindProperty("cells");
        cellProp.arraySize = cellsList.Count;
        for (int i = 0; i < cellsList.Count; i++)
            cellProp.GetArrayElementAtIndex(i).objectReferenceValue = cellsList[i];
        so.ApplyModifiedPropertiesWithoutUndo();
        return go;
    }

    GameObject BuildTelemetryCorner(GameObject parent, bool isLeft)
    {
        string side = isLeft ? "Left" : "Right";
        float xAnchor = isLeft ? 0 : 1;
        float xPivot = isLeft ? 0 : 1;
        float xPos = isLeft ? margin + 150 : -(margin + 150);

        var go = CreateChild(parent, $"TelemetryCorner_{side}");
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xAnchor, 0);
        rt.anchorMax = new Vector2(xAnchor, 0);
        rt.pivot = new Vector2(xPivot, 0);
        rt.sizeDelta = new Vector2(100, 100);
        rt.anchoredPosition = new Vector2(xPos, margin);

        var ringGO = CreateChild(go, "OuterRing");
        StretchFull(ringGO);
        ringGO.AddComponent<Image>().color = new Color(primary.r, primary.g, primary.b, 0.45f);

        var crossH = CreateChild(go, "CrosshairH");
        SetRect(crossH, new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.52f));
        crossH.AddComponent<Image>().color = new Color(primary.r, primary.g, primary.b, 0.25f);

        var crossV = CreateChild(go, "CrosshairV");
        SetRect(crossV, new Vector2(0.48f, 0.1f), new Vector2(0.52f, 0.9f));
        crossV.AddComponent<Image>().color = new Color(primary.r, primary.g, primary.b, 0.25f);

        var dotGO = CreateChild(go, "InputDot");
        var dotRt = dotGO.GetComponent<RectTransform>();
        dotRt.anchorMin = dotRt.anchorMax = dotRt.pivot = new Vector2(0.5f, 0.5f);
        dotRt.sizeDelta = new Vector2(12, 12);
        dotRt.anchoredPosition = Vector2.zero;
        dotGO.AddComponent<Image>().color = primary;

        var lblGO = CreateChild(go, "AxisLabel");
        SetRect(lblGO, new Vector2(0, -0.3f), new Vector2(1, 0));
        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text = isLeft ? "L·INPUT" : "R·INPUT";
        lbl.fontSize = 8;
        lbl.color = primary;
        lbl.alignment = TextAlignmentOptions.Center;

        var valGO = CreateChild(go, "AxisValue");
        SetRect(valGO, new Vector2(0, -0.55f), new Vector2(1, -0.32f));
        var val = valGO.AddComponent<TextMeshProUGUI>();
        val.text = "(0.00, 0.00)";
        val.fontSize = 7;
        val.color = textLight;
        val.alignment = TextAlignmentOptions.Center;

        if (isLeft)
        {
            var comp = go.AddComponent<JoystickTelemetryUI>();
            var so = new SerializedObject(comp);
            SetSerializedRef(so, "outerRing", ringGO.GetComponent<Image>());
            SetSerializedRef(so, "innerDot", dotGO.GetComponent<Image>());
            SetSerializedRef(so, "crosshairH", crossH.GetComponent<Image>());
            SetSerializedRef(so, "crosshairV", crossV.GetComponent<Image>());
            SetSerializedRef(so, "axisLabel", lbl);
            SetSerializedRef(so, "axisValueText", val);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        return go;
    }

    // ── Helpers ────────────────────────────────────────────
    GameObject CreateChild(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    void StretchFull(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void SetSerializedRef(SerializedObject so, string propName, Object value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.objectReferenceValue = value;
    }

    void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    void SaveAsPrefab()
    {
        if (generatedRoot == null) return;
        if (!AssetDatabase.IsValidFolder(PREFAB_PATH))
        {
            string[] parts = PREFAB_PATH.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
        string path = $"{PREFAB_PATH}/{PREFAB_NAME}";
        PrefabUtility.SaveAsPrefabAssetAndConnect(generatedRoot, path, InteractionMode.UserAction);
        Debug.Log("Prefab saved to " + path);
    }
}
#endif