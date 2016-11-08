using UnityEngine;

public class AttackData : MonoBehaviour
{
    #region Variables
    [SerializeField]
    [Tooltip("Attacker relative force direction\nx = forward\ny = up\nz = right")]
    private Vector3 force_direction;

    public float regular_force;

    public float finisher_force;

    public float damage;

    public float attack_distance;

    public bool heavy_hit;

    public GameObject visual_effect;
    #endregion Variables

    #region Red
    public void ApplyForce(Transform attacker, Rigidbody enemy, bool finisher)
    {
        float force_amount = finisher ? finisher_force : regular_force;

        Vector3 fb = force_direction.x * attacker.forward;

        Vector3 ud = force_direction.y * attacker.up;

        Vector3 lr = force_direction.z * attacker.right;

        Vector3 calculated_force = (fb + ud + lr) * force_amount;

        if(calculated_force.magnitude>0)
        {
            enemy.AddForce(calculated_force, ForceMode.VelocityChange);
        }
    }
    #endregion Red
}
