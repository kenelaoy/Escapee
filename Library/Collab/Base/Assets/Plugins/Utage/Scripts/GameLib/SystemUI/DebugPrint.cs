using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace Utage
{
	/// <summary>
	/// デバッグ情報表示表示
	/// </summary>
	[AddComponentMenu("Utage/Lib/System UI/DebugPrint")]
	public class DebugPrint : MonoBehaviour
	{
		public const string Version = "2.6.12";

		//シングルトンなインスタンス
		static DebugPrint GetInstance()
		{
			if (null == instance)
			{
				instance = (DebugPrint)FindObjectOfType(typeof(DebugPrint));
			}
			return instance;
		}
		static DebugPrint instance;

		/// <summary>
		/// デバッグログの追加
		/// </summary>
		/// <param name="message">デバッグログ</param>
		static public void Log(object message)
		{
			GetInstance().AddLog(message as string);
		}

		/// <summary>
		/// デバッグエラーログの追加
		/// </summary>
		/// <param name="message">デバッグエラーログ</param>
		static public void LogError(object message)
		{
			GetInstance().AddLogError(message as string);
		}

		/// <summary>
		/// デバッグ例外ログの追加
		/// </summary>
		/// <param name="ex">例外</param>
		static public void LogException(System.Exception ex)
		{
			GetInstance().AddLogError(ex.Message);
		}


		/// <summary>
		/// デバッグワーニングログの追加
		/// </summary>
		/// <param name="message">デバッグワーニングログ</param>
		static public void LogWarning(object message)
		{
			GetInstance().AddLogWarning(message as string);
		}

		/// <summary>
		/// デバッグログの文字列取得
		/// </summary>
		/// <returns></returns>
		static public string GetLogString()
		{
			string tmp = "";
			foreach (string log in GetInstance().logList)
			{
				tmp += log + "\n";
			}
			return tmp;
		}
		List<string> GetLogList()
		{
			return GetInstance().logList;
		}

		/// <summary>
		/// デバッグ情報の文字列取得
		/// </summary>
		/// <returns></returns>
		static public string GetDebugString()
		{
			return
				GetInstance().VersionString()
				+ GetInstance().FpsToString()
				+ GetInstance().MemToString();
		}

		List<string> logList = new List<string>();
		float oldTime;
		int frame = 0;
		float frameRate = 0f;
		const float INTERVAL = 1.0f; // この時間おきにFPSを計算して表示させる

		float memSizeSystem;			//割り当て可能な最大メモリサイズ
		float memSizeGraphic;			//割り当て可能な最大グラフィック・メモリサイズ
		float memSizeUsedHeap;			//使用中のヒープ・メモリサイズ
		float memSizeGC;				//GC管理下のメモリサイズ
		float memSizeMonoHeap;			//monoのヒープ・メモリサイズ
		float memSizeMonoUsedHeap;		//未使用のmonoのヒープ・メモリサイズ

		//バージョン情報表示文字列取得
		string VersionString()
		{
			return "UTAGE version " + Version + "\n";
		}
		
		//FPS表示文字列取得
		string FpsToString()
		{
			return "FPS:" + frameRate.ToString() + "\n";
		}
		//メモリ情報表示文字列取得
		string MemToString()
		{
			return "Mem:\n"
				+ "System " + memSizeSystem.ToString() + "\n"
				+ "Graphic " + memSizeGraphic.ToString() + "\n"
				+ "GC " + memSizeGC.ToString() + "\n"
				+ "UsedHeap " + memSizeUsedHeap.ToString() + "\n"
				+ "MonoHeap " + memSizeMonoHeap.ToString() + "\n"
				+ "MonoUsedHeap " + memSizeMonoUsedHeap.ToString() + "\n"
				;
		}

		void Awake()
		{
			if (null == instance) instance = this;
		}

		void Start()
		{
			oldTime = Time.realtimeSinceStartup;
			Debug.Log("Utage Ver " + DebugPrint.Version +  " Start!");
		}

		void Update()
		{
			UpdateFPS();
			UpdateMemSize();
		}

		void UpdateFPS()
		{
			frame++;
			float time = Time.realtimeSinceStartup - oldTime;
			if (time >= INTERVAL)
			{
				// この時点でtime秒あたりのframe数が分かる
				// time秒を1秒あたりに変換したいので、frame数からtimeを割る
				frameRate = frame / time;
				oldTime = Time.realtimeSinceStartup;
				frame = 0;
			}
		}

		void UpdateMemSize()
		{
			memSizeSystem = SystemInfo.systemMemorySize;
			memSizeGraphic = SystemInfo.graphicsMemorySize;
			memSizeGC = 1.0f * System.GC.GetTotalMemory(false) / 1024 / 1024;
			memSizeUsedHeap = 1.0f * Profiler.usedHeapSize / 1024 / 1024;
			memSizeMonoHeap = 1.0f * Profiler.GetMonoHeapSize() / 1024 / 1024; ;
			memSizeMonoUsedHeap = 1.0f * Profiler.GetMonoUsedSize() / 1024 / 1024; ;
		}

		void AddLog(string message)
		{
			AddLogSub(message);
		}

		void AddLogError(string message)
		{
			AddLogSub(message);
		}

		void AddLogWarning(string message)
		{
			AddLogSub(message);
		}

		void AddLogSub(string message)
		{
			logList.Add(message);
		}
	}
}