using UnityEngine;
using System.Collections;
using System.Threading;

public class CoroutineTest : MonoBehaviour
{
    // 유니티의 코루틴이란
    // 비동기 실행과 제어를 효율적으로 제공하는 기능임
    // 1. 비동기 실행
    // 2. 시간에 따른 실행 제어

    // -IEnumerator : 코루틴 함수의 반환형
    // -StartCoroutine(함수()) : 유니티에서 코루틴 함수를 실행하는 방법
    // -yield : 정해진 시간/프레임 동안 쉼
    private void Start()
    {
        Debug.Log("시작!");
        //Countdown 이라는 메서드를 비동기 방식으로 실행
        StartCoroutine(Countdown_Coroutine());

        // 그러므로 아래라인의 작업은 위에서 비동기 방식으로 실행한 코드를 기다릴 필요가 없음
        Debug.Log("끝!");
    }

    private IEnumerator Countdown_Coroutine()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("1초!");

        yield return new WaitForSeconds(1f);
        Debug.Log("2초!");

        yield return new WaitForSeconds(1f);
        Debug.Log("3초!");

        yield return new WaitForSeconds(1f);
        Debug.Log("나로호 발사!");
    }
}