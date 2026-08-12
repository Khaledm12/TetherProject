using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Respawnable : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Rigidbody body;
    private XRGrabInteractable interactable;

    void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        body = GetComponent<Rigidbody>();
        interactable = GetComponent<XRGrabInteractable>();
    }

    public void Respawn()
    {
        if (interactable != null && interactable.isSelected) return;   
        if (body != null && body.isKinematic) return;                 

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        transform.SetPositionAndRotation(initialPosition, initialRotation);
    }
}