using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Move : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider;
    public int nextMove;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        Invoke("Think", 5); // 5초 뒤에 실행
    }
    
    void FixedUpdate()
    {
        // Move
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        // Platform Check (지형이 바닥에 있는지 체크)
        Vector2 frontVector = new Vector2(rigid.position.x+nextMove*0.2f, rigid.position.y);
        Debug.DrawRay(frontVector, Vector3.down, new Color(0, 1, 0));

        RaycastHit2D rayHit = Physics2D.Raycast(frontVector, Vector2.down, 1, LayerMask.GetMask("Platform"));

        // 레이히트가 바닥에 안닿으면 반대방향으로 움직이도록
        if (rayHit.collider == null)
        {
            Turn();
        }

    }

    // 재귀 함수 (몬스터가 움직일 방향을 랜덤으로 선택함)
    void Think()
    {
        // -1, 0, 1 중 하나 선택 (왼쪽, 멈춤, 오른쪽)
        nextMove = Random.Range(-1, 2); 

        // Sprite Animation (왼쪽, 오른쪽 애니메이션 방향)
        anim.SetInteger("WalkSpeed", nextMove);

        // 캐릭터가 바라보는 방향 FlipX
        if (nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == 1;
        }

        // 다음 호출
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    void Turn()
    {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove == 1;

        CancelInvoke(); // 초를 세던 Invoke를 멈춤
        Invoke("Think", 5);
    }

    public void OnDamaged()
    {
        // Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        // Sprite Flip Y
        spriteRenderer.flipY = true;
        // Collider Disable
        boxCollider.enabled = false;
        // Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        // Destroy
        Invoke("DeActive", 5);
    }

    void DeActive()
    {
        gameObject.SetActive(false);
    }
}
