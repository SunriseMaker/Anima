public interface iHealth
{
    float CurrentHealthNormalized();

    float CurrentHealth();

    float MaxHealth();

    bool IsImmune();

    void SetImmunity(bool v_immune);

    bool IsImmortal();

    void SetImmortality(bool v_immortal);

    bool IsDead();

    bool Kill();

    void Resurrect(float hp);

    float Heal(float hp);

    float Injure(float hp);
}
