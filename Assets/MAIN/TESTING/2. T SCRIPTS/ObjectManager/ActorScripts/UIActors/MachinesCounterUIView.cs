using TMPro;
using UnityEngine;

public class MachinesCounterUIView : MonoBehaviour
{
    public TMP_Text counterText;
    public LevelManager levelManager;

    private void OnEnable()
    {
        if (levelManager != null)
            levelManager.OnMachinesRemainingChanged.AddListener(UpdateCount);
    }

    private void OnDisable()
    {
        if (levelManager != null)
            levelManager.OnMachinesRemainingChanged.RemoveListener(UpdateCount);
    }

    private void UpdateCount(int remaining)
    {
        if (counterText != null)
            counterText.text = $"x{remaining}";
    }
}