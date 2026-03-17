using TMPro;
using UnityEngine;
using System.Collections;

public class ComputerLogin : MonoBehaviour
{
    [Header("数据引用")]
    public EmployeeDatabase employeeDb; // 拖入刚才创建的数据库文件

    [Header("UI引用 (需挂载CanvasGroup)")]
    public CanvasGroup loginCanvasGroup;
    public CanvasGroup desktopCanvasGroup;
    
    public TMP_InputField employeeIdInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI messageText;

    [Header("动画设置")]
    public float fadeDuration = 0.5f;

    public bool IsLoggedIn { get; private set; }
    public string CurrentEmployeeId { get; private set; }
    public static string ActiveEmployeeId { get; private set; }
    private bool isPoweredOn;
    private Coroutine currentFadeCoroutine;

    private void Start()
    {
        SetPanelAlpha(loginCanvasGroup, 0f);
        SetPanelAlpha(desktopCanvasGroup, 0f);
    }

    private void Update()
    {
        if (!isPoweredOn || IsLoggedIn) return;

        // 🌟 优化1：Tab 键切换输入框
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (employeeIdInput.isFocused) passwordInput.ActivateInputField();
            else if (passwordInput.isFocused) employeeIdInput.ActivateInputField();
        }

        // 🌟 优化2：Enter 键直接尝试登录
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            AttemptLogin();
        }
    }

    public void ApplyPowerState(bool powerOn)
    {
        isPoweredOn = powerOn;

        if (!isPoweredOn)
        {
            IsLoggedIn = false;
            CurrentEmployeeId = string.Empty;
            StartTransition(null); 
            ClearInputs();
            return;
        }

        IsLoggedIn = false;
        CurrentEmployeeId = string.Empty;
        StartTransition(loginCanvasGroup);
        ClearInputs();
        SetMessage("请输入工号以继续");
    }

    public void AttemptLogin()
    {
        if (!isPoweredOn || IsLoggedIn) return;

        string enteredId = employeeIdInput.text.Trim();
        string enteredPw = passwordInput.text;

        // 🌟 优化3：从数据库验证
        if (employeeDb != null && employeeDb.ValidateLogin(enteredId, enteredPw))
        {
            IsLoggedIn = true;
            CurrentEmployeeId = enteredId;
            ActiveEmployeeId = enteredId;
            StartTransition(desktopCanvasGroup);
            SetMessage(string.Empty);
        }
        else
        {
            SetMessage("<color=red>工号或密码错误</color>");
            passwordInput.text = string.Empty;
            passwordInput.ActivateInputField();
        }
    }

    // --- 以下保持渐变逻辑基本不变 ---

    private void StartTransition(CanvasGroup targetPanel)
    {
        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = StartCoroutine(FadeRoutine(targetPanel));
    }

    IEnumerator FadeRoutine(CanvasGroup targetPanel)
    {
        yield return StartCoroutine(FadeAlpha(loginCanvasGroup, 0f));
        yield return StartCoroutine(FadeAlpha(desktopCanvasGroup, 0f));

        if (targetPanel != null)
        {
            yield return StartCoroutine(FadeAlpha(targetPanel, 1f));
            if (targetPanel == loginCanvasGroup && employeeIdInput != null)
                employeeIdInput.ActivateInputField();
        }
    }

    IEnumerator FadeAlpha(CanvasGroup cg, float target)
    {
        if (cg == null) yield break;
        float startAlpha = cg.alpha;
        float elapsed = 0;

        if (target > 0.1f) { cg.blocksRaycasts = true; cg.interactable = true; }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, target, elapsed / fadeDuration);
            yield return null;
        }

        cg.alpha = target;
        if (target <= 0.1f) { cg.blocksRaycasts = false; cg.interactable = false; }
    }

    private void SetPanelAlpha(CanvasGroup cg, float alpha)
    {
        if (cg == null) return;
        cg.alpha = alpha;
        cg.interactable = alpha > 0.1f;
        cg.blocksRaycasts = alpha > 0.1f;
    }

    private void ClearInputs()
    {
        if (employeeIdInput != null) employeeIdInput.text = string.Empty;
        if (passwordInput != null) passwordInput.text = string.Empty;
    }

    private void SetMessage(string message)
    {
        if (messageText != null) messageText.text = message;
    }
}