using UnityEngine;
using TMPro;

public class PlayerInteractionUI : MonoBehaviour
{
    public static bool IsGlobalLocked = false; 
    public static bool IsFirstPerson = false; 

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

        if (Physics.Raycast(ray, out hit, dist, filterMask))
        {
            ProcessInteractable(hit.collider.GetComponent<IInteractable>());
            // 文字跟随鼠标
            if (promptText != null && currentInteractable != null)
                promptText.transform.position = Input.mousePosition + new Vector3(20, -20, 0);
        }
        else { ClearInteraction(); }
    }

    void RaycastCheck(LayerMask filterMask)
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;
        float dist = IsFirstPerson ? seatedInteractRange : interactRange;

        if (Physics.Raycast(ray, out hit, dist, filterMask))
        {
            ProcessInteractable(hit.collider.GetComponent<IInteractable>());
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