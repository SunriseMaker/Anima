using UnityEngine;
using System.Collections.Generic;

public class FighterAI : MonoBehaviour
{
    #region Variables
    private delegate void AttackDelegate();

    private static float combo_min_delay = 0.25f;

    private static float combo_max_delay = 1.0f;

    private static float enemy_distance = 1.2f;

    private static float distance_accuracy = 0.1f;

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
                    new AttackInfo(_fighter.Punch, 0.3f),
                    new AttackInfo(_fighter.Kick, 1.2f)
                }
            )
            ,
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    new AttackInfo(_fighter.Punch, 0.3f),
                    new AttackInfo(_fighter.Punch, 0.3f),
                    new AttackInfo(_fighter.Kick, 1.7f)
                }
            )
            ,
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    new AttackInfo(JumpKick, 2.1f)
                }
            )
            ,
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    new AttackInfo(_fighter.Kick, 0.3f),
                    new AttackInfo(_fighter.Punch, 1.0f)
                }
            )
            ,
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    new AttackInfo(_fighter.Kick, 0.3f),
                    new AttackInfo(Jump, 1.3f)
                }
            )
            ,
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    new AttackInfo(_fighter.Kick, 0.3f),
                    new AttackInfo(_fighter.Kick, 1.0f)
                }
            )
            ,
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    new AttackInfo(BackDoubleKick, 0.5f),
                    new AttackInfo(RisingUppercut, 1.5f)
                }
            )
            ,
            new Combo
            (
                _fighter,
                new List<AttackInfo>()
                {
                    new AttackInfo(Slide, 1.4f)
                }
            )
        };
    }
    
    private void Update()
    {
        if (busy || _fighter.Stunned || _fighter.health.IsDead())
        {
            return;
        }

        if (_fighter.EnemyDistance() - enemy_distance > distance_accuracy)
        {
            _fighter.Move(1.0f, 0.0f, false);
        }
        else
        {
            RandomCombo();
        }
    }
    #endregion MonoBehaviour

    #region Combo
    private void RandomCombo()
    {
        int random_combo = Mathematics.random.Next(combos.Count);

        float combo_delay = Mathematics.random.NextFloatMinMax(combo_min_delay, combo_max_delay);

        StartCoroutine(StartCombo(combos[random_combo], combo_delay));
    }

    private System.Collections.IEnumerator StartCombo(Combo combo, float delay)
    {
        busy = true;

        yield return StartCoroutine(combo.PerformCombo());

        yield return new WaitForSeconds(delay);

        busy = false;
    }
    #endregion Combo

    #region Moves
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

    private void Jump()
    {
        _fighter.Jump(false);
    }

    private void RisingUppercut()
    {
        _fighter.ComplexMove(0.0f, 1.0f, true, false, false);
    }

    private void BackDoubleKick()
    {
        _fighter.ComplexMove(-1.0f, 0.0f, false, true, false);
    }
    #endregion Moves

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
            for(int i=0; i < attacks.Count; i++)
            {
                fighter.LookAtEnemy();

                attacks[i].attack();

                yield return new WaitForSeconds(attacks[i].delay_after_attack);

                // Chance to continue combo and perform next attack
                if (fighter.Stunned || !(Mathematics.Probability(combo_chance) && Mathematics.Probability(attacks[i].next_attack_chance)))
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