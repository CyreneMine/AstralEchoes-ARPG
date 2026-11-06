using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DialoguePanel : BasePanel
{
    public GameObject OptionsPanelObj;
    public TMP_Text txtSpeakName, txtSpeakContext;
    private List<DialogueData> dialogueData;
    private int currentIndex = 0;
    private OptionsPanel optionsPanel;
    private bool waitChoiceDialogue = false;
    private bool accept = false;
    public bool canOnClick = true;
    private int lastChoiceFrame = -1;
    private int currentMissionId;
    public override void UpdateDoSomething()
    {
        base.UpdateDoSomething();
        if (Time.frameCount == lastChoiceFrame)
            return;
        //TODO 后续可以考虑补全文字流式出现效果
        if (!waitChoiceDialogue&&canOnClick)
        {
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                CheckNext();
            }
        }
    }

    private void CheckNext()
    {
        if (dialogueData == null)
        {
            EndDialogue();
            return;
        }
            
        if (currentIndex > dialogueData.Count-1)
        { 
            EndDialogue();
        }
        else
        { 
            NextDialogue();
        }
    }
    public override void ShowMe()
    {
        base.ShowMe();
        DialogueMgr.Instance.dialoguePanel = this;
        Cursor.lockState = CursorLockMode.None;
    }

    public override void HideMe(UnityAction callback = null)
    {
        base.HideMe(callback);
        PlayerMgr.Instance.OpenInput();
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void Init()
    {
        
        
    }
    //依照规则命名的对话 "对话类型(Main、NPC、Test)/JSON文件名"
    public void InitData(List<DialogueData> data,int missionId)
    {
        this.currentMissionId = missionId;
        if (data == null)
        {
            EndDialogue();
            return;
        }
        PlayerMgr.Instance.CloseInput();
        dialogueData = data;
        currentIndex = 0;
        if (data.Count == 0)
        {
            EndDialogue();
            data = null;
            return;
        }
        NextDialogue();
    }
    public void NextDialogue()
    {
        if (dialogueData == null || currentIndex >= dialogueData.Count)
        {
            EndDialogue();
            return;
        }
        txtSpeakName.text = dialogueData[currentIndex].speakerName;
        txtSpeakContext.text = dialogueData[currentIndex].context;
        if (dialogueData[currentIndex].options!=null && dialogueData[currentIndex].options.Length > 0 )
        {
            optionsPanel = UIManager.Instance.ShowPanel<OptionsPanel>();
            // optionsPanel.ShowOptions();
            waitChoiceDialogue = true;
            canOnClick = false;
        }
        int tempIndex = currentIndex;
        currentIndex++;
        DialogueMgr.Instance.nowDialogueIndex = tempIndex;
    }

    public void AcceptMission()
    {
        accept = true;
    }
    public void EndDialogue()
    {
        if (accept)
        {
            //TODO 处理接受任务逻辑 后续任务管理器完善后填充
            MissionMgr.Instance.AcceptMission(currentMissionId);
        }
        UIManager.Instance.HidePanel<DialoguePanel>();
        UIManager.Instance.HidePanel<OptionsPanel>();
    }

    public void MakeChoice()
    {
        lastChoiceFrame = Time.frameCount;
        waitChoiceDialogue = false;
        canOnClick = true;
    }
    
}
