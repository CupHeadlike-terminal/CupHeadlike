using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アクターのレーザーオブジェクトクラス
/// </summary>
public class ActorLaser : MonoBehaviour
{
    //オブジェクト・コンポーネント
    private SpriteRenderer spriteRenderer;

    //各種変数
    private int damage;   //命中時のダメージ
    private float limitTime;  //存在時間(秒)この時間が過ぎると消滅
    private float maxLimitTime;  //最大存在時間

    ///<summary>
    ///初期化関数(生成元から呼出
    ///</summary>
    ///<param name="_damage">命中時のダメージ</param>
    ///<param name="_limitTime">存在時間</param>
    public void Init (int _damage, float _limitTime)
    {
        //参照習得
        spriteRenderer = GetComponent<SpriteRenderer> ();

        //変数習得
        damage = _damage;
        limitTime = maxLimitTime = _limitTime;
    }

    //Update
    void Update ()
    {
        //消滅判定
        limitTime -= Time.deltaTime;

        //スプライト透明化
        Color color = spriteRenderer.color;
        color.a = limitTime / maxLimitTime;
        spriteRenderer.color = color;

        //存在時間が0になったら消滅
        if (limitTime < 0.0f)
        {
            Destroy(gameObject);
        }
    }

    //各トリガー呼び出し処理
    //トリガー進出時に呼び出し
    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.gameObject.tag;

        //命中対象ごとの処理
        if(tag == "Enemy")
        {//エネミーに命中
            EnemyBase enemyBase = collision.gameObject.GetComponent<EnemyBase>();
            if (enemyBase != null && !enemyBase.isInvis)
            {
                enemyBase.Damaged(damage); //ダメージ処理
            }
        }
    }
}
