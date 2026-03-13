using UnityEngine;

public class DeskCameraLook : MonoBehaviour
{
    public float sensitivity = 2f;    
    public float maxYAngle = 50f;     
    public float maxXAngle = 60f;     

    private float rotationX = 0f;
    private float rotationY = 0f;
    
    // 把记录初始角度的变量改成专门存放“默认角度”的变量
    private Quaternion defaultRotation; 

    // 🌟 1. 改动这里：在游戏加载的瞬间，永远锁死这个正前方的角度
    void Awake()
    {
        defaultRotation = transform.localRotation;
    }

    // 🌟 2. 改动这里：每次重新激活相机时，强行复位
    void OnEnable()
    {
        // 强制把头扭回默认的正前方
        transform.localRotation = defaultRotation;
        
        // 清零鼠标累积的偏移量
        rotationX = 0f;
        rotationY = 0f;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;

        rotationX = Mathf.Clamp(rotationX, -maxYAngle, maxYAngle);
        rotationY = Mathf.Clamp(rotationY, -maxXAngle, maxXAngle);

        // 注意这里：用锁死的 defaultRotation 去叠加鼠标偏移
        transform.localRotation = defaultRotation * Quaternion.Euler(rotationX, rotationY, 0f);
    }
}