public class CharacterStats
{
    private CharacterConfig.CharacterData config;
    private float atk, def, maxHp;
    public float Atk => atk;
    public float Def => def;
    public float MaxHp => maxHp;

    public CharacterStats(CharacterConfig.CharacterData config)
    {
        this.config = config;
    }

    public void Recalculate(int level)
    {
        int realLv = level - config.initLv;
        atk = config.atk + config.atkPerLv*realLv;
        def = config.def + config.defPerLv*realLv;
        maxHp = config.maxHp + config.maxHpPerLv*realLv;
    }
    
}
