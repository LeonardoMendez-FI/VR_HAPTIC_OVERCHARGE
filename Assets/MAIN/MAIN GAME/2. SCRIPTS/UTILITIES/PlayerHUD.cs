using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [Header("HUD Root")]
    [Tooltip("El GameObject que contiene todos los elementos del HUD (barras, iconos, textos).")]
    public GameObject hudRoot;

    public void ShowHUD()
    {
        if (hudRoot != null)
            hudRoot.SetActive(true);
    }

    public void HideHUD()
    {
        if (hudRoot != null)
            hudRoot.SetActive(false);
    }
}