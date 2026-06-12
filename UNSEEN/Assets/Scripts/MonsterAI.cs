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
    // 카메라 자식으로 미리 세팅해둔 몬스터 오브젝트
    public GameObject jumpScareMonster;
    public AudioClip screamSound;
    public GameObject gameOverUI;

    private int currentPoint = 0;
    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioSource;

    private bool isChasing = false;
    private bool isWalkingOut = false;
    private bool isClimbing = false;
    private bool isGameOver = false;

    private int myWalkType;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // 시작할 때 UI와 깜툭튀용 몬스터는 숨김 처리
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

            // 진동 시간 3.0초, X축(좌우) 0.2 추가
            jumpScareMonster.transform.DOShakePosition(3.0f, new Vector3(0.2f, 0.5f, 0.5f), 30, 90, false, true);
        }

        if (screamSound != null)
        {
            audioSource.PlayOneShot(screamSound);
        }

        // 진동 시간에 맞춰 UI 호출 시간을 3.5초로 연장
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