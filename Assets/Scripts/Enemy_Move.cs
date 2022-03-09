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

        Invoke("Think", 5); // 5�� �ڿ� ����
    }
    
    void FixedUpdate()
    {
        // Move
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        // Platform Check (������ �ٴڿ� �ִ��� üũ)
        Vector2 frontVector = new Vector2(rigid.position.x+nextMove*0.2f, rigid.position.y);
        Debug.DrawRay(frontVector, Vector3.down, new Color(0, 1, 0));

        RaycastHit2D rayHit = Physics2D.Raycast(frontVector, Vector2.down, 1, LayerMask.GetMask("Platform"));

        // ������Ʈ�� �ٴڿ� �ȴ����� �ݴ�������� �����̵���
        if (rayHit.collider == null)
        {
            Turn();
        }

    }

    // ��� �Լ� (���Ͱ� ������ ������ �������� ������)
    void Think()
    {
        // -1, 0, 1 �� �ϳ� ���� (����, ����, ������)
        nextMove = Random.Range(-1, 2); 

        // Sprite Animation (����, ������ �ִϸ��̼� ����)
        anim.SetInteger("WalkSpeed", nextMove);

        // ĳ���Ͱ� �ٶ󺸴� ���� FlipX
        if (nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == 1;
        }

        // ���� ȣ��
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    void Turn()
    {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove == 1;

        CancelInvoke(); // �ʸ� ���� Invoke�� ����
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
