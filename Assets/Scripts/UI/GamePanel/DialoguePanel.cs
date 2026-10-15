using TMPro;
using UnityEngine;

public class DialoguePanel : BasePanel
{
    public GameObject OptionsPanelObj;
    public TMP_Text txtSpeakName, txtSpeakContext;
    public override void Init()
    {
        OptionsPanelObj.SetActive(false);
        
    }
    
}
