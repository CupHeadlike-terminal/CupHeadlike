
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アクターの通常弾処理クラス
/// </summary>
public class ActorNormalShot : MonoBehaviour
{
    // 各種変数
    protected float speed;      // 弾速
    protected float angle;      // 角度(0-360)0で右・90で上
    protected int damage;       // 命中時のダメージ
    protected float limitTime;  // 存在時間(秒)この時間が過ぎると消滅

    /// <summary>
    /// 初期化関数(生成元から呼出)
    /// </summary>
    /// <param name="_speed">弾速</param>
    /// <param name="_angle">角度</param>
    /// <param name="_damage">命中時のダメージ</param>
    /// <param name="_limitTime">存在時間(秒)</param>
    public void Init(float _speed, float _angle, int _damage, float _limitTime)
    {
        // 変数取得
        speed = _speed;
        angle = _angle;
        damage = _damage;
        limitTime = _limitTime;
    }

    // Update
    void Update()
    {
        // 移動ベクトル計算(1フレーム分の進行方向と距離を取得)
        Vector3 vec = new Vector3(speed * Time.deltaTime, 0.0f, 0.0f);
        vec = Quaternion.Euler(0, 0, angle) * vec; // ベクトル回転

        // 移動ベクトルをもとに移動
        transform.Translate(vec);

        // 消滅判定
        limitTime -= Time.deltaTime;
        if (limitTime < 0.0f)
        {// 存在時間が0になったら消滅
            Destroy(gameObject);
        }
    }

    // 各トリガー呼び出し処理
    // トリガー進入時に呼出
    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.gameObject.tag;

        // 命中対象ごとの処理
        if (tag == "Enemy")
        {// エネミーに命中
            EnemyBase enemyBase = collision.gameObject.GetComponent<EnemyBase>();
            if (enemyBase != null && !enemyBase.isInvis)
            {
                bool result = enemyBase.Damaged(damage); // ダメージ処理
                                                         // ダメージを与えられたなら弾オブジェクト削除
                if (result)
                {
                    // この弾を削除
                    Destroy(gameObject);
                }
            }
        }
        else if (tag == "Ground")
        {// 地面・壁に命中
            Destroy(gameObject);
        }
    }
}