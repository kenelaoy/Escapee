//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Utage
{

	//「Utage」のシナリオデータ用のエクセルファイルの管理エディタウイドウ
	public class CreateNewProjectWindow : EditorWindow
	{
		enum Type
		{
			CreateNewAdvScene,			//ADV用新規シーンを作成
			AddToCurrentScene,			//現在のシーンに追加
			CreateScenarioAssetOnly,	//シナリオ用プロジェクトファイルのみ作成
		};
		Type createType;
		string newProjectName = "";
		string newProjectDir = "";
		const string TemplateName = "Template";
		string TemplateAssetsDir
		{
			get
			{
				return MonoScriptHelper.CurrentUtageRootDirectory + TemplateName;
			}
		}

		string secretKey = "InputOriginalKey";

		int gameScreenWidth = 800;
		int gameScreenHeight = 600;

		string layerName = "Utage";
		string layerNameUI = "UtageUI";

		const string OldLayerName = "Default";
		const string OldLayerNameUI = "UI";

		Font UiFont { get { return uiFont ?? (uiFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font); } }
		Font uiFont = null;

		void OnGUI()
		{
			UtageEditorToolKit.BeginGroup("Create New Project");
//			GUIStyle style = new GUIStyle();
			GUILayout.Space(4f);
			UtageEditorToolKit.BoldLabel("Input New Project Name", GUILayout.Width(200f));
			newProjectName = GUILayout.TextField(newProjectName);

			GUILayout.Space(4f);
			UtageEditorToolKit.BoldLabel("Select Create Type", GUILayout.Width(200f));
			Type type = (Type)EditorGUILayout.EnumPopup("Type", createType);
			if (createType != type)
			{
				createType = type;
			}
			UtageEditorToolKit.EndGroup();

			//レイヤー設定
			bool isDisableLayer = false;
			if (type == Type.AddToCurrentScene)
			{
				UtageEditorToolKit.BeginGroup("Layer Setting");
				layerName = EditorGUILayout.TextField("Layer Name", layerName);
				layerNameUI = EditorGUILayout.TextField("UI Layer Name", layerNameUI);
				UtageEditorToolKit.EndGroup();

				if (string.IsNullOrEmpty(layerName) || string.IsNullOrEmpty(layerNameUI))
				{
					isDisableLayer = true;
				}
			}

			//ゲームの画面サイズ
			EditorGUI.BeginDisabledGroup(!(createType != Type.CreateScenarioAssetOnly));
			GUILayout.Space(4f);
			UtageEditorToolKit.BeginGroup("Game Screen Size");
			int width = EditorGUILayout.IntField("Width", gameScreenWidth);
			if (gameScreenWidth != width && width > 0)
			{
				gameScreenWidth = width;
			}
			int height = EditorGUILayout.IntField("Height", gameScreenHeight);
			if (gameScreenHeight != height && height > 0)
			{
				gameScreenHeight = height;
			}
			UtageEditorToolKit.EndGroup();
			EditorGUI.EndDisabledGroup();

			//ゲームの画面サイズ
			EditorGUI.BeginDisabledGroup(!(createType != Type.CreateScenarioAssetOnly));
			GUILayout.Space(4f);
			UtageEditorToolKit.BeginGroup("Font");
			this.uiFont = EditorGUILayout.ObjectField("Font", UiFont, typeof(Font), false) as Font;
			UtageEditorToolKit.EndGroup();
			EditorGUI.EndDisabledGroup();

			//秘密キー
			EditorGUI.BeginDisabledGroup(!(createType != Type.CreateScenarioAssetOnly));
			GUILayout.Space(4f);
			UtageEditorToolKit.BeginGroup("Security");
			this.secretKey = EditorGUILayout.TextField("File Write Key", this.secretKey);
			bool isEnabelSecretKey = !string.IsNullOrEmpty(this.secretKey);
			EditorGUI.EndDisabledGroup();
			UtageEditorToolKit.EndGroup();

			bool isProjectNameEnable = IsEnableProjcetName(newProjectName);
			EditorGUI.BeginDisabledGroup(!isProjectNameEnable || isDisableLayer || !isEnabelSecretKey);
			bool isCreate = GUILayout.Button("Create", GUILayout.Width(80f));
			EditorGUI.EndDisabledGroup();
			if(isCreate) Create();
		}

		//新たなプロジェクトを作成
		void Create()
		{
			switch (createType)
			{
				case Type.CreateNewAdvScene:
					if (!WrapperUnityVersion.SaveCurrentSceneIfUserWantsTo()) return;
					break;
				default:
					break;
			}

			newProjectDir = ToProjectDir(newProjectName);

			UnityEngine.Profiling.Profiler.BeginSample("CopyTemplate");
			//テンプレートをコピー
			CopyTemplate();
			UnityEngine.Profiling.Profiler.EndSample();

			//プロジェクトファイルを作成
			string path = FileUtil.GetProjectRelativePath(newProjectDir +  newProjectName + ".project.asset");
			AdvScenarioDataProject ProjectData = UtageEditorToolKit.CreateNewUniqueAsset<AdvScenarioDataProject>(path);

			//プロジェクトにエクセルファイルを設定
			ProjectData.InitDefault(GetExcelRelativePath());
			//プロジェクトにカスタムインポートフォルダを設定
			ProjectData.AddCustomImportAudioFloders(LoadAudioFloders());
			ProjectData.AddCustomImportSpriteFloders(LoadSpriteFloders());
			ProjectData.AddCustomImportMovieFloders(LoadMovieFloders());
			//プロジェクトファイルを設定してインポート
			AdvScenarioDataBuilderWindow.ProjectData = ProjectData;
			AdvScenarioDataBuilderWindow.Import();

			UnityEngine.Profiling.Profiler.BeginSample("SceneEdting");
			switch (createType)
			{
				case Type.CreateNewAdvScene:
					//ADV用新規シーンを作成
					CreateNewAdvScene();
					break;
				case Type.AddToCurrentScene:
					//テンプレートシーンをコピー
					AddToCurrentScene();
					break;
				case Type.CreateScenarioAssetOnly:
					AssetDatabase.DeleteAsset(GetSceneRelativePath());
					break;
			}
			UnityEngine.Profiling.Profiler.EndSample();
		}

		//オーディオフォルダを取得
		List<Object> LoadAudioFloders()
		{
			List<Object> assetList = new List<Object>();

			string rootDir = GetProjectRelativeDir() + "/Resources/" + newProjectName + "/Sound/";
			assetList.Add(UtageEditorToolKit.LoadAssetAtPath<Object>(rootDir + "Ambience"));
			assetList.Add(UtageEditorToolKit.LoadAssetAtPath<Object>(rootDir + "BGM"));
			assetList.Add(UtageEditorToolKit.LoadAssetAtPath<Object>(rootDir + "SE"));
			assetList.Add(UtageEditorToolKit.LoadAssetAtPath<Object>(rootDir + "Voice"));

			return assetList;
		}

		//スプライトフォルダを取得
		List<Object> LoadSpriteFloders()
		{
			List<Object> assetList = new List<Object>();

			string rootDir = GetProjectRelativeDir() + "/Resources/" + newProjectName + "/Texture/";
			assetList.Add(UtageEditorToolKit.LoadAssetAtPath<Object>(rootDir + "BG"));
			assetList.Add(UtageEditorToolKit.LoadAssetAtPath<Object>(rootDir + "Character"));
			assetList.Add(UtageEditorToolKit.LoadAssetAtPath<Object>(rootDir + "Event"));
			assetList.Add(UtageEditorToolKit.LoadAssetAtPath<Object>(rootDir + "Sprite"));
			assetList.Add(UtageEditorToolKit.LoadAssetAtPath<Object>(rootDir + "Thumbnail"));

			return assetList;
		}

		//ムービーフォルダを取得
		List<Object> LoadMovieFloders()
		{
			List<Object> assetList = new List<Object>();

			//			assetList.Add(UtageEditorToolKit.LoadAssetAtPath<Object>(GetProjectRelativeDir() + "/Resources/" + newProjectName + "/Movie"));
			return assetList;
		}

		//ADV用新規シーンを作成
		void CreateNewAdvScene()
		{
			//シーンを開く
			string scenePath = GetSceneRelativePath();
			WrapperUnityVersion.OpenScene(scenePath);
			WrapperUnityVersion.SaveScene();

			//「宴」エンジンの初期化
			InitUtageEngine();
			FontChange(true);
			WrapperUnityVersion.SaveScene();
			Selection.activeObject = AssetDatabase.LoadAssetAtPath(scenePath, typeof(Object));
		}

		void AddToCurrentScene()
		{
			//すでにカメラがある場合は、宴関係のレイヤー設定を無効化する
			ChangeCameraMaskInScene();

			//すでにイベントシステムがある場合は、新しいほうを削除するために
			UnityEngine.EventSystems.EventSystem eventSystem = UtageEditorToolKit.FindComponentAllInTheScene<UnityEngine.EventSystems.EventSystem>();
			
			//シーンを開く
			string scenePath = GetSceneRelativePath();
			WrapperUnityVersion.OpenSceneAdditive(scenePath);


			//余分なオブジェクトを削除
			UtageUguiTitle title = UtageEditorToolKit.FindComponentAllInTheScene<UtageUguiTitle>();
			GameObject.DestroyImmediate(title.transform.root.gameObject);
			SystemUi systemUi = UtageEditorToolKit.FindComponentAllInTheScene<SystemUi>();
			GameObject.DestroyImmediate(systemUi.gameObject);

			//シーンのアセットを削除
			AssetDatabase.DeleteAsset(scenePath);

			//「宴」エンジンの初期化
			InitUtageEngine();

			//エンジン休止状態に
			AdvEngine engine = GameObject.FindObjectOfType<AdvEngine>();
			engine.gameObject.SetActive(false);

			ChangeLayerInCurrentScene();

			//すでにイベントシステムがある場合は、新しいほうを削除する
			if (eventSystem != null)
			{
				UnityEngine.EventSystems.EventSystem[] eventSystems = UtageEditorToolKit.FindComponentsAllInTheScene<UnityEngine.EventSystems.EventSystem>();
				foreach( var item in eventSystems )
				{
					if (item != eventSystem)
					{
						GameObject.DestroyImmediate(item.gameObject);
						break;
					}
				}
			}
			Selection.activeObject = AssetDatabase.LoadAssetAtPath(scenePath, typeof(Object));
			FontChange(false);
		}

		void ChangeCameraMaskInScene()
		{
			//すでにカメラがある場合は、宴関係のレイヤー設定を無効化する
			Camera[] cameras = UtageEditorToolKit.FindComponentsAllInTheScene<Camera>();

			List<string> changeLayers = new List<string>();
			if (OldLayerName != layerName)
			{
				changeLayers.Add(layerName);
				UtageEditorToolKit.TryAddLayerName(layerName);
			}
			if (OldLayerNameUI != layerNameUI)
			{
				changeLayers.Add(layerNameUI);
				UtageEditorToolKit.TryAddLayerName(layerNameUI);
			}
			int mask = LayerMask.GetMask(changeLayers.ToArray());
			foreach (Camera camera in cameras)
			{
				camera.cullingMask = camera.cullingMask & ~mask;
			}
		}

		void ChangeLayerInCurrentScene()
		{
			//レイヤー設定を変える
			AdvEngine engine = UtageEditorToolKit.FindComponentAllInTheScene<AdvEngine>();
			SwapLayerInChidren(engine.gameObject, OldLayerName, layerName);
			SwapLayerInChidren(engine.gameObject, OldLayerNameUI, layerNameUI);
			BootCustomProjectSetting projectSetting = UtageEditorToolKit.FindComponentAllInTheScene<BootCustomProjectSetting>();
			SwapLayerInChidren(projectSetting.gameObject, OldLayerNameUI, layerNameUI);

			foreach (Camera camera in projectSetting.GetComponentsInChildren<Camera>())
			{
				ChangeCameraLayer(camera);
			}
		}

		void ChangeCameraLayer( Camera camera )
		{
			switch (camera.gameObject.name)
			{
				case "SpriteCamera":
					SwapLayerInChidren(camera.gameObject, OldLayerName, layerName);
					camera.cullingMask = LayerMask.GetMask(new string[] { layerName });

					//AudioListenerが二つなら削除
					AudioListener[] audioListeners = UtageEditorToolKit.FindComponentsAllInTheScene<AudioListener>();
					if (audioListeners.Length > 1)
					{
						DestroyImmediate( camera.GetComponent<AudioListener>() );
					}
					break;
				case "UICamera":
				case "ClearCamera":
					SwapLayerInChidren(camera.gameObject, OldLayerNameUI, layerNameUI);
					camera.cullingMask = LayerMask.GetMask(new string[] { layerNameUI });
					break;
			}
		}

		void SwapLayerInChidren( GameObject go, string oldLayerName, string newLayerName)
		{
			int oldLayer = LayerMask.NameToLayer(oldLayerName);
			int newLayer = LayerMask.NameToLayer(newLayerName);

			foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
			{
				if( child.gameObject.layer == oldLayer )
				{
					child.gameObject.layer = newLayer;
				}
			}
		}

		//シーン内のAdvエンジンの初期設定
		void InitUtageEngine()
		{
			//シナリオデータの設定
			AdvEngine engine = GameObject.FindObjectOfType<AdvEngine>();
			AdvEngineStarter starter = GameObject.FindObjectOfType<AdvEngineStarter>();

//			AdvScenarioDataExported exportedScenarioAsset = UtageEditorToolKit.LoadAssetAtPath<AdvScenarioDataExported>(GetScenarioAssetRelativePath());
//			AdvScenarioDataExported[] exportedScenarioDataTbl = { exportedScenarioAsset };
			starter.InitOnCreate(engine, AdvScenarioDataBuilderWindow.ProjectData.Scenarios, newProjectName);
			starter.ScenarioDataProject = AdvScenarioDataBuilderWindow.ProjectData;

			UguiLetterBoxCamera[] cameras = GameObject.FindObjectsOfType<UguiLetterBoxCamera>();
			foreach (UguiLetterBoxCamera camera in cameras)
			{
				camera.Width = camera.MaxWidth = gameScreenWidth;
				camera.Height = camera.MaxHeight = gameScreenHeight;
			}

			//セーブファイルの場所の設定
			AdvSaveManager saveManager = GameObject.FindObjectOfType<AdvSaveManager>();
			saveManager.DirectoryName = "Save" + newProjectName;

			AdvSystemSaveData systemSaveData = GameObject.FindObjectOfType<AdvSystemSaveData>();
			systemSaveData.DirectoryName = "Save" + newProjectName;

			//シークレットキーの設定
			FileIOManager[] fileIOManagers = GameObject.FindObjectsOfType<FileIOManager>();
			foreach( FileIOManager item in fileIOManagers )
			{
				item.SetCryptKey(this.secretKey);
			}

			//シーン内の全てのテンプレートアセットをクローンアセットに置き換える
			ReplaceAssetsFromTempleateToCloneInSecne();
		}


		bool IsEnableProjcetName(string name)
		{
			if( string.IsNullOrEmpty(name) ) return false;
			if (System.IO.Directory.Exists(ToProjectDir(name))) return false;
			return true;
		}
		string ToProjectDir(string name)
		{
			return Application.dataPath + "/" + name + "/";
		}

		string GetProjectRelativeDir()
		{
			return "Assets/" + newProjectName;
		}
		string GetProjectRelativePath()
		{
			return GetProjectRelativeDir() + "/" + newProjectName;
		}
		string GetExcelRelativePath()
		{
			return GetProjectRelativePath() + ".xls";
		}
		string GetSceneRelativePath()
		{
			return GetProjectRelativePath() + ".unity";
		}
		string GetScenarioAssetRelativePath()
		{
			return GetProjectRelativePath() + AdvExcelImporter.ScenarioAssetExt;
		}


		void CopyTemplate()
		{
//			FileUtil.CopyFileOrDirectory(TemplateAssetsDir, GetProjectRelativeDir() );
			AssetDatabase.CopyAsset(TemplateAssetsDir, GetProjectRelativeDir());
			//リフレッシュ必須
			AssetDatabase.Refresh();
			//Templateというファイル名をリネーム
			foreach (string filePath in System.IO.Directory.GetFiles(newProjectDir, "*", SearchOption.AllDirectories))
			{
				if (Path.GetFileNameWithoutExtension(filePath) == TemplateName && Path.GetExtension(filePath) != ".meta")
				{
					string src = UtageEditorToolKit.SystemIOFullPathToAssetPath(filePath);
					string error = AssetDatabase.RenameAsset(src, newProjectName);
					if (!string.IsNullOrEmpty(error))
					{
						Debug.LogError(src + " " + error);
					}
				}
			}

			//Templateというフォルダ名をリネーム
			foreach (string dirPath in System.IO.Directory.GetDirectories(newProjectDir, "*", SearchOption.AllDirectories))
			{
				if (Path.GetFileName(dirPath) == TemplateName)
				{
					string src = UtageEditorToolKit.SystemIOFullPathToAssetPath(dirPath);
					string error = AssetDatabase.RenameAsset(src, newProjectName);
					if (!string.IsNullOrEmpty(error))
					{
						Debug.LogError(src + " " + error);
					}
				}
			}
			//アセットの再設定
			RebuildAssets();
		}
		//アセットの再設定
		void RebuildAssets()
		{
			//いったんアセットをリフレッシュ
			AssetDatabase.Refresh();
			//アセットの編集開始
			AssetDatabase.StartAssetEditing();

			Debug.Log("RebuildAssets･･･");
			RebuildAssetsSub();
			Debug.Log("...End RebuildAssets");

			//アセットの編集終了
			AssetDatabase.StopAssetEditing();
			//アセットのセーブ
			AssetDatabase.SaveAssets();
			//いったんアセットをリフレッシュ
			AssetDatabase.Refresh();
		}

		//テンプレートからコピーしたアセットの
		Dictionary<Object, Object> cloneAssetPair = new Dictionary<Object, Object>();		
		//アセットの再設定
		void RebuildAssetsSub()
		{
			cloneAssetPair = new Dictionary<Object, Object>();
			//クローンしたアセットにパッキングタグなどの必要な処置をして
			//テンプレートのアセットとのペアでリスト化する
			List<string> pathList = UtageEditorToolKit.GetAllAssetPathList(newProjectDir);
			foreach (string assetpath in pathList)
			{
				if (Path.GetExtension(assetpath) == ".unity") continue;

				//テンプレート（クローン元）のアセット
				string templatePath = assetpath.Replace(GetProjectRelativeDir() + "/", TemplateAssetsDir + "/");
				//クローンしたアセット
				foreach (Object clone in AssetDatabase.LoadAllAssetsAtPath(assetpath))
				{
					if (PrefabUtility.GetPrefabType(clone) == PrefabType.Prefab)
					{
						//プレハブの場合
						Object prefab = PrefabUtility.FindRootGameObjectWithSameParentPrefab(clone as GameObject) as Object;
						Object template = AssetDatabase.LoadAssetAtPath(templatePath, prefab.GetType());
						if (template != null)
						{
							if (cloneAssetPair.ContainsKey(template))
							{
								Debug.LogError(templatePath + " is already contains");
							}
							else
							{
								cloneAssetPair.Add(template, prefab);
							}
						}
						else
						{
							Debug.LogError(templatePath + " not found");
						}
						break;
					}
					else
					{
						Sprite sprite = clone as Sprite;
						if (sprite != null)
						{
							//スプライトの場合のみ
							//インポーターのパッキングタグを変える
							TextureImporter importer = AssetImporter.GetAtPath(assetpath) as TextureImporter;
							if (importer != null)
							{
								importer.spritePackingTag = newProjectName + "_UI";
								AssetDatabase.ImportAsset(assetpath);
								EditorUtility.SetDirty(importer);
							}
						}
						Object template = AssetDatabase.LoadAssetAtPath(templatePath, clone.GetType());
						if (template != null)
						{
							if (cloneAssetPair.ContainsKey(template))
							{
								Debug.LogError(templatePath + " is already contains");
							}
							else
							{
								cloneAssetPair.Add(template, clone);
							}
						}
					}
				}
			}

			//クローンしたプレハブやScriptableObject内にテンプレートアセットへのリンクがあったら、クローンアセットのものに変える
			foreach( Object obj in cloneAssetPair.Values )
			{
				bool isReplaced = false;
				if (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab)
				{
					//プレハブの場合
					GameObject go = obj as GameObject;
					foreach (Component component in go.GetComponentsInChildren<Component>(true))
					{
						isReplaced |= UtageEditorToolKit.ReplaceSerializedProperties(new SerializedObject(component), cloneAssetPair);
					}
				}
				else if (UtageEditorToolKit.IsScriptableObject(obj))
				{
					//ScriptableObjectの場合
					isReplaced |= UtageEditorToolKit.ReplaceSerializedProperties(new SerializedObject(obj), cloneAssetPair);
				}

				if (isReplaced)
				{
					EditorUtility.SetDirty(obj);
				}
			}
		}
		
		//シーン内の全てのテンプレートアセットをクローンアセットに置き換える
		void ReplaceAssetsFromTempleateToCloneInSecne()
		{
//			Debug.Log(System.DateTime.Now + " プレハブインスタンスを検索");
			//プレハブインスタンスを検索
			List<GameObject> prefabInstanceList = new List<GameObject>();
			foreach (GameObject go in UtageEditorToolKit.GetAllObjectsInScene())
			{
				if (PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
				{
					GameObject prefabInstance = PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);
					if(!prefabInstanceList.Contains(prefabInstance))
					{
						prefabInstanceList.Add(prefabInstance);
					}
				}
			}
//			Debug.Log(System.DateTime.Now + " prefabInstanceList");
			//プレハブインスタンスはいったん削除して、クローンプレハブからインスタンスを作って置き換える
			foreach (GameObject go in prefabInstanceList)
			{
				GameObject prefabAsset = PrefabUtility.GetPrefabParent(go) as GameObject;
				Object clonePrefabAsset;
				if (cloneAssetPair.TryGetValue(prefabAsset, out clonePrefabAsset))
				{
					GameObject cloneInstance = PrefabUtility.InstantiatePrefab(clonePrefabAsset) as GameObject;
					cloneInstance.transform.SetParent(go.transform.parent);
					GameObject.DestroyImmediate(go);
				}
				else
				{
					Debug.LogError( go.name + " Not Find Clone Prefab.");
				}
			}

//			Debug.Log(System.DateTime.Now + "ReplaceSerializedProperties");
			//オブジェクト内のコンポーネントからのリンクを全て、テンプレートからクローンに置き換える
			UtageEditorToolKit.ReplaceSerializedPropertiesAllComponentsInSene(cloneAssetPair);
//			Debug.Log(System.DateTime.Now);
		}

		//フォントを変更
		void FontChange( bool autoSave )
		{
			Font arialFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;

			if (this.UiFont == arialFont) return;
			if (this.UiFont == null) return;

			//シーンをセーブ
			if (autoSave)
			{
				WrapperUnityVersion.SaveScene();
			}
			else
			{
				if (!WrapperUnityVersion.SaveCurrentSceneIfUserWantsTo())
				{
					return;
				}
			}

			Debug.Log("Font Change Arial to " + this.UiFont.name);
			AssetDatabase.Refresh();
			ReferenceAssetChanger.FindAndChangeAll(arialFont, UiFont, this.newProjectDir);
		}
	}
}
