using UnityEngine;

public class AttackData : MonoBehaviour
{
    #region Variables
    private enum ForceDirectionFB { Zero, Forward, Backward }

    private enum ForceDirectionUD { Zero, Up, Down };

    private enum ForceDirectionLR { Zero, Right, Left };

    [SerializeField]
    private ForceDirectionFB force_direction_fb;

    [SerializeField]
    private ForceDirectionUD force_direction_ud;

    [SerializeField]
    private ForceDirectionLR force_direction_lr;

    public float damage;

    public float force;

    public float attack_distance;

    public bool heavy_hit;
    #endregion Variables

    #region Red
    public Vector3 CalculateForce(Transform attacker)
    {
        Vector3 fb, ud, lr;

        switch(force_direction_fb)
        {
            case ForceDirectionFB.Forward:
                fb = attacker.forward;
                break;

            case ForceDirectionFB.Backward:
                fb = attacker.forward * -1;
                break;

            default:
                fb = Vector3.zero;
                break;
        }

        switch (force_direction_ud)
        {
            case ForceDirectionUD.Up:
                ud = attacker.up;
                break;

            case ForceDirectionUD.Down:
                ud = attacker.up*-1;
                break;

            default:
                ud = Vector3.zero;
                break;
        }

        switch (force_direction_lr)
        {
            case ForceDirectionLR.Right:
                lr = attacker.right;
                break;

            case ForceDirectionLR.Left:
                lr = attacker.right * -1;
                break;

            default:
                lr = Vector3.zero;
                break;
        }

        return (fb + ud + lr) * force;
    }
    #endregion Red
}
