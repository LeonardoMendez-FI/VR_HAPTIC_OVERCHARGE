using UnityEngine;
using UnityEngine.UI;

public class MovementModeIconUIView : MonoBehaviour
{
    [Header("References")]
    public Image modeIconImage;
    public MoveManager moveManager;

    [Header("Sprites")]
    public Sprite walkSprite;
    public Sprite flightSprite;

    private void OnEnable()
    {
        if (moveManager != null)
            moveManager.OnFlightModeChanged.AddListener(UpdateMode);
    }

    private void OnDisable()
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