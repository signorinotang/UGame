using UnityEngine;
using System.Collections;

/// <summary>
/// 根据帧率动态调整画质
/// </summary>
public class ssGFXQualitySelector_Dynamic : ssGFXQualitySelector
{
	public override void Initialize()
	{

	}

	public override void Tick()
	{
		if (bGameMode)
		{
			float curTime = Time.realtimeSinceStartup;
			if (bEstimating)
			{
				++FrameCount;
				float elapsedTime = curTime - EstimateStartTime;

				if (elapsedTime > EstimateInterval)
				{
					float avgFPS = FrameCount / elapsedTime;
					SelectQuality(avgFPS);

					ResetEstimate(curTime);
				}

			}
			else
			{
				if (curTime - EnterGameModeTime > EstimateDelay)
				{
					bEstimating = true;
					ResetEstimate(curTime);
				}
			}
		}
	}

	public override void OnAppPause(bool bPause)
	{
		if (!bPause)
		{
			ResetEstimate(Time.realtimeSinceStartup);
		}
	}

	public override void OnEnterGameMode()
	{
		bGameMode = true;
		EnterGameModeTime = Time.realtimeSinceStartup;
	}

	public override void OnExitGameMode(ssGFXQuality.Settings curSettings)
	{
		bGameMode = false;
		bEstimating = false;
	}

	protected virtual void SelectQuality(float fps)
	{
		if (fps < DowngradeFPS)
		{
			if (CurrentQuality < ssGFXQuality.EQualityLevel.QL_VeryLow)
			{
				CurrentQuality += 1;
				//ssGFXQuality.Instance.Apply(CurrentQuality);

				bCanUpgrade = false;

				////ssLogger.Log("Downgrade Quality to: " + CurrentQuality);
			}

		}
		else if (bCanUpgrade && fps > UpgradeFPS)
		{
			if (CurrentQuality > ssGFXQuality.EQualityLevel.QL_Ultra)
			{
				CurrentQuality -= 1;
				//ssGFXQuality.Instance.Apply(CurrentQuality);

				////ssLogger.Log("Upgrade Quality to: " + CurrentQuality);
			}
		}
	}

	void ResetEstimate(float curTime)
	{
		FrameCount = 0;
		EstimateStartTime = curTime;
	}

	// 进入单局后的评估延迟时间
	public float EstimateDelay = 5.0f;
	// 一次评估间隔时间
	public float EstimateInterval = 15.0f;

	// 帧率小于此则降级
	public float DowngradeFPS = 16.0f;
	// 帧率大于此则升级
	public float UpgradeFPS = 27.0f;

	[HideInInspector]
	public bool bCanUpgrade = true;

	protected bool bGameMode = false;
	protected bool bEstimating = false;
	protected float EnterGameModeTime = 0.0f;

	protected int FrameCount = 0;
	protected float EstimateStartTime = 0.0f;

	protected ssGFXQuality.EQualityLevel CurrentQuality = ssGFXQuality.EQualityLevel.QL_Middle;
}
