//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utage;

/// <summary>
/// セーブロード画面のサンプル
/// </summary>
[AddComponentMenu("Utage/TemplateUI/SaveLoad")]
public class UtageUguiSaveLoad : UguiView
{
	[SerializeField]
	UguiGridPage gridPage;

	/// <summary>
	/// リストビューアイテムのリスト
	/// </summary>
	List<AdvSaveData> itemDataList;

	/// <summary>ADVエンジン</summary>
	public AdvEngine Engine { get { return this.engine ?? (this.engine = FindObjectOfType<AdvEngine>() as AdvEngine); } }
	[SerializeField]
	AdvEngine engine;

	/// <summary>メイン画面</summary>
	public UtageUguiMainGame mainGame;

	/// <summary>タイトル表記（セーブ画面かロード画面か）</summary>
	public GameObject saveRoot;

	/// <summary>タイトル表記（セーブ画面かロード画面か）</summary>
	public GameObject loadRoot;
	
	bool isSave;

	bool isInit = false;
	int lastPage;


	/// <summary>
	/// セーブ画面を開く
	/// </summary>
	/// <param name="prev">前の画面</param>
	public void OpenSave(UguiView prev)
	{
		isSave = true;
		saveRoot.SetActive(true);
		loadRoot.SetActive(false);
		Open(prev);
	}

	/// <summary>
	/// ロード画面を開く
	/// </summary>
	/// <param name="prev">前の画面</param>
	public void OpenLoad(UguiView prev)
	{
		isSave = false;
		saveRoot.SetActive(false);
		loadRoot.SetActive(true);
		Open(prev);
	}

	/// <summary>
	/// オープンしたときに呼ばれる
	/// </summary>
	void OnOpen()
	{
		isInit = false;
		this.gridPage.ClearItems();
		StartCoroutine(CoWaitOpen());
	}

	/// <summary>
	/// クローズしたときに呼ばれる
	/// </summary>
	void OnClose()
	{
		lastPage = gridPage.CurrentPage;
		this.gridPage.ClearItems();
	}

	//起動待ちしてから開く
	IEnumerator CoWaitOpen()
	{
		while (Engine.IsWaitBootLoading)
		{
			yield return 0;
		}
		AdvSaveManager saveManager = Engine.SaveManager;
		saveManager.ReadAllSaveData();
		List<AdvSaveData> list = new List<AdvSaveData>();
		if (saveManager.IsAutoSave) list.Add(saveManager.AutoSaveData);
		foreach( var data in saveManager.SaveDataList ){
			list.Add(data);
		}
		this.itemDataList = list;
		gridPage.Init(itemDataList.Count, CallBackCreateItem);
		gridPage.CreateItems( lastPage );
		isInit = true;
	}


	/// <summary>
	/// リストビューのアイテムが作成されるときに呼ばれるコールバック
	/// </summary>
	/// <param name="go">作成されたアイテムのGameObject</param>
	/// <param name="index">作成されたアイテムのインデックス</param>
	void CallBackCreateItem(GameObject go, int index)
	{
		UtageUguiSaveLoadItem item = go.GetComponent<UtageUguiSaveLoadItem>();
		AdvSaveData data = itemDataList[index];
		item.Init(data, OnTap, index, isSave);
	}

	void Update()
	{
		//右クリックで戻る
		if (isInit && InputUtil.IsMousceRightButtonDown())
		{
			Back();
		}
	}


	/// <summary>
	/// 各アイテムが押された
	/// </summary>
	/// <param name="button">押されたアイテム</param>
	public void OnTap(UtageUguiSaveLoadItem item)
	{
		if (isSave)
		{
			//セーブ画面なら、セーブ処理
			Engine.WriteSaveData(item.Data);
			item.Refresh(true);
		}
		else
		{
			//ロード画面
			if (item.Data.IsSaved)
			{
				//セーブ済みのデータならこの画面は閉じてロードをする
				Close();
				mainGame.OpenLoadGame(item.Data);
			}
		}
	}
}
