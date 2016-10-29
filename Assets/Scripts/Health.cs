using System;

public sealed class Health : iHealth
{
    #region Constructors
    public Health(float v_max_hp, float v_current_hp, bool v_immortal=false, bool v_immune=false)
    {
        UnityEngine.Debug.Assert(v_max_hp > 0 && v_current_hp > 0, "ASSERTION FAILED: Initial health is zero.");
        UnityEngine.Debug.Assert(v_max_hp>= v_current_hp, "ASSERTION FAILED: Maximum health is less than initial health.");

        max_hp = v_max_hp;
        current_hp = v_current_hp;
        immortal = v_immortal;
        immune = v_immune;
    }
    #endregion Constructors

    #region Variables
    private float max_hp;
    private float current_hp;
    private bool immortal;
    private bool immune;
    #endregion Variables

    #region iHealth
    public float CurrentHealthNormalized()
    {
        return current_hp / max_hp;
    }

    public float CurrentHealth()
    {
        return current_hp;
    }

    public float MaxHealth()
    {
        return max_hp;
    }

    public bool IsImmune()
    {
        return immune;
    }

    public void SetImmunity(bool v_immune)
    {
        immune = v_immune;
    }

    public bool IsImmortal()
    {
        return immortal;
    }

    public void SetImmortality(bool v_immortal)
    {
        immortal = v_immortal;
    }

    public bool IsDead()
    {
        return IsImmortal() ? false : current_hp <= 0;
    }

    public bool Kill()
    {
        bool killed = false;

        if(!IsImmortal())
        {
            current_hp = 0.0f;
            killed = true;
        }

        return killed;
    }

    public void Resurrect(float hp)
    {
        if (IsDead())
        {
            AffectHealth(hp, true);
        }
    }

    public float Heal(float hp)
    {
        return AffectHealth(Math.Abs(hp), false);
    }

    public float Injure(float hp)
    {
        if(IsImmune())
        {
            return 0.0f;
        }
        else
        {
            return AffectHealth(-Math.Abs(hp), false);
        }
    }
    #endregion iHealth

    #region Red
    private float AffectHealth(float hp, bool resurrect)
    {
        if (IsDead() && (hp <= 0 || !resurrect))
        {
            return 0.0f;
        }

        float old_health = current_hp;

        float calculated_effect = hp;

        current_hp += calculated_effect;

        current_hp = Math.Max(current_hp, 0.0f);

        current_hp = Math.Min(current_hp, max_hp);

        float delta_health = current_hp - old_health;

        return delta_health;
    }
    #endregion Red
}
