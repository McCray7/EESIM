public interface IInteractable
{
    string InteractionPrompt { get; } // 动态提示文字（如：剩余咖啡 2/3）
    bool CanInteract { get; }         // 当前是否允许交互
    void Interact();                  // 核心交互逻辑
    void OnHover();                   // 准星指向时的视觉反馈
    void OnExit();                    // 准星移开时的视觉反馈
}