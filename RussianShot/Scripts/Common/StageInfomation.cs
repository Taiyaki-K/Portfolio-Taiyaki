using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/StageInfo")]
public class StageInfomation : ScriptableObject
{
    public int StageNum;
    public CharacterCapacity[] NPCCapacities;
    public int earnedExp;
}
