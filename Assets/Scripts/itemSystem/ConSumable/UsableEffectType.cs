/// <summary>
/// 사용 아이템이 가질 수 있는 효과 종류.
/// 
/// 새 효과를 추가하고 싶다면:
/// 1. 이 enum에 새 타입 추가
/// 2. ItemUseService.ApplyEffect() switch문에 처리 로직 추가
/// 3. 필요하면 SquadStatCalculator / PlayerSquadBridge / 버프 UI 쪽도 확장
/// 
/// 예:
/// DefenseBuff,
/// ReloadSpeedBuff,
/// HealOverTime,
/// ShieldGain
/// </summary>
public enum UsableEffectType
{
    None = 0,

    // 즉시 체력 회복
    Heal = 1,

    // 일정 시간 이동속도 증가
    MoveSpeedBuff = 2,

    // 일정 시간 공격력 증가
    AttackBuff = 3
}