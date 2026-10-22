using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

public class ListTest : MonoBehaviour
{
    // 자료구조 (Data Structure)
    // 이런 데이터들을 "효율적"으로 관리하기 위해 저장하는 구조방식
    // C : Create : 생성
    // R : Remove : 삭제
    // U : Update : 수정
    // D : Delete : 삭제
    // S : Search : 조회

    // BREAD

    // 선형 자료구조 : ex) 배열, 리스트, 큐, 스택 등등
    // 비선형 자료구조 : ex) 트리, 그래프, 해쉬테이블 등등

    private void Start()
    {
        int[] ages = new int[] { 22, 21, 20, 35 }; // 배열의 선언과 정리

        // 배열은 선언할 떄 지정한 배열의 크기를 변경할 수 없다.
        // -> 배열에 데이터를 추가로 추가하거나 삭제하려면 새로운 배열을 생성해야한다.
        // -> 배열은 데이터를 중간에 추가하거나 삭제하는 경우 비효율적인 과정이 필요하다
        // -> 그래서 이 배열의 단점을 보완하기 위해 자료구조는 동적 배열 혹은 연결 리스트를 사용하는데
        // -> C#은 이를 "콜렉션" 네임스페이스 안에 List로 구현해놨다
        // Collection : 컴퓨터 공학의 자료구조를 C#에서 구조화하여 클래스로 구현해 놓은 것들

        //List : 배열과 똑같은데 필요에 따라 크기가 동적으로 증감하는 배열

        var ages2 = new List<int> { 22, 21, 20, 35 }; // 리스트의 선언과 정의
        ages2[0] = 17;
        ages2[1] = 40;
        ages2[2] = 27;
        ages2[3] = 42;

        ages2.Add(24); // 데이터가 끝에 추가됨
        ages2.Insert(1, 12); // 데이터를 중간 특정지점에 삽입
        ages2.RemoveAt(3); // 제거할 데이터의 위치를 지정, 제거

        foreach (int age in ages2) Debug.Log(age); // foreach 를 통한 조회

        for (int i = 0; i < ages.Length; i++) Debug.Log(ages[i]); // 인덱스를 통한 조회
    }
}