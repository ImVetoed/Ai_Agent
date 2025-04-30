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
    private MazeManager mazeManager;

    [SerializeField] private Transform checkpoint1Transform;
    [SerializeField] private Transform checkpoint2Transform;

    private Collider checkpoint1Collider;
    private Collider checkpoint2Collider;

    private bool hasHitCheckpoint1 = false;
    private bool hasHitCheckpoint2 = false;

    private float goalReward = 30f;
    private float wallPenalty = -1f;
    private Vector3 previousPosition;
    private float oldDistance;
    private Rigidbody rb;
   [SerializeField] private int wallHits;

    private float wallContactTime = 0f;



    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        mazeManager = GetComponentInParent<MazeManager>();
      
    }

    public override void OnEpisodeBegin()
    {
        oldDistance = Vector3.Distance(transform.position, goal.position);
        transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        rb.linearVelocity = Vector3.zero;
        previousPosition = transform.localPosition;

        wallContactTime = 0f;

        checkpoint1Collider = checkpoint1Transform.GetComponent<Collider>();
        checkpoint2Collider = checkpoint2Transform.GetComponent<Collider>();

        hasHitCheckpoint1 = false;
        hasHitCheckpoint2 = false;
        mazeManager.SetupMaze();

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(goal.localPosition);

      
        CollectObstacleSensor(sensor);
    }
    void CollectObstacleSensor(VectorSensor sensor)
    {
        float raycastRange = 5f;
        int Rays = 12;
        float angleIncrement = 360f / Rays;

        for (int i = 0; i < Rays; i++)
        {
            float angle = i * angleIncrement;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, raycastRange))
            {
                float normalizedDistance = hit.distance / raycastRange;
                sensor.AddObservation(normalizedDistance);
            }
            else
            {
                sensor.AddObservation(0f); 
            }
        }
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
        float currentDistance = Vector3.Distance(transform.position, goal.position);
        float progressReward = (oldDistance - currentDistance) * 0.05f;
        AddReward(progressReward);
        oldDistance = currentDistance;

        if (!hasHitCheckpoint1 && checkpoint1Collider.bounds.Contains(transform.position))
        {
            AddReward(1.0f);
            hasHitCheckpoint1 = true;
        }

        if (!hasHitCheckpoint2 && checkpoint2Collider.bounds.Contains(transform.position))
        {
            AddReward(1.5f);
            hasHitCheckpoint2 = true;
        }

       
        float distanceMoved = Vector3.Distance(transform.localPosition, previousPosition);
        AddReward(-0.001f);
        
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

    private void OnCollisionStay(Collision collision)
    {
      
        if (collision.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            wallContactTime += Time.deltaTime;

            if (wallContactTime >= 0.5f)
            {
                AddReward(wallPenalty);
                floorMeshRenderer.material = loseMaterial;

                EndEpisode();

            }
           
           
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
