using UnityEngine;

public class MainCamera : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private float camera_min_distance;

    [SerializeField]
    private float camera_top;

    [SerializeField]
    private float camera_speed;

    private Fighter follow_fighter;

    private enum CameraState { Static, Follow1, Follow2 }

    private CameraState camera_state = CameraState.Static;
    #endregion Variables

    #region MonoBehaviour
    private void Start()
    {
        GameEventSystem.StartListening(GameEventSystem.EventID.FightStart, Event_FightStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundPreStart, Event_RoundPreStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundPreStart, Event_RoundPreStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundEnd, Event_RoundEnd);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundResults, Event_RoundResults);

        FocusOnTwoFighters(true);
    }

    private void LateUpdate()
    {
        switch (camera_state)
        {
            case CameraState.Static:
                break;

            case CameraState.Follow2:
                FocusOnTwoFighters(false);
                break;

            case CameraState.Follow1:
                FocusOnOneFighter(false);
                break;
        }
    }
    #endregion MonoBehaviour

    #region Events
    private void Event_FightStart()
    {
        Debug.Assert(FightData.fighter1 != null);
        Debug.Assert(FightData.fighter2 != null);

        FocusOnTwoFighters(true);
        camera_state = CameraState.Static;
    }

    private void Event_RoundPreStart()
    {
        FocusOnTwoFighters(true);
        camera_state = CameraState.Follow2;
    }

    private void Event_RoundEnd()
    {
        if (FightData.current_round_winner == null)
        {
            camera_state = CameraState.Follow2;
        }
        else
        {
            camera_state = CameraState.Follow1;
            // Loser
            follow_fighter = FightData.current_round_winner == FightData.fighter1 ? FightData.fighter2 : FightData.fighter1;
            FocusOnOneFighter(true);
        }
    }

    private void Event_RoundResults()
    {
        follow_fighter = FightData.current_round_winner;

        if (FightData.current_round_winner == null)
        {
            camera_state = CameraState.Follow2;
        }
        else
        {
            camera_state = CameraState.Follow1;
            FocusOnOneFighter(true);
        }
    }

    #endregion Events

    #region Focus
    private void FocusOnOneFighter(bool immediately)
    {
        Vector3 look_at_point = follow_fighter.transform.position + Vector3.up * camera_top;
        Vector3 end_position = look_at_point + follow_fighter.transform.forward * camera_min_distance;

        Focus(end_position, look_at_point, immediately);
    }

    private void FocusOnTwoFighters(bool immediately)
    {
        // Center between two fighters then 90 degrees away (like triangle)
        Vector3 direction3 = FightData.fighter2.transform.position - FightData.fighter1.transform.position;
        Vector2 direction2 = Mathematics.Vector3ToVector2xz(direction3);
        Vector2 normal2 = Mathematics.Vector2Rotate90Clockwise(direction2).normalized;
        float distance = System.Math.Max(camera_min_distance, direction2.magnitude);
        Vector3 center_point3 = FightData.fighter1.MiddlePosition();
        Vector2 center_point2 = Mathematics.Vector3ToVector2xz(center_point3);
        Vector2 end_position2 = center_point2 + normal2 * distance;
        Vector3 look_at_point = center_point3 + Vector3.up * camera_top;
        Vector3 end_position = new Vector3(end_position2.x, look_at_point.y, end_position2.y);

        Focus(end_position, look_at_point, immediately);
    }

    private void Focus(Vector3 end_position, Vector3  look_at_point, bool immediately)
    {
        if(immediately)
        {
            transform.position = end_position;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, end_position, Time.deltaTime * camera_speed);
        }

        transform.LookAt(look_at_point);
    }
    #endregion Focus
}
