using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GamePanel : BasePanel
{
    public TMP_Text lvTxt,txtBuffCount;
    [SerializeField] private PlayerHealthBar playerHealthBar;
    [SerializeField] private InteractPanel interactPanel;
    public Button btnMission,btnBag,btnBuff;
    private int choiceCount;
    public override void Init()
    {
        OnBuffChoiceChange(BuffMgr.Instance.ChoiceCount);
        btnMission.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel<MissionPanel>();
            PlayerMgr.Instance.inputActions.Disable();
        });
        btnBag.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPanel<BagPanel>();
            PlayerMgr.Instance.inputActions.Disable();
        });
        btnBuff.onClick.AddListener(() =>
        {
            if (BuffMgr.Instance.poolCount <=0)
            {
                //TODO 浮动文字 你已经足够强了等
                return;
            }
            UIManager.Instance.ShowPanel<BuffPanel>();
            PlayerMgr.Instance.inputActions.Disable();
        });
        UpdateLvTxt();
    }
    protected override void Update()
    {
        base.Update();
        if (UIManager.Instance.GetPanel<MissionPanel>() != null)
            return;
        if (Keyboard.current.leftAltKey.wasPressedThisFrame)
            PlayerMgr.Instance.SetCursorState(true);
        if (Keyboard.current.leftAltKey.wasReleasedThisFrame)
            PlayerMgr.Instance.SetCursorState(false);
    }

    public void OnBuffChoiceChange(int choice)
    {
        choiceCount = choice;
        if (choice > 0)
        {
            btnBuff.gameObject.SetActive(true);
            txtBuffCount.text = $"剩余强化次数:{choiceCount}";
        }else
            btnBuff.gameObject.SetActive(false);
    }
    public void UpdateHpBar(int currentHp, int maxHp)
    {
        playerHealthBar.UpdatePlayerHealthBar(currentHp, maxHp);
    }
    public void OpenInteractPanel()
    {
        interactPanel = UIManager.Instance.ShowPanel<InteractPanel>();
    }

    public void CloseInteractPanel()
    {
        UIManager.Instance.HidePanel<InteractPanel>(false);
    }
    public override void ShowMe()
    {
        base.ShowMe();
        PlayerMgr.Instance.inputActions.Enable();
    }

    private void OnEnable()
    {
        PlayerMgr.Instance.OnPlayerLevelChange += UpdateLvTxt;
        BuffMgr.Instance.onChoiceCountChange += OnBuffChoiceChange;
    }

    private void OnDisable()
    {
        PlayerMgr.Instance.OnPlayerLevelChange -= UpdateLvTxt;
        BuffMgr.Instance.onChoiceCountChange -= OnBuffChoiceChange;
    }

    public void UpdateLvTxt()
    {
        lvTxt.text = PlayerMgr.Instance.saveData.level + "";
    }
}
