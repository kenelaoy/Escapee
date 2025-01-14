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
	/// マクロのデータ
	/// </summary>
	public class AdvMacroData
	{
		public string Name { get; private set; }
		public StringGridRow Header { get; private set; }
		public List<StringGridRow> DataList { get; private set; }
		public AdvMacroData(string name, StringGridRow header, List<StringGridRow> dataList)
		{
			this.Name = name;
			this.Header = header;
			this.DataList = dataList;
		}

		//マクロ処理した行データを取得
		public List<StringGridRow> MakeMacroRows(StringGridRow args)
		{
			List<StringGridRow> list = new List<StringGridRow>();
			if(DataList.Count<=0) return list;

			StringGrid macroGrid = DataList[0].Grid;
			string gridName = args.Grid.Name + ":" + (args.RowIndex+1).ToString() + "-> Macro : " + macroGrid.Name;
			StringGrid grid = new StringGrid(gridName, args.Grid.SheetName, macroGrid.Type);
			grid.Macro = new StringGrid.MacroInfo(args);
			grid.ColumnIndexTbl = macroGrid.ColumnIndexTbl;
			List<int> ignoreIndexArray = AdvParser.CreateMacroOrEntityIgnoreIndexArray(grid);

			for (int i = 0; i < DataList.Count; ++i)
			{
				StringGridRow macroData = DataList[i].Clone(() => (grid) );
				macroData.Macro(args, Header, ignoreIndexArray);
				list.Add(macroData);
			}
			return list;
		}
	}
}