using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

public static class Utility
{
    
#if UNITY_ANDROID
    private const string PackageName = "com/tencent/tmgp/vdefense/GamePluginManager";
    private static IntPtr GamePluginMgrPtr;
#endif

    static Utility()
    {
#if UNITY_ANDROID
        IntPtr ptr = IntPtr.Zero;
        ptr = AndroidJNI.FindClass(PackageName); if (ptr != IntPtr.Zero) GamePluginMgrPtr = AndroidJNI.NewGlobalRef(ptr);
#endif
    }

    public class ClassWrapper<T>
    {
        public T Value;
    }

    // 包内文件
    // 返回扩展数据目录（bytes文件 和 字体）
    public static string streamingAssetsPath
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Application.streamingAssetsPath + "/";
#elif UNITY_IPHONE && !UNITY_EDITOR
            return string.Format("file://{0}/Raw/", Application.dataPath);
#else
            return string.Format("file://{0}/", Application.streamingAssetsPath);
#endif
        }
    }
	
    /// <summary>
    /// 包外文件
    /// 本地可读可写得一个
    /// 对于iOS版本，直接放在Documents/Caches/下
    /// 对于Editor以及Standalone版本，为了区分不同分支，使用了相对与工程Assets的一个路径
    /// 对于Android版本，从Java层获取使用内部存储里面的Caches目录
    /// </summary>
    /// <returns></returns>
    public static string LocalStoragePath
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return Directory.GetParent(Application.dataPath).FullName + "/Caches";
            //return Application.dataPath + "/../Caches"; 
#elif UNITY_IPHONE
            return Application.temporaryCachePath;
#elif UNITY_ANDROID
            return AndroidLocalStoragePath;
#endif
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// 在Android设备上返回必定可以读写的路径，这个路径在没有SD卡的情况下可以依然正常工作
    /// Application.temporaryCachePath 返回 /mnt/sdcard/Android/data/com.tencent.game.SSGame/cache
    /// 我们需要 /data/data/com.tencent.game.SSGame/cache/Assets
    /// </summary>
    /// <returns></returns>
    private static string sAndroidNativeStoragePath = null;
    private static string sAndroidSDCardStoragePath = null;
    private static bool? sUseSDCardStorage = null;
    
    public static string AndroidNativeStoragePath
    {
        get
        {
            if (string.IsNullOrEmpty(sAndroidNativeStoragePath))
            {
                AndroidJNI.AttachCurrentThread();
                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
                string path = jo.Call<AndroidJavaObject>("getCacheDir").Call<string>("getCanonicalPath");
                sAndroidNativeStoragePath = path.Replace("cache", "files") + "/assets";
            }
            return sAndroidNativeStoragePath;
        }
    }

    public static string AndroidSDCardStoragePath
    {
        get
        {
            if (string.IsNullOrEmpty(sAndroidSDCardStoragePath))
            {
                sAndroidSDCardStoragePath= GetAndroidSDCardRoot() 
                    + "/Android/data/com.tencent.tmgp.vdefense/files/assets";
            }
            return sAndroidSDCardStoragePath;
        }
    }
    
    private static long MinSDCardUsageKb = 300 * 1024;
    public static bool UseSDCardStorage
    {
        get
        {
            if (sUseSDCardStorage == null)
            {
                var flagBytes = SafeReadFile(AndroidNativeStoragePath + "/StorageFlag");
                int flag = 0;
                if (flagBytes != null)
                {
                    var flagStr =  StringUtility.UTF8BytesToString(flagBytes);
                    int.TryParse(flagStr, out flag);
                }
                if (flag != 1 && flag != 2)
                {
                    long sdCardFree = Utility.GetAndroidSDFreeSizeKb();
                    long sysFree = Utility.GetAndroidSystemFreeSizeKb();
                    //ssLogger.Log("sdCardFree:{0}kb, SysFree:{1}kb", sdCardFree, sysFree);
                    flag = (IsExistSDCard() 
                        && sysFree < MinSDCardUsageKb 
                        && sdCardFree >= MinSDCardUsageKb) 
                        ? 2 : 1;
                }
                SafeWriteFile(AndroidNativeStoragePath + "/StorageFlag",
                    StringUtility.StringToUTF8Bytes(flag.ToString()));
                
                sUseSDCardStorage = (flag == 2);
            }
            return sUseSDCardStorage.Value;
        }
        set
        {
            int flag = value ? 2 : 1;
            SafeWriteFile(AndroidNativeStoragePath + "/StorageFlag", 
                StringUtility.StringToUTF8Bytes(flag.ToString()));
            sUseSDCardStorage = value;
        }
    }

    public static string AndroidLocalStoragePath
    {
        get
        {
            return UseSDCardStorage ? AndroidSDCardStoragePath : AndroidNativeStoragePath;
        }
    }

#endif


    //StreamingAsset目录对应URL
    public static string GetStreamingAssetsURL(string reletivePath, string fileName)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return string.Format("file://{0}/{1}/{2}", Application.streamingAssetsPath, reletivePath, fileName);
#elif UNITY_ANDROID
        return string.Format("jar:file://{0}!/assets/{1}/{2}", Application.dataPath, reletivePath, fileName);
#elif UNITY_IPHONE
        return string.Format("file://{0}/Raw/{1}/{2}", Application.dataPath, reletivePath, fileName);
#endif
    }

    //StreamingAsset目录对应URL
    public static string GetStreamingAssetsURL(string reletivePath)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return string.Format("file://{0}/{1}", Application.streamingAssetsPath, reletivePath);
#elif UNITY_ANDROID
        return string.Format("jar:file://{0}!/assets/{1}", Application.dataPath, reletivePath);
#elif UNITY_IPHONE
        return string.Format("file://{0}/Raw/{1}", Application.dataPath, reletivePath);
#endif
    }

    //本地缓存目录对应URL
    public static string GetLocalURL(string reletivePath, string fileName)
    {       
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return string.Format("file://{0}/{1}/{2}",Application.dataPath + "/../Caches",reletivePath,fileName);
#elif UNITY_IPHONE
        return string.Format("file://{0}/{1}/{2}",Application.temporaryCachePath,reletivePath,fileName);
#elif UNITY_ANDROID
        return string.Format("file://{0}/{1}/{2}",AndroidLocalStoragePath,reletivePath,fileName);
#endif      
        return null;
    }


    //StreamingAsset目录对应文件路径
    public static string GetStreamingAssetsPath(string reletivePath, string fileName)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        return string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, reletivePath,fileName);
#elif UNITY_ANDROID
        return string.Format("jar:file://{0}!/assets/{1}/{2}", Application.dataPath, reletivePath,fileName);
#elif UNITY_IPHONE
        return string.Format("{0}/Raw/{1}/{2}", Application.dataPath, reletivePath,fileName);
#endif
        return null;
    }


    public static string GetCacheFilePath(string relativePath, string fileName)
    {
        return string.Format("{0}/{1}/{2}", LocalStoragePath, relativePath, fileName);
    }

 

    /// <summary>
    /// 在各个操作系统一个指定的可读可写目录下,写入文件
    /// 强烈警告:目前只支持在这个目录下有一层子目录.
    /// </summary>
    /// <param name="reletivePath"></param>
    /// <param name="fileName"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static bool SaveFile(string reletivePath, string fileName, byte[] data)
    {
#if UNITY_WEBPLAYER
        return false;
#else
        string path = string.Format("{0}/{1}", LocalStoragePath, reletivePath);

        try
        {

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            File.WriteAllBytes(path + "/" + fileName, data);
            return true;
        }
        catch (System.Exception e)
        {
            ssLogger.LogWarning(e.ToString());
            return false;
        }
#endif
    }
	
	public static void DeleteFile(string relativePath, string fileName)
	{
#if UNITY_WEBPLAYER
        return ;
#else
        try
        {
            string url = string.Format("{0}/{1}/{2}", LocalStoragePath, relativePath, fileName);
            if (!File.Exists(url))
            {
                return ;
            }
			
			File.Delete(url);
			
            return ;
        }
        catch (System.Exception ex)
        {
            ssLogger.LogWarning(ex.ToString());
            return ;
        }
#endif
	}

  
    public static bool Exist(string relativePath, string fileName)
    {
#if UNITY_WEBPLAYER
        return false;
#else
        try
        {
            string url = string.Format("{0}/{1}/{2}", LocalStoragePath, relativePath, fileName);
            if (File.Exists(url))
            {
                return true;
            }
            return false;
        }
        catch (System.Exception ex)
        {
            ssLogger.LogWarning(ex.ToString());
            return false;
        }
#endif
    }
	
	
	 public static string ReadFileAsTxt(string reletivePath, string fileName)
    {
#if UNITY_WEBPLAYER
        return null;
#else
        try
        {
            string url = string.Format("{0}/{1}/{2}", LocalStoragePath, reletivePath, fileName);
            if (File.Exists(url))
            {
                return File.ReadAllText(url);
            }
            else
            {
                return null;
            }
        }
        catch (System.Exception ex)
        {
            ssLogger.LogWarning(ex.ToString());
            return null;
        }
#endif
    }

     public static byte[] SafeReadFile(string path)
     {
         try
         {
             if (!File.Exists(path))
             {
                 return null;
             }
             return File.ReadAllBytes(path);
         }
         catch (Exception e)
         {
             //ssLogger.Log(e.ToString());
             return null;
         }
     }

     public static bool SafeWriteFile(string path, byte[] bytes)
     {
         try
         {
             var folder = Directory.GetParent(path);
             if (!folder.Exists)
             {
                 folder.Create();
             }
             File.WriteAllBytes(path, bytes);
             return true;
         }
         catch (Exception e)
         {
             //ssLogger.Log(e.ToString());
             return false;
         }
     }

    public static bool WriteFile(string relativePath, string filename, string content)
     {
#if UNITY_WEBPLAYER
        return false;
#else
         try
         {
             string url = string.Format("{0}/{1}/{2}", LocalStoragePath, relativePath, filename);
             if (File.Exists(url))
             {
                 FileStream stream = new FileStream(url, FileMode.Append);
                 byte[] data = StringUtility.StringToUTF8Bytes(content);
                 stream.Write(data, 0, data.Length);
                 stream.Flush();
                 stream.Close();
             }
             else
             {
                 FileStream stream = new FileStream(url, FileMode.CreateNew);
                 byte[] data = StringUtility.StringToUTF8Bytes(content);
                 stream.Write(data, 0, data.Length);
                 stream.Flush();
                 stream.Close();
             }
             return true;
         }
         catch (System.Exception ex)
         {
             ssLogger.LogWarning(ex.ToString());
             return false;
         }
#endif
     }


    public static byte[] ReadFile(string reletivePath, string fileName)
    {
#if UNITY_WEBPLAYER
        return null;
#else
        try
        {
            string url = string.Format("{0}/{1}/{2}", LocalStoragePath, reletivePath, fileName);
            if (File.Exists(url))
            {
                return File.ReadAllBytes(url);
            }
            else
            {
                return null;
            }
        }
        catch (System.Exception ex)
        {
            ssLogger.LogWarning(ex.ToString());
            return null;
        }
#endif
    }

    /// <summary>
    /// 获取文件距离上次被写入的时间
    /// </summary>
    /// <param name="reletivePath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static TimeSpan GetFileWriteAge(string reletivePath, string fileName)
    {
#if UNITY_WEBPLAYER
        return new TimeSpan(TimeSpan.TicksPerDay * 365);
#else
        try
        {
            string url = string.Format("{0}/{1}/{2}", LocalStoragePath, reletivePath, fileName);
            if (File.Exists(url))
            {
                return DateTime.Now - File.GetLastWriteTime(url);
            }
            else
            {
                return new TimeSpan(TimeSpan.TicksPerDay * 365);
            }
        }
        catch (System.Exception ex)
        {
            ssLogger.LogWarning(ex.ToString());
            return new TimeSpan(TimeSpan.TicksPerDay * 365);
        }
#endif
    }

    public static bool IsFileTheSame(string filePath1, string filePath2)
    {
        try
        {
            if (!File.Exists(filePath1) || !File.Exists(filePath2))
            {
                return false;
            }

            var bytes1 = File.ReadAllBytes(filePath1);
            var bytes2 = File.ReadAllBytes(filePath2);
            if (bytes1.Length != bytes2.Length)
            {
                return false;
            }

            for (int i = 0; i < bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i])
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void SearchRelativeFilePath(string dir, List<string> result, string baseDir = "")
    {
        if (!Directory.Exists(dir))
        {
            return;
        }

        DirectoryInfo direcInfo = new DirectoryInfo(dir);
        FileInfo[] files = direcInfo.GetFiles();
        foreach (FileInfo file in files)
        {
            result.Add(baseDir + file.Name);
        }

        DirectoryInfo[] direcInfoArr = direcInfo.GetDirectories();
        foreach (DirectoryInfo d in direcInfoArr)
        {
            SearchRelativeFilePath(Path.Combine(dir, d.Name), result, baseDir + d.Name + "/");
        }
    }

    public static void SearchRelativeFilePathWithExt(string dir, string ext, List<string> result, string baseDir = "")
    {
        if (!Directory.Exists(dir))
        {
            return;
        }

        DirectoryInfo direcInfo = new DirectoryInfo(dir);
        FileInfo[] files = direcInfo.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Extension.Equals(ext, StringComparison.CurrentCultureIgnoreCase))
            {
                result.Add(baseDir + file.Name);
            }
        }

        DirectoryInfo[] direcInfoArr = direcInfo.GetDirectories();
        foreach (DirectoryInfo d in direcInfoArr)
        {
            SearchRelativeFilePathWithExt(Path.Combine(dir, d.Name), ext, result, baseDir + d.Name + "/");
        }
    }

    public static void SearchFilePathWithExt(string dir, string ext, List<string> result)
    {
        if (!Directory.Exists(dir))
        {
            return;
        }

        DirectoryInfo direcInfo = new DirectoryInfo(dir);
        FileInfo[] files = direcInfo.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Extension.Equals(ext, StringComparison.CurrentCultureIgnoreCase))
            {
                result.Add(Path.Combine(dir, file.Name));
            }
        }

        DirectoryInfo[] direcInfoArr = direcInfo.GetDirectories();
        foreach (DirectoryInfo d in direcInfoArr)
        {
            SearchFilePathWithExt(Path.Combine(dir, d.Name), ext, result);
        }
    }

    /// <summary>
    /// 目录复制
    /// </summary>
    /// <param name="direcSource">源目录</param>
    /// <param name="direcTarget">目标目录</param>
    public static void CopyFolder(string direcSource, string direcTarget)
    {
        if (!Directory.Exists(Path.GetFullPath(direcSource)))
        {
            //ssLogger.Log("dir: " + direcSource + "not exist");
            return;
        }

        if (!Directory.Exists(direcTarget))
            Directory.CreateDirectory(direcTarget);

        DirectoryInfo direcInfo = new DirectoryInfo(direcSource);
        FileInfo[] files = direcInfo.GetFiles();
        foreach (FileInfo file in files)
        {
            file.CopyTo(Path.Combine(direcTarget, file.Name), true);
        }

        DirectoryInfo[] direcInfoArr = direcInfo.GetDirectories();
        foreach (DirectoryInfo dir in direcInfoArr)
        {
            CopyFolder(Path.Combine(direcSource, dir.Name), Path.Combine(direcTarget, dir.Name));
        }
    }

    public static void CopyFiles(List<string> sourceFiles, string direcTarget)
    {
        if (!Directory.Exists(direcTarget))
            Directory.CreateDirectory(direcTarget);

        foreach (var filePath in sourceFiles)
        {
            var fileName = Path.GetFileName(filePath);
            if (!string.IsNullOrEmpty(fileName))
            {
                File.Copy(filePath, Path.Combine(direcTarget, fileName));
            }
        }
    }

    public static T DeserializeXml<T>(string path) where T : class
    {
        try
        {
            if (File.Exists(path))
            {
                var formatter = new XmlSerializer(typeof(T));
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    return formatter.Deserialize(fs) as T;
                }
            }
        }
        catch (Exception e)
        {
            ssLogger.LogWarning(e.ToString());
        }

        return null;
    }

    public static void  SerializeXml<T>(string path, T data) where T : class
    {
        try
        {
            var folder = Directory.GetParent(path);
            if (!folder.Exists)
            {
                folder.Create();
            }

            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false,
                Encoding = Encoding.UTF8
            };

            using (var fs = new FileStream(path, FileMode.Create))
            using (XmlWriter xmlWriter = XmlWriter.Create(fs, xmlWriterSettings))
            {
                var x = new XmlSerializer(data.GetType());
                x.Serialize(xmlWriter, data);
            }
        }
        catch (Exception e)
        {
            ssLogger.LogWarning(e.ToString());
        }
    }

    public static string AssetPathToSysPath(string assetPath)
    {
        string dataPath = Application.dataPath;
        if (assetPath.StartsWith("/../"))
        {
            return dataPath.Substring(0, dataPath.Length - 6) + assetPath.Substring("/../".Length);
        }
        else
        {
            if (assetPath.StartsWith("Assets"))
            {
                return Application.dataPath + assetPath.Substring("Assets".Length);
            }
            else
            {
                return string.Format("{0}/{1}", Application.dataPath, assetPath);
            }
        }
    }

    public static string AssetPathToResourcsPath(string assetPath)
    {
        const string ResPathTag = "Resources/";
        var startIdx = assetPath.LastIndexOf(ResPathTag) + ResPathTag.Length;
        var endIdx = assetPath.LastIndexOf('.');
        if (endIdx == -1)
        {
            return assetPath.Substring(startIdx);
        }
        else
        {
            return assetPath.Substring(startIdx, endIdx - startIdx);
        }
    }

    public static int BinaryFindByIdx<T, I>(T[] ary, I target, System.Func<T, I> idxFunc)
        where T : class
        where I : IComparable<I>
    {
        return BinaryFindByIdxExec(ary, target, idxFunc, 0, ary.Length);
    }

    private static int BinaryFindByIdxExec<T, I>(T[] ary, I target, System.Func<T, I> idxFunc, int s, int e)
        where T : class
        where I : IComparable<I>
    {
        if (s >= e)
        {
            return -1;
        }
        var mid = (e + s) / 2;
        var cmp = target.CompareTo(idxFunc(ary[mid]));
        if (cmp == 0)
        {
            return mid;
        }
        if (cmp < 0)
        {
            return BinaryFindByIdxExec(ary, target, idxFunc, s, mid);
        }
        else
        {
            return BinaryFindByIdxExec(ary, target, idxFunc, mid + 1, e);
        }
    }


    public static bool IsExistSDCard()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            string sig = AndroidJNIHelper.GetSignature<bool>(new object[0]);
            var ptr = AndroidJNI.GetStaticMethodID(GamePluginMgrPtr, "ExistSDCard", sig);
            bool b = AndroidJNI.CallStaticBooleanMethod(GamePluginMgrPtr, ptr, new jvalue[0]);
            return b;
        }
        catch (Exception exception)
        {
            //ssLogger.Log(exception.Message);
        }
#endif
        return false;
    }

    public static long GetAndroidSDFreeSizeKb()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            string sig = AndroidJNIHelper.GetSignature<long>(new object[0]);
            var ptr = AndroidJNI.GetStaticMethodID(GamePluginMgrPtr, "GetSDFreeSizeKb", sig);
            long kb = AndroidJNI.CallStaticLongMethod(GamePluginMgrPtr, ptr, new jvalue[0]);
            return kb;
        }
        catch (Exception exception)
        {
            //ssLogger.Log(exception.Message);
        }
#endif
        return 0;
    }

    public static long GetAndroidSystemFreeSizeKb()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            string sig = AndroidJNIHelper.GetSignature<long>(new object[0]);
            var ptr = AndroidJNI.GetStaticMethodID(GamePluginMgrPtr, "GetSystemFreeSizeKb", sig);
            long kb = AndroidJNI.CallStaticLongMethod(GamePluginMgrPtr, ptr, new jvalue[0]);
            return kb;
        }
        catch (Exception exception)
        {
            //ssLogger.Log(exception.Message);
        }
#endif
        return 0;
    }


    public static string GetAndroidSDCardRoot()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            string sig = AndroidJNIHelper.GetSignature<string>(new object[0]);
            var ptr = AndroidJNI.GetStaticMethodID(GamePluginMgrPtr, "GetSdCardRoot", sig);
            string path = AndroidJNI.CallStaticStringMethod(GamePluginMgrPtr, ptr, new jvalue[0]);
            return path;
        }
        catch (Exception exception)
        {
            //ssLogger.Log(exception.Message);
        }
#endif
        return null;
    }

#if UNITY_IPHONE
    [DllImport("__Internal")]
    public static extern int GetAvailableMemory();

    [DllImport("__Internal")]
    public static extern void CollectVMemoryInfo();

    [DllImport("__Internal")]
    public static extern int GetVMPageSize();

    [DllImport("__Internal")]
    public static extern int GetVMFreePageCount();

    [DllImport("__Internal")]
    public static extern int GetVMActivePageCount();

    [DllImport("__Internal")]
    public static extern int GetVMInactivePageCount();

    [DllImport("__Internal")]
    public static extern int GetVMWirePageCount();

#endif

    public static float GetAvailMemory()
    {
        long size = 0;
#if UNITY_EDITOR
        size = 9999;
#elif UNITY_ANDROID
            try
            {
				string sig = AndroidJNIHelper.GetSignature<long>(new object[0]);
				var ptr = AndroidJNI.GetStaticMethodID(GamePluginMgrPtr, "GetAvailMemory", sig);
				size = AndroidJNI.CallStaticLongMethod(GamePluginMgrPtr, ptr, new jvalue[0]);
				size = size / 1024 / 1024;
            }
            catch (Exception exception)
            {
                //ssLogger.Log(exception.Message);
            }
#elif UNITY_IPHONE
        // Object-C long 是32位，转换后再取回来
        //size = GetAvailableMemory();
        //ssLogger.LogWarning("AvailMemory: {0} M", size);
        size = 9999; //IOS的不管
#endif
        return (float)size;
    }
}
