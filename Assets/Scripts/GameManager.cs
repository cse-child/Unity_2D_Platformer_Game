using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    static public int bestScore = 0;
    public int resultPoint;

    public PlayerMove player;
    public GameObject[] Stages;
    public GameObject[] background;

    public Image[] UIHealth;
    public Text UIPoint;
    public Text UIStage;
    public GameObject UIRestartBtn;

    // ���� ���� ȭ��
    public GameObject GameImage;
    public GameObject CoverImage;
    public GameObject StartStage;
    public GameObject Player;

    // ���� Ŭ���� ȭ��
    public Text UIScore;
    public Text UIBestScore;
    public GameObject ClearImage;
    public GameObject NewImage;

    // ������ ���?
    public RawImage mScreen = null;
    public VideoPlayer mVideoPlayer = null;



    void Update()
    {
        // ���� ������Ʈ
        UIPoint.text = (totalPoint + stagePoint).ToString();
        resultPoint = totalPoint + stagePoint;
    }

    public void OnClickStartButton()
    {
        CoverImage.SetActive(false);
        GameImage.SetActive(true);
        StartStage.SetActive(true);
        Player.SetActive(true);
    }

    public void OnClickExitButton()
    {
        Application.Quit();
    }


    public void NextStage()
    {
        // Change Stage
        if (stageIndex < Stages.Length - 1)
        {
            Stages[stageIndex].SetActive(false);
            background[stageIndex].SetActive(false);

            stageIndex++;

            Stages[stageIndex].SetActive(true);
            background[stageIndex].SetActive(true);

            PlayerReposition();

            UIStage.text = "STAGE " + (stageIndex + 1);

            
        }
        else // Game Clear
        {
            // Player Control Lock
            Time.timeScale = 0;

            // Result UI
            Debug.Log("���� Ŭ����!");

            // Clrear Image UI
            ClearImage.SetActive(true);
            OnClearEnable();

        }
     
        // Calculate Point
        totalPoint += stagePoint;
        stagePoint = 0;
        
    }

    public void HealthDown()
    {
        if (health > 1)
        {
            health--;
            UIHealth[health].color = new Color(1, 0, 0, 0.4f);
        }
        else // ����� �� ������
        {
            // All Health UI Off
            UIHealth[0].color = new Color(1, 0, 0, 0.4f);

            // Player Die Effect
            player.OnDie();

            // Retry Button UI
            UIRestartBtn.SetActive(true);
            

        }
    }

    public void HealthUp()
    {
        if(health <= 2)
        {
            Debug.Log("Health++ �ε��� : " + health);
            UIHealth[health].color = new Color(1, 1, 1);
            health++;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // Player Reposition
            if (health > 1)
            {
                PlayerReposition();
            }

            // Health Down
            HealthDown();

            // �������� �Ҹ�
            player.PlaySound("DROP");
        }
    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(-4, 1, -1);
        player.VelocityZero();
    }

    public void Restart(int index)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(index);
    }

    void OnClearEnable()
    {
        UIScore.text = resultPoint.ToString();
        Debug.Log("UIScore.text : " + UIScore.text);
        Debug.Log("totalPoint.ToString() : " + resultPoint.ToString());
        Debug.Log("bestScore : " + bestScore);

        if (bestScore < resultPoint)
        {
            bestScore = int.Parse(UIScore.text);
            UIBestScore.text = bestScore.ToString();
            NewImage.SetActive(true);
        }
        else
        {
            NewImage.SetActive(false);
        }

        UIBestScore.text = bestScore.ToString();

        // ������
        if (mScreen != null && mVideoPlayer != null)
        {
            // ���� �غ� �ڷ�ƾ ȣ��
            StartCoroutine(PrepareVideo());

            PlayVideo();
            Debug.Log("���� �÷���");
        }


    }

    protected IEnumerator PrepareVideo()
    {
        // ���� �غ�
        mVideoPlayer.Prepare();

        // ������ �غ�Ǵ� ���� ��ٸ�
        while (!mVideoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // VideoPlayer�� ��� texture�� RawImage�� texture�� �����Ѵ� 
        mScreen.texture = mVideoPlayer.texture;
    }

    public void PlayVideo()
    {
        if (mVideoPlayer != null && mVideoPlayer.isPrepared)
        {
            // ���� ���
            mVideoPlayer.Play();
        }
    }
}
