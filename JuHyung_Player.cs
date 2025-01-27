
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;


public class JuHyung_Player : MonoBehaviour
{
    /*
    // [SerializeField] public HitHandler hitHandler;
    [SerializeField] public Statistics stats; // ScriptableObject 참조    

    protected string entityName;
    protected int LEVEL;
    protected int MAX_HP;
    protected int MAX_MP;
    protected int MAX_EXP;
    protected int ATK;
    protected int DEF;

    protected float moveSpeed;                  // 이동 속도
    protected float ATKSpeed;

    protected MonsterHPBar healthBar;
    [SerializeField] protected int currentHP; // 현재 HP
    [SerializeField] protected int currentMP; // 현재 MP


    [SerializeField] private bool isAttacking = false;
    [SerializeField] private bool isJumpAttacking = false;
    [SerializeField] private bool isSprintJumpAttacking = false;
    [SerializeField] private bool isGuarding = false;
    [SerializeField] private bool isDodging = false;

    protected Animator animator;


    [Header("Movement Settings")]
    public float sprintSpeed = 5.0f;
    public float rotationSmoothTime = 0.12f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    // public float mouseSensitivity = 200f;
    private float rotationX = 0f; // ī�޶��� ���� ȸ�� ����
    private float rotationY = 0f; // ī�޶� ���� ȸ�� ����
    

    [Header("Player States")]
    public LayerMask groundMask;
    private Vector3 velocity;
    private float groundCheckRadius = 0.4f;

    float currentSpeed; // �̵� �������� ���
    Vector3 moveDirection;  // �̵� �������� ���
    Vector3 finalMoveDirection; // �̵� �������� ���
    Transform followTarget;
    private int currentEXP;

    private Vector2 moveInput; // �̵� �Է� ��
    private Vector2 lookInput; // ī�޶� ȸ�� �Է� ��
    private bool jumpInput;    // ���� �Է�
    private bool sprintInput;  // ������Ʈ �Է�


    [SerializeField] private float rollDistance = 5f; // ������ �Ÿ�
    [SerializeField] private float rollDuration = 0.5f; // ������ ���� �ð�
    private Vector3 rollStartPosition; // ������ ���� ��ġ
    private Vector3 rollTargetPosition; // ������ ��ǥ ��ġ

    private Vector3 sprintJumpAttackStartPosition; // ������ ���� ��ġ
    private Vector3 sprintJumpAttackTargetPosition; // ������ ��ǥ ��ġ

    private CharacterController controller;
    public PlayerAttackHandler attackHandler;
    private PlayerInput playerInput;
    public BarManager barManager;

    */

    [Header("Statistics")]
    [SerializeField] private Statistics stats;

    private string entityName;
    private int level, maxHP, maxMP, maxEXP, atk, def;
    private int currentHP, currentMP, currentEXP;
    private float moveSpeed, atkSpeed;

    [Header("Movement Settings")]
    public float sprintSpeed = 5.0f;
    public float rotationSmoothTime = 0.12f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private Vector3 moveDirection;
    private Vector3 finalMoveDirection;    

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool isDodging;

    float currentSpeed;

    [SerializeField] private float rollDistance = 5f; // ������ �Ÿ�
    [SerializeField] private float rollDuration = 0.5f; // ������ ���� �ð�
    private Vector3 rollStartPosition; // ������ ���� ��ġ
    private Vector3 rollTargetPosition; // ������ ��ǥ ��ġ

    private Vector3 sprintJumpAttackStartPosition; // ������ ���� ��ġ
    private Vector3 sprintJumpAttackTargetPosition; // ������ ��ǥ ��ġ


    [Header("Combat Settings")]
    [SerializeField] private bool isAttacking, isJumpAttacking, isSprintJumpAttacking, isGuarding;
    public PlayerAttackHandler attackHandler;

    [Header("References")]
    public Transform cameraTransform;
    private Animator animator;
    private CharacterController controller;
    private PlayerInput playerInput;
    public BarManager barManager;
    private MonsterHPBar healthBar;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        InitializeEntity();
        barManager.InitPlayerBars();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!isDodging)
        {
            if (context.performed && controller.isGrounded)
            {
                jumpInput = true;
                animator.SetTrigger("Jump");
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else if (context.canceled)
            {
                jumpInput = false;
            }
        }        
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!controller.isGrounded && !isJumpAttacking)
            {
                if (sprintInput)
                {
                    isSprintJumpAttacking = true;
                    animator.SetTrigger("SprintJumpAttack");
                    //SprintAttackMove(6f, 0.8f);
                }
                else
                {
                    isJumpAttacking = true;
                    JumpAttack();
                }
            }
            else
            {
                if (sprintInput)
                {
                    isAttacking = true;
                    animator.SetTrigger("SprintAttack");
                    SprintAttackMove(4f, 0.3f); // ������Ʈ�� �������� 4 ���� �̵�, �̵� �ð��� 1��                
                }
                else
                {
                    // �Ϲ� ���¿��� ���� �Է�                
                    isAttacking = true;
                    Attack();
                }                
            }
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            sprintInput = true;
        }
        else if (context.canceled)
        {
            sprintInput = false;
        }
    }

    public void OnGuard(InputAction.CallbackContext context)
    {
        if (controller.isGrounded)
        {
            if (context.started) // 'Z' Ű�� ������ ������ ��
            {
                isGuarding = true;
                animator.SetBool("isGuarding", true); // ��� �ִϸ��̼� ����
            }
            else if (context.canceled) // 'Z' Ű�� ������ ��
            {
                isGuarding = false;
                animator.SetBool("isGuarding", false); // ��� �ִϸ��̼� ����
            }
        }        
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (controller.isGrounded)
        {
            if (context.started)
            {
                // isGuarding = true;
                animator.SetTrigger("Dodge");
            }
        }
    }

    void FixedUpdate()
    {
        GroundCheck();
        HandleMovement();
        // HandleCameraRotation();
    }

    // 레벨 스케일링 적용하기
    private void InitializeEntity()
    {
        if (stats != null)
        {
            entityName = stats.name;
            level = stats.LEVEL;
            maxHP = stats.MAX_HP;
            maxMP = stats.MAX_MP;
            maxEXP = stats.MAX_EXP;
            atk = stats.ATK;
            def = stats.DEF;
            moveSpeed = stats.moveSpeed;
            atkSpeed = stats.ATKSpeed;

            currentHP = maxHP;
            currentMP = maxMP;
            currentEXP = 0;

            if (healthBar != null)
            {
                healthBar.SetMaxHealth(maxHP);
                healthBar.SetNameText(entityName);
            }
        }
        else
        {
            Debug.LogError("Stats ScriptableObject is not assigned!");
        }
    }

    public int GetCurrentHP() { return this.currentHP; }
    public int GetMAX_HP() { return this.maxHP; }
    public int GetCurrentMP() { return this.currentMP; }
    public int GetMAX_MP() { return this.maxMP; }
    public int GetMAX_EXP() { return this.maxEXP; }

    public int GetATK() { return this.atk; }
    public int GetDEF() { return this.def; }
    public float GetMoveSpeed() { return this.moveSpeed; }
    public float GetATKSpeed() { return this.atkSpeed; }
    public void SetHealthBar(MonsterHPBar HP) { this.healthBar = HP; }



    public void GainEXP(int exp)
    {
        currentEXP += exp;

        while (currentEXP >= maxEXP)
        {
            LevelUp();
        }
    }

    public int GetCurrentEXP()
    {
        return currentEXP;
    }

    private void LevelUp()
    {
        level++;
        currentEXP -= maxEXP;
        maxEXP += maxEXP / 10;
        maxHP += 10;
        currentHP = maxHP;
        atk += 5;
        def += 3;
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
        isAttacking = true;
    }

    public void JumpAttack()
    {
        animator.SetTrigger("JumpAttack");
    }

    public void UsingSkill()
    {
        animator.SetTrigger("UsingSkill");
    }

    public void Hit(int damage)
    {
        if (!isDodging || IsAlive())
        {
            if (isGuarding)
            {
                animator.ResetTrigger("Jump");
                animator.ResetTrigger("Attack");

                animator.SetTrigger("GuardAttack");
                isGuarding = false;
                isAttacking = true;
            }
            else
            {
                currentHP -= damage;

                if (healthBar != null)
                    healthBar.SetHealth(currentHP);

                // 피격 처리 실행

                if (currentHP > 0) animator.SetTrigger("Hit");
                else Die(3f);

                SetIsAttackingFalse();
                ResetAttackTrigger();
                ResetJumpAttackTrigger();
            }
        }        
    }

    public void Die(float delayOfDead)
    {
        animator.SetTrigger("Died");
        if (healthBar != null)
            healthBar.SetActiveFalseHealthBar();

        StartCoroutine(DieAfterDelay(delayOfDead));
    }

    private IEnumerator DieAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    public bool IsAlive()
    {
        return currentHP > 0;
    }

    public void EnableAttackHandler()
    {
        attackHandler.EnableBoxCollider();
    }

    public void DisableAttackHandler()
    {
        attackHandler.DisableBoxCollider();
    }

    /// <summary>
    /// Guard Attack 애니메이션 이벤트로 실행
    /// </summary>
    public void GuardAttackColSizeBigger()
    {
        attackHandler.GuardAttackSizeBigger();
    }

    public void GuardAttackColSizeSmaller()
    {
        attackHandler.GuardAttackSizeSmaller();
    }


    public void SetIsAttackingFalse()
    {
        isAttacking = false;
        isJumpAttacking = false;
    }

    public void SetIsJumpAttackingFalse()
    {
        isJumpAttacking = false;
    }

    public void SetIsSprintJumpAttackFalse()
    {
        isSprintJumpAttacking = false;
    }

    public void ResetAttackTrigger()
    {
        animator.ResetTrigger("Attack");
    }

    public void ResetJumpAttackTrigger()
    {        
        animator.ResetTrigger("JumpAttack");
    }
    

    private void GroundCheck()
    {        
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // ���鿡 ���� �� Y�� �ӵ� �ʱ�ȭ
        }
    }

    private void HandleMovement()
    {
        if (IsAlive() && !isAttacking && !isJumpAttacking && !isGuarding)
        {
            // �̵� �ӵ� ����
            currentSpeed = moveInput.magnitude > 0 ? (sprintInput ? moveSpeed * 1.5f : moveSpeed) : 0f;

            // �Է°��� ���� �̵� ���� ����
            moveDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;

            if (moveDirection.magnitude >= 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothTime * Time.deltaTime);
            }

            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;

            // ���� �̵� ���� ��� �� �̵� ó��
            if (!isDodging)
            {
                finalMoveDirection = moveDirection * currentSpeed + new Vector3(0, velocity.y, 0);
                controller.Move(finalMoveDirection * Time.deltaTime);
            }

            animator.SetFloat("isWalking", moveInput.magnitude);
            animator.SetBool("isSprinting", sprintInput);
        }
        else if (isSprintJumpAttacking)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public bool GetSprintInput() { return sprintInput; }

    public void SprintAttackMove(float distance, float duration)
    {
        // ��ǥ ��ġ ���
        Vector3 targetPosition = transform.position + transform.forward * distance;

        // �̵� �ڷ�ƾ ����
        StartCoroutine(PerformMoveForward(targetPosition, duration));
    }

    private IEnumerator PerformMoveForward(Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = transform.position;

        // duration ���� �̵�
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �̵� ����
        transform.position = targetPosition;
        sprintInput = false;
    }


    public void StartDodege()
    {
        isDodging = true;

        // ������ ��ǥ ��ġ ���
        rollStartPosition = transform.position;
        rollTargetPosition = rollStartPosition + transform.forward * rollDistance;

        // ������ ���� ����
        StartCoroutine(RollCoroutine());
    }

    public void EndDodege()     
    {
        controller.Move(rollTargetPosition * Time.deltaTime);
        isDodging = false;       
    }

    private IEnumerator RollCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < rollDuration)
        {
            // ������ �̵� ó��
            transform.position = Vector3.Lerp(rollStartPosition, rollTargetPosition, elapsedTime / rollDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}



public class PlayerMovement
{
    private Transform transform;
    private CharacterController controller;
    private Animator animator;

    private float moveSpeed;
    private float sprintSpeedMultiplier;
    private float gravity;
    private Vector3 velocity;
    private bool isSprinting;

    public PlayerMovement(Transform transform, CharacterController controller, Animator animator, float moveSpeed, float sprintSpeedMultiplier, float gravity)
    {
        this.transform = transform;
        this.controller = controller;
        this.animator = animator;
        this.moveSpeed = moveSpeed;
        this.sprintSpeedMultiplier = sprintSpeedMultiplier;
        this.gravity = gravity;
    }

    /// <summary>
    /// 이동 로직 처리
    /// </summary>
    public void Move(Vector2 input, bool sprintInput)
    {
        float speed = sprintInput ? moveSpeed * sprintSpeedMultiplier : moveSpeed;
        Vector3 direction = (transform.forward * input.y + transform.right * input.x).normalized;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        velocity.y += gravity * Time.deltaTime;
        Vector3 movement = direction * speed + new Vector3(0, velocity.y, 0);
        controller.Move(movement * Time.deltaTime);

        // 애니메이터 상태 업데이트
        animator.SetFloat("isWalking", direction.magnitude);
        animator.SetBool("isSprinting", sprintInput);

        isSprinting = sprintInput; // 스프린트 상태 저장
    }

    /// <summary>
    /// 스프린트 공격 이동 처리
    /// </summary>
    public void SprintAttackMove(float distance, float duration, MonoBehaviour monoBehaviour)
    {
        if (isSprinting)
        {
            Vector3 targetPosition = transform.position + transform.forward * distance;
            monoBehaviour.StartCoroutine(PerformMoveForward(targetPosition, duration));
        }
    }

    private IEnumerator PerformMoveForward(Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = transform.position;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isSprinting = false;
    }

    public bool IsSprinting()
    {
        return isSprinting;
    }
}


public class PlayerAttack
{
    private Animator animator;
    private PlayerStats stats;

    public PlayerAttack(Animator animator, PlayerStats stats)
    {
        this.animator = animator;
        this.stats = stats;
    }

    public void NormalAttack()
    {
        animator.SetTrigger("Attack");
    }

    public void SprintAttack()
    {
        animator.SetTrigger("SprintAttack");
    }

    public void JumpAttack()
    {
        animator.SetTrigger("JumpAttack");
    }

    public void ApplyDamage(int damage, PlayerDefense defense)
    {
        if (defense.IsGuarding)
        {
            defense.HandleGuard(damage);
        }
        else
        {
            stats.ReduceHP(damage);
            animator.SetTrigger("Hit");
        }
    }
}

public class PlayerDefense
{
    private Animator animator;
    public bool IsGuarding { get; private set; }

    public PlayerDefense(Animator animator)
    {
        this.animator = animator;
        this.IsGuarding = false;
    }

    public void StartGuard()
    {
        IsGuarding = true;
        animator.SetBool("isGuarding", true);
    }

    public void StopGuard()
    {
        IsGuarding = false;
        animator.SetBool("isGuarding", false);
    }

    public void HandleGuard(int damage)
    {
        // 방어 중 데미지 감소 처리
        Debug.Log($"Blocked {damage / 2} damage!");
    }
}

public class PlayerStats
{
    public int HP { get; private set; }
    public int MP { get; private set; }
    public int Level { get; private set; }
    public int EXP { get; private set; }

    public PlayerStats(int hp, int mp, int level)
    {
        HP = hp;
        MP = mp;
        Level = level;
        EXP = 0;
    }

    public void ReduceHP(int amount)
    {
        HP = Mathf.Max(0, HP - amount);
    }

    public void GainEXP(int amount)
    {
        EXP += amount;
        // Level up 처리 추가 가능
    }
}
