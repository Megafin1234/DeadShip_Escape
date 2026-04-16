using System;

[Serializable]
public class RaidSquadContext //스쿼드 실행 컨텍스트. squadruntime의 코드 복잡을 막기위해 레이드안에서만 쓰는 래퍼 컨텍스트를 따로 둠.
{
    public SquadRuntime Squad; //스쿼드. 항상 자신

    public UnityEngine.GameObject PlayerBody; //조종하는 캐릭터의 바디 오브젝트
    public UnityEngine.GameObject CompanionBody;  //ai동료 바디

    public RaidSquadContext(SquadRuntime squad) //이후 피격, 동료사격, 추척 등등을 처리?
    {
        Squad = squad;
    }
}