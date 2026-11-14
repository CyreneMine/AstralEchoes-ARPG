using System;
using UnityEngine;

public class Main : MonoBehaviour
{
    private void Start()
    {
        //测试期间直接显示游戏面板后续从 开始面板进入
        UIManager.Instance.ShowPanel<GamePanel>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
