using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UIElements;

public class UnifiedEnemyHandler : MonoBehaviour
{
    [Header("Target Settings")]
    public float stopChaseRadius = 15f; // ���� �ߴ� �ݰ�

    [Header("Patrol Settings")]
    public float patrolRange = 10f; // ��Ʈ�� ����
    public float idleTime = 3f; // ��� �ð�        

    [Header("References")]
    [SerializeField] private BoxCollider attackCollider; // ���� �ݶ��̴�

    [SerializeField] private NavMeshAgent agent; // NavMeshAgent ������Ʈ
    [SerializeField] private JuHyung_Enemy enemy; // ���� Ŭ���� ����
    [SerializeField] private Transform targetTransform;
    [SerializeField] private JuHyung_Player player; // �÷��̾� Ŭ���� ����
    private Vector3 originalPosition; // ���� �ʱ� ��ġ
    private float patrolTimer; // ��Ʈ�� ��� �ð� Ÿ�̸�
    float distanceToTarget;
    bool canNotMove;

    private bool isChasing; // ���� ���� ������ �ʵ�
    private Coroutine attackCoroutine; // ���� ���� �ڷ�ƾ�� �����ϴ� ����

    public bool IsChasing
    {
        get => isChasing; // ���� ��ȯ
        set
        {
            // ���� ����� ���� �޼��带 ����
            if (isChasing != value)
            {
                isChasing = value; // �� ����

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

        // NavMeshAgent �ʱ� ����
        agent.speed = enemy.GetMoveSpeed();
        originalPosition = transform.position; // �ʱ� ��ġ ����

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
                StopChasing(); // ���� �غ�
            }
            else
            {
                StartChasing(); // ����
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
        agent.isStopped = false; // ������Ʈ �̵� ����
        agent.SetDestination(targetTransform.position); // ��ǥ ��ġ ����
        enemy.SetBoolIsWalking(true); // �ȱ� �ִϸ��̼�
    }

    private void StopChasing()
    {
        agent.ResetPath();
        agent.isStopped = true;
        enemy.SetBoolIsWalking(false); // ��� �ִϸ��̼�
    }

    /// <summary>
    /// ���Ͱ� ����ϰų� ��Ʈ�� ������ �����ϴ� �Լ�.
    /// ���� �ð� ��� �� ���� ��ġ�� �̵��ϰų�, �̵� �� �ٽ� ��� ���·� ��ȯ.
    /// </summary>
    private void PatrolOrIdle()
    {
        // NavMeshAgent�� ���� ���� ��
        if (agent.isStopped)
        {
            patrolTimer += Time.deltaTime; // ��� �ð� ����

            // ��� �ð��� idleTime�� �ʰ��ϸ� ��Ʈ�� ����
            if (patrolTimer >= idleTime)
            {
                Vector3 patrolPoint = GetRandomPatrolPoint(); // ���� ��Ʈ�� ���� ���
                agent.SetDestination(patrolPoint); // NavMeshAgent�� �ش� �������� �̵��ϵ��� ����
                agent.isStopped = false; // NavMeshAgent �̵� �簳
                enemy.SetBoolIsWalking(true); // �ȱ� �ִϸ��̼� ����
                patrolTimer = 0f; // ��� Ÿ�̸� �ʱ�ȭ

                Debug.Log("��Ʈ�� ����");
            }
        }

        // NavMeshAgent�� ��ǥ ������ ������ ���
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true; // NavMeshAgent �̵� ����
            enemy.SetBoolIsWalking(false); // ��� �ִϸ��̼� ����
        }
    }

    /// <summary>
    /// ���Ͱ� ������ ��Ʈ�� ������ �����ϴ� �Լ�.
    /// ������ ���� ������ ��ȿ�� NavMesh ���� ��ġ�� ��ȯ.
    /// </summary>
    /// <returns>��ȿ�� ���� ��Ʈ�� ���� �Ǵ� �ʱ� ��ġ</returns>
    private Vector3 GetRandomPatrolPoint()
    {
        // �ʱ� ��ġ�� �������� ������ ������ ��Ʈ�� ���� ���
        Vector3 randomPoint = originalPosition + new Vector3(
            Random.Range(-patrolRange, patrolRange), // X�� ������ ���� ����
            0, // ���̴� 0���� ����
            Random.Range(-patrolRange, patrolRange) // Z�� ������ ���� ����
        );

        // NavMesh �󿡼� ��ȿ�� ��ġ�� Ȯ��
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRange, NavMesh.AllAreas))
        {
            return hit.position; // ��ȿ�� ��ġ ��ȯ
        }
        else
        {
            return originalPosition; // ��ȿ���� �ʴٸ� �ʱ� ��ġ ��ȯ
        }
    }

    private void OnTriggerEnter(Collider other)
    {                
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // ���� ���� ���� �÷��̾� ���� �� ���� �Ÿ� ����
            agent.stoppingDistance = Vector3.Distance(transform.position, other.transform.position) - 0.5f;
            player = other.GetComponent<JuHyung_Player>();
            enemy.IsAttacking = true; // ���� ���� Ȱ��ȭ
        }            
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player") && enemy.IsAttacking)
        {
            if (attackCoroutine == null) // ���� ���� �ڷ�ƾ�� ���� ��쿡�� ����
            {
                attackCoroutine = StartCoroutine(AttackDelay(other));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {                
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (attackCoroutine != null) // ���� ���� �ڷ�ƾ�� �ִٸ� ����
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null; // �ڷ�ƾ ���� �ʱ�ȭ
            }

            enemy.IsAttacking = false;
            player = null;
        }
    }

    private IEnumerator AttackDelay(Collider other)
    {
        while (enemy.IsAttacking) // ���� ���°� �����Ǵ� ���� �ݺ�
        {
            yield return new WaitForSeconds(enemy.GetATKDelay()); // ������ �� ���� ����
            if (player != null) // �÷��̾ ��ȿ�� ��쿡�� ����
            {
                enemy.Attack();
            }
        }
        attackCoroutine = null; // �ڷ�ƾ ���� �ʱ�ȭ
    }




    /// <summary>
    /// �ִϸ��̼� �̺�Ʈ(���� Ÿ�ֿ̹� �˸°� ȣ���)
    /// </summary>
    public void AttackPlayer()
    {
        if(player != null)
            player.Hit(GameManager.Instance.CalcDamage(enemy.GetATK(), player.GetDEF()));        

        // null �� ��� ������ ����̴� �̿� ���� ���� �����
    }
}
