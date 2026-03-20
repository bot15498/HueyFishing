using System.Collections;
using UnityEngine;

public class TargetWhiteFlash : MonoBehaviour
{
    public SpriteRenderer sr;
    public float flashDuration = 0.08f;

    private Coroutine flashRoutine;
    private Color originalColor;

    void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            originalColor = sr.color;
    }

    public void Flash()
    {
        if (sr == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(DoFlash());
    }

    IEnumerator DoFlash()
    {
        sr.color = Color.white;
        yield return new WaitForSeconds(flashDuration);
        sr.color = originalColor;
        flashRoutine = null;
    }
}
