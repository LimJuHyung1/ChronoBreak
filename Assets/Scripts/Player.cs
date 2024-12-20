using UnityEngine;
using UnityEngine.EventSystems;

public class Player : BaseEntity
{
    private float speed = 5f;                  // 이동 속도
    private float gravity = -9.81f;           // 중력 값
    private float jumpHeight = 2f;            // 점프 높이
    private float experience;

    private float horizontal;
    private float vertical;

    private CharacterController controller;  // CharacterController 참조
    private Vector3 velocity;                // 현재 속도
    private bool isGrounded;                 // 바닥 체크    

    public Transform cameraTransform;        // 메인 카메라의 Transform
    public float rotationSpeed = 10f;        // 회전 속도
    Vector3 moveDirection;

    protected override void Start()
    {
        base.Start();

        controller = GetComponent<CharacterController>();

        Initialize("player", 1, 10);
    }

    protected override void Update()
    {
        // 바닥 체크
        isGrounded = controller.isGrounded;

        // 이동 처리
        if (moveDirection.magnitude >= 0.1f)
        {
            // Animator에 "isWalking" 설정
            animator.SetBool("isWalking", true);
        }
        else
        {
            // 이동하지 않을 때 "isWalking" 해제
            animator.SetBool("isWalking", false);
        }

        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump"); // 점프 트리거 실행
        }
    }

    protected override void FixedUpdate()
    {
        if (!IsAnimationPlaying("H2H_AxeKick_InPlace"))
        {
            // 지면에 있으면 Y축 속도 초기화
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // 방향키 입력 받기
            horizontal = Input.GetAxis("Horizontal"); // A/D 또는 좌/우 방향키
            vertical = Input.GetAxis("Vertical");     // W/S 또는 상/하 방향키

            // 카메라 기준 이동 방향 계산
            moveDirection = (cameraTransform.right * horizontal + cameraTransform.forward * vertical).normalized;

            // 수직 축 제거 (Y 축 방향 이동 방지)
            moveDirection.y = 0;

            // 이동 처리
            if (moveDirection.magnitude >= 0.1f)
            {
                // 목표 회전 각도 계산
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

                // 부드러운 회전
                float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, angle, 0);

                // 이동 적용
                controller.Move(moveDirection * speed * Time.deltaTime);
            }

            // 중력 적용
            velocity.y += gravity * Time.deltaTime;

            // 중력 속도 적용
            controller.Move(velocity * Time.deltaTime);
        }            
    }

    public override void Initialize(string name, int level, int health)
    {
        base.Initialize(name, level, health);
        experience = 0;
        ATK = 10;
        DEF = 5;
    }

    public void GainExperience(int exp)
    {
        experience += exp;
        Debug.Log($"{entityName} gained {exp} experience.");

        // 경험치가 레벨 기준치를 초과하는 동안 계속 레벨업
        while (experience >= LEVEL * 100)
        {
            LevelUp();
        }
    }


    private void LevelUp()
    {
        LEVEL++;
        experience -= 100;
        MAX_HP += 10;
        CURRENT_HP = MAX_HP;
        ATK += 5;
        DEF += 3;

        Debug.Log($"{entityName} leveled up! Level: {LEVEL}, Health: {MAX_HP}, Attack: {ATK}, Defense: {DEF}");
    }

    public override void Attack(GameObject target)
    {
        base.Attack(target);
    }

    public override void Damaged(int damage)
    {
        base.Damaged(damage);
    }

    public override void UsingSkill()
    {
        base.UsingSkill();
    }

    public override void Die()
    {
        base.Die();
    }



    private bool IsAnimationPlaying(string stateName, int layerIndex = 0)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

        // 현재 상태 이름과 비교
        return stateInfo.IsName(stateName);
    }
}

