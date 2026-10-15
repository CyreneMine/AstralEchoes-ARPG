using UnityEngine;
using UnityEngine.InputSystem;

public class TestDialogue : MonoBehaviour
{
    void Start()
    {
        
    }
    
    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            UIManager.instance.ShowPanel<DialoguePanel>();
            DialogueMgr.instance.StartDialogue("Test/Test1_1");
        }
    }
}
