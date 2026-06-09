using UnityEngine;
using UnityEngine.UI;

public class JoystickDebugIconView : ViewBase
{
    [Header("References")]
    public Image joystickImage;
    public InputLogic inputLogic;       // actualizado a InputLogic

    [Header("Joystick Side")]
    public bool isLeftJoystick = true;

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite forwardSprite;
    public Sprite backwardSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    [Header("Button Colors")]
    public Color defaultColor = new Color(0.36f, 0.86f, 0.48f);
    public Color pressedColor = new Color(0.86f, 0.36f, 0.41f);

    private void Update()
    {
        if (joystickImage == null || inputLogic == null) return;

        Vector2 input = isLeftJoystick ? inputLogic.LeftJoystickRaw : inputLogic.RightJoystickRaw;
        bool buttonPressed = isLeftJoystick ? inputLogic.LeftButtonHeld : inputLogic.RightButtonHeld;

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

    // No necesita suscripciones, pero implementamos los métodos abstractos vacíos
    protected override void Subscribe() { }
    protected override void Unsubscribe() { }
}