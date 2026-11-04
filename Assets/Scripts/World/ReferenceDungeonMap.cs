using UnityEngine;

public class ReferenceDungeonMap : MonoBehaviour
{
    [System.Serializable] public class Marker { public int x; public int y; }
    [System.Serializable] public class Layout { public int width; public int height; public string[] rows; public Marker[] enemies; }
    public TextAsset layoutAsset;
    public float cellSize = 1.5f;
    private Layout data;
    public Layout Data { get { if (data == null) data = JsonUtility.FromJson<Layout>(layoutAsset.text); return data; } }
    public Vector3 Cell(float x, float y) => transform.position + new Vector3((x - 19) * cellSize, (9 - y) * cellSize, 0);
    public Vector2 Grid(Vector3 position) => new Vector2((position.x - transform.position.x) / cellSize + 19, 9 - (position.y - transform.position.y) / cellSize);
    public bool Walkable(Vector2 position, Vector2 halfSize)
    {
        Vector2 grid = Grid(position);
        int minX = Mathf.FloorToInt(grid.x - halfSize.x / cellSize + 0.5f);
        int maxX = Mathf.FloorToInt(grid.x + halfSize.x / cellSize + 0.5f);
        int minY = Mathf.FloorToInt(grid.y - halfSize.y / cellSize + 0.5f);
        int maxY = Mathf.FloorToInt(grid.y + halfSize.y / cellSize + 0.5f);
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if (x < 0 || y < 0 || x >= Data.width || y >= Data.height || Data.rows[y][x] != '.') return false;
        return true;
    }
}
