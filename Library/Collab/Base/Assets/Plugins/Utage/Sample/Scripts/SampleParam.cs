//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;
using Utage;
using System.Collections;


/// <summary>
/// Sample LoadErrorのコールバック関数を書き換え
/// </summary>
[AddComponentMenu("Utage/ADV/Examples/SampleParam")]
public class SampleParam : MonoBehaviour
{
	/// <summary>ADVエンジン</summary>
	public AdvEngine Engine { get { return this.engine ?? (this.engine = FindObjectOfType<AdvEngine>()); } }
	[SerializeField]
	protected AdvEngine engine;

	public void ParmTest()
	{
		Engine.Param.GetParameter("flag1");
		Engine.Param.TrySetParameter("flag1",true);
	}
}
