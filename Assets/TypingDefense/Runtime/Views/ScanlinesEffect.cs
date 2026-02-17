using UnityEngine;

namespace TypingDefense
{
    [RequireComponent(typeof(Camera))]
    public class ScanlinesEffect : MonoBehaviour
    {
        [SerializeField] Shader scanlineShader;
        [SerializeField, Range(100, 800)] float scanlineCount = 300f;
        [SerializeField, Range(0f, 1f)] float scanlineIntensity = 0.15f;
        [SerializeField] float scanlineSpeed = 0.5f;
        [SerializeField, Range(0f, 1f)] float vignetteIntensity = 0.4f;
        [SerializeField, Range(0f, 2f)] float vignetteRadius = 0.8f;
        [SerializeField, Range(0f, 0.1f)] float flickerIntensity = 0.02f;

        Material material;

        void Start()
        {
            material = new Material(scanlineShader);
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            material.SetFloat("_ScanlineCount", scanlineCount);
            material.SetFloat("_ScanlineIntensity", scanlineIntensity);
            material.SetFloat("_ScanlineSpeed", scanlineSpeed);
            material.SetFloat("_VignetteIntensity", vignetteIntensity);
            material.SetFloat("_VignetteRadius", vignetteRadius);
            material.SetFloat("_FlickerIntensity", flickerIntensity);
            Graphics.Blit(src, dest, material);
        }

        void OnDestroy()
        {
            if (material != null)
                Destroy(material);
        }
    }
}
