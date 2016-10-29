using UnityEngine;

public static class LayerMasks
{
    private static int _enemy;
    private static int _ground;

    public static int Enemy { get { return _enemy; } }
    public static int Ground { get { return _ground; } }

    static LayerMasks()
    {
        _enemy = LayerMask.GetMask("Fighters");
        _ground = LayerMask.GetMask("Default");
    }
}
