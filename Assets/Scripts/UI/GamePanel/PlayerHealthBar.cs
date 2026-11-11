using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public Image healthBarFill;
    public TMP_Text txtHp;

    public void UpdatePlayerHealthBar(int currentHp, int maxHp)
    {
        healthBarFill.fillAmount = currentHp / maxHp;
        txtHp.text = currentHp + "/" + maxHp;
    }
}
