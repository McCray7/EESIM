using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("核心属性")]
    public float hp = 100f;
    public float sanity = 100f;
    public float hunger = 100f;
    public float age = 25f;

    [Header("基础消耗速率")]
    public float sanityDrainBase = 1f;
    public float hungerDrainBase = 0.5f;
    public float hpDamageRate = 5f; // 精神归零后的扣血速度

    [Header("当前压力倍率")]
    public float stressMultiplier = 1f; // 正常为1，开任务列表变5

    void Update()
    {
        // 1. 基础消耗
        hunger -= hungerDrainBase * Time.deltaTime;
        sanity -= (sanityDrainBase * stressMultiplier) * Time.deltaTime;

        // 2. 限制范围
        hp = Mathf.Clamp(hp, 0, 100);
        sanity = Mathf.Clamp(sanity, 0, 100);
        hunger = Mathf.Clamp(hunger, 0, 100);

        // 3. 生存逻辑：精神或饥饿为0，开始扣血
        if (sanity <= 0 || hunger <= 0)
        {
            hp -= hpDamageRate * Time.deltaTime;
        }

        if (hp <= 0) Debug.Log("你倒在了工位上...");
    }
}