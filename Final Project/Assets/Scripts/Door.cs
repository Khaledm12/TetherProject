using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) anim.SetBool("Near", true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) anim.SetBool("Near", false);
    }
}