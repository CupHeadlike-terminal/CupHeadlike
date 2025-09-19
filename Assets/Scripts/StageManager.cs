using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �X�e�[�W�}�l�[�W���N���X
/// </summary>
public class StageManager : MonoBehaviour
{
    [HideInInspector] public ActorController actorController; // �A�N�^�[����N���X
    [HideInInspector] public CameraController cameraController; // �J��������N���X

    [Header("�����G���A��AreaManager")]
    public AreaManager initArea; // �X�e�[�W���̍ŏ��̃G���A(�����G���A)

    // �X�e�[�W���̑S�G���A�̔z��(Start�Ŏ擾)
    private AreaManager[] inStageAreas;

    // Start
    void Start()
    {
        // �Q�Ǝ擾
        actorController = GetComponentInChildren<ActorController>();
        cameraController = GetComponentInChildren<CameraController>();

        // �X�e�[�W���̑S�G���A���擾�E������
        inStageAreas = GetComponentsInChildren<AreaManager>();
        foreach (var targetAreaManager in inStageAreas)
            targetAreaManager.Init(this);

        // �����G���A���A�N�e�B�u��(���̑��̃G���A�͑S�Ė�����)
        initArea.ActiveArea();
    }

    /// <summary>
    /// �X�e�[�W���̑S�G���A�𖳌�������
    /// </summary>
    public void DeactivateAllAreas()
    {
        foreach (var targetAreaManager in inStageAreas)
            targetAreaManager.gameObject.SetActive(false);
    }
}