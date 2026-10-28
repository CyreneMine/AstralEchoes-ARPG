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
        public int id, atk, def, maxHp;
        public CharacterType type;
    }
}
