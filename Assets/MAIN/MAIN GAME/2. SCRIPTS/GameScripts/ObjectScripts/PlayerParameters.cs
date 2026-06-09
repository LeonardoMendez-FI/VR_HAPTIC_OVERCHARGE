public static class PlayerParameters
{
    // ── Player locomotion ────────────────────────────────────────────────────
    /// <summary>Base linear speed in m/s. Used for Rigidbody movement.</summary>
    public const float MEDIUM_LINEAR_SPEED = 15f;

    /// <summary>Base angular speed in rad/s. Used for Rigidbody angularVelocity only.</summary>
    public const float MEDIUM_ANGULAR_SPEED = 0.75f;

    public const float JUMP_FORCE_RATIO = 1.5f;
    public const float FLIGHT_ASCEND_FORCE_RATIO = 0.6f;
    public const float GRAVITY = -25f;

    // ── Energy ───────────────────────────────────────────────────────────────
    public const float MAX_ENERGY = 100f;
    public const float DRAIN_TIME_FULL = 20f;
    public const float ENERGY_TO_STRUCT = 1f;
    public static float DrainRate => MAX_ENERGY / DRAIN_TIME_FULL;

    // ── Enemy detection ──────────────────────────────────────────────────────
    /// <summary>Distance at which ground enemies first detect the player.</summary>
    public const float ENEMY_DETECTION_RANGE = 125f;

    /// <summary>Distance at which ground enemies abandon the chase.</summary>
    public const float ENEMY_LOSE_RANGE = 250f;

    // ── Enemy locomotion ─────────────────────────────────────────────────────
    /// <summary>
    /// Speed multiplier applied to MEDIUM_LINEAR_SPEED for enemy NavMeshAgents.
    /// Keeps enemy ground speed relative to the player and provides a single
    /// dial for level designers without touching individual prefabs.
    /// </summary>
    public const float ENEMY_SPEED_MULTIPLIER = 0.8f;

    /// <summary>
    /// NavMeshAgent angular speed in DEGREES/sec.
    /// NavMeshAgent.angularSpeed expects degrees, unlike Rigidbody which uses rad/s.
    /// Do NOT use MEDIUM_ANGULAR_SPEED for NavMeshAgents.
    /// </summary>
    public const float ENEMY_ANGULAR_SPEED_DEG = 120f;

    // ── Enemy projectiles ────────────────────────────────────────────────────
    /// <summary>Default projectile travel speed in m/s.</summary>
    public const float PROJECTILE_SPEED = 15f;

    /// <summary>Seconds before an unfired projectile is auto-destroyed.</summary>
    public const float PROJECTILE_LIFETIME = 6f;

    // ── Enemy energy ─────────────────────────────────────────────────────────
    /// <summary>
    /// Default time in seconds for an enemy to fully recharge from empty.
    /// Used by EnemyPassiveRechargeActor to derive a recharge rate from max_energy.
    /// </summary>
    public const float ENEMY_RECHARGE_TIME = 10f;
}
