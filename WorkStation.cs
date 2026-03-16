using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;

public class WorkStation : MonoBehaviour
{
    [Header("工位身份")]
    public string workstationID; // 自动获取的 ID
    public WorkStationNameplate nameplate; // 拖入子物体中的姓名牌脚本
    
    [Header("引用")]
    public GameObject player;
    public Camera mainCam;
    
    // 🌟 修改1：改为 GameObject，因为它应该是一个 CinemachineVirtualCamera，而不是物理相机
    public GameObject firstPersonCam; 
    
    public Transform sitPoint;    // 坐下的位置
    public Transform standPoint;  // 🌟 起立后的安全位置

    [Header("设置")]
    public float transitionSpeed = 2f;
    public float standUpTime = 1.5f; 

    private bool isNear = false;
    private bool isWorking = false;
    private bool isTransitioning = false;

    private MonoBehaviour controller; 
    private Animator animator;
    private CharacterController charController;
    
    [Header("UI设置")]
    public GameObject interactionUI;

    void Start()
    {
        if (nameplate != null)
        {
            workstationID = nameplate.GetUniqueID(); 
            Debug.Log($"工位已加载：[{workstationID}]");
        }
        if (player != null)
        {
            controller = player.GetComponent("ThirdPersonController") as MonoBehaviour;
            animator = player.GetComponent<Animator>();
            charController = player.GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            // 🌟 核心修复 (Bug 1)：如果当前处于全局锁定状态（正在喝咖啡或看屏幕），直接无视 E 键！
            if (PlayerInteractionUI.IsGlobalLocked) 
            {
                Debug.Log("正在交互中，无法站起！请先按 Esc 退出办公模式。");
                return; 
            }

            if (isWorking) StartCoroutine(ExitWorkRoutine());
            else if (isNear) StartCoroutine(EnterWorkRoutine());
        }
    }

    IEnumerator EnterWorkRoutine()
    {
        isTransitioning = true;
        
        PlayerInteractionUI.IsFirstPerson = true;
        
        // 禁用物理和控制
        if (charController != null) charController.enabled = false; 
        if (controller != null) controller.enabled = false;

        player.transform.position = sitPoint.position;
        player.transform.rotation = sitPoint.rotation;
        if (animator != null) animator.SetTrigger("SitDown");

        // 1. 关闭大脑，准备手动运镜
        CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
        if (brain != null) brain.enabled = false;

        Vector3 startPos = mainCam.transform.position;
        Quaternion startRot = mainCam.transform.rotation;
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * transitionSpeed;
            mainCam.transform.position = Vector3.Lerp(startPos, firstPersonCam.transform.position, progress);
            mainCam.transform.rotation = Quaternion.Lerp(startRot, firstPersonCam.transform.rotation, progress);
            yield return null;
        }

        // 🌟🌟 核心修改开始 🌟🌟
        // 删除了 mainCam.enabled = false; 绝对不能关主相机！
        
        firstPersonCam.SetActive(true); // 激活第一人称虚拟相机（它的 Priority 应该是 15）
        
        // 运镜到位后，立刻重新激活大脑！
        // 此时大脑醒来，发现第一人称虚拟相机优先级最高，就会无缝接管
        // 之后点击电脑，Vcam_Monitor 优先级变成 100，大脑就会立刻切过去！
        if (brain != null) brain.enabled = true;
        // 🌟🌟 核心修改结束 🌟🌟

        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;

        isWorking = true;
        isTransitioning = false;
    }

    IEnumerator ExitWorkRoutine()
    {
        isTransitioning = true;
        
        PlayerInteractionUI.IsFirstPerson = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 🌟🌟 核心修改开始 🌟🌟
        // 退出坐下状态时，关闭第一人称虚拟相机
        firstPersonCam.SetActive(false);

        // 关闭大脑，准备手动把相机飞回角色背后
        CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
        if (brain != null) brain.enabled = false;
        // 🌟🌟 核心修改结束 🌟🌟

        // 注意这里：起点改为 mainCam 当前的位置，这样起飞才平滑
        Vector3 fpExitPos = mainCam.transform.position; 
        Quaternion fpExitRot = mainCam.transform.rotation;

        if (animator != null) animator.SetTrigger("StandUp");

        float cameraFlightDuration = standUpTime * 0.8f; 

        Vector3 targetBackPos;
        Quaternion targetBackRot;
        if (standPoint != null)
        {
            targetBackPos = standPoint.position - standPoint.forward * 3f + Vector3.up * 2f;
            targetBackRot = Quaternion.LookRotation(standPoint.position + Vector3.up * 1.5f - targetBackPos);
        }
        else
        {
            targetBackPos = player.transform.position - player.transform.forward * 3f + Vector3.up * 2f;
            targetBackRot = Quaternion.LookRotation(player.transform.position + Vector3.up * 1.5f - targetBackPos);
        }

        float elapsedTime = 0f;
        while (elapsedTime < cameraFlightDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / cameraFlightDuration; 
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            mainCam.transform.position = Vector3.Lerp(fpExitPos, targetBackPos, easedT);
            mainCam.transform.rotation = Quaternion.Lerp(fpExitRot, targetBackRot, easedT);
            yield return null;
        }

        if (standPoint != null)
        {
            player.transform.position = standPoint.position;
            player.transform.rotation = standPoint.rotation;
        }
        
        var tpController = player.GetComponent<StarterAssets.ThirdPersonController>();
        if (tpController != null)
        {
            tpController.ForceCameraRotation(standPoint.rotation.eulerAngles.y, 0f);
        }
        
        SyncStarterAssetsCamera(standPoint.rotation.eulerAngles.y);

        // 🌟🌟 飞行结束，重新把大脑打开，控制权交还给平时跟随角色的 PlayerFollowCamera
        if (brain != null) brain.enabled = true;

        yield return new WaitForSeconds(standUpTime - cameraFlightDuration);

        if (charController != null) charController.enabled = true; 
        if (controller != null) controller.enabled = true;
        
        isWorking = false;
        isTransitioning = false;
        Debug.Log("起立运镜同步完成");
    }
    
    private void SyncStarterAssetsCamera(float newYaw)
    {
        var tpController = player.GetComponent<StarterAssets.ThirdPersonController>();
        if (tpController != null)
        {
            Transform camTarget = player.transform.Find("PlayerCameraRoot");
            if (camTarget != null)
            {
                camTarget.rotation = Quaternion.Euler(0, newYaw, 0);
            }
        }
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) isNear = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) isNear = false; }
}