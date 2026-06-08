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

    private int currentPoint = 0;
    private NavMeshAgent agent;
    private Animator anim;
    private bool isChasing = false;
    private bool isWalkingOut = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // 엘리베이터에서 시작하는지 확인
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
        // 엘리베이터에서 나오는 중일 때 강제 직진
        if (isWalkingOut)
        {
            transform.Translate(Vector3.forward * walkOutSpeed * Time.deltaTime);
            return;
        }

        // 플레이어 추격 중일 때
        if (isChasing)
        {
            if (player != null)
            {
                agent.destination = player.position;
            }
            return;
        }

        // 웨이포인트 이동 중일 때
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (currentPoint == waypoints.Length - 1)
            {
                StartChasing();
            }
            else
            {
                currentPoint++;
                MoveToNextPoint();
            }
        }
    }

    void StartWalkOut()
    {
        isWalkingOut = true;
        // 내비메시 에이전트를 잠시 꺼서 에러 방지
        agent.enabled = false;

        // 걷기 애니메이션 실행 (0번 걷기)
        anim.SetInteger("WalkType", 0);

        // 설정한 시간 뒤에 탈출 종료 함수 실행
        Invoke("FinishWalkOut", walkOutTime);
    }

    void FinishWalkOut()
    {
        isWalkingOut = false;
        // 파란색 내비메시 영역에 도착했으므로 다시 에이전트 켜기
        agent.enabled = true;

        MoveToNextPoint();
    }

    void MoveToNextPoint()
    {
        if (waypoints.Length == 0) return;

        int randomWalk = Random.Range(0, 3);
        anim.SetInteger("WalkType", randomWalk);

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