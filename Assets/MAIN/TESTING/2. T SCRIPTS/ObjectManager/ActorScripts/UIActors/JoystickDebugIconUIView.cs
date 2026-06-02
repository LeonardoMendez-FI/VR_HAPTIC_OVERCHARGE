using UnityEngine;
using UnityEngine.UI;

public class JoystickDebugIconView : MonoBehaviour
{
    [Header("References")]
    public Image joystickImage;
    public InputManager inputManager;      // Única dependencia

    [Header("Joystick Side")]
    public bool isLeftJoystick = true;     // true = joystick izquierdo, false = derecho

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite forwardSprite;
    public Sprite backwardSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    [Header("Button Colors")]
    public Color defaultColor = new Color(0.36f, 0.86f, 0.48f);   // #5BDC7A
    public Color pressedColor = new Color(0.86f, 0.36f, 0.41f);   // #DC5B69

    private void Update()
    {
        if (joystickImage == null || inputManager == null) return;

        // Obtener el vector del joystick correspondiente
        Vector2 input = isLeftJoystick ? inputManager.LeftJoystickRaw : inputManager.RightJoystickRaw;
        bool buttonPressed = isLeftJoystick ? inputManager.LeftButtonHeld : inputManager.RightButtonHeld;

        // Seleccionar sprite según eje dominante (umbral 0.3)
        Sprite chosenSprite = idleSprite;
        float threshold = 0.3f;
        if (input.magnitude >= threshold)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                chosenSprite = input.x > 0 ? rightSprite : leftSprite;
            else
                chosenSprite = input.y > 0 ? forwardSprite : backwardSprite;
        }

        joystickImage.sprite = chosenSprite;
        joystickImage.color = buttonPressed ? pressedColor : defaultColor;
    }
}