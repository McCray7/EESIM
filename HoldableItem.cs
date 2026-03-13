using UnityEngine;

public abstract class HoldableItem : InteractableBase
{
    public string itemName = "未命名物品"; // 🌟 新增：物品名称
    [Header("手持状态提示文字")]
    public string holdingPrompt = "按 F 使用 | X 丢弃";

    // 准星对着它，且手是空的，才允许抓取
    public override bool CanInteract => !PlayerHand.Instance.HasItem && !PlayerInteractionUI.IsGlobalLocked;

    // 左键点击它时执行抓取
    public override void Interact()
    {
        PlayerHand.Instance.PickUp(this);
    }

    // 🌟 修改：现在返回一个 bool，告诉手部是否成功使用了
    public abstract bool UseItem();
    
    public virtual string GetItemStatus() { return ""; }
}