using System;

public enum ItemCategory
{
    None = 0,

    Valuable = 1,      // 유실물 / 판매 중심
    Material = 2,      // 제작 / 성장 재료
    Consumable = 3,    // 사용형 아이템
    Equipment = 4      // 장비 계열 공통 상위 타입
}

public enum ItemRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
}

public enum EquipmentType
{
    None = 0,

    Weapon = 1,
    Helmet = 2,
    Armor = 3,
    Gloves = 4,
    Boots = 5,
    Bag = 6,
    Special = 7
}

public enum EquipSlotType
{
    None = 0,

    Weapon = 1,
    Helmet = 2,
    Armor = 3,
    Gloves = 4,
    Boots = 5,
    Bag = 6,
    Special = 7
}

public enum PositionIndex
{
    None = 0,

    Position1 = 1, // 메인 조작 캐릭터
    Position2 = 2, // AI 동료
    Position3 = 3  // 비전투 지원
}

public enum SpecialEquipmentType
{
    None = 0,

    Utility = 1,
    Scanner = 2,
    Support = 3,
    Insurance = 4
}

public enum StatType
{
    None = 0,

    Attack = 1,
    Health = 2,
    MoveSpeed = 3,
    CarryWeight = 4,

    HungerRecovery = 5,
    OxygenEfficiency = 6,
    ContaminationRecovery = 7,

    BulletSpreadVertical = 8,
    BulletSpreadHorizontal = 9,
    FireInterval = 10,
    AttackRange = 11
}