using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : MonoBehaviour
{
    [SerializeField]
    private GameObject enemy;
    [SerializeField]
    private int enemyCount;
    void Update()
    {
        if (Enemy.total < enemyCount)
            CreateEnemy();
    }
    RaycastHit hit;
    void CreateEnemy()
    {
        transform.position = new Vector3(Random.Range(-65, 56), transform.position.y, Random.Range(-65, 71));
        if (Physics.Raycast(transform.position, -transform.up, out hit, 100))
        {
            Debug.DrawRay(transform.position, hit.point - transform.position, Color.red);
            if (hit.collider.tag == "Ground")
            {
                Instantiate(enemy, hit.point, Quaternion.identity);
                Enemy.total++;
            }
        }
    }
}
