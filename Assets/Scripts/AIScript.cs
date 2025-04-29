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

    private float goalReward = 50f;
    private float wallPenalty = -20f;
    private Vector3 previousPosition;
    private float oldDistance;
    private Rigidbody rb;


    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        rb.linearVelocity = Vector3.zero;
        previousPosition = transform.localPosition;

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
        float strafeInput = actions.ContinuousActions[2];

        float moveSpeed = 3f;
        float turnSpeed = 60f;

        transform.Rotate(Vector3.up, turnInput * turnSpeed * Time.deltaTime);

        Vector3 forwardMovement = transform.forward * moveInput * moveSpeed * Time.deltaTime;
        Vector3 strafeMovement = transform.right * strafeInput * moveSpeed * Time.deltaTime;

        transform.localPosition += forwardMovement + strafeMovement;
        float currentDistance = Vector3.Distance(transform.position, goal.transform.position);


        if (currentDistance < oldDistance)
        {
            AddReward(0.03f); 
        }
        else
        {
            AddReward(-0.04f); 
        }
        float distanceMoved = Vector3.Distance(transform.localPosition, previousPosition);
        AddReward(-0.002f);
        
        previousPosition = transform.localPosition;
        oldDistance = currentDistance;
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Vertical");
        continuousActions[1] = Input.GetAxisRaw("Horizontal");
        continuousActions[2] = Input.GetAxisRaw("Strafe");
    }

    private void OnCollisionEnter(Collision other)
    {
      
        if (other.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
           
            AddReward(wallPenalty);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
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
