using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Manager : MonoBehaviour
{
    public GameObject powerBar;
    public GameObject spinBar;
    public RectTransform spinBarRect;
    public GameObject lobbyCam;
    public GameObject fpsCam;
    public GameObject stoneViewCam;
    public GameObject lobbyUI;
    public GameObject playUI;
    public GameObject player;
    public TextMeshProUGUI pointText;
    public TextMeshProUGUI pointAddictionText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI messageBoardText;
    public FPCam fpCam;
    public GameObject goldenBallPrefab;
    public int goldenBallCounts = 5;
    private float spin;
    private float power;
    private bool powerHitMax;
    private bool aiming;
    private bool flying;
    private bool ingame;
    private bool tutorialOpen;
    private int point;
    private int bullet;
    private static int highScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        spin = 0f;
        power = 0f;
        powerHitMax = false;
        aiming = false;
        flying = false;
        ingame = false;
        tutorialOpen = false;
        point = 0;
        bullet = 5;
    }

    // Update is called once per frame
    void Update()
    {
        // 일인칭 모드에서만 작동
        if (fpsCam.activeSelf)
        {
            // 점수 갱신
            pointText.text = "Score: " + point;
            highScoreText.text = "High Score: " + highScore;
            // 날아가는 돌이 없을 때
            if (!flying)
            {
                // 잔여 탄약이 있으면
                if (bullet > 0)
                {
                    // 회전 방향과 회전력 설정
                    // 컨트롤 키는 왼쪽 회전, 알트 키는 오른쪽 회전
                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        spin = spin < -32f ? spin : spin - 1f;
                    }
                    if (Input.GetKey(KeyCode.LeftAlt))
                    {
                        spin = spin > 32f ? spin : spin + 1f;
                    }
                    // 스페이스 바를 누르고 있을 때 던지는 힘이 늘었다 줄었다 함
                    if (Input.GetKey(KeyCode.Space))
                    {
                        power = powerHitMax ? power - 1f : power + 1f;
                        if (power >= 64f) powerHitMax = true;
                        if (power <= 0f) powerHitMax = false;
                    }
                    // 스페이스 바를 놓으면 돌을 생성하고 던지도록 함
                    // 동시에 돌이 발사되지 않도록 비행중 논리값을 설정하고 탄약 감소
                    if (Input.GetKeyUp(KeyCode.Space))
                    {
                        flying = true;
                        fpCam.ThrowStone(power, spin);
                        power = 0f;
                        spin = 0f;
                        bullet--;
                    }
                    // 마우스 우클릭 시 조준모드 논리값 설정. 이 값을 참조하여 몸통 숙이기 메서드 작동
                    if (Input.GetMouseButton(1)) aiming = true;
                }
                // 날아가는 돌이 없을 때만 ESC를 눌러 정지 화면으로 나갈 수 있음
                // 메뉴 호출 시 일인칭 마우스 잠금 해제
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    fpsCam.GetComponent<FirstPersonController>().SetMouseLock(false);
                    SwapUI(false);
                }
            }
            // 발사 후에 조준 모드를 해제해도 몸이 바로 세워짐
            if (!Input.GetMouseButton(1)) aiming = false;
            // 회전력과 힘을 UI 바에 반영하여 시각적 표현
            spinBar.GetComponent<Slider>().value = Math.Abs(spin) / 32f;
            powerBar.GetComponent<Slider>().value = power / 64f;
            // 몸을 숙이는 동작 처리
            fpCam.FlipCamera(spin);
            fpCam.ChargingCameraMoving(aiming);

            // 회전 방향에 따라 회전력 바의 방향 조정
            if (spin < 0)
            {
                spinBarRect.offsetMin = new Vector2(0, 0);
                spinBarRect.offsetMax = new Vector2(-240, 0);
                spinBarRect.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                spinBarRect.offsetMin = new Vector2(240, 0);
                spinBarRect.offsetMax = new Vector2(0, 0);
                spinBarRect.localScale = new Vector3(1, 1, 1);
            }
            // 탄을 전부 소진하면 플레이어가 점프할 수 있게 되고, 돌 던지기와 관련된 UI 제거
            // 탄 소진 후 한 번만 호출되도록 탄을 음수로 변경
            if (bullet == 0)
            {
                player.GetComponent<FirstPersonController>().gameEnd = true;
                powerBar.SetActive(false);
                spinBar.SetActive(false);
                bullet--;
            }
        }
    }

    // 점수 변경 시 호출
    public void PostPoint(int p)
    {
        point += p;
        // 점수 추가 시 효과 부여
        ApplyPointAddiction(p);
        highScore = highScore < point ? point : highScore;
    }

    // 돌이 던져지고 튕기기 시작하면 호출
    public void StoneShot(GameObject stone)
    {
        // UI를 지우고 돌을 따라가는 카메라를 킴
        powerBar.SetActive(false);
        spinBar.SetActive(false);
        stoneViewCam.SetActive(true);
        stoneViewCam.GetComponent<FollowingCam>().FollowThisStone(stone);
    }

    // 돌이 멈추고 나서 호출
    public void StoneRemoved()
    {
        // 메시지 보드 초기화
        PostMessageBoard("");
        // 마지막 기회일 때 메시지로 알림
        if (bullet == 1)
        {
            PostMessageBoard("마지막 기회!");
        }
        // 탄이 없으면 UI를 부활시키지 않음
        if (bullet > 0)
        {
            powerBar.SetActive(true);
            spinBar.SetActive(true);
        }
        // 돌 추적 캠을 끄고 점수 변경 효과를 제거, 돌을 다시 날릴 수 있게 비행중 토글
        stoneViewCam.SetActive(false);
        Invoke(nameof(RemovePointAddiction), 1);
        flying = false;
    }

    // 메인 UI에서 플레이 버튼 클릭 시 호출
    public void PlayButtonClicked()
    {
        // 일시정지 화면일 때
        if (ingame)
        {
            // 장면 리로드
            SceneManager.LoadScene(0);
        }
        // 튜토리얼을 열지 않았을 때만 작동 == 플레이 버튼이 가려지지 않았을 때
        if (!ingame && !tutorialOpen)
        {
            // 일시정지 화면을 위해 버튼 내용 변경
            lobbyUI.transform.Find("PlayButton").transform.Find("PlayText").GetComponent<TextMeshProUGUI>().text = "Exit";
            lobbyUI.transform.Find("HowToButton").transform.Find("HowToText").GetComponent<TextMeshProUGUI>().text = "Return to Game";
            SwapUI(true);
            // 추가 점수 볼을 배치
            for (int i = goldenBallCounts; i > 0; i--)
            {
                Instantiate(goldenBallPrefab);
            }
            // 게임 진행 중 표시 올리기
            ingame = true;
        }
    }

    // 튜토리얼 버튼을 눌렀을 때 호출
    public void HowToButtonClicked()
    {
        // 일시정지 화면일 때
        if (ingame)
        {
            // 일시정지 해제. 마우스 제어권 반환.
            fpsCam.GetComponent<FirstPersonController>().SetMouseLock(true);
            SwapUI(true);
            return;
        }
        // 튜토리얼이 닫혀 있었다면
        if (!tutorialOpen)
        {
            // 튜토리얼 이미지를 뿌리고 튜토리얼 닫기 텍스트로 변경
            lobbyUI.transform.Find("HowToButton").transform.Find("HowToText").GetComponent<TextMeshProUGUI>().text = "Click Here to Return";
            lobbyUI.transform.Find("HowToImage").GetComponent<RawImage>().enabled = true;
            tutorialOpen = true;
        }
        // 튜토리얼이 열려 있었다면
        else
        {
            // 튜토리얼 이미지를 지우고 원래 상태로 복귀
            lobbyUI.transform.Find("HowToButton").transform.Find("HowToText").GetComponent<TextMeshProUGUI>().text = "How to Play";
            lobbyUI.transform.Find("HowToImage").GetComponent<RawImage>().enabled = false;
            tutorialOpen = false;
        }
    }

    // 추가 점수를 받을 때 메시지 표시
    public void GoldenHit(int count) {
        switch(count) {
            case 0: break;
            case 1: PostMessageBoard("보너스!"); break;
            case 2: PostMessageBoard("일석이조!"); break;
            case 3: PostMessageBoard("일석삼조?!"); break;
            case 4: PostMessageBoard("Wow!"); break;
            case 5: PostMessageBoard("물수제비의 신"); break;
        }
    }

    // 로비 -> 일인칭 혹은 일인칭 -> 일시정지 화면으로 변경 시 호출
    private void SwapUI(bool playMode)
    {
        lobbyUI.SetActive(!playMode);
        lobbyCam.SetActive(!playMode);
        playUI.SetActive(playMode);
        fpsCam.SetActive(playMode);
    }

    // 점수 추가 시 마다 호출. 효과 부여
    private void ApplyPointAddiction(int p)
    {
        // 추가된 점수가 0점이 아닐 때
        if (p != 0)
        {
            // 녹색으로 글씨 변경, 부여된 점수 표시
            // 잠시 지연 후에 효과 제거
            pointAddictionText.color = Color.green;
            pointAddictionText.text = "+" + p;
            Invoke(nameof(RecolorPointAddiction), 0.2f);
        }
    }

    // 바로 위에서 효과 제거 시 호출
    private void RecolorPointAddiction()
    {
        // 글씨 색 복귀
        pointAddictionText.color = Color.white;
    }

    // 추가 점수 글씨 지울 때 호출
    private void RemovePointAddiction()
    {
        pointAddictionText.text = "";
    }

    // 메시지 보드 갱신 메서드
    // 지연을 사용하여 색 강조 표현
    private void PostMessageBoard(string message = "")
    {
        messageBoardText.text = message;
        if (message != "")
        {
            StartCoroutine(SetMessageBoardColor(Color.cyan, 0f));
            StartCoroutine(SetMessageBoardColor(Color.white, 0.2f));
            StartCoroutine(SetMessageBoardColor(Color.cyan, 0.4f));
            StartCoroutine(SetMessageBoardColor(Color.white, 0.6f));
        }
    }

    // 메시지 보드 색 설정용 지연 메서드
    private IEnumerator SetMessageBoardColor(Color color, float delay)
    {
        yield return new WaitForSeconds(delay);
        messageBoardText.color = color;
    }
}
