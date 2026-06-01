using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountersUI : MonoBehaviour
{
    [SerializeField] TMP_Text eliminationLabel;
    [SerializeField] TMP_Text eliminationCount;
    [SerializeField] TMP_Text objectiveLabel;
    [SerializeField] TMP_Text objectiveCount;
    [SerializeField] Color labelColor = new Color(0, 0.92f, 1, 0.75f);
    [SerializeField] Color valueColor = Color.white;
    [SerializeField] Color pulseColor = new Color(1, 0.3f, 0, 1);

    int currentEliminations;
    int currentObjectives;

    void Awake()
    {
        if (eliminationLabel) { eliminationLabel.text = "ELIMINATED"; eliminationLabel.color = labelColor; }
        if (objectiveLabel) { objectiveLabel.text = "REMAINING"; objectiveLabel.color = labelColor; }
        SetEliminations(0);
        SetObjectives(0);
    }

    public void SetEliminations(int count)
    {
        currentEliminations = count;
        if (eliminationCount) { eliminationCount.text = count.ToString("D3"); eliminationCount.color = valueColor; StartCoroutine(PulseText(eliminationCount)); }
    }

    public void SetObjectives(int remaining)
    {
        currentObjectives = remaining;
        if (objectiveCount) { objectiveCount.text = remaining.ToString("D3"); objectiveCount.color = remaining == 0 ? pulseColor : valueColor; }
    }

    IEnumerator PulseText(TMP_Text t)
    {
        Color original = t.color;
        t.color = pulseColor;
        yield return new WaitForSeconds(0.25f);
        t.color = original;
    }
}