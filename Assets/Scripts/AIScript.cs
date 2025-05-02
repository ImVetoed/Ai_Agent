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
   

    private float maxMazeDiagonal;

    [SerializeField] private Transform checkpoint1Transform;
    [SerializeField] private Transform checkpoint2Transform;

    private MazeGenerator mazeGenerator;
    /*private bool touchingWall = false;
    private float wallContactTimer = 0f;
    private const float maxWallContactTime = 0.3f;*/

    private float goalReward = 40f;
    private float wallPenalty = -20f;
    private Vector3 previousPosition;
    private float oldDistance;
    private Rigidbody rb;


    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();

       
        mazeGenerator = GetComponentInParent<MazeGenerator>();
        maxMazeDiagonal = Mathf.Sqrt(
            mazeGenerator.width * mazeGenerator.width +
            mazeGenerator.height * mazeGenerator.height
        );

        //touchingWall = false;
        //wallContactTimer = 0f;
    }

    private void FixedUpdate()
    {
        
        /*if (touchingWall)
        {
            wallContactTimer += Time.fixedDeltaTime;
            if (wallContactTimer >= maxWallContactTime)
            {
                AddReward(wallPenalty);         
                floorMeshRenderer.material = loseMaterial;
                EndEpisode();
            }
        }*/
    }

    public override void OnEpisodeBegin()
    {
       // transform.localPosition = new Vector3(1, 0.3f, -1);
        rb.linearVelocity = Vector3.zero;

       

        
        if (mazeGenerator != null)
        {
            mazeGenerator.GenerateMaze();
        }
       
        previousPosition = transform.localPosition;
        oldDistance = Vector3.Distance(transform.position, goal.position);
       // touchingWall = false;
        //wallContactTimer = 0f;

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 toGoal = goal.position - transform.position;
        Vector3 localDir = transform.InverseTransformDirection(toGoal.normalized);

        sensor.AddObservation(localDir);                            
        sensor.AddObservation(toGoal.magnitude / maxMazeDiagonal);


        CollectObstacleSensor(sensor);
    }
    void CollectObstacleSensor(VectorSensor sensor)
    {
        float raycastRange = maxMazeDiagonal;
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
                sensor.AddObservation(1f); 
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        float moveInput = actions.ContinuousActions[0];
        float turnInput = actions.ContinuousActions[1];
        float strafeInput = actions.ContinuousActions[2];

        float moveSpeed = 2f;
        float turnSpeed = 60f;

        Vector3 move = (transform.forward * moveInput + transform.right * strafeInput) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turnInput * turnSpeed * Time.fixedDeltaTime, 0f));

       
        float currentDistance = Vector3.Distance(transform.position, goal.position);
        float delta = oldDistance - currentDistance;
        AddReward(delta * 1f);
        oldDistance = currentDistance;


        /*if (!hasHitCheckpoint1 && checkpoint1Collider.bounds.Contains(transform.position))
        {
            AddReward(1.0f);
            hasHitCheckpoint1 = true;
        }

        if (!hasHitCheckpoint2 && checkpoint2Collider.bounds.Contains(transform.position))
        {
            AddReward(1.5f);
            hasHitCheckpoint2 = true;
        }*/


        float distanceMoved = Vector3.Distance(transform.localPosition, previousPosition);
        AddReward(-0.0001f);
        
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
        if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(wallPenalty);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }

    /*private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            touchingWall = false;
            wallContactTimer = 0f;          
        }
    }*/

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
