using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PowerConsole : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public XRSocketInteractor socket;
        public string requiredCellId;      
        public Renderer indicator;          
    }

    [Header("Slots")]
    [SerializeField] private Slot[] slots;

    [Header("Indicator Materials")]
    [SerializeField] private Material emptyMat;
    [SerializeField] private Material correctMat;
    [SerializeField] private Material wrongMat;

    [Header("Events")]
    public UnityEvent onSolved;

    private bool solved;

    private void OnEnable()
    {
        foreach (var s in slots)
        {
            if (s.socket != null)
            {
                s.socket.selectEntered.AddListener(_ => Evaluate());
                s.socket.selectExited.AddListener(_ => Evaluate());
            }
        }
    }

    private void Evaluate()
    {
        if (solved) return;

        bool allCorrect = true;

        foreach (var s in slots)
        {
            var held = s.socket.GetOldestInteractableSelected();
            if (held == null)
            {
                if (s.indicator != null) s.indicator.material = emptyMat;
                allCorrect = false;
                continue;
            }

            var cell = held.transform.GetComponent<PowerCell>();
            bool correct = cell != null && cell.cellId == s.requiredCellId;
            if (s.indicator != null) s.indicator.material = correct ? correctMat : wrongMat;
            if (!correct) allCorrect = false;
        }

        if (allCorrect)
        {
            solved = true;
            StartCoroutine(LockAfterSeating());
        }
    }

    private IEnumerator LockAfterSeating()
    {
        yield return new WaitForSeconds(0.35f);
        LockCells();
        onSolved?.Invoke();
    }

    private void LockCells()
    {
        foreach (var s in slots)
        {
            var held = s.socket.GetOldestInteractableSelected();
            if (held == null) continue;

            var rb = held.transform.GetComponent<Rigidbody>();
            var grab = held.transform.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

            if (grab != null) grab.enabled = false;   
            if (rb != null) rb.isKinematic = true;    
        }
    }
}