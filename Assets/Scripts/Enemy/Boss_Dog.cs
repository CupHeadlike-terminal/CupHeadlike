using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ʓG�N���X(�{�X)�FDog
/// �ːi�U���E�W�����v�U��
/// </summary>
public class Boss_Dog : EnemyBase
{
    // �I�u�W�F�N�g�E�R���|�[�l���g

    // �摜�f��
    [Header("�摜�f��")]
    [SerializeField] private Sprite sprite_Wait = null; // �ҋ@��
    [SerializeField] private Sprite sprite_Move = null; // �ړ���
    [SerializeField] private Sprite sprite_Jump = null; // �W�����v��

    // �ݒ荀��
    [Header("�U���Ԋu")]
    public float attackInterval;
    [Header("�ړ����x")]
    public float movingSpeed;
    [Header("�W�����v���ړ����x")]
    public float jumpSpeed;
    [Header("�W�����v��(�ŏ�)")]
    public float jumpPower_Min;
    [Header("�W�����v��(�ő�)")]
    public float jumpPower_Max;
    [Header("�W�����v�m��(0-100)")]
    public int jumpRatio;

    // �e��ϐ�
    private float nextAttackTime; // ���̍U���܂ł̎c�莞��

    // Start
    void Start()
    {
        // �ϐ�������
        nextAttackTime = attackInterval / 2.0f;
    }
    /// <summary>
    /// ���̃����X�^�[�̋���G���A�ɃA�N�^�[���i���������̏���(�G���A�A�N�e�B�u��������)
    /// </summary>
    public override void OnAreaActivated()
    {
        base.OnAreaActivated();
    }

    // Update
    void Update()
    {
        // ���Œ��Ȃ珈�����Ȃ�
        if (isVanishing)
            return;

        // �\���X�v���C�g�ύX
        // �ڒn����擾
        ContactPoint2D[] contactPoints = new ContactPoint2D[2];
        rigidbody2D.GetContacts(contactPoints);
        bool isGround = contactPoints[1].enabled;
        // �X�v���C�g���f
        if (!isGround)
        {// �W�����v��
            spriteRenderer.sprite = sprite_Jump;
        }
        else if (Mathf.Abs(rigidbody2D.linearVelocity.x) >= 0.1f)
        {// ���ړ���
            spriteRenderer.sprite = sprite_Move;
        }
        else
        {// �ҋ@��
            spriteRenderer.sprite = sprite_Wait;
        }

        // �U���Ԋu����
        nextAttackTime -= Time.deltaTime;
        if (nextAttackTime > 0.0f)
            return;
        nextAttackTime = attackInterval;
        // ��x�ł��U��������d�͉����x��������
        rigidbody2D.gravityScale = 0.5f;

        // �U���J�n
        Vector2 velocity = new Vector2(); // ���x
                                          // �U���̎�ތ���
        if (Random.Range(0, 100) < jumpRatio)
        {// �W�����v�U��
            velocity.x = jumpSpeed;
            velocity.y = Random.Range(jumpPower_Min, jumpPower_Max);
        }
        else
        {// �ʏ�ړ�
            velocity.x = movingSpeed;
        }

        // �A�N�^�[�Ƃ̈ʒu�֌W�������������
        if (transform.position.x > actorTransform.position.x)
        {// ������
            SetFacingRight(false);
            velocity.x *= -1.0f;
        }
        else
        {// �E����
            SetFacingRight(true);
        }

        // ���x�𔽉f
        rigidbody2D.linearVelocity = velocity;
    }
}
