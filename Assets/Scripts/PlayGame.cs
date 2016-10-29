using UnityEngine;

public class PlayGame : MonoBehaviour
{
    [SerializeField]
    private GameData.GameMode game_mode;

    public void Play()
    {
        GameData.game_mode = game_mode;
        
        Scenes.LoadScene(Scenes.AvailableScenes.Arena);
    }
}
