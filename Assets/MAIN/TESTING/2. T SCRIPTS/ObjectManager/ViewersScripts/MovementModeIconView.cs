using UnityEngine;
using UnityEngine.UI;

public class MovementModeIconView : ViewBase
{
    [Header("References")]
    public Image modeIconImage;
    public MoveManager moveManager;

    [Header("Sprites")]
    public Sprite walkSprite;
    public Sprite flightSprite;

    protected override void Subscribe()
    {
        if (moveManager != null)
            moveManager.OnFlightModeChanged.AddListener(UpdateMode);
    }

    protected override void Unsubscribe()
    {
        if (moveManager != null)
            moveManager.OnFlightModeChanged.RemoveListener(UpdateMode);
    }

    private void UpdateMode(bool isFlying)
    {
        if (modeIconImage == null) return;
        modeIconImage.sprite = isFlying ? flightSprite : walkSprite;
    }
}