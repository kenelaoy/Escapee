//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：SE再生
	/// </summary>
	internal class AdvCommandSe : AdvCommand
	{

		public AdvCommandSe(StringGridRow row, AdvSettingDataManager dataManager)
			: base(row)
		{
			this.label = ParseCell<string>(AdvColumnName.Arg1);
			if (!dataManager.SoundSetting.Contains(label, SoundType.Se))
			{
				Debug.LogError(ToErrorString(label + " is not contained in file setting"));
			}
			this.isLoop = ParseCellOptional<bool>(AdvColumnName.Arg2, false);

			this.file = AddLoadFile(dataManager.SoundSetting.LabelToFilePath(label, SoundType.Se), dataManager.SoundSetting.FindRowData(label) );
		}

		public override void DoCommand(AdvEngine engine)
		{
			if (!engine.Page.CheckSkip () || !engine.Config.SkipVoiceAndSe) 
			{
				engine.SoundManager.PlaySe(file, label, SoundPlayMode.Add, isLoop);
			}
		}

		string label;
		AssetFile file;
		bool isLoop;
	}
}