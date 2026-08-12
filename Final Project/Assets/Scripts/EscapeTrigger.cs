using UnityEngine;
using UnityEngine.Events;

public class EscapeTrigger : MonoBehaviour
{
    public UnityEvent onEscaped;
    private bool fired;

    private void OnTriggerEnter(Collider other)
    {
        if (fired || !other.CompareTag("Player")) return;
        fired = true;
        onEscaped?.Invoke();
    }
}