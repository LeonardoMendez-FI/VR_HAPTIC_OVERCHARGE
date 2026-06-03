using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class HapticService : ServiceScript
{
    [Header("Recepción de entrada (ESP32 Entradas)")]
    public int listenPort = 5005;

    [Header("Envío a motores (ESP32 Salidas)")]
    public string motorIPPrimary = "192.168.4.2";
    public string motorIPFallback = "192.168.4.1";
    public int motorPort = 5006;

    [Header("Configuración")]
    public float joystickDeadZone = 0.05f;
    public float sendInterval = 0.03f;
    public float motorReconnectDelay = 2f;

    public Vector2 LeftJoystick  { get; private set; }
    public Vector2 RightJoystick { get; private set; }
    public bool LeftButton  { get; private set; }
    public bool RightButton { get; private set; }
    public bool IsConnected { get; private set; }
    public bool IsMotorConnected { get; private set; }

    private HapticInputSnapshot _currentSnapshot;
    private volatile bool _snapshotReady;
    private readonly object _snapLock = new object();

    private UdpClient receiveClient;
    private UdpClient sendClient;
    private Thread receiveThread;
    private bool threadRunning;
    private float lastReceiveTime;

    private float[] targetDamage = new float[4];
    private float[] targetCharge = new float[4];
    private float[] currentDamage = new float[4];
    private float[] currentCharge = new float[4];
    private float sendTimer;
    private float motorReconnectTimer;

    private Coroutine attackRoutine;

    void Start()
    {
        StartCoroutine(InitReceiveThreadSafe());
    }

    IEnumerator InitReceiveThreadSafe()
    {
        int maxAttempts = 3;
        bool portOpened = false;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                receiveClient = new UdpClient(listenPort);
                portOpened = true;
                break;
            }
            catch (SocketException)
            {
                // No hacer nada, lo gestionamos después
            }

            if (!portOpened)
            {
                Debug.LogWarning($"[HapticService] Intento {attempt + 1}/{maxAttempts}: Puerto {listenPort} ocupado. Reintentando en 2s...");
                yield return new WaitForSeconds(2f);
            }
        }

        if (!portOpened)
        {
            Debug.LogWarning("[HapticService] No se pudo abrir el puerto. Entrada háptica desactivada (teclado disponible).");
            IsConnected = false;
            yield break;
        }

        Debug.Log($"[HapticService] Recepción iniciada en puerto {listenPort}");
        threadRunning = true;
        receiveThread = new Thread(ReceiveLoop);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        sendClient = new UdpClient();
        motorReconnectTimer = motorReconnectDelay;
    }

    void Update()
    {
        if (!threadRunning) return;

        lock (_snapLock)
        {
            if (_snapshotReady)
            {
                _snapshotReady = false;
                var snap = _currentSnapshot;
                LeftJoystick   = snap.leftJoystick;
                RightJoystick  = snap.rightJoystick;
                LeftButton     = snap.leftButton;
                RightButton    = snap.rightButton;
                IsConnected    = snap.isConnected;
                lastReceiveTime = Time.time;
            }
            else if (Time.time - lastReceiveTime > 1f)
            {
                IsConnected = false;
            }
        }

        float t = Time.deltaTime * 20f;
        for (int i = 0; i < 4; i++)
        {
            currentDamage[i] = Mathf.Lerp(currentDamage[i], targetDamage[i], t);
            currentCharge[i] = Mathf.Lerp(currentCharge[i], targetCharge[i], t);
        }

        sendTimer += Time.deltaTime;
        motorReconnectTimer += Time.deltaTime;

        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;

            if (IsMotorConnected)
            {
                if (!SendMotorPacket())
                {
                    IsMotorConnected = false;
                    motorReconnectTimer = 0f;
                }
            }
            else
            {
                if (motorReconnectTimer >= motorReconnectDelay)
                {
                    motorReconnectTimer = 0f;
                    if (TryMotorHandshake())
                        IsMotorConnected = true;
                }
            }
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            threadRunning = false;
            receiveClient?.Close();
            receiveThread?.Join(500);
        }
        else
        {
            StartCoroutine(InitReceiveThreadSafe());
        }
    }

    void OnDestroy()
    {
        threadRunning = false;
        receiveClient?.Close();
        sendClient?.Close();
        if (receiveThread != null && receiveThread.IsAlive)
            receiveThread.Join(500);
    }

    void ReceiveLoop()
    {
        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
        while (threadRunning)
        {
            try
            {
                byte[] data = receiveClient.Receive(ref remote);
                string msg = Encoding.ASCII.GetString(data);
                ParseAndSnapshot(msg);
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { break; }
        }
    }

    void ParseAndSnapshot(string msg)
    {
        string[] parts = msg.Split(',');
        if (parts.Length < 6) return;

        int.TryParse(parts[0], out int x1); int.TryParse(parts[1], out int y1);
        int.TryParse(parts[2], out int x2); int.TryParse(parts[3], out int y2);
        int.TryParse(parts[4], out int b1); int.TryParse(parts[5], out int b2);

        var snap = new HapticInputSnapshot
        {
            leftJoystick  = new Vector2(x1, y1),
            rightJoystick = new Vector2(x2, y2),
            leftButton    = b1 == 1,
            rightButton   = b2 == 1,
            isConnected   = true
        };

        lock (_snapLock)
        {
            _currentSnapshot = snap;
            _snapshotReady = true;
        }
    }

    bool SendMotorPacket()
    {
        if (sendClient == null) return false;
        byte[] packet = new byte[8];
        for (int i = 0; i < 4; i++)
        {
            packet[i]     = (byte)Mathf.Clamp((int)currentDamage[i], 0, 255);
            packet[i + 4] = (byte)Mathf.Clamp((int)currentCharge[i], 0, 255);
        }

        try
        {
            sendClient.Send(packet, packet.Length, motorIPPrimary, motorPort);
            return true;
        }
        catch (SocketException)
        {
            try
            {
                sendClient.Send(packet, packet.Length, motorIPFallback, motorPort);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }

    bool TryMotorHandshake()
    {
        byte[] dummy = new byte[8];
        try
        {
            sendClient?.Send(dummy, dummy.Length, motorIPPrimary, motorPort);
            return true;
        }
        catch (SocketException)
        {
            try
            {
                sendClient?.Send(dummy, dummy.Length, motorIPFallback, motorPort);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
    }

    // ── API pública ────────────────────────────────────────

    public void TriggerDamageFeedback(Vector3 enemyDir)
    {
        Transform player = Camera.main?.transform;
        if (player == null) return;
        Vector3 localDir = player.InverseTransformDirection(enemyDir.normalized);
        float angle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

        int motorIndex = -1;
        if (angle > -45 && angle <= 45)       motorIndex = 1;
        else if (angle > 45 && angle <= 135)  motorIndex = 0;
        else if (angle < -45 && angle >= -135) motorIndex = 3;
        else                                   motorIndex = 2;

        if (motorIndex >= 0)
            StartCoroutine(DamagePulse(motorIndex));
    }

    IEnumerator DamagePulse(int idx)
    {
        targetDamage[idx] = 255;
        yield return new WaitForSeconds(0.15f);
        targetDamage[idx] = 0;
    }

    public void SetChargeProgress(float normalized)
    {
        float intensity = Mathf.Clamp01(normalized);
        for (int i = 0; i < 4; i++)
        {
            float threshold = (i + 1) * 0.25f;
            if (intensity <= 0) targetCharge[i] = 0;
            else if (intensity >= threshold) targetCharge[i] = 255;
            else
            {
                float localProgress = Mathf.InverseLerp(i * 0.25f, threshold, intensity);
                targetCharge[i] = localProgress * 255f;
            }
        }
    }

    public void StopChargeEffect()
    {
        for (int i = 0; i < 4; i++) targetCharge[i] = 0;
    }

    public void PlayFlightActivationEffect()
    {
        StartCoroutine(AllPulse(0.2f, 0.1f));
    }

    public void PlayEnergyDepletedEffect()
    {
        StartCoroutine(AllPulse(0.1f, 0.05f));
    }

    IEnumerator AllPulse(float duration, float interval)
    {
        for (int i = 0; i < 4; i++) targetCharge[i] = 255;
        yield return new WaitForSeconds(interval);
        for (int i = 0; i < 4; i++) targetCharge[i] = 0;
        yield return new WaitForSeconds(duration - interval);
    }

    public void StartAttackEffect(float duration)
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackBlink(duration));
    }

    IEnumerator AttackBlink(float duration)
    {
        float elapsed = 0f;
        bool on = true;
        float interval = 0.1f;
        while (elapsed < duration)
        {
            for (int i = 0; i < 4; i++) targetCharge[i] = on ? 255 : 0;
            on = !on;
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
        StopChargeEffect();
    }

    public void EndAttackEffect()
    {
        StartCoroutine(SmoothStopCharge(1.5f));
    }

    IEnumerator SmoothStopCharge(float duration)
    {
        float[] startVals = new float[4];
        for (int i = 0; i < 4; i++) startVals[i] = currentCharge[i];
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            for (int i = 0; i < 4; i++)
                targetCharge[i] = Mathf.Lerp(startVals[i], 0, t);
            yield return null;
            elapsed += Time.deltaTime;
        }
        for (int i = 0; i < 4; i++) targetCharge[i] = 0;
    }
}