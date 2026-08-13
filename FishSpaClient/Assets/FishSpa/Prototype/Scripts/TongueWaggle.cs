using UnityEngine;

namespace FishSpa.Prototype
{
    public sealed class TongueWaggle : MonoBehaviour
    {
        public float frequency = 0.18f;
        public float yawAmplitude = 8f;
        public float sideAmplitude = 0.35f;
        public float pushStrength = 0.75f;

        private Vector3 startLocalPosition;
        private Quaternion startLocalRotation;

        private void Awake()
        {
            startLocalPosition = transform.localPosition;
            startLocalRotation = transform.localRotation;
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.time * frequency * Mathf.PI * 2f);
            transform.localPosition = startLocalPosition + Vector3.right * (wave * sideAmplitude);
            transform.localRotation = startLocalRotation * Quaternion.Euler(0f, wave * yawAmplitude, 0f);
        }

        private void OnTriggerStay(Collider other)
        {
            FishPlayerController fish = other.GetComponentInParent<FishPlayerController>();
            if (fish == null)
            {
                return;
            }

            Vector3 away = (other.transform.position - transform.position).normalized;
            if (away.sqrMagnitude < 0.01f)
            {
                away = transform.right;
            }

            fish.AddImpulse((away + Vector3.up * 0.15f).normalized * (pushStrength * Time.deltaTime));
        }
    }
}
