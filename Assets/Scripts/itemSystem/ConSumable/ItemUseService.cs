using UnityEngine;

/// <summary>
/// "사용 아이템" 전용 처리 서비스.
/// 
/// 역할:
/// - 아이템 정의(ConsumableDefinition)에 적힌 효과들을 읽는다
/// - 각 효과를 실제 스쿼드/플레이어 상태에 반영한다
/// - 사용 후 스택을 1 줄인다
/// 
/// 확장 가이드:
/// 
/// [새 효과를 추가하고 싶을 때]
/// 1. UsableEffectType에 enum 추가
/// 2. ApplyEffect() switch문에 분기 추가
/// 3. 즉시 효과면 bridge에 바로 반영
/// 4. 지속 효과면 bridge.AddTimedBuff() 또는 별도 시스템으로 연결
/// 
/// 예:
/// - 방어력 증가 버프
/// - 장전속도 증가 버프
/// - 지속 회복(HealOverTime)
/// - 실드 획득
/// </summary>
public class ItemUseService
{
    /// <summary>
    /// 사용 아이템 1개를 실제로 사용합니다.
    /// 
    /// 처리 순서:
    /// 1. 유효성 검사
    /// 2. 정의에 적힌 모든 효과 적용
    /// 3. 스택 1개 소비
    /// 4. 브리지 재계산 및 반영
    /// </summary>
    public ItemMoveResult UseItem(
        ItemInstance itemInstance,
        ConsumableDefinition definition,
        PlayerSquadBridge bridge)
    {
        if (itemInstance == null)
            return ItemMoveResult.Fail("아이템 인스턴스가 없습니다.");

        if (definition == null)
            return ItemMoveResult.Fail("사용 아이템 정의가 없습니다.");

        if (bridge == null)
            return ItemMoveResult.Fail("브리지가 없습니다.");

        UsableEffectData[] effects = definition.Effects;
        if (effects == null || effects.Length == 0)
            return ItemMoveResult.Fail("적용할 효과가 없습니다.");

        // 정의에 적힌 효과를 순서대로 전부 적용
        for (int i = 0; i < effects.Length; i++)
        {
            ApplyEffect(effects[i], bridge);
        }

        // 실제 사용 후 1개 소비
        ItemMoveResult consumeResult = bridge.ConsumeOneItem(itemInstance);
        if (!consumeResult.Success)
            return consumeResult;

        // 사용 후 최종 재계산
        // 예: 공격력 버프, 이속 버프, 체력 UI 갱신 등
        bridge.RecalculateAndApply();

        return ItemMoveResult.Ok($"{definition.DisplayName} 사용");
    }

    /// <summary>
    /// 효과 1줄을 실제 게임 상태에 반영합니다.
    /// 
    /// 새 효과 추가 방법:
    /// - UsableEffectType에 enum 추가
    /// - 여기 switch에 새 case 추가
    /// - bridge에 필요한 헬퍼 함수가 없으면 먼저 만들기
    /// </summary>
    private void ApplyEffect(UsableEffectData effect, PlayerSquadBridge bridge)
    {
        switch (effect.EffectType)
        {
            case UsableEffectType.Heal:
                // 즉시 회복 효과
                // value를 정수 회복량으로 사용
                bridge.HealIncoming(Mathf.RoundToInt(effect.Value));
                break;

            case UsableEffectType.MoveSpeedBuff:
                // 일정 시간 이동속도 증가
                // value = 증가량
                // duration = 지속시간
                bridge.AddTimedBuff(ActiveBuffType.MoveSpeed, effect.Value, effect.Duration);
                break;

            case UsableEffectType.AttackBuff:
                // 일정 시간 공격력 증가
                bridge.AddTimedBuff(ActiveBuffType.Attack, effect.Value, effect.Duration);
                break;
        }
    }
}