using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class HapticManager : MonoBehaviour
{
    [Header("ESP32 Entradas (Joysticks + Botones)")]
    public int listenPort = 5005;

    [Header("ESP32 Salidas (Chaleco Háptico)")]
    public string salidasIP = "192.168.43.2";
    public int salidasPort = 5006;

    [Header("Configuración")]
    public float joystickDeadZone = 0.05f;
    public float sendInterval = 0.03f;

    // ── Estado público (leído por InputManager) ─────────────
    public Vector2 LeftJoystick { get; private set; }
    public Vector2 RightJoystick { get; private set; }
    public bool LeftButton { get; private set; }
    public bool RightButton { get; private set; }
    public bool IsConnected { get; private set; }

    // ── Snapshots para comunicación thread-safe ─────────────
    private HapticInputSnapshot _currentSnapshot;
    private volatile bool _snapshotReady;

    // ── Interno ─────────────────────────────────────────────
    private UdpClient receiveClient;
    private UdpClient sendClient;
    private Thread receiveThread;
    private bool threadRunning;
    private float lastReceiveTime;

    // ── Motores (valores objetivo 0-255) ───────────────────
    private float[] targetDamage = new float[4];
    private float[] targetCharge = new float[4];
    private float[] currentDamage = new float[4];
    private float[] currentCharge = new float[4];
    private float sendTimer;

    // ── Referencia a corrutinas de efectos ──────────────────
    private Coroutine attackRoutine;

    void Start()
    {
        threadRunning = true;
        receiveThread = new Thread(ReceiveLoop);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        sendClient = new UdpClient();
        sendClient.Connect(salidasIP, salidasPort);
    }

    void Update()
    {
        // Copiar snapshot del hilo de recepción si hay uno nuevo
        if (_snapshotReady)
        {
            _snapshotReady = false;
            var snap = _currentSnapshot;
            LeftJoystick = snap.leftJoystick;
            RightJoystick = snap.rightJoystick;
            LeftButton = snap.leftButton;
            RightButton = snap.rightButton;
            IsConnected = snap.isConnected;
            lastReceiveTime = Time.time;
        }
        else if (Time.time - lastReceiveTime > 1f)
        {
            IsConnected = false;
            // Desconexión: apagar todos los motores
            if (IsConnected == false) // solo cuando cambia
            {
                StopChargeEffect();
                for (int i = 0; i < 4; i++) targetDamage[i] = 0;
            }
        }

        // Interpolar motores hacia objetivos
        float t = Time.deltaTime * 20f;
        for (int i = 0; i < 4; i++)
        {
            currentDamage[i] = Mathf.Lerp(currentDamage[i], targetDamage[i], t);
            currentCharge[i] = Mathf.Lerp(currentCharge[i], targetCharge[i], t);
        }

        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            SendMotorPacket();
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

    // ── Hilo de recepción ──────────────────────────────────
    void ReceiveLoop()
    {
        receiveClient = new UdpClient(listenPort);
        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
        while (threadRunning)
        {
            try
            {
                byte[] data = receiveClient.Receive(ref remote);
                string msg = Encoding.ASCII.GetString(data);
                ParseAndSnapshot(msg);
            }
            catch (SocketException) { /* timeout o cierre */ }
        }
    }

    void ParseAndSnapshot(string msg)
    {
        string[] parts = msg.Split(',');
        if (parts.Length < 6) return;

        int.TryParse(parts[0], out int x1); int.TryParse(parts[1], out int y1);
        int.TryParse(parts[2], out int x2); int.TryParse(parts[3], out int y2);
        int.TryParse(parts[4], out int b1); int.TryParse(parts[5], out int b2);

        _currentSnapshot = new HapticInputSnapshot
        {
            leftJoystick = new Vector2(x1, y1),
            rightJoystick = new Vector2(x2, y2),
            leftButton = b1 == 1,
            rightButton = b2 == 1,
            isConnected = true
        };
        _snapshotReady = true;
    }

    // ── Envío de motores ───────────────────────────────────
    void SendMotorPacket()
    {
        byte[] packet = new byte[8];
        for (int i = 0; i < 4; i++)
        {
            packet[i] = (byte)Mathf.Clamp((int)currentDamage[i], 0, 255);
            packet[i + 4] = (byte)Mathf.Clamp((int)currentCharge[i], 0, 255);
        }
        sendClient?.Send(packet, 8);
    }

    // ── API para otros sistemas ────────────────────────────

    public void TriggerDamageFeedback(Vector3 enemyDir)
    {
        Transform player = Camera.main?.transform;
        if (player == null) return;
        Vector3 localDir = player.InverseTransformDirection(enemyDir.normalized);
        float angle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

        int motorIndex = -1;
        if (angle > -45 && angle <= 45) motorIndex = 1;
        else if (angle > 45 && angle <= 135) motorIndex = 0;
        else if (angle < -45 && angle >= -135) motorIndex = 3;
        else motorIndex = 2;

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