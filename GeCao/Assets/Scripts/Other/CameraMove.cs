using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    Player player;
    float angle = 180; //摄像机角度
    void Start()
    {
        player = FindObjectOfType<Player>();
    }
    void LateUpdate()
    {
        if (MainGame.Instanc.second <= 0)
            return;

        if (Time.timeScale != 1) //时间缩放时(放大招),快速围绕主角旋转
            angle += Time.deltaTime * 2750;        
        if (PlayerData.Instanc.hp == 0) //主角死了围绕主角旋转
            angle += Time.deltaTime * 20;
        FollowPlayer(Input.GetAxis("Horizontal") * Time.deltaTime * 90); //主角左右移动时跟随旋转
    }
    void FollowPlayer(float x)
    {
        //确定摄像机位置       
        transform.position = player.transform.position + Vector3.up * 4 + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * 6;
        //主角移动时旋转
        if (player.playerAnima.animaState.IsName("run"))
            angle += x;
        //摄像机需要看向的点(主角头上)
        Vector3 cameraLookPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        //摄像机看向点的方向
        transform.rotation = Quaternion.LookRotation(cameraLookPos - transform.position);
        
        //手动控制摄像机旋转
        if (Time.timeScale == 1)
        {
            if (Input.GetKey(KeyCode.F))
                angle += Time.deltaTime * 180;
            if (Input.GetKey(KeyCode.H))
                angle -= Time.deltaTime * 180;
        }
    }
}
