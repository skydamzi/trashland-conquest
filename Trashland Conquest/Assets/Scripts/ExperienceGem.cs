using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceGem : MonoBehaviour
{
    public int experienceAmount = 1;
    public float magnetRange = 2f;
    public float attractionSpeed = 5f;

    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= magnetRange)
            {
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                transform.position += direction * attractionSpeed * Time.deltaTime;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (PlayerStatus.instance != null)
            {
                PlayerStatus.instance.AddExperience(experienceAmount);
            }
            Destroy(gameObject);
        }
    }

    public void SetExperience(int amount)
    {
        experienceAmount = amount;
    }
}
