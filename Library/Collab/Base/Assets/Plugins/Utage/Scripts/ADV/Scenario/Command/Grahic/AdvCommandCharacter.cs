//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：キャラクター＆台詞表示
	/// </summary>
	internal class AdvCommandCharacter : AdvCommand
	{

		public AdvCommandCharacter(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row)
		{
			//名前
			string name = ParseCell<string>(AdvColumnName.Arg1);
			//パターンラベル
			string patternLabel = ParseCellOptional<string>(AdvColumnName.Arg2, "");
			//キャラの表示情報を取得
			string errorMsg;
			this.characterInfo = dataManager.CharacterSetting.ParseCharacterInfo(name, patternLabel, out errorMsg);
			
			//キャラの表示情報の記述エラー
			if (!string.IsNullOrEmpty(errorMsg))
			{
				Debug.LogError(ToErrorString(errorMsg));
			}

			AddLoadGraphic(characterInfo.Graphic);
//			AddLoadGraphic(characterInfo.IconGraphic);

			//表示レイヤー
			this.layerName = ParseCellOptional<string>(AdvColumnName.Arg3, "");
			if (!string.IsNullOrEmpty(layerName) && !dataManager.LayerSetting.Contains(layerName, AdvLayerSettingData.LayerType.Character))
			{
				//表示レイヤーが見つからない
				Debug.LogError(ToErrorString(layerName + " is not contained in layer setting"));
			}

			//グラフィック表示処理を作成
			this.graphicOperaitonArg 
				= new AdvGraphicOperaitonArg( 
					this, 
					characterInfo.Graphic, 
					ParseCellOptional<float>(AdvColumnName.Arg6, 0.2f), 
					characterInfo.IsNonePattern );

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
			pageData.InitMessageWindowName(this, ParseCellOptional<string>(AdvColumnName.WindowType,""));
		}

		protected override void InitTextDataInPage(AdvScenarioTextDataInPage textDataInPage)
		{
			base.InitTextDataInPage(textDataInPage);
			TextDataInPage.VoiceFile = voiceFile;
			TextDataInPage.CharacterInfo = characterInfo;
		}

		public override void DoCommand(AdvEngine engine)
		{
			//キャラクター表示更新
			if (!engine.GraphicManager.IsEventMode)
			{

				//非表示なら
				if (characterInfo.IsHide)
				{
					engine.GraphicManager.CharacterManager.FadeOut( characterInfo.Label, graphicOperaitonArg.FadeTime );
				}
				else
				{
					if ( characterInfo.Graphic == null || characterInfo.Graphic.Main == null)
					{

					}
					else
					{
						//パターン指定なしで、既に同名のキャラがいるなら、表示パターンを引き継ぐ仕様は廃止？
						//if (data.CharacterInfo.IsNonePattern && IsContiansDefalutGraphic(data.Name))

						//表示する
						engine.GraphicManager.CharacterManager.DrawCharacter(layerName, characterInfo.Label, graphicOperaitonArg);
					}
				}
			}


			if (null != voiceFile)
			{
				if (!engine.Page.CheckSkip () || !engine.Config.SkipVoiceAndSe) 
				{
					engine.SoundManager.PlayVoice (characterInfo.Label, voiceFile);
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

		protected AdvGraphicOperaitonArg graphicOperaitonArg;
		protected AdvCharacterInfo characterInfo;
		protected string layerName;
		protected AssetFile voiceFile;
	}

}