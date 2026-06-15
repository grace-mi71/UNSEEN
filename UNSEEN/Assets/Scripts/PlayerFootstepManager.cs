/*
 * Owner: Eunyeong Choi
 * Function of this code: Plays player footstep audio after the XR camera moves a configured horizontal distance.
 * Additional notes: Vertical movement is ignored so climbing or head-height changes do not trigger footsteps.
 */
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootstepManager : MonoBehaviour
{
    [Header("=== Footstep Settings ===")]
    [Tooltip("발자국 소리 파일")]
    public AudioClip footstepSound;

    [Tooltip("소리 크기")]
    [Range(0f, 1f)] public float volume = 0.5f;

    [Tooltip("얼마나 이동했을 때 소리를 낼 것인지 (보폭)")]
    public float stepDistance = 0.8f;

    private AudioSource audioSource;
    private Transform headTransform;
    private Vector2 lastPositionXZ;
    private float accumulatedDistance = 0f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (Camera.main != null)
        {
            headTransform = Camera.main.transform;
            lastPositionXZ = new Vector2(headTransform.position.x, headTransform.position.z);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Main Camera를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (headTransform == null) return;

        // Use a Vector2 so vertical movement does not contribute to footsteps.
        Vector2 currentPositionXZ = new Vector2(headTransform.position.x, headTransform.position.z);

        // Measure horizontal movement since the previous frame.
        float distanceMoved = Vector2.Distance(currentPositionXZ, lastPositionXZ);

        // Accumulate movement until one step distance is reached.
        accumulatedDistance += distanceMoved;
        lastPositionXZ = currentPositionXZ;

        // Play a footstep when the accumulated distance exceeds the configured stride.
        if (accumulatedDistance >= stepDistance)
        {
            PlayFootstep();

            // Reset the accumulated distance after playing a step.
            accumulatedDistance = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (footstepSound != null)
        {
            audioSource.PlayOneShot(footstepSound, volume);
        }
    }
}
