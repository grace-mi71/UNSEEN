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

    [Header("Jump Scare Settings")]
    public GameObject jumpScareMonster;
    public AudioClip screamSound;

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

        if (jumpScareMonster != null) jumpScareMonster.SetActive(false);

        myWalkType = Random.Range(0, 3);
        anim.SetInteger("WalkType", myWalkType);

        if (startInElevator)
            StartWalkOut();
        else
            MoveToNextPoint();
    }

    void Update()
    {
        if (isGameOver) return;

        HandleFootsteps();

        if (isWalkingOut || isClimbing)
        {
            if (isWalkingOut)
                transform.Translate(Vector3.forward * walkOutSpeed * Time.deltaTime);
            return;
        }

        if (isChasing)
        {
            if (player != null)
                agent.destination = player.position;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (currentPoint == waypoints.Length - 1)
                StartChasing();
            else
            {
                if (currentPoint == climbWaypointIndex)
                    StartClimbing();
                else
                {
                    currentPoint++;
                    MoveToNextPoint();
                }
            }
        }
    }

    void HandleFootsteps()
    {
        if (isClimbing) return;

        bool isMoving = isWalkingOut ||
                        (agent.enabled && agent.velocity.sqrMagnitude > 0.01f);

        if (isMoving)
        {
            stepTimer += Time.deltaTime;
            float currentInterval = isChasing ? runStepInterval : walkStepInterval;
            if (stepTimer >= currentInterval)
            {
                stepTimer = 0f;
                if (footstepSound != null)
                    audioSource.PlayOneShot(footstepSound, footstepVolume);
            }
        }
        else
        {
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

        if (chaseStartSound != null)
            audioSource.PlayOneShot(chaseStartSound);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isGameOver)
            TriggerJumpScare();
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
            audioSource.PlayOneShot(screamSound);

        // 3초 뒤 페이드 아웃 → 현재 스테이지 재시작
        Invoke("TriggerFadeRestart", 3f);
    }

    void TriggerFadeRestart()
    {
        FadeManager.Instance?.FadeAndGoToMainMenu(0f);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}