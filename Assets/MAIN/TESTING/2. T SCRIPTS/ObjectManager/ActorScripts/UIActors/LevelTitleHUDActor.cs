using UnityEngine;

public class LevelTitleHUDActor : UIActor<ManagerScript>
{
    public LevelTitleUI levelTitleUI;

    public void Show(string title, string subtitle) => levelTitleUI?.SetTitle(title, subtitle);

    protected override void Subscribe() { }
    protected override void Unsubscribe() { }
}