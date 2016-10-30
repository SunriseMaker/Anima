using UnityEngine;
using System.Collections.Generic;

public class FighterAI : MonoBehaviour
{
    const float ENEMY_DISTANCE = 1.5f;
    const float DISTANCE_ACCURACY = 0.1f;
    const float COMBO_DELAY = 0.5f;

    private delegate void AttackDelegate();

    #region Variables

    private static double combo_chance = 1.0d;

    private Fighter _fighter;

    private bool busy;

    private List<Combo> combos;
    #endregion Variables

    #region MonoBehaviour
    private void Awake()
    {
        busy = false;

        _fighter = GetComponent<Fighter>();

        // TODO: combo constructor
        // different styles, animators and combo timings

        AttackInfo attack_info_punch_0_4 = new AttackInfo(_fighter.Punch, 0.4f);
        AttackInfo attack_info_kick_1_0 = new AttackInfo(_fighter.Kick, 1.0f);

        combos = new List<Combo>()
        {
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    new AttackInfo(RandomEvade, 1.0f)
                }
            )
            ,
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    attack_info_punch_0_4,
                    attack_info_punch_0_4,
                    attack_info_kick_1_0
                }
            )
            ,
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    attack_info_punch_0_4,
                    attack_info_punch_0_4,
                    new AttackInfo(JumpKick, 1.0f)
                }
            )
            ,
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    attack_info_punch_0_4,
                    attack_info_kick_1_0,
                    new AttackInfo(Slide, 1.0f)
                }
            )
            ,
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    new AttackInfo(_fighter.Kick, 0.2f),
                    attack_info_kick_1_0,
                    new AttackInfo(_fighter.Jump, 1.5f)
                }
            )
        };
    }

    private void Update()
    {
        if (busy || !_fighter.controllable || _fighter.health.IsDead())
        {
            return;
        }

        if (_fighter.EnemyDistance() - ENEMY_DISTANCE > DISTANCE_ACCURACY)
        {
            _fighter.LookAtEnemy();
            _fighter.Move(1.0f, 0.0f, false);
        }
        else
        {
            RandomCombo();
        }
    }
    #endregion MonoBehaviour

    #region Red
    private void RandomCombo()
    {
        int random_combo = Mathematics.random.Next(combos.Count);

        StartCoroutine(StartCombo(combos[random_combo], COMBO_DELAY));
    }

    private System.Collections.IEnumerator StartCombo(Combo combo, float delay)
    {
        busy = true;

        yield return StartCoroutine(combo.PerformCombo());

        yield return new WaitForSeconds(delay);

        busy = false;
    }

    private void RandomEvade()
    {
        float forward = 0.0f;
        float up = 0.0f;

        bool p1 = Mathematics.Probability(0.5d);
        bool p2 = Mathematics.Probability(0.5d);

        if (p1)
        {
            forward = p2 ? 1.0f : -1.0f;
        }
        else
        {
            up = p2 ? 1.0f : -1.0f;
        }

        _fighter.ComplexMove(forward, up, false, false, true);
    }

    private void JumpKick()
    {
        _fighter.ComplexMove(0.0f, 1.0f, false, true, false);
    }

    private void Slide()
    {
        _fighter.ComplexMove(0.0f, -1.0f, false, true, false);
    }
    #endregion Red

    #region ChildClasses
    private class Combo
    {
        #region Variables
        private Fighter fighter;
        private List<AttackInfo> attacks = new List<AttackInfo>();
        #endregion Variables

        #region Constructors
        public Combo(Fighter v_fighter, List<AttackInfo> v_attacks)
        {
            fighter = v_fighter;
            attacks = v_attacks;
        }
        #endregion Constructors

        #region Red
        public System.Collections.IEnumerator PerformCombo()
        {
            for(int i =0; i < attacks.Count; i++)
            {
                fighter.LookAtEnemy();

                attacks[i].attack();

                yield return new WaitForSeconds(attacks[i].delay_after_attack);

                // Chance to continue combo and perform next attack
                if (!fighter.controllable || !(Mathematics.Probability(combo_chance) && Mathematics.Probability(attacks[i].next_attack_chance)))
                {
                    break;
                }
            }
        }
        #endregion Red
    }

    private class AttackInfo
    {
        #region Variables
        public AttackDelegate attack;

        public float delay_after_attack;

        public float next_attack_chance;
        #endregion Variables

        #region Constructor
        public AttackInfo(AttackDelegate v_attack, float v_delay_after_attack, float v_next_attack_chance=1.0f)
        {
            attack = v_attack;
            delay_after_attack = v_delay_after_attack;
            next_attack_chance = v_next_attack_chance;
        }
        #endregion Constructor

        #region Red
        public override string ToString()
        {
            return attack.Method.Name + "(" + delay_after_attack.ToString() + ")";
        }
        #endregion Red
    }
    #endregion ChildClasses
}