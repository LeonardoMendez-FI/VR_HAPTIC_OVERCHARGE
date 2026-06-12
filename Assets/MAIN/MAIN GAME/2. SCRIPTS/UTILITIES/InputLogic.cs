using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Fuente unificada de entrada para tres dispositivos:
///
///   1. Chaleco háptico (ESP32 via UDP)  — prioridad exclusiva cuando está conectado
///   2. Teclado                          — activo cuando no hay háptico
///   3. Gamepad estándar                 — activo cuando no hay háptico
///
/// Cuando el háptico está conectado, teclado y gamepad se ignoran completamente.
/// Cuando el háptico no está conectado, teclado y gamepad se suman (OR).
///
/// ── Mapeo unificado ────────────────────────────────────────────────────────
///
///   Acción          Teclado          Háptico           Gamepad
///   ──────────────────────────────────────────────────────────
///   Mover           W/S/A/D          JS izq.           JS izq.
///   Rotar/Cabeza    Flechas          JS der.           JS der.
///   Botón izq.      Tab              Btn izq.          L3 / LT
///   Botón der.      Space            Btn der.          R3 / RT
///   Vuelo           Space x2         Btn der. x2       R3 x2
///   Aterrizar       Tab x2           Btn izq. x2       L3 x2
///
/// Campos públicos de salida (leídos por actores y vistas):
///   LeftJoystickRaw  — Vector2 (x=strafe, y=forward)
///   RightJoystickRaw — Vector2 (x=rotation, y=pitch)
///   LeftButtonHeld   — Tab / BtnIzq / L3 o LT
///   RightButtonHeld  — Space / BtnDer / R3 o RT
///   JumpPressed      — primer frame del RightButton
///   W, S, A, D       — bools derivados de LeftJoystickRaw
///   UpArrow, DownArrow, LeftArrow, RightArrow — bools derivados de RightJoystickRaw
/// </summary>
public class InputLogic : MonoBehaviour
{
    // ── Referencias ─────────────────────────────────────────────────────────

    [Header("Háptico (opcional — prioridad exclusiva si conectado)")]
    public HapticService hapticService;

    [Header("Permisos")]
    public PlayerPermissions permissions;

    // ── Configuración ────────────────────────────────────────────────────────

    [Header("Doble pulsación")]
    public float doubleTapThreshold = 0.3f;

    [Header("Teclado — teclas configurables")]
    public KeyCode keyForward  = KeyCode.W;
    public KeyCode keyBack     = KeyCode.S;
    public KeyCode keyLeft     = KeyCode.A;
    public KeyCode keyRight    = KeyCode.D;
    public KeyCode keyRotLeft  = KeyCode.LeftArrow;
    public KeyCode keyRotRight = KeyCode.RightArrow;
    public KeyCode keyLookUp   = KeyCode.UpArrow;
    public KeyCode keyLookDown = KeyCode.DownArrow;
    public KeyCode keyLeftBtn  = KeyCode.Tab;
    public KeyCode keyRightBtn = KeyCode.Space;

    [Header("Gamepad — umbrales")]
    [Tooltip("Umbral de joystick para considerar movimiento (gamepad).")]
    [Range(0.05f, 0.5f)] public float gamepadDeadzone = 0.15f;
    [Tooltip("Umbral de gatillo para usarlo como botón (fallback si no hay botón en joystick).")]
    [Range(0.3f, 0.9f)]  public float triggerThreshold = 0.5f;

    // ── Eventos ──────────────────────────────────────────────────────────────

    [Header("Eventos de modo de vuelo")]
    public UnityEvent OnFlightRequested;
    public UnityEvent OnLandRequested;

    // ── Salidas (leídas por actores y vistas) ────────────────────────────────

    [HideInInspector] public Vector2 LeftJoystickRaw;   // (strafe X, forward Y)
    [HideInInspector] public Vector2 RightJoystickRaw;  // (rotation X, pitch Y)

    [HideInInspector] public bool LeftButtonHeld;
    [HideInInspector] public bool RightButtonHeld;
    [HideInInspector] public bool JumpPressed;          // primer frame RightButton

    // Bools derivados de los joysticks (compatibilidad con actores existentes)
    [HideInInspector] public bool W, S, A, D;
    [HideInInspector] public bool UpArrow, DownArrow, LeftArrow, RightArrow;

    // ── Estado interno ───────────────────────────────────────────────────────

    private bool _prevRightBtn;
    private bool _prevLeftBtn;

    // Para doble tap (edge detection sobre rising edge del botón)
    private bool _prevRightEdge;
    private bool _prevLeftEdge;
    private float _lastRightTapTime = -10f;
    private float _lastLeftTapTime  = -10f;

    // ── Unity ────────────────────────────────────────────────────────────────

    void Update()
    {
        if (hapticService != null && hapticService.IsConnected)
            ReadHaptic();
        else
            ReadKeyboardAndGamepad();

        DeriveBools();
        DetectDoubleTaps();

        _prevRightBtn = RightButtonHeld;
        _prevLeftBtn  = LeftButtonHeld;
    }

    // ── Lecturas de fuente ───────────────────────────────────────────────────

    /// <summary>Háptico: prioridad exclusiva.</summary>
    private void ReadHaptic()
    {
        LeftJoystickRaw  = hapticService.LeftJoystick;
        RightJoystickRaw = hapticService.RightJoystick;
        LeftButtonHeld   = hapticService.LeftButton;
        RightButtonHeld  = hapticService.RightButton;
        JumpPressed      = RightButtonHeld && !_prevRightBtn;
    }

    /// <summary>Teclado y gamepad combinados (OR). Activo cuando el háptico no está.</summary>
    private void ReadKeyboardAndGamepad()
    {
        // ── Teclado ──────────────────────────────────────────────────────────
        float kMoveX = (Input.GetKey(keyRight)    ? 1f : 0f) - (Input.GetKey(keyLeft)     ? 1f : 0f);
        float kMoveY = (Input.GetKey(keyForward)   ? 1f : 0f) - (Input.GetKey(keyBack)     ? 1f : 0f);
        float kRotX  = (Input.GetKey(keyRotRight)  ? 1f : 0f) - (Input.GetKey(keyRotLeft)  ? 1f : 0f);
        float kRotY  = (Input.GetKey(keyLookUp)    ? 1f : 0f) - (Input.GetKey(keyLookDown) ? 1f : 0f);

        bool kLeftBtn  = Input.GetKey(keyLeftBtn);
        bool kRightBtn = Input.GetKey(keyRightBtn);

        // ── Gamepad (legacy Input — sin New Input System) ─────────────────
        // Unity mapea el primer gamepad conectado en los ejes nombrados.
        // Nombres estándar en el sistema legacy:
        //   "Horizontal"  / "Vertical"  → JS izq.
        //   "RightStickHorizontal" / "RightStickVertical" → JS der.
        //   "joystick button 8"  → L3
        //   "joystick button 9"  → R3
        //   "joystick button 14" → L3 (PS4/PS5 via legacy)  
        //   "joystick button 15" → R3 (PS4/PS5 via legacy)
        //   Axis 8 / Axis 9  → LT / RT
        //
        // Se detecta automáticamente si hay gamepad activo comparando magnitud del eje.
        float gMoveX = Input.GetAxisRaw("Horizontal");
        float gMoveY = Input.GetAxisRaw("Vertical");
        float gRotX  = GetRightStickX();
        float gRotY  = GetRightStickY();

        // Botones del joystick (L3 / R3) con fallback a gatillos
        bool gLeftBtn  = ReadLeftGamepadButton();
        bool gRightBtn = ReadRightGamepadButton();

        // ── Combinar (OR / max) ───────────────────────────────────────────
        // Para ejes: se usa el que tenga mayor magnitud absoluta.
        float finalMoveX = AbsMax(kMoveX, ApplyDeadzone(gMoveX));
        float finalMoveY = AbsMax(kMoveY, ApplyDeadzone(gMoveY));
        float finalRotX  = AbsMax(kRotX,  ApplyDeadzone(gRotX));
        float finalRotY  = AbsMax(kRotY,  ApplyDeadzone(gRotY));

        LeftJoystickRaw  = new Vector2(finalMoveX, finalMoveY);
        RightJoystickRaw = new Vector2(finalRotX,  finalRotY);

        LeftButtonHeld  = kLeftBtn  || gLeftBtn;
        RightButtonHeld = kRightBtn || gRightBtn;

        // JumpPressed = primer frame en que RightButton pasa de false a true
        JumpPressed = RightButtonHeld && !_prevRightBtn;
    }

    // ── Helpers de gamepad (legacy Input) ────────────────────────────────────

    private float GetRightStickX()
    {
        // Unity 2019+ legacy: "RightStickHorizontal" si el eje está configurado en Input Manager.
        // Si no está en el proyecto, cae a 0 silenciosamente con try/catch.
        try   { return Input.GetAxisRaw("RightStickHorizontal"); }
        catch { return 0f; }
    }

    private float GetRightStickY()
    {
        try   { return Input.GetAxisRaw("RightStickVertical"); }
        catch { return 0f; }
    }

    private bool ReadLeftGamepadButton()
    {
        // L3 en la mayoría de gamepads
        if (Input.GetKey("joystick button 8"))  return true;   // XInput L3
        if (Input.GetKey("joystick button 10")) return true;   // legacy alternativo
        // Fallback: gatillo izquierdo
        float lt = GetAxis("3");   // Axis 3 = LT en XInput legacy
        return lt > triggerThreshold;
    }

    private bool ReadRightGamepadButton()
    {
        // R3
        if (Input.GetKey("joystick button 9"))  return true;   // XInput R3
        if (Input.GetKey("joystick button 11")) return true;   // legacy alternativo
        // Fallback: gatillo derecho
        float rt = GetAxis("4");   // Axis 4 = RT en XInput legacy (varía por plataforma)
        return rt > triggerThreshold;
    }

    private float GetAxis(string axisName)
    {
        try   { return Input.GetAxisRaw("Axis " + axisName); }
        catch { return 0f; }
    }

    // ── Derivar bools desde joysticks ────────────────────────────────────────

    private void DeriveBools()
    {
        W = LeftJoystickRaw.y  >  gamepadDeadzone;
        S = LeftJoystickRaw.y  < -gamepadDeadzone;
        A = LeftJoystickRaw.x  < -gamepadDeadzone;
        D = LeftJoystickRaw.x  >  gamepadDeadzone;

        RightArrow = RightJoystickRaw.x >  gamepadDeadzone;
        LeftArrow  = RightJoystickRaw.x < -gamepadDeadzone;
        UpArrow    = RightJoystickRaw.y >  gamepadDeadzone;
        DownArrow  = RightJoystickRaw.y < -gamepadDeadzone;
    }

    // ── Doble tap ────────────────────────────────────────────────────────────

    private void DetectDoubleTaps()
    {
        // Rising edge del botón derecho → vuelo
        bool rightEdge = RightButtonHeld && !_prevRightEdge;
        if (rightEdge)
        {
            if (Time.time - _lastRightTapTime <= doubleTapThreshold)
            {
                if (permissions == null || permissions.canToggleFlight)
                    OnFlightRequested?.Invoke();
                _lastRightTapTime = -10f;   // consumir para no re-disparar
            }
            else
            {
                _lastRightTapTime = Time.time;
            }
        }
        _prevRightEdge = RightButtonHeld;

        // Rising edge del botón izquierdo → aterrizar
        bool leftEdge = LeftButtonHeld && !_prevLeftEdge;
        if (leftEdge)
        {
            if (Time.time - _lastLeftTapTime <= doubleTapThreshold)
            {
                if (permissions == null || permissions.canLand)
                    OnLandRequested?.Invoke();
                _lastLeftTapTime = -10f;
            }
            else
            {
                _lastLeftTapTime = Time.time;
            }
        }
        _prevLeftEdge = LeftButtonHeld;
    }

    // ── Utilidades ───────────────────────────────────────────────────────────

    private float ApplyDeadzone(float value)
    {
        return Mathf.Abs(value) >= gamepadDeadzone ? value : 0f;
    }

    /// <summary>Devuelve el valor con mayor magnitud absoluta, preservando signo.</summary>
    private float AbsMax(float a, float b)
    {
        return Mathf.Abs(a) >= Mathf.Abs(b) ? a : b;
    }
}
