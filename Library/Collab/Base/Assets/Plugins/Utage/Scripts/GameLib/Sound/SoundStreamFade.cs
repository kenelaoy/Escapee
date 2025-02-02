using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// フェード処理に対応するサウンド再生ストリーム
	/// 基本はシステム内部で使うので外から呼ばないこと
	/// </summary>
	[AddComponentMenu("Utage/Lib/Sound/SoundStreamFade")]
	internal class SoundStreamFade : MonoBehaviour
	{
		public SoundStream Current { get { return current; } }
		SoundStream current;
		SoundStream last;

		//曲が終わっているか
		public bool IsStop()
		{
			if (null != current) return false;
			if (null != last) return false;
			return true;
		}

		//指定のサウンドが再生中か
		public bool IsPlaying(AudioClip clip)
		{
			if (null == current) return false;
			else
			{
				return current.IsPlaying(clip);
			}
		}

		//再生中か
		public bool IsPlaying()
		{
			return (current != null && current.IsPlaying());
		}

		//ループ再生中か
		public bool IsPlayingLoop()
		{
			return (current != null && current.IsPlaying() && current.IsLoop);
		}


		//再生（直前のBGMをフェードアウトしてから再生）
		internal SoundStream Play(AudioClip clip, float fadeInTime, float fadeOutTime, float volume, bool isLoop, bool isStreaming, Func<float> callbackMasterVolume)
		{
			return Play(clip, fadeInTime, fadeOutTime, volume, isLoop, isStreaming, callbackMasterVolume, null);
		}
		internal SoundStream Play(AudioClip clip, float fadeInTime, float fadeOutTime, float volume, bool isLoop, bool isStreaming, Func<float> callbackMasterVolume, Action callbackEnd)
		{
			if (null != last) GameObject.Destroy(last.gameObject);
			
			if (null == current)
			{
				current = UtageToolKit.AddChildGameObjectComponent<SoundStream>(this.transform, clip.name);
				//即時再生
				current.Play(clip, fadeInTime, volume, isLoop, isStreaming, callbackMasterVolume, callbackEnd);
				return current;
			}
			else
			{
				//フェードアウト後に再生
				last = current;
				last.FadeOut(fadeOutTime);
				current = UtageToolKit.AddChildGameObjectComponent<SoundStream>(this.transform, clip.name);
				current.Ready(clip, fadeInTime, volume, isLoop, isStreaming, callbackMasterVolume, callbackEnd);
				return current;
			}
		}
		//サウンドを終了
		public void Stop(float fadeTime)
		{
			if (null != last) last.End();
			if (null != current) current.FadeOut(fadeTime);
		}

		void Update()
		{
			if (null != last) last.Update();
			if (null != current) current.Update();

			if (null != current)
			{
				if (current.IsReady() && last == null)
				{
					current.Play();
				}
			}
		}

		const int VERSION = 0;
		//セーブデータ用のバイナリ書き込み
		public void WriteSaveData(BinaryWriter writer)
		{
			if( IsLoading )
			{
				Debug.LogError("Cant save when loading sound");
			}
			writer.Write(VERSION);
			if (current != null && (current.IsPlaying() || current.IsReady() ))
			{
				writer.Write(current.FileName);
				writer.Write(current.RequestVolume);
				writer.Write(current.IsLoop);
				writer.Write(current.IsStreaming);
			}
			else
			{
				writer.Write("");
				writer.Write(0.0f);
				writer.Write(false);
				writer.Write(false);
			}
		}
		//セーブデータ用のバイナリ読み込み
		public void ReadSaveData(BinaryReader reader, Func<float> callbackMasterVolume)
		{
			int version = reader.ReadInt32();
			if (version == VERSION)
			{
				string fileName = reader.ReadString();
				float volume = reader.ReadSingle();
				bool isLoop = reader.ReadBoolean();
				bool isStreaming = reader.ReadBoolean();
				isLoading = true;
				StartCoroutine(CoLoadAndPlayFile(fileName, volume, isLoop, isStreaming, callbackMasterVolume));
			}
			else
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion,version));
			}
		}

		//指定のファイル名をロードして鳴らす
		IEnumerator CoLoadAndPlayFile(string path, float volume, bool isLoop, bool isStreaming, Func<float> callbackMasterVolume)
		{
			if (!string.IsNullOrEmpty(path))
			{
				AssetFile file = AssetFileManager.GetFileCreateIfMissing(path);
				if (isStreaming) file.AddLoadFlag( AssetFileLoadFlags.Streaming );
				AssetFileManager.Load(file, this);
				while (!file.IsLoadEnd) yield return 0;
				SoundStream stream = Play(file.Sound, 0.1f, 0.1f, volume, isLoop, isStreaming, callbackMasterVolume);
				file.AddReferenceComponet(stream.gameObject);
				file.Unuse(this);
			}
			isLoading = false;
		}

		internal float GetCurrentSamplesVolume()
		{
			return IsPlaying() ? current.GetCurrentSamplesVolume(): 0;
		}

		public bool IsLoading { get { return isLoading; } }
		bool isLoading;
	}
}