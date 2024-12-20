using UnityEngine;
using UnityEngine.EventSystems;

public class Player : BaseEntity
{
    private float speed = 5f;                  // �̵� �ӵ�
    private float gravity = -9.81f;           // �߷� ��
    private float jumpHeight = 2f;            // ���� ����
    private float experience;

    private float horizontal;
    private float vertical;

    private CharacterController controller;  // CharacterController ����
    private Vector3 velocity;                // ���� �ӵ�
    private bool isGrounded;                 // �ٴ� üũ    

    public Transform cameraTransform;        // ���� ī�޶��� Transform
    public float rotationSpeed = 10f;        // ȸ�� �ӵ�
    Vector3 moveDirection;

    protected override void Start()
    {
        base.Start();

        controller = GetComponent<CharacterController>();

        Initialize("player", 1, 10);
    }

    protected override void Update()
    {
        // �ٴ� üũ
        isGrounded = controller.isGrounded;

        // �̵� ó��
        if (moveDirection.magnitude >= 0.1f)
        {
            // Animator�� "isWalking" ����
            animator.SetBool("isWalking", true);
        }
        else
        {
            // �̵����� ���� �� "isWalking" ����
            animator.SetBool("isWalking", false);
        }

        // ����
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump"); // ���� Ʈ���� ����
        }
    }

    protected override void FixedUpdate()
    {
        if (!IsAnimationPlaying("H2H_AxeKick_InPlace"))
        {
            // ���鿡 ������ Y�� �ӵ� �ʱ�ȭ
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // ����Ű �Է� �ޱ�
            horizontal = Input.GetAxis("Horizontal"); // A/D �Ǵ� ��/�� ����Ű
            vertical = Input.GetAxis("Vertical");     // W/S �Ǵ� ��/�� ����Ű

            // ī�޶� ���� �̵� ���� ���
            moveDirection = (cameraTransform.right * horizontal + cameraTransform.forward * vertical).normalized;

            // ���� �� ���� (Y �� ���� �̵� ����)
            moveDirection.y = 0;

            // �̵� ó��
            if (moveDirection.magnitude >= 0.1f)
            {
                // ��ǥ ȸ�� ���� ���
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

                // �ε巯�� ȸ��
                float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, angle, 0);

                // �̵� ����
                controller.Move(moveDirection * speed * Time.deltaTime);
            }

            // �߷� ����
            velocity.y += gravity * Time.deltaTime;

            // �߷� �ӵ� ����
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

        // ����ġ�� ���� ����ġ�� �ʰ��ϴ� ���� ��� ������
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

        // ���� ���� �̸��� ��
        return stateInfo.IsName(stateName);
    }
}

