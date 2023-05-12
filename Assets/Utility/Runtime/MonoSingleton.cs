/**
    @file   MonoSingleton.cs
    @date   2018.07.30
    @author 황준성(hns17.tistory.com)
*/
using UnityEngine;

/**
    @class  MonoSingleton
    @date   2018.07.30
    @author 황준성(hns17.tistory.com)
    @brief  MonoBehaviour를 싱글톤 형태로 생성한다.
*/
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance = null;
    public static T Instance {
        get {
            if(instance == null)
                instance = FindObjectOfType(typeof(T)) as T;

            if (instance == null) {
                Debug.Log(typeof(T).ToString());
                instance = new GameObject("@" + typeof(T).ToString(),
                                           typeof(T)).GetComponent<T>();
                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }
}