using UnityEngine;

public class Water
{
}

public class Cola
{
}

public class Cup<T>
{
    public T Drink; // 클래스를 선언할때 T의 자료형이 정해짐

    public Cup(T drink)
    {
        Drink = drink;
    }
}

public class GenericExample : MonoBehaviour
{
    // 제네릭(Generic : 일반화)은 타입(자료형)에 상관없는~
    // -> 이 자료형을 사용할 때 자료형이 무엇인지 정의하지 않고 그때그때 쓰는 것
    // -> Cup of Tea

    private Cup<int> _cup1 = new(17);
    private Cup<float> _cup2 = new(0.2f);
    private Cup<string> _cup3 = new("콜라");
    private Cup<bool> _cup4 = new(true);
    private Cup<Water> _cup5 = new(new Water());
    private Cup<Cola> _cup6 = new(new Cola());
}