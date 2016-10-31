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

    private enum CameraState { Static, Follow }
    private CameraState camera_state;
    #endregion Variables

    #region MonoBehaviour
    private void Awake()
    {
        camera_state = CameraState.Static;
    }
    
    private void Start()
    {
        GameEventSystem.StartListening(GameEventSystem.EventID.FightStart, Event_FightStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundPreStart, Event_RoundPreStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundPreStart, Event_RoundPreStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundEnd, Event_RoundEnd);
    }

    private void LateUpdate()
    {
        switch(camera_state)
        {
            case CameraState.Static:
                break;

            case CameraState.Follow:
                Vector3 look_at_point, end_position;
                LookAndPosition(out look_at_point, out end_position);

                transform.position = Vector3.MoveTowards(transform.position, end_position, Time.deltaTime * camera_speed);
                transform.LookAt(look_at_point);
                break;
        }
    }
    #endregion MonoBehaviour

    #region Events
    private void Event_FightStart()
    {
        Debug.Assert(FightData.fighter1 != null);
        Debug.Assert(FightData.fighter2 != null);

        camera_state = CameraState.Static;
        Vector3 look_at_point, end_position;
        LookAndPosition(out look_at_point, out end_position);

        transform.position = end_position;
        transform.LookAt(look_at_point);
    }

    private void Event_RoundPreStart()
    {
        camera_state = CameraState.Follow;
    }

    private void Event_RoundEnd()
    {
        camera_state = CameraState.Static;
    }
    #endregion Events

    #region Red
    private void LookAndPosition(out Vector3 look_at_point, out Vector3 end_position)
    {
        look_at_point = Vector3.zero;
        end_position = Vector3.zero;

        Vector3 direction3 = FightData.fighter2.transform.position - FightData.fighter1.transform.position;
        Vector2 direction2 = Mathematics.Vector3ToVector2xz(direction3);
        Vector2 normal2 = Mathematics.Vector2Rotate90Clockwise(direction2).normalized;
        float distance = System.Math.Max(camera_min_distance, direction2.magnitude);
        Vector3 center_point3 = FightData.fighter1.MiddlePosition();
        Vector2 center_point2 = Mathematics.Vector3ToVector2xz(center_point3);
        Vector2 end_position2 = center_point2 + normal2 * distance;
        look_at_point = center_point3 + Vector3.up * camera_top;
        end_position = new Vector3(end_position2.x, look_at_point.y, end_position2.y);
    }
    #endregion Red
}
