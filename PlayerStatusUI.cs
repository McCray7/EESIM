using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("引用")]
    public PlayerStats stats;

    [Header("折叠模式 (图标)")]
    public GameObject foldedContainer;
    public Image healthIconFill;
    public Image sanityIconFill;
    public Image hungerIconFill;
    public Image bladderIconFill;

    [Header("展开模式 (详细)")]
    public CanvasGroup detailedPanelCG; 
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public Slider sanitySlider;
    public TextMeshProUGUI sanityText;
    public Slider hungerSlider;
    public TextMeshProUGUI hungerText;
    public Slider bladderSlider;
    public TextMeshProUGUI bladderText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI ageText;

    private bool isExpanded = false;

    void Start()
    {
        SetExpandedState(false);
    }

    void Update()
    {
        // 🌟 修改点 1：将 H 改为 Tab 键
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetExpandedState(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            SetExpandedState(false);
        }

        UpdateVisuals();
    }

    private void SetExpandedState(bool expand)
    {
        isExpanded = expand;

        // 1. 处理详细面板显隐
        if (detailedPanelCG != null)
        {
            detailedPanelCG.alpha = expand ? 1f : 0f;
            detailedPanelCG.interactable = expand;
            detailedPanelCG.blocksRaycasts = expand;
        }

        // 2. 鼠标与锁定逻辑处理
        if (expand)
        {
            // 打开面板时，无论如何都要显示鼠标
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 🌟 修改点 2 (修复 Bug)：关闭面板时进行逻辑判断
            // 检查全局变量 PlayerInteractionUI.IsGlobalLocked
            // 如果为 true，说明玩家正在“办公模式”或“抽屉模式”，鼠标应该保持自由，不予干涉
            // 如果为 false，说明玩家在“行走模式”，此时应该锁定鼠标
            if (!PlayerInteractionUI.IsGlobalLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                // 如果当前已经是锁定状态（比如坐着但没看电脑），则不进行额外操作
                // 保持现状即可，因为办公系统会自行接管鼠标
            }
        }
    }

    private void UpdateVisuals()
    {
        if (stats == null) return;

        // --- 更新图标水位 ---
        if (healthIconFill) healthIconFill.fillAmount = stats.health / stats.maxHealth;
        if (sanityIconFill) sanityIconFill.fillAmount = stats.sanity / stats.maxSanity;
        if (hungerIconFill) hungerIconFill.fillAmount = stats.hunger / stats.maxHunger;
        if (bladderIconFill) bladderIconFill.fillAmount = stats.bladder / stats.maxBladder;

        // --- 更新详细数值 (仅展开时) ---
        if (isExpanded)
        {
            if (healthSlider) healthSlider.value = stats.health / stats.maxHealth;
            if (healthText) healthText.text = $"{(int)stats.health} / {(int)stats.maxHealth}";

            if (sanitySlider) sanitySlider.value = stats.sanity / stats.maxSanity;
            if (sanityText) sanityText.text = $"{(int)stats.sanity}%";

            if (hungerSlider) hungerSlider.value = stats.hunger / stats.maxHunger;
            if (hungerText) hungerText.text = $"{(int)stats.hunger}%";

            if (bladderSlider) bladderSlider.value = stats.bladder / stats.maxBladder;
            if (bladderText) bladderText.text = $"{(int)stats.bladder}%";

            if (moneyText) moneyText.text = $"资产: ¥ {stats.money:F2}";
            if (ageText) ageText.text = $"年龄: {stats.age} 岁";
        }
    }
}