using System.Security.Policy;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using System;
using Path = System.IO.Path;

public class ZipFileUtility 
{
    static ZipFileUtility()
    {
        ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = System.Text.Encoding.Default.CodePage;
    }

    static byte[] readBuffer;

    public static void ZipFile(string baseDir, List<string> filePathList, string targetFile)
    {
        var folder = Directory.GetParent(targetFile);
        if (!folder.Exists)
        {
            folder.Create();
        }

        using (FileStream zip = System.IO.File.Create(targetFile))
        {
            using (ZipOutputStream s = new ZipOutputStream(zip))
            {
                foreach (string f in filePathList)
                {
                    ZipFileOneFile(s, baseDir, f);
                }
            }
        }
    }

    private static void ZipFileOneFile(ZipOutputStream s, string baseDir, string relativeFilePath)
    {
        Crc32 crc = new Crc32();
        var filePath = baseDir + "/" + relativeFilePath;
        using (FileStream fs = File.OpenRead(filePath))
        {
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            ZipEntry entry = new ZipEntry(relativeFilePath);
            entry.DateTime = DateTime.Now;
            entry.Size = fs.Length;
            fs.Close();
            crc.Reset();
            crc.Update(buffer);
            entry.Crc = crc.Value;
            s.PutNextEntry(entry);
            s.Write(buffer, 0, buffer.Length);
        }  
    }

    //folder压缩后的一级目录，直接对应解压后Caches下的一级子目录
    public static string ZipFile(List<string> sourceList,string targetFile,string folder)
    {
        using (FileStream zip = System.IO.File.Create(targetFile))
        {
            using (ZipOutputStream s = new ZipOutputStream(zip))
            {                
                foreach (string sourceFile in sourceList)
                {
                    ZipFile(s, sourceFile, folder);
                }
            }
        }
        return null;
    }

    private static void ZipFile(ZipOutputStream s, string sourceFile,string folder)
    {        
        Crc32 crc = new Crc32();
        using (FileStream fs = File.OpenRead(sourceFile))
        {
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            string fileName = Path.GetFileName(sourceFile);
            if (folder != null)
            {
                fileName = folder + "/" + fileName;
            }
            ZipEntry entry = new ZipEntry(fileName);
            entry.DateTime = DateTime.Now;
            entry.Size = fs.Length;
            fs.Close();
            crc.Reset();
            crc.Update(buffer);
            entry.Crc = crc.Value;
            s.PutNextEntry(entry);
            s.Write(buffer, 0, buffer.Length);
        }       
        
    }

    public static List<string> UnzipFile(string sourceFile,string unzipDir)
    {
        byte[] fileData = File.ReadAllBytes(sourceFile);
        if (fileData != null)
        {
            return Unzip(fileData, unzipDir);
        }
        return null;
    }

	public static List<string> Unzip(byte[] fileData,string unzipDir)
    {       
        //FastZip解压目录在Mac上有问题                 
        //    FastZipEvents ev = new FastZipEvents();
        //    ev.CompletedFile = OnCompletedFile;
        //    (new FastZip(ev)).ExtractZip(sourceFile, Utility.LocalStoragePath, "");

        //先存为临时文件，再一次性重命名至目标文件，减少处理一半的可能性
        List<string> tmpFiles = new List<string>();
        List<string> targetFiles = new List<string>();
        List<string> entryNames = new List<string>();
        if(readBuffer == null)
        {
             readBuffer = new byte[256 * 1024];
        }
        int size = 0;
        ZipEntry entry = null;
        using (ZipInputStream zis = new ZipInputStream(new MemoryStream(fileData)))
        {
            while ((entry = zis.GetNextEntry()) != null)
            {
                string unzipPath = Path.Combine(unzipDir, entry.Name);
                string dirPath = Path.GetDirectoryName(unzipPath);
                if (!string.IsNullOrEmpty(dirPath))
                {
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);
                }

                if (entry.IsFile)
                {
                    string fileName = Path.GetFileName(unzipPath);
                    string tmpPath = string.Format("{0}/{1}.tmp", dirPath, fileName);

                    if (File.Exists(tmpPath))
                    {
                        File.Delete(tmpPath);
                    }
                    using (FileStream streamWriter = File.Create(tmpPath))
                    {
                        while (true)
                        {
                            size = zis.Read(readBuffer, 0, readBuffer.Length);
                            if (size <= 0) break;
                            streamWriter.Write(readBuffer, 0, size);
                        }
                        streamWriter.Close();
                    }
                    targetFiles.Add(unzipPath);
                    tmpFiles.Add(tmpPath);
                    entryNames.Add(entry.Name);
                }
            }
        }

        for (int i = 0; i < targetFiles.Count; i++)
        {
            FileInfo info = new FileInfo(tmpFiles[i]);
            string targetFile = targetFiles[i];
            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }
            info.MoveTo(targetFile);
        }
	    return entryNames;
    }

    public static IEnumerator UnzipAsync(string sourceFile, string unzipDir, Func<string, IEnumerator> outputFileCallback, Utility.ClassWrapper<bool> result = null)
    {
        if (result != null)
        {
            result.Value = false;
        }

        byte[] fileData = File.ReadAllBytes(sourceFile);
        if (fileData == null)
        {
            yield break;
        }

        if (readBuffer == null)
        {
            readBuffer = new byte[256 * 1024];
        }
        int size = 0;
        ZipEntry entry = null;
        using (ZipInputStream zis = new ZipInputStream(new MemoryStream(fileData)))
        {
            while (true)
            {
                if (!TryGetNextEntry(zis, out entry))
                {
                    yield break;
                }
                if (entry == null)
                {
                    break;
                }

                yield return null;
                string unzipPath = Path.Combine(unzipDir, entry.Name);
                string dirPath = Path.GetDirectoryName(unzipPath);
                if (!string.IsNullOrEmpty(dirPath))
                {
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);
                }

                if (entry.IsFile)
                {
                    string fileName = Path.GetFileName(unzipPath);
                    string tmpPath = string.Format("{0}/{1}.tmp", dirPath, fileName);

                    if (File.Exists(tmpPath))
                    {
                        File.Delete(tmpPath);
                    }
                    using (FileStream streamWriter = File.Create(tmpPath))
                    {
                        while (true)
                        {
                            size = zis.Read(readBuffer, 0, readBuffer.Length);
                            if (size <= 0) break;
                            streamWriter.Write(readBuffer, 0, size);
                        }
                        streamWriter.Close();
                    }
                    yield return null;

                    FileInfo info = new FileInfo(tmpPath);
                    if (File.Exists(unzipPath))
                    {
                        File.Delete(unzipPath);
                    }
					info.MoveTo(unzipPath);
					if (outputFileCallback != null)
					{
						var itor = outputFileCallback(entry.Name);
						while (itor.MoveNext())
						{
							yield return itor.Current;
						}
					}
                }
            }
        }
        if (result != null)
        {
            result.Value = true;
        }

    }

    static bool TryGetNextEntry(ZipInputStream zis, out ZipEntry entry)
    {
        try
        {
            entry = zis.GetNextEntry();
            return true;
        }
        catch
        {
            entry = null;
            return false;
        }
    }

	static public int SortByFileSize(string a, string b)
	{
		return (int)(new FileInfo(a).Length - new FileInfo(b).Length);
	}

	public static int BuildStramingAssetZip(string path, string pattern, string zippath, string unzipfolder, int index)
	{
		return BuildZip(Application.streamingAssetsPath + path, pattern, Application.dataPath + zippath, unzipfolder, index);
	}

	public static int BuildZip(string path, string pattern, string zippath, string unzipfolder, int index)
	{
		long totalSize = 1024 * 1024 * 4;

		if (!Directory.Exists(zippath))
		{
			Directory.CreateDirectory(zippath);
		}

		List<string> files = new List<string>(Directory.GetFiles(path, "*" + pattern));
		List<string> sourceList = new List<string>();
		long curSize = 0;
		files.Sort(SortByFileSize);
		foreach (string file in files)
		{
			FileInfo info = new FileInfo(file);
			if (!(sourceList.Count <= 0 || curSize + info.Length <= totalSize))
			{
				string targetFile = string.Format("{0}data_{1}.zip", zippath, index);
				ZipFileUtility.ZipFile(sourceList, targetFile, unzipfolder);
				sourceList.Clear();
				index++;
				curSize = 0;
			}
			curSize += info.Length;
			sourceList.Add(file);
		}
		if (sourceList.Count > 0)
		{
			string targetFile = string.Format("{0}data_{1}.zip", zippath, index);
			ZipFileUtility.ZipFile(sourceList, targetFile, unzipfolder);
			index++;
		}

		List<string> directories = new List<string>(Directory.GetDirectories(path));
		foreach (string directory in directories)
		{
			string subpath = directory.Replace(path, "");
			index = BuildZip(path + subpath, pattern, zippath, unzipfolder + subpath, index);
		}

		return index;
	}
}
