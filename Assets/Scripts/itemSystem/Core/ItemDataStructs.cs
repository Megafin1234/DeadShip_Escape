using System;
using UnityEngine;

[Serializable]
public struct StatModifierData
{
    public StatType statType;
    public float value;
}

[Serializable]
public struct UseEffectData
{
    public StatType effectType;
    public float value;
    public float duration;
    public bool isInstant;
}

[Serializable]
public struct WeaponStatBlock
{
    public int attack;
    public int ammoCount;
    public float attackRange;
    public float fireInterval;
    public float spreadVertical;
    public float spreadHorizontal;
}