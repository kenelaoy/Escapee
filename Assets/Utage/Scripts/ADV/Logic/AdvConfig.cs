//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.IO;
using UnityEngine;

namespace Utage
{
	/// <summary>
	/// ゲームのコンフィグ
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/Config")]
	public class AdvConfig : MonoBehaviour
	{
		[SerializeField]
		bool dontUseSystemSaveData = false;		//システムセーブデータを使わない

		[SerializeField]
		float sendCharWaitSecMax = 1.0f / 10;	//一文字送りの待ち時間の最大値
		[SerializeField]
		float autoPageWaitSecMax = 2.5f;		//オート改ページ待ち時間の最大値
		[SerializeField]
		float autoPageWaitSecMin = 0f;			//オート改ページ待ち時間の最小値
		[SerializeField]
		float bgmVolumeFilterOfPlayingVoice = 0.5f;	//音声再生時のBGMボリューム調整
		[SerializeField]
		bool forceSkipInputCtl= true;			//CTRL入力で強制スキップ

		/// <summary>
		/// スキップ中の演出速度の倍率
		/// </summary>
		public float SkipSpped { get { return skipSpeed; } }

		[UnityEngine.Serialization.FormerlySerializedAs("skipSpped"), SerializeField]
		float skipSpeed = 20.0f;

		public bool SkipVoiceAndSe { get { return skipVoiceAndSe; } }
		[SerializeField]
		bool skipVoiceAndSe = false;

		[SerializeField]
		AdvConfigSaveData defaultData;

		AdvConfigSaveData current = new AdvConfigSaveData();


		/// <summary>
		/// 初回（セーブデータがない場合）のみの初期化
		/// </summary>
		public void InitDefault()
		{
			SetData(defaultData,false);
		}

		/// <summary>
		/// バイナリ読み込み
		/// </summary>
		/// <param name="reader">バイナリリーダー</param>
		public void Read(BinaryReader reader)
		{
			AdvConfigSaveData data = new AdvConfigSaveData();
			data.Read(reader);
			if (!dontUseSystemSaveData)
			{
				SetData(data, false);
			}
			else
			{
				InitDefault();
			}
		}

		/// <summary>
		/// バイナリ書き込み
		/// </summary>
		/// <param name="writer">バイナリライーター</param>
		public void Write(BinaryWriter writer)
		{
			current.Write(writer);
		}

		/// <summary>
		/// 全てデフォルト値で初期化
		/// </summary>
		public void InitDefaultAll()
		{
			SetData(defaultData, true);
		}

		//データを設定
		void SetData(AdvConfigSaveData data, bool isSetDefault)
		{
			if (UtageToolKit.IsPlatformStandAloneOrEditor())
			{
				//PC版のみ、フルスクリーン切り替え
				IsFullScreen = data.isFullScreen;
			}
			IsMouseWheelSendMessage = data.isMouseWheelSendMessage;

			//エフェクトON・OFF切り替え
			IsEffect = data.isEffect;
			//未読スキップON・OFF切り替え
			IsSkipUnread = data.isSkipUnread;
			//選択肢でスキップ解除ON・OFF切り替え
			IsStopSkipInSelection = data.isStopSkipInSelection;
			//文字送り速度
			MessageSpeed = data.messageSpeed;
			//オート改ページ速度
			AutoBrPageSpeed = data.autoBrPageSpeed;
			//メッセージウィンドウの透過色
			MessageWindowTransparency = data.messageWindowTransparency;
			//音量設定 サウンド全体
			SoundMasterVolume = data.soundMasterVolume;
			//音量設定 BGM
			BgmVolume = data.bgmVolume;
			//音量設定 SE
			SeVolume = data.seVolume;
			//音量設定 環境音
			AmbienceVolume = data.ambienceVolume;
			//音量設定 ボイス
			VoiceVolume = data.voiceVolume;
			//音声設定（クリックで停止、次の音声まで再生を続ける）
			VoiceStopType = data.voiceStopType;

			int max = data.isVoiceActiveArray.Length;
			current.isVoiceActiveArray = new bool[max];
			for (int i = 0; i < max; i++)
			{
				SetVoiceActive(i, data.isVoiceActiveArray[i]);
			}

			if (!isSetDefault)
			{
				//コンフィグ例外（コンフィグ画面でのデフォルトに戻すに関与しない）
				IsAutoBrPage = data.isAutoBrPage;						//オート改ページ
			}
		}

		/// <summary>
		/// フルスクリーンか
		/// </summary>
		public bool IsFullScreen{
			get { return current.isFullScreen; }
			set {
				if (UtageToolKit.IsPlatformStandAloneOrEditor())
				{
					//PC版のみ、フルスクリーン切り替え
#if UNITY_5
					Unity5ChangeScreen(value);
#else
					Screen.fullScreen = value;
#endif
					current.isFullScreen = value;
				}
			}
		}
#if UNITY_5
		//ウィンドウサイズを戻すための処理
		bool isSavedWindowSize = false;
		int windowWidth;
		int windowHeight;
		void Start()
		{
			windowWidth = Screen.width;
			windowHeight = Screen.height;
			isSavedWindowSize = true;
		}

		void Unity5ChangeScreen(bool fullScreen)
		{
			if (!fullScreen)
			{
				LoadWindowSize();
			}
			else
			{
//				SaveWindowSize();
				Screen.fullScreen = true;
			}
		}
/*		void SaveWindowSize()
		{
			if (!Screen.fullScreen && !current.isFullScreen)
			{
				windowWidth = Screen.width;
				windowHeight = Screen.height;
				isSavedWindowSize = true;
			}
		}
*/		void LoadWindowSize()
		{
			if (isSavedWindowSize)
			{
				Screen.SetResolution(windowWidth, windowHeight, false);
			}
			else
			{
				Screen.fullScreen = false;
			}
		}
#endif


		/// <summary>
		/// フルスクリーン切り替え
		/// </summary>
		public void ToggleFullScreen()
		{
			IsFullScreen = !IsFullScreen;
		}

		/// <summary>
		/// マウスホイールでメッセージ送り切り替えるか
		/// </summary>
		public bool IsMouseWheelSendMessage
		{
			get { return current.isMouseWheelSendMessage; }
			set	{ current.isMouseWheelSendMessage = value; }
		}		
		/// <summary>
		/// マウスホイールでメッセージ送り切り替え
		/// </summary>
		public void ToggleMouseWheelSendMessage()
		{
			IsMouseWheelSendMessage = !IsMouseWheelSendMessage;
		}		

		/// <summary>
		/// エフェクトが有効か
		/// </summary>
		public bool IsEffect
		{ 
			get { return current.isEffect; }
			set { current.isEffect = value; }
		}
		/// <summary>
		/// エフェクトON・OFF切り替え
		/// </summary>
		public void ToggleEffect()
		{
			IsEffect = !IsEffect;
		}


		/// <summary>
		/// 未読スキップが有効か
		/// </summary>
		public bool IsSkipUnread { 
			get { return current.isSkipUnread; }
			set { current.isSkipUnread = value; }
		}
		/// <summary>
		/// 未読スキップON・OFF切り替え
		/// </summary>
		public void ToggleSkipUnread()
		{
			IsSkipUnread = !IsSkipUnread;
		}

		/// <summary>
		/// 選択肢でスキップ解除するか
		/// </summary>
		public bool IsStopSkipInSelection
		{
			get { return current.isStopSkipInSelection; }
			set { current.isStopSkipInSelection = value; }
		}
		/// <summary>
		/// 選択肢でスキップ解除ON・OFF切り替え
		/// </summary>
		public void ToggleStopSkipInSelection()
		{
			IsStopSkipInSelection = !IsStopSkipInSelection;
		}
		
		/// <summary>
		/// 文字送り速度
		/// </summary>
		public float MessageSpeed
		{ 
			get { return current.messageSpeed; }
			set { current.messageSpeed = value; }
		}
		/// <summary>
		/// 一文字進めるのにかかる時間
		/// </summary>
		public float TimeSendChar { get { return (1.0f - MessageSpeed ) * sendCharWaitSecMax; } }

		/// <summary>
		/// オート改ページ速度
		/// </summary>
		public float AutoBrPageSpeed
		{
			get { return current.autoBrPageSpeed; }
			set { current.autoBrPageSpeed = value; }
		}
		/// <summary>
		/// オート改ページの待ち時間
		/// </summary>
		public float AutoPageWaitTime
		{
			get { return (1.0f - AutoBrPageSpeed) * (autoPageWaitSecMax - autoPageWaitSecMin) + autoPageWaitSecMin; }
		}

		/// <summary>
		/// メッセージウィンドウの透過度
		/// </summary>
		/// <returns></returns>
		public float MessageWindowTransparency
		{
			get { return current.messageWindowTransparency; }
			set { current.messageWindowTransparency = value; }
		}
		/// <summary>
		/// メッセージウィンドウのアルファ値（不透明度）
		/// </summary>
		public float MessageWindowAlpha { get { return 1.0f - MessageWindowTransparency; } }


		/// <summary>
		/// サウンド全体のボリューム
		/// </summary>
		public float SoundMasterVolume
		{
			get { return current.soundMasterVolume; }
			set
			{
				current.soundMasterVolume = value;
				SoundManager manager = SoundManager.GetInstance();
				if (manager)
				{
					manager.MasterVolume = value;
				}
			}
		}

		/// <summary>
		/// BGMのボリューム
		/// </summary>
		public float BgmVolume
		{
			get { return current.bgmVolume; }
			set
			{
				current.bgmVolume = value;
				SoundManager manager = SoundManager.GetInstance();
				if (manager)
				{
					manager.BgmVolume = value;
					manager.BgmVolumeFilterOfPlayingVoice = bgmVolumeFilterOfPlayingVoice;
				}
			}
		}

		/// <summary>
		/// SEのボリューム
		/// </summary>
		public float SeVolume
		{
			get { return current.seVolume; }
			set
			{
				current.seVolume = value;
				SoundManager manager = SoundManager.GetInstance();
				if (manager)
				{
					manager.SeVolume = value;
				}
			}
		}

		/// <summary>
		/// 環境音のボリューム
		/// </summary>
		public float AmbienceVolume
		{
			get { return current.ambienceVolume; }
			set
			{
				current.ambienceVolume = value;
				SoundManager manager = SoundManager.GetInstance();
				if (manager)
				{
					manager.AmbienceVolume = value;
				}
			}
		}

		/// <summary>
		/// ボイスのボリューム
		/// </summary>
		public float VoiceVolume
		{
			get { return current.voiceVolume; }
			set
			{
				current.voiceVolume = value;
				SoundManager manager = SoundManager.GetInstance();
				if (manager)
				{
					manager.VoiceVolume = value;
				}
			}
		}

		/// <summary>
		/// ボイスの止め方
		/// </summary>
		public VoiceStopType VoiceStopType
		{
			get { return current.voiceStopType; }
			set { current.voiceStopType = value; }
		}

		/// <summary>
		/// キャラ別ボイスのON・OFF（未使用）
		/// </summary>
		/// <param name="index">キャラのインデックス</param>
		void ToggleVoiceActive(int index)
		{
			SetVoiceActive(index, GetVoiceActive(index));
		}
		bool GetVoiceActive(int index)
		{
			return current.isVoiceActiveArray[index];
		}
		void SetVoiceActive(int index, bool val)
		{
			current.isVoiceActiveArray[index] = val;
			// TODO: キャラ別ボイスのON・OFF
		}

		/// <summary>
		/// スキップのチェック
		/// </summary>
		/// <param name="isReadPage">既読のページかどうか</param>
		/// <returns>スキップする</returns>
		public bool CheckSkip( bool isReadPage )
		{
			if (forceSkipInputCtl && InputUtil.IsInputControl())
			{
				//強制スキップ
				return true;
			}
			else if (isSkip)
			{
				if ( IsSkipUnread)
				{	//未読でもスキップ
					return true;
				}
				else
				{	//既読スキップ
					return isReadPage;
				}
			}
			return false;
		}

		//スキップフラグ
		public bool IsSkip
		{
			get { return isSkip; }
			set { isSkip = value; }
		}
		bool isSkip = false;

		/// <summary>
		/// スキップのON・OFF切り替え
		/// </summary>
		public void ToggleSkip()
		{
			isSkip = !isSkip;
		}

		/// <summary>
		/// 選択肢の最後でのスキップ解除処理
		/// </summary>
		public void StopSkipInSelection()
		{
			if (IsStopSkipInSelection && isSkip)
			{
				isSkip = false;
			}
		}

		/// <summary>
		/// 自動メッセージ送り
		/// </summary>
		public bool IsAutoBrPage
		{
			get { return current.isAutoBrPage; }
			set { current.isAutoBrPage = value; }
		}
		/// <summary>
		/// 自動メッセージ送り切り替え
		/// </summary>
		public void ToggleAuto()
		{
			IsAutoBrPage = !IsAutoBrPage;
		}
	}
}