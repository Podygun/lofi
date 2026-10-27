using UnityEngine;

namespace LofiRoom
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class GentleFlicker : MonoBehaviour
    {
        [Range(0f, 1f)] public float minAlpha = 0.45f;
        [Range(0f, 1f)] public float maxAlpha = 0.85f;

        [Min(0.05f)] public float speed = 1.2f;

        [Range(0f, 0.3f)] public float scalePulse = 0.04f;

        public AmbienceMixer mixer;

        public string linkedTrackId = "";

        [Range(0f, 1f)] public float dimmedAlpha = 0.15f;

        SpriteRenderer _renderer;
        Vector3 _baseScale;
        float _seed;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
            _seed = Random.value * 100f;
        }

        void Update()
        {
            float noise = Mathf.PerlinNoise(_seed, Time.time * speed);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, noise);

            if (mixer != null && !string.IsNullOrEmpty(linkedTrackId) && !mixer.IsOn(linkedTrackId))
                alpha = dimmedAlpha;

            Color c = _renderer.color;
            _renderer.color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, alpha, Time.deltaTime * 6f));

            if (scalePulse > 0f)
                transform.localScale = _baseScale * (1f + (noise - 0.5f) * 2f * scalePulse);
        }
    }
}
