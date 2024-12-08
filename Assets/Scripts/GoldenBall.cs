using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenBall : MonoBehaviour
{
    private ParticleSystem particle;
    private MeshRenderer rend;
    private AudioSource sound;
    private int axle;
    public float areaXmin = -110f;
    public float areaXmax = -25f;
    public float areaZmin = -60f;
    public float areaZmax = -10f;
    public float velocity = 20f;

    // Start is called before the first frame update
    void Start()
    {
        // 생성 직후 부모 객체 연결 해제. 월드 좌표를 이용하게 됨.
        transform.SetParent(null);
        particle = GetComponent<ParticleSystem>();
        rend = GetComponent<MeshRenderer>();
        sound = GetComponent<AudioSource>();
        transform.position = new Vector3(Random.Range(areaXmin, areaXmax), 1, Random.Range(areaZmin, areaZmax));
        axle = Random.Range(0, 4);
    }

    // 무작위로 설정된 축에 따라 x축, z축, 두 대각 방향으로 공이 움직임.
    // 물리 엔진 업데이트 위치에서 처리해야 끼이는 현상을 방지할 수 있음.
    void FixedUpdate()
    {
        switch(axle) {
            case 0: Movement(1, 0); break;
            case 1: Movement(0, 1); break;
            case 2: Movement(1, 1); break;
            case 3: Movement(1, -1); break;
        }
    }

    // 공이 사라질 때의 처리. 소리, 파티클 생성 후 삭제
    public void RemoveBall() {
        rend.enabled = false;
        sound.Play();
        particle.Play();
        Destroy(gameObject, 0.5f);
    }

    // 공이 지정된 범위 내에서만 축을 따라 계속 움직이도록 함.
    // 범위 초과 시 방향 반전
    private void Movement(int xAxle, int zAxle) {
        Vector3 pos = transform.position;
        if(pos.x > areaXmax || pos.x < areaXmin || pos.z > areaZmax || pos.z < areaZmin) {
            velocity *= -1;
        }
        transform.Translate(0.01f * velocity * new Vector3(xAxle, 0, zAxle));
    }
}
