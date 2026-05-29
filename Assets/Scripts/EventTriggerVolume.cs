using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class EventTriggerVolume : MonoBehaviour
{
    [Header("═══════════════ EVENT TRIGGER ═══════════════")]
    public string eventName = "Nowy Event";
    public bool triggerOnce = true;
    public float delayBeforeTrigger = 0f;

    [Header("═══════════════ TRIGGER CONDITIONS ═══════════════")]
    public bool requireTimeThreshold = false;
    public float requiredTime = 0f;

    [Header("═══════════════ ACTIONS ═══════════════")]
    public UnityEvent onTrigger;

    private bool hasTriggered = false;
    private LevelSystem levelSystem;

    void Start()
    {
        FindLevelSystem();
    }

    void FindLevelSystem()
    {
        levelSystem = FindFirstObjectByType<LevelSystem>();

        if (levelSystem == null)
        {
            GameObject gm = GameObject.Find("GameManager");
            if (gm != null)
                levelSystem = gm.GetComponent<LevelSystem>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;
        if (!other.CompareTag("Player")) return;

        if (requireTimeThreshold && levelSystem != null)
        {
            if (levelSystem.gameTime < requiredTime) return;
        }

        StartCoroutine(TriggerAfterDelay());
    }

    IEnumerator TriggerAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeTrigger);
        onTrigger?.Invoke();
        hasTriggered = true;
        Debug.Log("Event wyzwolony: " + eventName);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.DrawWireCube(transform.position, box.bounds.size);
        }
    }
}