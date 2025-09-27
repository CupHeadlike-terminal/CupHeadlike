using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージギミック：ジャンプ台
/// </summary>
public class Gimmic_JumpBlock : MonoBehaviour
{
	// 設定項目
	[Header ("ジャンプ力")]
	public float JumpPower;

	// 各トリガー呼び出し処理
	// トリガー滞在時に呼出
	private void OnTriggerStay2D (Collider2D collision)
	{
		// 接しているのがアクターの接地判定オブジェクトでない場合は終了
		ActorGroundSensor actorGroundSensor = collision.gameObject.GetComponent<ActorGroundSensor> ();
		if (actorGroundSensor == null)
			return;

		// アクターを移動させる
		var rigidbody2D = collision.gameObject.GetComponentInParent<Rigidbody2D> ();
		rigidbody2D.linearVelocity += new Vector2 (0.0f, JumpPower);
	}
}