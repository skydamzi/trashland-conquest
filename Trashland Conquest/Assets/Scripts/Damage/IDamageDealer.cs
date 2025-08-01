using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageDealer
{
    float GetDamage(); // 이 공격이 주는 데미지 양
    GameObject GetOwner(); // 공격의 주체 (누가 때렸는지)
}
