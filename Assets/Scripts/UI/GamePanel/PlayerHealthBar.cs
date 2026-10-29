using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public Image healthBarFill;
    public TMP_Text txtHp;
    private CharacterHealth source;

    private void OnEnable() => TryBindPlayer();
    private void Start() => TryBindPlayer();

    private void TryBindPlayer()
    {
        PlayerObject player = PlayerMgr.Instance.player;
        if (player != null && player.Health != null)
            Bind(player.Health);
    }

    public void Bind(CharacterHealth health)
    {
        if (source != null)
            source.HealthChanged -= UpdatePlayerHealthBar;
        source = health;
        if (source == null)
            return;
        source.HealthChanged += UpdatePlayerHealthBar;
        UpdatePlayerHealthBar(source.CurrentHp, source.MaxHp);
    }

    private void OnDisable()
    {
        if (source != null)
            source.HealthChanged -= UpdatePlayerHealthBar;
        source = null;
    }

    public void UpdatePlayerHealthBar(int currentHp, int maxHp)
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = maxHp > 0 ? Mathf.Clamp01((float)currentHp / maxHp) : 0f;
        if (txtHp != null)
            txtHp.text = currentHp + "/" + maxHp;
    }
}
