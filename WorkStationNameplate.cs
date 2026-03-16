using UnityEngine;
using TMPro;

public class WorkStationNameplate : MonoBehaviour
{
    [Header("员工信息配置")]
    public string employeeName = "张三";
    public string department = "设备工程部";

    [Header("文字组件引用")]
    public TextMeshPro nameDisplay;
    public TextMeshPro deptDisplay;

    // 🌟 核心：在编辑器里改完点一下空白处，场景里的字和物体名立刻同步
    private void OnValidate()
    {
        if (nameDisplay != null) nameDisplay.text = employeeName;
        if (deptDisplay != null) deptDisplay.text = department;
        
        // 更新物体名字，让你在 Hierarchy 里一眼看出谁是谁
        this.gameObject.name = $"Nameplate_{employeeName}_{department}";
    }

    // 提供一个拼接后的唯一ID给 WorkStation
    public string GetUniqueID()
    {
        // 格式：部门_姓名 (例如：Equipment_ZhangSan)
        return $"{department}_{employeeName}";
    }
}