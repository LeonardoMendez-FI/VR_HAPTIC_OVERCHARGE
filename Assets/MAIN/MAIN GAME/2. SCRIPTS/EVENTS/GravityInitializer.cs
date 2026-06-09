using UnityEngine;

public class GravityInitializer : MonoBehaviour
{
    void Awake()
    {
        Physics.gravity = new Vector3(0f, PlayerParameters.GRAVITY, 0f);
        Debug.Log($"[Gravity] Gravity set to {PlayerParameters.GRAVITY}");
    }
}