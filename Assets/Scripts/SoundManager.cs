using UnityEngine;

public static class SoundManager //게임 내 sfx가 아니라 슈팅게임에서의 소음으로 인한 효과를 관리
{
    public static void EmitSound(Vector3 position, float radius)  //playershooter에서 소리 퍼지는 범위를 지정
    {
        Collider[] hits = Physics.OverlapSphere(position, radius);

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.OnHearSound(position, radius);
            }
        }
    }
}
