using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MonsterAI : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform player;

    [Header("Elevator Settings")]
    public bool startInElevator = false;
    public float walkOutTime = 2.0f;
    public float walkOutSpeed = 2.0f;

    [Header("Climb Settings")]
    public int climbWaypointIndex = 1;
    public float climbDuration = 4.0f;

    [Header("Jump Scare & UI Settings")]
    public GameObject jumpScareMonster;
    public AudioClip screamSound;
    public GameObject gameOverUI;

    [Header("Sound Settings")]
    public AudioClip footstepSound;
    [Range(0f, 1f)] public float footstepVolume = 0.5f;
    public float walkStepInterval = 0.6f;
    public float runStepInterval = 0.3f;
    public AudioClip chaseStartSound;

    private int currentPoint = 0;
    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioSource;

    private bool isChasing = false;
    private bool isWalkingOut = false;
    private bool isClimbing = false;
    private bool isGameOver = false;

    private int myWalkType;
    private float stepTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (jumpScareMonster != null) jumpScareMonster.SetActive(false);

        myWalkType = Random.Range(0, 3);
        anim.SetInteger("WalkType", myWalkType);

        if (startInElevator)
        {
            StartWalkOut();
        }
        else
        {
            MoveToNextPoint();
        }
    }

    void Update()
    {
        if (isGameOver) return;

        // 매 프레임 발자국 소리 로직 처리
        HandleFootsteps();

        if (isWalkingOut || isClimbing)
        {
            if (isWalkingOut)
            {
                transform.Translate(Vector3.forward * walkOutSpeed * Time.deltaTime);
            }
            return;
        }

        if (isChasing)
        {
            if (player != null)
            {
                agent.destination = player.position;
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (currentPoint == waypoints.Length - 1)
            {
                StartChasing();
            }
            else
            {
                if (currentPoint == climbWaypointIndex)
                {
                    StartClimbing();
                }
                else
                {
                    currentPoint++;
                    MoveToNextPoint();
                }
            }
        }
    }

    // 몬스터가 움직이는지 판별하여 발자국 소리를 재생
    void HandleFootsteps()
    {
        if (isClimbing) return;

        bool isMoving = false;

        if (isWalkingOut)
        {
            isMoving = true;
        }
        else if (agent.enabled && agent.velocity.sqrMagnitude > 0.01f)
        {
            isMoving = true;
        }

        if (isMoving)
        {
            stepTimer += Time.deltaTime;
            float currentInterval = isChasing ? runStepInterval : walkStepInterval;

            if (stepTimer >= currentInterval)
            {
                stepTimer = 0f;
                if (footstepSound != null)
                {
                    audioSource.PlayOneShot(footstepSound, footstepVolume);
                }
            }
        }
        else
        {
            // 멈춰있을 경우 타이머 초기화
            stepTimer = 0f;
        }
    }

    void StartWalkOut()
    {
        isWalkingOut = true;
        agent.enabled = false;
        anim.SetInteger("WalkType", myWalkType);
        Invoke("FinishWalkOut", walkOutTime);
    }

    void FinishWalkOut()
    {
        isWalkingOut = false;
        agent.enabled = true;
        MoveToNextPoint();
    }

    void StartClimbing()
    {
        isClimbing = true;
        agent.enabled = false;
        anim.SetInteger("WalkType", 5);
        anim.SetTrigger("Climb");
        Invoke("FinishClimbing", climbDuration);
    }

    void FinishClimbing()
    {
        isClimbing = false;
        agent.enabled = true;
        currentPoint++;
        MoveToNextPoint();
    }

    void MoveToNextPoint()
    {
        if (waypoints.Length == 0) return;
        anim.SetInteger("WalkType", myWalkType);
        agent.destination = waypoints[currentPoint].position;
    }

    void StartChasing()
    {
        isChasing = true;
        agent.speed = 4.0f;
        anim.SetInteger("WalkType", 3);

        // 추격 시작 시 소리 한 번 재생
        if (chaseStartSound != null)
        {
            audioSource.PlayOneShot(chaseStartSound);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isGameOver)
        {
            TriggerJumpScare();
        }
    }

    [ContextMenu("Test Jump Scare")]
    void TriggerJumpScare()
    {
        isGameOver = true;
        agent.enabled = false;

        transform.position = new Vector3(0, -1000f, 0);

        if (jumpScareMonster != null)
        {
            jumpScareMonster.SetActive(true);
            jumpScareMonster.transform.DOShakePosition(3.0f, new Vector3(0.2f, 0.5f, 0.5f), 30, 90, false, true);
        }

        if (screamSound != null)
        {
            audioSource.PlayOneShot(screamSound);
        }

        Invoke("ShowGameOverUI", 3.5f);
    }

    void ShowGameOverUI()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}