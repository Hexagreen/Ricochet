using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCam : MonoBehaviour
{
    private static bool initiated = false;
    private static Quaternion initAngle = Quaternion.identity;
    private float delta = 0.1f;
    private Vector3 angle;

    // Start is called before the first frame update
    void Start()
    {
        // 기본 각도를 무작위로 하여 단조로움 방지
        // 정적 변수를 이용해 장면 리로드 시 끊김 현상 방지
        if (!initiated)
        {
            transform.localRotation = Quaternion.Euler(Random.Range(15f, 40f), Random.Range(130f, 260f), 0);
            initiated = true;
        }
        else
        {
            transform.localRotation = initAngle;
        }
    }

    // Update is called once per frame
    // 특정 범위 내에서 카메라가 월드 전경을 살피게 함.
    void Update()
    {
        angle = transform.localEulerAngles;
        if (angle.y < 120) delta = 0.1f;
        if (angle.y > 290) delta = -0.1f;
        transform.localRotation = Quaternion.Euler(angle.x, angle.y + delta, angle.z);
        initAngle = transform.localRotation;
    }
}
