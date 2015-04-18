using UnityEngine;

public class ssGFXQualityLOD : MonoBehaviour
{
    public GameObject[] High;
    public GameObject[] Low;

    void Awake()
    {
        if (!ssGFXQuality.Instance.CurSetting.HideAllEffect)
        {
            for (int i = 0; i < Low.Length; ++i)
            {
                Destroy(Low[i]);
            }

            Low = null;
        }
        else
        {
            for (int i = 0; i < High.Length; ++i)
            {
                Destroy(High[i]);
            }

            High = null;
        }
    }
}
