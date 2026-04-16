using System;

/// <summary>
/// 현재 스쿼드에 적용 중인 버프 1개를 표현하는 런타임 데이터.
/// 
/// 이 데이터는 저장용이 아니라 "지금 플레이 중인 상태"를 담는 용도입니다.
/// duration이 지나면 제거됩니다.
/// </summary>
[Serializable]
public class ActiveBuffRuntime
{
    public ActiveBuffType BuffType;
    public float Value;
    public float RemainingTime;

    public ActiveBuffRuntime(ActiveBuffType buffType, float value, float duration)
    {
        BuffType = buffType;
        Value = value;
        RemainingTime = duration;
    }
}