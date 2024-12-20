using UnityEngine;
using System.Collections;

public class BaseEntity : MonoBehaviour
{
    public string entityName;

    [SerializeField] protected int LEVEL;
    [SerializeField] protected int MAX_HP;
    [SerializeField] protected int CURRENT_HP;

    [SerializeField] public int ATK { get; protected set; }
    [SerializeField] public int DEF { get; protected set; }

    [SerializeField] protected Animator animator;


    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    // 레벨 스케일링 적용하기
    public virtual void Initialize(string name, int level, int health)
    {
        this.LEVEL = level;
        entityName = name;
        MAX_HP = health;
        CURRENT_HP = health;

        Debug.Log($"{entityName} is initialized");
    }

    public virtual void Attack(GameObject target)
    {
        animator.SetTrigger("Attack");

        BaseEntity enemy = target.GetComponent<BaseEntity>();

        enemy.Damaged(CalculateDamage(enemy));
    }

    private int CalculateDamage(BaseEntity target)
    {
        int damage = Mathf.Max(0, ATK - target.DEF);        // 데미지가 음수가 되는 것을 방지
        Debug.Log($"{entityName} attacks {target.entityName} for {damage} damage!");
        return damage;
    }

    public virtual void Damaged(int damage)
    {
        Debug.Log($"{entityName} is damaged!");
        CURRENT_HP -= damage;
        if (CURRENT_HP > 0) animator.SetTrigger("Damaged");
        else Die();
    }    

    public virtual void UsingSkill()
    {
        Debug.Log($"{entityName} has used skill");

        animator.SetTrigger("UsingSkill");
    }

    public virtual void Die()
    {
        Debug.Log($"{entityName} has died.");

        animator.SetTrigger("Died");

        StartCoroutine(DisableAfterDelay(5f));
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
