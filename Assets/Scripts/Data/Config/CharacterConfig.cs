using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterConfig", menuName = "SO/CharacterConfig")]
public class CharacterConfig : ScriptableObject
{
    public enum CharacterType
    {
        Player=0,
        Enemy=1
    }
    public List<CharacterData> characterDatas = new List<CharacterData>();
    [System.Serializable]
    public class CharacterData
    {
        public int id;
        public string name;
        public int initLv,maxLv,initExpPerLv,atkPerLv,defPerLv,maxHpPerLv,expPerLv;
        public float atk, def, maxHp,atkCooldown,atkRange;
        public CharacterType type;
    }
}
