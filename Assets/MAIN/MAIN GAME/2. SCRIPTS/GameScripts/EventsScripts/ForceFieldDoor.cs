using UnityEngine;

public class ForceFieldDoor : MonoBehaviour
{
    [Header("Level Service")]
    public LevelService levelService;           // arrastrar el LevelService de la escena

    [Header("Door Components")]
    public Collider doorCollider;               // collider que bloquea el paso
    public GameObject forceFieldEffect;         // objeto con el shader de campo de fuerza

    void Start()
    {
        if (levelService != null)
        {
            levelService.OnMachinesRemainingChanged.AddListener(OnMachinesRemainingChanged);
            // Verificar estado inicial por si ya no hay máquinas al cargar la escena
            OnMachinesRemainingChanged(levelService.machines != null ? levelService.machines.Length : 0);
        }
    }

    void OnDestroy()
    {
        if (levelService != null)
            levelService.OnMachinesRemainingChanged.RemoveListener(OnMachinesRemainingChanged);
    }

    void OnMachinesRemainingChanged(int remaining)
    {
        if (remaining <= 0)
        {
            if (doorCollider != null) doorCollider.enabled = false;
            if (forceFieldEffect != null) forceFieldEffect.SetActive(false);
        }
    }
}