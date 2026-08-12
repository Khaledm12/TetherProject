using UnityEngine;

public class EventDoor : MonoBehaviour
{
    [SerializeField] private Animator anim;

    public void Open() { anim.SetBool("Open", true); }
    public void Close() { anim.SetBool("Open", false); }
}