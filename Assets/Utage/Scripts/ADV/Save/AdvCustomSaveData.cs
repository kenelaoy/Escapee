//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.IO;

namespace Utage
{
	//カスタムセーブデータの入出力用のインターフェース
	public interface IAdvCustomSaveDataIO
	{
		//セーブデータのキー
		string SaveKey{get;}

		//クリアする
		void OnClear();

		//書き込み
		void OnWrite(System.IO.BinaryWriter writer);

		//読み込み
		void OnRead(System.IO.BinaryReader reader);
	}

	//カスタムセーブデータクラス
	public class AdvCustomSaveData
	{
		public Dictionary<string, byte[]> Buffers { get { return buffers; } }
		Dictionary<string, byte[]> buffers = new Dictionary<string, byte[]>();

		public AdvCustomSaveData()
		{
		}

		//データを書き込み
		public void WriteCustomSaveData(List<IAdvCustomSaveDataIO> ioList)
		{
			ioList.ForEach((x) => WriteCustomSaveData(x));
		}
		void WriteCustomSaveData(IAdvCustomSaveDataIO io)
		{
			if (Buffers.ContainsKey(io.SaveKey))
			{
				Debug.LogError(string.Format("Custom save data[{0}] is already exsits. Please use another key.", io.SaveKey));
				return;
			}

			byte[] buffer = BinaryUtil.BinaryWrite(io.OnWrite);
			Buffers.Add(io.SaveKey, buffer);
		}

		
		//データ読み込み
		public void ReadCustomSaveData(List<IAdvCustomSaveDataIO> ioList)
		{
			ioList.ForEach((x) => ReadCustomSaveData(x));
		}
		void ReadCustomSaveData(IAdvCustomSaveDataIO io)
		{
			if (Buffers.ContainsKey(io.SaveKey))
			{
				BinaryUtil.BinaryRead(Buffers[io.SaveKey], io.OnRead);
			}
		}

		//中身をコピーした新しいインスタンスを作成
		public AdvCustomSaveData Clone()
		{
			AdvCustomSaveData clone = new AdvCustomSaveData();
			foreach (string key in Buffers.Keys)
			{
				clone.Buffers.Add(key,Buffers[key]);
			}
			return clone;
		}

		//セーブデータのバイナリから初期化
		public AdvCustomSaveData(BinaryReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; ++i )
			{
				Buffers.Add(reader.ReadString(), reader.ReadBytes(reader.ReadInt32()));
			}
		}

		internal void Write(BinaryWriter writer)
		{
			writer.Write(Buffers.Count);
			foreach (string key in Buffers.Keys)
			{
				writer.Write(key);
				byte[] buffer = Buffers[key];
				writer.Write(buffer.Length);
				writer.Write(buffer);
			}
		}

	}
}