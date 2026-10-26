using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : BasePanel
{
    public GameObject optionObj;
    public GameObject[] optionButtonObj;
    private int index =0;
    public void ClearOptions()
    {
        optionButtonObj = null;
        UIManager.Instance.HidePanel<OptionsPanel>(false);
    }
    //0为正向 其余是负向
    public void OnOptionSelected(int id)
    {
        ClearOptions();
        DialogueMgr.Instance.SelectOptions(id);
    }
    public override void Init()
    {
        optionButtonObj = new GameObject[DialogueMgr.Instance.dialogueDatas[DialogueMgr.Instance.nowDialogueIndex].options.Length];
        index = 0;
        foreach (DialogueOptionData option in DialogueMgr.Instance.dialogueDatas[DialogueMgr.Instance.nowDialogueIndex].options)
        {
            optionButtonObj[index] = GameObject.Instantiate(optionObj,transform,false);
            index++;
        }
        index = 0;
        if (optionButtonObj ==null)
        {
            ClearOptions();
        }
        foreach (GameObject optionButtonObj in optionButtonObj)
        {
            int currentIndex = index;
            optionButtonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnOptionSelected(currentIndex);
            });
            optionButtonObj.GetComponent<Button>().GetComponentInChildren<TMP_Text>().text = 
                DialogueMgr.Instance.dialogueDatas[DialogueMgr.Instance.nowDialogueIndex].options[index].text;
            index++;
        }
    }

    
}
