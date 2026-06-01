using UnityEngine;
using UnityEngine.UI;

public class HUDRoot : MonoBehaviour
{
    public Camera uiCamera;
    public Canvas canvas;
    public CanvasScaler canvasScaler;
    public GraphicRaycaster graphicRaycaster;
    public RectTransform hudRect;
    public RectTransform topArea;
    public RectTransform bottomLeft;
    public RectTransform bottomCenter;
    public RectTransform bottomRightCorner;
    public RectTransform bottomLeftCorner;

    public VisorFrameUI visorFrame;
    public StructureBarUI structureBar;
    public EnergyCellsUI energyCells;
    public MovementModeUI movementMode;
    public JoystickTelemetryUI joystickTelemetry;
    public LevelTitleUI levelTitle;
    public CountersUI counters;
}