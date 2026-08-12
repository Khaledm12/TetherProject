using UnityEngine;
using UnityEngine.Events;
using Unity.VRTemplate;

public class ContainmentField : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XRKnob dial;
    [SerializeField] private GauntletSelector gauntlet;
    [SerializeField] private Renderer gaugeBar;      
    [SerializeField] private Collider fieldVolume;   

    [Header("Charge")]
    [SerializeField] private float chargeRate = 0.5f;   
    [SerializeField] private float decayRate = 0.4f;    
    [SerializeField] private float bandMin = 0.65f;
    [SerializeField] private float bandMax = 0.85f;

    [Header("Feedback")]
    [SerializeField] private Color inBandColor = Color.cyan;
    [SerializeField] private Color outOfBandColor = Color.red;

    [Header("Events")]
    public UnityEvent onActivated;   

    private float charge;
    private bool solved;
    private Vector3 gaugeFullScale;

    void OnEnable()
    {
        if (gauntlet != null)
            gauntlet.onSuspendReleased.AddListener(OnCoreReleased);
    }

    void OnDisable()
    {
        if (gauntlet != null)
            gauntlet.onSuspendReleased.RemoveListener(OnCoreReleased);
    }

    void Awake()
    {
        if (gaugeBar != null)
            gaugeFullScale = gaugeBar.transform.localScale;
    }

    void Update()
    {
        if (solved) return;

        if (dial.isSelected)
            charge = Mathf.MoveTowards(charge, dial.value, chargeRate * Time.deltaTime);
        else
            charge = Mathf.MoveTowards(charge, 0f, decayRate * Time.deltaTime);

        UpdateGauge();
    }

    private bool InBand => charge >= bandMin && charge <= bandMax;

    private void UpdateGauge()
    {
        if (gaugeBar == null) return;

        // Bar fill: scale X by charge
        Vector3 s = gaugeFullScale;
        s.x = gaugeFullScale.x * charge;
        gaugeBar.transform.localScale = s;

        Color c = InBand ? inBandColor : outOfBandColor;
        gaugeBar.material.SetColor("_EmissionColor", c);
    }

    private void OnCoreReleased(Rigidbody released)
    {
        if (solved) return;
        if (!released.CompareTag("PowerCore")) return;
        if (!fieldVolume.bounds.Contains(released.position)) return;
        if (!InBand) return;   // wrong moment: core just drops, no punishment

        solved = true;
        released.isKinematic = true;   // lock the core in the field
        onActivated?.Invoke();
        // TODO: success feedback — field visual, sound
    }
}