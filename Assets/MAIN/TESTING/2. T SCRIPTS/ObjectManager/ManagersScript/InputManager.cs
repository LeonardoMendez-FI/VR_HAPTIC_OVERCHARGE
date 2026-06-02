using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    [Header("Haptic Manager (opcional)")]
    public HapticManager hapticManager;

    [Header("Move Manager (para conocer modo vuelo/suelo)")]
    public MoveManager moveManager;

    [Header("Detección de doble pulsación")]
    public float doubleTapThreshold = 0.3f;

    [Header("Eventos de cambio de modo")]
    public UnityEvent OnFlightRequested;
    public UnityEvent OnLandRequested;

    [Header("Teclas de movimiento (fallback sin Haptic)")]
    public KeyCode forwardKey1 = KeyCode.W;        // Left  joystick Y+
    public KeyCode backwardKey1 = KeyCode.S;       // Left  joystick Y-
    public KeyCode leftKey1 = KeyCode.A;           // Left  joystick X-
    public KeyCode rightKey1 = KeyCode.D;          // Left  joystick X+
    public KeyCode forwardKey2 = KeyCode.UpArrow;  // Right joystick Y+
    public KeyCode backwardKey2 = KeyCode.DownArrow;//Right joystick Y-
    public KeyCode leftKey2 = KeyCode.LeftArrow;   // Right joystick X-
    public KeyCode rightKey2 = KeyCode.RightArrow; // Right joystick X+
    public KeyCode ascendKey = KeyCode.Return;     // tecla sin ghosting
    public KeyCode descendKey = KeyCode.Tab;
    public KeyCode jumpKey = KeyCode.Space;        // solo suelo
    public KeyCode rotateLeftKey = KeyCode.A;      // suelo: rotación (igual a leftKey1)
    public KeyCode rotateRightKey = KeyCode.D;     // suelo: rotación (igual a rightKey1)

    // ── Salidas para actores ────────────────────────────────
    [HideInInspector] public Vector2 LeftJoystickRaw;   // vector crudo del joystick izquierdo
    [HideInInspector] public Vector2 RightJoystickRaw;  // vector crudo del joystick derecho
    [HideInInspector] public Vector2 MoveInput;         // movimiento (RightJoystick en suelo, LeftJoystick en vuelo? – se ajusta según modo)
    [HideInInspector] public float GroundRotInput;      // rotación en suelo (LeftJoystick.x)
    [HideInInspector] public float YawInput;            // rotación para vuelo (obsoleta, mantenida por compatibilidad)
    [HideInInspector] public float AscendInput;
    [HideInInspector] public bool JumpPressed;

    // ── Estados de botones (unificados) ─────────────────────
    [HideInInspector] public bool LeftButtonHeld;
    [HideInInspector] public bool RightButtonHeld;

    // ── Teclas individuales para el sistema de propulsión ───
    [HideInInspector] public bool W, A, S, D;
    [HideInInspector] public bool UpArrow, DownArrow, LeftArrow, RightArrow;

    // ── Estados internos ────────────────────────────────────
    private float lastFlightTapTime = -10f;
    private float lastLandTapTime = -10f;
    private bool prevLeftButton;
    private bool prevRightButton;

    void Update()
    {
        if (hapticManager != null && hapticManager.IsConnected)
            ReadHapticInput();
        else
            ReadKeyboardInput();

        DetectDoubleTaps();
    }

    void ReadHapticInput()
    {
        LeftJoystickRaw  = hapticManager.LeftJoystick;
        RightJoystickRaw = hapticManager.RightJoystick;

        // Mapear a teclas individuales para propulsión
        W = LeftJoystickRaw.y > 0.3f;
        S = LeftJoystickRaw.y < -0.3f;
        A = LeftJoystickRaw.x < -0.3f;
        D = LeftJoystickRaw.x > 0.3f;
        UpArrow    = RightJoystickRaw.y > 0.3f;
        DownArrow  = RightJoystickRaw.y < -0.3f;
        LeftArrow  = RightJoystickRaw.x < -0.3f;
        RightArrow = RightJoystickRaw.x > 0.3f;

        // Vectores procesados según modo
        bool isFlying = moveManager != null && moveManager.isFlying;
        if (isFlying)
        {
            MoveInput = LeftJoystickRaw;   // en vuelo ambos joysticks se usan directamente en propulsión
            GroundRotInput = 0f;
            YawInput = LeftJoystickRaw.x;   // por si alguien lo lee
        }
        else
        {
            MoveInput = RightJoystickRaw;   // suelo: movimiento con joystick derecho
            GroundRotInput = LeftJoystickRaw.x;  // rotación con eje X del izquierdo
            YawInput = 0f;
        }

        LeftButtonHeld  = hapticManager.LeftButton;
        RightButtonHeld = hapticManager.RightButton;

        bool rightDown = hapticManager.RightButton;
        bool leftDown  = hapticManager.LeftButton;

        if (isFlying)
        {
            AscendInput = (rightDown ? 1f : 0f) + (leftDown ? -1f : 0f);
            JumpPressed = false;
        }
        else
        {
            JumpPressed = rightDown && !prevRightButton;
            AscendInput = 0f;
        }

        prevLeftButton  = leftDown;
        prevRightButton = rightDown;
    }

    void ReadKeyboardInput()
    {
        // --- Joystick izquierdo (WASD) ---
        bool w = Input.GetKey(forwardKey1);
        bool s = Input.GetKey(backwardKey1);
        bool a = Input.GetKey(leftKey1);
        bool d = Input.GetKey(rightKey1);
        float lx = (d ? 1f : 0f) - (a ? 1f : 0f);
        float ly = (w ? 1f : 0f) - (s ? 1f : 0f);
        LeftJoystickRaw = new Vector2(lx, ly);

        // --- Joystick derecho (Flechas) ---
        bool up    = Input.GetKey(forwardKey2);
        bool down  = Input.GetKey(backwardKey2);
        bool left  = Input.GetKey(leftKey2);
        bool right = Input.GetKey(rightKey2);
        float rx = (right ? 1f : 0f) - (left ? 1f : 0f);
        float ry = (up ? 1f : 0f) - (down ? 1f : 0f);
        RightJoystickRaw = new Vector2(rx, ry);

        // Teclas individuales para propulsión (lectura directa)
        W = w; A = a; S = s; D = d;
        UpArrow = up; DownArrow = down; LeftArrow = left; RightArrow = right;

        bool isFlying = moveManager != null && moveManager.isFlying;
        if (isFlying)
        {
            MoveInput = LeftJoystickRaw;   // en vuelo el izquierdo también mueve
            GroundRotInput = 0f;
            YawInput = 0f;
        }
        else
        {
            MoveInput = RightJoystickRaw;   // suelo: movimiento con derecho
            GroundRotInput = LeftJoystickRaw.x; // rotación con eje X del izquierdo
            YawInput = 0f;
        }

        // Botones
        LeftButtonHeld  = Input.GetKey(descendKey);
        RightButtonHeld = Input.GetKey(ascendKey) || Input.GetKey(jumpKey);

        if (isFlying)
        {
            float ascend = 0f;
            if (Input.GetKey(ascendKey)) ascend += 1f;
            if (Input.GetKey(descendKey)) ascend -= 1f;
            AscendInput = ascend;
            JumpPressed = false;
        }
        else
        {
            AscendInput = 0f;
            JumpPressed = Input.GetKeyDown(jumpKey);
        }
    }

    void DetectDoubleTaps()
    {
        bool isFlying = moveManager != null && moveManager.isFlying;

        if (Input.GetKeyDown(ascendKey))
        {
            if (!isFlying && Time.time - lastFlightTapTime <= doubleTapThreshold)
            {
                OnFlightRequested?.Invoke();
                lastFlightTapTime = -10f;
            }
            else lastFlightTapTime = Time.time;
        }

        if (Input.GetKeyDown(descendKey))
        {
            if (isFlying && Time.time - lastLandTapTime <= doubleTapThreshold)
            {
                OnLandRequested?.Invoke();
                lastLandTapTime = -10f;
            }
            else lastLandTapTime = Time.time;
        }

        // Doble toque por botones hápticos (se mantiene)
        if (hapticManager != null && hapticManager.IsConnected)
        {
            if (hapticManager.RightButton && !prevRightButton)
            {
                if (!isFlying && Time.time - lastFlightTapTime <= doubleTapThreshold)
                {
                    OnFlightRequested?.Invoke();
                    lastFlightTapTime = -10f;
                }
                else lastFlightTapTime = Time.time;
            }

            if (hapticManager.LeftButton && !prevLeftButton)
            {
                if (isFlying && Time.time - lastLandTapTime <= doubleTapThreshold)
                {
                    OnLandRequested?.Invoke();
                    lastLandTapTime = -10f;
                }
                else lastLandTapTime = Time.time;
            }
        }
    }
}