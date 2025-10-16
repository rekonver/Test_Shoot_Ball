using UnityEngine;

public interface IHitble
{
    /// <summary>
    /// Викликається, коли об'єкт був вражений снарядом.
    /// </summary>
    /// <param name="proj">Снаряд, який вразив.</param>
    /// <param name="hitCollider">Коллайдер, який отримав контакт (може бути той самий об'єкт).</param>
    void HitBy(Projectile proj, Collider hitCollider);
}