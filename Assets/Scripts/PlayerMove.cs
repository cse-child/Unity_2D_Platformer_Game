using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    public AudioClip audioPotion;
    public AudioClip audioDrop;
    public float maxSpeed;
    public float jumpPower;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider;
    Animator anim;
    AudioSource audioSource;
    

    
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // 게임 소리 파트
    public void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                break;
            case "POTION":
                audioSource.clip = audioPotion;
                break;
            case "DROP":
                audioSource.clip = audioDrop;
                break;
        }
        audioSource.Play();
    }

    // 단발적인 입력에 한에서는 Update에 소스코드를 작성하는 것이 좋다.
    void Update()
    {
        // Jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            //Debug.Log("is Jumping == true");

            //Sound
            PlaySound("JUMP");
        }

        // Stop Speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x*0.5f, rigid.velocity.y);
        }

        // Direction Sprite
        if (Input.GetButton("Horizontal")){
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }

        // Animation
        if(Mathf.Abs(rigid.velocity.x) < 0.3)
        {
            anim.SetBool("isWalking", false);
            //Debug.Log("is Walking == false");
        }
        else
        {
            anim.SetBool("isWalking", true);
            //Debug.Log("is Walking == true");
        }
        
    }
    void FixedUpdate()
    {
        // Move By Key Control
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        // Max Speed
        if (rigid.velocity.x > maxSpeed) // Right Max Speed
        {
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        } else if (rigid.velocity.x < maxSpeed * (-1))
        {
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
        }

        // Landing Platform
        if(rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            
            // 레이캐스트를 이용한 플랫폼 탐지
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector2.down, 1, LayerMask.GetMask("Platform"));

            if (rayHit.collider != null)
            {
                //Debug.Log(rayHit.distance);
                if (rayHit.distance < 0.9f)
                {
                    //Debug.Log(rayHit.collider.name);
                    anim.SetBool("isJumping", false); // 바닥에 닿으면 점프 멈춤
                    //Debug.Log("isJumping == false");
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            // Attack
            if(rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }
            else // Damaged
            {
                OnDamaged(collision.transform.position);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            // Point
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");
            bool isPotion = collision.gameObject.name.Contains("Potion");

            if (isBronze)
            {
                gameManager.stagePoint += 50;
                PlaySound("ITEM");
            }
            else if (isSilver)
            {
                gameManager.stagePoint += 80;
                PlaySound("ITEM");
            }
            else if (isGold)
            {
                gameManager.stagePoint += 100;
                PlaySound("ITEM");
            }
            else if (isPotion)
            {
                gameManager.HealthUp();
                PlaySound("POTION");
            }

            // Deactive Item
            // 아이템(코인) 먹으면 사라지게끔
            collision.gameObject.SetActive(false);
        }

        // 캐릭터가 Finish 깃발에 닿았을 때 -> 다음 스테이지 넘어감
        else if (collision.gameObject.tag == "Finish") 
        {
            // Next Stage
            gameManager.NextStage();
            //Sound
            PlaySound("FINISH");
        }
    }

    void OnAttack(Transform enemy)
    {
        // Point
        // 몬스터 한마리당 100점
        gameManager.stagePoint += 100;
        // Reaction Force
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        // Enemy Die
        Enemy_Move enemyMove = enemy.GetComponent<Enemy_Move>();
        enemyMove.OnDamaged();

        //Sound
        PlaySound("ATTACK");
    }

    void OnDamaged(Vector2 targetPos)
    {
        // Health Down
        gameManager.HealthDown();

        // Change Layer (Immortal Active)
        gameObject.layer = 11;

        // View Alpha (무적모드 설정)
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // Reaction Force
        // 오른쪽에서 장애물에 부딪히면 왼쪽으로 튕기고, 왼쪽에서 맞으면 오른쪽으로 튕기고
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1)*7, ForceMode2D.Impulse);

        // Animation
        anim.SetTrigger("doDamaged");

        //Sound
        PlaySound("DAMAGED");

        // 무적시간 3초 후 정상상태로 돌아오기
        Invoke("OffDamaged", 3);
    }

    void OffDamaged()
    {
        gameObject.layer = 10;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        // Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        // Sprite Flip Y
        spriteRenderer.flipY = true;
        // Collider Disable
        boxCollider.enabled = false;
        // Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        //Sound
        PlaySound("DIE");
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
}
