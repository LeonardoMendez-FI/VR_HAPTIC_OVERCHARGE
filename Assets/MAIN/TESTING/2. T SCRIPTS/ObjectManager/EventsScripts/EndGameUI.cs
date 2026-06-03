using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TMP_Text currentStatsText;
    public TMP_Text bestStatsText;
    public GameSessionData gameSessionData;

    private string FormatRun(RunRecord r)
    {
        return $"Robots destruidos: {r.robotsDestroyed}\n" +
               $"Daño infligido: {r.damageDealt:F1}\n" +
               $"Daño recibido: {r.damageReceived:F1}\n" +
               $"Energía absorbida: {r.energyAbsorbed:F1}\n" +
               $"Nivel alcanzado: {r.levelReached}\n" +
               $"Victoria: {(r.victory ? "Sí" : "No")}";
    }

    public void Show(RunRecord current, RunRecord best)
    {
        currentStatsText.text = "─── RUN ACTUAL ───\n" + FormatRun(current);
        bestStatsText.text = "─── MEJOR RUN ───\n" + FormatRun(best);
        panel.SetActive(true);

        // Pausar sistemas de juego (evita daños post-mortem)
        if (GamePauseController.Instance != null)
            GamePauseController.Instance.PauseGame();
    }

    public void RestartGame()
    {
        // Resetear sesión antes de recargar
        if (gameSessionData != null)
            gameSessionData.ResetData();

        // Reanudar (por si acaso, aunque la escena se recargará)
        if (GamePauseController.Instance != null)
            GamePauseController.Instance.ResumeGame();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}