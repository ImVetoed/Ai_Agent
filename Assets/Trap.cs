using UnityEngine;

public class Trap : MonoBehaviour
{
    public float trapKillTime = 3.0f;
    public float timeInTrap = 0f;
    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<AIScript>(out AIScript agent))    
        {
            timeInTrap += Time.deltaTime;
            if (timeInTrap >= trapKillTime)
            {
                agent.SetReward(-1f);
                agent.EndEpisode();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<AIScript>(out AIScript agent))
        {
            timeInTrap = 0f; 
        }
    }
}
