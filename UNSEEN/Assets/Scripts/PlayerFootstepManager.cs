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

        // 높이(Y축) 변화는 무시하고, 앞뒤좌우(X, Z축) 이동만 계산하기 위해 Vector2 사용
        Vector2 currentPositionXZ = new Vector2(headTransform.position.x, headTransform.position.z);

        // 이전 프레임과 비교해서 이동한 거리 계산
        float distanceMoved = Vector2.Distance(currentPositionXZ, lastPositionXZ);

        // 이동 거리를 계속 누적
        accumulatedDistance += distanceMoved;
        lastPositionXZ = currentPositionXZ;

        // 누적된 거리가 설정한 보폭(stepDistance)을 넘으면 소리 재생
        if (accumulatedDistance >= stepDistance)
        {
            PlayFootstep();

            // 소리를 냈으니 누적 거리는 다시 0으로 초기화
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