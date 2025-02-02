//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Utage
{
	/// <summary>
	/// CSVのタイプ
	/// </summary>
	public enum CsvType
	{
		Csv,
		Tsv,
	};

	/// <summary>
	/// 文字列のグリッド（CSVなどに使う）
	/// </summary>
	[System.Serializable]
	public class StringGrid
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="name">名前</param>
		public StringGrid(string name, string sheetName, CsvType type)
		{
			this.name = name;
			this.sheetName = sheetName;
			this.type = type;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="name">名前</param>
		public StringGrid(string name, CsvType type, string csvText, int headerRow )
		{
			Create( name, type, csvText, headerRow);
		}
		public StringGrid(string name, CsvType type, string csvText )
		{
			Create( name, type, csvText, 0);
		}
		void Create(string name, CsvType type, string csvText, int headerRow )
		{
			this.name = name;
			this.type = type;
			Rows.Clear();
			//CSVデータを作成
			string[] lines = csvText.Split("\n"[0]);
			for (int i = 0; i < lines.Length; i++)
			{
				StringGridRow row = new StringGridRow(GetThis, Rows.Count);
				row.InitFromCsvText(type,lines[i]);
				Rows.Add(row);
			}
			ParseHeader(headerRow);
			textLength = csvText.Length;
		}

		/// <summary>
		/// 行のデータ
		/// </summary>
		public List<StringGridRow> Rows { get { return this.rows ?? (rows = new List<StringGridRow>()); } }
		[SerializeField]
		List<StringGridRow> rows;

		/// <summary>
		/// 名前
		/// </summary>
		public string Name { get { return name; } }
		[SerializeField]
		string name;

		/// <summary>
		/// シート名
		/// </summary>
		public string SheetName
		{
			get
			{
				if (string.IsNullOrEmpty(sheetName))
				{
					int sheetnameIndex = Name.LastIndexOf(":");
					this.sheetName = Name;
					if (sheetnameIndex > 0)
					{
						this.sheetName = sheetName.Remove(0, sheetnameIndex + 1);
					}

					if (sheetName.Contains("."))
					{
						this.sheetName = FilePathUtil.GetFileNameWithoutDoubleExtensiton(this.Name).Replace("%7B", "{").Replace("%7D", "}");
					}
				}
				return sheetName;
			}
		}
		string sheetName;


		/// <summary>
		/// CSVのタイプ
		/// </summary>
		public CsvType Type { get { return type; } }
		[SerializeField]
		CsvType type;

		/// <summary>
		/// CSVの区切り文字
		/// </summary>
		public char CsvSeparator { get { return (Type == CsvType.Csv) ? ',' : '\t'; } }

		/// <summary>
		/// テキストのサイズ（メモリ管理の目安にとっておく）
		/// </summary>
		public int TextLength { get { return textLength; } }
		[SerializeField]
		int textLength;

		//列インデックスの名前引きテーブル
		Dictionary<string, int> columnIndexTbl;

		public Dictionary<string, int> ColumnIndexTbl
		{
			get { return columnIndexTbl; }
			set { columnIndexTbl = value; }
		}

		//ヘッダ情報の行番号
		[SerializeField]
		int headerRow = 0;

		//データの先頭行番号
		public int DataTopRow{ get { return headerRow+1; }}

		//コールバック用に
		StringGrid GetThis() { return this; }

		//マクロ用の情報
		internal class MacroInfo
		{
			internal StringGridRow args;
			internal MacroInfo(StringGridRow args)
			{
				this.args = args;
			}
		};
		internal MacroInfo Macro { get; set; }

		/// <summary>
		/// 行データとのリンクを設定
		/// ScriptableObjectなどで読み込んだ場合、参照が切れているのでそれを再設定するために
		/// </summary>
		public void InitLink()
		{
			foreach (var row in Rows)
			{
				row.InitLink(GetThis);
			}
			ParseHeader(headerRow);
		}


		/// <summary>
		/// 文字列リストから行を追加
		/// </summary>
		/// <param name="stringList"></param>
		public void AddRow(List<string> stringList)
		{
			StringGridRow row = new StringGridRow(GetThis, Rows.Count);
			row.InitFromStringList(stringList);
			Rows.Add(row);
			foreach( string str in stringList ){
				textLength += str.Length;
			}
		}


		/// <summary>
		/// ヘッダーの解析
		/// </summary>
		/// <param name="headerRow">ヘッダー情報のある行番号</param>
		public void ParseHeader(int headerRow)
		{
			this.headerRow = headerRow;
			ColumnIndexTbl = new Dictionary<string, int>();
			if (headerRow < Rows.Count)
			{
				StringGridRow row = Rows[headerRow];
				for (int i = 0; i < row.Strings.Length; ++i)
				{
					string key = row.Strings[i];
					if( ColumnIndexTbl.ContainsKey(key) )
					{
						string errorMsg = "";
						if( !string.IsNullOrEmpty(key) )
						{
							errorMsg += row.ToErrorString( ColorUtil.AddColorTag(key,Color.red)+ "  is already contains");
							Debug.LogError(errorMsg);
						}
					}
					else
					{
						ColumnIndexTbl.Add(key, i);
					}
				}
			}
			else
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.StringGridParseHaeder, headerRow, this.name));
			}
		}
		public void ParseHeader()
		{
			ParseHeader(0);
		}

		/// <summary>
		/// 列の名前があるか
		/// </summary>
		/// <param name="name">名前</param>
		/// <returns>成否</returns>
		public bool ContainsColumn(string name)
		{
			return ColumnIndexTbl.ContainsKey(name);
		}
		
		/// <summary>
		/// 列の名前から列番号インデックスを取得
		/// </summary>
		/// <param name="name"></param>
		/// <returns>列番号インデックス</returns>
		public int GetColumnIndex(string name)
		{
			int index;
			if (TryGetColumnIndex(name, out index))
			{
				return index;
			}
			else
			{
				Debug.LogError( LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.StringGridGetColumnIndex, name, this.name) );
				return 0;
			}
		}

		/// <summary>
		/// 列の名前から列番号インデックスを取得を試みる
		/// </summary>
		/// <param name="name">名前</param>
		/// <param name="index">列番号インデックス</param>
		/// <returns>成否</returns>
		public bool TryGetColumnIndex(string name, out int index)
		{
			return ColumnIndexTbl.TryGetValue(name, out index);
		}
		public string ToText()
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			char separator = CsvSeparator;
			foreach (StringGridRow row in Rows)
			{
				for (int i = 0; i < row.Strings.Length; ++i)
				{
					//CSVの書式にあわせる
					string line = row.Strings[i].Replace("\n", "\\n");
					builder.Append(line);
					if (i < row.Strings.Length - 1)
					{
						builder.Append(separator);
					}
				}
				builder.Append("\n");
			}
			return builder.ToString();
		}
	}
}