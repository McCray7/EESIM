using UnityEngine;
using System.Collections;

public class CoffeeCup : HoldableItem
{
    [Header("咖啡设置")]
    public int coffeeCount = 3;
    public int maxCount = 3;

    // 准星对着桌上的咖啡杯时的提示（保持不变，用于拾取前）
    public override string InteractionPrompt => "左键 拿起咖啡杯";

    protected override void Awake()
    {
        base.Awake();
        // 1. 设置物品名称，对应 PlayerHandUI 的 itemNameText
        itemName = "咖啡杯"; 
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

    // 2. 重写基类方法，专门负责返回状态文字，对应 PlayerHandUI 的 statusText
    public override string GetItemStatus()
    {
        if (coffeeCount > 0)
        {
            return $"({coffeeCount}/{maxCount})";
        }
        else
        {
            return "(已空)";
        }
    }

    // 3. 更新操作提示文字，对应 PlayerHandUI 的 actionText
    private void UpdateHoldingPrompt()
    {
        if (coffeeCount > 0)
        {
            // 这里只保留操作指令，不再包含数量，因为数量已经由 GetItemStatus 处理了
            holdingPrompt = "[F] 饮用  [X] 丢弃";
        }
        else
        {
            holdingPrompt = "[X] 丢弃";
        }
    }
}