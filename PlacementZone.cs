using UnityEngine;
using System.Collections.Generic;

public class PlacementZone : InteractableBase
{
    [Header("放置点设置")]
    public Transform snapPoint; // 放置的精准位置点（建议建一个空物体作为子物体拖进来）
    
    [Header("准入名单")]
    [Tooltip("允许放置的物体名称列表。如果列表为空，则允许放置任何 HoldableItem。")]
    public List<string> allowedItemNames = new List<string>(); 

    // 🌟 核心：只有当手里有东西、没被占用、且东西在名单内时，才允许交互
    public override bool CanInteract 
    {
        get 
        {
            // 1. 全局锁定检查
            if (PlayerInteractionUI.IsGlobalLocked) return false;

            // 2. 必须手里拿了东西才能放
            if (!PlayerHand.Instance.HasItem) return false;

            // 3. 🌟 占用检查：如果已经有东西了，不允许再放
            if (IsOccupied()) return false;

            // 4. 🌟 名单检查：检查手里拿的是不是你要的那个东西
            return IsItemAllowed(PlayerHand.Instance.currentItem);
        }
    }

    // 动态提示文字
    public override string InteractionPrompt 
    {
        get 
        {
            if (IsOccupied()) return "位置已满";
            
            // 如果手里有东西但名单不对
            if (PlayerHand.Instance.HasItem && !IsItemAllowed(PlayerHand.Instance.currentItem))
                return "此处不匹配该物品";

            return "左键 放置物品";
        }
    }

    public override void Interact()
    {
        // 再次安全检查（防止快速双击等极端情况）
        if (IsOccupied()) return;

        HoldableItem itemToPlace = PlayerHand.Instance.currentItem;
        if (itemToPlace == null) return;

        // 1. 手部管理器释放该物品
        // 传入 false 确保它是“轻轻放下”，而不是“扔出去”
        PlayerHand.Instance.DropItem(false);

        // 2. 物理锁定：必须在 SetParent 之前或之后立即处理，防止它乱滚
        Rigidbody rb = itemToPlace.GetComponent<Rigidbody>();
        if (rb != null) 
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 3. 🌟 精准吸附并建立父子关系
        // 这样 IsOccupied 就能通过检测 snapPoint 的子物体数量来判断占用
        itemToPlace.transform.SetParent(snapPoint); 
        itemToPlace.transform.localPosition = Vector3.zero;
        itemToPlace.transform.localRotation = Quaternion.identity;

        Debug.Log($"{itemToPlace.name} 已完美放置在 {gameObject.name}");
    }

    // 🌟 判定函数：利用 Unity 层级树判断是否占用
    public bool IsOccupied()
    {
        if (snapPoint == null) return false;

        // 如果 snapPoint 下面有任何子物体，就认为是占用了
        // 这种方法比检查变量引用要稳定得多
        return snapPoint.childCount > 0;
    }

    // 🌟 判定函数：检查物品名单
    private bool IsItemAllowed(HoldableItem item)
    {
        if (item == null) return false;

        // 如果你没有在 Inspector 里设置任何名字，默认允许所有物品（方便测试）
        if (allowedItemNames.Count == 0) return true;

        // 清理掉 Unity 自动生成的 (Clone) 后缀
        string cleanName = item.name.Replace("(Clone)", "").Trim();

        // 检查名单
        return allowedItemNames.Contains(cleanName);
    }
}