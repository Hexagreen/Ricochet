using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCam : MonoBehaviour
{
    public GameObject stonePrefab;
    private bool flipped;

    // Start is called before the first frame update
    void Start()
    {
        flipped = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 돌을 생성하고 던지기. 부모 객체를 해제하여 월드좌표를 이용.
    public void ThrowStone(float power, float rotation)
    {
        GameObject stone = Instantiate(stonePrefab, transform.GetChild(0).position, Quaternion.identity);
        stone.transform.SetParent(null);
        stone.GetComponent<Stone>().ShootStone(gameObject.transform.forward, power, rotation);
    }

    // 회전을 주는 방향에 따라 몸을 숙이는 방향 변경
    public void FlipCamera(float decision)
    {
        flipped = decision < 0;
    }

    // 조준 중일 때 점차적으로 몸을 숙이는 연출. 숙이는 만큼 돌이 생성되는 높이 달라짐.
    // 숙이는 범위는 아래 메서드에서 제어.
    public void ChargingCameraMoving(bool aiming)
    {
        Vector3 pos = transform.localPosition;
        int sign = aiming ? 1 : -1;
        pos.x = flipped ? InRangeMutator(pos.x, -0.4f, 0f, -sign) : InRangeMutator(pos.x, 0f, 0.4f, sign); ;
        pos.y = InRangeMutator(pos.y, 0.3f, 0.8f, -sign);
        pos.z = InRangeMutator(pos.z, -1f, 0f, -sign);
        transform.localPosition = pos;
    }

    // 수치를 특정 범위 내에서만 변동시키는 메서드
    private float InRangeMutator(float target, float min, float max, int sign)
    {
        target += (max - min) * sign * Time.deltaTime;
        if (target >= max) return max;
        else if (target <= min) return min;
        else return target;
    }
}
