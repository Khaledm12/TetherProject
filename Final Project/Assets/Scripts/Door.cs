using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource openSound;
    [SerializeField] private float soundDelay = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("Near", true);
            if (openSound != null) openSound.PlayDelayed(soundDelay);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) anim.SetBool("Near", false);
    }
}