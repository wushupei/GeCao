using UnityEngine.UI;
using UnityEngine;
public class PlayerData
{
    private static PlayerData _Instanc;
    public static PlayerData Instanc //单例
    {
        get
        {
            if (_Instanc == null)
                _Instanc = new PlayerData();
            return _Instanc;
        }
    }
    public float maxHp = 100; //最大血量
    public float hp = 100; //当前血量
    public void SubHp(float _hp, Slider slider) //减血
    {
        if (hp > _hp)
            hp -= _hp;
        else
            hp = 0;
        slider.value = hp / maxHp; //显示到界面
    }

    public float maxGas = 100; //最大真气
    public float gas = 0; //当前真气
    public void AddGas(float _gas, Slider slider) //加真气
    {
        if (gas + _gas < maxGas)
            gas += _gas;
        else
            gas = maxGas;
        slider.value = gas / maxGas;
    }
    public void SubGas(float _gas, Slider slider) //减真气
    {
        gas -= _gas;
        slider.value = gas / maxGas;
    }

    public float maxAnger = 100; //最大怒气
    public float anger = 0; //当前怒气
    public void AddAnger(float _anger, Slider slider) //加怒气
    {
        if (anger + _anger < maxAnger)
            anger += _anger;
        else
            anger = maxAnger;
        slider.value = anger / maxAnger;
    }
    public void SubAnger(Slider slider) //减怒气
    {
        anger = 0;
        slider.value = anger / maxAnger;
    }
}
