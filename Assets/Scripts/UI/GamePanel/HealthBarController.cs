using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    public Image imgFill;

    public void updateHealthBar(int hp,int maxHp)
    {
        float currentHp = ((float)hp / (float)maxHp);
        imgFill.fillAmount = currentHp;
    }
}
