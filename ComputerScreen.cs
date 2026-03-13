using UnityEngine;
using Cinemachine;
using System.Collections;

public class ComputerScreen : InteractableBase
{
    [Header("相机与UI引用")]
    public CinemachineVirtualCamera monitorVcam;
    public CanvasGroup screenCanvasGroup; 
    public GameObject interactionUI;

    [Header("设置")]
    public float fadeSpeed = 2f; 
    public AudioSource audioSource; 
    public AudioClip startupSound;  

    private bool isFocusing = false;
    private bool isPowerOn = false; 
    private Coroutine fadeCoroutine;

    // 只要没进入特写且全局未锁定，就允许交互（坐下后 Layer 切换会自动处理权限）
    public override bool CanInteract => !isFocusing && !PlayerInteractionUI.IsGlobalLocked;
    public override string InteractionPrompt => "左键 进入办公模式";

    public override void Interact()
    {
        isFocusing = true;
        PlayerInteractionUI.IsGlobalLocked = true; // 锁定UI和鼠标
        if (monitorVcam != null) { monitorVcam.gameObject.SetActive(true); monitorVcam.Priority = 100; }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (interactionUI != null) interactionUI.SetActive(true);
    }

    public void TogglePower()
    {
        isPowerOn = !isPowerOn;
        if (isPowerOn && audioSource != null && startupSound != null) audioSource.PlayOneShot(startupSound);
        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeScreen(isPowerOn ? 1f : 0f));
    }

    IEnumerator FadeScreen(float targetAlpha)
    {
        float startAlpha = screenCanvasGroup.alpha;
        float time = 0;
        while (time < 1f)
        {
            time += Time.deltaTime * fadeSpeed;
            screenCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time);
            yield return null;
        }
        screenCanvasGroup.alpha = targetAlpha;
        screenCanvasGroup.blocksRaycasts = (targetAlpha > 0.1f);
        screenCanvasGroup.interactable = (targetAlpha > 0.1f);
    }

    public void StopFocusing()
    {
        isFocusing = false;
        PlayerInteractionUI.IsGlobalLocked = false;
        if (monitorVcam != null) monitorVcam.Priority = 0;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}