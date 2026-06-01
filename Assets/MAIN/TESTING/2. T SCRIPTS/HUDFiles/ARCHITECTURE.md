# ⬡ ROBOTIC HUD SYSTEM — COMPLETE ARCHITECTURE REFERENCE
**Sci-Fi First-Person Robot Game · Unity 2022.3+**

---

## 📁 FILE STRUCTURE

```
Assets/
└── HUDSystem/
    ├── Editor/
    │   └── HUDGeneratorTool.cs        ← Run this first
    │
    ├── Runtime/
    │   ├── Core/
    │   │   ├── UIManager.cs           ← Event bridge (no gameplay)
    │   │   └── HUDRoot.cs             ← Reference container
    │   │
    │   ├── Elements/
    │   │   ├── VisorFrameUI.cs        ← Holographic border
    │   │   ├── StructureBarUI.cs      ← Segmented health bar
    │   │   ├── EnergyCellsUI.cs       ← Energy cell array
    │   │   ├── MovementModeUI.cs      ← Flight/Walk indicator
    │   │   ├── JoystickTelemetryUI.cs ← Debug input widget
    │   │   └── LevelCounterUI.cs      ← Title + Counters
    │   │
    │   └── Events/
    │       └── HUDEvents.cs           ← Typed UnityEvent definitions
    │
    ├── Shaders/
    │   └── HolographicVisor.shader    ← Custom holographic UI shader
    │
    └── Prefabs/
        └── RoboticHUD.prefab          ← Auto-saved by generator
```

---

## 🎮 GENERATED HIERARCHY

```
HUDRoot [Canvas · Screen Space Camera]
│   ├── Canvas Scaler (Scale With Screen Size · 1920×1080)
│   ├── Graphic Raycaster
│   ├── HUDRoot (component)
│   └── UIManager (component)
│
├── VisorFrame          [Image + VisorFrameUI]
│
├── TopArea             [RectTransform]
│   ├── LevelTitle      [CanvasGroup + LevelTitleUI]
│   │   ├── AccentLineLeft   [Image]
│   │   ├── AccentLineRight  [Image]
│   │   ├── TitleText        [TMP_Text]
│   │   └── SubtitleText     [TMP_Text]
│   │
│   └── Counters        [RectTransform + CountersUI]
│       ├── EliminationRow
│       │   ├── Label   [TMP_Text]
│       │   └── Value   [TMP_Text]
│       └── ObjectiveRow
│           ├── Label   [TMP_Text]
│           └── Value   [TMP_Text]
│
├── BottomLeft          [RectTransform]
│   ├── StructureBar    [Image + StructureBarUI]
│   │   ├── StructureLabel   [TMP_Text]
│   │   ├── StructureValue   [TMP_Text]
│   │   └── Segments/
│   │       ├── Seg_00  [Image]  ← cyan → orange → red
│   │       ├── Seg_01  [Image]
│   │       └── ...     (configurable 4–20)
│   │
│   └── MovementMode    [Image + MovementModeUI]
│       ├── ModeIcon    [Image]
│       └── ModeLabel   [TMP_Text]
│
├── BottomCenter        [RectTransform]
│   └── EnergySystem    [Image + EnergyCellsUI]
│       ├── EnergyLabel   [TMP_Text]
│       ├── EnergyPercent [TMP_Text]
│       └── Cells/
│           ├── Cell_00 [Image]  ← ripple glow animation
│           ├── Cell_01 [Image]
│           └── ...     (configurable 4–16)
│
├── TelemetryCorner_Left   [RectTransform + JoystickTelemetryUI]
│   ├── OuterRing       [Image · circular border]
│   ├── CrosshairH      [Image]
│   ├── CrosshairV      [Image]
│   ├── InputDot        [Image · moves with input]
│   ├── AxisLabel       [TMP_Text · "L·INPUT"]
│   └── AxisValue       [TMP_Text · "(x, y)"]
│
└── TelemetryCorner_Right  [RectTransform]
    └── (mirror of Left, no script — wire separately if needed)
```

---

## ⚡ EVENT INTEGRATION — HOW TO CONNECT YOUR MANAGERS

### Option A — Wire events in Inspector (recommended)

In your `EnergyManager.cs`, expose:
```csharp
public FloatEvent OnEnergyChanged;   // broadcast 0-1 every frame
```

Drag `EnergyManager` into `UIManager → On Energy Changed` slot in Inspector.

Done. Zero coupling.

---

### Option B — Use HUDEvents singleton bus

```csharp
// In EnergyManager.Update():
HUDEvents.Instance.OnEnergyChanged.Invoke(currentEnergy / maxEnergy);
```

In the Inspector, drag your `HUDEvents` GameObject into
`UIManager → Hud Events Bus`.

---

### Option C — Direct reference (simplest)

```csharp
// Anywhere in code:
FindObjectOfType<UIManager>().UpdateEnergy(0.65f);
FindObjectOfType<UIManager>().UpdateStructure(0.80f);
```

---

## 🎬 ANIMATION ARCHITECTURE

### Runtime Animations (code-driven, no Animator overhead)

| Element | Animation | Technique |
|---|---|---|
| VisorFrame | Pulse glow | `Mathf.Sin` alpha modulation |
| VisorFrame | Scanlines | Shader `_ScanlineOffset` float |
| VisorFrame | Danger flash | Color lerp red on `SetCriticalWarning(true)` |
| EnergyCells | Ripple glow | Per-cell staggered `Mathf.Sin` |
| MovementMode | Mode switch | Coroutine flicker (alpha on/off) |
| LevelTitle | Scan reveal | Coroutine: fade in → hold → fade to persist |
| StructureBar | Hit flash | Brief color override + timer |
| CountersUI | Count pulse | Brief color to orange on change |

### Optional Animator-based (for cinematic events only)

Use Unity's `Animator` component **only** for:
- Level start intro sweep
- Death screen HUD disintegration
- Boss encounter HUD transformation

Keep frame-to-frame updates **code-driven** (no Animator for live values).

---

## 🔮 SHADER SETUP

### Assigning the Holographic Shader

1. Create a Material: `Assets/Materials/HolographicVisor.mat`
2. Set shader to `RoboticHUD/HolographicVisor`
3. Configure:
   ```
   Color:             (0, 0.92, 1, 0.18)   ← thin cyan tint
   Glow Color:        (0, 0.92, 1, 1.0)
   Glow Intensity:    1.2
   Edge Fresnel Power:2.5
   Scanline Density:  240
   ```
4. Assign to `VisorFrame → Image → Material`

### Bloom (Post-Processing)

Using URP Post-Processing:
```
Volume → Bloom
  Threshold:  0.85
  Intensity:  0.6
  Scatter:    0.7
  Tint:       (0, 0.92, 1)   ← matches HUD cyan
```

The cyan HUD elements naturally bloom if their alpha is near 1
and your post-processing stack is configured correctly.

---

## 📱 VR FUTURE COMPATIBILITY

The Canvas is generated with:
- `renderMode = ScreenSpaceCamera` with dedicated UI Camera
- All elements on the `UI` layer

**To migrate to VR:**
1. Change `canvas.renderMode = RenderMode.WorldSpace`
2. Set `HUDRoot.vrCameraOverride` to your VR camera
3. Position the Canvas at ~2m in front of the player
4. The `JoystickTelemetryUI` (debug widgets) can be hidden in VR

---

## 🔧 INSPECTOR ORGANIZATION TIPS

### HUDRoot
- All element references auto-populated by generator
- Use this as your single source of truth for HUD refs

### UIManager
- Wire `OnEnergyChanged`, `OnStructureChanged` from your Managers
- OR assign `Hud Events Bus` for bus-style architecture

### Per-Element Scripts
Each element has `[Header]`-grouped Inspector sections:
- `─── Components` → drag refs
- `─── Colors`     → palette overrides
- `─── Settings`   → timing, thresholds
- `[ContextMenu]`  → right-click preview modes

---

## ✅ QUICK START CHECKLIST

- [ ] Import TextMeshPro (Window → TextMeshPro → Import TMP Essentials)
- [ ] Copy all `.cs` files into your Unity project preserving folder structure
- [ ] Add `RoboticHUD.Elements` and `RoboticHUD.Core` assembly definitions (optional)
- [ ] Open `Tools > HUD System > Generate HUD`
- [ ] Configure colors/layout if needed, press **Generate HUD Hierarchy**
- [ ] Assign the `HolographicVisor.shader` material to the VisorFrame
- [ ] Wire UIManager events from your game managers in Inspector
- [ ] Set up URP Bloom post-processing with cyan tint
- [ ] (Optional) Press **Save as Prefab** in the generator window
- [ ] Call `UIManager.ShowLevelTitle("LEVEL 1 — ROBOTICS LABORATORY")` on scene load

---

## 🏷 NAMING CONVENTIONS

| Prefix | Meaning |
|---|---|
| `*UI.cs` | HUD element MonoBehaviour |
| `UIManager.cs` | Event bridge only, no gameplay |
| `HUDEvents.cs` | Event type definitions |
| `HUDRoot.cs` | Reference container |
| `HUDGenerator*` | Editor-only |
