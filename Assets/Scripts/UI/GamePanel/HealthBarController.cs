using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    public Image imgFill;

    public void updateHealthBar(float hp,float maxHp)
    {
        float currentHp = (hp / maxHp);
        imgFill.fillAmount = currentHp;
    }
}
