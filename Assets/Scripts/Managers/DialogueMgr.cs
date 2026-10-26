using System.Collections.Generic;
using UnityEngine;

public class DialogueMgr :BaseManager<DialogueMgr>
{
    public DialoguePanel dialoguePanel;
    public List<DialogueData> dialogueDatas;
    public int nowDialogueIndex = 0;
    private DialogueMgr()
    {
    }
    private string currentDialogue;
    public void StartDialogue(string dialogue)
    {
        dialogueDatas = JsonMgr.Instance.LoadData<List<DialogueData>>("Dialogue/" + dialogue);
        dialoguePanel.InitData(dialogueDatas);
    }
    public void SelectOptions(int dialogue = 1)
    {
        DialoguePanel dialoguePanel = UIManager.Instance.GetPanel<DialoguePanel>();
        if (dialogue == 0)
        {
            dialoguePanel.AcceptMission();
        }
        dialoguePanel.MakeChoice();
        dialoguePanel.NextDialogue();
    }
}
