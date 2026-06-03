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
               $"Máquinas destruidas: {r.machinesDestroyed}\n" +
               $"Daño infligido: {r.damageDealt:F1}\n" +
               $"Daño recibido: {r.damageReceived:F1}\n" +
               $"Energía absorbida: {r.energyAbsorbed:F1}\n" +
               $"Nivel alcanzado: {r.levelReached}\n" +
               $"Tiempo de juego: {r.gameTime:F1} s\n" +
               $"Puntuación: {r.score:F0}\n" +
               $"Victoria: {(r.victory ? "Sí" : "No")}";
    }

    public void Show(RunRecord current, RunRecord best)
    {
        currentStatsText.text = "─── PARTIDA ACTUAL ───\n" + FormatRun(current);
        bestStatsText.text = "─── MEJOR PUNTUACIÓN ───\n" + FormatRun(best);
        panel.SetActive(true);

        if (GamePauseController.Instance != null)
            GamePauseController.Instance.PauseGame();
    }

    public void RestartGame()
    {
        if (gameSessionData != null)
            gameSessionData.ResetData();

        if (GamePauseController.Instance != null)
            GamePauseController.Instance.ResumeGame();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}