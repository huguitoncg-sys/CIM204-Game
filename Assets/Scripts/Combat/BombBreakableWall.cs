using UnityEngine;

public class BombBreakableWall : MonoBehaviour
{
    public void Break()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
