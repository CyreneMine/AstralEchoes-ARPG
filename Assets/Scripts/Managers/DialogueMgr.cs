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
        MissionSaveData currentMissionData = MissionMgr.Instance.GetMissionSaveData(missionId);
        if (currentMissionData == null)
        {
            Debug.Log("玩家未接取任务");
            dialogueDatas = JsonMgr.Instance.LoadData<List<DialogueData>>("Dialogue/" + dialogue);
            dialoguePanel.InitData(dialogueDatas,missionId);
        }
        else
        {
            if (!currentMissionData.status
                &&!currentMissionData.isFinished
                &&ConfigMgr.Instance.GetMissionData(missionId)
                    .stages[currentMissionData.currentStageIndex]
                    .missionType == MissionType.Battle)
            {
                dialogueDatas = JsonMgr.Instance.LoadData<List<DialogueData>>($"Dialogue/Npc/{npcId}/Mission_{missionId}_InProgress");
                dialoguePanel.InitData(dialogueDatas,missionId);
            }
            else if (ConfigMgr.Instance.GetMissionData(missionId)
                         .stages[currentMissionData.currentStageIndex]
                         .missionType == MissionType.Dialogue 
                     &&
                     !currentMissionData.status)
            {
                dialogueDatas = JsonMgr.Instance.LoadData<List<DialogueData>>($"Dialogue/Npc/{npcId}/Mission_{missionId}_Stage_{currentMissionData.currentStageIndex}");
                dialoguePanel.InitData(dialogueDatas,missionId);
                MissionMgr.Instance.CheckFinishMission(MissionType.Dialogue,npcId);
            }
            else if ((currentMissionData.currentStageIndex == ConfigMgr.Instance.GetMissionData(missionId).stages.Count - 1)
                     &&(currentMissionData.status))
            {
                dialogueDatas = JsonMgr.Instance.LoadData<List<DialogueData>>($"Dialogue/Npc/{npcId}/Mission_{missionId}_Finished");
                dialoguePanel.InitData(dialogueDatas,missionId);
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
