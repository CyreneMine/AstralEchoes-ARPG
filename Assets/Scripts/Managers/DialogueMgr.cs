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
    public void StartDialogue(string dialogue,int missionId,int npcId)
    {
        if (MissionMgr.Instance.GetMissionSaveData(missionId) == null)
        {
            dialogueDatas = JsonMgr.Instance.LoadData<List<DialogueData>>("Dialogue/" + dialogue);
            dialoguePanel.InitData(dialogueDatas,missionId);
        }
        else
        {
            //TODO 暂时写死后续更改
            
            if (!MissionMgr.Instance.GetMissionSaveData(missionId).status
                &&!MissionMgr.Instance.GetMissionSaveData(missionId).isFinished
                &&ConfigMgr.Instance.GetMissionData(missionId)
                    .stages[MissionMgr.Instance.GetMissionSaveData(missionId).currentStageIndex]
                    .missionType == MissionType.Battle)
            {
                dialogueDatas = JsonMgr.Instance.LoadData<List<DialogueData>>("Dialogue/Npc/0/Mission_0_InProgress");
                dialoguePanel.InitData(dialogueDatas,missionId);
            }
            else if (ConfigMgr.Instance.GetMissionData(missionId)
                         .stages[MissionMgr.Instance.GetMissionSaveData(missionId).currentStageIndex]
                         .missionType == MissionType.Dialogue 
                     &&
                     !MissionMgr.Instance.GetMissionSaveData(missionId).status)
            {
                dialogueDatas = JsonMgr.Instance.LoadData<List<DialogueData>>("Dialogue/Npc/0/Mission_0_Stage_1");
                dialoguePanel.InitData(dialogueDatas,missionId);
                MissionMgr.Instance.CheckFinishMission(MissionType.Dialogue,npcId);
            }
        }
        
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
