using UnityEngine;
using TMPro;

public class PlayerHandUI : MonoBehaviour
{
    public static PlayerHandUI Instance;

    [Header("UI 容器")]
    public GameObject handUIPanel;      // 整个右下角提示的父物体

    [Header("文字组件")]
    public TextMeshProUGUI itemNameText;  // 显示：咖啡杯
    public TextMeshProUGUI statusText;    // 显示：(3/3)
    public TextMeshProUGUI actionText;    // 显示：[F] 饮用  [X] 丢弃

    void Awake()
    {
        Instance = this;
        if(handUIPanel != null) handUIPanel.SetActive(false);
    }

    void Update()
    {
        // 每一帧同步手部状态
        if (PlayerHand.Instance != null && PlayerHand.Instance.HasItem)
        {
            if (!handUIPanel.activeSelf) handUIPanel.SetActive(true);
            
            HoldableItem item = PlayerHand.Instance.currentItem;
            
            // 更新 UI 内容
            if (itemNameText != null) itemNameText.text = item.itemName;
            if (statusText != null) statusText.text = item.GetItemStatus();
            if (actionText != null) actionText.text = item.holdingPrompt;
        }
        else
        {
            if (handUIPanel.activeSelf) handUIPanel.SetActive(false);
        }
    }
}