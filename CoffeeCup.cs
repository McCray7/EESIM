using UnityEngine;
using System.Collections;

public class CoffeeCup : HoldableItem
{
    [Header("咖啡设置")]
    public int coffeeCount = 3;
    public int maxCount = 3;

    // 准星对着桌上的咖啡杯时的提示
    public override string InteractionPrompt => "左键 拿起咖啡杯";

    protected override void Awake()
    {
        base.Awake();
        UpdateHoldingPrompt(); 
    }

    public override bool UseItem()
    {
        if (coffeeCount > 0)
        {
            coffeeCount--;
            UpdateHoldingPrompt(); 
            Debug.Log($"喝了一口咖啡，剩余: {coffeeCount}/{maxCount}");
            return true;
        }
        else
        {
            Debug.Log("杯子已经空了！");
            return false;
        }
    }

    // 🌟 修复：这里的文字必须改为 X，否则玩家会很困惑
    private void UpdateHoldingPrompt()
    {
        if (coffeeCount > 0)
        {
            holdingPrompt = $"按 F 饮用 ({coffeeCount}/{maxCount}) | X 丢弃";
        }
        else
        {
            holdingPrompt = "杯子已空 | X 丢弃";
        }
    }
}