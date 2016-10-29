using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    // TODO: queue of messages

    #region Variables
    [SerializeField]
    private Text name1;

    [SerializeField]
    private Text name2;

    [SerializeField]
    private Image hp1;

    [SerializeField]
    private Image hp2;

    [SerializeField]
    private Text time_text;

    [SerializeField]
    private Text status_text;

    [SerializeField]
    private float critical_time_value;

    [SerializeField]
    private Material time_font_normal_material;

    [SerializeField]
    private Material time_font_critical_material;
    #endregion Variables

    #region MonoBehaviour
    private void Awake()
    {
        status_text.text = "";
        time_text.text = "";
        time_text.material = time_font_normal_material;
    }

    private void FixedUpdate()
    {
        UpdateHUD();
    }
    #endregion MonoBehaviour

    #region Red
    private void UpdateHUD()
    {
        if(FightData.fighter1!=null)
        {
            hp1.fillAmount = FightData.fighter1.health.CurrentHealthNormalized();
            name1.text = FightData.fighter1._name;
        }

        if (FightData.fighter2 != null)
        {
            hp2.fillAmount = FightData.fighter2.health.CurrentHealthNormalized();
            name2.text = FightData.fighter2._name;
        }

        float remains = System.Math.Max(0, FightData.round_duration - FightData.round_time);

        Material font_material = remains < critical_time_value ? time_font_critical_material : time_font_normal_material;
        
        if (time_text.material != font_material)
        {
            time_text.material = font_material;
        }

        time_text.text = remains.ToString("0");
    }

    public void Status(string s, float duration)
    {
        status_text.text = s;

        if(duration>0)
        {
            StartCoroutine(cStatusClear(duration));
        }
    }

    private System.Collections.IEnumerator cStatusClear(float delay)
    {
        yield return new WaitForSeconds(delay);

        status_text.text = "";
    }
    #endregion Red
}
