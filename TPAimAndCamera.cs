using UnityEngine;
using Cinemachine;

public class TPAimAndCamera : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;
    private Cinemachine3rdPersonFollow thirdPersonFollow;
    
    [Header("右键瞄准设置")]
    public float normalFOV = 40f;   // 第三视角正常视野
    public float aimFOV = 25f;      // 按住右键放大的视野
    public float zoomSpeed = 10f;   // 放大缩小的速度

    [Header("滚轮距离设置")]
    public float scrollSpeed = 2f;  // 滚轮缩放速度
    public float minDistance = 1.5f;// 最近距离
    public float maxDistance = 6f;  // 最远距离

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            // 假设你用的是 StarterAssets 默认的 3rd Person Follow 身体类型
            thirdPersonFollow = vcam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            
            // 初始化 FOV 和距离
            vcam.m_Lens.FieldOfView = normalFOV;
        }
    }

    void Update()
    {
        // 🌟 如果是第一人称，或者是全局锁定状态，跳过这些操作
        if (PlayerInteractionUI.IsFirstPerson || PlayerInteractionUI.IsGlobalLocked)
        {
            if (vcam != null) vcam.m_Lens.FieldOfView = normalFOV; // 恢复正常FOV
            return;
        }

        // 🌟 1. 右键瞄准放大镜头 (FOV 缩放)
        bool isAiming = Input.GetMouseButton(1);
        float targetFOV = isAiming ? aimFOV : normalFOV;
        
        if (vcam != null)
        {
            vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        }

        // 🌟 2. 滚轮控制距离 (第三人称摄像机到人的距离)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f && thirdPersonFollow != null)
        {
            // 往上滚 scrollInput 是正数，减去它表示拉近
            thirdPersonFollow.CameraDistance = Mathf.Clamp(thirdPersonFollow.CameraDistance - scrollInput * scrollSpeed, minDistance, maxDistance);
        }
    }
}