using UnityEngine;

public class FieldVisual : MonoBehaviour
{
    [SerializeField] private Renderer fieldRenderer;
    [SerializeField] private Material idleMaterial;
    [SerializeField] private Material activeMaterial;     
    [SerializeField] private GameObject activeEffect;     
    [SerializeField] private bool hideRendererOnActivate;  

    void Start()
    {
        fieldRenderer.material = idleMaterial;
        if (activeEffect != null) activeEffect.SetActive(false);
    }

    public void SetActive()
    {
        if (hideRendererOnActivate)
            fieldRenderer.enabled = false;
        else if (activeMaterial != null)
            fieldRenderer.material = activeMaterial;

        if (activeEffect != null) activeEffect.SetActive(true);
    }
}