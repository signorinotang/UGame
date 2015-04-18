using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ssLogger
{
#if UNITY_EDITOR
    public static bool EnableCommonLog = true;
#else
    public static bool EnableCommonLog = false;
#endif

    public static void Log(string format, params object[] args)
    {
#if !DISTRIBUTION_VERSION
        if (!EnableCommonLog)
        {
            return;
        }
        string message = format;
        try
        {
            message = System.String.Format(format, args);
        }
        catch 
        {
            for (int i = 0; i < args.Length; i++)
            {
                message += args[i].ToString();
            }
        }
        Debug.Log(message);
#endif
    }

    public static void Log(object message, Object context)
    {
#if !DISTRIBUTION_VERSION
        if (!EnableCommonLog)
        {
            return;
        }
        Debug.Log(message, context);
#endif
    }

    public static void LogWarning(string format, params object[] args)
    {
#if !DISTRIBUTION_VERSION
        string message = format;
        try
        {
            message = System.String.Format(format, args);
        }
        catch
        {
            for (int i = 0; i < args.Length; i++)
            {
                message += args[i].ToString();
            }
        }
        Debug.LogWarning(message);
        RealTimeLog.LogError(message.ToString());     
#endif
    }

    public static void LogWarning(object message, Object context)
    {
#if !DISTRIBUTION_VERSION
            Debug.LogWarning(message, context);
            RealTimeLog.LogError(message.ToString());
#endif
    }

    public static void LogError(string format, params object[] args)
    {
#if !DISTRIBUTION_VERSION
        string message = format;
        try
        {
            message = System.String.Format(format, args);
        }
        catch
        {
            for (int i = 0; i < args.Length; i++)
            {
                message += args[i].ToString();
            }
        }
        Debug.LogError(message);
        RealTimeLog.LogError(message.ToString());
#endif
    }

    public static void LogError(object message, Object context)
    {
#if !DISTRIBUTION_VERSION
            Debug.LogError(message, context);
#endif
    }


}

