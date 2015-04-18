using UnityEngine;
using System.Collections;

/// <summary>
/// 画面质量控制
/// </summary>
public class ssGFXQuality : MonoBehaviour
{
	public static ssGFXQuality Instance
	{
		get
		{
			if (instance == null)
			{
                GameObject go = Resources.Load("GFX/ssGFXQuality") as GameObject;
				instance = go.GetComponent<ssGFXQuality>();

				instance.Initialize();
			}

			return instance;
		}
	}
	private static ssGFXQuality instance;


	

	public enum EQualityLevel
	{
		QL_Ultra,
		QL_High,
		QL_Middle,
		QL_Low,
		QL_VeryLow
	}

	[System.Serializable]
	public class Settings
	{
		public int Level = 0;

		// 分辨率比率
		public float ResolutionScale = 1.0f;

		public bool HideAllEffect = false;

        public bool LowQuality = false;

		public bool OptimizeUIUpdate = false;

		public bool OptimizeAnimUpdate = false;

		public void CopyFrom(Settings src)
		{
			Level = src.Level;
			ResolutionScale = src.ResolutionScale;
            HideAllEffect = src.HideAllEffect;
            LowQuality = src.LowQuality;

			OptimizeUIUpdate = src.OptimizeUIUpdate;
			OptimizeAnimUpdate = src.OptimizeAnimUpdate;
		}
	}

	public void Initialize()
	{

#if !UNITY_EDITOR
#if UNITY_IPHONE
        CachedGO = gameObject;
        Selector = CachedGO.GetComponent<ssGFXQualitySelector_iOS>();
#endif

#if UNITY_ANDROID
        CachedGO = gameObject;
        Selector = CachedGO.GetComponent<ssGFXQualitySelector_Android>();
#endif
#endif

		FullWidth = Screen.width;
		FullHeight = Screen.height;
		FullDPI = Screen.dpi;
		bFullScreen = Screen.fullScreen;
		bGameMode = false;

		ReInitialize();

		RunningAvailMem = Utility.GetAvailMemory();

		//QualitySettings.vSyncCount = 2;
		//CurrentSettings.HideAllEffect = true;
		//CurrentSettings.LowQuality = true;
		//CurrentSettings.OptimizeAnimUpdate = true;
		//CurrentSettings.OptimizeUIUpdate = true;

        //ssLogger.Log("Quality Level:{0}\nHide All Effect:{1}\nLowQuality:{2}\nOptimizeAnimUpdate:{3}",
        //    CurrentSettings.Level,
        //    CurrentSettings.HideAllEffect,
        //    CurrentSettings.LowQuality,
        //    CurrentSettings.OptimizeAnimUpdate);
    }

	public void ReInitialize()
	{
        if (Selector)
        {
            Selector.Initialize();
            Apply(Selector.InitQuality);            
        }
	}

	private void Apply(EQualityLevel Quality)
	{
		if (CurrentSettings.Level != (int)Quality)
		{
			Apply(SettingsGroup[(int)Quality]);
		}
	}

	void Apply(Settings InSettings)
	{
#if !UNITY_IPHONE
		int width = Mathf.RoundToInt(FullWidth * InSettings.ResolutionScale);
		int height = Mathf.RoundToInt(FullHeight * InSettings.ResolutionScale);
		Screen.SetResolution(width, height, bFullScreen);
#endif
		CurrentSettings.CopyFrom(InSettings);
	}

	public void ApplyLayerCullingSetting()
	{
		
	}

	public void Tick()
	{
		if (Selector)
		{
			Selector.Tick();
		}
	}

	public void OnAppPause(bool bPause)
	{
		if (Selector)
		{
			Selector.OnAppPause(bPause);
		}
	}

	public void OnEnterGameMode()
	{
		bGameMode = true;

#if !UNITY_EDITOR
		Application.targetFrameRate = 30;
#else
        Application.targetFrameRate = -1;
#endif

	}

	public void OnExitGameMode()
	{
		bGameMode = false;

#if !UNITY_EDITOR
		Application.targetFrameRate = 30;
#else
        Application.targetFrameRate = -1;
#endif
	}

	void _ApplyHideDecorationLayer(bool bHide)
	{

	}

	public Settings[] SettingsGroup = new Settings[5];

	public EQualityLevel CurrentLevel
	{
		get { return (EQualityLevel)CurrentSettings.Level; }
	}
	public Settings CurSetting
	{
		get { return CurrentSettings; }
	}
	Settings CurrentSettings = new Settings();
#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
    GameObject CachedGO;
#endif
	ssGFXQualitySelector Selector = null;

	// 运行时的可用内存 November
	public float RunningAvailMem
	{
		get;
		private set;
	}

	// 是否在低内存状态下运行 November 
	public bool LowMemMode
	{
		get { return RunningAvailMem < 120 && RunningAvailMem > 0; } // 要算上初始加载的差不多40M内存，就是160M
	}

	public int ScreenWidth
	{
		get
		{
			return FullWidth;
		}
	}
	int FullWidth;

	public int ScreenHeight
	{
		get
		{
			return FullHeight;
		}
	}
	int FullHeight;

	public float ScreenDPI
	{
		get
		{
			return FullDPI;
		}
	}
	float FullDPI;

	public bool OptimizeUIUpdate
	{
		get { return CurrentSettings.OptimizeUIUpdate && bGameMode; }
	}

	public bool OptimizeAnimUpdate
	{
		get { return CurrentSettings.OptimizeAnimUpdate && bGameMode; }
	}

	bool bFullScreen;
	bool bGameMode;

	public bool InGameMode
	{
		get { return bGameMode; }
	}
}
