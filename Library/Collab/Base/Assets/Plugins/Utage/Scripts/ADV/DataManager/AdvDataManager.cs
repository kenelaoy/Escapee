//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// データ管理
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/DataManager ")]
	public partial class AdvDataManager : MonoBehaviour
	{
		//バックグランドでリソースのDLをするか
		[SerializeField]
		bool isBackGroundDownload = true;
		public bool IsBackGroundDownload
		{
			get { return isBackGroundDownload; }
			set { isBackGroundDownload = value; }
		}

		/// <summary>
		/// 設定データ
		/// </summary>
		public AdvSettingDataManager SettingDataManager { get { return this.settingDataManager; } }
		AdvSettingDataManager settingDataManager = new AdvSettingDataManager();

		//シナリオデータ
		Dictionary<string, AdvScenarioData> scenarioDataTbl = new Dictionary<string, AdvScenarioData>();

		/// <summary>
		/// 設定データが準備済みか
		/// </summary>
		public bool IsReadySettingData { get { return (settingDataManager != null); } }

		/// <summary>
		/// マクロ
		/// </summary>
		public AdvMacroManager MacroManager { get { return this.macroManager; } }
		AdvMacroManager macroManager = new AdvMacroManager();

		/// <summary>
		/// 起動用TSVをロード
		/// </summary>
		/// <param name="url">CSVのパス</param>
		/// <param name="version">シナリオバージョン（-1以下で必ずサーバーからデータを読み直す）</param>
		/// <returns></returns>
		public IEnumerator CoLoadChapter(string url, int version)
		{
			string chapterName = FilePathUtil.GetFileNameWithoutDoubleExtensiton(url);
			AdvChapterData chapter = SettingDataManager.FindChapter(chapterName);
			if (chapter != null)
			{
				yield break;
			}
			else
			{
				chapter = new AdvChapterData(chapterName);
				yield return StartCoroutine(chapter.CoLoadFromTsv(url, version));
				SettingDataManager.RunTimeChapters.Add(chapter);
			}
		}

		public void LoadExcel(string excelPath)
		{
/*			//対象のエクセルファイルを全て読み込み
			Dictionary<string, StringGridDictionary> bookDictionary = new Dictionary<string, StringGridDictionary>();
			if (!string.IsNullOrEmpty(excelPath))
			{
				StringGridDictionary book = ExcelParser.Read(excelPath);
				if (book.List.Count > 0)
				{
					bookDictionary.Add(excelPath, book);
				}
			}
			
			//設定データロード
			foreach (var book in bookDictionary)
			{
				foreach (var sheet in book.Value.List)
				{
					StringGrid grid = sheet.Grid;
					if (AdvSettingDataManager.IsSettingsSheet(grid.SheetName))
					{
						SettingDataManager.AddRuntimeChapterData("",grid);
					}
				}
			}

			//シナリオデータロード
			foreach (var book in bookDictionary)
			{
				foreach (var sheet in book.Value.List)
				{
					StringGrid grid = sheet.Grid;
					//設定データか、シナリオデータかチェック
					if (!AdvSettingDataManager.IsSettingsSheet(sheet.Name))
					{
						if (!MacroManager.TryAddMacroData(sheet.Name, grid))
						{
							scenarioDataTbl.Add(sheet.Name, new AdvScenarioData(sheet.Name, grid));
						}
					}
				}
			}*/
		}

		/// <summary>
		/// 起動時の初期化
		/// </summary>
		/// <param name="rootDirResource">ルートディレクトリのリソース</param>
		public void BootInit( string rootDirResource )
		{
			settingDataManager.BootInit(rootDirResource);
		}

		/// <summary>
		/// 章の起動時の初期化
		/// </summary>
		internal AdvChapterData InitChapterData(string url)
		{
			string chapterName = FilePathUtil.GetFileNameWithoutDoubleExtensiton(url);
			AdvChapterData chapter = settingDataManager.FindChapter(chapterName);
			if (!chapter.IsInited)
			{
				settingDataManager.BootInitChapter(chapter);
				AddScenario(chapter);
				foreach (var data in chapter.ScenarioDataList)
				{
					data.Init(this.settingDataManager, this.MacroManager);
				}
			}
			return chapter;
		}


		/// <summary>
		/// シナリオデータのロードと初期化を開始
		/// </summary>
		public void BootInitScenariodData()
		{
			//シナリオのインポート済みのデータをまず初期化
			if (this.settingDataManager.ImportedScenarios != null)
			{
				this.settingDataManager.ImportedScenarios.Chapters.ForEach(x => AddScenario(x));
			}
			//ランタイムのデータをロード
			this.settingDataManager.RunTimeChapters.ForEach(x => AddScenario(x));

			//シナリオデータの初期化
			foreach (var data in scenarioDataTbl.Values)
			{
				data.Init(this.settingDataManager, this.MacroManager);
			}
		}

		void AddScenario( AdvChapterData chapter )
		{
			foreach (var grid in chapter.ScenarioList)
			{
				string sheetName = grid.SheetName;
				if (!MacroManager.TryAddMacroData(sheetName, grid))
				{
					//既にある（エクスポートされたデータの可能性あり）
					if (scenarioDataTbl.ContainsKey(sheetName))
					{
						Debug.LogWarning(sheetName + " is already contains");
					}
					else
					{
						AdvScenarioData data = new AdvScenarioData(sheetName, grid);
						chapter.ScenarioDataList.Add(data);
						scenarioDataTbl.Add(sheetName, data);
					}
				}
			}
		}

		
		/// <summary>
		/// リソースファイル(画像やサウンド)のダウンロードをバックグラウンドで進めておく
		/// </summary>
		/// <param name="startScenario">開始シナリオ名</param>
		public void StartBackGroundDownloadResource( string startScenario )
		{
			if (isBackGroundDownload)
			{
				StartCoroutine(CoBackGroundDownloadResource(startScenario));
				SettingDataManager.DownloadAll();
			}
		}
		IEnumerator CoBackGroundDownloadResource(string label)
		{
			if (label.Length > 1 && label[0] == '*')
			{
				label = label.Substring(1);
			}

			AdvScenarioData data = FindScenarioData(label);
			if (data == null)
			{
				Debug.LogError(label + " is not found in all scenario");
				yield break;
			}
			if (!data.IsAlreadyBackGroundLoad)
			{
				data.Download(this);
				foreach (AdvScenarioJumpData jumpData in data.JumpScenarioData)
				{
					//シナリオファイルのロード待ち
					while (!IsLoadEndScenarioLabel(jumpData))
					{
						yield return 0;
					}
					yield return StartCoroutine(CoBackGroundDownloadResource(jumpData.ToLabel));
				}
			}
		}

		/// <summary>
		/// リソースファイル(画像やサウンド)のダウンロードをバックグラウンドで進めておく
		/// </summary>
		public void StartBackGroundDownloadResourceInChapter(AdvChapterData chapter)
		{
			if (isBackGroundDownload)
			{
				foreach (AdvScenarioData scenario in chapter.ScenarioDataList)
				{
					scenario.Download(this);
				}
				SettingDataManager.DownloadAll();
			}
		}

		/// <summary>
		/// 指定のシナリオラベルが既にロード終了しているか
		/// </summary>
		public bool IsLoadEndScenarioLabel(AdvScenarioJumpData jumpData)
		{
			return IsLoadEndScenarioLabel(jumpData.ToLabel);
		}

		/// <summary>
		/// 指定のシナリオラベルが既にロード終了しているか
		/// </summary>
		public bool IsLoadEndScenarioLabel(string label)
		{
			AdvScenarioData scenarioData = FindScenarioData(label);
			if (null != scenarioData) return true;

			string msg = LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.NotFoundScnarioLabel, label);
			Debug.LogError(msg);
			return false;
		}

		/// <summary>
		///  シナリオデータを検索して取得
		/// </summary>
		/// <param name="ScebarioLabel">シナリオラベル</param>
		/// <returns>シナリオデータ。見つからなかったらnullを返す</returns>
		public AdvScenarioData FindScenarioData(string label)
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values )
			{
				if (data.IsContainsScenarioLabel(label))
				{
					return data;
				}
			}
			return null;
		}

		/// <summary>
		///  シナリオデータを検索して取得
		/// </summary>
		/// <param name="ScebarioLabel">シナリオラベル</param>
		/// <returns>シナリオデータ。見つからなかったらnullを返す</returns>
		public AdvScenarioLabelData FindScenarioLabelData(string scenarioLabel)
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values)
			{
				AdvScenarioLabelData labelData = data.FindScenarioLabelData(scenarioLabel);
				if (labelData!=null)
				{
					return labelData;
				}
			}
			return null;
		}


		public AdvScenarioLabelData NextScenarioLabelData(string scenarioLabel)
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values)
			{
				AdvScenarioLabelData labelData = data.FindNextScenarioLabelData(scenarioLabel);
				if (labelData != null)
				{
					return labelData;
				}
			}
			return null;
		}

		//サブルーチンの帰り先を見つけて情報を設定
		internal void SetSubroutineRetunInfo( string scenarioLabel, int subroutineCommandIndex, SubRoutineInfo info)
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values)
			{
				AdvScenarioLabelData labelData = data.FindScenarioLabelData(scenarioLabel);
				if (labelData == null) continue;

				labelData.SetSubroutineRetunInfo(subroutineCommandIndex, info);
				break;
			}
		}


		public HashSet<AssetFile> MakePreloadFileList(string scenarioLabel, int page, int maxFilePreload)
		{
			foreach (AdvScenarioData data in scenarioDataTbl.Values)
			{
				if (data.IsContainsScenarioLabel(scenarioLabel))
				{
					return data.MakePreloadFileList( scenarioLabel, page, maxFilePreload);
				}
			}
			return null;
		}
	}
}
