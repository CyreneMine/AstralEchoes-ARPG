using System;

public class CharacterStats
{
    private CharacterConfig.CharacterData config;
    private float atk, def, maxHp;
    public float Atk => atk;
    public float Def => def;
    public float MaxHp => maxHp;
    private float atkBuffPercent,defBuffPercent,maxHpBuffPercent;
    public CharacterStats(CharacterConfig.CharacterData config)
    {
        this.config = config;
    }
    public void ClearPlayerBuff()
    {
        atkBuffPercent = 0;
        defBuffPercent = 0;
        maxHpBuffPercent = 0;
    }

    public void AddBuff(BuffType type, float value)
    {
        switch (type)
        {
            case BuffType.atk:
                atkBuffPercent += value;
                break;
            case BuffType.def:
                defBuffPercent += value;
                break;
            case BuffType.maxHp:
                maxHpBuffPercent += value;
                break;
        }
        
    }
    public void Recalculate(int level)
    {
        int realLv = level - config.initLv;
        atk = (config.atk + config.atkPerLv * realLv)*(1+atkBuffPercent);
        def = (config.def + config.defPerLv * realLv)*(1+defBuffPercent);
        maxHp = (config.maxHp + config.maxHpPerLv * realLv)*(1+maxHpBuffPercent);
    }
    
}
