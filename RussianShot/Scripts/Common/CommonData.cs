using System;
using System.ComponentModel;

public struct CharacterChoice
{
    public ActionType actionType;
    public int targetId;
    //実弾転移用
    public int? protectedId;
    public int? attackedId;
}
public struct TurnInfo
{
    public int round;
    public int currentActionCharacterId;
    //イコール演算子の定義
    public static bool operator ==(TurnInfo a, TurnInfo b)
    {
        return a.round == b.round && a.currentActionCharacterId == b.currentActionCharacterId;
    }
    public static bool operator !=(TurnInfo a, TurnInfo b)
    {
        return !(a == b);
    }
    public override bool Equals(object obj)
    {
        if (obj is TurnInfo other)
        {
            return this == other;
        }
        return false;
    }
    public override int GetHashCode()
    {
        return round.GetHashCode() ^ currentActionCharacterId.GetHashCode();
    }
};
public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
    }
}
public enum BulletType 
{
    Live,
    Empty,
    None 
}
public enum ActionType
{
    //撃つ
    Shot,
    //アイテム
    Spray,
    Muzzle,
    Glass,
    //スキル
    FakeShot,
    BulletAnalysis,
    SkillSensing,
    Shield,
    Counter,
    LiveBulletTransfer,
    Checkmate,
    //キャンセル
    Cancel
}
public enum ShotAnimationType
{
    NotDeath,
    Death,
    Shield,
    Counter,
    LiveBulletTransfer,
    Checkmate
}
public enum SkillType
{
    [Description("SkillIcons/FakeShot")]
    FakeShot,
    [Description("SkillIcons/BulletAnalysis")]
    BulletAnalysis,
    [Description("SkillIcons/SkillSensing")]
    SkillSensing,
    [Description("SkillIcons/Shield")]
    Shield,
    [Description("SkillIcons/Counter")]
    Counter,
    [Description("SkillIcons/LiveBulletTransfer")]
    LiveBulletTransfer,
    [Description("SkillIcons/Checkmate")]
    Checkmate
}
public enum SkillState
{
    NotUsed,
    Used,
    InUse
}
public enum Item
{
    Spray,
    Muzzle,
    Glass,
    None
}
public enum Capacity
{
    Hp,
    Fortune,
    Stealth
}