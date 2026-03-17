using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("核心数值")]
    public float health = 100f;
    public float maxHealth = 100f;
    
    public float sanity = 100f;
    public float maxSanity = 100f;

    public float hunger = 100f;
    public float maxHunger = 100f;

    public float bladder = 100f; // 100代表舒适，0代表内急
    public float maxBladder = 100f;

    [Header("基本信息")]
    public float money = 500f;
    public int age = 25;
    public float dayCounter = 0f; // 用于计算年龄增长

    [Header("消耗速率 (每秒)")]
    public float hungerDepletionRate = 0.5f;
    public float sanityDepletionRate = 0.2f;
    public float bladderDepletionRate = 0.3f;
    public float healthDrainRate = 2.0f; // 当任意数值为0时的扣血速度

    [Header("状态倍率")]
    public float stressMultiplier = 1f; // 之前任务面板提到的压力倍率

    public bool IsDead { get; private set; }

    void Update()
    {
        if (IsDead) return;

        // 1. 随时间消耗数值
        hunger = Mathf.Max(0, hunger - hungerDepletionRate * Time.deltaTime);
        bladder = Mathf.Max(0, bladder - bladderDepletionRate * Time.deltaTime);
        
        // 精神值消耗受压力倍率影响
        sanity = Mathf.Max(0, sanity - (sanityDepletionRate * stressMultiplier) * Time.deltaTime);

        // 2. 检查是否有数值归零，触发扣血
        if (hunger <= 0 || sanity <= 0 || bladder <= 0)
        {
            TakeDamage(healthDrainRate * Time.deltaTime);
        }

        // 3. 时间与年龄逻辑 (假设游戏中一分钟代表一天，或者根据你的需求调整)
        UpdateAge();
    }

    public void TakeDamage(float amount)
    {
        health = Mathf.Max(0, health - amount);
        if (health <= 0) Die();
    }

    private void UpdateAge()
    {
        dayCounter += Time.deltaTime;
        // 示例：每 360 秒（6分钟）增长一岁
        if (dayCounter >= 360f)
        {
            age++;
            dayCounter = 0;
            Debug.Log("祝贺你，又老了一岁！当前年龄：" + age);
        }
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log("角色已死亡");
        // 这里可以触发死亡UI弹出或重新加载场景
    }

    // 提供给其他脚本（如吃东西、上厕所）的修改接口
    public void AddHunger(float amount) => hunger = Mathf.Min(maxHunger, hunger + amount);
    public void AddBladder(float amount) => bladder = Mathf.Min(maxBladder, bladder + amount);
    public void AddSanity(float amount) => sanity = Mathf.Min(maxSanity, sanity + amount);
    public void AddMoney(float amount) => money += amount;
}