using UnityEngine;
using System.Collections;

public class ssGFXQualitySelector_iOS : ssGFXQualitySelector
{
#if UNITY_IPHONE
    public override ssGFXQuality.EQualityLevel InitQuality
    {
        get
        {
            switch (iPhone.generation)
            {
                case iPhoneGeneration.iPhone3GS:
                case iPhoneGeneration.iPodTouch3Gen:
                    return iPhone3GS;
                case iPhoneGeneration.iPad1Gen:
                    return iPad1;
                case iPhoneGeneration.iPhone4:
                case iPhoneGeneration.iPodTouch4Gen:
                    return iPhone4;
                case iPhoneGeneration.iPad2Gen:
                    return iPad2;
                case iPhoneGeneration.iPhone4S:
                    return iPhone4S;
                case iPhoneGeneration.iPad3Gen:
                    return iPad3;
                case iPhoneGeneration.iPhone5:
                case iPhoneGeneration.iPodTouch5Gen:
                    return iPhone5;
                case iPhoneGeneration.iPad4Gen:
                    return iPad4;
                case iPhoneGeneration.iPadMini1Gen:
                    return iPadMini1;
                case iPhoneGeneration.Unknown:
                case iPhoneGeneration.iPhoneUnknown:
                case iPhoneGeneration.iPodTouchUnknown:
                    return iPhoneUnknown;
                case iPhoneGeneration.iPadUnknown:
                    return iPadUnknown;
                default:
                    return ssGFXQuality.EQualityLevel.QL_Ultra;
            }
        }
    }
#endif

	public ssGFXQuality.EQualityLevel iPhone3GS;
	public ssGFXQuality.EQualityLevel iPad1;
	public ssGFXQuality.EQualityLevel iPhone4;
	public ssGFXQuality.EQualityLevel iPad2;
	public ssGFXQuality.EQualityLevel iPhone4S;
	public ssGFXQuality.EQualityLevel iPad3;
	public ssGFXQuality.EQualityLevel iPhone5;
	public ssGFXQuality.EQualityLevel iPad4;
	public ssGFXQuality.EQualityLevel iPadMini1;
	public ssGFXQuality.EQualityLevel iPhoneUnknown;
	public ssGFXQuality.EQualityLevel iPadUnknown;
}
