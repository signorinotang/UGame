using UnityEngine;
using System.Collections;
using System.IO;

public class ssGFXQualitySelector_Android : ssGFXQualitySelector
{
	/// <summary>
	/// 硬件要求
	/// </summary>
	[System.Serializable]
	public class HardwareRequirement
	{
		// 内存容量要求
		public int MemSize = -1;
		// 型号
		public string[] DeviceModels;

		// 型号匹配
		public bool MatchModels
		{
			get
			{
				int len = DeviceModels.Length;
				for (int i = 0; i < len; ++i)
				{
					if (SystemInfo.deviceModel.ToLower().Contains(DeviceModels[i].ToLower()))
					{
						return true;
					}
				}

				return false;
			}
		}

		// 是否满足要求
		public bool CanFulFill
		{
			get
			{
				if (MemSize < 0)
				{
					return false;
				}

				if (SystemInfo.systemMemorySize < MemSize)
				{
					return false;
				}

				return true;
			}
		}
	}

	public override ssGFXQuality.EQualityLevel InitQuality
	{
		get
		{
			return BestQL;
		}
	}

	public override void Initialize()
	{
		//ssLogger.Log("memsize: " + SystemInfo.systemMemorySize);
		//ssLogger.Log("gmemsize: " + SystemInfo.graphicsMemorySize);
		bool bHasSelected = false;
		int len = HRGroup.Length;

		 for (int i = 0; i < len; ++i )
		{
			if (HRGroup[i].MatchModels)
			{
				BestQL = (ssGFXQuality.EQualityLevel)i;
				bHasSelected = true;
				break;
			}
		} 

		if (!bHasSelected)
		{
            int i = 0;
			for (; i < len; ++i)
			{
				if (HRGroup[i].CanFulFill)
				{
					BestQL = (ssGFXQuality.EQualityLevel)i;
					break;
				}
			}

            if (i >= len)
            {
                BestQL = ssGFXQuality.EQualityLevel.QL_VeryLow;
            }
		}
	}

	public HardwareRequirement[] HRGroup = new HardwareRequirement[5];

	ssGFXQuality.EQualityLevel BestQL = ssGFXQuality.EQualityLevel.QL_Middle;
}
