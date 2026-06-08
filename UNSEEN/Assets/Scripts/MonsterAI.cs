using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform player;

    [Header("Elevator Settings")]
    public bool startInElevator = false;
    public float walkOutTime = 2.0f;
    public float walkOutSpeed = 2.0f;

    [Header("Climb Settings")]
    // 클라이밍을 실행할 웨이포인트 인덱스 (0이 첫 번째 웨이포인트)
    public int climbWaypointIndex = 1;
    // 클라이밍(올라가기+내려오기) 애니메이션이 완전히 끝날 때까지 기다리는 시간
    public float climbDuration = 4.0f;

    private int currentPoint = 0;
    private NavMeshAgent agent;
    private Animator anim;
    private bool isChasing = false;
    private bool isWalkingOut = false;
    private bool isClimbing = false;

    // 고유 걸음걸이 저장용
    private int myWalkType;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

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
        // 엘리베이터에서 나오거나 클라이밍 중일 때는 길찾기 업데이트 중지
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
                // 현재 도착한 곳이 지정한 클라이밍 위치라면 클라이밍 시작
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

        // 걷기 상태가 유지되지 않도록 임시로 존재하지 않는 WalkType 값 할당
        anim.SetInteger("WalkType", 5);
        anim.SetTrigger("Climb");

        // 지정한 시간(climbDuration) 뒤에 클라이밍 종료 함수 실행
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

        // 다시 고유 걸음걸이로 복구 (Any State에서 자연스럽게 걷기로 넘어감)
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
        if (other.CompareTag("Player"))
        {
            agent.isStopped = true;
            anim.SetTrigger("Attack");
            Debug.Log("GameOver");
        }
    }
}