using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.Events;

public class GauntletSelector : MonoBehaviour
{
    [Header("Selection")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private LayerMask interactableLayer;

    [Header("Pull - Launch")]
    [SerializeField] private Transform handTransform;
    [SerializeField] private float launchSpeedScale = 1.2f;
    [SerializeField] private float minLaunchSpeed = 3f;
    [SerializeField] private float maxLaunchSpeed = 9f;
    [SerializeField] private float upwardArc = 0.3f;
    [SerializeField] private float flightSpeed = 6f;

    [Header("Push")]
    [SerializeField] private InputActionReference pushAction;
    [SerializeField] private float pushSpeed = 6f;

    [Header("Pull - Correction")]
    [SerializeField] private float correctionStrength = 15f;
    [SerializeField] private float catchDistance = 0.3f;
    [SerializeField] private float pullTimeout = 3f;

    [Header("Input")]
    [SerializeField] private InputActionReference activateAction;

    [Header("Grab Handoff")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor nearFarInteractor;
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.XRInteractionManager interactionManager;

    [Header("Ray Visual")]
    [SerializeField] private LineRenderer rayLine;
    [SerializeField] private Color rayColorIdle = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color rayColorActive = Color.yellow;

    [Header("Flight Trail")]
    [SerializeField] private GameObject trailPrefab;

    [Header("Suspend")]
    [SerializeField] private InputActionReference suspendAction;

    [Header("Events")]
    public UnityEvent<Rigidbody> onSuspendReleased;


    // State
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable heldInteractable;
    private GameObject currentTarget;
    private Color originalColor;
    private Renderer targetRenderer;

    private Rigidbody pulledObject;
    private float pullStartTime;
    private bool triggerWasPressed = false;
    private GameObject activeTrail;
    private enum GauntletState { Idle, Pulling, Holding}
    private GauntletState state = GauntletState.Idle;

    private Rigidbody suspendedObject;
    private bool suspendWasPressed = false;
    private bool pushWasPressed = false;

    void OnEnable()
    {
        if (activateAction != null) activateAction.action.Enable();
        if (suspendAction != null) suspendAction.action.Enable();
        if (pushAction != null) pushAction.action.Enable();
        if (nearFarInteractor != null)
            nearFarInteractor.selectExited.AddListener(OnInteractorSelectExited);
    }

    private void OnInteractorSelectExited(SelectExitEventArgs args)
    {
        if ((Object)args.interactableObject == heldInteractable)
        {
            heldInteractable = null;
            if (state == GauntletState.Holding) state = GauntletState.Idle;
        }
    }

    void OnDisable()
    {
        if (activateAction != null) activateAction.action.Disable();
        if (suspendAction != null) suspendAction.action.Disable();
        if (pushAction != null) pushAction.action.Disable();
        if (nearFarInteractor != null)
            nearFarInteractor.selectExited.RemoveListener(OnInteractorSelectExited);
    }

    void Update()
    {
        if (state != GauntletState.Pulling)
        {
            UpdateSelection();
        }

        if (state == GauntletState.Pulling && rayLine != null)
        {
            rayLine.enabled = false;
        }

        bool triggerIsPressed = activateAction != null && activateAction.action.IsPressed();

        if (triggerIsPressed && !triggerWasPressed && currentTarget != null && state == GauntletState.Idle)
        {
            LaunchPull();
        }

        if (!triggerIsPressed && triggerWasPressed)
        {
            if (state == GauntletState.Pulling) EndPull();
            else if (state == GauntletState.Holding) ReleaseHeld();
        }

        bool suspendIsPressed = suspendAction != null && suspendAction.action.IsPressed();

        // Press while holding: pin the object
        if (suspendIsPressed && !suspendWasPressed)
        {
            if (state == GauntletState.Holding) SuspendHeld();
            else if (suspendedObject != null) ReleaseSuspend();
        }

        bool pushIsPressed = pushAction != null && pushAction.action.IsPressed();

        if (pushIsPressed && !pushWasPressed && currentTarget != null && state == GauntletState.Idle)
        {
            PushTarget();
        }

        suspendWasPressed = suspendIsPressed;
        triggerWasPressed = triggerIsPressed;
        pushWasPressed = pushIsPressed;
    }

    void FixedUpdate()
    {
        if (state != GauntletState.Pulling || pulledObject == null) return;

        // Timeout safety net
        if (Time.time - pullStartTime > pullTimeout)
        {
            EndPull();
            return;
        }

        Vector3 toHand = handTransform.position - pulledObject.position;
        float distance = toHand.magnitude;

        // Arrival detection
        if (distance < catchDistance)
        {
            CatchObject();
            return;
        }

        // Correction steer velocity toward the hand at a controlled speed
        Vector3 correctionDirection = toHand / distance;
        Vector3 desiredVelocity = correctionDirection * flightSpeed;
        Vector3 velocityError = desiredVelocity - pulledObject.linearVelocity;
        pulledObject.AddForce(velocityError * correctionStrength, ForceMode.Acceleration);
    }

    private void UpdateSelection()
    {
        if (state == GauntletState.Holding)
        {
            ClearHighlight();
            if (rayLine != null) rayLine.enabled = false;
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        bool hitValid = false;
        Vector3 rayEnd = transform.position + transform.forward * maxDistance;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableLayer))
        {
            // ray terminates wherever it physically hit
            rayEnd = hit.point;   

            if (hit.rigidbody != null)
            {
                GameObject hitObject = hit.rigidbody.gameObject;
                var grab = hitObject.GetComponent<XRGrabInteractable>();

                if (grab == null || !grab.isSelected)
                {
                    hitValid = true;
                    if (hitObject != currentTarget)
                    {
                        ClearHighlight();
                        ApplyHighlight(hitObject);
                    }
                }
                else
                {
                    // valid object, but someone's holding it
                    ClearHighlight();   
                }
            }
            else
            {
                ClearHighlight();
            }
        }
        else
        {
            ClearHighlight();
        }

        DrawRay(transform.position, rayEnd, hitValid);
    }

    private void DrawRay(Vector3 start, Vector3 end, bool hitValid)
    {
        if (rayLine == null) return;

        rayLine.enabled = true;
        rayLine.positionCount = 2;
        rayLine.SetPosition(0, start);
        rayLine.SetPosition(1, end);

        Color c = hitValid ? rayColorActive : rayColorIdle;
        rayLine.startColor = c;
        rayLine.endColor = c;
    }

    private void LaunchPull()
    {
        Rigidbody rb = currentTarget.GetComponent<Rigidbody>();
        if (rb == null) return;
        if (rb == suspendedObject)
        {
            rb.isKinematic = false;
            suspendedObject = null;
        }

        pulledObject = rb;
        state = GauntletState.Pulling;
        pullStartTime = Time.time;
        rb.useGravity = false;

        Vector3 toHandVec = handTransform.position - rb.position;
        float distance = toHandVec.magnitude;
        Vector3 toHand = toHandVec / distance;
        Vector3 launchDirection = (toHand + (Vector3.up * upwardArc)).normalized;

        float launchSpeed = Mathf.Clamp(distance * launchSpeedScale, minLaunchSpeed, maxLaunchSpeed);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(launchDirection * launchSpeed, ForceMode.VelocityChange);

        // Spawn the trail and attach it to the object
        if (trailPrefab != null)
        {
            activeTrail = Instantiate(trailPrefab, rb.transform);
            activeTrail.transform.localPosition = Vector3.zero;
        }

        ClearHighlight();
    }

    private void CatchObject()
    {
        if (pulledObject == null) return;

        pulledObject.useGravity = true;
        pulledObject.linearVelocity = Vector3.zero;
        pulledObject.angularVelocity = Vector3.zero;

        var grabInteractable = pulledObject.GetComponent<XRGrabInteractable>();

        if (grabInteractable != null && nearFarInteractor != null && interactionManager != null)
        {
            interactionManager.SelectEnter(
                (IXRSelectInteractor)nearFarInteractor,
                (IXRSelectInteractable)grabInteractable
            );
            heldInteractable = grabInteractable;
            state = GauntletState.Holding;
        }
        else
        {
            state = GauntletState.Idle;
        }

        // Clean up the trail
        if (activeTrail != null)
        {
            var tr = activeTrail.GetComponent<TrailRenderer>();
            activeTrail.transform.SetParent(null);
            if (tr != null) tr.emitting = false;
            Destroy(activeTrail, tr != null ? tr.time : 1f);
            activeTrail = null;
        }

        pulledObject = null;
    }

    private void ReleaseHeld()
    {
        if (heldInteractable != null && nearFarInteractor != null && interactionManager != null)
        {
            interactionManager.SelectExit(
                (IXRSelectInteractor)nearFarInteractor,
                (IXRSelectInteractable)heldInteractable
            );
        }
        heldInteractable = null;
        state = GauntletState.Idle;
    }

    private void EndPull()
    {
        if (pulledObject != null)
        {
            pulledObject.useGravity = true;
        }
       
        if (activeTrail != null)
        {
            var tr = activeTrail.GetComponent<TrailRenderer>();
            activeTrail.transform.SetParent(null);
            if (tr != null) tr.emitting = false;
            Destroy(activeTrail, tr != null ? tr.time : 1f);
            activeTrail = null;
        }
        pulledObject = null;
        state = GauntletState.Idle;
    }

    private void ApplyHighlight(GameObject obj)
    {
        currentTarget = obj;
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
        {     
            targetRenderer = rend;

            if (rend.material.HasProperty("_BaseColor"))
            {
                originalColor = rend.material.GetColor("_BaseColor");
                rend.material.SetColor("_BaseColor", highlightColor);
            }
            else if (rend.material.HasProperty("_Color"))
            {
                originalColor = rend.material.GetColor("_Color");
                rend.material.SetColor("_Color", highlightColor);
            }
        }
    } 

    private void ClearHighlight()
    {
        if (targetRenderer != null)
        {
            if (targetRenderer.material.HasProperty("_BaseColor"))
            {
                targetRenderer.material.SetColor("_BaseColor", originalColor);
            }
            else if (targetRenderer.material.HasProperty("_Color"))
            {
                targetRenderer.material.SetColor("_Color", originalColor);
            }
        }
        currentTarget = null;
        targetRenderer = null;
    }

    void Awake()
    {
        if (handTransform == null) Debug.LogError("GauntletSelector: handTransform not assigned", this);
        if (nearFarInteractor == null) Debug.LogError("GauntletSelector: nearFarInteractor not assigned", this);
        if (interactionManager == null) Debug.LogError("GauntletSelector: interactionManager not assigned", this);
    }

    private void SuspendHeld()
    {
        if (heldInteractable == null) return;

        Rigidbody rb = heldInteractable.GetComponent<Rigidbody>();
        if (rb == null) return;
        state = GauntletState.Idle;
        // Release from hand first, then pin in place
        interactionManager.SelectExit(
            (IXRSelectInteractor)nearFarInteractor,
            (IXRSelectInteractable)heldInteractable
        );
        heldInteractable = null;

        rb.isKinematic = true;
        suspendedObject = rb;
    }

    private void ReleaseSuspend()
    {
        if (suspendedObject != null)
        {
            suspendedObject.isKinematic = false;
            suspendedObject.useGravity = true;
            onSuspendReleased?.Invoke(suspendedObject);
        }
        suspendedObject = null;
    }

    private void PushTarget()
    {
        Rigidbody rb = currentTarget.GetComponent<Rigidbody>();
        if (rb == null) return;

        // Pushing a suspended object un-pins it first
        if (rb == suspendedObject)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            suspendedObject = null;
        }

        Vector3 pushDirection = (rb.position - handTransform.position).normalized;
        rb.AddForce(pushDirection * pushSpeed, ForceMode.VelocityChange);
    }

}