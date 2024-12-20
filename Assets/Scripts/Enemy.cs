using UnityEngine;

public class Enemy : BaseEntity
{
    protected override void Start()
    {
        base.Start();
        Initialize("slime", 1, 10);
    }

    public override void Initialize(string name, int level, int health)
    {
        base.Initialize(name, level, health);
        ATK = 3;
        DEF = 5;
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
}
