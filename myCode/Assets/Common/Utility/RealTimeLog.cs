using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Common;


public class RealTimeLog : MonoSingleton<RealTimeLog>
{    
    const int MaxNum = 10;
    const float StayTime = 20;
    public static LogLevel Level = LogLevel.Error;
    GUIStyle InfoStyle;
    GUIStyle ErrorStyle;
    int FontSize = 20;
    int Height = 20;

    public enum LogLevel
    {
        Info,
        Error,
        None,
    }
    struct LogItem
    {
        public float time;
        public string log;
        public LogLevel level;
    }

    void Start()
    {
        FontSize = (Screen.height - 100) / MaxNum;
        Height = FontSize + 5;
        InfoStyle = new GUIStyle();
        InfoStyle.fontSize = FontSize;
        InfoStyle.normal.textColor = Color.green;

        ErrorStyle = new GUIStyle();
        ErrorStyle.fontSize = FontSize;
        ErrorStyle.normal.textColor = Color.red;     
        string str = PlayerPrefs.GetString("RealTimeLog");
        if(!string.IsNullOrEmpty(str))
        {
            Level = (LogLevel)Enum.Parse(typeof(LogLevel), str);
        }       
    }


    public static void Log(string log)
    {
#if VD_RealTimeLog
        if (Level > LogLevel.Info || !Application.isPlaying)
        {
            return;
        }
        RealTimeLog.Instance().LogImpl(log,LogLevel.Info);
#endif
    }

    public static void LogError(string log)
    {
#if VD_RealTimeLog
        if (Level > LogLevel.Error || !Application.isPlaying)
        {
            return;
        }
        RealTimeLog.Instance().LogImpl(log,LogLevel.Error);
#endif
    }

    private List<LogItem> AllLogs = new List<LogItem>();

    private void LogImpl(string log,LogLevel level)
    {
        if (level < Level)
        {
            return;
        }
        if (AllLogs.Count > MaxNum)
        {
            AllLogs.RemoveAt(0);
        }
        LogItem item;
        item.time = Time.realtimeSinceStartup;
        item.log = log;
        item.level = level;
        AllLogs.Add(item);      
    }

#if VD_RealTimeLog
    void Update()
    {
        if (AllLogs.Count > 0)
        {
            if (Time.realtimeSinceStartup - AllLogs[0].time > StayTime)
            {
                AllLogs.RemoveAt(0);
            }
        }
    }

	void OnGUI()
	{

		for (int i = 0; i < AllLogs.Count; i++)
		{
			LogItem item = AllLogs[i];
			GUIStyle style = null;
			if (item.level == LogLevel.Error)
			{
				style = ErrorStyle;
			}
			else
			{
				style = InfoStyle;
			}
			string slog = string.Format("{0} {1}", item.time.ToString("0.00"), item.log);
			GUI.Label(new Rect(20, Screen.height - 80 - i * Height, Screen.width - 200, Height), slog, style);
		}
	}
#endif
}