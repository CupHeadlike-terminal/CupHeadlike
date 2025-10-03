using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// アクターのスプライトを設定するクラス
/// </summary>
public class ActorSprite : MonoBehaviour
{
    private ActorController actorController; // アクター制御クラス
    private SpriteRenderer spriteRenderer; // アクターのSpriteRenderer
    public GameObject defeatParticlePrefab = null; // 被撃破パーティクルPrefab

    // 画像素材参照
    public List<Sprite> walkAnimationRes; // 歩行アニメーション(装備別*コマ数)
    public List<Sprite> stuckSpriteRes; // スタンスプライト(装備別)

    // 各種変数
    private float walkAnimationTime; // 歩行アニメーション経過時間
    private int walkAnimationFrame; // 歩行アニメーションの現在のコマ番号
    private Tween blinkTween;   // 点滅処理Tween
    public bool stuckMode;      // スタン画像表示モード

    // 定数定義
    private const int WalkAnimationNum = 3; // 歩行アニメーションの1種類あたりの枚数
    private const float WalkAnimationSpan = 0.3f; // 歩行アニメーションのスプライト切り替え時間

    // 初期化関数(ActorController.csから呼出)
    public void Init(ActorController _actorController)
    {
        // 参照取得
        actorController = _actorController;
        spriteRenderer = actorController.GetComponent<SpriteRenderer>();
    }

    // Update
    void Update()
    {
        // 被撃破中なら終了
        if (actorController.isDefeat)
            return;

        // スタン画像表示モード中ならスタン画像を表示
        if (stuckMode)
        {
            spriteRenderer.sprite = stuckSpriteRes[0];
            spriteRenderer.sprite = stuckSpriteRes[(int)actorController.nowWeapon];
            return;
        }

        // 歩行アニメーション時間を経過(横移動している間のみ)
        if (Mathf.Abs(actorController.xSpeed) > 0.0f)
            walkAnimationTime += Time.deltaTime;
        // 歩行アニメーションコマ数を計算
        if (walkAnimationTime >= WalkAnimationSpan)
        {
            walkAnimationTime -= WalkAnimationSpan;
            // コマ数を増加
            walkAnimationFrame++;
            // コマ数が歩行アニメーション枚数を越えているなら0に戻す
            if (walkAnimationFrame >= WalkAnimationNum)
                walkAnimationFrame = 0;
        }

        // 歩行アニメーション更新
        spriteRenderer.sprite =
            walkAnimationRes[walkAnimationFrame];
    }

    /// <summary>
    /// 点滅開始処理
    /// </summary>
    public void StartBlinking()
    {
        // DoTweenを使った点滅処理
        blinkTween = spriteRenderer.DOFade(0.0f, 0.15f) // 1回分の再生時間:0.15秒
            .SetDelay(0.3f) // 0.3秒遅延
            .SetEase(Ease.Linear) // 線形変化
            .SetLoops(-1, LoopType.Yoyo);    // 無限ループ再生(偶数回は逆再生)
    }
    /// <summary>
    /// 被撃破演出開始
    /// </summary>
    public void StartDefeatAnim()
    {
        // 被撃破パーティクルを生成
        var obj = Instantiate(defeatParticlePrefab);
        obj.transform.position = transform.position;
        // 被撃破スプライト表示
        spriteRenderer.sprite = stuckSpriteRes[0];
        // 点滅演出終了
        if (blinkTween != null)
            blinkTween.Kill();
        // スプライト非表示化アニメーション(DOTween)
        spriteRenderer.DOFade(0.0f, 2.0f); // 2.0秒かけてスプライトの非透明度を0.0fにする
    }
    /// <summary>
    /// 点滅終了処理
    /// </summary>
    public void EndBlinking()
    {
        // DoTweenの点滅処理を終了させる
        if (blinkTween != null)
        {
            blinkTween.Kill(); // Tweenを終了
            spriteRenderer.color = Color.white; // 色を元に戻す
            blinkTween = null;  // Tween情報を初期化
        }
    }
}