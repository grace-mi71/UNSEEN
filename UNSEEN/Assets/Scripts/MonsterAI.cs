using UnityEngine;
using UnityEngine.AI;

// 몬스터 순찰, 랜덤 이동 및 클라이밍 스크립트
public class MonsterAI : MonoBehaviour
{
    public Transform[] waypoints;
    private int currentPoint = 0;

    private NavMeshAgent agent;
    private Animator anim;

    // 클라이밍 애니메이션과 역재생이 끝나는 데 걸리는 총 시간
    public float climbDuration = 4.0f;

    // 중복 실행을 막기 위한 상태 확인 변수
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        MoveToNextPoint();
    }

    void Update()
    {
        // 대기 중이 아닐 때 목표 지점 도착 확인
        if (!isWaiting && !agent.pathPending && agent.remainingDistance < 0.1f)
        {
            isWaiting = true;

            // 이동 애니메이션 중지 및 클라이밍 시작
            anim.SetBool("IsWalking", false);
            anim.SetTrigger("Climb");

            // 설정한 시간(climbDuration) 대기 후 다음 지점으로 이동
            Invoke("MoveToNextPoint", climbDuration);
        }
    }

    void MoveToNextPoint()
    {
        if (waypoints.Length == 0) return;

        isWaiting = false;

        // 0부터 4까지 총 5개의 이동 애니메이션 중 하나를 랜덤으로 선택
        int randomWalk = Random.Range(0, 5);
        anim.SetInteger("WalkType", randomWalk);

        // 이동 상태로 전환 및 목적지 설정
        anim.SetBool("IsWalking", true);
        agent.destination = waypoints[currentPoint].position;

        currentPoint = (currentPoint + 1) % waypoints.Length;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 에이전트 정지 
            agent.isStopped = true;
            anim.SetBool("IsWalking", false);

            Debug.Log("GameOver");
        }
    }
}