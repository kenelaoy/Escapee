//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// 文字列グリッドデータの行
	/// </summary>
	[System.Serializable]
	public class StringGridRow
	{
		/// <summary>
		/// 元になるグリッド
		/// </summary>
		public StringGrid Grid { get { return callBackGetGrid(); } }

		/// <summary>
		/// 行番号
		/// </summary>
		public int RowIndex { get { return this.rowIndex; } }
		[SerializeField]
		int rowIndex;

		/// <summary>
		/// 文字列データ
		/// </summary>
		public string[] Strings { get { return this.strings; } }
		[SerializeField]
		string[] strings;

		/// <summary>
		/// 文字列データの長さ
		/// </summary>
		public int Length { get { return strings.Length; } }

		/// <summary>
		/// データが空かどうか
		/// </summary>
		public bool IsEmpty { get { return isEmpty; } }
		[SerializeField]
		bool isEmpty;

		/// <summary>
		/// コメントアウトされているか
		/// </summary>
		public bool IsCommentOut { get { return isCommentOut; } }
		[SerializeField]
		bool isCommentOut;

		/// <summary>
		/// データが空かどうか
		/// </summary>
		public bool IsEmptyOrCommantOut { get { return IsEmpty || IsCommentOut; } }
		
		Func<StringGrid> callBackGetGrid;


		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="grid">元になる文字列グリッド</param>
		/// <param name="rowIndex">行番号</param>
		public StringGridRow(Func<StringGrid> callBackGetGrid, int rowIndex )
		{
			this.rowIndex = rowIndex;
			InitLink(callBackGetGrid);
		}

		//複製
		internal StringGridRow Clone(Func<StringGrid> callBackGetGrid)
		{
			//複製
			StringGridRow clone = new StringGridRow(callBackGetGrid, this.RowIndex);
			clone.strings = new string[this.strings.Length];
			Array.Copy( this.strings, clone.strings, this.strings.Length);
			clone.isEmpty = this.IsEmpty;
			clone.isCommentOut = this.IsCommentOut;
			return clone;
		}

		//マクロ処理
		internal void Macro(StringGridRow macroArgs, StringGridRow macroHeader, List<int> ignoreIndexArray)
		{

			//マクロ条件に合致するものを変換する
			for (int i = 0; i < strings.Length; ++i)
			{
				if (ignoreIndexArray.Contains(i)) continue;
				if (strings[i].Length <= 1) continue;

				string macroText;
				if (CheckMacroArg(strings[i], macroArgs, macroHeader, out macroText))
				{
					strings[i] = macroText;
				}
			}
		}

		bool CheckMacroArg(string str, StringGridRow macroArgs, StringGridRow macroHeader, out string macroText)
		{
			bool ret = false;
			int index = 0;
			macroText = "";
			while (index < str.Length)
			{
				bool isFind = false;
				if (str[index] == '%')
				{
					foreach (string key in Grid.ColumnIndexTbl.Keys)
					{
						if (key.Length <= 0) continue;
						for (int i = 0; i < key.Length; ++i)
						{
							if (key[i] != str[index + 1 + i])
							{
								break;
							}
							else if (i == key.Length-1)
							{
								isFind = true;
							}
						}
						if (isFind)
						{
							string def = macroHeader.ParseCellOptional<string>(key, "");
							macroText += macroArgs.ParseCellOptional<string>(key, def);
							index += key.Length;
							ret = true;
							break;
						}
					}
				}
				if(!isFind)
				{
					macroText += str[index];
				}
				++index;
			}
			return ret;
		}

		/// <summary>
		/// 親とのリンクを初期化
		/// ScriptableObjectなどで読み込んだ場合、参照が切れているのでそれを再設定するために
		/// </summary>
		/// <param name="grid">元になる文字列グリッド</param>
		public void InitLink(Func<StringGrid> callBackGetGrid)
		{
			this.callBackGetGrid = callBackGetGrid;
		}

		/// <summary>
		/// CSVテキストから初期化
		/// </summary>
		/// <param name="type">CSVタイプ</param>
		/// <param name="text">CSVテキスト</param>
		public void InitFromCsvText(CsvType type, string text )
		{
/*			const string conmmaSeparatePattern  = @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)";
			const string tabSeparatePattern = @"(((?<x>(?=[\t\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^\t\r\n]+))\t?)";

			string pattern;
			switch(type)
			{
				case CsvType.Tsv:
					pattern = tabSeparatePattern;
					break;
				case CsvType.Csv:
				default:
					pattern = conmmaSeparatePattern;
					break;
			}
			strings = (
				from System.Text.RegularExpressions.Match m 
					in System.Text.RegularExpressions.Regex.Matches(text,
					pattern,
					System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
				select m.Groups[1].Value).ToArray();
*/
			this.strings = text.Split( type == CsvType.Tsv ? '\t' : ',');
			this.isEmpty = CheckEmpty();
			this.isCommentOut = CheckCommentOut();
		}

		/// <summary>
		/// 文字列リストから初期化
		/// </summary>
		/// <param name="stringList">文字列リスト</param>
		public void InitFromStringList(List<string> stringList)
		{
			strings = stringList.ToArray();
			this.isEmpty = CheckEmpty();
			this.isCommentOut = CheckCommentOut();
		}

		//空データかチェック
		bool CheckEmpty()
		{
			foreach (var str in strings)
			{
				if (!string.IsNullOrEmpty(str))
				{
					return false;
				}
			}
			return true;
		}
		//コメントアウトされているかチェック
		bool CheckCommentOut()
		{
			if (this.Strings.Length <= 0) return false;
			return this.Strings[0].StartsWith("//");
		}


		/// <summary>
		/// 指定した列名のセルが空かどうか
		/// </summary>
		/// <param name="columnName">列の名前</param>
		/// <returns>空ならture、データがあればfalse</returns>
		public bool IsEmptyCell(string columnName)
		{
			int index;
			if (Grid.TryGetColumnIndex(columnName, out index))
			{
				return IsEmptyCell(index);
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// 列名がついたセル全て空かどうか
		/// </summary>
		/// <returns></returns>
		internal bool IsAllEmptyCellNamedColumn()
		{
			foreach( int index in Grid.ColumnIndexTbl.Values)
			{
				if (!IsEmptyCell(index))
				{
					return false;
				}
			}
			return true;
		}


		//指定した列インデックスのセルが空かどうか
		bool IsEmptyCell(int index)
		{
			return !(index < Length && !string.IsNullOrEmpty(strings[index]));
		}

		/// <summary>
		/// 指定した列名のセルを値に変換
		/// </summary>
		/// <typeparam name="T">値の型</typeparam>
		/// <param name="columnName">列の名前</param>
		/// <returns>変換後の値</returns>
		public T ParseCell<T>(string columnName)
		{
			T ret;
			if (!TryParseCell(columnName, out ret))
			{
				Debug.LogError(ToErrorStringWithPraseColumnName(columnName));
			}
			return ret;
		}

		/// <summary>
		/// 指定した列名のセルを値に変換
		/// 要素が空だった場合は、デフォルト値を返す
		/// </summary>
		/// <typeparam name="T">値の型</typeparam>
		/// <param name="columnName">列の名前</param>
		/// <param name="defaultVal">デフォルト値</param>
		/// <returns>変換後の結果</returns>
		public T ParseCellOptional<T>(string columnName, T defaultVal)
		{
			T ret;
			return TryParseCell(columnName, out ret) ? ret : defaultVal;
		}

		/// <summary>
		/// 指定した列名のセルを値に変換を試みる。
		/// </summary>
		/// <typeparam name="T">値の型</typeparam>
		/// <param name="columnName">列の名前</param>
		/// <param name="val">変換後の結果</param>
		/// <returns>成功したらtrue。失敗したらfalse</returns>
		public bool TryParseCell<T>(string columnName, out T val)
		{
			int index;
			if (Grid.TryGetColumnIndex(columnName, out index))
			{
				return TryParseCellSub(index, out val);
			}
			else
			{
				val = default(T);
				return false;
			}
		}

		//指定した列インデックスのセルを値に変換
		bool TryParseCellSub<T>(int index, out T val)
		{
			if (!IsEmptyCell(index))
			{
				if (TryParse<T>(strings[index], out val))
				{
					return true;
				}
				else
				{
					Debug.LogError(ToErrorStringWithPrase(strings[index], index));
					return false;
				}
			}
			else
			{
				val = default(T);
				return false;
			}
		}

		/// <summary>
		/// 文字列を値に変換
		/// </summary>
		/// <typeparam name="T">値の型</typeparam>
		/// <param name="str">文字列</param>
		/// <param name="val">値</param>
		/// <returns>変換に成功したらtrue、書式違いなどで変換できなかったらfalse</returns>
		public static bool TryParse<T>(string str, out T val)
		{
			try
			{
				System.Type type = typeof(T);
				if (type == typeof(string))
				{
					val = (T)(object)str;
				}
				else if (type.IsEnum)
				{
					val = (T)System.Enum.Parse(typeof(T), str);
				}
				else if (type == typeof(Color))
				{
					Color color = Color.white;
					bool ret = ColorUtil.TryParseColor(str, ref color);
					val = ret ? (T)(object)color : default(T);
					return ret;
				}
				else if( type == typeof(int) )
				{
					val = (T)(object)int.Parse(str);
				}
				else if (type == typeof(float))
				{
					val = (T)(object)float.Parse(str);
				}
				else if (type == typeof(double))
				{
					val = (T)(object)double.Parse(str);
				}
				else if (type == typeof(bool))
				{
					val = (T)(object)bool.Parse(str);
				}
				else
				{
					System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(type);
					val = (T)converter.ConvertFromString(str);
				}
				return true;
			}
			catch
			{
				val = default(T);
				return false;
			}
		}

		
		/// <summary>
		/// デバッグ文字列に変換
		/// </summary>
		/// <returns>デバッグ文字列</returns>
		string ToDebugString()
		{
			char separator = Grid.CsvSeparator;

			string textOutput = "";
			foreach (string str in strings)
			{
				textOutput += " " + str + separator;
			}
			return textOutput;
		}

		/// <summary>
		/// エラー用の文字列を取得
		/// </summary>
		/// <param name="msg">エラーメッセージ</param>
		/// <returns>エラー用のテキスト</returns>
		public string ToErrorString(string msg)
		{
			if (!msg.EndsWith("\n")) msg += "\n";

			int lineNo = rowIndex + 1;
			if (Grid.Macro == null)
			{
				string sheetName = Grid.SheetName;
				msg += sheetName + ":" + lineNo + " ";
			}
			else
			{
				string sheetName = Grid.Macro.args.Grid.SheetName;
				msg += sheetName + ":" + (Grid.Macro.args.rowIndex+1) + " ";
			}
			return msg
				+ ColorUtil.AddColorTag(ToDebugString(), Color.red) + "\n"
				+ "<b>" + Grid.Name + "</b>" + "  : " + lineNo;
		}

		/// <summary>
		/// エラー用の文字列を取得
		/// </summary>
		/// <param name="msg">エラーメッセージ</param>
		/// <returns>エラー用のテキスト</returns>
		public string ToStringOfFileSheetLine()
		{
			int lineNo = rowIndex + 1;
			return "<b>" + Grid.Name + "</b>" + "  : " + lineNo;
		}

		//列名指定パースエラー出力
		string ToErrorStringWithPraseColumnName(string columnName)
		{
			return ToErrorString( LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.StringGridRowPraseColumnName, columnName ) );
		}
		//列インデックス指定パースエラー出力
		string ToErrorStringWithPraseColumnIndex(int index)
		{
			return ToErrorString( LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.StringGridRowPraseColumnIndex, index ) );
		}
		//パースエラー出力
		string ToErrorStringWithPrase(string column, int index)
		{
			return ToErrorString( LanguageErrorMsg.LocalizeTextFormat( ErrorMsg.StringGridRowPrase, index,column));
		}
	}
}