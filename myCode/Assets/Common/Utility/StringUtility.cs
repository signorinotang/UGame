using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;



/// <summary>
/// 工具类，提供一些常用的静态工具函数
/// </summary>
public class StringUtility
{

    /// <summary>
    /// 功能同c语言中的strlen
    /// </summary>
    /// <param name="str">输入的bytes</param>
    /// <returns>输入bytes的string长度</returns>
    static public int strlen(byte[] str)
    {
        if (str == null)
            return 0;

        byte nullChar = 0x00;
        int count = 0;
        for (int i = 0; i < str.Length; i++)
        {
            if (nullChar == str[i])
            {
                break;
            }

            count++;
        }

        return count;
    }

    /// <summary>
    /// 功能同c语言中的strlen
    /// </summary>
    /// <param name="str">输入的bytes</param>
    /// <returns>输入bytes的string长度</returns>
    static public int strlen(sbyte[] str)
    {
        if (str == null)
            return 0;

        byte nullChar = 0x00;
        int count = 0;
        for (int i = 0; i < str.Length; i++)
        {
            if (nullChar == str[i])
            {
                break;
            }

            count++;
        }

        return count;
    }

    /// <summary>
    /// 将一个UTF8编码格式的byte数组，转换为一个String
    /// </summary>
    /// <param name="str">编码格式的数组</param>
    /// <returns>转换的string</returns>
    static public string UTF8BytesToString(byte[] str)
    {
        if (str == null)
            return null;

        //为了让string的长度正确，
        byte[] tempStr = new byte[strlen(str)];
        System.Buffer.BlockCopy(str, 0, tempStr, 0, tempStr.Length);
        return System.Text.Encoding.UTF8.GetString(tempStr);
    }

    /// <summary>
    /// 将一个UTF8编码格式的byte数组，转换为一个String
    /// </summary>
    /// <param name="str">编码格式的数组</param>
    /// <returns>转换的string</returns>
    static public string UTF8BytesToString(sbyte[] str)
    {
        if (str == null)
            return null;

        //为了让string的长度正确，
        byte[] tempStr = new byte[strlen(str)];
        System.Buffer.BlockCopy(str, 0, tempStr, 0, tempStr.Length);
        return System.Text.Encoding.UTF8.GetString(tempStr);
    }



    /// <summary>
    /// 将一个GB2312编码格式的byte数组，转换为一个String,
    /// ios、目录服务器特供
    /// </summary>
    /// <param name="str">编码格式的数组</param>
    /// <returns>转换的string</returns>
    static public string AutoCJK936_UTF8BytesToString(byte[] str)
    {
        string ret = "";
        if (str == null)
            return null;

        //为了让string的长度正确，
        byte[] tempStr = new byte[strlen(str)];
        System.Buffer.BlockCopy(str, 0, tempStr, 0, tempStr.Length);

        if (StringUtility.IsUTF8(tempStr))
        {
            ret = System.Text.Encoding.UTF8.GetString(tempStr);//服务器又改成utf8了
        }
        else
		{
            ret = System.Text.Encoding.UTF8.GetString(tempStr);  
		}
        return ret;
    }


    /// <summary>
    /// 将一个string转换为UTF-8数组，复制到buffer中。
    /// </summary>
    /// <param name="str">需要转换的string</param>
    /// <param name="buffer">目标buffer</param>
    static public void StringToUTF8Bytes(string str, ref byte[] buffer)
    {
        if (str == null || buffer == null)
            return;

        if (!str.EndsWith("\0"))
        {
            str += "\0";
        }

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
        int length = bytes.Length > buffer.Length ? buffer.Length : bytes.Length;
        System.Buffer.BlockCopy(bytes, 0, buffer, 0, length);
        buffer[buffer.Length - 1] = 0;
    }


    /// <summary>
    /// 将一个string转换为UTF-8数组，复制到buffer中。
    /// </summary>
    /// <param name="str">需要转换的string</param>
    /// <param name="buffer">目标buffer</param>
    static public void StringToUTF8Bytes(string str, ref sbyte[] buffer)
    {
        if (str == null || buffer == null)
            return;

        if (!str.EndsWith("\0"))
        {
            str += "\0";
        }

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
        int length = bytes.Length > buffer.Length ? buffer.Length : bytes.Length;
        System.Buffer.BlockCopy(bytes, 0, buffer, 0, length);
        buffer[buffer.Length - 1] = 0;
    }

    /// <summary>
    /// 给定一个String，返回直接返回一个UTF8的byte数组
    /// </summary>
    /// <param name="str">要转换的string，如果为空或者长度为0则返回null</param>
    /// <returns>返回UTF-8编码的byte数组</returns>
    static public byte[] StringToUTF8Bytes(string str)
    {
        if (str == null || str.Length == 0)
            return null;

        if (!str.EndsWith("\0"))
        {
            str += "\0";
        }

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
        return bytes;
    }

    /// <summary>
    /// 比较两个byte数组是否一致
    /// </summary>
    /// <param name="srcbuf1"></param>
    /// <param name="srcbuf2"></param>
    /// <returns></returns>
    static public bool strcmp(ref byte[] srcbuf1, ref byte[] srcbuf2)
    {
        if (srcbuf1 == null || srcbuf2 == null)
        {
            return srcbuf1 == srcbuf2;
        }

        int cmpLength = srcbuf1.Length < srcbuf2.Length ? srcbuf1.Length : srcbuf2.Length;

        for (int i = 0; i < cmpLength; i++)
        {
            if (srcbuf1[i] != srcbuf2[i])
                return false;
        }

        return true;
    }
	
	static public bool strcpy(ref byte[] destbuf, ref byte[] srcbuf)
	{
		if (srcbuf == null)
			return false;
		
		int len = srcbuf.Length;
		destbuf = new byte[len];
		for (int i = 0; i < len; ++i)
		{
			destbuf[i] = srcbuf[i];
		}
		return true;
	}

    /// <summary>
    /// 获取一个时间长度的字符串表示
    /// </summary>
    /// <param name="time">时间，单位秒</param>
    /// <returns>返回时间格式的字符串</returns>
    static public string GetFormatTime(float time)
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(time);
        return timeSpan.Hours.ToString("D2") + " : " + timeSpan.Minutes.ToString("D2") + " : " + timeSpan.Seconds.ToString("D2");
    }

    /// <summary>
    /// 获取一个时间长度的字符串表示
    /// </summary>
    /// <param name="time">时间，单位秒</param>
    /// /// <param name="spliter">分隔符</param>
    /// <returns>返回时间格式的字符串</returns>
    static public string GetFormatTime(float time, string spliter)
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(time);
        return timeSpan.Hours.ToString("D2") + spliter + timeSpan.Minutes.ToString("D2") + spliter + timeSpan.Seconds.ToString("D2");
    }

    /// <summary>
    /// 获取一个紧凑格式时间长度的字符串表示
    /// </summary>
    /// <param name="time">时间，单位秒</param>
    /// <returns>返回时间格式的字符串</returns>
    static public string GetCloseFormatTime(float time)
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(time);
        return timeSpan.Hours.ToString("D2") + ":" + timeSpan.Minutes.ToString("D2") + ":" + timeSpan.Seconds.ToString("D2");
    }

    /// <summary>
    /// 获取一个时间长度的日期
    /// </summary>
    /// <param name="time">时间，单位秒</param>
    /// /// <param name="spliter">字符串分隔符</param>
    /// <returns>返回日期格式的字符串</returns>
    static public string GetFormatDate(float time, string spliter)
    {
        System.DateTime startDT = new System.DateTime(1970, 1, 1);
        System.DateTime dateTime = startDT.AddSeconds(time).ToLocalTime();
        return dateTime.Year.ToString("D4") + spliter + dateTime.Month.ToString("D2") + spliter + dateTime.Day.ToString("D2");
    }

    /// <summary>
    /// 获取一个时间长度的日期和具体时间
    /// </summary>
    /// <param name="time">时间，单位秒</param>
    /// /// <param name="spliter">字符串分隔符</param>
    /// <returns>返回日期格式的字符串</returns>
    static public string GetFormatDateWithTime(float time, string spliter)
    {
        System.DateTime startDT = new System.DateTime(1970, 1, 1);
        System.DateTime dateTime = startDT.AddSeconds(time).ToLocalTime();
        return dateTime.Year.ToString("D4") + spliter + dateTime.Month.ToString("D2") + spliter + dateTime.Day.ToString("D2") + "   " + dateTime.Hour.ToString("D2") + " : " + dateTime.Minute.ToString("D2");
    }


	/// <summary>
    /// 获取一个时间长度的天数
    /// </summary>
    /// <param name="time">时间，单位秒</param>
    /// <returns>返回天数的字符串</returns>
	static public string GetFormatDayNum(UInt64 time)
	{
		return (time / 86400).ToString();
	}

    public static bool ParseDateTime(string dateStr, out DateTime dt)
    {
        string[] dateStrArray = dateStr.Split(',');
        int[] dateInt = new int[dateStrArray.Length];
        if (dateStrArray.Length >= 6)
        {
            for (int i = 0; i < dateInt.Length; ++i)
            {
                dateInt[i] = int.Parse(dateStrArray[i]);
            }

            dt = new System.DateTime(
                dateInt[0], dateInt[1], dateInt[2], dateInt[3], dateInt[4], dateInt[5], DateTimeKind.Utc);

            return true;
        }
        else
        {
            dt = new DateTime();
            return false;
        }
    }

    /// <summary>
    /// 根据时间的秒数值得到其hh:mm:ss格式的字符串
    /// </summary>
    /// <param name="timeInSeconds">以秒为单位的时间值</param>
    /// <returns>hh:mm:ss</returns>
    public static string GetFormatTimeString(int timeInSeconds)
    {
        int hours = timeInSeconds / 3600;
        int minute = (timeInSeconds % 3600) / 60;
        int seconds = timeInSeconds % 60;

        string time = hours.ToString().PadLeft(2, '0') + ":" + minute.ToString().PadLeft(2, '0') + ":" + seconds.ToString().PadLeft(2, '0');
        return time;
    }


    /// <summary>
    /// 判断一块buffer是否是utf8
    /// </summary>
    /// <returns></returns>
    /// 
    private static bool IsUTF8(byte[] buffer)
    {
         /* IsTextUTF8
         *
         * UTF-8 is the encoding of Unicode based on Internet Society RFC2279
         * ( See http://www.cis.ohio-state.edu/htbin/rfc/rfc2279.html )
         *
         * Basicly:
         * 0000 0000-0000 007F - 0xxxxxxx  (ascii converts to 1 octet!)
         * 0000 0080-0000 07FF - 110xxxxx 10xxxxxx    ( 2 octet format)
         * 0000 0800-0000 FFFF - 1110xxxx 10xxxxxx 10xxxxxx (3 octet format)
         * (this keeps going for 32 bit unicode)
         *
         *
         * Return value:  TRUE, if the text is in UTF-8 format.
         *                FALSE, if the text is not in UTF-8 format.
         *                We will also return FALSE is it is only 7-bit ascii, so the right code page
         *                will be used.
         *
         *                Actually for 7 bit ascii, it doesn't matter which code page we use, but
         *                notepad will remember that it is utf-8 and "save" or "save as" will store
         *                the file with a UTF-8 BOM.  Not cool.
         */
        int i;
        byte cOctets = 0; // octets to go in this UTF-8 encoded character
        bool bAllAscii = true;
        int iLen = buffer.Length;
        byte[] buf = new byte[iLen];
        System.Buffer.BlockCopy(buffer, 0, buf, 0, iLen);

        for (i = 0; i < iLen; i++)
        {
            if ((buf[i] & 0x80) != 0) bAllAscii = false;
            if (cOctets == 0)
            {
                if (buf[i] >= 0x80)
                {
                    do
                    {
                        buf[i] <<= 1;
                        cOctets++;
                    }
                    while ((buf[i] & 0x80) != 0);
                    cOctets--;
                    if (cOctets == 0)
                        return false;
                }
            }

            else
            {
                if ((buf[i] & 0xC0) != 0x80)
                    return false;
                cOctets--;
            }
        }

        if (cOctets > 0)
            return false;

        if (bAllAscii)
            return false;
        return true;
    }
    
    /// <summary>
    /// 字节数据转换为16进制表示的字符串
    /// 强行调用web中的一个转换函数
    /// http://blogs.msdn.com/b/blambert/archive/2009/02/22/blambert-codesnip-fast-byte-array-to-hex-string-conversion.aspx
    /// </summary>
    /// <returns></returns>
    public static string ByteArrayToHexString(byte[] Bytes, int len)
    {
        StringBuilder Result = new StringBuilder(Bytes.Length * 2);
        string HexAlphabet = "0123456789ABCDEF";

        int count = 0;
        foreach (byte B in Bytes)
        {
            if (count++ == len)
            {
                break;
            }

            Result.Append(HexAlphabet[(int)(B >> 4)]);
            Result.Append(HexAlphabet[(int)(B & 0xF)]);
        }

        return Result.ToString();
    }

    public static byte[] HexStringToByteArray(string Hex)
    {
        byte[] Bytes = new byte[Hex.Length / 2];
        int[] HexValue = new int[] {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 
            0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
            0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F 
        };

        for (int x = 0, i = 0; i < Hex.Length; i += 2, x += 1)
        {
            Bytes[x] = (byte)(HexValue[Char.ToUpper(Hex[i + 0]) - '0'] << 4 |
                                HexValue[Char.ToUpper(Hex[i + 1]) - '0']);
        }
        return Bytes;
    }

    public static List<string> StringSplit(string str, char[] sChars)
    {
        if (str == null || str.Length == 0)
            return null;

        List<string> result = new List<string>();
        bool isInQuote = false;
        StringBuilder strBuilder = new StringBuilder();
        for (int i = 0; i < str.Length; ++i)
        {
            char c = str[i];
            if (isInQuote)
            {
                if (c == '"')
                {
                    if (strBuilder.Length != 0)
                    {
                        result.Add(strBuilder.ToString());
                        strBuilder.Remove(0, strBuilder.Length);
                    }

                    isInQuote = false;
                }
                else
                {
                    strBuilder.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    isInQuote = true;
                    continue;
                }

                bool isSplitChar = false;
                for (int j = 0; j < sChars.Length; ++j)
                {
                    if (sChars[j] == c)
                    {
                        isSplitChar = true;
                        break;
                    }
                }

                if (isSplitChar)
                {
                    if (strBuilder.Length != 0)
                    {
                        result.Add(strBuilder.ToString());
                        strBuilder.Remove(0, strBuilder.Length);
                    }
                }
                else
                {
                    strBuilder.Append(c);
                }
            }
        }

        if (strBuilder.Length != 0)
        {
            result.Add(strBuilder.ToString());
        }

        return result;
    }

    public static string ShortString(string str, int len)
    {
        if (str == null || str.Length <= len)
            return str;

		int realLength = 0;
		for (int i = 0; i < str.Length; ++i)
		{
			if ((str[i] >= 48 && str[i] <= 57) || (str[i] >= 97 && str[i] <= 122)) // ANSI 数字和小写字母认为是按1个字符宽绘制的
			{
				realLength += 1;
			}
			else 
			{
				realLength += 2;
			}
			
			if (realLength > len * 2 && i > 2)
			{
				i -= 2;
				return String.Format("{0}...", str.Substring(0, i));
			}
		}
		
        return str;
    }
}
