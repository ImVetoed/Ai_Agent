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

    private float goalReward = 200f;
    private float wallPenalty = -0.5f;
    private float previousDistanceToGoal;
    private float previousPositionMagnitude;
    private const float StepPenalty = -0.01f;
    private Rigidbody rb;


    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        rb.linearVelocity = Vector3.zero;
        previousDistanceToGoal = Vector3.Distance(transform.localPosition, goal.localPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(goal.localPosition);

      
        CollectObstacleSensor(sensor);
    }
    void CollectObstacleSensor(VectorSensor sensor)
    {
        float raycastRange = 3f;

        RaycastHit hit;
        sensor.AddObservation(Physics.Raycast(transform.position, transform.forward, out hit, raycastRange) ? 1f : 0f);
        sensor.AddObservation(Physics.Raycast(transform.position, transform.right, out hit, raycastRange) ? 1f : 0f);
        sensor.AddObservation(Physics.Raycast(transform.position, -transform.right, out hit, raycastRange) ? 1f : 0f);
        sensor.AddObservation(Physics.Raycast(transform.position, -transform.forward, out hit, raycastRange) ? 1f : 0f);

      
        sensor.AddObservation(Physics.Raycast(transform.position, (transform.forward + transform.right).normalized, out hit, raycastRange) ? 1f : 0f);
        sensor.AddObservation(Physics.Raycast(transform.position, (transform.forward - transform.right).normalized, out hit, raycastRange) ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveInput = actions.ContinuousActions[0];
        float turnInput = actions.ContinuousActions[1];
        float moveSpeed = 3f;
        float turnSpeed = 180f;

        transform.Rotate(Vector3.up, turnInput * turnSpeed * Time.deltaTime);

        // Move forward/backward
        Vector3 forwardMovement = transform.forward * moveInput * moveSpeed * Time.deltaTime;
        // Strafe left/right
        Vector3 strafeMovement = transform.right * actions.ContinuousActions[2] * moveSpeed * Time.deltaTime;

        transform.localPosition += forwardMovement + strafeMovement;

        Vector3 currentPosition = transform.localPosition;
        float currentDistanceToGoal = Vector3.Distance(currentPosition, goal.localPosition);
        float reward = (previousDistanceToGoal - currentDistanceToGoal) * 0.5f + StepPenalty;

        AddReward(reward);
        previousDistanceToGoal = currentDistanceToGoal;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Vertical");
        continuousActions[1] = Input.GetAxisRaw("Horizontal");
        continuousActions[2] = Input.GetAxisRaw("Strafe");
    }

    private void OnCollisionStay(Collision other)
    {
      
        if (other.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
           
            SetReward(wallPenalty);
            floorMeshRenderer.material = loseMaterial;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(goalReward);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
    }
}
