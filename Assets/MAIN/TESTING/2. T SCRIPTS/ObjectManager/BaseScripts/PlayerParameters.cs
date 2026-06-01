using UnityEngine;

public static class PlayerParameters
{
    // Valores MEDIUM (100% = jugador en suelo)
    public const float MEDIUM_LINEAR_SPEED = 10f;      // 40 * 0.1
    public const float MEDIUM_ANGULAR_SPEED = 0.75f;   // 35 * 0.02

    // Ratios de fuerzas derivadas (proporciones respecto a MEDIUM_LINEAR_SPEED)
    public const float JUMP_FORCE_RATIO = 1.5f;      // 7 / 4
    public const float FLIGHT_ASCEND_FORCE_RATIO = 0.4f; // 1 / 4

    public const float GRAVITY = -25f;   // m/s², negativo porque apunta hacia abajo

    // Energía
    public const float MAX_ENERGY = 100f;
    public const float DRAIN_TIME_FULL = 20f;         // tiempo en llenar 0→100%
    public const float ENERGY_TO_STRUCT = 1f;         // relación energía → vida

    // Tasa de drenado calculada
    public static float DrainRate => MAX_ENERGY / DRAIN_TIME_FULL; // 5 u/s
}