using UnityEngine;

public class Player : MonoBehaviour
{
    #region Variables
    public enum PlayerIndex { Player1, Player2 }

    [HideInInspector]
    public PlayerIndex player_index;

    private Fighter _fighter;

    #region Input
    private bool input_kick;

    private bool input_punch;

    private bool input_jump;

    private bool input_cancel;

    private bool input_taunt;

    private float input_horizontal;

    private float input_vertical;

    private string axis_prefix;
    #endregion Input
    #endregion Variables

    private void Awake()
    {
        _fighter = GetComponent<Fighter>();
    }

    private void Start()
    {
        switch(player_index)
        {
            case PlayerIndex.Player1:
                axis_prefix = "P1_";
                break;

            case PlayerIndex.Player2:
                axis_prefix = "P2_";
                break;
        }
    }

    private void Update()
    {
        if (_fighter.Stunned || _fighter.health.IsDead())
        {
            return;
        }

        input_horizontal = System.Math.Sign(Input.GetAxis(axis_prefix + "Horizontal"));
        input_vertical = System.Math.Sign(Input.GetAxis(axis_prefix + "Vertical"));
        input_kick = Input.GetButtonDown(axis_prefix + "Kick");
        input_punch = Input.GetButtonDown(axis_prefix + "Punch");
        input_jump = Input.GetButtonDown(axis_prefix + "Jump");
        input_taunt = Input.GetButtonDown(axis_prefix + "Taunt");
        input_cancel = Input.GetButtonDown(axis_prefix + "Cancel");

        if (input_horizontal != 0 || input_vertical != 0)
        {
            _fighter.Move(input_horizontal, input_vertical, true);
        }

        if (input_kick || input_punch || input_jump)
        {
            _fighter.LookAtEnemy();
        }

        if (input_taunt)
        {
            _fighter.Taunt();
        }

        if(input_kick)
        {
            _fighter.Kick();
        }
        
        if(input_punch)
        {
            _fighter.Punch();
        }

        if(input_jump)
        {
            _fighter.Jump(true);
        }
    }
}
