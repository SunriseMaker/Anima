using UnityEngine;

public static class Layers
{
    private static int _fighters;
    private static int _default;

    public static int Fighters { get { return _fighters; } }
    public static int Default { get { return _default; } }

    static Layers()
    {
        _fighters = LayerMask.NameToLayer("Fighters");
        _default = LayerMask.NameToLayer("Default");
    }
}
