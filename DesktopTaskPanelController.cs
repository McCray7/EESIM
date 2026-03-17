using UnityEngine;

public class DesktopTaskPanelController : MonoBehaviour
{
    [Header("引用")]
    public CanvasGroup taskPanelCanvasGroup;
    public DailyTaskBoard taskBoard;
    public PlayerStats playerStats;

    [Header("行为")]
    public bool openPanelOnStart;
    public float stressWhenOpen = 5f;

    private bool isOpen;

    private void Start()
    {
        SetPanel(openPanelOnStart);
    }

    public void OnTaskIconClicked()
    {
        SetPanel(!isOpen);
    }

    public void ClosePanel()
    {
        SetPanel(false);
    }

    private void SetPanel(bool visible)
    {
        isOpen = visible;

        if (taskPanelCanvasGroup != null)
        {
            taskPanelCanvasGroup.alpha = visible ? 1f : 0f;
            taskPanelCanvasGroup.interactable = visible;
            taskPanelCanvasGroup.blocksRaycasts = visible;
        }

        if (taskBoard != null && visible)
        {
            taskBoard.RefreshTaskUI();
        }

        if (playerStats != null)
        {
            playerStats.stressMultiplier = visible ? stressWhenOpen : 1f;
        }
    }
}