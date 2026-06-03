using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TutorialController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text instructionText;
    public GameObject instructionPanel;   // antes CanvasGroup

    [Header("Instructions (in order)")]
    public string[] instructions = new string[]
    {
        "Paso 1: Presiona el botón izquierdo.",
        "Paso 2: Presiona el botón derecho.",
        "Paso 3: La cámara seguirá el movimiento de tu cabeza. Gira la cabeza.",
        "Paso 4: El joystick izquierdo permite rotar al robot. Muévelo a izquierda o derecha.",
        "Paso 5: El joystick derecho permite mover en 8 direcciones. Pruébalo.",
        "Paso 6: El botón derecho permite saltar. Presiónalo para saltar.",
        "Paso 7: Doble click para comenzar a volar. La energía es importante. Activa el vuelo.",
        "Paso 8: Mueve ambos joysticks hacia adelante para avanzar.",
        "Paso 9: Mueve los joysticks en sentidos contrarios para rotar en vuelo.",
        "Paso 10: Observa un objeto y destrúyelo.",
        "Paso 11: Bien hecho. Busca el botón para comenzar el juego y míralo fijamente."
    };

    [Header("References")]
    public InputLogic inputLogic;
    public MoveManager moveManager;
    public GazeManager gazeManager;
    public AttackSequenceActor attackSequenceActor;
    public Transform headTransform;

    [Header("Step 10 Target")]
    public GameObject practiceEnemy;

    [Header("Step 11 World Button")]
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
    private bool doubleTapDetected = false;

    void Start()
    {
        if (inputLogic != null)
            inputLogic.OnFlightRequested.AddListener(OnFlightRequestedHandler);

        if (attackSequenceActor != null)
            attackSequenceActor.OnEnemyDestroyed.AddListener(OnEnemyDestroyedHandler);

        ShowStep(0);
    }

    void OnDestroy()
    {
        if (inputLogic != null)
            inputLogic.OnFlightRequested.RemoveListener(OnFlightRequestedHandler);
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
            case 6: CheckDoubleTapFlight(); break;
            case 7: CheckBothJoysticksForward(); break;
            case 8: CheckContraryJoysticks(); break;
            case 9: CheckEnemyDestroyed(); break;
        }
    }

    void CheckLeftButton()
    {
        if (inputLogic == null) return;
        if (inputLogic.LeftButtonHeld)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= buttonHoldTime)
                AdvanceStep();
        }
        else holdTimer = 0f;
    }

    void CheckRightButton()
    {
        if (inputLogic == null) return;
        if (inputLogic.RightButtonHeld)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= buttonHoldTime)
                AdvanceStep();
        }
        else holdTimer = 0f;
    }

    void CheckHeadRotation()
    {
        if (headTransform == null) return;

        float yaw = headTransform.localEulerAngles.y;
        float pitch = headTransform.localEulerAngles.x;

        if (Mathf.Abs(yaw) > headRotationThreshold || Mathf.Abs(pitch) > headRotationThreshold)
            hasRotatedHead = true;

        if (hasRotatedHead)
            AdvanceStep();
    }

    void CheckLeftJoystick()
    {
        if (inputLogic == null) return;
        if (Mathf.Abs(inputLogic.LeftJoystickRaw.x) > joystickThreshold)
            AdvanceStep();
    }

    void CheckRightJoystick()
    {
        if (inputLogic == null) return;
        if (inputLogic.RightJoystickRaw.magnitude > joystickThreshold)
            AdvanceStep();
    }

    void CheckJump()
    {
        if (inputLogic == null) return;
        if (inputLogic.JumpPressed)
            AdvanceStep();
    }

    void CheckDoubleTapFlight()
    {
        if (doubleTapDetected)
            AdvanceStep();
    }

    void OnFlightRequestedHandler()
    {
        if (currentStep == 6)
            doubleTapDetected = true;
    }

    void CheckBothJoysticksForward()
    {
        if (inputLogic == null || moveManager == null) return;
        if (!moveManager.isFlying) return;

        if (inputLogic.W && inputLogic.UpArrow)
            AdvanceStep();
    }

    void CheckContraryJoysticks()
    {
        if (inputLogic == null || moveManager == null) return;
        if (!moveManager.isFlying) return;

        bool leftForward_rightBack = inputLogic.W && inputLogic.DownArrow;
        bool leftBack_rightForward = inputLogic.S && inputLogic.UpArrow;

        if (leftForward_rightBack || leftBack_rightForward)
            AdvanceStep();
    }

    void CheckEnemyDestroyed()
    {
        if (enemyDestroyed)
            AdvanceStep();
    }

    void OnEnemyDestroyedHandler()
    {
        if (currentStep == 9)
            enemyDestroyed = true;
    }

    void AdvanceStep()
    {
        holdTimer = 0f;
        currentStep++;

        if (currentStep < instructions.Length)
            ShowStep(currentStep);

        if (currentStep == 9 && practiceEnemy != null)
            practiceEnemy.SetActive(true);

        if (currentStep == 10 && worldStartButton != null)
            worldStartButton.SetActive(true);
    }

    void ShowStep(int index)
    {
        if (instructionPanel != null)
            instructionPanel.SetActive(true);

        if (instructionText != null)
            instructionText.text = instructions[index];
    }

    public void HideInstructions()
    {
        if (instructionPanel != null)
            instructionPanel.SetActive(false);
    }
}