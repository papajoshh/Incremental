using Coffee.UIExtensions;
using UnityEngine;

namespace Programental
{
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(UIParticle))]
    public class UIParticleAutoSetup : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private float startLifetime = 0.6f;
        [SerializeField] private float minSpeed = 50f;
        [SerializeField] private float maxSpeed = 150f;
        [SerializeField] private float minSize = 8f;
        [SerializeField] private float maxSize = 20f;
        [SerializeField] private Color colorA = new Color(1f, 0.9f, 0.3f);
        [SerializeField] private Color colorB = new Color(0.3f, 1f, 1f);
        [SerializeField] private float gravity = 0.3f;
        [SerializeField] private int maxParticles = 100;

        [Header("Shape")]
        [SerializeField] private ParticleSystemShapeType shapeType = ParticleSystemShapeType.Circle;
        [SerializeField] private float shapeRadius = 5f;

        [Header("Material")]
        [SerializeField] private Material overrideMaterial;

        private void Awake()
        {
            Configure(GetComponent<ParticleSystem>());

            var uiParticle = GetComponent<UIParticle>();
            uiParticle.positionMode = UIParticle.PositionMode.Absolute;
        }

        private void Configure(ParticleSystem ps)
        {
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = startLifetime;
            main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = maxParticles;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = ps.emission;
            emission.enabled = false;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = shapeType;
            shape.radius = shapeRadius;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (overrideMaterial != null)
            {
                renderer.material = overrideMaterial;
            }
            else
            {
                var shader = Shader.Find("UI/Default");
                if (shader != null)
                    renderer.material = new Material(shader);
            }

            ps.Stop();
        }
    }
}
