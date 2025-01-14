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
	public enum SoundPlayMode
	{
		Add,
		Cancel,
		NoPlay,
//		CrossFade,
	};

	/// <summary>
	/// サウンド管理
	/// </summary>
	public interface SoundManagerSysytemInterface
	{
		void Init(SoundManager soundMangaer, List<string> saveStreamNameList);

		/// <summary>
		/// サウンドの再生
		/// </summary>
		/// <param name="type">サウンドのタイプ</param>
		/// <param name="file">サウンドファイル</param>
		/// <param name="isLoop">ループ再生するか</param>
		/// <param name="fadeTime">フェード時間</param>
		/// <param name="isReplay">直前が同じサウンドなら鳴らしなおすか</param>
		void Play(string streamName, AssetFile file, float volume, bool isLoop, float fadeInTime, float fadeOutTime, bool isReplay, Func<float> callbackMasterVolume);

		/// <summary>
		/// サウンドの再生
		/// </summary>
		/// <param name="type">サウンドのタイプ</param>
		/// <param name="clip">オーディオクリップ</param>
		/// <param name="isLoop">ループ再生するか</param>
		/// <param name="isStreaming">ストリーム再生するか</param>
		/// <param name="fadeTime">フェード時間</param>
		/// <param name="isReplay">直前が同じサウンドなら鳴らしなおすか</param>
		void Play(string streamName, AudioClip clip, float volume, bool isLoop, bool isStreaming, float fadeInTime, float fadeOutTime, bool isReplay, Func<float> callbackMasterVolume);

		/// <summary>
		/// フェードアウトして曲を停止
		/// </summary>
		/// <param name="type">タイプ</param>
		/// <param name="fadeTime">フェードする時間</param>
		void Stop(string streamName, float fadeTime);

		/// <summary>
		/// 指定のサウンドが鳴っているか
		/// </summary>
		/// <param name="type">タイプ</param>
		/// <returns>鳴っていればtrue、鳴っていなければfalse</returns>
		bool IsPlaying(string streamName);

		/// <summary>
		/// 指定のサウンドがループで鳴っているか
		/// </summary>
		/// <param name="type">タイプ</param>
		/// <returns>鳴っていればtrue、鳴っていなければfalse</returns>
		bool IsPlayingLoop(string streamName);

		/// <summary>
		/// 現在のボリュームを波形から計算して取得
		/// </summary>
		float GetCurrentSamplesVolume(string streamName);

		/// <summary>
		/// フェードアウトして曲全てを停止
		/// </summary>
		/// <param name="fadeTime">フェードアウトの時間</param>
		void StopAll(float fadeTime);

		/// <summary>
		/// SEの再生
		/// </summary>
		/// <param name="clip">オーディオクリップ</param>
		/// <param name="volume">再生ボリューム</param>
		/// <returns>再生をしているAudioSource</returns>
		void PlaySe(AudioClip clip, float volume, string label, SoundPlayMode playMode = SoundPlayMode.Add, bool isLoop = false);

		/// <summary>
		/// SEの再生
		/// </summary>
		/// <param name="volume">再生ボリューム</param>
		/// <param name="file">サウンドファイル</param>
		/// <returns>再生をしているAudioSource</returns>
		void PlaySe(AssetFile file, float volume, string label, SoundPlayMode playMode = SoundPlayMode.Add, bool isLoop = false);

		/// <summary>
		/// SEの停止
		/// </summary>
		void StopSe(string clipName, float fadeTime);

		/// <summary>
		/// 毎フレームの更新
		/// </summary>
		void LateUpdate();

		/// <summary>
		/// セーブデータ用のバイナリ変換
		/// 再生中のBGMのファイル情報などをバイナリ化
		/// </summary>
		void WriteSaveData(BinaryWriter writer);

		/// <summary>
		/// セーブデータを読みこみ
		/// </summary>
		void ReadSaveDataBuffer(BinaryReader reader);

		/// ロード中か
		bool IsLoading { get; }

		/// マスターボリュームの変更
		void ChangeMasterVolume(string name, float volume);
	}
}