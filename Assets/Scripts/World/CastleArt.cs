using UnityEngine;

public static class CastleArt
{
    public static Sprite Get(string name)
    {
        foreach (var sprite in Resources.LoadAll<Sprite>("CastleTiles"))
            if (sprite.name == name) return sprite;
        return null;
    }
}
