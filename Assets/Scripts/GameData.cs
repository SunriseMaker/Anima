using UnityEngine;
using UnityEngine.EventSystems;

public class GameData : MonoBehaviour
{
    #region Variables
    public enum GameMode { Arcade, Versus, AIFight }
    public static GameMode game_mode = GameMode.Arcade;

    public enum FighterType { Player, Enemy }
    private static FighterType _fighter1_type;
    private static FighterType _fighter2_type;

    public static FighterType fighter1_type
    {
        get { return _fighter1_type; }
        private set { _fighter1_type = value; }
    }

    public static FighterType fighter2_type
    {
        get { return _fighter2_type; }
        private set { _fighter2_type = value; }
    }

    [Header("PREFABS")]
    [SerializeField]
    private Fighter fighter1_prefab;

    [SerializeField]
    private Fighter fighter2_prefab;

    [SerializeField]
    private HUD hud_prefab;

    [SerializeField]
    private FightEvents fight_events_prefab;

    [SerializeField]
    private EventSystem event_system_prefab;

    [Header("FIGHT_DATA")]
    [SerializeField]
    private Transform[] spawn_points;

    [SerializeField]
    private float distance_between_fighters;

    [SerializeField]
    private float fight_data_fighter_hp;

    [SerializeField]
    private int fight_data_win_count;

    [Header("TIMING")]
    [SerializeField]
    private float fight_data_round_duration;

    [SerializeField]
    private float delay_before_first_round;

    [SerializeField]
    private float delay_after_last_round;

    [SerializeField]
    private float delay_before_round_start;

    [SerializeField]
    private float delay_after_round_end;

    [SerializeField]
    private float delay_between_rounds;

    public static int current_scene_index;
    #endregion Variables

    #region MonoBehaviour
    private void Awake()
    {
        //game_mode
        current_scene_index = Scenes.CurrentSceneIndex();

        #region GameEventSystem
        GameEventSystem.Nullify();
        #endregion GameEventSystem

        #region FightData
        FightData.Nullify();

        Debug.Assert(fight_data_win_count > 0);
        Debug.Assert(fight_data_round_duration > 0);
        Debug.Assert(fight_data_fighter_hp > 0);

        FightData.fighter_hp = fight_data_fighter_hp;
        FightData.win_count = fight_data_win_count;
        FightData.round_duration = fight_data_round_duration;
        FightData.current_round = 0;
        FightData.delay_before_first_round = delay_before_first_round;
        FightData.delay_after_last_round = delay_after_last_round;
        FightData.delay_before_round_start = delay_before_round_start;
        FightData.delay_after_round_end = delay_after_round_end;
        FightData.delay_between_rounds = delay_between_rounds;
        
        Debug.Assert(spawn_points.Length > 0);

        Transform random_spawn_point = RandomSpawnPoint();
        float half_distance = distance_between_fighters / 2;

        switch (game_mode)
        {
            case GameMode.AIFight:
                fighter1_type = FighterType.Enemy;
                fighter2_type = FighterType.Enemy;
                break;

            case GameMode.Arcade:
                fighter1_type = FighterType.Player;
                fighter2_type = FighterType.Enemy;
                break;

            case GameMode.Versus:
                fighter1_type = FighterType.Player;
                fighter2_type = FighterType.Player;
                break;

            default:
                goto case GameMode.Versus;
        }

        InstantiateFighter(true, fighter1_prefab, ref FightData.fighter1, fighter1_type, random_spawn_point, half_distance);
        InstantiateFighter(false, fighter2_prefab, ref FightData.fighter2, fighter2_type, random_spawn_point, half_distance);
        #endregion FightData

        #region Singletones
        #region HUD
        Debug.Assert(hud_prefab != null);
        Singletones.hud = Instantiate(hud_prefab).GetComponent<HUD>();
        Debug.Assert(Singletones.hud != null);
        #endregion HUD

        #region EventSystem
        Debug.Assert(event_system_prefab != null);
        Singletones.event_system = FindObjectOfType<EventSystem>();

        if (Singletones.event_system == null)
        {
            Singletones.event_system = Instantiate(event_system_prefab).GetComponent<EventSystem>();
        }

        Debug.Assert(Singletones.event_system != null);
        #endregion EventSystem

        #region MainCamera
        Singletones.main_camera = FindObjectOfType<MainCamera>();
        Debug.Assert(Singletones.main_camera != null);
        #endregion MainCamera

        #region FightEvents
        Debug.Assert(fight_events_prefab != null);
        Instantiate(fight_events_prefab);
        #endregion FightEvents
        #endregion Singletones
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Scenes.LoadScene(Scenes.AvailableScenes.MainMenu);
        }
    }
    #endregion MonoBehaviour

    #region Red
    private void InstantiateFighter(
        bool first,
        Fighter fighter_prefab,
        ref Fighter fightdata_fighter, 
        FighterType fighter_type,
        Transform spawn_point,
        float offset
    )
    {
        Debug.Assert(fighter_prefab != null);

        float direction = first ? 1 : -1;

        fightdata_fighter = Instantiate(
            fighter_prefab,
            spawn_point.position + offset * spawn_point.right * direction,
            spawn_point.rotation
            ) as Fighter;

        float angle = first ? 270 : 90;

        fightdata_fighter.transform.Rotate(Vector3.up, angle);

        switch(fighter_type)
        {
            case FighterType.Enemy:
                fightdata_fighter.gameObject.AddComponent<FighterAI>();
                break;

            case FighterType.Player:
                Player player_component = fightdata_fighter.gameObject.AddComponent<Player>();
                player_component.player_index = first ? Player.PlayerIndex.Player1 : Player.PlayerIndex.Player2;
                break;
        }
    }

    private Transform RandomSpawnPoint()
    {
        int random_spawn_point_index = Mathematics.random.Next(spawn_points.Length);

        return spawn_points[random_spawn_point_index];
    }
    #endregion Red
}
