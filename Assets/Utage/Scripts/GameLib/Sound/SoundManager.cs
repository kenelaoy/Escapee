//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// サウンド管理
	/// </summary>
	[AddComponentMenu("Utage/Lib/Sound/SoundManager")]
	public class SoundManager : MonoBehaviour
	{
		public const string IdBgm = "Bgm";
		public const string IdAmbience = "Ambience";
		public const string IdVoice = "Voice";
		public const string IdSe = "Se";

		/// <summary>
		/// シングルトンなインスタンスの取得
		/// </summary>
		/// <returns></returns>
		public static SoundManager GetInstance()
		{
			if (null == instance)
			{
				instance = FindObjectOfType<SoundManager>();
			}
			return instance;
		}
		static SoundManager instance;
		
		//設定
		/// <summary>
		/// マスターボリューム
		/// </summary>
		public float MasterVolume
		{
			get { return this.masterVolume; }
			set {
				TryChangeFloat(ref masterVolume, value);
				System.ChangeMasterVolume(IdBgm, GetMasterVolume(IdBgm));
				System.ChangeMasterVolume(IdAmbience, GetMasterVolume(IdAmbience));
				System.ChangeMasterVolume(IdVoice, GetMasterVolume(IdVoice));
				System.ChangeMasterVolume(IdSe, GetMasterVolume(IdSe));
			}
		}
		[Range(0, 1), SerializeField]
		float masterVolume = 1.0f;

		/// <summary>
		/// BGMのボリューム
		/// </summary>
		public float BgmVolume
		{
			get { return this.bgmVolume; }
			set {
				TryChangeFloat(ref bgmVolume, value);
				System.ChangeMasterVolume(IdBgm, GetMasterVolume(IdBgm));
			}
		}
		[SerializeField, Range(0, 1)]
		float bgmVolume = 1.0f;

		/// <summary>
		/// 環境音のボリューム
		/// </summary>
		public float AmbienceVolume
		{
			get { return this.ambienceVolume; }
			set {
				TryChangeFloat(ref ambienceVolume, value);
				System.ChangeMasterVolume(IdAmbience, GetMasterVolume(IdAmbience));
			}
		}
		[SerializeField, Range(0, 1)]
		float ambienceVolume = 1.0f;

		/// <summary>
		/// ボイスのボリューム
		/// </summary>
		public float VoiceVolume
		{
			get { return this.voiceVolume; }
			set {
				TryChangeFloat(ref voiceVolume, value);
				System.ChangeMasterVolume(IdVoice, GetMasterVolume(IdVoice));
			}
		}
		[SerializeField, Range(0, 1)]
		float voiceVolume = 1.0f;

		/// <summary>
		/// SEのボリューム
		/// </summary>
		public float SeVolume
		{
			get { return this.seVolume; }
			set {
				TryChangeFloat(ref seVolume, value);
				System.ChangeMasterVolume(IdSe, GetMasterVolume(IdSe));
			}
		}
		[SerializeField, Range(0, 1)]
		float seVolume = 1.0f;

		/// <summary>
		/// 音声再生中はBGMを少し小さく鳴らすための補正値
		/// </summary>
		public float BgmVolumeFilterOfPlayingVoice
		{
			get { return this.bgmVolumeFilterOfPlayingVoice; }
			set { TryChangeFloat(ref bgmVolumeFilterOfPlayingVoice, value); }
		}
		[SerializeField, Range(0, 1)]
		float bgmVolumeFilterOfPlayingVoice = 0.5f;

		/// <summary>
		/// BGMなどのフェード時間のデフォルト値
		/// </summary>
		public float DefaultFadeTime
		{
			get { return this.defaultFadeTime; }
			set { TryChangeFloat(ref defaultFadeTime, value); }
		}
		[SerializeField]
		float defaultFadeTime = 0.2f;

		/// <summary>
		/// ボイスのフェード時間のデフォルト値
		/// </summary>
		public float DefaultVoiceFadeTime
		{
			get { return this.defaultVoiceFadeTime; }
			set { TryChangeFloat(ref defaultVoiceFadeTime, value); }
		}
		[SerializeField]
		float defaultVoiceFadeTime = 0.05f;

		/// <summary>
		/// ボリュームのデフォルト値
		/// </summary>
		public float DefaultVolume
		{
			get { return this.defaultVolume; }
			set { TryChangeFloat(ref defaultVolume, value); }
		}
		[SerializeField, Range(0, 1)]
		float defaultVolume = 1.0f;
		
		//現在のボイスのキャラクターラベル
		public string CurrentVoiceCharacterLabel { get; set; }


		[System.Serializable]
		public class SoundManagerEvent : UnityEvent<SoundManager> { }
		public SoundManagerEvent OnCreateSoundSytem
		{
			get { return onCreateSoundSystem; }
			set { onCreateSoundSystem = value; }
		}
		[SerializeField]
		SoundManagerEvent onCreateSoundSystem = new SoundManagerEvent();

		bool TryChangeFloat(ref float volume, float value)
		{
			if (Mathf.Approximately(volume, value)) return false;
			volume = value;
			return true;
		}

		internal float GetMasterVolume(string streamName)
		{
			switch (streamName)
			{
				case IdBgm:
					{
						float vol = MasterVolume * BgmVolume;
						if (System.IsPlaying(IdVoice))
						{
							vol *= BgmVolumeFilterOfPlayingVoice;
						}
						return vol;
					}
				case IdAmbience:
					return MasterVolume * AmbienceVolume;
				case IdVoice:
					return MasterVolume * VoiceVolume;
				case IdSe:
					return MasterVolume * SeVolume;					
				default:
					return MasterVolume;
			}
		}

#if UNITY_EDITOR
		void OnValidate()
		{
			if (!Application.isPlaying) return;
			System.ChangeMasterVolume(IdBgm, GetMasterVolume(IdBgm));
			System.ChangeMasterVolume(IdAmbience, GetMasterVolume(IdAmbience));
			System.ChangeMasterVolume(IdVoice, GetMasterVolume(IdVoice));
			System.ChangeMasterVolume(IdSe, GetMasterVolume(IdSe));
		}
#endif

		//実際の処理をするシステム部分
		public SoundManagerSysytemInterface System
		{
			get
			{
				if (system == null)
				{
					OnCreateSoundSytem.Invoke(this);
					if (system == null)
					{
						system = new SoundManagerSystem();
					}
					//BGMと環境音のみを再生
					List<string> saveStreamNameList = new List<string>( new string[]{ IdBgm, IdAmbience } );
					system.Init(this, saveStreamNameList);
				}
				return system;
			}
			set
			{
				system = value;
			}
		}
		SoundManagerSysytemInterface system;

		//************ BGM ************//
		public void PlayBgm(AudioClip clip, bool isLoop)
		{
			System.Play(IdBgm, clip, DefaultVolume, isLoop, true, 0,  DefaultFadeTime, false, () => GetMasterVolume(IdBgm));
		}

		public void PlayBgm(AssetFile file)
		{
			PlayBgm(file, 0, DefaultFadeTime );
		}

		public void PlayBgm(AssetFile file, float fadeInTime, float fadeOutTime)
		{
			System.Play( IdBgm, file, DefaultVolume, true, fadeInTime, fadeOutTime, false, ()=>GetMasterVolume(IdBgm) );
		}

		public void StopBgm()
		{
			StopBgm(DefaultFadeTime );
		}

		public void StopBgm(float fadeTime)
		{
			System.Stop(IdBgm, fadeTime);
		}

		//************ 環境音 ************//
		public void PlayAmbience(AssetFile file, bool isLoop)
		{
			PlayAmbience(file, isLoop, 0, DefaultFadeTime);
		}

		public void PlayAmbience(AssetFile file, bool isLoop, float fadeInTime, float fadeOutTime)
		{
			System.Play(IdAmbience, file, DefaultVolume, isLoop, fadeInTime, fadeOutTime, false, () => GetMasterVolume(IdAmbience));
		}

		public void PlayAmbience(AudioClip clip, bool isLoop)
		{
			PlayAmbience(clip, isLoop, 0, DefaultFadeTime);
		}

		public void PlayAmbience(AudioClip clip, bool isLoop, float fadeInTime, float fadeOutTime)
		{
			System.Play(IdAmbience, clip, DefaultVolume, isLoop, true, fadeInTime, fadeOutTime, false, () => GetMasterVolume(IdAmbience));
		}
		
		public void StopAmbience()
		{
			StopAmbience(DefaultFadeTime);
		}

		public void StopAmbience(float fadeTime)
		{
			System.Stop(IdAmbience, fadeTime);
		}

		//************ Voice ************//
		public void PlayVoice(string charcterLabel, AssetFile file)
		{
			PlayVoice(charcterLabel, file, false);
		}

		public void PlayVoice(string charcterLabel, AssetFile file, float fadeInTime, float fadeOutTime)
		{
			PlayVoice(charcterLabel, file, false, fadeInTime, fadeOutTime);
		}
		
		public void PlayVoice(string charcterLabel, AssetFile file, bool isLoop)
		{
			PlayVoice(charcterLabel, file, isLoop, 0, DefaultVoiceFadeTime);
		}

		public void PlayVoice(string charcterLabel, AssetFile file, bool isLoop, float fadeInTime, float fadeOutTime)
		{
			CurrentVoiceCharacterLabel = charcterLabel;
			System.Play(IdVoice, file, DefaultVolume, isLoop, fadeInTime, fadeOutTime, true, () => GetMasterVolume(IdVoice));
		}

		public void PlayVoice(string charcterLabel, AudioClip clip, bool isLoop)
		{
			PlayVoice(charcterLabel, clip, isLoop, 0, DefaultVoiceFadeTime);
		}

		public void PlayVoice(string charcterLabel, AudioClip clip, bool isLoop, float fadeInTime, float fadeOutTime)
		{
			CurrentVoiceCharacterLabel = charcterLabel;
			System.Play(IdVoice, clip, DefaultVolume, isLoop, true, fadeInTime, fadeOutTime, false, () => GetMasterVolume(IdVoice));
		}

		public void StopVoice()
		{
			StopVoice(DefaultVoiceFadeTime);
		}

		public void StopVoice(float fadeTime)
		{
			System.Stop(IdVoice, fadeTime);
		}

		public bool IsPlayingVoice()
		{
			return System.IsPlaying(IdVoice);
		}

		public bool IsPlayingLoopVoice()
		{
			return System.IsPlayingLoop(IdVoice);
		}

		internal bool IsPlayingVoice(string charcterLabel)
		{
			return (CurrentVoiceCharacterLabel == charcterLabel) && IsPlayingVoice();
		}

		internal float GetVoiceCurrentSamplesVolume()
		{
			return System.GetCurrentSamplesVolume(IdVoice);
		}


		//************ SE ************//

		/// <summary>
		/// SEの再生
		/// </summary>
		/// <param name="file">サウンドファイル</param>
		/// <returns>再生をしているサウンドストリーム</returns>
		public void PlaySe(AssetFile file, string label = "", SoundPlayMode playMode = SoundPlayMode.Add, bool isLoop = false)
		{
			PlaySe(file, DefaultVolume, label, playMode, isLoop);
		}

		/// <summary>
		/// SE再生
		/// </summary>
		/// <param name="file">サウンドファイル</param>
		/// <param name="volume">再生ボリューム</param>
		/// <returns>再生をしているサウンドストリーム</returns>
		public void PlaySe(AssetFile file, float volume, string label = "", SoundPlayMode playMode = SoundPlayMode.Add, bool isLoop = false)
		{
			System.PlaySe(file, volume * GetMasterVolume(IdSe), label, playMode, isLoop);
		}
		
		/// <summary>
		/// SEの再生
		/// </summary>
		/// <param name="clip">オーディオクリップ</param>
		/// <returns>再生をしているサウンドストリーム</returns>
		public void PlaySe(AudioClip clip, string label = "", SoundPlayMode playMode = SoundPlayMode.Add, bool isLoop = false)
		{
			PlaySe(clip, DefaultVolume, label, playMode, isLoop);
		}

		/// <summary>
		/// SE再生
		/// </summary>
		/// <param name="clip">オーディオクリップ</param>
		/// <param name="volume">再生ボリューム</param>
		/// <returns>再生をしているサウンドストリーム</returns>
		public void PlaySe(AudioClip clip, float volume, string label = "", SoundPlayMode playMode = SoundPlayMode.Add, bool isLoop = false)
		{
			System.PlaySe(clip, volume * GetMasterVolume(IdSe), label, playMode, isLoop);
		}

		public void StopSe(string label, float fadeTime)
		{
			System.StopSe(label, fadeTime);
		}


		//************ All ************//

		/// <summary>
		/// フェードアウトして曲全てを停止
		/// </summary>
		/// <param name="fadeTime">フェードアウトの時間</param>
		public void StopAll(float fadeTime)
		{
			System.StopAll(fadeTime);
		}

		//ロード中か
		public bool IsLoading { get { return System.IsLoading; } }

		/// <summary>
		/// セーブデータ用のバイナリ変換
		/// 再生中のBGMのファイル情報などをバイナリ化
		/// </summary>
		/// <returns>データのバイナリ</returns>
		public byte[] ToSaveDataBuffer()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				//バイナリ化
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					System.WriteSaveData(writer);
				}
				return stream.ToArray();
			}
		}

		/// <summary>
		/// セーブデータを読みこみ
		/// </summary>
		/// <param name="buffer">セーブデータのバイナリ</param>
		public void ReadSaveDataBuffer(byte[] buffer)
		{
			StopAll(0.1f);
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					System.ReadSaveDataBuffer(reader);
				}
			}
		}

		void LateUpdate()
		{
			System.LateUpdate();
		}
	}
}