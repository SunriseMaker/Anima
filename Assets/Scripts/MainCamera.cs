using UnityEngine;

public class MainCamera : MonoBehaviour
{
    #region Variables
    public float camera_min_distance;
    public float camera_top;

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
        if (camera_state != CameraState.Follow)
        {
            return;
        }

        CenterBetweenTwoFighters();
    }
    #endregion MonoBehaviour

    #region Events
    private void Event_FightStart()
    {
        camera_state = CameraState.Follow;
    }

    private void Event_RoundPreStart()
    {
        camera_state = CameraState.Follow;
    }

    private void Event_RoundEnd()
    {
        StartCoroutine(cRoundEnd());
    }

    private System.Collections.IEnumerator cRoundEnd()
    {
        yield return new WaitForSeconds(FightData.delay_after_round_end);
        camera_state = CameraState.Static;
    }
    #endregion Events

    #region Red
    private void CenterBetweenTwoFighters()
    {
        if(FightData.fighter1 == null)
        {
            return;
        }

        Vector3 end_position3;
        Vector3 look_at_point3;
        Vector3 f1 = FightData.fighter1.transform.position;

        if (FightData.fighter2 == null)
        {
            // One fighter
            look_at_point3 = f1 + Vector3.up * camera_top;
            end_position3 = look_at_point3 + Vector3.right * camera_min_distance;
        }
        else
        {
            // Two fighters
            Vector3 f2 = FightData.fighter2.transform.position;
            Vector3 direction3 = f2 - f1;
            Vector3 center_point3 = (f1 + f2) / 2;

            look_at_point3 = center_point3 + Vector3.up * camera_top;

            Vector2 center_point2 = Mathematics.Vector3ToVector2xz(center_point3);
            Vector2 direction2 = Mathematics.Vector3ToVector2xz(direction3);
            Vector2 normal2 = Mathematics.Vector2Rotate90Clockwise(direction2).normalized;
            float distance = System.Math.Max(camera_min_distance, direction2.magnitude);

            Vector2 end_position2 = center_point2 + normal2 * distance;
            end_position3 = new Vector3(end_position2.x, look_at_point3.y, end_position2.y);
        }

        transform.position = end_position3;
        transform.LookAt(look_at_point3);
    }
    #endregion Red
}

/*
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    #region Variables
    public float camera_min_distance;
    public float camera_top;

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
        if (camera_state != CameraState.Follow)
        {
            return;
        }

        CenterBetweenTwoFighters();
    }
    #endregion MonoBehaviour

    #region Events
    private void Event_FightStart()
    {
        camera_state = CameraState.Follow;
    }

    private void Event_RoundPreStart()
    {
        camera_state = CameraState.Follow;
    }

    private void Event_RoundEnd()
    {
        StartCoroutine(cRoundEnd());
    }

    private System.Collections.IEnumerator cRoundEnd()
    {
        yield return new WaitForSeconds(FightData.delay_after_round_end);
        camera_state = CameraState.Static;
    }
    #endregion Events

    #region Red
    private void CenterBetweenTwoFighters()
    {
        if(FightData.fighter1 == null)
        {
            return;
        }

        Vector3 end_position3;
        Vector3 look_at_point3;
        Vector3 f1 = FightData.fighter1.transform.position;

        if (FightData.fighter2 == null)
        {
            // One fighter
            look_at_point3 = f1 + Vector3.up * camera_top;
            end_position3 = look_at_point3 + Vector3.right * camera_min_distance;
        }
        else
        {
            // Two fighters
            Vector3 f2 = FightData.fighter2.transform.position;
            Vector3 enemy_direction3 = f1 - f2;
            Vector3 center_point3 = (f1 + f2) / 2;

            look_at_point3 = center_point3 + Vector3.up * camera_top;

            Vector2 center_point2 = Mathematics.Vector3ToVector2xz(center_point3);
            Vector2 enemy_direction2 = Mathematics.Vector3ToVector2xz(enemy_direction3);
            Vector2 normal2 = Mathematics.Vector2Rotate90Clockwise(enemy_direction2).normalized;
            float distance = System.Math.Max(camera_min_distance, enemy_direction2.magnitude);

            Vector2 end_position2 = center_point2 + normal2 * distance;
            end_position3 = new Vector3(end_position2.x, look_at_point3.y, end_position2.y);
        }

        transform.position = end_position3;
        transform.LookAt(look_at_point3);
    }
    #endregion Red
}
*/