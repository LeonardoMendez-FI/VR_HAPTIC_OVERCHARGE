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
    public KeyCode forwardKey = KeyCode.UpArrow;
    public KeyCode backwardKey = KeyCode.DownArrow;
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public KeyCode ascendKey = KeyCode.Space;
    public KeyCode descendKey = KeyCode.Tab;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode yawLeftKey = KeyCode.Q;
    public KeyCode yawRightKey = KeyCode.E;
    public KeyCode rotateLeftKey = KeyCode.A;
    public KeyCode rotateRightKey = KeyCode.D;

    // ── Salidas para actores ────────────────────────────────
    [HideInInspector] public Vector2 MoveInput;
    [HideInInspector] public float GroundRotInput;
    [HideInInspector] public float YawInput;
    [HideInInspector] public float AscendInput;
    [HideInInspector] public bool JumpPressed;

    // ── Estados internos ────────────────────────────────────
    private float lastFlightTapTime = -10f;
    private float lastLandTapTime = -10f;

    // Botones hápticos previos
    private bool prevLeftButton;
    private bool prevRightButton;

    // ────────────────────────────────────────────────────────
    void Update()
    {
        if (hapticManager != null && hapticManager.IsConnected)
        {
            ReadHapticInput();
        }
        else
        {
            ReadKeyboardInput();
        }
        DetectDoubleTaps();
    }

    // ── LECTURA DESDE HAPTICMANAGER ─────────────────────────
    void ReadHapticInput()
    {
        // Joystick izquierdo → movimiento (X: strafe, Y: forward/back)
        MoveInput = hapticManager.LeftJoystick;

        // Joystick derecho (eje X) → rotación
        float rightX = hapticManager.RightJoystick.x;
        GroundRotInput = rightX;
        YawInput = rightX;

        // ── Botones según modo ──────────────────────────────
        bool isFlying = moveManager != null && moveManager.isFlying;

        // Botón derecho
        bool rightDown = hapticManager.RightButton;
        // Botón izquierdo
        bool leftDown = hapticManager.LeftButton;

        if (isFlying)
        {
            // En vuelo: botón derecho → ascender; botón izquierdo → descender
            AscendInput = (rightDown ? 1f : 0f) + (leftDown ? -1f : 0f);
            JumpPressed = false;   // no se salta en vuelo
        }
        else
        {
            // En suelo: botón derecho → saltar (flanco ascendente)
            JumpPressed = rightDown && !prevRightButton;
            AscendInput = 0f;      // no hay ascenso/descenso en suelo
        }

        // Actualizar estados previos
        prevLeftButton = leftDown;
        prevRightButton = rightDown;
    }

    // ── LECTURA DESDE TECLADO (FALLBACK) ────────────────────
    void ReadKeyboardInput()
    {
        // Movimiento horizontal
        float moveX = 0f;
        if (Input.GetKey(rightKey)) moveX += 1f;
        if (Input.GetKey(leftKey)) moveX -= 1f;

        float moveY = 0f;
        if (Input.GetKey(forwardKey)) moveY += 1f;
        if (Input.GetKey(backwardKey)) moveY -= 1f;

        MoveInput = new Vector2(moveX, moveY);

        // Rotación
        float yaw = 0f;
        if (Input.GetKey(yawRightKey)) yaw += 1f;
        if (Input.GetKey(yawLeftKey)) yaw -= 1f;
        YawInput = yaw;

        float groundRot = 0f;
        if (Input.GetKey(rotateRightKey)) groundRot += 1f;
        if (Input.GetKey(rotateLeftKey)) groundRot -= 1f;
        GroundRotInput = groundRot;

        // Ascenso/descenso (según modo)
        bool isFlying = moveManager != null && moveManager.isFlying;
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

    // ── DOBLE PULSACIÓN (teclado + háptico) ─────────────────
    void DetectDoubleTaps()
    {
        bool isFlying = moveManager != null && moveManager.isFlying;

        // ── Detección por teclado ───────────────────────────
        if (Input.GetKeyDown(ascendKey))   // flightKey original
        {
            if (!isFlying && Time.time - lastFlightTapTime <= doubleTapThreshold)
            {
                OnFlightRequested?.Invoke();
                lastFlightTapTime = -10f;
            }
            else
            {
                lastFlightTapTime = Time.time;
            }
        }

        if (Input.GetKeyDown(descendKey))  // landKey original
        {
            if (isFlying && Time.time - lastLandTapTime <= doubleTapThreshold)
            {
                OnLandRequested?.Invoke();
                lastLandTapTime = -10f;
            }
            else
            {
                lastLandTapTime = Time.time;
            }
        }

        // ── Detección por botones hápticos ──────────────────
        if (hapticManager != null && hapticManager.IsConnected)
        {
            // Botón derecho → doble toque para volar (solo en suelo)
            if (hapticManager.RightButton && !prevRightButton)  // flanco ascendente
            {
                if (!isFlying && Time.time - lastFlightTapTime <= doubleTapThreshold)
                {
                    OnFlightRequested?.Invoke();
                    lastFlightTapTime = -10f;
                }
                else
                {
                    lastFlightTapTime = Time.time;
                }
            }

            // Botón izquierdo → doble toque para aterrizar (solo en vuelo)
            if (hapticManager.LeftButton && !prevLeftButton)    // flanco ascendente
            {
                if (isFlying && Time.time - lastLandTapTime <= doubleTapThreshold)
                {
                    OnLandRequested?.Invoke();
                    lastLandTapTime = -10f;
                }
                else
                {
                    lastLandTapTime = Time.time;
                }
            }
        }
    }
}