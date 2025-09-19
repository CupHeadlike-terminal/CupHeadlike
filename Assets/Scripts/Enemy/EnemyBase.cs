using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �S�G�l�~�[���ʏ����N���X
/// </summary>
public class EnemyBase : MonoBehaviour
{
    // �I�u�W�F�N�g�E�R���|�[�l���g
    [HideInInspector] public AreaManager areaManager; // �G���A�}�l�[�W��
    protected Rigidbody2D rigidbody2D; // RigidBody2D
    protected SpriteRenderer spriteRenderer;// �G�X�v���C�g
    protected Transform actorTransform; // ��l��(�A�N�^�[)��Transform

    // �e��ϐ�
    // ��b�f�[�^(�C���X�y�N�^�������)
    [Header("�ő�̗�(�����̗�)")]
    public int maxHP;
    [Header("�ڐG���A�N�^�[�ւ̃_���[�W")]
    public int touchDamage;
    // ���̑��f�[�^
    [HideInInspector] public int nowHP; // �c��HP
    [HideInInspector] public bool isInvis; // ���G���[�h
    [HideInInspector] public bool rightFacing; // �E�����t���O(false�ō�����)

    // �������֐�(AreaManager.cs����ďo)
    public void Init(AreaManager _areaManager)
    {
        // �Q�Ǝ擾
        areaManager = _areaManager;
        actorTransform = areaManager.stageManager.actorController.transform;
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // �ϐ�������
        nowHP = maxHP;
        if (transform.localScale.x > 0.0f)
            rightFacing = true;

        // �G���A���A�N�e�B�u�ɂȂ�܂ŉ������������ҋ@
        gameObject.SetActive(false);
    }
    /// <summary>
    /// ���̃����X�^�[�̋���G���A�ɃA�N�^�[���i���������̏���(�G���A�A�N�e�B�u��������)
    /// </summary>
    public virtual void OnAreaActivated()
    {
        // ���̃����X�^�[���A�N�e�B�u��
        gameObject.SetActive(true);
    }

    /// <summary>
    /// �_���[�W���󂯂�ۂɌĂяo�����
    /// </summary>
    /// <param name="damage">�_���[�W��</param>
    /// <returns>�_���[�W�����t���O true�Ő���</returns>
    public bool Damaged(int damage)
    {
        // �_���[�W����
        nowHP -= damage;

        if (nowHP <= 0.0f)
        {// HP0�̏ꍇ
            Vanish();
        }
        else
        {// �܂�HP���c���Ă���ꍇ

        }

        return true;
    }
    /// <summary>
    /// �G�l�~�[�����ł���ۂɌĂяo�����
    /// </summary>
    private void Vanish()
    {
        // �I�u�W�F�N�g����
        Destroy(gameObject);
    }

    /// <summary>
    /// �A�N�^�[�ɐڐG�_���[�W��^���鏈��
    /// </summary>
    public void BodyAttack(GameObject actorObj)
    {
        // �A�N�^�[�̃R���|�[�l���g���擾
        ActorController actorCtrl = actorObj.GetComponent<ActorController>();
        if (actorCtrl == null)
            return;

        // �A�N�^�[�ɐڐG�_���[�W��^����
        actorCtrl.Damaged(touchDamage);
    }

    /// <summary>
    /// �I�u�W�F�N�g�̌��������E�Ō��肷��
    /// </summary>
    /// <param name="isRight">�E�����t���O</param>
    public void SetFacingRight(bool isRight)
    {
        if (!isRight)
        {// ������
         // �X�v���C�g��ʏ�̌����ŕ\��
            spriteRenderer.flipX = false;
            // �E�����t���Ooff
            rightFacing = false;
        }
        else
        {// �E����
         // �X�v���C�g�����E���]���������ŕ\��
            spriteRenderer.flipX = true;
            // �E�����t���Oon
            rightFacing = true;
        }
    }
}
