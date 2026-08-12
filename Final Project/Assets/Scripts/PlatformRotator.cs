using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PlatformRotator : XRBaseInteractable
{
    [Header("Rotation")]
    [SerializeField] private Transform platform;        // the rotating top mesh
    [SerializeField] private float angleIncrement = 15f; // 0 = smooth

    private IXRSelectInteractor grabbingInteractor;
    private float lastHandAngle;
    private float accumulatedAngle;   // un-quantised true angle

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        grabbingInteractor = args.interactorObject;
        lastHandAngle = GetHandAngle();
        accumulatedAngle = platform.localEulerAngles.y;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        grabbingInteractor = null;
    }

    void Update()
    {
        if (grabbingInteractor == null) return;

        float handAngle = GetHandAngle();
        float delta = Mathf.DeltaAngle(lastHandAngle, handAngle);
        lastHandAngle = handAngle;

        accumulatedAngle += delta;

        float applied = angleIncrement > 0f
            ? Mathf.Round(accumulatedAngle / angleIncrement) * angleIncrement
            : accumulatedAngle;

        platform.localEulerAngles = new Vector3(0f, applied, 0f);
    }

    private float GetHandAngle()
    {
        Vector3 local = transform.InverseTransformPoint(
            grabbingInteractor.GetAttachTransform(this).position);
        return Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
    }
}