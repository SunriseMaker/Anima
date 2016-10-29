using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public static class Scenes
{
    public enum AvailableScenes { MainMenu, Arena }
    public static List<KeyValuePair<AvailableScenes, int>> all_scenes;

    static Scenes()
    {
        all_scenes = new List<KeyValuePair<AvailableScenes, int>>();
        all_scenes.Add(new KeyValuePair<AvailableScenes, int>(AvailableScenes.MainMenu, 0));
        all_scenes.Add(new KeyValuePair<AvailableScenes, int>(AvailableScenes.Arena, 1));
    }

    public static void LoadScene(AvailableScenes scene)
    {
        int scene_index = all_scenes.Find((x) => x.Key == scene).Value;
        SceneManager.LoadScene(scene_index);
    }

    public static int CurrentSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }
}
