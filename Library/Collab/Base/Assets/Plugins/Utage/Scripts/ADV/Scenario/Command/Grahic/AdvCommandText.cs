//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------
using UnityEngine;
using System.Text;

namespace Utage
{

	/// <summary>
	/// コマンド：テキスト表示（地の文）
	/// </summary>
	internal class AdvCommandText : AdvCommand
	{

		public AdvCommandText(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row)
		{
			//ボイス
			string voice = ParseCellOptional<string>(AdvColumnName.Voice, "");
			int voiceVersion = ParseCellOptional<int>(AdvColumnName.VoiceVersion, 0);
			//ボイスファイル設定
			if (!string.IsNullOrEmpty(voice))
			{
				if (AdvCommand.IsEditorErrorCheck)
				{
				}
				else
				{
					voiceFile = AddLoadFile(dataManager.BootSetting.GetLocalizeVoiceFilePath(voice), this.RowData);
					if (voiceFile != null) voiceFile.Version = voiceVersion;
					//ストリーミング再生にバグがある模様。途中で無音が混じると飛ばされる？
					//				voiceFile.LoadFlags = AssetFileLoadFlags.Streaming;
				}
			}
		}

		//ページ用のデータを作成
		public override void MakePageData(AdvScenarioPageData pageData)
		{
			InitTextDataInPage(pageData.AddTextDataInPage(this));
			pageData.InitMessageWindowName(this, ParseCellOptional<string>(AdvColumnName.WindowType, ""));
		}
		protected override void InitTextDataInPage(AdvScenarioTextDataInPage textDataInPage)
		{
			base.InitTextDataInPage(textDataInPage);
			TextDataInPage.VoiceFile = voiceFile;
		}

		public override void DoCommand(AdvEngine engine)
		{
			if (null != voiceFile)
			{
				if (!engine.Page.CheckSkip () || !engine.Config.SkipVoiceAndSe) 
				{
					engine.SoundManager.PlayVoice ("", voiceFile);
				}
			}
			engine.Page.UpdatePageTextData(TextDataInPage);
		}

		public override bool Wait(AdvEngine engine)
		{
			return engine.Page.IsWaitTextCommand;
		}

		//ページ区切りのコマンドか
		public override bool IsTypePageEnd() { return TextDataInPage.IsPageEnd; }

		protected AssetFile voiceFile;
	}
}