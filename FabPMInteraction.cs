using UnityEngine;

// 🌟 继承你写好的 InteractableBase，这样准星对准机器会有发光提示和文字！
public class FabPMInteraction : InteractableBase
{
    [Header("机台设置")]
    [Tooltip("机台ID，要和 DailyTaskBoard 的目标机台ID 一致，例如 NCECU21")]
    public string machineId = "NCECU21";

    // 准星看向机台时的提示文字
    public override string InteractionPrompt => $"左键 对机台 {machineId} 进行 PM";

    // 只要没打开全局锁定（比如没在看电脑），就可以交互
    public override bool CanInteract => !PlayerInteractionUI.IsGlobalLocked;

    public override void Interact()
    {
        if (DailyTaskBoard.Instance == null)
        {
            Debug.LogWarning("[FabPMInteraction] 场景中未找到 DailyTaskBoard，无法记录 PM。");
            return;
        }

        // 🌟 关键：向任务板提交此机台的 PM 完成请求
        DailyTaskBoard.Instance.RecordPMCompletion(machineId);
        
        // 视觉/音效反馈可以在这里加
        Debug.Log($"[FabPMInteraction] 玩家正在维修机台：{machineId}");
        
        // 可选：交互后让机台暂时无法交互，或者改变颜色
    }
}