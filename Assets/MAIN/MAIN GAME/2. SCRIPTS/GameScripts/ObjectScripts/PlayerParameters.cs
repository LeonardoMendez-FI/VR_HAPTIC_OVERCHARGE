public static class PlayerParameters
{
    public const float MEDIUM_LINEAR_SPEED = 10f;
    public const float MEDIUM_ANGULAR_SPEED = 0.75f;
    public const float JUMP_FORCE_RATIO = 1.5f;
    public const float FLIGHT_ASCEND_FORCE_RATIO = 0.4f;
    public const float GRAVITY = -25f;

    public const float MAX_ENERGY = 100f;
    public const float DRAIN_TIME_FULL = 20f;       // tiempo en segundos para llenar de 0 a 100
    public const float ENERGY_TO_STRUCT = 2f;

    public static float DrainRate => MAX_ENERGY / DRAIN_TIME_FULL; // 5 u/s
}