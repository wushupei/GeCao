using UnityEngine;
using UnityEngine.UI;

public class MainGame : MonoBehaviour
{
    public float second; //秒
    [SerializeField]
    private Text timeText;
    public static MainGame Instanc;
    void Start()
    {
        Instanc = this;
    }
    void Update()
    {
        CountDown();
    }
    void CountDown() //倒计时
    {
        if (second > 0)
        {
            timeText.text = (int)second / 60 + ":" + ((int)second % 60).ToString("00");
            second -= Time.deltaTime;
            if (second < 10)
                timeText.color = Color.red;
        }
    }
}
