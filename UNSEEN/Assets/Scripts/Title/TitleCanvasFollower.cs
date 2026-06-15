// TitleCanvasFollower.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.UI;

public class TitleCanvasFollower : MonoBehaviour
{
    [SerializeField] private float distance = 1.5f;   // 눈에서 얼마나 앞
    [SerializeField] private float smoothSpeed = 8f;  // 부드럽게 따라오는 속도

    private Transform cameraTransform;

    private void Start()
    {
        // XROrigin의 Camera를 가져옴
        var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        cameraTransform = xrOrigin != null ? xrOrigin.Camera.transform : Camera.main.transform;
        InstallPokeOnButtons();
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        // 카메라 앞 distance 미터 위치
        var targetPos = cameraTransform.position + cameraTransform.forward * distance;

        // Yaw만 따라감 — 고개를 위아래로 숙여도 캔버스는 수평 유지
        var targetRot = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
    }

    private void InstallPokeOnButtons()
    {
        foreach (var button in GetComponentsInChildren<Button>())
        {
            var go = button.gameObject;

            if (go.GetComponent<BoxCollider>() == null)
            {
                var col = go.AddComponent<BoxCollider>();
                // RectTransform 크기에 맞게 콜라이더 설정
                var rect = go.GetComponent<RectTransform>();
                if (rect != null)
                    col.size = new Vector3(rect.rect.width, rect.rect.height, 0.02f);
            }

            var interactable = go.GetComponent<XRSimpleInteractable>() 
                            ?? go.AddComponent<XRSimpleInteractable>();

            var pokeFilter = go.GetComponent<XRPokeFilter>() 
                        ?? go.AddComponent<XRPokeFilter>();
            pokeFilter.pokeInteractable = interactable;
            pokeFilter.pokeCollider = go.GetComponent<Collider>();

            // XR 인터랙션 → Unity UI Button 클릭 연결
            interactable.selectEntered.AddListener(_ => button.onClick.Invoke());
        }
    }
}