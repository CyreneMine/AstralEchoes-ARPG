using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public Image healthBarFill;
    public TMP_Text txtHp;

    public void UpdatePlayerHealthBar(int currentHp, int maxHp)
    {
        healthBarFill.fillAmount = (float)currentHp / (float)maxHp;
        txtHp.text = currentHp + "/" + maxHp;
    }

    public void Start()
    {
        UpdatePlayerHealthBar(PlayerMgr.Instance.player.Health.CurrentHp, PlayerMgr.Instance.player.Health.MaxHp);
        PlayerMgr.Instance.player.Health.OnHealthChange += UpdatePlayerHealthBar;
    }

    private void OnDestroy()
    {
        if (PlayerMgr.Instance.player.Health != null)
        {
            PlayerMgr.Instance.player.Health.OnHealthChange -= UpdatePlayerHealthBar;
        }
    }
}
