using UnityEngine;

public sealed class BrailleBlockSound : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float detectionHeight = 0.5f;

    private bool playerIsOn = false;
    private Transform playerTransform;
    private Collider[] brailleColliders; // 자식 전체 Collider

    private void Start()
    {
        // 자식 포함 모든 Collider 수집
        brailleColliders = GetComponentsInChildren<Collider>();

        var playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    private void Update()
    {
        if (playerTransform == null || brailleColliders == null)
            return;

        var playerPos = playerTransform.position;
        bool isAboveAny = false;

        foreach (var col in brailleColliders)
        {
            var bounds = col.bounds;

            bool isAbove = playerPos.x >= bounds.min.x && playerPos.x <= bounds.max.x &&
                           playerPos.z >= bounds.min.z && playerPos.z <= bounds.max.z &&
                           playerPos.y >= bounds.max.y - 0.1f &&
                           playerPos.y <= bounds.max.y + detectionHeight;

            if (isAbove) { isAboveAny = true; break; }
        }

        if (isAboveAny && !playerIsOn)
        {
            playerIsOn = true;
            SoundStateManager.Instance?.SetOnBraille(true);
        }
        else if (!isAboveAny && playerIsOn)
        {
            playerIsOn = false;
            SoundStateManager.Instance?.SetOnBraille(false);
        }
    }
}