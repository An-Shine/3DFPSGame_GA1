using System.Collections.Generic;
using UnityEngine;

public class DictionaryTest : MonoBehaviour
{
    // List 는 인덱스를 이용해 데이터에 접근했지만
    // 딕셔너리는 요소를 저장할때 "키" 와 "값" 쌍의 형태로 저장한다
    // HashTable(= DIctionary)는 키와 값이 쌍으로 이루어진 데이터를 관리할 떄 사용하는 자료구조

    private void Start()
    {
        var ages = new List<int> { 22, 21, 20, 35 };

        var students = new Dictionary<string, int>
            { { "오나현", 22 }, { "박철규", 21 }, { "최재원", 20 } };

        Debug.Log(students["오나현"]);
        students["박철규"] = 23;
        students.Add("송재욱", 17);
        students.Remove("최재원");
        bool has = students.ContainsKey("송재욱");

        foreach (KeyValuePair<string, int> item in students) Debug.Log($"{item.Key}의 나이 : {item.Value}");
    }
}