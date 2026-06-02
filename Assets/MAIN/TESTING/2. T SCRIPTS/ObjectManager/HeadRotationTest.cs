using UnityEngine;

public class HeadRotationTest : MonoBehaviour
{
    [Header("Head Transform")]
    public Transform headTransform;       // Arrastra aquí el transform de la cabeza

    [Header("Keys")]
    public KeyCode leftKey = KeyCode.H;   // Girar izquierda
    public KeyCode rightKey = KeyCode.J;  // Girar derecha
    public KeyCode upKey = KeyCode.U;     // Mirar arriba
    public KeyCode downKey = KeyCode.K;   // Mirar abajo

    [Header("Settings")]
    public float rotationSpeed = 90f;     // Grados por segundo
    [Range(1, 90)] public float maxAngle = 60f;   // Límite máximo en cada eje

    private float currentYaw;             // Acumulado horizontal (Y)
    private float currentPitch;           // Acumulado vertical (X)

    void Start()
    {
        if (headTransform == null)
            headTransform = transform;

        // Guardar la rotación local inicial y limitarla
        Vector3 startAngles = headTransform.localEulerAngles;
        currentYaw   = NormalizeAngle(startAngles.y);
        currentPitch = NormalizeAngle(startAngles.x);
        ClampAndApply();
    }

    void Update()
    {
        // Leer entradas
        float yawInput = 0f;
        if (Input.GetKey(leftKey))  yawInput -= 1f;
        if (Input.GetKey(rightKey)) yawInput += 1f;

        float pitchInput = 0f;
        if (Input.GetKey(upKey))    pitchInput -= 1f;   // En Unity, pitch positivo mira hacia abajo; invertimos
        if (Input.GetKey(downKey))  pitchInput += 1f;

        // Acumular ángulos (en grados)
        currentYaw   += yawInput * rotationSpeed * Time.deltaTime;
        currentPitch += pitchInput * rotationSpeed * Time.deltaTime;

        ClampAndApply();
    }

    void ClampAndApply()
    {
        // Limitar cada eje al rango ±maxAngle
        currentYaw   = Mathf.Clamp(currentYaw, -maxAngle, maxAngle);
        currentPitch = Mathf.Clamp(currentPitch, -maxAngle, maxAngle);

        // Aplicar rotación local (Yaw sobre Y, Pitch sobre X)
        headTransform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    // Convierte un ángulo de 0-360 a -180..180
    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f)  angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }
}