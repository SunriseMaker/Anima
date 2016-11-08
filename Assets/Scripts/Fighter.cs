using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Fighter : MonoBehaviour
{
    #region Variables
    const float FULL_CIRCLE_RADIANS = 6.3f;

    const float ROTATION_SPEED = 1.0f;

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

    private Vector3 spawn_position;

    private Quaternion spawn_rotation;

    private float forward_input_coefficient;

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
    private static int ap_show_start;
    private static int ap_show_win;
    private static int ap_show_lose;
    private static int ap_show_taunt;
    private static int ap_combat;
    private static int ap_dead;
    #endregion AnimatorParameters

    #region StateIDs
    private static int sid_light_hit;
    private static List<int> sid_moves;
    private static List<int> sid_knockdown;
    #endregion StateIDs
    #endregion Variables

    private static float total_mass = 18.0f;

    #region StaticConstructor
    static Fighter()
    {
        ap_show_start = Animator.StringToHash("Show_Start");
        ap_show_win = Animator.StringToHash("Show_Win");
        ap_show_lose = Animator.StringToHash("Show_Lose");
        ap_show_taunt = Animator.StringToHash("Show_Taunt");
        ap_kick = Animator.StringToHash("Kick");
        ap_punch = Animator.StringToHash("Punch");
        ap_jump = Animator.StringToHash("Jump");
        ap_forward = Animator.StringToHash("Forward");
        ap_up = Animator.StringToHash("Up");
        ap_light_hit = Animator.StringToHash("Light_Hit");
        ap_heavy_hit = Animator.StringToHash("Heavy_Hit");
        ap_combat = Animator.StringToHash("Combat");
        ap_dead = Animator.StringToHash("Dead");

        sid_light_hit = Animator.StringToHash("Base.LightHit");

        sid_knockdown = new List<int>()
        {
            Animator.StringToHash("Base.Hitted.HeavyHit"),
            Animator.StringToHash("Base.Hitted.Headspring")
        };

        sid_moves = new List<int>()
        {
            Animator.StringToHash("Base.MoveForward"),
            Animator.StringToHash("Base.MoveBackward")
        };
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

        DistributeMass();

        EnableRagdoll(false);
    }

    private void Start()
    {
        spawn_position = transform.position;
        spawn_rotation = transform.rotation;
        
        if(this == FightData.fighter1)
        {
            enemy = FightData.fighter2;
            forward_input_coefficient = 1.0f;
        }
        else
        {
            enemy = FightData.fighter1;
            forward_input_coefficient = -1.0f;
        }

        float fighter_hp = FightData.fighter_hp;
        health = new Health(fighter_hp, fighter_hp);

        GameEventSystem.StartListening(GameEventSystem.EventID.FightStart, Event_FightStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundPreStart, Event_RoundPreStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundResults, Event_RoundResults);
    }

    private void FixedUpdate()
    {
        AnimatorParameters();

        if (!Controllable())
        {
            return;
        }

        const float RAY_MAX_DISTANCE = 10.0f;

        // Ray forward
        //Debug.DrawRay(transform.position, transform.forward * 2, Color.red, 2.0f);

        // Ray down
        //Debug.DrawRay(transform.position + transform.up, transform.up * -1 * RAY_MAX_DISTANCE, Color.green, 0.5f);

        Ray ray = new Ray(transform.position + transform.up, transform.up * -1);

        if (!Physics.Raycast(ray, RAY_MAX_DISTANCE, LayerMasks.Ground))
        {
            health.Kill();
            EnableRagdoll(true);
        }
    }
    #endregion MonoBehaviour

    #region Red
    public void Attack(UnityEngine.Object attack_data_object)
    {
        AttackData attack_data = ((GameObject)attack_data_object).GetComponent<AttackData>();

        Debug.Assert(attack_data != null);

        RaycastHit hit_info;
        Physics.Raycast(transform.position + transform.up, transform.forward, out hit_info, attack_data.attack_distance, LayerMasks.Enemy);
        Debug.DrawRay(transform.position + transform.up, transform.forward * attack_data.attack_distance, Color.red, 2.0f);
        Collider collider = hit_info.collider;

        if (collider == null)
        {
            return;
        }

        Debug.Assert(collider.name != name);

        Fighter _enemy = collider.GetComponent<Fighter>();

        Debug.Assert(_enemy != null);

        if (_enemy.health.IsDead())
        {
            return;
        }

        if(attack_data.visual_effect!=null)
        {
            Instantiate(attack_data.visual_effect, hit_info.point, attack_data.visual_effect.transform.rotation);
        }

        _enemy.health.Injure(attack_data.damage);

        if (_enemy.health.IsDead())
        {
            _enemy.EnableRagdoll(true);

            attack_data.ApplyForce(transform, _enemy.main_rigidbody, true);

            // Death animation
            //_enemy._animator.SetBool(ap_dead, true);
        }
        else
        {
            int trigger = attack_data.heavy_hit ? ap_heavy_hit : ap_light_hit;

            // Nothing can interrupt HeavyHit state
            // HeavyHit can interrupt LightHit state
            
            if (!_enemy.InKnockdownState())
            {
                _enemy.LookAtEnemy();
                _enemy._animator.SetTrigger(trigger);
            }

            attack_data.ApplyForce(transform, _enemy.main_rigidbody, false);
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

    private void Event_RoundResults()
    {
        const float FAR_AWAY = 1000.0f;

        if (health.IsDead())
        {
            transform.position = new Vector3(FAR_AWAY, FAR_AWAY, FAR_AWAY);
            return;
        }

        TeleportToSpawnPoint();
        LookAtCamera(true);

        if (FightData.current_round_winner == this)
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

    private void DistributeMass()
    {
        int rb_count = all_rigidbodies.Count();
        
        Debug.Assert(rb_count > 0);
        
        float rb_mass = total_mass / rb_count;
        
        foreach (Rigidbody r in all_rigidbodies)
        {
            r.mass = rb_mass;
        }
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

    public bool InKnockdownState()
    {
        return InState(sid_knockdown);
    }

    public bool InHittedState()
    {
        return InKnockdownState() || InState(ap_light_hit);
    }

    private bool InState(int state)
    {
        return state == CurrentStateID();
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

    private void LookAtCamera(bool instant_rotation)
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
    public void Move(float input_forward, float input_up, bool absolute_input)
    {
        if (absolute_input)
        {
            input_forward = input_forward * forward_input_coefficient;
        }

        current_input_forward = input_forward;
        current_input_up = input_up;

        if (InMoveState())
        {
            if (input_forward != 0)
            {
                LookAtEnemy();

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

    public void Jump(bool user)
    {
        if(user)
        {
            StartCoroutine(TriggerBoolParameter(ap_jump));
        }
        else
        {
            _animator.SetBool(ap_jump, true);
        }
    }

    public void Taunt()
    {
        LookAtEnemy();
        _animator.SetTrigger(ap_show_taunt);
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

    public float EnemyDistance()
    {
        return Vector2.Distance(
            Mathematics.Vector3ToVector2xz(enemy.transform.position),
            Mathematics.Vector3ToVector2xz(transform.position)
            );
    }

    public bool Controllable()
    {
        return !(!FightData.action || health.IsDead() || InHittedState()); 
    }
    #endregion Control
}
