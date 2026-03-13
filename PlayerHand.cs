using UnityEngine;
using System.Collections;
using UnityEngine.Animations.Rigging;

public class PlayerHand : MonoBehaviour
{
    public static PlayerHand Instance;

    [Header("Rigging (IK 设置)")]
    public Rig leftHandRig; 
    public float rigSmoothSpeed = 10f;
    private float targetWeight = 0f;

    [Header("交互与物理设置")]
    public Transform handSocket; 
    public float throwForce = 5f;
    public Collider playerCollider; 

    [HideInInspector]
    public HoldableItem currentItem; 
    public bool HasItem => currentItem != null;

    private Animator playerAnim; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerAnim = GetComponent<Animator>(); 
    }

    void Update()
    {
        // 1. 平滑处理 IK 权重的过渡
        leftHandRig.weight = Mathf.Lerp(leftHandRig.weight, targetWeight, Time.deltaTime * rigSmoothSpeed);
    
        // 2. 持有物品时的快捷键操作
        if (HasItem && !PlayerInteractionUI.IsGlobalLocked)
        {
            // 按 F 使用物品 (喝咖啡)
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (currentItem.UseItem())
                {
                    if (playerAnim != null) playerAnim.SetTrigger("DoDrink");
                    StartCoroutine(TemporarilyDisableRig(3.5f)); // 保持你的 3.5s 设定
                }
                else 
                {
                    Debug.Log("物品已耗尽或无法在此刻使用");
                }
            }

            // 🌟 核心改动：按 X 键扔出物品（不再占用左键交互）
            if (Input.GetKeyDown(KeyCode.X))
            {
                DropItem(true);
            }
        }
    }

    public void PickUp(HoldableItem item)
    {
        currentItem = item;
        
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
        
        Collider col = item.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        item.transform.SetParent(handSocket);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        targetWeight = 1f; 
    }

    public void DropItem(bool throwForward)
    {
        if (!HasItem) return;

        HoldableItem itemToDrop = currentItem;
        currentItem = null; 

        targetWeight = 0f; 

        if (throwForward)
        {
            if (playerAnim != null) playerAnim.SetTrigger("Throw"); 
            StartCoroutine(DelayedThrowRoutine(itemToDrop, 0.82f)); // 保持你的 0.82s 设定
        }
        else
        {
            ExecutePhysicalDrop(itemToDrop, false);
        }
    }

    IEnumerator TemporarilyDisableRig(float duration)
    {
        targetWeight = 0f; 
        yield return new WaitForSeconds(duration); 
        if (HasItem) targetWeight = 1f;
    }

    IEnumerator DelayedThrowRoutine(HoldableItem itemToDrop, float delay)
    {
        PlayerInteractionUI.IsGlobalLocked = true; 
        yield return new WaitForSeconds(delay); 
        PlayerInteractionUI.IsGlobalLocked = false; 

        if (itemToDrop != null)
        {
            ExecutePhysicalDrop(itemToDrop, true);
        }
    }

    void ExecutePhysicalDrop(HoldableItem itemToDrop, bool throwForward)
    {
        itemToDrop.transform.SetParent(null); 

        Collider col = itemToDrop.GetComponent<Collider>();
        if (col != null) 
        {
            col.enabled = true;
            if (playerCollider != null)
                StartCoroutine(IgnoreCollisionTemporarily(col, playerCollider, 0.5f));
        }

        Rigidbody rb = itemToDrop.GetComponent<Rigidbody>();
        if (rb == null) rb = itemToDrop.gameObject.AddComponent<Rigidbody>();
        
        rb.isKinematic = false; 
        rb.useGravity = true; 
        
        if (throwForward)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 throwDirection = Camera.main.transform.forward;
            throwDirection.y += 0.35f; 
            throwDirection.Normalize(); 

            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

            Vector3 randomTorque = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            rb.AddTorque(randomTorque, ForceMode.Impulse);
        }
    }

    IEnumerator IgnoreCollisionTemporarily(Collider itemCol, Collider playerCol, float delay)
    {
        Physics.IgnoreCollision(itemCol, playerCol, true);
        yield return new WaitForSeconds(delay);
        if (itemCol != null && playerCol != null)
            Physics.IgnoreCollision(itemCol, playerCol, false);
    }
}