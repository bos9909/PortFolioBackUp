using UnityEngine;

public interface IDamageable
{
    //이걸 상속한 스크립트에서 구현해야함
    void TakeDamage(float damageAmount, Vector3 hitPoint, Vector3 hitNormal);
}
