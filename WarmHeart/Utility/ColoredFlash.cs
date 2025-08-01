using System.Collections;
using UnityEngine;

public class ColoredFlash : MonoBehaviour
{
    [Tooltip("Material to switch to during the flash.")]
    [SerializeField] private Material flashMaterial;

    [Tooltip("Duration of the flash.")]
    [SerializeField] public float Duration;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Coroutine flashRoutine;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
        //flashMaterial = new Material(flashMaterial);
    }

    public void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.material = flashMaterial;
        //flashMaterial.color = color;

        yield return new WaitForSeconds(Duration);

        spriteRenderer.material = originalMaterial;
        flashRoutine = null;
    }
}