using UnityEngine;
using UnityEngine.Events;

public class InputLogic : MonoBehaviour
{
    [Header("Haptic Service (opcional)")]
    public HapticService hapticService;

    [Header("Permissions")]
    public PlayerPermissions permissions;

    [Header("Detección de doble pulsación")]
    public float doubleTapThreshold = 0.3f;

    [Header("Eventos de cambio de modo")]
    public UnityEvent OnFlightRequested;
    public UnityEvent OnLandRequested;

    [Header("Teclas (fallback sin Haptic)")]
    public KeyCode forwardKey1 = KeyCode.W;
    public KeyCode backwardKey1 = KeyCode.S;
    public KeyCode leftKey1 = KeyCode.A;
    public KeyCode rightKey1 = KeyCode.D;
    public KeyCode forwardKey2 = KeyCode.UpArrow;
    public KeyCode backwardKey2 = KeyCode.DownArrow;
    public KeyCode leftKey2 = KeyCode.LeftArrow;
    public KeyCode rightKey2 = KeyCode.RightArrow;
    public KeyCode ascendKey = KeyCode.Return;
    public KeyCode descendKey = KeyCode.Tab;
    public KeyCode jumpKey = KeyCode.Space;

    [HideInInspector] public Vector2 LeftJoystickRaw;
    [HideInInspector] public Vector2 RightJoystickRaw;
    [HideInInspector] public bool LeftButtonHeld;
    [HideInInspector] public bool RightButtonHeld;
    [HideInInspector] public bool JumpPressed;

    [HideInInspector] public bool W, A, S, D;
    [HideInInspector] public bool UpArrow, DownArrow, LeftArrow, RightArrow;

    private bool prevLeftButton = false;
    private bool prevRightButton = false;
    private bool prevLeftForTap = false;
    private bool prevRightForTap = false;

    private float lastFlightTapTime = -10f;
    private float lastLandTapTime = -10f;

    void Update()
    {
        if (hapticService != null && hapticService.IsConnected)
            ReadHapticInput();
        else
            ReadKeyboardInput();

        DetectDoubleTaps();

        prevRightButton = RightButtonHeld;
        prevLeftButton = LeftButtonHeld;
    }

    void ReadHapticInput()
    {
        LeftJoystickRaw  = hapticService.LeftJoystick;
        RightJoystickRaw = hapticService.RightJoystick;

        W = LeftJoystickRaw.y > 0.3f;
        S = LeftJoystickRaw.y < -0.3f;
        A = LeftJoystickRaw.x < -0.3f;
        D = LeftJoystickRaw.x > 0.3f;
        UpArrow    = RightJoystickRaw.y > 0.3f;
        DownArrow  = RightJoystickRaw.y < -0.3f;
        LeftArrow  = RightJoystickRaw.x < -0.3f;
        RightArrow = RightJoystickRaw.x > 0.3f;

        LeftButtonHeld  = hapticService.LeftButton;
        RightButtonHeld = hapticService.RightButton;

        JumpPressed = RightButtonHeld && !prevRightButton;
    }

    void ReadKeyboardInput()
    {
        W = Input.GetKey(forwardKey1);
        S = Input.GetKey(backwardKey1);
        A = Input.GetKey(leftKey1);
        D = Input.GetKey(rightKey1);
        UpArrow    = Input.GetKey(forwardKey2);
        DownArrow  = Input.GetKey(backwardKey2);
        LeftArrow  = Input.GetKey(leftKey2);
        RightArrow = Input.GetKey(rightKey2);

        LeftJoystickRaw  = new Vector2((D ? 1 : 0) - (A ? 1 : 0), (W ? 1 : 0) - (S ? 1 : 0));
        RightJoystickRaw = new Vector2((RightArrow ? 1 : 0) - (LeftArrow ? 1 : 0),
                                       (UpArrow ? 1 : 0) - (DownArrow ? 1 : 0));

        LeftButtonHeld  = Input.GetKey(descendKey);
        RightButtonHeld = Input.GetKey(ascendKey) || Input.GetKey(jumpKey);
        JumpPressed      = Input.GetKeyDown(jumpKey);
    }

    void DetectDoubleTaps()
    {
        if (hapticService != null && hapticService.IsConnected)
        {
            bool rightEdge = RightButtonHeld && !prevRightForTap;
            bool leftEdge = LeftButtonHeld && !prevLeftForTap;

            if (rightEdge)
            {
                if (Time.time - lastFlightTapTime <= doubleTapThreshold)
                {
                    if (permissions == null || permissions.canToggleFlight)
                        OnFlightRequested?.Invoke();
                    lastFlightTapTime = -10f;
                }
                else
                    lastFlightTapTime = Time.time;
            }

            if (leftEdge)
            {
                if (Time.time - lastLandTapTime <= doubleTapThreshold)
                {
                    if (permissions == null || permissions.canLand)
                        OnLandRequested?.Invoke();
                    lastLandTapTime = -10f;
                }
                else
                    lastLandTapTime = Time.time;
            }

            prevRightForTap = RightButtonHeld;
            prevLeftForTap = LeftButtonHeld;
        }
        else
        {
            if (Input.GetKeyDown(ascendKey))
            {
                if (Time.time - lastFlightTapTime <= doubleTapThreshold)
                {
                    if (permissions == null || permissions.canToggleFlight)
                        OnFlightRequested?.Invoke();
                    lastFlightTapTime = -10f;
                }
                else
                    lastFlightTapTime = Time.time;
            }

            if (Input.GetKeyDown(descendKey))
            {
                if (Time.time - lastLandTapTime <= doubleTapThreshold)
                {
                    if (permissions == null || permissions.canLand)
                        OnLandRequested?.Invoke();
                    lastLandTapTime = -10f;
                }
                else
                    lastLandTapTime = Time.time;
            }
        }
    }
}