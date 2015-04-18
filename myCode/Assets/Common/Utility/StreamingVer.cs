using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public enum EResourceVerType
{
    ClientVersion,
    MiniDownLoad,//MiniPackageMaxIndex
	Sound,
	Font,
    BuildVersion,
	Max,
}

public class StreamingVer
{
	static bool[] Dirty = null;
	static string[] CachedVer = null;

	static StreamingVer()
	{
		Dirty = new bool[(int)EResourceVerType.Max];
		for (int i = 0; i < Dirty.Length; i++)
		{
			Dirty[i] = true;
		}

		CachedVer = new string[(int)EResourceVerType.Max];
	}

	public static int ReadResourceVer(EResourceVerType VerType)
	{
		string Ver = ReadResourceStrVer(VerType);
        int iVer = 0;
        try
        {
            iVer = Convert.ToInt32(Ver);
        }
        catch
        {

        }
        return iVer;
	}

	public static string ReadResourceStrVer(EResourceVerType VerType)
	{
		TextAsset asset = Resources.Load("LocalConfig/" + VerType.ToString()) as TextAsset;

		string Ver = "-1";
		if (asset != null)
		{
			Ver = asset.text;
		}
        
#if !DISTRIBUTION_VERSION
        //ssLogger.Log("ReadResourceStrVer:" + VerType + ":" + Ver);
#endif

		Resources.UnloadAsset(asset);
		return Ver;
	}

	public static void WriteResourceVer(EResourceVerType VerType, int Ver)
	{
		WriteResourceStrVer(VerType, Ver.ToString());
	}

	public static void WriteResourceStrVer(EResourceVerType VerType, string Ver)
	{
		string path = Application.dataPath + "/Resources/LocalConfig/" + VerType.ToString() + ".txt";

		WriteFileVer(path, Ver);
	}

	public static int ReadStreamingVer(EResourceVerType VerType)
	{
		string Ver = ReadStreamingStrVer(VerType);

        int iVer = 0;
        try
        {
            iVer = Convert.ToInt32(Ver);
        }
        catch 
        {
        	
        }
        return iVer;
	}

	public static string ReadStreamingStrVer(EResourceVerType VerType)
	{
		if (!Dirty[(int)VerType])
		{
			return CachedVer[(int)VerType];
		}

        string path = Utility.LocalStoragePath + "/Ver/" + VerType.ToString();

		string Ver = ReadFileVer(path);

		Dirty[(int)VerType] = false;
		CachedVer[(int)VerType] = Ver;

		return Ver;
	}

	public static void WriteStreamingVer(EResourceVerType VerType, int Ver)
	{
		WriteStreamingStrVer(VerType, Ver.ToString());
	}

	public static void WriteStreamingStrVer(EResourceVerType VerType, string Ver)
	{
        string path = Utility.LocalStoragePath + "/Ver/" + VerType.ToString();

		WriteFileVer(path, Ver);

		Dirty[(int)VerType] = true;
		CachedVer[(int)VerType] = Ver;
	}

    public static void RemoveStreamingVer(EResourceVerType VerType)
    {
        string path = Utility.LocalStoragePath + "/Ver/" + VerType.ToString();
        RemoveFileVer(path);
    }

	private static string ReadFileVer(string path)
	{
		string Ver = "-1";
		if (File.Exists(path))
		{
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    StreamReader sr = new StreamReader(fs);
                    fs.Position = 0;
                    Ver = sr.ReadLine();
                    sr.Close();
                }
            }
            catch
            {

            }
		}

		return Ver;
	}

	private static void WriteFileVer(string path, string Ver)
	{
	    var folder = Directory.GetParent(path);
	    if (!folder.Exists)
	    {
	        folder.Create();
	    }

		using (FileStream fs = new FileStream(path, FileMode.Create))
		{
			StreamWriter sw = new StreamWriter(fs);
			fs.Position = 0;
			sw.Write(Ver);
			sw.Close();
		}
	}

    private static void RemoveFileVer(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch 
        {
            
        }

    }

    public static void ClearAllStreamVer()
    {
        for (int i = 0; i < Dirty.Length; i++)
        {
            RemoveStreamingVer((EResourceVerType) i);
            Dirty[i] = true;
        }
    }
}