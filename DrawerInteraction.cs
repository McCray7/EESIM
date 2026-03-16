using UnityEngine;
using System.Collections;

public class DrawerInteraction : InteractableBase
{
    public static DrawerInteraction ActiveDrawer { get; private set; }

    [Header("设置")]
    public GameObject drawerCamera; 
    public Transform drawerTransform; 
    public Vector3 openOffset = new Vector3(0, 0, 0.5f); 
    public float moveSpeed = 5f;

    [Header("状态")]
    public bool isOpened = false; 

    private Vector3 closedPos;
    private Vector3 targetPos;

    // 权限由 PlayerInteractionUI 的 LayerMask 决定，这里始终允许
    public override bool CanInteract => true; 
    public override string InteractionPrompt => isOpened ? "左键 收回抽屉" : "左键 打开抽屉";

    protected override void Awake()
    {
        base.Awake();

        // 🌟 核心修复：如果 Inspector 里没拖入变量，就默认操作物体本身
        if (drawerTransform == null) drawerTransform = transform;
    
        // 这样下面这行就不会报错了
        closedPos = drawerTransform.localPosition;
        targetPos = closedPos;
    
        if (drawerCamera != null) drawerCamera.SetActive(false);
    }

    void Update()
    {
        drawerTransform.localPosition = Vector3.Lerp(drawerTransform.localPosition, targetPos, Time.deltaTime * moveSpeed);
    }

    public override void Interact()
    {
        if (!isOpened)
        {
            OpenDrawer();
        }
        else
        {
            CloseDrawer();
        }
    }

    public void OpenDrawer()
    {
        if (ActiveDrawer != null && ActiveDrawer != this)
        {
            ActiveDrawer.CloseDrawer();
        }

        isOpened = true;
        targetPos = closedPos + openOffset;
        ActiveDrawer = this;
        PlayerInteractionUI.IsGlobalLocked = true;
        if (drawerCamera != null) drawerCamera.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseDrawer()
    {
        isOpened = false;
        targetPos = closedPos;
        if (drawerCamera != null) drawerCamera.SetActive(false);

        if (ActiveDrawer == this)
        {
            ActiveDrawer = null;
        }

        PlayerInteractionUI.IsGlobalLocked = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}