#define COMPARE_INSTANCE_ID

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Object = UnityEngine.Object;


/// <summary>
/// 常量/工具函数
/// </summary>
public static class ssUtil
{
    public static readonly Vector3 ZeroVect = new Vector3(0, 0, 0);
    public static readonly Vector3 OneVect = new Vector3(1, 1, 1);
    public static System.DateTime InitTime = new System.DateTime(1970, 1, 1);
    public static readonly Quaternion IdentityQuat = new Quaternion(0, 0, 0, 1);
    public static readonly string EmptyName = "_empty_name_";
    public static readonly string ZeroString = "0";
    public static System.Int64 ServerTime = 0;   // 服务器时间, 自1970年1月1日的秒数
    public static float lastUpdateSeverTime = 0;

    public static bool InitFirstRunFail = false;
    public static string AppMd5 = string.Empty;

    //public static Texture2D DefaultPlayerPortrait = Resources.Load("UI/DefaultPortrait/NormalIcon.png") as Texture2D;
    //public static Texture2D OfficialPortrait = Resources.Load("UI/DefaultPortrait/OfficialPortrait.png") as Texture2D;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_ANDROID
    private const string mDllname = "mono";
#else
    private const string mDllname = "__Internal";
#endif

    [DllImport(mDllname)]
    public static extern long mono_gc_get_used_size();

    [DllImport(mDllname)]
    public static extern long mono_gc_get_heap_size();

#if UNITY_IPHONE && !UNITY_EDITOR
    //[DllImport("__Internal")]
	//extern static void ShowAlertDialog_iOS(string title, string message, string cancelButtonTitle);
#endif

    public delegate void ProgressNotifyDelegate(float percent);

    public static IEnumerator PrepareForRun( ProgressNotifyDelegate notifyProgress )
    {
        string srcPath = Utility.LocalStoragePath;
		
#if UNITY_IPHONE
            srcPath = Application.dataPath + "/Raw";    
#elif UNITY_EDITOR || UNITY_STANDALONE_WIN
		srcPath = Application.streamingAssetsPath;
#endif
		
		string oggSrcPath = srcPath + "/Sounds_Ogg/";
        string targetPath = srcPath + "/Sounds/";

#if UNITY_IPHONE
            targetPath = Utility.LocalStoragePath + "/Sounds/";    
#endif
        if (!InitFirstRunFail)
        {
            var enmrt = ssUtil.ConvertOgg2Wav(oggSrcPath, targetPath, notifyProgress);
            while(enmrt.MoveNext())
            {
                yield return enmrt.Current;
            }	
        }

        if (!InitFirstRunFail)
        {
			int Ver = StreamingVer.ReadResourceVer(EResourceVerType.Sound);
			StreamingVer.WriteStreamingVer(EResourceVerType.Sound, Ver);
        }

#if !UNITY_EDITOR
		if ( Directory.Exists(oggSrcPath) && !oggSrcPath.Contains("/Raw/Sounds_Ogg/") )
        {
            SafeDeleteDirectory(oggSrcPath, true);
        }
#endif

		yield return null;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
	// 把Databin搞出来 后边都从外边读不会把java堆肚子搞大
	public static IEnumerator AndroidPrepareForDatabin()
	{
		// 读List
		WWW list = new WWW(Utility.streamingAssetsPath + "Databin/List.bytes");
		yield return list;

		string destpath_list = Utility.LocalStoragePath + "/Databin/List.bytes";
		destpath_list = destpath_list.Replace("jar:file:/", "");

		// 创建Databin文件夹
		string DatabinDic = destpath_list.Remove(destpath_list.LastIndexOf("/"));
		Directory.CreateDirectory(DatabinDic);

		// 写入List
		FileStream fs_list = File.Create(destpath_list);
		fs_list.Write(list.bytes, 0, list.bytes.Length);
		fs_list.Close();

		// 读List
		string[] paths = list.text.Split(new char[2] { '\r', '\n' });

		// 写入Databin
		for (int i = 0; i < paths.Length; i++ )
		{
			string path = paths[i];
			if (string.IsNullOrEmpty(path))
				continue;

			WWW file = new WWW(Utility.streamingAssetsPath + "Databin/" + path + ".bin");
			yield return file;

			string destpath = Utility.LocalStoragePath + "/Databin/" + path + ".bin";
			destpath = destpath.Replace("jar:file:/", "");

			string destdic = destpath.Remove(destpath.LastIndexOf("/"));
			Directory.CreateDirectory(destdic);

			FileStream fs = File.Create(destpath);
			fs.Write(file.bytes, 0, file.bytes.Length);
			fs.Close();

			file.Dispose();
		}

		list.Dispose();
	}

	// 把Sound_Ogg里的文件都解压出来
	public static IEnumerator AndroidPrepareForSound(ProgressNotifyDelegate notifyProgress)
	{
		// 读List
		WWW list = new WWW(Utility.streamingAssetsPath + "Sounds_Ogg/List.bytes");
		yield return list;

		string destpath_list = Utility.LocalStoragePath + "/Sounds_Ogg/List.bytes";
		destpath_list = destpath_list.Replace("jar:file:/", "");

		// 创建Sounds_Ogg文件夹
		string OggDic = destpath_list.Remove(destpath_list.LastIndexOf("/"));
		Directory.CreateDirectory(OggDic);

		// 写入List
		FileStream fs_list = File.Create(destpath_list);
		fs_list.Write(list.bytes, 0, list.bytes.Length);
		fs_list.Close();

		// 读List
		string[] paths = list.text.Split(new char[2] { '\r', '\n' });

		// 写入ogg
		for (int i = 0; i < paths.Length; i++ )
		{
			string path = paths[i];
			if (string.IsNullOrEmpty(path))
				continue;

			WWW file = new WWW(Utility.streamingAssetsPath + "Sounds_Ogg/" + path + ".bytes");
			yield return file;

			string destpath = Utility.LocalStoragePath + "/Sounds_Ogg/" + path + ".bytes";
			destpath = destpath.Replace("jar:file:/", "");

			string destdic = destpath.Remove(destpath.LastIndexOf("/"));
			Directory.CreateDirectory(destdic);

			FileStream fs = File.Create(destpath);
			fs.Write(file.bytes, 0, file.bytes.Length);
			fs.Close();

			file.Dispose();

			if (notifyProgress != null)
			{
				notifyProgress((float)(i + 1) / (float)paths.Length);
			}
		}

		list.Dispose();
	}

	// 把Font合并了
	public static IEnumerator AndroidPrepareForFont(ProgressNotifyDelegate notifyProgress)
	{
		// 复制numer.ttf
		WWW number = new WWW(Utility.streamingAssetsPath + "Font/number.ttf");
		yield return number;

		string destpath_number = Utility.AndroidNativeStoragePath + "/Font/number.ttf";
		destpath_number = destpath_number.Replace("jar:file:/", "");

		string FontDic = destpath_number.Remove(destpath_number.LastIndexOf("/"));

         // 写入,重试3次
        bool success = false;
	    for (int i = 0; i < 3; i++)
	    {
	        try
	        {
                Directory.CreateDirectory(FontDic);
                success = true;
	        }
	        catch
	        {
	        }
	        if (success)
	        {
	            break;
	        }
	        yield return new WaitForSeconds(0.5f);
	    }
	    if (!success)
	    {
	        InitFirstRunFail = true;
	        yield break;
	    }


	    // 写入,重试3次
	    success = false;
	    for (int i = 0; i < 3; i++)
	    {
	        if (Utility.SafeWriteFile(destpath_number, number.bytes))
	        {
	            success = true;
	            break;
	        }
	        yield return new WaitForSeconds(0.5f);
	    }
	    if (!success)
	    {
	        InitFirstRunFail = true;
	        yield break;
	    }


		// 复制vdFont.ttf子文件
		WWW list = new WWW(Utility.streamingAssetsPath + "Font/List.bytes");
		yield return list;

		string destpath = Utility.AndroidNativeStoragePath + "/Font/vdFont.ttf";
		destpath = destpath.Replace("jar:file:/", "");

		//string destdic = destpath.Remove(destpath.LastIndexOf("/"));
		//Directory.CreateDirectory(destdic);

		FileStream fs = File.Create(destpath);
		
		string[] paths = list.text.Split(new char[2] { '\r', '\n' });

		for (int i = 0; i < paths.Length; i++)
		{
			string path = paths[i];
			if (string.IsNullOrEmpty(path))
				continue;

			WWW file = new WWW(Utility.streamingAssetsPath + "Font/" + path + ".bytes");
			yield return file;

			fs.Write(file.bytes, 0, file.bytes.Length);

			if (notifyProgress != null)
			{
				notifyProgress((float)(i + 1) / (float)paths.Length);
			}

			file.Dispose();
		}

		fs.Close();

		list.Dispose();

		number.Dispose();
	}
#endif

    //     public static bool NeedPrefpareforMiniPackage
// 	{
// 		get
// 		{
// 			return Downloader.Instance().HasMiniPackageTask()
    // 				|| Directory.Exists(Utility.LocalStoragePath + "/Sounds_Ogg/");
// 		}
// 	}

// 	public static bool FirstRun
//     {
//         get
//         {
// 			int Ver = StreamingVer.ReadResourceVer(EResourceVerType.Sound);
// 			if (Ver == StreamingVer.ReadStreamingVer(EResourceVerType.Sound))
// 			{
// 				return false;
// 			}
// 			else
// 			{
// 				return true;
// 			}
//         }
//     }

    public static void GenerateFileList(string DirPath)
    {
#if !UNITY_WEBPLAYER
        string path = Application.dataPath + "/StreamingAssets/" + DirPath + "/";
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

        StreamWriter sw = new StreamWriter(path + "/" + "List.txt");

        foreach (string file in files)
        {
            if (!file.EndsWith(".meta") && !file.Contains(".svn") && !file.Contains("List.txt"))
            {
                FileInfo f = new FileInfo(file);
                string name = f.FullName.Remove(0, path.Length);
                name = name.Remove(name.LastIndexOf('.'));
                name = name.Replace('\\', '/');
                sw.WriteLine(name);
            }
        }

        sw.Close();
#endif
    }

    public static string GetLatestFile(string path, string searchPattern)
    {
        string result = null;

#if !UNITY_WEBPLAYER
        string[] files = Directory.GetFiles(path, searchPattern);

        long minTicks = long.MaxValue;
        foreach ( string file in files )
        {
            System.TimeSpan ts = System.DateTime.Now - File.GetLastWriteTime(file);
            if ( ts.Ticks < minTicks )
            {
                minTicks = ts.Ticks;
                result = file;
            }
        }
#endif

        return result;
    }

    public static void DeleteFiles(string path, string searchPattern)
    {
#if !UNITY_WEBPLAYER
        string[] files = Directory.GetFiles(path, searchPattern);

        foreach (string file in files)
        {
            try
            {
                File.Delete(file);
            }
            catch (System.Exception ex)
            {
                //ssLogger.Log(ex.ToString());
            }
        }
#endif
    }

    public static void SafeDeleteDirectory(string path, bool recursive)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }
        catch(System.Exception e)
        {
            ssLogger.LogWarning(e.ToString());
        }
    }

#if UNITY_IPHONE
    [DllImport("__Internal")]
#else
    [DllImport("Ogg2WavPlugin")]
#endif
    extern public static int PluginConvertOgg2Wav( string oggFile, string wavFile );
    public static IEnumerator ConvertOgg2Wav(string Path, string targetPath, ProgressNotifyDelegate notifyProgress)
    {
        string listFilePath = Path + "/List.bytes";
        StreamReader sr = new StreamReader(listFilePath);
        List<string> fileList = new List<string>();
        while (!sr.EndOfStream)
        {
            fileList.Add(sr.ReadLine());
        }
        sr.Close();
        int fileCount = fileList.Count;
        for (int i = 0; i < fileCount; ++i)
        {
            string filePath = fileList[i];
            FileInfo waveFileInfo = new FileInfo(targetPath + filePath + ".wav");
#if UNITY_EDITOR
            if (waveFileInfo.Exists)
            {
                continue;
            }
#endif
#if !UNITY_WEBPLAYER
            if (!waveFileInfo.Directory.Exists)
            {
                waveFileInfo.Directory.Create();
            }
#endif

            string srcFullPath = Path + filePath + ".bytes";
            if (File.Exists(srcFullPath))
            {
				PluginConvertOgg2Wav(srcFullPath, waveFileInfo.FullName);
            }
            else
            {
                ssLogger.LogWarning("Can't find ogg file :" + srcFullPath);
                if (!InitFirstRunFail)
                {
                    InitFirstRunFail = true;
					//ssUtil.ShowAlertDialog(
					//	Localization.instance.Get("AlertDialogTitle"),
					//	Localization.instance.Get("InitFailedAlertMessage"),
					//	Localization.instance.Get("AlertDialogCancelButtonTitle"));

					ssLogger.LogError("Timi Init Failed!");
                }
            }

            if (notifyProgress != null)
            {
                notifyProgress((float)(i + 1) / (float)fileCount);
                yield return null;
            }
        }
    }

	public static string RetriveName(string url)
	{
		int last = url.LastIndexOf('.', url.Length - 1);                        // 得到后最位置”
		string sufix = url.Substring(last, url.Length - last);      // 得到后缀
		last = url.LastIndexOf('/', url.Length - (sufix.Length + 1));                        // 去掉最后3个字符 “/40”
		return url.Substring(last + 1, url.Length - last - (sufix.Length + 1));   // 得到头像图片的MD5串作为缓存名称
	}

	/// <summary>
	/// 获取CPU的最高频率
	/// </summary>
	/// <returns></returns>
	public static int MaxCPUFreq
	{
		get
		{
			if (_MaxCPUFreq == -1)
			{
#if !UNITY_EDITOR
#if UNITY_ANDROID
        System.IntPtr c = AndroidJNI.FindClass("com/tencent/tmgp/vdefense/GamePluginManager");
        System.IntPtr ptr = AndroidJNI.GetStaticMethodID(c, "GetCpuFreq", "()Ljava/lang/String;");
        jvalue[] args = new jvalue[0];
        string str = AndroidJNI.CallStaticStringMethod(c, ptr, args);
        if ( str != "N/A" )
        {
            long freq = -1;
            if ( long.TryParse( str, out freq ) )
            {
                _MaxCPUFreq = (int)(freq / 1000);
            }
        }
#endif
#endif
            }

			return _MaxCPUFreq;
		}
	}
	static int _MaxCPUFreq = -1;

	public static string Model
	{
		get
		{
			if (_Model == "")
			{
#if !UNITY_EDITOR
#if UNITY_ANDROID
        System.IntPtr c = AndroidJNI.FindClass("com/tencent/tmgp/vdefense/GamePluginManager");
        System.IntPtr ptr = AndroidJNI.GetStaticMethodID(c, "GetModel", "()Ljava/lang/String;");
        jvalue[] args = new jvalue[0];
        string str = AndroidJNI.CallStaticStringMethod(c, ptr, args);
        if ( str != "N/A" )
        {
           _Model = str;
        }
#endif
#endif
            }
			return _Model;
		}
	}
	static string _Model = "";


    /// <summary>
    /// 设置TargetFPS唯一入口,方便调试
    /// </summary>
    /// <param name="fps"></param>
    public static void SetTargetFPS(int fps)
    {
        Application.targetFrameRate = fps;
    }

    /// <summary>
    /// 弹出系统警示对话框
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="cancelButtonTitle"></param>
    public static void ShowAlertDialog(string title, string message, string cancelButtonTitle)
    {

    }
	
    public static string GetExpireString(uint exptime)
    {
        Localization local = Localization.instance;
        uint day = 86400;
        if (exptime < 60)
            return exptime.ToString() + local.Get("seconds") ;          // 60内显示秒数
        else if (exptime < 3600)
            return (exptime / 60).ToString() + local.Get("minutes");    // 1小时内显示分钟
        else if (exptime < day)
            return (exptime / 3600).ToString() + local.Get("hours");    // 1天内显示小时
        else if (exptime % day == 0 )
            return (exptime / day).ToString() + local.Get("day");
        else
            return (exptime / day + 1).ToString() + local.Get("day");   // 24小时以上，显示为天数，1天零1分钟显示为2天，29天显示为30天

    }

    public static int GetCurrentServerTime()
    {
        return (int)(ServerTime + Time.realtimeSinceStartup - lastUpdateSeverTime);
    }

    public static string GetRestTime(uint obtainTime, int validTime)       // 返回物品过期时间
    {
        int curSvrTime = GetCurrentServerTime();
        int ExpireTime = (int)(validTime + obtainTime);
        if (ExpireTime - curSvrTime <= 0f)
            return Localization.instance.Get("alreadyexpired");
        else
            return GetExpireString((uint)(ExpireTime - curSvrTime)) + Localization.instance.Get("Later") + Localization.instance.Get("expire");
    }

    public static IEnumerator DumpAssets( System.Type assetType )
    {
        yield return Resources.UnloadUnusedAssets();
#if !UNITY_WEBPLAYER
        string filePath = Path.Combine(Application.persistentDataPath, assetType.ToString()+".log");
        //ssLogger.Log(filePath);
        StreamWriter sw = File.CreateText(filePath);
       
        Object[] assetArray = Resources.FindObjectsOfTypeAll( assetType );
        int len = assetArray.Length;
        float totalSize = 0;
        System.Array.Sort(assetArray, SortBySize);
        for (int i = 0; i < len;  ++i)
        {
            Object obj = assetArray[i];
            float sizeKB = Profiler.GetRuntimeMemorySize(obj) / 1024.0f;
            totalSize += sizeKB;
            sw.WriteLine(obj.name + "(" + obj.GetInstanceID() + ")" + ": " + sizeKB + " kB");
        }

        sw.WriteLine();

        sw.WriteLine("TotalCount: " + len);
        sw.WriteLine("TotalSize: " + totalSize + " kB");

        sw.WriteLine("Mono heap used size: " + mono_gc_get_used_size()/1024.0f + " kB");
        sw.WriteLine("Mono heap size: " + mono_gc_get_heap_size()/1024.0f + " kB");

        sw.Close();
#endif
    }

    public static float GetMonoHeapSize()
    {
        return mono_gc_get_heap_size() / 1024f / 1024f;
    }

    public static float GetMonoHeapUsedSize()
    {
        return mono_gc_get_used_size() / 1024f / 1024f;
    }

    static int SortBySize(Object a, Object b)
    {
        return Profiler.GetRuntimeMemorySize(b) - Profiler.GetRuntimeMemorySize(a);
    }

    public static string GetMD5(byte[] data)
    {
        var md5 = new MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(data);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("X2"));
        }
        return sb.ToString();
    }


    struct UnityObjectInfo
    {
        public string Name;
        public int ID;

        public override bool Equals(object obj)
        {
            if ( obj == null ||
                !(obj is UnityObjectInfo))
            {
                return false;
            }

            UnityObjectInfo rhs = (UnityObjectInfo)obj;

#if COMPARE_INSTANCE_ID
            if ( ID == rhs.ID )
            {
                return true;
            }
#endif
            if ( Name == rhs.Name )
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        
    }
}
