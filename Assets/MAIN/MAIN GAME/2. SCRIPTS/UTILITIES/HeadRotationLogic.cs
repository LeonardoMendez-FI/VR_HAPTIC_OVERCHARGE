using UnityEngine;

public class HeadRotationLogic : MonoBehaviour
{
    [Header("Head Transform")]
    public Transform headTransform;

    [Header("Keys")]
    public KeyCode leftKey = KeyCode.H;
    public KeyCode rightKey = KeyCode.J;
    public KeyCode upKey = KeyCode.U;
    public KeyCode downKey = KeyCode.K;

    [Header("Settings")]
    public float rotationSpeed = 90f;
    [Range(1, 90)] public float maxAngle = 60f;

    private float currentYaw;
    private float currentPitch;

    void Start()
    {
        if (headTransform == null)
            headTransform = transform;

        Vector3 startAngles = headTransform.localEulerAngles;
        currentYaw   = NormalizeAngle(startAngles.y);
        currentPitch = NormalizeAngle(startAngles.x);
        ClampAndApply();
    }

    void Update()
    {
        float yawInput = 0f;
        if (Input.GetKey(leftKey))  yawInput -= 1f;
        if (Input.GetKey(rightKey)) yawInput += 1f;

        float pitchInput = 0f;
        if (Input.GetKey(upKey))    pitchInput -= 1f;
        if (Input.GetKey(downKey))  pitchInput += 1f;

        currentYaw   += yawInput * rotationSpeed * Time.deltaTime;
        currentPitch += pitchInput * rotationSpeed * Time.deltaTime;

        ClampAndApply();
    }

    void ClampAndApply()
    {
        currentYaw   = Mathf.Clamp(currentYaw,   -maxAngle, maxAngle);
        currentPitch = Mathf.Clamp(currentPitch, -maxAngle, maxAngle);

        headTransform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f)  angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }
}