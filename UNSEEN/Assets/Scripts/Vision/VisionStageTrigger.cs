using UnityEngine;

namespace Unseen.Vision
{
    [RequireComponent(typeof(Collider))]
    public sealed class VisionStageTrigger : MonoBehaviour
    {
        [SerializeField] private VisionMode visionMode = VisionMode.Normal;
        [SerializeField] private bool triggerOnce = true;

        private bool hasTriggered;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce)
                return;

            var controller = FindFirstObjectByType<VisionEffectController>();
            if (controller == null)
                return;

            controller.SetVisionMode(visionMode);
            hasTriggered = true;
        }
    }
}
