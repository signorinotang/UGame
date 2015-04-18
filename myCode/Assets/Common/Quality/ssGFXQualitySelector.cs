using UnityEngine;
using System.Collections;

public abstract class ssGFXQualitySelector : MonoBehaviour
{
	public virtual ssGFXQuality.EQualityLevel InitQuality
	{
		get
		{
			return ssGFXQuality.EQualityLevel.QL_Middle;
		}
	}

	public virtual void Initialize() { }
	public virtual void Tick() { }
	public virtual void OnAppPause(bool bPause) { }
	public virtual void OnEnterGameMode() { }
	public virtual void OnExitGameMode(ssGFXQuality.Settings curSettings) { }
}
