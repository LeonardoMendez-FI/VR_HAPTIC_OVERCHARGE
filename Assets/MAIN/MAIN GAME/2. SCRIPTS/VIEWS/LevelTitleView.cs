using TMPro;
using UnityEngine;

public class LevelTitleView : ViewBase
{
    public TMP_Text titleText;
    public LevelService levelService;   // renombrado

    protected override void Subscribe()
    {
        if (levelService != null)
            levelService.OnLevelTitleChanged.AddListener(UpdateTitle);
    }

    protected override void Unsubscribe()
    {
        if (levelService != null)
            levelService.OnLevelTitleChanged.RemoveListener(UpdateTitle);
    }

    private void UpdateTitle(string newTitle)
    {
        if (titleText != null)
            titleText.text = newTitle;
    }
}