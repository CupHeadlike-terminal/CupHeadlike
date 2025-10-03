using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// ステージマネージャクラス
/// </summary>
public class StageManager : MonoBehaviour
{
    [HideInInspector] public ActorController actorController; // アクター制御クラス
    [HideInInspector] public CameraController cameraController; // カメラ制御クラス
    public Image bossHPGage; // ボス用HPゲージImage

    [Header("初期エリアのAreaManager")]
    public AreaManager initArea; // ステージ内の最初のエリア(初期エリア)
    [Header("ボス戦用BGMのAudioClip")]
    public AudioClip bossBGMClip;

    // ステージ内の全エリアの配列(Startで取得)
    private AreaManager[] inStageAreas;

    // Start
    void Start()
    {
        // 参照取得
        actorController = GetComponentInChildren<ActorController>();
        cameraController = GetComponentInChildren<CameraController>();

        // ステージ内の全エリアを取得・初期化
        inStageAreas = GetComponentsInChildren<AreaManager>();
        foreach (var targetAreaManager in inStageAreas)
            targetAreaManager.Init(this);

        // 初期エリアをアクティブ化(その他のエリアは全て無効化)
        initArea.ActiveArea();

        // UI初期化
        bossHPGage.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// ステージ内の全エリアを無効化する
    /// </summary>
    public void DeactivateAllAreas()
    {
        foreach (var targetAreaManager in inStageAreas)
            targetAreaManager.gameObject.SetActive(false);
    }

    /// <summary>
    /// ボス戦用BGMを再生する
    /// </summary>
    public void PlayBossBGM()
    {
        // BGMを変更する
        GetComponent<AudioSource>().clip = bossBGMClip;
        GetComponent<AudioSource>().Play();
    }

    /// <summary>
    /// ステージクリア時処理
    /// </summary>
    public void StageClear()
    {
        // ステージクリア処理
    }

    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    public void GameOver()
    {
        // ゲームオーバー処理
    }
}