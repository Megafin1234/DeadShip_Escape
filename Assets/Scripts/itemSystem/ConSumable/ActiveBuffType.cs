/// <summary>
/// 런타임 버프 종류.
/// 
/// UsableEffectType와 이름이 비슷하지만 역할은 다릅니다.
/// 
/// - UsableEffectType:
///   "아이템이 어떤 효과를 갖고 있는가"
/// 
/// - ActiveBuffType:
///   "현재 스쿼드에 어떤 버프가 적용 중인가"
/// 
/// 새 지속형 버프를 만들고 싶다면:
/// 1. 여기 enum 추가
/// 2. ItemUseService.ApplyEffect()에서 AddTimedBuff 호출
/// 3. SquadStatCalculator.ApplyActiveBuffs()에서 실제 수치 반영
/// </summary>
public enum ActiveBuffType
{
    None = 0,
    MoveSpeed = 1,
    Attack = 2
}