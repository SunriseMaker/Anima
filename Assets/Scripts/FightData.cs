// Initializes in GameData

public static class FightData
{
    public static Fighter fighter1;

    public static Fighter fighter2;

    public static int win_count;

    public static int fighter1_win_count;

    public static int fighter2_win_count;

    public static float fighter_hp;

    public static float round_time;

    public static float round_duration;

    public static int current_round;

    public static float delay_before_first_round;

    public static float delay_after_last_round;

    public static float delay_before_round_start;

    public static float delay_after_round_end;

    public static float delay_between_rounds;

    public static bool action;

    public static void Nullify()
    {
        fighter1 = null;
        fighter2 = null;
        win_count = 0;
        fighter1_win_count = 0;
        fighter2_win_count = 0;
        fighter_hp = 0.0f;
        round_time = 0.0f;
        round_duration = 0.0f;
        current_round = 0;
        delay_before_first_round = 0.0f;
        delay_after_last_round = 0.0f;
        delay_before_round_start = 0.0f;
        delay_after_round_end = 0.0f;
        delay_between_rounds = 0.0f;
        action = false;
    }

    public static Fighter RoundWinner()
    {
        bool f1_dead = fighter1.health.IsDead();
        bool f2_dead = fighter2.health.IsDead();

        if (f1_dead && f2_dead)
        {
            return null;
        }

        if (f1_dead)
        {
            return fighter2;
        }

        if (f2_dead)
        {
            return fighter1;
        }

        float f1_hp = fighter1.health.CurrentHealth();
        float f2_hp = fighter2.health.CurrentHealth();

        if (f1_hp == f2_hp)
        {
            return null;
        }

        return f1_hp > f2_hp ? fighter1 : fighter2;
    }

    public static bool OneOfFightersIsDead()
    {
        // 0 fighters
        if (fighter1 == null)
        {
            return false;
        }

        bool fighter1_dead = fighter1.health.IsDead();

        // 1 fighter
        if (fighter2 == null)
        {
            return fighter1_dead;
        }

        // 2 fighters
        return fighter1_dead || fighter2.health.IsDead();
    }

    public static Fighter FightWinner()
    {
        if(fighter1_win_count == win_count)
        {
            return fighter1;
        }

        if (fighter2_win_count == win_count)
        {
            return fighter2;
        }

        return null;
    }
}