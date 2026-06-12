public static class PlayerParameters
{
    // ── Player locomotion ───────────────────────────────────────────────────
    public const float MEDIUM_LINEAR_SPEED   = 10f;
    public const float MEDIUM_ANGULAR_SPEED  = 0.75f;
    public const float JUMP_FORCE_RATIO      = 1.5f;
    public const float FLIGHT_ASCEND_FORCE_RATIO = 0.4f;
    public const float GRAVITY               = -25f;

    // ── Energy ──────────────────────────────────────────────────────────────
    public const float MAX_ENERGY        = 100f;
    public const float DRAIN_TIME_FULL   = 20f;
    public const float ENERGY_TO_STRUCT  = 1f;
    public static float DrainRate => MAX_ENERGY / DRAIN_TIME_FULL;

    // ── Enemy locomotion base multipliers ───────────────────────────────────
    public const float ENEMY_BASE_SPEED_MULTIPLIER   = 0.8f;
    public const float ENEMY_BASE_ANGULAR_SPEED_DEG  = 120f;

    // ── Enemy detection ──────────────────────────────────────────────────────
    public const float ENEMY_DETECTION_TIME  = 15f;
    public const float ENEMY_LOSE_TIME       = 30f;

    // ── Enemy projectiles ────────────────────────────────────────────────────
    public const float PROJECTILE_SPEED    = 10f;
    public const float PROJECTILE_LIFETIME = 5f;

    // ── Enemy energy ─────────────────────────────────────────────────────────
    public const float ENEMY_RECHARGE_TIME = 10f;

    // ── Enemy attack ranges ───────────────────────────────────────────────────
    public const float ENEMY_MELEE_TIME    = 2f;
    public const float ENEMY_RANGED_TIME   = 15f;

    // ── Enemy damage ─────────────────────────────────────────────────────────
    public const float ENEMY_BASE_MELEE_DAMAGE  = 10f;
    public const float ENEMY_BASE_RANGED_DAMAGE = 8f;
}