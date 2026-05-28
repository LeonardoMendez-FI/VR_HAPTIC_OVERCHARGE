using UnityEngine;

public static class PlayerParameters
{
    // Valores MEDIUM (100% = jugador en suelo)
    public const float MEDIUM_LINEAR_SPEED = 4f;      // 40 * 0.1
    public const float MEDIUM_ANGULAR_SPEED = 0.7f;   // 35 * 0.02

    // Ratios de fuerzas derivadas (proporciones respecto a MEDIUM_LINEAR_SPEED)
    public const float JUMP_FORCE_RATIO = 1.75f;      // 7 / 4
    public const float FLIGHT_ASCEND_FORCE_RATIO = 0.25f; // 1 / 4

    // Energía
    public const float MAX_ENERGY = 100f;
    public const float DRAIN_TIME_FULL = 20f;         // tiempo en llenar 0→100%
    public const float ENERGY_TO_STRUCT = 1f;         // relación energía → vida

    // Tasa de drenado calculada
    public static float DrainRate => MAX_ENERGY / DRAIN_TIME_FULL; // 5 u/s
}