using System;

[Serializable]
public class SkillLevelStat
{
    public float damage = 10f;
    public float cooldown = 1f;
    public int count = 1;          // 투사체/칼날 등 개수
    public float area = 1f;        // 범위/반경
    public float speed = 10f;
    public int pierceCount = 0;
    public float duration = 1f;    // 지속시간
    public float tickInterval = 0.5f; // 장판 등 틱 간격
}
