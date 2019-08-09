using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public static int total = 0;
    static int killCount = 0;
    [HideInInspector]
    public Player player;
    EnemyAnima enemyAnima;
    Rigidbody rig;
    Collider cameraTH; //摄像机

    public ParticleSystem bloodEffect; //飙血特效

    bool isGround; //是否在地面
    float attCD = 0;//攻击冷却
    [SerializeField]
    private float speed;
    public float attack;
    bool hitFly = false; //是否被击飞
    Text killText;
    void Start()
    {        
        player = FindObjectOfType<Player>();
        enemyAnima = GetComponentInChildren<EnemyAnima>();
        enemyAnima.enemy = this;
        InitHpSlider();
        rig = GetComponent<Rigidbody>();
        killText = GameObject.Find("KillCount").GetComponent<Text>();
        cameraTH = GameObject.Find("CameraTH").GetComponent<Collider>();
    }
    void Update()
    {
        if (MainGame.Instanc.second <= 0)
            return;
        if (transform.position.y < -20) //掉落到一定距离销毁尸体
        {
            Destroy(gameObject);
            total--;
            ShowKillCount();
        }

        //判断和玩家距离,//不同距离播放不同动画    
        float dis = (player.transform.position - transform.position).magnitude;
        enemyAnima.SwitchAnimaForDis(dis);

        //跑步动画时面向玩家前进
        if (enemyAnima.animaState.IsName("run"))
        {
            LookAtPlayer(player.transform.position);
            Run();
        }
        //准备动画时面向玩家攻击
        if (enemyAnima.animaState.IsName("readyAttack") && PlayerData.Instanc.hp > 0)
        {
            LookAtPlayer(player.transform.position);
            AtWillAttack();
        }
        else
            attCD = 0;

        HideEnemyHpSlider();
    }
    public void LookAtPlayer(Vector3 position) //面向玩家
    {
        Vector3 pos = new Vector3(position.x, transform.position.y, position.z);
        Vector3 dir = pos - transform.position; //玩家方向
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5);
    }
    public void Run() //前进
    {
        transform.Translate(transform.forward * Time.deltaTime * speed, Space.World);
    }
    void AtWillAttack() //随意攻击 
    {
        attCD += Time.deltaTime;
        //每两秒决策一次是否攻击
        if (attCD >= 2)
        {
            attCD = 0;
            enemyAnima.PlayAttackAnim(Random.Range(0, 5)); //3/5的攻击概率
            Util.Instance.Delay(0.1f, () => enemyAnima.PlayAttackAnim(0));
        }
    }
    public void Hit(float damage) //被打
    {
        Damage(damage);
        if (hp > 0)
            enemyAnima.PlayHitAnim();
        else
            enemyAnima.PlayDeathAnim();
    }

    public void HitFly(Transform target, float force, float damage) //被击飞
    {
        hitFly = true;
        enemyAnima.PlayHitFlyAnim();
        Vector3 pos = new Vector3(target.position.x, transform.position.y, target.position.z);
        rig.AddForce((Vector3.up + (transform.position - pos).normalized) * force, ForceMode.Impulse);
        Damage(damage);
    }
    float maxHp; //满血血量
    float hp = 100; //当前血量
    Slider hpSlider; //血条
    void InitHpSlider() //初始化血条
    {
        hpSlider = Instantiate(Resources.Load<Slider>("Prefab/HPSlider"), GameObject.Find("EnemyHps").transform);
        maxHp = hp;
        hpSlider.value = hp / maxHp;
    }
    RaycastHit hit;
    void HideEnemyHpSlider() //隐藏敌人血条
    {
        Vector3 pos = transform.position + Vector3.up * 2.5f; //射线点
        Physics.Raycast(pos, cameraTH.transform.position - pos, out hit, 100);
        //当敌人进入视锥且未被遮挡时显示血条
        if (hpSlider != null)
        {
            if (hit.collider == cameraTH && onCamera)
                hpSlider.gameObject.SetActive(true);
            else
                hpSlider.gameObject.SetActive(false);
            hpSlider.transform.position = Camera.main.WorldToScreenPoint(pos); //血条跟随
        }
    }
    void Damage(float damage) //受伤
    {
        if (hp > damage)
            hp -= damage;
        else
        {
            hp = 0;
            Death();
            ShowKillCount();
        }
        if (hpSlider != null)
            hpSlider.value = hp / maxHp;
    }
    void Death() //死亡方法
    {
        if (!hitFly) //没有被击飞时才播放死亡动画
            enemyAnima.PlayDeathAnim();
        if (hpSlider != null)
            Destroy(hpSlider.gameObject);
    }
    public void DestoryBody(bool enab) //销毁尸体
    {
        GetComponent<CapsuleCollider>().enabled = enab;
    }
    void ShowKillCount() //显示击杀数
    {
        if (killText != null)
        {
            killText.text = "击杀 <color=yellow>" + ++killCount + "</color>";
            killText.GetComponent<Animator>().SetTrigger("ShowCount");
            killText = null;
        }
    }
    void OnCollisionEnter(Collision other) //开始碰撞
    {
        if (enemyAnima.animaState.IsName("hitFly"))
        {
            if (other.collider.tag == tag)
            {   //如果飞行中碰到同伴,将其撞到(播放倒地动画),被撞的人面向撞他的人
                Vector3 pos = transform.position;
                other.transform.LookAt(new Vector3(pos.x, other.transform.position.y, pos.z));
                other.collider.GetComponent<Enemy>().enemyAnima.PlayFallDownAnim();
            }
            if (other.collider.CompareTag("Ground") && hitFly)
            {
                rig.AddForce((Vector3.up + -transform.forward) * 6, ForceMode.Impulse);
                hitFly = false;
            }
        }
    }
    float FallDownTime; //躺地时间
    void OnTriggerStay(Collider other) //每帧触发
    {
        //被打飞落地后如果落地,1s后起身
        if (other.CompareTag("Ground"))
        {
            isGround = true;
            if (enemyAnima.animaState.IsName("hitFly"))
            {
                FallDownTime += Time.deltaTime;
                if (FallDownTime >= 1)
                {
                    FallDownTime = 0;
                    //没死起身,死了销毁尸体
                    if (hp > 0)
                    {
                        enemyAnima.PlayGieUpAnim();
                        hitFly = true;
                    }
                    else
                        DestoryBody(false);
                }
            }
        }
        //踩到同伴自行错开位置,防止重叠
        if (other.tag == tag)
        {
            if (isGround)
                transform.Translate(-Time.deltaTime, 0, -Time.deltaTime);
            else
                transform.Translate(Time.deltaTime, 0, Time.deltaTime);
        }
    }
    void OnTriggerExit(Collider other) //离开触发
    {
        if (other.CompareTag("Ground"))
        {
            isGround = false;
            FallDownTime = 0;
        }
    }


    //进入镜头显示血条,离开则隐藏
    bool onCamera; //是否进入摄像机
    void OnBecameVisible()
    {
        onCamera = true;
    }
    void OnBecameInvisible()
    {
        onCamera = false;
    }
}
