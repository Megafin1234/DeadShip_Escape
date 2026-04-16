using UnityEngine;
/// <summary>
/// 실제로 "사용할 수 있는 아이템" 정의.
/// 
/// ItemDefinitionBase를 상속하며,
/// IsUsable = true를 반환하기 때문에 컨텍스트 메뉴에서 "사용" 버튼이 뜹니다.
/// 
/// 새 사용 아이템을 만들고 싶다면:
/// 1. Unity에서 Create > Item System > Definition > Consumable
/// 2. itemId / displayName / icon 등 입력
/// 3. effects 배열에 원하는 효과 추가
/// 
/// 예:
/// - 소형 구급킷: Heal 30
/// - 스피드 스팀: MoveSpeedBuff 2 / 10초
/// - 전투 자극제: AttackBuff 10 / 10초
/// </summary>

[CreateAssetMenu(menuName = "Item System/Definitions/Consumable")]
public class ConsumableDefinition : ItemDefinitionBase
{
    [Header("Consumable")]
    [SerializeField] private int maxUseCount = 1;

    [Header("Use Effects")]
    [SerializeField] private UsableEffectData[] effects;

    public int MaxUseCount => maxUseCount;
    public UsableEffectData[] Effects => effects;

    // ItemDefinitionBase 쪽에서 IsUsable 기본값은 false이므로
    // 사용 아이템은 여기서 true로 override합니다.
    public override bool IsUsable => true;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (maxUseCount < 1)
            maxUseCount = 1;
    }
#endif
}