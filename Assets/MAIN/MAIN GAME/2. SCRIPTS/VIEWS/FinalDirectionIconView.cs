using UnityEngine;
using UnityEngine.UI;

public class FinalDirectionIconView : ViewBase
{
    [Header("References")]
    public Image directionImage;
    public MoveManager moveManager;
    public InputLogic inputLogic;   // actualizado

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite forwardSprite, forwardRightSprite, rightSprite, backRightSprite, backSprite, backLeftSprite, leftSprite, forwardLeftSprite;
    public Sprite rotateLeftSprite, rotateRightSprite;
    public Sprite forwardRotateLeftSprite, forwardRotateRightSprite, backwardRotateLeftSprite, backwardRotateRightSprite;

    [Header("Threshold")]
    public float threshold = 0.3f;

    private void Update()
    {
        if (directionImage == null || moveManager == null || inputLogic == null) return;

        Sprite chosen = moveManager.isFlying ? GetFlightSprite() : GetGroundSprite();
        directionImage.sprite = chosen != null ? chosen : idleSprite;
    }

    Sprite GetGroundSprite()
    {
        float moveX = 0f, moveZ = 0f, rot = 0f;
        if (inputLogic.UpArrow)    moveZ += 1f;
        if (inputLogic.DownArrow)  moveZ -= 1f;
        if (inputLogic.RightArrow) moveX += 1f;
        if (inputLogic.LeftArrow)  moveX -= 1f;
        if (inputLogic.D) rot += 1f;
        if (inputLogic.A) rot -= 1f;

        bool hasMovement = Mathf.Abs(moveX) >= threshold || Mathf.Abs(moveZ) >= threshold;
        bool hasRotation = Mathf.Abs(rot) >= threshold;

        if (hasMovement && hasRotation)
        {
            if (moveZ > threshold)
                return rot > 0 ? forwardRotateRightSprite : forwardRotateLeftSprite;
            if (moveZ < -threshold)
                return rot > 0 ? backwardRotateRightSprite : backwardRotateLeftSprite;
            return rot > 0 ? rotateRightSprite : rotateLeftSprite;
        }
        if (hasRotation)
            return rot > 0 ? rotateRightSprite : rotateLeftSprite;
        if (hasMovement)
        {
            float angle = Mathf.Atan2(moveX, moveZ) * Mathf.Rad2Deg;
            if (angle >= -22.5f && angle < 22.5f) return forwardSprite;
            if (angle >= 22.5f && angle < 67.5f) return forwardRightSprite;
            if (angle >= 67.5f && angle < 112.5f) return rightSprite;
            if (angle >= 112.5f && angle < 157.5f) return backRightSprite;
            if (angle >= 157.5f || angle < -157.5f) return backSprite;
            if (angle >= -157.5f && angle < -112.5f) return backLeftSprite;
            if (angle >= -112.5f && angle < -67.5f) return leftSprite;
            return forwardLeftSprite;
        }
        return null;
    }

    Sprite GetFlightSprite()
    {
        bool w = inputLogic.W, up = inputLogic.UpArrow;
        bool s = inputLogic.S, down = inputLogic.DownArrow;
        bool a = inputLogic.A, left = inputLogic.LeftArrow;
        bool d = inputLogic.D, right = inputLogic.RightArrow;

        float moveX = 0f, moveZ = 0f, torque = 0f;

        if (w && !up && !s && !down) { moveZ = 1f; torque = 1f; }
        else if (!w && up && !s && !down) { moveZ = 1f; torque = -1f; }
        else if (!w && !up && s && !down) { moveZ = -1f; torque = -1f; }
        else if (!w && !up && !s && down) { moveZ = -1f; torque = 1f; }
        else if (w && up && !s && !down) { moveZ = 1f; torque = 0f; }
        else if (!w && !up && s && down) { moveZ = -1f; torque = 0f; }
        else if (w && !up && !s && down) { moveZ = 0f; torque = 1f; }
        else if (!w && up && s && !down) { moveZ = 0f; torque = -1f; }
        else if (w && s) { moveZ = 0f; torque = 0f; }

        if (a && !d && !right) moveX = -1f;
        else if (!a && d && !left) moveX = 1f;
        else if (left && !a && !d && !right) moveX = -1f;
        else if (right && !d && !a && !left) moveX = 1f;
        if (a && left) moveX = -2f;
        if (d && right) moveX = 2f;
        if ((a && right) || (d && left)) moveX = 0f;

        bool hasMovement = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f;
        bool hasRotation = Mathf.Abs(torque) > 0.1f;

        if (hasMovement && hasRotation)
        {
            if (Mathf.Abs(moveX) > 0.1f && Mathf.Abs(moveZ) < 0.1f)
                return torque > 0 ? rotateRightSprite : rotateLeftSprite;
            if (moveZ > 0.1f)
                return torque > 0 ? forwardRotateRightSprite : forwardRotateLeftSprite;
            if (moveZ < -0.1f)
                return torque > 0 ? backwardRotateRightSprite : backwardRotateLeftSprite;
            return torque > 0 ? rotateRightSprite : rotateLeftSprite;
        }
        if (hasRotation)
            return torque > 0 ? rotateRightSprite : rotateLeftSprite;
        if (hasMovement)
        {
            if (moveZ > 0.1f) return forwardSprite;
            if (moveZ < -0.1f) return backSprite;
            if (moveX > 0.1f) return rightSprite;
            if (moveX < -0.1f) return leftSprite;
        }
        return null;
    }

    protected override void Subscribe() { }
    protected override void Unsubscribe() { }
}