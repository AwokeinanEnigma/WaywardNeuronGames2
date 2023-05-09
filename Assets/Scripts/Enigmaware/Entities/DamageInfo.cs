#region

using Enigmaware.Entities;
using UnityEngine;

#endregion

/// <summary>
///     Contains information about a damage event.
/// </summary>
public class DamageInfo
{
    /// <summary>
    ///     The amount of damage that will be subtracted from the health. Subject to modifiers like armor, weakness,
    ///     resistance, invulnerability, etc.
    /// </summary>
    public float Damage;

    /// <summary>
    ///     The object used to inflict the damage, e.g. the sword.
    /// </summary>
    public GameObject Inflictor;

    /// <summary>
    ///     The object responsible for the damage, e.g. the player holding the sword.
    /// </summary>
    public GameObject Attacker;

    /// <summary>
    ///     The force of this damage, both direction and magnitude. It's up to the object taking the damage to interpret this
    ///     in a meaningful way.
    /// </summary>
    public Vector3 Force;

    /// <summary>
    ///     Set this field to true in the 'OnIncomingDamage' message to prevent the damage from being dealt.
    /// </summary>
    public bool Rejected = false;

    /// <summary>
    ///     Modifies the damage info based on the hitbox type.
    /// </summary>
    /// <param name="hitbox">The hitbox to modify the damage info off of.</param>
    public void ModifyDamageInfo(HitboxHandler hitbox)
    {
        switch (hitbox.type)
        {
            case HitboxHandler.HitboxType.Head:
                Damage *= 2;
                break;
            case HitboxHandler.HitboxType.WeakPoint:
                Damage *= 1.5f;
                break;
            case HitboxHandler.HitboxType.Package:
                Damage *= 2000;
                Debug.Log("Package hit!!");
                break;
        }
    }
}