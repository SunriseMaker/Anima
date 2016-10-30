using UnityEngine;

public class FightEvents : MonoBehaviour
{
    #region MonoBehaviour
    private void Start()
    {
        GameEventSystem.StartListening(GameEventSystem.EventID.FightStart, Event_FightStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundPreStart, Event_RoundPreStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundStart, Event_RoundStart);
        GameEventSystem.StartListening(GameEventSystem.EventID.RoundEnd, Event_RoundEnd);
        GameEventSystem.StartListening(GameEventSystem.EventID.FightEnd, Event_FightEnd);

        GameEventSystem.TriggerEvent(GameEventSystem.EventID.FightStart);
    }

    private void Update()
    {
        if (FightData.action)
        {
            FightData.round_time += Time.deltaTime;

            if (FightData.round_time >= FightData.round_duration || FightData.OneOfFightersIsDead())
            {
                GameEventSystem.TriggerEvent(GameEventSystem.EventID.RoundEnd);
            }
        }
    }
    #endregion MonoBehaviour

    #region Red
    #region FightStart
    private void Event_FightStart()
    {
        StartCoroutine(cFightStart());
    }

    private System.Collections.IEnumerator cFightStart()
    {
        Singletones.hud.Status(FightData.fighter1._name + "\nVS\n " + FightData.fighter2._name, FightData.delay_before_first_round / 2);

        yield return new WaitForSeconds(FightData.delay_before_first_round);

        GameEventSystem.TriggerEvent(GameEventSystem.EventID.RoundPreStart);
    }
    #endregion FightStart

    #region RoundPreStart
    private void Event_RoundPreStart()
    {
        FightData.round_time = 0;
        FightData.current_round++;

        Singletones.hud.Status("ROUND "+FightData.current_round.ToString() + "\nFIGHT!", FightData.delay_before_round_start);

        StartCoroutine(cRoundPreStart());
    }

    private System.Collections.IEnumerator cRoundPreStart()
    {
        yield return new WaitForSeconds(FightData.delay_before_round_start);

        GameEventSystem.TriggerEvent(GameEventSystem.EventID.RoundStart);
    }
    #endregion RoundPreStart

    #region RoundStart
    private void Event_RoundStart()
    {
        FightData.action = true;

        // Next event will be triggered in Update function
    }
    #endregion RoundStart

    #region RoundEnd
    private void Event_RoundEnd()
    {
        FightData.action = false;

        StartCoroutine(cRoundEnd());
    }

    private System.Collections.IEnumerator cRoundEnd()
    {
        string message = FightData.round_time >= FightData.round_duration ? "TIME IS UP" : "K.O.";

        Singletones.hud.Status(message, FightData.delay_after_round_end);

        yield return new WaitForSeconds(FightData.delay_after_round_end);

        Fighter winner = FightData.RoundWinner();

        if(winner==FightData.fighter1)
        {
            FightData.fighter1_win_count++;
        }
        else
        {
            if (winner == FightData.fighter2)
            {
                FightData.fighter2_win_count++;
            }
        }

        if(winner == null)
        {
            message = "DRAW";
        }
        else
        {
            message = winner._name+ "\nWINS";

            if(winner.health.CurrentHealth()== winner.health.MaxHealth())
            {
                message += "\nPERFECT";
            }
        }

        Singletones.hud.Status(message, FightData.delay_between_rounds);

        if (System.Math.Max(FightData.fighter1_win_count, FightData.fighter2_win_count) < FightData.win_count)
        {
            yield return new WaitForSeconds(FightData.delay_between_rounds);

            GameEventSystem.TriggerEvent(GameEventSystem.EventID.RoundPreStart);
        }
        else
        {
            yield return new WaitForSeconds(FightData.delay_after_last_round);

            GameEventSystem.TriggerEvent(GameEventSystem.EventID.FightEnd);
        }
    }
    #endregion RoundEnd

    #region FightEnd
    private void Event_FightEnd()
    {
        StartCoroutine(cFightEnd());
    }

    private System.Collections.IEnumerator cFightEnd()
    {
        Fighter winner = FightData.FightWinner();
        string winner_name = winner == null ? "" : winner._name;

        Singletones.hud.Status(winner_name+"\n WON THE FIGHT", FightData.delay_after_last_round);

        yield return new WaitForSeconds(FightData.delay_after_last_round);

        UnityEngine.SceneManagement.SceneManager.LoadScene(GameData.current_scene_index, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    #endregion FightEnd
    #endregion Red
}
