using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TutorialController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text instructionText;
    public GameObject instructionPanel;

    private readonly string[] instructions = new string[]
    {
        "Paso 1: Mantén presionado el botón izquierdo.",
        "Paso 2: Mantén presionado el botón derecho.",
        "Paso 3: La cámara seguirá el movimiento de tu cabeza. Gira la cabeza.",
        "Paso 4: El joystick izquierdo permite rotar al robot. Muévelo a izquierda o derecha.",
        "Paso 5: El joystick derecho permite mover en 8 direcciones. Pruébalo.",
        "Paso 6: El botón derecho permite saltar. Presiónalo para saltar.",
        "Paso 7: Doble click derecho para comenzar a volar.",
        "Paso 8: Asciende con el botón derecho y desciende con el botón izquierdo. Prueba ambos.",
        "Paso 9: Mueve ambos joysticks hacia adelante para avanzar en vuelo.",
        "Paso 10: Mueve los joysticks en sentidos contrarios para rotar en vuelo.",
        "Paso 11: Doble click izquierdo para aterrizar. La energía es importante, se consume al volar.",
        "Paso 12: Busca al enemigo y míralo fijamente para destruirlo.",
        "Paso 13: Bien hecho. Busca el botón para comenzar el juego y míralo fijamente."
    };

    [Header("References")]
    public InputLogic inputLogic;
    public MoveManager moveManager;
    public GazeManager gazeManager;
    public AttackSequenceActor attackSequenceActor;
    public Transform headTransform;
    public PlayerPermissions playerPermissions;

    [Header("Step 12 Target")]
    public GameObject practiceEnemy;

    [Header("Step 13 World Button")]
    public GameObject worldStartButton;

    [Header("Events")]
    public UnityEvent OnTutorialComplete;

    [Header("Settings")]
    public float headRotationThreshold = 15f;
    public float joystickThreshold = 0.5f;
    public float buttonHoldTime = 0.3f;

    private int currentStep = 0;
    private float holdTimer = 0f;
    private bool hasRotatedHead = false;
    private bool enemyDestroyed = false;
    private bool doubleTapRightDetected = false;
    private bool doubleTapLeftDetected = false;
    private bool rightButtonPressed = false;
    private bool leftButtonPressed = false;

    void Start()
    {
        // Validaciones de referencias críticas
        if (headTransform == null)
            Debug.LogError("[TutorialController] headTransform no está asignado. El paso de girar la cabeza nunca se completará.");
        if (playerPermissions == null)
            Debug.LogError("[TutorialController] playerPermissions no está asignado. Los permisos no se modificarán.");

        if (playerPermissions != null)
            playerPermissions.ResetAll();

        if (gazeManager != null) gazeManager.enabled = false;
        if (attackSequenceActor != null) attackSequenceActor.enabled = false;

        if (inputLogic != null)
        {
            inputLogic.OnFlightRequested.AddListener(OnFlightRequestedHandler);
            inputLogic.OnLandRequested.AddListener(OnLandRequestedHandler);
        }

        if (attackSequenceActor != null)
            attackSequenceActor.OnEnemyDestroyed.AddListener(OnEnemyDestroyedHandler);

        ShowStep(0);
    }

    void OnDestroy()
    {
        if (inputLogic != null)
        {
            inputLogic.OnFlightRequested.RemoveListener(OnFlightRequestedHandler);
            inputLogic.OnLandRequested.RemoveListener(OnLandRequestedHandler);
        }
        if (attackSequenceActor != null)
            attackSequenceActor.OnEnemyDestroyed.RemoveListener(OnEnemyDestroyedHandler);
    }

    void Update()
    {
        if (currentStep >= instructions.Length) return;

        switch (currentStep)
        {
            case 0: CheckLeftButton(); break;
            case 1: CheckRightButton(); break;
            case 2: CheckHeadRotation(); break;
            case 3: CheckLeftJoystick(); break;
            case 4: CheckRightJoystick(); break;
            case 5: CheckJump(); break;
            case 6: CheckDoubleTapRight(); break;
            case 7: CheckAscendDescend(); break;
            case 8: CheckBothJoysticksForward(); break;
            case 9: CheckContraryJoysticks(); break;
            case 10: CheckDoubleTapLeft(); break;
            case 11: CheckEnemyDestroyed(); break;
        }
    }

    void CheckLeftButton()
    {
        if (inputLogic == null) return;
        if (inputLogic.LeftButtonHeld)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= buttonHoldTime) AdvanceStep();
        }
        else holdTimer = 0f;
    }

    void CheckRightButton()
    {
        if (inputLogic == null) return;
        if (inputLogic.RightButtonHeld)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= buttonHoldTime) AdvanceStep();
        }
        else holdTimer = 0f;
    }

    void CheckHeadRotation()
    {
        if (headTransform == null) return;
        float yaw = headTransform.localEulerAngles.y;
        float pitch = headTransform.localEulerAngles.x;
        float deltaYaw = Mathf.Abs(Mathf.DeltaAngle(0, yaw));
        float deltaPitch = Mathf.Abs(Mathf.DeltaAngle(0, pitch));
        // Opcional: Debug para ver si el giro se detecta
        // Debug.Log($"[Tutorial] Yaw delta: {deltaYaw:F2}°, Pitch delta: {deltaPitch:F2}°");
        if (deltaYaw > headRotationThreshold || deltaPitch > headRotationThreshold)
            hasRotatedHead = true;

        if (hasRotatedHead) AdvanceStep();
    }

    void CheckLeftJoystick()
    {
        if (inputLogic == null) return;
        if (Mathf.Abs(inputLogic.LeftJoystickRaw.x) > joystickThreshold) AdvanceStep();
    }

    void CheckRightJoystick()
    {
        if (inputLogic == null) return;
        if (inputLogic.RightJoystickRaw.magnitude > joystickThreshold) AdvanceStep();
    }

    void CheckJump()
    {
        if (inputLogic == null) return;
        if (inputLogic.JumpPressed) AdvanceStep();
    }

    void CheckDoubleTapRight()
    {
        if (doubleTapRightDetected) AdvanceStep();
    }

    void CheckDoubleTapLeft()
    {
        if (doubleTapLeftDetected) AdvanceStep();
    }

    void OnFlightRequestedHandler()
    {
        if (currentStep == 6) doubleTapRightDetected = true;
    }

    void OnLandRequestedHandler()
    {
        if (currentStep == 10) doubleTapLeftDetected = true;
    }

    void CheckAscendDescend()
    {
        if (inputLogic == null) return;
        if (inputLogic.RightButtonHeld) rightButtonPressed = true;
        if (inputLogic.LeftButtonHeld) leftButtonPressed = true;
        if (rightButtonPressed && leftButtonPressed) AdvanceStep();
    }

    void CheckBothJoysticksForward()
    {
        if (inputLogic == null || moveManager == null) return;
        if (!moveManager.isFlying) return;
        if (inputLogic.W && inputLogic.UpArrow) AdvanceStep();
    }

    void CheckContraryJoysticks()
    {
        if (inputLogic == null || moveManager == null) return;
        if (!moveManager.isFlying) return;
        bool leftForward_rightBack = inputLogic.W && inputLogic.DownArrow;
        bool leftBack_rightForward = inputLogic.S && inputLogic.UpArrow;
        if (leftForward_rightBack || leftBack_rightForward) AdvanceStep();
    }

    void CheckEnemyDestroyed()
    {
        if (enemyDestroyed) AdvanceStep();
    }

    void OnEnemyDestroyedHandler()
    {
        if (currentStep == 11) enemyDestroyed = true;
    }

    void AdvanceStep()
    {
        holdTimer = 0f;
        currentStep++;
        ApplyPermissionsForStep(currentStep);

        if (currentStep < instructions.Length)
            ShowStep(currentStep);

        if (currentStep == 11 && practiceEnemy != null)
            practiceEnemy.SetActive(true);

        if (currentStep == 12 && worldStartButton != null)
            worldStartButton.SetActive(true);
    }

    void ApplyPermissionsForStep(int step)
    {
        if (playerPermissions == null) return;
        playerPermissions.ResetAll();

        switch (step)
        {
            case 0: case 1: case 2:
                break;
            case 3:
                playerPermissions.canRotate = true;
                break;
            case 4:
                playerPermissions.canRotate = true;
                playerPermissions.canMove = true;
                break;
            case 5:
                playerPermissions.canRotate = true;
                playerPermissions.canMove = true;
                playerPermissions.canJump = true;
                break;
            case 6:
                playerPermissions.canRotate = true;
                playerPermissions.canMove = true;
                playerPermissions.canJump = true;
                playerPermissions.canToggleFlight = true;
                playerPermissions.canLand = false;
                playerPermissions.flightEnergyDrainEnabled = false;
                break;
            case 7:
                playerPermissions.canRotate = true;
                playerPermissions.canMove = true;
                playerPermissions.canJump = true;
                playerPermissions.canToggleFlight = true;
                playerPermissions.canLand = false;
                playerPermissions.flightEnergyDrainEnabled = false;
                break;
            case 8: case 9:
                playerPermissions.canRotate = true;
                playerPermissions.canMove = true;
                playerPermissions.canJump = true;
                playerPermissions.canToggleFlight = true;
                playerPermissions.canLand = false;
                playerPermissions.flightEnergyDrainEnabled = false;
                break;
            case 10:
                playerPermissions.canRotate = true;
                playerPermissions.canMove = true;
                playerPermissions.canJump = true;
                playerPermissions.canToggleFlight = true;
                playerPermissions.canLand = true;
                playerPermissions.flightEnergyDrainEnabled = true;
                break;
            case 11:
                playerPermissions.canRotate = true;
                playerPermissions.canMove = true;
                playerPermissions.canJump = true;
                playerPermissions.canToggleFlight = true;
                playerPermissions.canLand = true;
                playerPermissions.flightEnergyDrainEnabled = true;
                playerPermissions.canGaze = true;
                playerPermissions.canAttack = true;

                if (gazeManager != null) gazeManager.enabled = true;
                if (attackSequenceActor != null) attackSequenceActor.enabled = true;
                break;
            case 12:
                playerPermissions.canRotate = true;
                playerPermissions.canMove = true;
                playerPermissions.canJump = true;
                playerPermissions.canToggleFlight = true;
                playerPermissions.canLand = true;
                playerPermissions.flightEnergyDrainEnabled = true;
                playerPermissions.canGaze = true;
                playerPermissions.canAttack = true;

                if (gazeManager != null) gazeManager.enabled = true;
                if (attackSequenceActor != null) attackSequenceActor.enabled = true;
                break;
        }
    }

    void ShowStep(int index)
    {
        if (instructionPanel != null) instructionPanel.SetActive(true);
        if (instructionText != null) instructionText.text = instructions[index];
    }

    public void HideInstructions()
    {
        if (instructionPanel != null) instructionPanel.SetActive(false);
    }
}