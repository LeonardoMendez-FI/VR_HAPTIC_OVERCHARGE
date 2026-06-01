using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;                     // panel raíz (inicia desactivado)
    public TMP_Text currentStatsText;
    public TMP_Text bestStatsText;

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
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}