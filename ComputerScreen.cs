using UnityEngine;
using Cinemachine;
using System.Collections;

public class ComputerScreen : InteractableBase
{
    [Header("相机与UI引用")]
    public CinemachineVirtualCamera monitorVcam;
    public CanvasGroup screenCanvasGroup; 
    public GameObject interactionUI;
    public ComputerLogin computerOS;

    [Header("设置")]
    public float fadeSpeed = 2f; 
    public AudioSource audioSource; 
    public AudioClip startupSound;  
    public AudioClip shutdownSound; // 🌟 新增：关机音效

    private bool isFocusing = false;
    private bool isPowerOn = false; 
    private Coroutine fadeCoroutine;

    public override bool CanInteract => !isFocusing && !PlayerInteractionUI.IsGlobalLocked;
    public override string InteractionPrompt => "左键 进入办公模式";

    public override void Interact()
    {
        isFocusing = true;
        PlayerInteractionUI.IsGlobalLocked = true; 
        if (monitorVcam != null) { monitorVcam.gameObject.SetActive(true); monitorVcam.Priority = 100; }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (interactionUI != null) interactionUI.SetActive(true);
    }

    public void TogglePower()
    {
        isPowerOn = !isPowerOn;

        // 🌟 修改：根据开关状态播放对应的音效
        if (audioSource != null)
        {
            if (isPowerOn)
            {
                if (startupSound != null) audioSource.PlayOneShot(startupSound);
            }
            else
            {
                if (shutdownSound != null) audioSource.PlayOneShot(shutdownSound);
            }
        }
        
        if (computerOS != null)
        {
            computerOS.ApplyPowerState(isPowerOn);
        }

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