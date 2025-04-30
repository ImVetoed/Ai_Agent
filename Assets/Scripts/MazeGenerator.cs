using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public Transform parent;
    public Transform agent;
    public Transform goal;

    private bool[,] visited;
    private HashSet<Vector2Int> floorCells = new();
    private List<Vector2Int> directions = new()
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public void Start()
    {
        GenerateMaze();
    }

    public void GenerateMaze()
    {
        visited = new bool[width, height];
        floorCells.Clear();
        ClearOldMaze();
        DFS(Vector2Int.zero);
        SpawnFloorAndWalls();
        SpawnBorderWalls();
        PositionAgentAndGoal();
    }

    void DFS(Vector2Int pos)
    {
        visited[pos.x, pos.y] = true;
        floorCells.Add(pos);

        var shuffled = new List<Vector2Int>(directions);
        shuffled.Shuffle();

        foreach (var dir in shuffled)
        {
            var next = pos + dir * 2;
            if (IsInside(next) && !visited[next.x, next.y])
            {
                floorCells.Add(pos + dir);
                DFS(next);
            }
        }
    }

    void SpawnFloorAndWalls()
    {
        // floor over bordered area
        for (int x = -1; x <= width; x++)
            for (int y = -1; y <= height; y++)
            {
                var fp = Instantiate(floorPrefab, parent);
                fp.transform.localPosition = new Vector3(x, 0f, y);
            }

        // interior walls only where no floor
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!floorCells.Contains(new Vector2Int(x, y)))
                {
                    var w = Instantiate(wallPrefab, parent);
                    w.transform.localPosition = new Vector3(x, 0.5f, y);
                }
            }
    }

    void SpawnBorderWalls()
    {
        var hor = Quaternion.identity;
        var ver = Quaternion.Euler(0f, 90f, 0f);

        // bottom & top
        for (int x = -1; x <= width; x++)
        {
            var wb1 = Instantiate(wallPrefab, parent);
            wb1.transform.localPosition = new Vector3(x, 0.5f, -1);
            wb1.transform.localRotation = hor;

            var wb2 = Instantiate(wallPrefab, parent);
            wb2.transform.localPosition = new Vector3(x, 0.5f, height);
            wb2.transform.localRotation = hor;
        }

        // left & right
        for (int y = -1; y <= height; y++)
        {
            var wl1 = Instantiate(wallPrefab, parent);
            wl1.transform.localPosition = new Vector3(-1, 0.5f, y);
            wl1.transform.localRotation = ver;

            var wl2 = Instantiate(wallPrefab, parent);
            wl2.transform.localPosition = new Vector3(width, 0.5f, y);
            wl2.transform.localRotation = ver;
        }
    }

    void PositionAgentAndGoal()
    {
        agent.localPosition = new Vector3(0, 0.5f, 0);
        goal.localPosition = new Vector3(8, 0.05f, 8);
    }

    void ClearOldMaze()
    {
        foreach (Transform c in parent) Destroy(c.gameObject);
    }

    bool IsInside(Vector2Int p)
    {
        return p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
    }
}

public static class Extensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
