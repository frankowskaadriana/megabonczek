using UnityEngine;


public class PlayerHealth : MonoBehaviour
{
    [Header("PlayerHealth")]
    public float HeathValue = 100f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TakeDamage();
    }
    void TakeDamage(float damage)
    {
        HeathValue -= damage;
        if (HeathValue <= 0f)
        {
            Die();
        }
    }
}
