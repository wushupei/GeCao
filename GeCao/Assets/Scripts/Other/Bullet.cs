using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Bullet : MonoBehaviour
{
    Player player;
    float attack;
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * 30 + Vector3.up * 10, ForceMode.Impulse);
    }
    public void Init(Player _player, float _attack)
    {
        player = _player;
        attack = _attack;
    }
    void OnCollisionEnter(Collision other) //碰撞
    {
        Destroy(gameObject);
    }
    void OnDisable()
    {
        //加载特效,2s后销毁
        GameObject effect = Resources.Load<GameObject>("Prefab/Effect/Boom");
        effect = Instantiate(effect, transform.position, effect.transform.rotation);
        Destroy(effect, 2);

        Collider[] collids = Physics.OverlapSphere(transform.position, 3, LayerMask.GetMask("Enemy"));
        for (int i = 0; i < collids.Length; i++)
        {
            collids[i].GetComponent<Enemy>().HitFly(transform, 5, attack);
        }
    }
}
