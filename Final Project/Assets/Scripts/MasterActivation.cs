using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class MasterActivation : MonoBehaviour
{
    [SerializeField] private XRSocketInteractor socket;
    [SerializeField] private int requiredSolves = 3;

    public UnityEvent onAllPuzzlesSolved;   // socket becomes active and lit with indicator
    public UnityEvent onMasterActivated;    // final cell placed exit door opens

    private int solveCount;
    private bool activated;

    void Start()
    {
        // dead until 3/3
        socket.socketActive = false;        
    }

    void OnEnable() { socket.selectEntered.AddListener(OnCellPlaced); }
    void OnDisable() { socket.selectEntered.RemoveListener(OnCellPlaced); }

    public void RegisterSolve()             
    {
        solveCount++;
        if (solveCount >= requiredSolves && !socket.socketActive)
        {
            socket.socketActive = true;
            Debug.Log("GATE OPENED — invoking onAllPuzzlesSolved");
            onAllPuzzlesSolved?.Invoke();
        }

        Debug.Log($"Solve count: {solveCount}");

    }

    private void OnCellPlaced(SelectEnterEventArgs args)
    {
        if (activated) return;
        activated = true;
        onMasterActivated?.Invoke();
    }
}