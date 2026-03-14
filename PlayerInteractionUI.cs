using UnityEngine;
using TMPro;

public class PlayerInteractionUI : MonoBehaviour
{
    public static bool IsGlobalLocked = false;
    public static bool IsFirstPerson = false;
    public static Transform InteractionFocus = null;

    public float interactRange = 6f;
    public float seatedInteractRange = 2f;

    public LayerMask standInteractLayer;
    public LayerMask seatedInteractLayer;

    public TextMeshProUGUI promptText; // 现在这个 text 只显示交互物体的文字
    public GameObject crosshairImage;

    private IInteractable currentInteractable;

    void Update()
    {
        LayerMask currentLayerMask = IsFirstPerson ? seatedInteractLayer : standInteractLayer;

        if (IsGlobalLocked)
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                if (crosshairImage != null) crosshairImage.SetActive(false);
                MouseRaycastCheck(currentLayerMask);

                if (Input.GetMouseButtonDown(0) && currentInteractable != null)
                {
                    // 🌟 核心修正：当点下鼠标，直接触发物体的交互
                    currentInteractable.Interact();
                }
            }
            return;
        }

        bool canAimAndInteract = IsFirstPerson || Input.GetMouseButton(1);
        ToggleUI(canAimAndInteract);

        if (!canAimAndInteract) { ClearInteraction(); return; }

        RaycastCheck(currentLayerMask);

        if (Input.GetMouseButtonDown(0) && currentInteractable != null && currentInteractable.CanInteract)
        {
            currentInteractable.Interact();
        }
    }

    void MouseRaycastCheck(LayerMask filterMask)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        float dist = IsFirstPerson ? seatedInteractRange : interactRange;

        // 🌟 核心修改：移除这里的 filterMask，让射线能撞击任何有 Collider 的物体
        // 或者使用一个包含“建筑/环境”和“交互层”的组合 Mask
        if (Physics.Raycast(ray, out hit, dist)) 
        {
            // 检查撞击到的物体是否在我们的交互层级中
            // 使用位运算：(1 << layer) & mask
            if (((1 << hit.collider.gameObject.layer) & filterMask) != 0)
            {
                ProcessInteractable(hit.collider.GetComponent<IInteractable>());
            }
            else
            {
                // 如果撞到了东西（比如桌面），但它不是交互层，就清除交互
                ClearInteraction();
            }
        }
        else { ClearInteraction(); }
    }

    void RaycastCheck(LayerMask filterMask)
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;
        float dist = IsFirstPerson ? seatedInteractRange : interactRange;

        // 🌟 不带 mask 进行检测，确保桌面能挡住射线
        if (Physics.Raycast(ray, out hit, dist))
        {
            if (((1 << hit.collider.gameObject.layer) & filterMask) != 0)
            {
                ProcessInteractable(hit.collider.GetComponent<IInteractable>());
            }
            else
            {
                ClearInteraction();
            }
        }
        else { ClearInteraction(); }
    }

    void ProcessInteractable(IInteractable interactable)
    {
        if (interactable != null && interactable.CanInteract)
        {
            if (currentInteractable != interactable)
            {
                if (currentInteractable != null) currentInteractable.OnExit();
                currentInteractable = interactable;
                currentInteractable.OnHover();
            }

            // 🌟 只显示物体的交互提示（如 "放置物品" 或 "打开抽屉"）
            if (promptText != null) promptText.text = interactable.InteractionPrompt;
        }
        else { ClearInteraction(); }
    }

    void ToggleUI(bool isActive)
    {
        if (crosshairImage != null)
            crosshairImage.SetActive(Cursor.lockState == CursorLockMode.Locked && isActive);
        if (promptText != null) promptText.gameObject.SetActive(isActive);
    }

    void ClearInteraction()
    {
        if (currentInteractable != null) { currentInteractable.OnExit(); currentInteractable = null; }
        if (promptText != null) promptText.text = "";
    }
}