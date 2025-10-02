using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// アクター操作・制御クラス
/// </summary>
public class ActorController : MonoBehaviour
{
    // オブジェクト・コンポーネント参照
    private Rigidbody2D rigidbody2D;
    private SpriteRenderer spriteRenderer;
    private ActorGroundSensor groundSensor; // アクター接地判定クラス
    private ActorSprite actorSprite; // アクタースプライト設定クラス
    public CameraController cameraController; // カメラ制御クラス
    public GameObject weaponBulletPrefab; // 弾プレハブ
    public Image hpGage; // HPゲージ

    // 体力変数
    [HideInInspector] public int nowHP; // 現在HP
    [HideInInspector] public int maxHP; // 最大HP

    // 移動関連変数
    [HideInInspector] public float xSpeed; // X方向移動速度
    [HideInInspector] public bool rightFacing; // 向いている方向(true.右向き false:左向き)
    private float remainJumpTime;   // 空中でのジャンプ入力残り受付時間

    // その他変数
    private float remainStuckTime; // 残り硬直時間(0以上だと行動できない)
    private float invincibleTime;   // 残り無敵時間(秒)
    [HideInInspector] public bool isDefeat; // true:撃破された(ゲームオーバー)
    [HideInInspector] public bool inWaterMode; // true:水中モード(メソッドから変更)

    // 定数定義
    private const int InitialHP = 30;           // 初期HP(最大HP)
    private const float InvicibleTime = 2.0f;   // 被ダメージ直後の無敵時間(秒)
    private const float StuckTime = 0.5f;       // 被ダメージ直後の硬直時間(秒)
    private const float KnockBack_X = 2.5f;     // 被ダメージ時ノックバック力(x方向)
    private const float WaterModeDecelerate_X = 0.8f;// 水中でのX方向速度倍率
    private const float WaterModeDecelerate_Y = 0.92f;// 水中でのY方向速度倍率

    // Start（オブジェクト有効化時に1度実行）
    void Start()
    {
        // コンポーネント参照取得
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        groundSensor = GetComponentInChildren<ActorGroundSensor>();
        actorSprite = GetComponent<ActorSprite>();

        // 配下コンポーネント初期化
        actorSprite.Init(this);

        // カメラ初期位置
        cameraController.SetPosition(transform.position);

        // 変数初期化
        rightFacing = true; // 最初は右向き
        nowHP = maxHP = InitialHP;
        hpGage.fillAmount = 1.0f; // HPゲージの初期FillAmount
    }

    // Update（1フレームごとに1度ずつ実行）
    void Update()
    {
        // 撃破された後なら終了
        if (isDefeat)
            return;

        // 無敵時間が残っているなら減少
        if (invincibleTime > 0.0f)
        {
            invincibleTime -= Time.deltaTime;
            if (invincibleTime <= 0.0f)
            {// 無敵時間終了時処理
                actorSprite.EndBlinking(); // 点滅終了
            }
        }
        // 硬直時間処理
        if (remainStuckTime > 0.0f)
        {// 硬直時間減少
            remainStuckTime -= Time.deltaTime;
            if (remainStuckTime <= 0.0f)
            {// スタン時間終了時処理
                actorSprite.stuckMode = false;
            }
            else
                return;
        }

        // 左右移動処理
        MoveUpdate();
        // ジャンプ入力処理
        JumpUpdate();

        // 攻撃入力処理
        StartShotAction();

        // 坂道で滑らなくする処理
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation; // Rigidbodyの機能のうち回転だけは常に停止
        if (groundSensor.isGround && !Input.GetKey(KeyCode.UpArrow))
        {
            // 坂道を登っている時上昇力が働かないようにする処理
            if (rigidbody2D.linearVelocity.y > 0.0f)
            {
                rigidbody2D.linearVelocity = new Vector2(rigidbody2D.linearVelocity.x, 0.0f);
            }
            // 坂道に立っている時滑り落ちないようにする処理
            if (Mathf.Abs(xSpeed) < 0.1f)
            {
                // Rigidbodyの機能のうち移動と回転を停止
                rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            }
        }

        // カメラに自身の座標を渡す
        cameraController.SetPosition(transform.position);
    }

    #region 移動関連
    /// <summary>
    /// Updateから呼び出される左右移動入力処理
    /// </summary>
    private void MoveUpdate()
    {
        // X方向移動入力
        if (Input.GetKey(KeyCode.RightArrow))
        {// 右方向の移動入力
         // X方向移動速度をプラスに設定
            xSpeed = 6.0f;

            // 右向きフラグon
            rightFacing = true;

            // スプライトを通常の向きで表示
            spriteRenderer.flipX = false;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {// 左方向の移動入力
         // X方向移動速度をマイナスに設定
            xSpeed = -6.0f;

            // 右向きフラグoff
            rightFacing = false;

            // スプライトを左右反転した向きで表示
            spriteRenderer.flipX = true;
        }
        else
        {// 入力なし
         // X方向の移動を停止
            xSpeed = 0.0f;
        }
    }

    /// <summary>
    /// Updateから呼び出されるジャンプ入力処理
    /// </summary>
    private void JumpUpdate()
    {
        // 空中でのジャンプ入力受付時間減少
        if (remainJumpTime > 0.0f)
            remainJumpTime -= Time.deltaTime;

        // ジャンプ操作
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {// ジャンプ開始
         // 接地していないなら終了(水中であれば続行)
            if (!groundSensor.isGround && !inWaterMode)
                return;

            // ジャンプ力を計算
            float jumpPower = 10.0f;
            // ジャンプ力を適用
            rigidbody2D.linearVelocity = new Vector2(rigidbody2D.linearVelocity.x, jumpPower);

            // 空中でのジャンプ入力受け付け時間設定
            remainJumpTime = 0.25f;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {// ジャンプ中（ジャンプ入力を長押しすると継続して上昇する処理）
         // 空中でのジャンプ入力受け付け時間が残ってないなら終了
            if (remainJumpTime <= 0.0f)
                return;
            // 接地しているなら終了
            if (groundSensor.isGround)
                return;

            // ジャンプ力加算量を計算
            float jumpAddPower = 30.0f * Time.deltaTime; // Update()は呼び出し間隔が異なるのでTime.deltaTimeが必要
                                                         // ジャンプ力加算を適用
            rigidbody2D.linearVelocity += new Vector2(0.0f, jumpAddPower);
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {// ジャンプ入力終了
            remainJumpTime = -1.0f;
        }
    }

    // FixedUpdate（一定時間ごとに1度ずつ実行・物理演算用）
    private void FixedUpdate()
    {
        // 移動速度ベクトルを現在値から取得
        Vector2 velocity = rigidbody2D.linearVelocity;
        // X方向の速度を入力から決定
        velocity.x = xSpeed;

        // 水中モードでの速度
        if (inWaterMode)
        {
            velocity.x *= WaterModeDecelerate_X;
            velocity.y *= WaterModeDecelerate_Y;
        }

        // 計算した移動速度ベクトルをRigidbody2Dに反映
        rigidbody2D.linearVelocity = velocity;
    }

    /// <summary>
    /// 水中モードをセットする
    /// </summary>
    /// <param name="mode">true:水中にいる</param>
    public void SetWaterMode(bool mode)
    {
        // 水中モード
        inWaterMode = mode;
        // 水中での重力
        if (inWaterMode)
        {
            rigidbody2D.gravityScale = 0.3f;
        }
        else
        {
            rigidbody2D.gravityScale = 1.0f;
        }
    }
    #endregion

    #region 戦闘関連
    /// <summary>
    /// ダメージを受ける際に呼び出される
    /// </summary>
    /// <param name="damage">ダメージ量</param>
    public void Damaged(int damage)
    {
        // 撃破された後なら終了
        if (isDefeat)
            return;

        // もし無敵時間中ならダメージ無効
        if (invincibleTime > 0.0f)
            return;

        // ダメージ処理
        nowHP -= damage;
        // HPゲージの表示を更新する
        float hpRatio = (float)nowHP / maxHP;
        hpGage.DOFillAmount(hpRatio, 0.5f);

        // HP0ならゲームオーバー処理
        if (nowHP <= 0)
        {
            isDefeat = true;
            // 被撃破演出開始
            actorSprite.StartDefeatAnim();
            // 物理演算を停止
            rigidbody2D.linearVelocity = Vector2.zero;
            xSpeed = 0.0f;
            rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            // ゲームオーバー処理
            GetComponentInParent<StageManager>().GameOver();
            return;
        }

        // スタン硬直
        remainStuckTime = StuckTime;
        actorSprite.stuckMode = true;

        // ノックバック処理
        // ノックバック力・方向決定
        float knockBackPower = KnockBack_X;
        if (rightFacing)
            knockBackPower *= -1.0f;
        // ノックバック適用
        xSpeed = knockBackPower;

        // 無敵時間発生
        invincibleTime = InvicibleTime;
        if (invincibleTime > 0.0f)
            actorSprite.StartBlinking(); // 点滅開始
    }

    /// <summary>
    /// 攻撃ボタン入力時処理
    /// </summary>
    public void StartShotAction()
    {
        // 攻撃ボタンが入力されていないなら終了
        if (!Input.GetKeyDown(KeyCode.Z))
            return;

        // このメソッド内で選択武器別のメソッドの呼び分けやエネルギー消費処理を行う。
        // 現在は初期武器のみなのでShotAction_Normalを呼び出すだけ
        ShotAction_Normal();
    }

    /// <summary>
    /// 射撃アクション：通常攻撃
    /// </summary>
    private void ShotAction_Normal()
    {
        // 弾の方向を取得
        float bulletAngle = 0.0f; // 右向き
                                  // アクターが左向きなら弾も左向きに進む
        if (!rightFacing)
            bulletAngle = 180.0f;

        // 弾丸オブジェクト生成・設定
        GameObject obj = Instantiate( // オブジェクト新規生成
            weaponBulletPrefab,     // 生成するオブジェクトのプレハブ
            transform.position,     // 生成したオブジェクトの初期座標
            Quaternion.identity);   // 初期Rotation(傾き)
                                    // 弾丸設定
        obj.GetComponent<ActorNormalShot>().Init(
            12.0f,      // 速度
            bulletAngle,// 角度
            1,          // ダメージ量
            5.0f);      // 存在時間
    }
    #endregion
}