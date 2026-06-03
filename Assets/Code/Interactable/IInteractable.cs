using UnityEngine;

// 定义一个接口：任何实现了这个接口的物体，都可以被交互
public interface IInteractable
{
    void Interact(); // 交互时发生什么
    string GetPromptText(); // 屏幕上显示什么提示文字
}
