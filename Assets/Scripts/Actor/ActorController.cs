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
    public Image energyGage = null; // 武器エネルギーゲージ
    public Image energyGageIcon = null; // 武器エネルギーゲージアイコン

    [Header("各武器で使用するプレハブリスト(定義の順番に設定)")]
    public List<GameObject> weaponBulletPrefabs;
    [Header("各武器のエネルギーゲージの画像")]
    public List<Sprite> weaponIconSprites;
    [Header("各武器のエネルギーゲージの色")]
    public List<Color> weaponGageColors;
    [Header("各武器の消費エネルギー量")]
    public List<int> weaponEnergyCosts;
    [Header("各武器の連射間隔(秒)")]
    public List<float> weaponIntervals;

    // 体力変数
    [HideInInspector] public int nowHP; // 現在HP
    [HideInInspector] public int maxHP; // 最大HP

    // 装備変数
    [HideInInspector] public ActorWeaponType nowWeapon;
    private int[] weaponEnergies; // 武器の残りエネルギーデータ(それぞれ最大値がMaxEnergy)
    private float weaponRemainInterval; // 武器が次に発射可能になるまでの残り時間(秒)

    // 移動関連変数
    [HideInInspector] public float xSpeed; // X方向移動速度
    [HideInInspector] public bool rightFacing; // 向いている方向(true.右向き false:左向き)
    private float remainJumpTime;   // 空中でのジャンプ入力残り受付時間

    // その他変数
    private float remainStuckTime; // 残り硬直時間(0以上だと行動できない)
    private float invincibleTime;   // 残り無敵時間(秒)
    [HideInInspector] public bool isDefeat; // true:撃破された(ゲームオーバー)

    // 定数定義
    private const int InitialHP = 20;           // 初期HP(最大HP)
    private const int MaxEnergy = 20;			// 武器エネルギーの最大値
    private const float InvicibleTime = 2.0f;   // 被ダメージ直後の無敵時間(秒)
    private const float StuckTime = 0.5f;       // 被ダメージ直後の硬直時間(秒)
    private const float KnockBack_X = 2.5f;     // 被ダメージ時ノックバック力(x方向)

    // アクター装備定義
    public enum ActorWeaponType
    {
        Normal,     // (通常)
        Tackle,     // タックル
        Windblow,   // 突風
        IceBall,    // 雪玉
        Lightning,  // 稲妻
        WaterRing,  // 水の輪
        Laser,      // レーザー
        _Max,
    }

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

        // 武器エネルギー初期化
        weaponEnergies = new int[(int)ActorWeaponType._Max];
        for (int i = 0; i < (int)ActorWeaponType._Max; i++)
            weaponEnergies[i] = MaxEnergy;
        ApplyWeaponChange(); // 初期装備を反映

        // 変数初期化
        rightFacing = true; // 最初は右向き
        nowHP = maxHP = InitialHP; // 初期HP
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

        // 武器切り替え処理
        ChangeWeaponUpdate();

        // 攻撃可能までの残り時間減少
        if (weaponRemainInterval > 0.0f)
            weaponRemainInterval -= Time.deltaTime;

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
         // 接地していないなら終了
            if (!groundSensor.isGround)
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

        // 計算した移動速度ベクトルをRigidbody2Dに反映
        rigidbody2D.linearVelocity = velocity;
    }
    #endregion

    #region 装備関連
	/// <summary>
	/// Updateから呼び出される武器切り替え処理
	/// </summary>
	private void ChangeWeaponUpdate ()
	{
		// 武器切り替え
		if (Input.GetKeyDown (KeyCode.A))
		{// 1つ前に切り替え
			if (nowWeapon == ActorWeaponType.Normal)
				nowWeapon = ActorWeaponType._Max;
			nowWeapon--;
			// 武器変更を反映
			ApplyWeaponChange ();
		}
		else if (Input.GetKeyDown (KeyCode.S))
		{// 1つ次に切り替え
			nowWeapon++;
			if (nowWeapon == ActorWeaponType._Max)
				nowWeapon = ActorWeaponType.Normal;
			// 武器変更を反映
			ApplyWeaponChange ();
		}
	}
 
	/// <summary>
	/// 特殊武器の変更を反映する
	/// </summary>
	public void ApplyWeaponChange ()
	{
		// エネルギーゲージ表示(通常武器以外)
		if (nowWeapon == ActorWeaponType.Normal)
			energyGage.transform.parent.gameObject.SetActive (false);
		else
			energyGage.transform.parent.gameObject.SetActive (true);
 
		// ゲージの色を反映
		energyGage.color = weaponGageColors[(int)nowWeapon];
		// ゲージの量を反映
		energyGage.fillAmount = (float)weaponEnergies[(int)nowWeapon] / MaxEnergy;
		// ゲージのアイコンを設定
		energyGageIcon.sprite = weaponIconSprites[(int)nowWeapon];
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

        // 武器エネルギーが足りないなら攻撃しない
        if (weaponEnergies[(int)nowWeapon] <= 0)
            return;
        // 攻撃可能までの時間が残っているなら終了
        if (weaponRemainInterval > 0.0f)
            return;

        // 武器エネルギー減少
        weaponEnergies[(int)nowWeapon] -= weaponEnergyCosts[(int)nowWeapon];
        if (weaponEnergies[(int)nowWeapon] < 0)
            weaponEnergies[(int)nowWeapon] = 0;
        // 武器エネルギーゲージ表示更新
        energyGage.fillAmount = (float)weaponEnergies[(int)nowWeapon] / MaxEnergy;
        // 次弾発射可能までの残り時間設定
        weaponRemainInterval = weaponIntervals[(int)nowWeapon];

        // 攻撃を発射
        switch (nowWeapon)
        {
            case ActorWeaponType.Normal:
                // 通常攻撃
                ShotAction_Normal();
                break;
            case ActorWeaponType.Tackle:
                // タックル
                ShotAction_Tackle();
                break;
            case ActorWeaponType.Windblow:
                // 突風
                ShotAction_Windblow();
                break;
            case ActorWeaponType.IceBall:
                // 雪玉
                ShotAction_IceBall();
                break;
            case ActorWeaponType.Lightning:
                // 稲妻
                ShotAction_Lightning();
                break;
            case ActorWeaponType.WaterRing:
                // 水の輪
                ShotAction_WaterRing();
                break;
            case ActorWeaponType.Laser:
                // レーザー
                ShotAction_Laser();
                break;
        }

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
        GameObject obj = Instantiate(weaponBulletPrefabs[(int)ActorWeaponType.Normal], transform.position, Quaternion.identity);
        obj.GetComponent<ActorNormalShot>().Init(
            12.0f,      // 速度
            bulletAngle,// 角度
            1,          // ダメージ量
            5.0f,
            nowWeapon);      // 存在時間
    }
    #endregion

    #region 特殊武器関連
    ///summaly
    ///射撃アクション:タックル
    ///summaly
    private void ShotAction_Tackle()
    {
        // 弾の方向を取得
        float bulletAngle = 0.0f; // 右向き
                                  // アクターが左向きなら弾も左向きに進む
        if (!rightFacing)
            bulletAngle = 180.0f;

        // 弾丸オブジェクト生成・設定
        GameObject obj = Instantiate(weaponBulletPrefabs[(int)ActorWeaponType.Tackle], transform.position, Quaternion.identity);
        obj.GetComponent<ActorNormalShot>().Init(
            20.0f, // 速度
            bulletAngle, // 角度
            1, // ダメージ量
            0.3f, // 存在時間
            nowWeapon); // 使用武器
        if (!rightFacing)
            obj.GetComponent<SpriteRenderer>().flipX = true;

        // 主人公の突進移動
        Vector3 moveVector = new Vector3(1.2f, 0.25f, 0.0f);
        if (!rightFacing)
            moveVector.x *= -1.0f;
        rigidbody2D.MovePosition(transform.position + moveVector);
        groundSensor.isGround = false;

        // 無敵時間発生
        invincibleTime = 0.6f;
    }
    /// <summary>
	/// 射撃アクション：突風
	/// </summary>
	private void ShotAction_Windblow()
    {
    }
    ///<summary>
    ///射撃アクション:雪玉
    ///</summary>
    private void ShotAction_IceBall()
    {
        //弾の初速ベクトル設定
        Vector2 velocity = new Vector2(14.0f, 8.0f);
        if (!rightFacing)
            velocity.x *= -1.0f;

        //弾丸オブジェクト生成・設定
        GameObject obj = Instantiate(weaponBulletPrefabs[(int)ActorWeaponType.IceBall], transform.position, Quaternion.identity);
            obj.GetComponent<ActorNormalShot>().Init(
                0.0f, //速度(rigidbodyで弾を動かすので設定不要)
                0.0f, //角度(rigidbodyで球を動かすので設定不要)
                1, //ダメージ量
                5.0f, //存在時間
                nowWeapon); //使用武器
        obj.GetComponent<Rigidbody2D> ().linearVelocity += velocity;
    }

    ///<summary>
    ///射撃アクション:稲妻
    ///</summary>
    private void ShotAction_Lightning()
    {
        //弾の発射位置を設定（主人公の右上or左上）
        Vector3 fixPos = new Vector3(4.0f, 5.0f, 0.0f);
        if (!rightFacing)
            fixPos.x *= -1.0f;

        //弾丸オブジェクト生成・設定
        GameObject obj = Instantiate(weaponBulletPrefabs[(int)ActorWeaponType.Lightning], transform.position + fixPos, Quaternion.identity);
        obj.GetComponent<ActorNormalShot>().Init(
            14.0f, //速度
            270, //角度
            2, //ダメージ量
            5.0f, //存在時間
            nowWeapon); //使用武器
    }
    ///<summary>
    ///射撃アクション:水の輪
    ///</summary>
    private void ShotAction_WaterRing ()
    {
        //弾丸オブジェクト生成・設定
        int bulletNum_Angle = 8; //発射方向数
        for (int i = 0; i < bulletNum_Angle; i++)
        {
            GameObject obj = Instantiate(weaponBulletPrefabs[(int)ActorWeaponType.WaterRing], transform.position, Quaternion.identity);
            obj.GetComponent<ActorNormalShot>().Init(
                3.0f, //速度
                (360 / bulletNum_Angle) * i, //角度
                1, //ダメージ量
                2.0f, //存在時間
                nowWeapon); //使用武器
        }
    }
    ///<summary>
    ///射撃アクションレーザー
    ///</summary>
    private void ShotAction_Laser ()
    {
        ///レーザーオブジェクト生成・設定
        GameObject obj = Instantiate(weaponBulletPrefabs[(int)ActorWeaponType.Laser], transform.position, Quaternion.identity);
        obj.GetComponent<ActorLaser>().Init(
            1, //ダメージ量
            1.0f); //存在時間
    }
    #endregion
}