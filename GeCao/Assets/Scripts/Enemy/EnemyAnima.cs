using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnima : MonoBehaviour
{
    [HideInInspector]
    public AnimatorStateInfo animaState; //动画状态
    Animator anim;
    [HideInInspector]
    public Enemy enemy; //敌人类
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }
    void Update()
    {
        //获取动画状态
        animaState = anim.GetCurrentAnimatorStateInfo(0);
    }
    public void SwitchAnimaForDis(float dis) //根据距离切换动画状态
    {
        anim.SetFloat("State", dis);
    }
    public void PlayAttackAnim(int attNum) //攻击动画,不同参数播放不同攻击动画
    {
        anim.SetInteger("Attack", attNum);
    }
    public void PlayHitAnim() //被打动画
    {
        anim.SetTrigger("Hit");
    }
    public void PlayHitFlyAnim() //被打飞动画
    {
        anim.SetTrigger("HitFly");
    }
    public void PlayFallDownAnim() //倒地动画
    {
        anim.SetTrigger("FallDown");
        Util.Instance.Delay(1, () => anim.SetTrigger("GitUp"));
    }
    public void PlayGieUpAnim() //起身动画
    {
        anim.SetTrigger("GitUp");
    }
    public void PlayDeathAnim() //死亡动画
    {
        anim.SetTrigger("Death");
    }
    public void Attack() //动画事件_普通攻击
    {
        if (enemy.player.superArmor)
            return;

        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, 3.5f, LayerMask.GetMask("Player")))
        {
            float att = enemy.attack;
            if (animaState.IsName("attack3")) //如果是重击则播放被打动画,伤害*2
            {
                enemy.player.Hit(enemy);
                att *= 2;
            }
            enemy.player.Damage(att);
            enemy.bloodEffect.Play();
        }
    }
    public void Death() //动画事件_死亡
    {
        enemy.DestoryBody(false);
    }

}
