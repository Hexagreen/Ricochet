using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public GameObject impactParticle;
    public float maxRicochetAngle = 30.0f;
    public float rotationInfluence = 0.7f;
    private int bounced;
    private int goldenHit;
    private Manager manager;
    private Rigidbody rb;
    private AudioSource sound;
    private float waterBounciness;
    private float waterDrag;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        waterBounciness = GameObject.Find("WaterPlane").GetComponent<BoxCollider>().material.bounciness;
        waterDrag = 1 - waterBounciness;
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 32f;
        bounced = 0;
        goldenHit = 0;
        sound = GetComponent<AudioSource>();
    }

    // 기본 물리 엔진을 사용하지 않고 충돌과 리바운드를 표현하기 위해 사용
    void FixedUpdate()
    {   
        if(transform.position.y < 1.01) {   // 수면 충돌 시
            Vector3 waterNormal = new Vector3(0f, 1f, 0f);  // 수면 법선
            float impactAngle = Vector3.Angle(rb.velocity, waterNormal);    // 충돌 각

            // 충돌 시 속도와 회전력이 충분하고 입사 각도가 적절할 때
            if(impactAngle > 90 && impactAngle < maxRicochetAngle + 90 && rb.angularVelocity.magnitude > 5f) {
                // 입사 벡터의 반사 벡터
                Vector3 reflectVec = Vector3.Reflect(rb.velocity, waterNormal);
                // 각속도와 반사 벡터의 외적 -> 좌우로 향하는 벡터 -> 회전에 의한 경로 휨
                Vector3 rotationalForce = Vector3.Cross(rb.angularVelocity.normalized, reflectVec.normalized) * rotationInfluence;
                reflectVec += rotationalForce;  // 경로 휨 벡터 추가
                // 각속도에 따라 수직으로 튕겨나가는 추가 힘. 경로 휨 벡터 계산 시 손실된 에너지가 추진력이 된다고 가정
                reflectVec += (1 - rotationInfluence) * waterDrag * rb.angularVelocity.magnitude * Vector3.up; 
                reflectVec *= waterBounciness;  // 수면의 탄성 손실
                rb.velocity = reflectVec;
                rb.angularVelocity *= waterBounciness;   // 수면 저항에 따른 각속도 손실
            }
            // 충돌력이 부족하거나 입사각이 범위를 벗어났을 때 돌이 물에 빠짐
            else {
                rb.velocity *= 0.5f;
                rb.angularVelocity *= 0;
            }
        }
    }

    // 기본 물리 엔진을 통해 충돌을 구현하지 않았기 때문에 충돌 판정 대신 트리거 판정 사용
    void OnTriggerEnter(Collider collider) {
        // 물에 닿았을 경우 점수 추가, 2번 이상 튕길 때 카메라 변경, 착수 효과음 재생, 파티클 생성
        if(collider.gameObject.CompareTag("Water")) {
            manager.PostPoint(bounced++);
            if(bounced == 2) manager.StoneShot(gameObject);
            sound.Play();
            GameObject particle = Instantiate(impactParticle, transform.position, Quaternion.identity);
            Destroy(particle, 1);
        }
        // 추가 점수 공에 닿았을 경우, 현재 튕겼던 횟수 만큼 추가 점수 => 여러번 튕기고 맞혔을 때 어드밴티지
        // 이후 추가 점수 공 삭제
        if(collider.gameObject.CompareTag("Scoreball")) {
            manager.PostPoint(bounced);
            manager.GoldenHit(++goldenHit);
            collider.gameObject.GetComponent<GoldenBall>().RemoveBall();
        }
    }

    // 지형과 충돌 시 => 더이상 튕길 수 없을 때
    void OnCollisionEnter(Collision collision) {
        // 물리엔진 부하를 줄이기 위해 속도를 지우고 3초 지연 후에 돌 삭제
        // 삭제 전에 돌이 사라졌다는 신호를 보내서 플레이 편의성 확보
        if(collision.gameObject.CompareTag("Terrain")) {
            rb.velocity *= 0;
            rb.angularVelocity *= 0;
            manager.StoneRemoved();
            Destroy(gameObject, 3);
        }
    }

    void Update() {
        
    }

    // 돌 던지기 메서드. 일인칭 카메라의 시선 방향을 받고 전달 받은 힘과 회전력 부여
    public void ShootStone(Vector3 direction, float power, float spin) {
        GetComponent<Rigidbody>().AddForce(direction * (100 + 400 * (power / 64f)));
        GetComponent<Rigidbody>().angularVelocity = new Vector3(0, spin, 0);
    }
}
