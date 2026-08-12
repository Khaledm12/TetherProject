using UnityEngine;

public class RespawnZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null) return;

        var respawnable = other.attachedRigidbody.GetComponent<Respawnable>();
        if (respawnable != null)
            respawnable.Respawn();
    }
}
