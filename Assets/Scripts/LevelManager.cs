using UnityEngine;
using Unity.MLAgents;

public class MazeManager : MonoBehaviour
{
    [SerializeField] private Transform goalTransform;
    [SerializeField] private GameObject middleWall;

    private void Start()
    {
        SetupMaze();
    }

    public void SetupMaze()
    {
        bool enableWall = Random.value > 0.5f;
        middleWall.SetActive(enableWall);
        int difficulty = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("maze_difficulty", 0);
        switch (difficulty)
        {
            case 0:
                goalTransform.localPosition = new Vector3(0f, 0f, 10f);
                break;
            case 1:
                goalTransform.localPosition = new Vector3(-10f, 0f, 23f);
                break;
            case 2:
                goalTransform.localPosition = new Vector3(-20f, 0f, 23f);
                break;
            case 3:
                goalTransform.localPosition = new Vector3(-30f, 0f, 10f);
                break;
        }
    }
}
