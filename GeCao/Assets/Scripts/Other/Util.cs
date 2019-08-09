using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Util : MonoBehaviour
{
    private static Util _Instance = null;
    public static Util Instance
    {
        get
        {
            if (_Instance == null)
            {
                GameObject obj = new GameObject("Util"); //协程必须依据gameObject才能运行
                _Instance = obj.AddComponent<Util>();

            }
            return _Instance;
        }
    }
    IEnumerator OnDelay(float timer, UnityAction func)
    {
        yield return new WaitForSecondsRealtime(timer);
        func();
    }
    public void Delay(float delay, UnityAction func)
    {
        StartCoroutine(OnDelay(delay, func));
    }
}
