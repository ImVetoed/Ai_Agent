using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AIScript : Agent
{
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;

    [SerializeField] private Transform goal;

    private float goalReward = 10f;
    private float wallPenalty = -1f;
    private float previousDistanceToGoal;
    private const float StepPenalty = -0.01f;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
        previousDistanceToGoal = Vector3.Distance(transform.localPosition, goal.localPosition);
        floorMeshRenderer.material = loseMaterial;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(goal.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float movespeed = 3f;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * movespeed;

        float currentDistanceToGoal = Vector3.Distance(transform.localPosition, goal.localPosition);
        float reward = (previousDistanceToGoal - currentDistanceToGoal) * 0.1f + StepPenalty;
        AddReward(reward);
        previousDistanceToGoal = currentDistanceToGoal;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(wallPenalty);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(goalReward);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
    }
}