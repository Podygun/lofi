using UnityEngine;

namespace LofiRoom
{
    public class SoftBob : MonoBehaviour
    {
        public float amplitude = 0.05f;

        [Min(0.01f)] public float speed = 0.35f;

        public float horizontalAmplitude = 0f;

        [Range(0f, 0.2f)] public float scalePulse = 0.01f;

        public float phaseOffset = 0f;

        public bool randomizePhase = true;

        Vector3 _basePosition;
        Vector3 _baseScale;

        void Start()
        {
            _basePosition = transform.localPosition;
            _baseScale = transform.localScale;
            if (randomizePhase) phaseOffset += Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            float t = Time.time * speed * Mathf.PI * 2f + phaseOffset;
            float wave = Mathf.Sin(t);

            transform.localPosition = _basePosition + new Vector3(
                Mathf.Cos(t * 0.5f) * horizontalAmplitude,
                wave * amplitude,
                0f);

            if (scalePulse > 0f)
                transform.localScale = _baseScale * (1f + wave * scalePulse);
        }
    }
}
