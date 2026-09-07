using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VRFadeEffect : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private GameObject escapeText;
    [SerializeField] private float fadeDuration = 2.0f;

    [Header("Locomotion Control")]
    [Tooltip("Drag the 'Locomotion' GameObject from your XR Origin here")]
    [SerializeField] private GameObject locomotionObject;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;

    public void StartFade()
    {
        //  Turns off Move, Turn, Teleport, Climb, and Gravity
        if (locomotionObject != null)
        {
            locomotionObject.SetActive(false);
        }
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        float timer = 0f;
        Color c = fadeImage.color;
        float startVolume = musicSource != null ? musicSource.volume : 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);
            c.a = t;
            fadeImage.color = c;
            if(musicSource != null)
                musicSource.volume = startVolume * (1f - t);
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;

        if (musicSource != null) musicSource.Stop();

        if (escapeText != null)
        {
            escapeText.SetActive(true);
        }
    }
}