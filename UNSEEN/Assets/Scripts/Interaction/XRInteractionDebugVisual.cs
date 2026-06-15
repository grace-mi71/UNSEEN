using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Unseen.Interaction
{
    [RequireComponent(typeof(XRBaseInteractable))]
    public sealed class XRInteractionDebugVisual : MonoBehaviour
    {
        [SerializeField] private Color idleColor = new(0.35f, 0.35f, 0.35f);
        [SerializeField] private Color hoverColor = new(1f, 0.75f, 0.05f);
        [SerializeField] private Color selectedColor = new(0.1f, 1f, 0.25f);

        private XRBaseInteractable interactable;
        private Renderer[] renderers;

        public void Configure(Color idle, Color hover, Color selected)
        {
            idleColor = idle;
            hoverColor = hover;
            selectedColor = selected;
            ApplyColor(idleColor);
        }

        private void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();
            renderers = GetComponentsInChildren<Renderer>(true);
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
            ApplyColor(idleColor);
        }

        private void OnHoverEntered(HoverEnterEventArgs args) => ApplyColor(hoverColor);
        private void OnHoverExited(HoverExitEventArgs args) => ApplyColor(interactable.isSelected ? selectedColor : idleColor);
        private void OnSelectEntered(SelectEnterEventArgs args) => ApplyColor(selectedColor);
        private void OnSelectExited(SelectExitEventArgs args) => ApplyColor(interactable.isHovered ? hoverColor : idleColor);

        private void ApplyColor(Color color)
        {
            if (renderers == null)
                renderers = GetComponentsInChildren<Renderer>(true);

            foreach (var targetRenderer in renderers)
                targetRenderer.material.color = color;
        }
    }
}
