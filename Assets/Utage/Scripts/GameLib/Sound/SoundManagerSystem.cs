//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// サウンド管理
	/// </summary>
	public class SoundManagerSystem : SoundManagerSysytemInterface
	{
		const string GameObjectNameSe = "One shot audio";

		List<string> saveStreamNameList = new List<string>();
		Dictionary<string, SoundStreamFade> streamTbl = new Dictionary<string, SoundStreamFade>();	//BGM等のストリーム
		List<SoundSe> curretFrameSeList = new List<SoundSe>();	//今のフレームで鳴らしたSEのリスト
		List<SoundSe> seList = new List<SoundSe>();				//現在管理中のSEリスト

		//サウンドマネージャー
		SoundManager SoundMangaer { get; set; }

		Transform CachedTransform { get; set; }

		public void Init(SoundManager soundMangaer, List<string> saveStreamNameList)
		{
			SoundMangaer = soundMangaer;
			CachedTransform = SoundMangaer.transform;
			this.saveStreamNameList = saveStreamNameList;
		}


		public void Play(string streamName, AssetFile file, float volume, bool isLoop, float fadeInTime, float fadeOutTime, bool isReplay, Func<float> callbackMasterVolume)
		{
			if (file.Sound == null)
			{
				Debug.LogError("sound file is not loaded " + file.FileName);
				return;
			}
			bool isStreaming = (file.LoadFlags & AssetFileLoadFlags.Streaming) == AssetFileLoadFlags.Streaming;
			SoundStream stream = PlaySub(streamName, file.Sound, volume, isLoop, isStreaming, fadeInTime, fadeOutTime, isReplay, callbackMasterVolume);
			file.AddReferenceComponet(stream.gameObject);
		}

		public void Play(string streamName, AudioClip clip, float volume, bool isLoop, bool isStreaming, float fadeInTime, float fadeOutTime, bool isReplay, Func<float> callbackMasterVolume)
		{
			PlaySub(streamName, clip, volume, isLoop, isStreaming, fadeInTime, fadeOutTime, isReplay, callbackMasterVolume);
		}

		SoundStream PlaySub(string streamName, AudioClip clip, float volume, bool isLoop, bool isStreaming, float fadeInTime, float fadeOutTime, bool isReplay, Func<float> callbackMasterVolume)
		{
			SoundStreamFade stream = GetStreamAndCreateIfMissing(streamName);
			if (isReplay || !stream.IsPlaying(clip))
			{
				return stream.Play(clip, fadeInTime, fadeOutTime, volume, isLoop, isStreaming, callbackMasterVolume);
			}
			else
			{
				return stream.Current;
			}
		}

		public void Stop(string streamName, float fadeTime)
		{
			SoundStreamFade stream = GetStreamAndCreateIfMissing(streamName);
			stream.Stop(fadeTime);
		}

		public bool IsPlaying(string streamName)
		{
			SoundStreamFade stream = GetStream(streamName);
			if (stream == null) return false;
			return stream.IsPlaying();
		}

		public bool IsPlayingLoop(string streamName)
		{
			SoundStreamFade stream = GetStream(streamName);
			if (stream == null) return false;
			return stream.IsPlayingLoop();
		}

		public float GetCurrentSamplesVolume(string streamName)
		{
			SoundStreamFade stream = GetStreamAndCreateIfMissing(streamName);
			return stream.GetCurrentSamplesVolume();
		}

		public void StopAll(float fadeTime)
		{
			foreach (var stream in streamTbl.Values)
			{
				stream.Stop(fadeTime);
			}
		}

		public void PlaySe(AssetFile file, float volume, string label, SoundPlayMode playMode = SoundPlayMode.Add, bool isLoop = false)
		{
			SoundSe audio = PlaySeSub(file.Sound, volume, label, playMode, isLoop);
			if (audio)
			{
				file.AddReferenceComponet(audio.gameObject);
			}
		}

		public void PlaySe(AudioClip clip, float volume, string label, SoundPlayMode playMode = SoundPlayMode.Add, bool isLoop = false)
		{
			PlaySeSub(clip, volume, label, playMode, isLoop);
		}

		// SE再生
		SoundSe PlaySeSub(AudioClip clip, float volume, string label, SoundPlayMode playMode, bool isLoop)
		{
			//音量0なので、鳴らさない
			if (volume <= 0) return null;
			//同一フレームで既に鳴っているので鳴らさない（多重再生防止）
			foreach (SoundSe audio in curretFrameSeList)
			{
				if (clip == audio.AudioSource.clip)
				{
					return null;
				}
			}

			SoundSe se = PlaySeClip(clip, volume, label, playMode, isLoop);
			if (se == null) return null;

			curretFrameSeList.Add(se);
			seList.Add(se);
			return se;
		}

		//オーディオクリップをSEとして再生
		SoundSe PlaySeClip(AudioClip clip, float volume, string label, SoundPlayMode playMode, bool isLoop)
		{
			if (string.IsNullOrEmpty(label))
			{
				label = clip.name;
			}

			switch (playMode)
			{
				case SoundPlayMode.Add:
					break;
				case SoundPlayMode.Cancel:
					if (IsPlayingSe(label))
					{
						Debug.Log("Cancel");
						StopSe(label,0.02f);
					}
					break;
				case SoundPlayMode.NoPlay:
					if (IsPlayingSe(label))
					{
						return null;
					}
					break;
			}
			GameObject go = UtageToolKit.AddChild(CachedTransform, new GameObject(label));
			SoundSe se = go.AddComponent<SoundSe>();
			se.Init(clip, volume, isLoop);
			return se;
		}

		//毎フレームのSeの更新
		void UpdateSe()
		{
			curretFrameSeList.Clear();
			seList.RemoveAll(x => (x == null));
		}

		public void StopSe(string label, float fadeTime)
		{
			foreach (var se in seList)
			{
				if (se.gameObject.name != label) continue;

				se.FadeOut(fadeTime);
			}
		}

		/// <summary>
		/// SE再生中か
		/// </summary>
		bool IsPlayingSe(string label)
		{
			foreach (var se in seList)
			{
				if (se.IsPlaying(label))
				{
					return true;
				}
			}
			return false;
		}

		public void LateUpdate()
		{
			UpdateSe();
		}

		public bool IsLoading
		{
			get
			{
				foreach (var stream in streamTbl.Values)
				{
					if (stream.IsLoading)
					{
						return true;
					}
				}
				return false;
			}
		}

		/// マスターボリュームの変更
		public void ChangeMasterVolume(string name, float volume)
		{
		}


		const int VERSION = 1;
		const int VERSION_0 = 0;
		//セーブデータ用のバイナリ書き込み
		public void WriteSaveData(BinaryWriter writer)
		{
			writer.Write(VERSION);
			writer.Write(saveStreamNameList.Count);
			foreach (string saveStreamName in saveStreamNameList)
			{
				writer.Write(saveStreamName);
				SoundStreamFade stream = GetStreamAndCreateIfMissing(saveStreamName);
				stream.WriteSaveData(writer);
			}
		}

		//セーブデータ用のバイナリ読み込み
		public void ReadSaveDataBuffer(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			if (version == VERSION)
			{
				int count = reader.ReadInt32();
				for (int i = 0; i < count; ++i)
				{
					string streamName = reader.ReadString();
					SoundStreamFade stream = GetStreamAndCreateIfMissing(streamName);
					stream.ReadSaveData(reader, () => SoundMangaer.GetMasterVolume(streamName));
				}
			}
			else if (version == VERSION_0)
			{
				//BGMと環境音のみを再生
				GetStreamAndCreateIfMissing(SoundManager.IdBgm).ReadSaveData(reader, () => SoundMangaer.GetMasterVolume(SoundManager.IdBgm));
				GetStreamAndCreateIfMissing(SoundManager.IdAmbience).ReadSaveData(reader, () => SoundMangaer.GetMasterVolume(SoundManager.IdAmbience));
			}
			else
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
			}
		}


		//指定の名前のストリームを取得。なければ作成。
		SoundStreamFade GetStreamAndCreateIfMissing(string name)
		{
			SoundStreamFade stream;
			if (!streamTbl.TryGetValue(name, out stream))
			{
				stream = UtageToolKit.AddChildGameObjectComponent<SoundStreamFade>(CachedTransform, name);
				streamTbl.Add(name, stream);
			}
			return stream;
		}

		//指定の名前のストリームを取得
		SoundStreamFade GetStream(string name)
		{
			SoundStreamFade stream;
			if (!streamTbl.TryGetValue(name, out stream))
			{
				return null;
			}
			return stream;
		}
	}
}