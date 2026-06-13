using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class BrailleBlockSound : MonoBehaviour
{
    [Tooltip("플레이어 태그")]
    [SerializeField] private string playerTag = "Player";

    private int playerCount = 0;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag(playerTag))
            return;

        playerCount++;
        if (playerCount == 1)
            SoundStateManager.Instance?.SetOnBraille(true);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.collider.CompareTag(playerTag))
            return;

        playerCount = Mathf.Max(0, playerCount - 1);
        if (playerCount == 0)
            SoundStateManager.Instance?.SetOnBraille(false);
    }
}