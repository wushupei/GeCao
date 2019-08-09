using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    [SerializeField]
    private Slider hpSlider, gasSlider, angerSlider; //状态条
    [SerializeField]
    private Animator hpAnima, angerAnima; //状态条动画

    [HideInInspector]
    public PlayerAnima playerAnima;
    [SerializeField]
    private float speed; 
    [SerializeField]
    private Transform cameraTh; //摄像机  
    [SerializeField]
    private Transform muzzle; //枪口
    [SerializeField]
    private GameObject bullet; //子弹
    [SerializeField]
    private float gasCost; //技能消耗
    [SerializeField]
    private float commonAttack; //普通攻击力
    [SerializeField]
    private float heavyAttack; //重击攻击力
    [SerializeField]
    private float skillAttack; //技能攻击力
    [SerializeField]
    private float bigSkillAttack; //大技能攻击力

    public GameObject runTrail; //移动轨迹
    public GameObject attackTrail; //刀光轨迹
    public ParticleSystem attack3_1, attack3_2; //攻击特效   
    public ParticleSystem bigSkill1, bigSkill2, bigSkill3; //大技能特效
    [SerializeField]
    private GameObject fireImage;

    CharacterController cc;
    [HideInInspector]
    public bool superArmor; //霸体状态

    bool skill = true; //是否释放小技能
    void Start()
    {
        playerAnima = GetComponentInChildren<PlayerAnima>();
        playerAnima.player = this;
        cc = GetComponent<CharacterController>();
        InitState();
    }
    void Update()
    {
        if (MainGame.Instanc.second <= 0)
        {
            playerAnima.PlayRunAnima(false);
            return;
        }
        if (PlayerData.Instanc.hp <= 0)
            return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        //在地面且非霸体状态才能移动攻击
        if (!playerAnima.animaState.IsName("hit") && !superArmor)
            Attack();
        Run(x, z);
        BigSkill();
        Skill();
        NearDeath();
        Guide();

    }
    float v = 0;
    void Run(float x, float z) //移动
    {
        cc.Move(Physics.gravity * Time.deltaTime);
        //播放跑步动画
        playerAnima.PlayRunAnima(x != 0 || z != 0);
        //摄像机指向方向,进行归一化消除加速度
        Vector3 dir = cameraTh.forward * z + cameraTh.right * x;
        //重力
        // dir.y -= Time.deltaTime;
        //只有在跑步动画时才能移动,显示移动轨迹
        if (playerAnima.animaState.IsName("run"))
        {
            runTrail.SetActive(true);
            cc.Move(dir.normalized * speed * Time.deltaTime);
            //使用transform进行移动时使用
            //前进方向为摄像机指向方向(摄像机本地方向转世界) 
            //Vector3 runDir = transform.InverseTransformDirection(dir);
            //transform.Translate(runDir * Time.deltaTime * speed); 
        }
        else
            runTrail.SetActive(false);
        //面向移动方向
        if (dir != Vector3.zero && !superArmor)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10);
    }

    private void Attack() //播放攻击动画
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            attackTrail.SetActive(true);
            playerAnima.PlayAttackAnima();
        }
    }
    [SerializeField]
    private GameObject skillGuide, bigSkillGuide;
    public void Guide() //新手引导
    {
        //小技能第一次达到发射条件显示引导
        if (PlayerData.Instanc.gas >= gasCost && skillGuide != null)
            skillGuide.SetActive(true);
        //大技能第一次达到发射条件显示引导
        if (bigSkillGuide != null)
        {
            if (PlayerData.Instanc.anger >= PlayerData.Instanc.maxAnger)
                bigSkillGuide.SetActive(true);
            else if (bigSkillGuide.activeInHierarchy)
                Destroy(bigSkillGuide);
        }
    }
    public void Skill() //释放小技能
    {
        if (Input.GetKeyDown(KeyCode.N) && skill && playerAnima.doubleHit == 0)
        {
            skill = false;
            Util.Instance.Delay(1, () => skill = true);
            if (PlayerData.Instanc.gas > gasCost) //真气足够释放技能
            {
                PlayerData.Instanc.SubGas(gasCost, gasSlider);
                playerAnima.PlaySkillAnima();
                if (skillGuide != null)
                    Destroy(skillGuide);
            }
        }

    }
    public void ShootSkill() //发射技能
    {
        Instantiate(bullet, muzzle.position, muzzle.rotation).AddComponent<Bullet>().Init(this, skillAttack);
    }
    private void BigSkill() //释放大技能
    {
        if (PlayerData.Instanc.anger == PlayerData.Instanc.maxAnger)
        {
            fireImage.SetActive(true);
            angerAnima.enabled = true;
            if (Input.GetKeyDown(KeyCode.U))
            {

                Time.timeScale = 0.1f;
                Util.Instance.Delay(1, () => Time.timeScale = 1);
                superArmor = true;
                bigSkill1.Play();
                playerAnima.PlayBigSkillAnima();
                PlayerData.Instanc.SubAnger(angerSlider);
            }
        }
        else
        {
            fireImage.SetActive(false);
            angerAnima.enabled = false;
        }
    }
    public void Hit(Enemy enemy) //被打
    {
        if (PlayerData.Instanc.hp > 0)
        {
            skill = true; //刷新小技能
            //面向攻击者
            Vector3 pos = new Vector3(enemy.transform.position.x, transform.position.y, enemy.transform.position.z);
            transform.rotation = Quaternion.LookRotation(pos - transform.position);
            //播放被打动画
            playerAnima.PlayHitAnima();
        }
    }
    public void Damage(float damage) //扣血
    {
        //减血并同步显示在界面
        PlayerData.Instanc.SubHp(damage, hpSlider);
        //增加怒气同步显示在界面
        PlayerData.Instanc.AddAnger(damage * 10, angerSlider);
        //没血了执行死亡方法
        if (PlayerData.Instanc.hp <= 0)
            Death();
    }
    void Death() //死亡
    {
        playerAnima.PlayDeathAnima(); //播放死亡动画
    }
    void NearDeath() //濒死状态
    {
        if (PlayerData.Instanc.hp / PlayerData.Instanc.maxHp < 0.2f)
        {
            PlayerData.Instanc.AddGas(Time.deltaTime * 10, gasSlider);
            PlayerData.Instanc.AddAnger(Time.deltaTime * 10, angerSlider);
            hpAnima.enabled = true;
        }
        else
            hpAnima.enabled = false;
    }
    //扇形射线检测敌人
    public void _RayCheckEnemy(Action<Collider> action, float _attRange, float _angle = 360)
    {
        //根据半径(攻击长度)获取周长,如果发射角度<360则获取弧长
        float length = _attRange * 2 * Mathf.PI / (360 / _angle);
        //长度除以检测物体的碰撞器直径得到所需射线数(这里物体宽度为1,所以不用再除)
        int rayCount = (int)length;
        float space = _angle / rayCount; //间隔角度

        List<Collider> enemys = new List<Collider>();
        //从右往左逆时针发射射线(扇形射线增加一根射线)
        for (int i = 0; i < rayCount + Convert.ToInt32(_angle != 360); i++)
        {
            Vector3 dir = Quaternion.AngleAxis(_angle / 2 - space * i, Vector3.up) * transform.forward;
            RaycastHit[] hit = Physics.RaycastAll(transform.position + Vector3.up, dir, _attRange, LayerMask.GetMask("Enemy"));
            foreach (var item in hit)
            {
                if (!enemys.Contains(item.collider))
                {
                    enemys.Add(item.collider);
                    action(item.collider); //具体攻击效果
                }
            }
        }
    }
    public void AttackEnemy(Collider item) //攻击敌人(回调函数)
    {
        //大技能造成更多伤害
        float damage = superArmor ? bigSkillAttack : commonAttack;
        item.GetComponent<Enemy>().Hit(damage);
        PlayerData.Instanc.AddGas(damage / 50, gasSlider);//增加真气
    }
    public void HitFlyEnemy(Collider item) //击飞敌人(回调函数)
    {
        //大技能的击飞力度不同于普通击飞,伤害也不同
        float force = superArmor ? 15 : 10;
        float damage = superArmor ? bigSkillAttack : heavyAttack;
        item.GetComponent<Enemy>().HitFly(transform, force, damage);
        PlayerData.Instanc.AddGas(damage / 100, gasSlider);
    }
    void InitState() //初始化状态界面
    {
        hpSlider.value = PlayerData.Instanc.hp / PlayerData.Instanc.maxHp;
        gasSlider.value = PlayerData.Instanc.gas / PlayerData.Instanc.maxGas;
        angerSlider.value = PlayerData.Instanc.anger / PlayerData.Instanc.maxAnger;
    }
}
