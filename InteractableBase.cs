using UnityEngine;

// 继承 MonoBehaviour 并实现 IInteractable 接口
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [Header("基础交互设置")]
    public string defaultPrompt = "左键交互";

    // 允许子类重写这些属性
    public virtual string InteractionPrompt => defaultPrompt;
    public virtual bool CanInteract => !PlayerInteractionUI.IsGlobalLocked;

    protected MeshRenderer meshRenderer;
    protected Color originalColor;
    protected bool isHighlighted = false;

    protected virtual void Awake()
    {
        // 自动获取模型渲染器（支持子物体）
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null) 
        {
            originalColor = meshRenderer.material.color;
        }
    }

    // 强制要求子类必须写自己的 Interact 逻辑
    public abstract void Interact();

    // 默认的高亮逻辑：开启自发光
    public virtual void OnHover()
    {
        if (isHighlighted) return;
        isHighlighted = true;
        
        if (meshRenderer != null && meshRenderer.material.HasProperty("_EmissionColor"))
        {
            meshRenderer.material.EnableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", Color.white * 0.3f); 
        }
    }

    // 默认的取消高亮逻辑：关闭自发光
    public virtual void OnExit()
    {
        isHighlighted = false;
        
        if (meshRenderer != null && meshRenderer.material.HasProperty("_EmissionColor"))
        {
            meshRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }
}