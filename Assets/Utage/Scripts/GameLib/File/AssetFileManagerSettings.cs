//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utage
{
	/// <summary>
	/// ファイル管理
	/// </summary>
	[System.Serializable]
	public class AssetFileManagerSettings
	{
		//ファイルのロードタイプの設定
		public enum LoadType
		{
			Local,					//ローカル（Resources)から読み込む
			LocalCompressed,		//容量削減版・テクスチャのみエンコードしたStreamingAssetsから読み込む
			Server,					//サーバーから読み込む。サウンドとUnityObjectはアセットバンドルにし、それ以外は独自のエンコードをかける
			ServerPure,				//サーバーから読み込む。ファイルをそのまま扱う。UnityObjectだけはアセットバンドル
			Advanced,				//ファイルの種類ごとに設定を細かく決める
			LocalCompressed2,		//容量削減版・テクスチャは独自エンコード、サウンドはアセットバンドルをStreamingAssetsから読み込む
		};

		[SerializeField]
		LoadType loadType;
		public LoadType LoadTypeSetting
		{
			get { return loadType; }
			set	{loadType = value;}
		}

		[SerializeField]
		List<AssetFileSetting> fileSettings
		= new List<AssetFileSetting>(){
			new AssetFileSetting(AssetFileType.Bytes, new string[] { ".bin", ".bytes" }),
			new AssetFileSetting(AssetFileType.Text, new string[] { ".txt", ".json", ".html", ".htm", ".xml", ".yaml", ".fnt" }),
			new AssetFileSetting(AssetFileType.Texture, new string[] { ".png", ".jpg", ".bmp" }),
			new AssetFileSetting(AssetFileType.Sound, new string[] { ".mp3", ".ogg", ".wav" }),
			new AssetFileSetting(AssetFileType.Csv, new string[] { ".csv", ".tsv" }),
			new AssetFileSetting(AssetFileType.UnityObject, new string[] { "" }),
		};
		public List<AssetFileSetting> FileSettings
		{
			get { return fileSettings; }
		}

		public AssetFileManagerSettings()
		{
			foreach (AssetFileSetting setting in FileSettings)
			{
				setting.InitLink(this);
			}
		}

		//拡張子を追加
		public void AddExtensions(AssetFileType type, string[] extensions)
		{
			Find(type).AddExtensitons(extensions);
		}

		//ファイルタイプから設定データを取得
		public AssetFileSetting Find(AssetFileType type)
		{
			return FileSettings.Find(x => (x.FileType == type));
		}

		//ファイルパスから設定データを取得
		public AssetFileSetting FindSettingFromPath(string path)
		{
			AssetFileSetting setting = fileSettings.Find(x => (x.ContaisnsExtenstions(path)));
			if (setting == null)
			{
				setting = Find(AssetFileType.UnityObject);
			}
			return setting;
		}		
	}

	/// <summary>
	/// ファイル管理
	/// </summary>
	[System.Serializable]
	public class AssetFileSetting
	{
		public AssetFileSetting(AssetFileType fileType, string[] extensions)
		{
			this.fileType = fileType;
			this.extensions = new List<string>(extensions);
		}

		//ファイルタイプ
		public AssetFileType FileType
		{
			get { return fileType; }
		}
		[SerializeField,HideInInspector]
		AssetFileType fileType;

		//StreamingAssetsから読み込むか
		[SerializeField]
		bool isStreamingAssets = false;
		public bool IsStreamingAssets
		{
			get
			{
				switch (LoadType)
				{
					case AssetFileManagerSettings.LoadType.LocalCompressed:
						//容量削減版・テクスチャのみエンコードしたStreamingAssetsから読み込む
						return (FileType == AssetFileType.Texture);
					case AssetFileManagerSettings.LoadType.LocalCompressed2:
						//容量削減版2・テクスチャとアセットバンドルはStreamingAssetsから読み込む
						return (FileType == AssetFileType.Texture) || (FileType == AssetFileType.Sound);
					//その他はStreamingAssets読み込みはしない
					case AssetFileManagerSettings.LoadType.Local:
					case AssetFileManagerSettings.LoadType.ServerPure:
					case AssetFileManagerSettings.LoadType.Server:
						return false;
					case AssetFileManagerSettings.LoadType.Advanced:
					default:
						return isStreamingAssets;
				}
			}
			set
			{
				isStreamingAssets = value;
			}
		}

		//エンコードのタイプ(WWWやStrage読み込みの際の暗号化の形式)
		[SerializeField]
		AssetFileEncodeType encodeType = AssetFileEncodeType.None;
		public AssetFileEncodeType EncodeType
		{
			get
			{
				switch (LoadType)
				{
					case AssetFileManagerSettings.LoadType.Local:
						//ローカル（Resources)から読み込む
						return AssetFileEncodeType.None;
					case AssetFileManagerSettings.LoadType.LocalCompressed:
						//容量削減版・テクスチャのみエンコードしたStreamingAssetsから読み込む
						return (FileType == AssetFileType.Texture) ? AssetFileEncodeType.AlreadyEncoded : AssetFileEncodeType.None;
					case AssetFileManagerSettings.LoadType.LocalCompressed2:
						switch (FileType)
						{
							case AssetFileType.Texture:
								return AssetFileEncodeType.AlreadyEncoded;
							case AssetFileType.Sound:
							case AssetFileType.UnityObject:
								return AssetFileEncodeType.AssetBundle;
							default:
								return AssetFileEncodeType.None;
						}
					case AssetFileManagerSettings.LoadType.Server:
						//サーバーから読み込む。サウンドとUnityObjectはアセットバンドルにし、それ以外は独自のエンコードをかける
						return (FileType == AssetFileType.UnityObject || FileType == AssetFileType.Sound) ? AssetFileEncodeType.AssetBundle : AssetFileEncodeType.AlreadyEncoded;
					case AssetFileManagerSettings.LoadType.ServerPure:
						//サーバーから読み込む。ファイルをそのまま扱う。UnityObjectだけはアセットバンドル
						if(FileType == AssetFileType.UnityObject)
						{
							return AssetFileEncodeType.AssetBundle;
						}
						else if(FileType == AssetFileType.Sound)
						{	//サウンドはエンコードできない
							return AssetFileEncodeType.None;
						}
						else
						{	//その他のファイルはキャッシュ書き込みの際にエンコード
							return AssetFileEncodeType.EncodeOnCache;
						}
					case AssetFileManagerSettings.LoadType.Advanced:
					default:
						return encodeType;
				}
			}
			set
			{
				switch (FileType)
				{
					case AssetFileType.UnityObject:
						if (value != AssetFileEncodeType.AssetBundle)
						{
							encodeType = AssetFileEncodeType.AssetBundle;
							return;
						}
						break;
					case AssetFileType.Sound:
						if (value == AssetFileEncodeType.EncodeOnCache || value == AssetFileEncodeType.AlreadyEncoded)
						{
							encodeType = AssetFileEncodeType.AssetBundle;
							return;
						}
						break;
				}
				encodeType = value;
			}
		}

		//現在のエンコードタイプ
		public AssetFileEncodeType GetRunTimeEncodeType(bool isServer)
		{
			if (!IsStreamingAssets && !isServer)
			{
				//Resources読み込みならエンコードなし
				return AssetFileEncodeType.None;
			}
			else
			{
				return EncodeType;
			}
		}


		//対象となる拡張子
		[SerializeField]
		List<string> extensions;
		public void AddExtensitons(string[] extensions)
		{
			this.extensions.AddRange(extensions);
		}

		//拡張子を比較
		internal bool ContaisnsExtenstions(string path)
		{
			//Utage用の二重拡張子を無視した拡張子を取得
			string ext = FilePathUtil.GetExtenstionWithOutDouble(path, ExtensionUtil.UtageFile);
			return this.extensions.Contains(ext);
		}

		[NonSerialized]
		AssetFileManagerSettings settings;
		AssetFileManagerSettings Settings { get { return settings; } }
		public void InitLink(AssetFileManagerSettings settings)
		{
			this.settings = settings;
		}

		AssetFileManagerSettings.LoadType LoadType { get { return Settings.LoadTypeSetting; } }

/*
		//ロード設定を変える
		internal void ChangeLoadType(AssetFileManagerSettings.LoadType loadType)
		{
			switch (loadType)
			{
				case AssetFileManagerSettings.LoadType.Local:
					//ローカル（Resources)から読み込む
					IsStreamingAssets = false;
					EncodeType = AssetFileEncodeType.None;
					break;
				case AssetFileManagerSettings.LoadType.LocalCompressed:
					//容量削減版・テクスチャのみエンコードしたStreamingAssetsから読み込む
					if(FileType== AssetFileType.Texture)
					{
						IsStreamingAssets = true;
						EncodeType = AssetFileEncodeType.AlreadyEncoded;
					}
					else
					{
						IsStreamingAssets = false;
						EncodeType = AssetFileEncodeType.None;
					}
					break;
				case AssetFileManagerSettings.LoadType.Server:
					//サーバーから読み込む。サウンドとUnityObjectはアセットバンドルにし、それ以外は独自のエンコードをかける
					switch(FileType)
					{
						case AssetFileType.UnityObject:
						case AssetFileType.Sound:
							//サウンドとUnityObjectはアセットバンドルに
							EncodeType = AssetFileEncodeType.AssetBundle;
							break;
						default:
							//それ以外は独自に符号化済みのデータを扱う
							EncodeType = AssetFileEncodeType.AlreadyEncoded;
							break;
					}
					break;
				case AssetFileManagerSettings.LoadType.ServerPure:
					//サーバーから読み込む。ファイルをそのまま扱う。UnityObjectだけはアセットバンドル
					{
						switch(FileType)
						{
							case AssetFileType.UnityObject:
								//UnityObjectはアセットバンドルに
								EncodeType = AssetFileEncodeType.AssetBundle;
								break;
							case AssetFileType.Sound:
								//サウンドはエンコードなし
								EncodeType = AssetFileEncodeType.None;
								break;
							default:
								//キャッシュ時にエンコードする
								EncodeType = AssetFileEncodeType.EncodeOnCache;
								break;
						}
					}
					break;
					//ファイルの種類ごとに設定を細かく決める
				case AssetFileManagerSettings.LoadType.Advanced:
					break;
			}
		}
 */ 
	}
}
