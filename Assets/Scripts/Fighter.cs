using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Fighter : MonoBehaviour
{
    #region Variables
    const float FULL_CIRCLE_RADIANS = 6.3f;

    public string _name;

    [Header("HEALTH")]
    [HideInInspector]
    public Health health;

    [SerializeField]
    private float speed_forward;

    [SerializeField]
    private float speed_backward;

    [SerializeField]
    private float speed_sideward;

    [SerializeField]
    private CharacterJoint main_joint;

    private Rigidbody[] all_rigidbodies;

    private Collider[] all_colliders;

    private Animator _animator;

    private Fighter enemy;

    private Collider main_collider;

    private Rigidbody main_rigidbody;

    private bool ragdoll;

    private bool _controllable;

    [HideInInspector]
    public bool controllable
    {
        get { return _controllable; }

        private set { _controllable = value; }
    }

    private Vector3 spawn_position;

    private Quaternion spawn_rotation;

    private float current_input_forward;
    private float previous_input_forward;

    private float current_input_up;
    private float previous_input_up;

    #region AnimatorParameters
    private static int ap_kick;
    private static int ap_punch;
    private static int ap_jump;
    private static int ap_forward;
    private static int ap_up;
    private static int ap_light_hit;
    private static int ap_heavy_hit;
    private static int ap_taunt;
    private static int ap_show_start;
    private static int ap_show_win;
    private static int ap_show_lose;
    private static int ap_combat;
    private static int ap_dead;
    #endregion AnimatorParameters

    private static float heavy_hit_duration;
    private static float light_hit_duration;

    #region StateIDs
    private static int sid_heavy_hit;
    private static int sid_light_hit;

    private static List<int> sid_moves;
    private static List<int> sid_hits;
    #endregion StateIDs
    #endregion Variables

    #region StaticConstructor
    static Fighter()
    {
        ap_show_start = Animator.StringToHash("Show_Start");
        ap_show_win = Animator.StringToHash("Show_Win");
        ap_show_lose = Animator.StringToHash("Show_Lose");
        ap_kick = Animator.StringToHash("Kick");
        ap_punch = Animator.StringToHash("Punch");
        ap_jump = Animator.StringToHash("Jump");
        ap_forward = Animator.StringToHash("Forward");
        ap_up = Animator.StringToHash("Up");
        ap_light_hit = Animator.StringToHash("Light_Hit");
        ap_heavy_hit = Animator.StringToHash("Heavy_Hit");
        ap_taunt = Animator.StringToHash("Taunt");
        ap_combat = Animator.StringToHash("Combat");
        ap_dead = Animator.StringToHash("Dead");

        sid_heavy_hit = Animator.StringToHash("Base.HeavyHit");
        sid_light_hit = Animator.StringToHash("Base.LightHit");

        sid_hits = new List<int>() { sid_heavy_hit, sid_light_hit };

        sid_moves = new List<int>();
        sid_moves.Add(Animator.StringToHash("Base.MoveForward"));
        sid_moves.Add(Animator.StringToHash("Base.MoveBackward"));

        heavy_hit_duration = 3.0f;
        light_hit_duration = 0.1f;
    }
    #endregion StaticConstructor

    #region MonoBehaviour
    private void Awake()
    {
        int id = GetInstanceID();

        _animator = GetComponent<Animator>();
        
        all_rigidbodies = GetComponentsInChildren<Rigidbody>();
        all_colliders = GetComponentsInChildren<Collider>();
        main_collider = GetComponent<Collider>();
        main_rigidbody = GetComponent<Rigidbody>();

        EnableRagdoll(false);
    }

    private void Start()
    {
        spawn_position = transform.position;
        spawn_rotation = transform.rotation;
        
        enemy = FightData.fighter1 == this ? FightData.fighter2 : FightData.fighter1;

        float fighter_hp = FightData.fighter_hp;
        health = new Health(fighter_hp, fighter_hp);

        GameEventSystem.StartListening(GameEventSystem.EventID.FightStart, Event_FightStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundPreStart, Event_RoundPreStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundStart, Event_RoundStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundEnd, Event_RoundEnd);
    }

    private void FixedUpdate()
    {
        AnimatorParameters();

        if (!FightData.action || health.IsDead())
        {
            return;
        }

        const float RAY_MAX_DISTANCE = 5.0f;

        //Debug.DrawRay(transform.position, transform.forward * 2, Color.red, 2.0f);
        
        //Debug.DrawRay(transform.position + transform.up, transform.up * -1 * RAY_MAX_DISTANCE, Color.green, 0.5f);

        Ray ray = new Ray(transform.position + transform.up, transform.up * -1);

        if (!Physics.Raycast(ray, RAY_MAX_DISTANCE, LayerMasks.Ground))
        {
            health.Kill();
        }
    }
    #endregion MonoBehaviour

    #region Red
    public void Attack(UnityEngine.Object attack_data_object)
    {
        GameObject attack_data_gameobject = (GameObject)attack_data_object;
        AttackData attack_data_component = attack_data_gameobject.GetComponent<AttackData>();

        RaycastHit hit_info;
        Physics.Raycast(transform.position + transform.up, transform.forward, out hit_info, attack_data_component.attack_distance, LayerMasks.Enemy);
        //Debug.DrawRay(transform.position + transform.up, transform.forward * attack_data_component.attack_distance, Color.red, 2.0f);
        Collider collider = hit_info.collider;

        if (collider != null)
        {
            Fighter _enemy = collider.GetComponent<Fighter>();
            _enemy.Hit(attack_data_component, transform);
        }
    }

    public System.Collections.IEnumerator ActivateRagdoll(float duration)
    {
        EnableRagdoll(true);

        bool trigger_control = controllable == true;

        if(trigger_control)
        {
            controllable = false;
        }

        yield return new WaitForSeconds(duration);

        EnableRagdoll(false);

        if (trigger_control)
        {
            controllable = true;
        }

        LookAtEnemy();
    }

    public System.Collections.IEnumerator cJump(float height, float duration, int steps)
    {
        float half_duration = duration / 2;
        int half_steps = steps / 2;
        float time_step = half_duration / half_steps;
        Vector3 delta = Vector3.up * height / half_steps;

        for (var i = 1; i < half_steps; i++)
        {
            transform.Translate(delta);

            yield return new WaitForSeconds(time_step);
        }

        delta *= -1;

        for (var i = 1; i < half_steps; i++)
        {
            transform.Translate(delta);

            yield return new WaitForSeconds(time_step);
        }
    }

    #region EventReactions
    private void Event_FightStart()
    {
        _animator.SetTrigger(ap_show_start);
    }

    private void Event_RoundPreStart()
    {
        if(health.IsDead())
        {
            health.Resurrect(FightData.fighter_hp);
            _animator.SetBool(ap_dead, false);
        }
        else
        {
            health.Heal(FightData.fighter_hp);
        }

        EnableRagdoll(false);

        TeleportToSpawnPoint();
        _animator.SetTrigger(ap_combat);
    }

    private void Event_RoundStart()
    {
        controllable = true;
    }

    private void Event_RoundEnd()
    {
        controllable = false;

        if (health.IsDead())
        {
            return;
        }

        StartCoroutine(cRoundEnd());
    }

    private System.Collections.IEnumerator cRoundEnd()
    {
        yield return new WaitForSeconds(FightData.delay_after_round_end);

        LookAtCamera();

        Fighter winner = FightData.RoundWinner();

        if (winner == this)
        {
            _animator.SetTrigger(ap_show_win);
        }
        else
        {
            _animator.SetTrigger(ap_show_lose);
        }
    }
    #endregion EventReactions

    private void TeleportToSpawnPoint()
    {
        transform.position = spawn_position;
        transform.rotation = spawn_rotation;
    }

    private void EnableRagdoll(bool b)
    {
        ragdoll = b;

        foreach (Rigidbody r in all_rigidbodies)
        {
            r.isKinematic = !b;
        }

        foreach (Collider c in all_colliders)
        {
            c.enabled = b;
        }

        main_joint.connectedBody = b ? main_rigidbody : null;
        main_rigidbody.isKinematic = false;
        main_collider.enabled = !b;

        _animator.enabled = !b;
    }

    private void Hit(AttackData attack_data, Transform attacker)
    {
        if(health.IsDead())
        {
            return;
        }

        health.Injure(attack_data.damage);

        if (health.IsDead())
        {
            // Ragdoll
            //StartCoroutine(ActivateRagdoll(FightData.delay_between_rounds));

            EnableRagdoll(true);

            // Force
            Vector3 force = attack_data.CalculateForce(attacker);

            if (force.magnitude > 0)
            {
                main_rigidbody.AddForce(force, ForceMode.VelocityChange);
            }
            else
            {
                _animator.SetBool(ap_dead, true);
            }
        }
        else
        {
            float hit_duration = attack_data.heavy_hit ? heavy_hit_duration : light_hit_duration;

            // Stun & Immunity
            StopCoroutine("TriggerHit");
            StartCoroutine(TriggerHit(hit_duration));

            int trigger = attack_data.heavy_hit ? ap_heavy_hit : ap_light_hit;
            int current_state_id = CurrentStateID();

            // Nothing can interrupt HeavyHit state
            // HeavyHit can interrupt LightHit state

            if (current_state_id != sid_heavy_hit && !(current_state_id == sid_light_hit && !attack_data.heavy_hit))
            {
                _animator.SetTrigger(trigger);
            }
        }
    }

    private System.Collections.IEnumerator TriggerHit(float duration)
    {
        health.SetImmunity(true);
        controllable = false;
        
        yield return new WaitForSeconds(duration);

        controllable = true;
        health.SetImmunity(false);
    }
    #endregion Red

    #region Animator
    private void AnimatorParameters()
    {
        if (current_input_forward != previous_input_forward)
        {
            _animator.SetFloat(ap_forward, current_input_forward);
            previous_input_forward = current_input_forward;
        }

        if (current_input_up != previous_input_up)
        {
            _animator.SetFloat(ap_up, current_input_up);
            previous_input_up = current_input_up;
        }

        current_input_forward = 0;
        current_input_up = 0;
    }

    private System.Collections.IEnumerator TriggerBoolParameter(int parameter)
    {
        _animator.SetBool(parameter, true);

        yield return new WaitForSeconds(0.1f);

        _animator.SetBool(parameter, false);
    }

    private bool InMoveState()
    {
        return InState(sid_moves);
    }

    private bool InHitState()
    {
        return InState(sid_hits);
    }

    private bool InState(List<int> states)
    {
        int state_id = CurrentStateID();

        return states.Any((x) => x == state_id);
    }

    private int CurrentStateID()
    {
        AnimatorStateInfo state_info = _animator.GetCurrentAnimatorStateInfo(0);

        return state_info.fullPathHash;
    }
    #endregion Animator

    #region Look
    public void LookAtEnemy()
    {
        LookAtTarget(enemy.transform.position);
    }

    private void LookAtCamera()
    {
        LookAtTarget(Singletones.main_camera.transform.position);
    }

    private void LookAtTarget(Vector3 target_position)
    {
        Vector3 target_direction = (target_position - transform.position).normalized;

        //Debug.DrawRay(transform.position, target_direction, Color.blue, 2.0f);

        // Instant rotation
        float step = FULL_CIRCLE_RADIANS;

        // Smooth rotation
        //  float step = FULL_CIRCLE_RADIANS * Time.deltaTime;

        Vector3 new_direction = Vector3.RotateTowards(transform.forward, target_direction, step, 0.0f);

        //Debug.DrawRay(transform.position, new_direction, Color.blue, 2.0f);

        Quaternion look_rotation = Quaternion.LookRotation(new_direction);

        transform.rotation = Quaternion.Euler(0, look_rotation.eulerAngles.y, 0);
    }
    #endregion Look

    #region Control
    public void Move(float horizontal_input, float input_up, bool absolute_input)
    {
        float input_forward;

        if (absolute_input)
        {
            Vector3 middle_position = MiddlePosition();
            Vector3 camera_position = Singletones.main_camera.transform.position;
            Vector3 v1 = middle_position - transform.position;
            Vector3 v2 = middle_position - enemy.transform.position;
            Vector3 v3 = camera_position - transform.position;
            Vector3 v4 = camera_position - enemy.transform.position;
            double angle1 = Mathematics.SignedAngleDegrees(Mathematics.Vector3ToVector2xz(v1), Mathematics.Vector3ToVector2xz(v3));
            double angle2 = Mathematics.SignedAngleDegrees(Mathematics.Vector3ToVector2xz(v3), Mathematics.Vector3ToVector2xz(v4));

            float forward_input_coefficient = angle1 > angle2 ? -1.0f : 1.0f;

            input_forward = horizontal_input * forward_input_coefficient;
        }
        else
        {
            input_forward = horizontal_input;
        }

        current_input_forward = input_forward;
        current_input_up = input_up;

        if (InMoveState())
        {
            if (input_forward != 0)
            {
                float speed = System.Math.Sign(input_forward) > 0 ? speed_forward : speed_backward;
                transform.position = Vector3.MoveTowards(transform.position, enemy.transform.position, speed * input_forward * Time.deltaTime);
            }
        }
    }

    public void Kick()
    {
        _animator.SetTrigger(ap_kick);
    }

    public void Punch()
    {
        _animator.SetTrigger(ap_punch);
    }

    public void Jump()
    {
        StartCoroutine(TriggerBoolParameter(ap_jump));
    }

    public void Taunt()
    {
        LookAtEnemy();
        _animator.SetTrigger(ap_taunt);
    }
    public void ComplexMove(float forward, float up, bool punch, bool kick, bool jump)
    {
        current_input_forward = forward;
        current_input_up = up;

        _animator.SetFloat(ap_forward, forward);
        _animator.SetFloat(ap_up, up);

        if (punch)
        {
            _animator.SetTrigger(ap_punch);
        }

        if (kick)
        {
            _animator.SetTrigger(ap_kick);
        }

        if (jump)
        {
            _animator.SetTrigger(ap_jump);
        }
    }

    public Vector3 MiddlePosition()
    {
        return (transform.position + enemy.transform.position) / 2;
    }

    public Vector2 EnemyDirection()
    {
        return enemy.transform.position - transform.position;
    }

    public float EnemyDistance()
    {
        return Vector3.Distance(enemy.transform.position, transform.position);
    }
    #endregion Control
}
