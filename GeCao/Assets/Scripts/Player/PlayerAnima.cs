using System.Collections.Generic;
using UnityEngine;

public class PlayerAnima : MonoBehaviour
{
    Animator anim;
    [HideInInspector]
    public AnimatorStateInfo animaState; //动画状态
    [SerializeField]
    private float attRange; //攻击范围
    [SerializeField]
    private float angle; //扇形射线角度范围
    [SerializeField]
    private float attRange1; //攻击范围
    [HideInInspector]
    public Player player;
    [HideInInspector]
    public int doubleHit; //连击计数
    void Start()
    {
        anim = GetComponent<Animator>();
    }
    void Update()
    {
        //待机动画和跑步动画以外的动画播放完后自动返回待机动画
        animaState = anim.GetCurrentAnimatorStateInfo(0);
        if (!animaState.IsName("idle") && !animaState.IsName("run") && animaState.normalizedTime > 1.0f)
        {
            doubleHit = 0;
            anim.SetInteger("AttNumber", doubleHit);
            player.attackTrail.SetActive(false);
        }
        //关闭攻击特效
        if (!animaState.IsName("attack3"))
            player.attack3_1.Stop();
        ShowCheckRange(Color.green, attRange, angle);
        ShowCheckRange(Color.red, attRange1);
    }

    public void PlayRunAnima(bool run) //跑动动画
    {
        anim.SetBool("Run", run);
    }
    public void PlayJumpAnima(bool jump) //跳跃动画
    {
        anim.SetBool("Jump", jump);
    }
    public void PlayHitAnima() //被打动画
    {
        anim.SetTrigger("Hit");
    }
    public void PlayDeathAnima() //死亡动画
    {
        anim.SetTrigger("Death");
    }
    public void PlayAttackAnima() //连击动画
    {
        switch (doubleHit)
        {
            case 0:
                anim.SetInteger("AttNumber", ++doubleHit);
                break;
            case 1:
                if (animaState.IsName("attack1") && animaState.normalizedTime > 0.6f && animaState.normalizedTime < 0.9f)
                    anim.SetInteger("AttNumber", ++doubleHit);
                break;
            case 2:
                if (animaState.IsName("attack2") && animaState.normalizedTime > 0.6f && animaState.normalizedTime < 0.9f)
                    anim.SetInteger("AttNumber", ++doubleHit);
                break;
        }
    }
    public void PlaySkillAnima() //技能动画
    {
        anim.SetTrigger("Skill");
    }
    public void PlayBigSkillAnima() //大技能动画
    {
        anim.SetTrigger("BigSkill");
    }

    public void JumpStart() //动画事件_起跳
    {
        //player.Jump();
    }
    public void CommonHit() //动画事件_普通攻击
    {
        player._RayCheckEnemy(player.AttackEnemy, attRange, angle);
    }
    public void HitFlyEnter() //动画事件_击飞1
    {
        player.attack3_1.Play(); //火焰特效
    }
    public void HitFlyStay() //动画事件_击飞2
    {
        player._RayCheckEnemy(player.AttackEnemy, attRange); //圆形普攻
    }
    public void HitFlyExit() //动画事件_击飞3
    {
        Transform tf = player.attack3_2.transform;
        Instantiate(player.attack3_2, tf.position, tf.rotation).Play();
        player._RayCheckEnemy(player.HitFlyEnemy, attRange, angle); //扇形击飞
        player.attack3_1.Stop();
    }
    public void Skill() //动画事件_技能
    {
        player.ShootSkill();
    }
    public void BigSkill1() //动画事件_大技能1
    {
        player.bigSkill2.Play();
        player._RayCheckEnemy(player.AttackEnemy, attRange1);
    }
    public void BigSkill2() //动画事件_大技能2
    {
        player.bigSkill3.Play();
        player._RayCheckEnemy(player.HitFlyEnemy, attRange1);
        Util.Instance.Delay(1, () => player.superArmor = false);
    }
    public void ShowCheckRange(Color _color, float _attRange, float _angle = 360)
    {
        if (_angle > 360)
            _angle = 360;
        //根据半径(攻击长度)获取周长,如果发射角度<360则获取弧长
        float length = _attRange * 2 * Mathf.PI / (360 / _angle);

        int rayCount = (int)length; //物体宽度为1,得到所需射线数
        float space = _angle / rayCount;//间隔角度
        Vector3 ori = transform.position + Vector3.up;
        //从右往左逆时针发射射线(扇形射线增加一根射线)
        for (int i = 0; i < rayCount + System.Convert.ToInt32(_angle != 360); i++)
        {
            Vector3 dir = Quaternion.AngleAxis(_angle / 2 - space * i, Vector3.up) * transform.forward;
            Debug.DrawLine(ori, ori + dir * _attRange, _color);
        }
    }
}
