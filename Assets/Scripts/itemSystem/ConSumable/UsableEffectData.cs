using System;
using UnityEngine;

/// <summary>
/// 소비 아이템 1개가 가지는 "효과 한 줄" 데이터.
/// 
/// 예를 들어:
/// - 구급킷: Heal 30
/// - 자극제A: MoveSpeedBuff +2 / 10초
/// - 자극제B: AttackBuff +10 / 10초
/// - 복합 스팀: Heal 10 + MoveSpeedBuff +1 / 5초
/// 
/// 즉, ConsumableDefinition 하나 안에 이 데이터가 여러 개 들어갈 수 있습니다.
/// </summary>
[Serializable]
public class UsableEffectData
{
    [SerializeField] private UsableEffectType effectType;

    // 효과 수치
    // Heal이면 회복량
    // MoveSpeedBuff면 증가량
    // AttackBuff면 증가량
    [SerializeField] private float value;

    // 지속 시간
    // 즉시 효과(Heal)는 0으로 두어도 됨
    // 버프류는 duration을 사용
    [SerializeField] private float duration;

    public UsableEffectType EffectType => effectType;
    public float Value => value;
    public float Duration => duration;
}