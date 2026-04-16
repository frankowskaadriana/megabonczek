using UnityEngine;

public class ExpOrbs : MonoBehaviour
{
    public int expValue = 10;
    public float pickupRange = 2f;
    public float moveSpeed = 5f;

    private Transform player;
    private LevelMechanic levelMechanic;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            levelMechanic = playerObj.GetComponent<LevelMechanic>();
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= pickupRange)
            {
                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

                if (distance < 0.1f)
                {
                    Collect();
                }
            }
        }
    }

    void Collect()
    {
        if (levelMechanic != null)
        {
            levelMechanic.AddExp(expValue);
            Debug.Log($"Zebrano kryszta³ EXP! +{expValue} EXP");
        }

        Destroy(gameObject);
    }
}