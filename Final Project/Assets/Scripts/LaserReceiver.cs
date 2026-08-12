using UnityEngine;
using UnityEngine.Events;

public class LaserReceiver : MonoBehaviour
{
    [SerializeField] private float requiredHoldTime = 0.5f;  // sustained hit to solve
    [SerializeField] private Renderer indicator;             // lights up
    [SerializeField] private Material unlitMaterial;
    [SerializeField] private Material litMaterial;
    [SerializeField] private int materialIndex = 1;

    public UnityEvent onActivated;

    private float hitTimer;
    private bool solved;

    public void SetBeamHit(bool isHit)
    {
        if (solved) return;

        hitTimer = isHit ? hitTimer + Time.deltaTime : 0f;

        if (indicator != null)
        {
            var mats = indicator.materials;
            mats[materialIndex] = isHit ? litMaterial : unlitMaterial;
            indicator.materials = mats;
        }

        if (hitTimer >= requiredHoldTime)
        {
            solved = true;
            onActivated?.Invoke();
        }
    }
}