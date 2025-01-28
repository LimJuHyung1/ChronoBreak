using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UIElements;

public class UnifiedEnemyHandler : MonoBehaviour
{
    [Header("Target Settings")]
    public float stopChaseRadius = 15f; // 추적 중단 반경

    [Header("Patrol Settings")]
    public float patrolRange = 10f; // 패트롤 범위
    public float idleTime = 3f; // 대기 시간        

    [Header("References")]
    [SerializeField] private BoxCollider attackCollider; // 공격 콜라이더

    [SerializeField] private NavMeshAgent agent; // NavMeshAgent 컴포넌트
    [SerializeField] private JuHyung_Enemy enemy; // 몬스터 클래스 참조
    [SerializeField] private Transform targetTransform;
    [SerializeField] private JuHyung_Player player; // 플레이어 클래스 참조
    private Vector3 originalPosition; // 몬스터 초기 위치
    private float patrolTimer; // 패트롤 대기 시간 타이머
    float distanceToTarget;
    bool canNotMove;

    private bool isChasing; // 실제 값을 저장할 필드
    private Coroutine attackCoroutine; // 실행 중인 코루틴을 저장하는 변수

    public bool IsChasing
    {
        get => isChasing; // 값을 반환
        set
        {
            // 값이 변경될 때만 메서드를 실행
            if (isChasing != value)
            {
                isChasing = value; // 값 설정

                if(!isChasing)
                    StopChasing();
            }
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<JuHyung_Enemy>();
        targetTransform = GameManager.Instance.GetPlayerTransform();

        // NavMeshAgent 초기 설정
        agent.speed = enemy.GetMoveSpeed();
        originalPosition = transform.position; // 초기 위치 저장

        attackCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (!enemy.IsAlive())
        {
            StopChasing();
            return;
        }

        distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

        if (distanceToTarget <= stopChaseRadius)
        {
            IsChasing = true;

            if (distanceToTarget <= agent.stoppingDistance)
            {
                StopChasing(); // 공격 준비
            }
            else
            {
                StartChasing(); // 추적
            }
        }
        else
        {
            IsChasing = false;
            PatrolOrIdle();
        }
    }

    private void StartChasing()
    {
        agent.isStopped = false; // 에이전트 이동 시작
        agent.SetDestination(targetTransform.position); // 목표 위치 설정
        enemy.SetBoolIsWalking(true); // 걷기 애니메이션
    }

    private void StopChasing()
    {
        agent.ResetPath();
        agent.isStopped = true;
        enemy.SetBoolIsWalking(false); // 대기 애니메이션
    }

    /// <summary>
    /// 몬스터가 대기하거나 패트롤 동작을 수행하는 함수.
    /// 일정 시간 대기 후 랜덤 위치로 이동하거나, 이동 후 다시 대기 상태로 전환.
    /// </summary>
    private void PatrolOrIdle()
    {
        // NavMeshAgent가 멈춰 있을 때
        if (agent.isStopped)
        {
            patrolTimer += Time.deltaTime; // 대기 시간 누적

            // 대기 시간이 idleTime을 초과하면 패트롤 시작
            if (patrolTimer >= idleTime)
            {
                Vector3 patrolPoint = GetRandomPatrolPoint(); // 랜덤 패트롤 지점 계산
                agent.SetDestination(patrolPoint); // NavMeshAgent가 해당 지점으로 이동하도록 설정
                agent.isStopped = false; // NavMeshAgent 이동 재개
                enemy.SetBoolIsWalking(true); // 걷기 애니메이션 설정
                patrolTimer = 0f; // 대기 타이머 초기화

                Debug.Log("패트롤 시작");
            }
        }

        // NavMeshAgent가 목표 지점에 도착한 경우
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true; // NavMeshAgent 이동 중지
            enemy.SetBoolIsWalking(false); // 대기 애니메이션 설정
        }
    }

    /// <summary>
    /// 몬스터가 랜덤한 패트롤 지점을 생성하는 함수.
    /// 지정된 범위 내에서 유효한 NavMesh 상의 위치를 반환.
    /// </summary>
    /// <returns>유효한 랜덤 패트롤 지점 또는 초기 위치</returns>
    private Vector3 GetRandomPatrolPoint()
    {
        // 초기 위치를 기준으로 랜덤한 방향의 패트롤 지점 계산
        Vector3 randomPoint = originalPosition + new Vector3(
            Random.Range(-patrolRange, patrolRange), // X축 방향의 랜덤 범위
            0, // 높이는 0으로 고정
            Random.Range(-patrolRange, patrolRange) // Z축 방향의 랜덤 범위
        );

        // NavMesh 상에서 유효한 위치를 확인
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRange, NavMesh.AllAreas))
        {
            return hit.position; // 유효한 위치 반환
        }
        else
        {
            return originalPosition; // 유효하지 않다면 초기 위치 반환
        }
    }

    private void OnTriggerEnter(Collider other)
    {                
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // 공격 범위 내에 플레이어 존재 시 정지 거리 설정
            agent.stoppingDistance = Vector3.Distance(transform.position, other.transform.position) - 0.5f;
            player = other.GetComponent<JuHyung_Player>();
            enemy.IsAttacking = true; // 공격 상태 활성화
        }            
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player") && enemy.IsAttacking)
        {
            if (attackCoroutine == null) // 실행 중인 코루틴이 없을 경우에만 실행
            {
                attackCoroutine = StartCoroutine(AttackDelay(other));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {                
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (attackCoroutine != null) // 실행 중인 코루틴이 있다면 중지
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null; // 코루틴 상태 초기화
            }

            enemy.IsAttacking = false;
            player = null;
        }
    }

    private IEnumerator AttackDelay(Collider other)
    {
        while (enemy.IsAttacking) // 공격 상태가 유지되는 동안 반복
        {
            yield return new WaitForSeconds(enemy.GetATKDelay()); // 딜레이 후 공격 실행
            if (player != null) // 플레이어가 유효한 경우에만 공격
            {
                enemy.Attack();
            }
        }
        attackCoroutine = null; // 코루틴 상태 초기화
    }




    /// <summary>
    /// 애니메이션 이벤트(공격 타이밍에 알맞게 호출됨)
    /// </summary>
    public void AttackPlayer()
    {
        if(player != null)
            player.Hit(GameManager.Instance.CalcDamage(enemy.GetATK(), player.GetDEF()));        

        // null 일 경우 빗맞춘 경우이니 이에 따른 로직 만들기
    }
}
