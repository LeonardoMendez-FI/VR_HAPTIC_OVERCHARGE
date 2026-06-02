using UnityEngine;
using UnityEngine.UI;

public class FinalDirectionIconView : MonoBehaviour
{
    [Header("References")]
    public Image directionImage;
    public MoveManager moveManager;
    public InputManager inputManager;

    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite forwardSprite, forwardRightSprite, rightSprite, backRightSprite, backSprite, backLeftSprite, leftSprite, forwardLeftSprite;
    public Sprite rotateLeftSprite, rotateRightSprite;
    public Sprite forwardRotateLeftSprite, forwardRotateRightSprite, backwardRotateLeftSprite, backwardRotateRightSprite;

    [Header("Threshold")]
    public float threshold = 0.3f;

    private void Update()
    {
        if (directionImage == null || moveManager == null || inputManager == null) return;

        Sprite chosen = moveManager.isFlying ? GetFlightSprite() : GetGroundSprite();
        directionImage.sprite = chosen != null ? chosen : idleSprite;
    }

    Sprite GetGroundSprite()
    {
        Vector2 move = inputManager.MoveInput;
        float rot = inputManager.GroundRotInput;
        bool hasMovement = move.magnitude >= threshold;
        bool hasRotation = Mathf.Abs(rot) >= threshold;

        if (hasMovement && hasRotation)
        {
            if (move.y > threshold)
                return rot > 0 ? forwardRotateRightSprite : forwardRotateLeftSprite;
            if (move.y < -threshold)
                return rot > 0 ? backwardRotateRightSprite : backwardRotateLeftSprite;
            return rot > 0 ? rotateRightSprite : rotateLeftSprite;
        }
        if (hasRotation)
            return rot > 0 ? rotateRightSprite : rotateLeftSprite;
        if (hasMovement)
        {
            float angle = Mathf.Atan2(move.x, move.y) * Mathf.Rad2Deg;
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
        bool w = inputManager.W, up = inputManager.UpArrow;
        bool s = inputManager.S, down = inputManager.DownArrow;
        bool a = inputManager.A, left = inputManager.LeftArrow;
        bool d = inputManager.D, right = inputManager.RightArrow;

        float moveX = 0f, moveZ = 0f, torque = 0f;

        // Lógica de propulsión idéntica al FlightPropulsionActor
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
}