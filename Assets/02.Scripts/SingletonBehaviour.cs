using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : class
{
    private static T _instance = null;
    public static T Instance => _instance;

    protected virtual void Awake()
    {
        // T가 class 제약이라 _instance != null 은 Unity의 파괴 검사 없이 참조만 비교한다.
        // 씬 재시작으로 파괴된 이전 인스턴스를 "살아있다"고 오판하지 않도록 UnityEngine.Object로 비교한다.
        UnityEngine.Object existing = _instance as UnityEngine.Object;
        if (existing != null && existing != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
    }
}