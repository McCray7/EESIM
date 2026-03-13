using UnityEngine;
using System.Collections;

public class DrawerInteraction : InteractableBase
{
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
            isOpened = true;
            targetPos = closedPos + openOffset;
            PlayerInteractionUI.IsGlobalLocked = true; // 锁定后进入鼠标点击模式
            if (drawerCamera != null) drawerCamera.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            isOpened = false;
            targetPos = closedPos;
            PlayerInteractionUI.IsGlobalLocked = false; // 解锁恢复正常模式
            if (drawerCamera != null) drawerCamera.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OpenDrawer()
    {
        isOpened = true;
        targetPos = closedPos + openOffset;
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
        PlayerInteractionUI.IsGlobalLocked = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}