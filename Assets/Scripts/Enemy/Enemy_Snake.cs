using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ʓG�N���X�FSnake
/// 
/// �A�N�^�[���߂��ɂ���Ɛڋ߂���
/// �U�����Ă��Ȃ����̓�����͂��Ă���
/// </summary>
public class Enemy_Snake : EnemyBase
{
    // Start
    void Start()
    {
    }
    /// <summary>
    /// ���̃����X�^�[�̋���G���A�ɃA�N�^�[���i���������̋N��������(�G���A�A�N�e�B�u��������)
    /// </summary>
    public override void OnAreaActivated()
    {
        // ���X�̋N�������������s
        base.OnAreaActivated();

        Debug.Log("�ǉ��̋N��������");
    }

    // Update
    void Update()
    {
    }
}
