// ============================================================
//  HUDEvents.cs
//  Sci-Fi Robot HUD System — Event Definitions
//  ============================================================
//  Drop this in your project. These UnityEvents are exposed
//  by your game managers (EnergyManager, StructManager, etc.)
//  and subscribed to by UIManager. Zero coupling.
// ============================================================

using UnityEngine;
using UnityEngine.Events;

namespace RoboticHUD.Events
{
    // ── Typed UnityEvent subclasses (serializable in Inspector) ──

    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    [System.Serializable]
    public class IntEvent : UnityEvent<int> { }

    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }

    [System.Serializable]
    public class Vector2Event : UnityEvent<Vector2> { }

    // ── HUD Event Container (optional singleton bus) ──
    // Attach to a persistent GameObject if you prefer a
    // global event bus over per-manager event references.

    public class HUDEvents : MonoBehaviour
    {
        public static HUDEvents Instance { get; private set; }

        [Header("─── Energy System ───────────────────────────")]
        /// <summary>0–1 normalized energy level.</summary>
        public FloatEvent OnEnergyChanged = new FloatEvent();

        [Header("─── Structure / Health ──────────────────────")]
        /// <summary>0–1 normalized structure integrity.</summary>
        public FloatEvent OnStructureChanged = new FloatEvent();

        [Header("─── Movement / Flight ───────────────────────")]
        /// <summary>True = flight mode active.</summary>
        public BoolEvent  OnFlightModeChanged = new BoolEvent();

        [Header("─── Joystick Telemetry ──────────────────────")]
        /// <summary>Raw joystick axis input (-1..1 per axis).</summary>
        public Vector2Event OnJoystickInputChanged = new Vector2Event();

        [Header("─── Level / Narrative ───────────────────────")]
        /// <summary>e.g. "LEVEL 2 — ROBOTICS LABORATORY"</summary>
        public StringEvent OnLevelTitleChanged = new StringEvent();

        [Header("─── Counters ────────────────────────────────")]
        public IntEvent OnEliminationCountChanged  = new IntEvent();
        public IntEvent OnObjectiveCountChanged    = new IntEvent();

        [Header("─── Danger / Warnings ───────────────────────")]
        /// <summary>True when structure critically low.</summary>
        public BoolEvent OnCriticalWarning = new BoolEvent();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
