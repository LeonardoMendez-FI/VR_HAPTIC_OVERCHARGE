using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    [Header("Detección de doble pulsación")]
    public KeyCode flightKey = KeyCode.Space;
    public KeyCode landKey = KeyCode.Tab;
    public float doubleTapThreshold = 0.3f;

    [Header("Eventos de cambio de modo")]
    public UnityEvent OnFlightRequested;
    public UnityEvent OnLandRequested;

    [Header("Teclas de propulsión (vuelo)")]
    public KeyCode forwardKey1 = KeyCode.W;        // W
    public KeyCode forwardKey2 = KeyCode.UpArrow;   // ↑
    public KeyCode backwardKey1 = KeyCode.S;        // S
    public KeyCode backwardKey2 = KeyCode.DownArrow; // ↓
    public KeyCode leftKey1 = KeyCode.A;            // A
    public KeyCode leftKey2 = KeyCode.LeftArrow;    // ←
    public KeyCode rightKey1 = KeyCode.D;           // D
    public KeyCode rightKey2 = KeyCode.RightArrow;  // →

    [Header("Teclas verticales / rotación suelo")]
    public KeyCode ascendKey = KeyCode.Space;
    public KeyCode descendKey = KeyCode.Tab;
    public KeyCode jumpKey = KeyCode.Space;          // mismo que ascend, modo suelo
    public KeyCode rotateLeftKey = KeyCode.A;       // A
    public KeyCode rotateRightKey = KeyCode.D;      // D

    // Salidas para actores
    [HideInInspector] public Vector2 MoveInput;       // solo para suelo, no se usa en vuelo
    [HideInInspector] public float GroundRotInput;    // A/D para rotación en suelo (-1 izquierda, 1 derecha)
    [HideInInspector] public float AscendInput;       // -1..1 (Tab a Space)
    [HideInInspector] public bool JumpPressed;        // true en el frame que se presiona Space (suelo)

    // Estados individuales de teclas (para el sistema de propulsión)
    [HideInInspector] public bool W, S, A, D;
    [HideInInspector] public bool UpArrow, DownArrow, LeftArrow, RightArrow;

    private float lastFlightKeyTime = -10f;
    private float lastLandKeyTime = -10f;

    void Update()
    {
        ReadKeyboardInput();
        DetectDoubleTaps();
    }

    void ReadKeyboardInput()
    {
        // Propulsión (vuelo)
        W = Input.GetKey(forwardKey1);
        UpArrow = Input.GetKey(forwardKey2);
        S = Input.GetKey(backwardKey1);
        DownArrow = Input.GetKey(backwardKey2);
        A = Input.GetKey(leftKey1);
        LeftArrow = Input.GetKey(leftKey2);
        D = Input.GetKey(rightKey1);
        RightArrow = Input.GetKey(rightKey2);

        // Rotación en suelo
        float rot = 0f;
        if (Input.GetKey(rotateLeftKey)) rot -= 1f;
        if (Input.GetKey(rotateRightKey)) rot += 1f;
        GroundRotInput = rot;

        // Ascenso/descenso continuo (para vuelo)
        float ascend = 0f;
        if (Input.GetKey(ascendKey)) ascend += 1f;
        if (Input.GetKey(descendKey)) ascend -= 1f;
        AscendInput = ascend;

        // Salto (suelo) – solo se activa en modo suelo
        JumpPressed = Input.GetKeyDown(jumpKey);

        // Movimiento en suelo (por ahora con flechas, aunque luego se podría unificar)
        float moveX = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) moveX += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveX -= 1f;
        float moveY = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) moveY += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveY -= 1f;
        MoveInput = new Vector2(moveX, moveY);
    }

    void DetectDoubleTaps()
    {
        // Doble toque para volar
        if (Input.GetKeyDown(flightKey))
        {
            if (Time.time - lastFlightKeyTime <= doubleTapThreshold)
            {
                OnFlightRequested?.Invoke();
                lastFlightKeyTime = -10f;
            }
            else
                lastFlightKeyTime = Time.time;
        }

        // Doble toque para aterrizar
        if (Input.GetKeyDown(landKey))
        {
            if (Time.time - lastLandKeyTime <= doubleTapThreshold)
            {
                OnLandRequested?.Invoke();
                lastLandKeyTime = -10f;
            }
            else
                lastLandKeyTime = Time.time;
        }
    }
}