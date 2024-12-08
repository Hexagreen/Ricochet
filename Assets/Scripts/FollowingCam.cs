using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCam : MonoBehaviour
{
    private GameObject stone;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 날아가는 돌을 추적
        transform.LookAt(stone.transform);
    }

    // 추적 대상을 설정
    public void FollowThisStone(GameObject stone) {
        this.stone = stone;
    }
}
