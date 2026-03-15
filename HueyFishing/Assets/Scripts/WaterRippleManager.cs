using UnityEngine;

public class WaterRippleManager : MonoBehaviour
{
    public RenderTexture rippleTexture;
    public Material rippleDrawMaterial;

    void Start()
    {
        rippleTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.RFloat);
        rippleTexture.Create();

        Shader.SetGlobalTexture("_RippleTex", rippleTexture);
    }

    public void AddRipple(Vector3 worldPos)
    {
        rippleDrawMaterial.SetVector("_RipplePos", worldPos);

        RenderTexture temp = RenderTexture.GetTemporary(rippleTexture.width, rippleTexture.height);

        Graphics.Blit(rippleTexture, temp);
        Graphics.Blit(temp, rippleTexture, rippleDrawMaterial);

        RenderTexture.ReleaseTemporary(temp);
    }
}
