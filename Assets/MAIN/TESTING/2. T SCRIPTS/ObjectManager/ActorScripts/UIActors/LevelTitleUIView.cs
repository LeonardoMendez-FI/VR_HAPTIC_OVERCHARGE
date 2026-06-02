using TMPro;
using UnityEngine;

public class LevelTitleUIView : MonoBehaviour
{
    public TMP_Text titleText;
    public LevelManager levelManager;

    private void OnEnable()
    {
        if (levelManager != null)
            levelManager.OnLevelTitleChanged.AddListener(UpdateTitle);
    }

    private void OnDisable()
    {
        if (levelManager != null)
            levelManager.OnLevelTitleChanged.RemoveListener(UpdateTitle);
    }

    private void UpdateTitle(string newTitle)
    {
        if (titleText != null)
            titleText.text = newTitle;
    }
}