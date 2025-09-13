using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private BulletPool pool;
    public float lifeTime = 3f; // ������܂ł̎���
    public float timer;

    void Awake() //start���������Ăяo�����@�����͂Ȃ�
    {
        rb = GetComponent<Rigidbody2D>(); //rb��Rigidbody����
    }
    public void Init(BulletPool bulletPool) //BulletPool�^��Pool�������Ƃ���
    {
        pool = bulletPool;�@//���̃N���X�̕ϐ�pool��BulletPool�N���X��Pool���������̃N���X�ł��g����悤��
    }
    public void Shoot(Vector2 velocity)
    {
        gameObject.SetActive(true);
        rb.linearVelocity = velocity;
        timer = 0f;
    }
  
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > lifeTime)�@
        {
            ReturnToPool(); //Pool�ɕԂ�
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Wall"))
        {
            ReturnToPool();
        }
    }
    private void ReturnToPool()
    {
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
        pool.ReturnObject(this);
    }
}
