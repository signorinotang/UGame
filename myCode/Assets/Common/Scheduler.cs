/********************************************************************
	created:	2013/11/14
	created:	14:11:2013   17:58
	filename: 	E:\Alice\UnityProj\Assets\Scripts\Common\Scheduler.cs
	file path:	E:\Alice\UnityProj\Assets\Scripts\Common
	file base:	Scheduler
	file ext:	cs
	author:		benzhou
	
	purpose:	调度器
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Common;
using UnityEngine;

/// <summary>
/// 调度器
/// </summary>
public class Scheduler : MonoSingleton<Scheduler>
{
    /// <summary>
    /// 更新委托
    /// </summary>
    public delegate void UpdateDelegate();

    /// <summary>
    /// 协同委托
    /// </summary>
    public delegate IEnumerator CoroutineDelegate();

    /// <summary>
    /// 定时委托
    /// </summary>
    public delegate void TimerFrameDelegate();

    /// <summary>
    /// 协同数据
    /// </summary>
    private class CoroutineData
    {
        // 是否销毁
        public bool Destroyed;

        // 接收器
        public CoroutineDelegate Handler;

        // 枚举
        public IEnumerator Enumerator;
    }

    /// <summary>
    /// 定时数据
    /// </summary>
    private class TimerData
    {
        // 间隔
        public float Interval;

        // 剩余时间
        public float RemainTime;

        // 是否重复
        public bool Repeat;

        // 是否销毁
        public bool Destroyed;

        // 定时接收器
        public TimerFrameDelegate TimerHandler;
    }

    /// <summary>
    /// 定帧数据
    /// </summary>
    private class FrameData
    {
        // 间隔
        public uint Interval;

        // 剩余帧数
        public uint RemainFrame;

        // 是否重复
        public bool Repeat;

        // 是否销毁
        public bool Destroyed;

        // 定帧接收器
        public TimerFrameDelegate FrameHandler;
    }

    // 更新接收器
    private event UpdateDelegate UpdateHandler;

    // 定时数据
    private List<CoroutineData> _coroutineData = new List<CoroutineData>();

    // 定时数据
    private List<TimerData> _timerData = new List<TimerData>();

    // 定帧数据
    private List<FrameData> _frameData = new List<FrameData>();

    /// <summary>
    /// 添加更新委托
    /// </summary>
    /// <param name="d">更新委托</param>
    public void AddUpdate(UpdateDelegate d)
    {
        UpdateHandler += d;
    }

    /// <summary>
    /// 移除更新委托
    /// </summary>
    /// <param name="d">更新委托</param>
    public void RemoveUpdate(UpdateDelegate d)
    {
        UpdateHandler -= d;
    }

    /// <summary>
    /// 添加协同委托
    /// </summary>
    /// <param name="d">协同委托</param>
    public void AddCoroutine(CoroutineDelegate d)
    {
        if (d == null)
        {
            return;
        }

        var data = new CoroutineData
        {
            Destroyed = false,
            Handler = d
        };

        _coroutineData.Add(data);
    }

    /// <summary>
    /// 移除协同委托
    /// </summary>
    /// <param name="d">协同委托</param>
    public void RemoveCoroutine(CoroutineDelegate d)
    {
        for (var i = 0; i < _coroutineData.Count; i++)
        {
            if (_coroutineData[i].Handler == d)
            {
                _coroutineData[i].Destroyed = true;
            }
        }
    }

    /// <summary>
    /// 添加定时
    /// </summary>
    /// <param name="interval">间隔时间</param>
    /// <param name="repeat">是否重复</param>
    /// <param name="d">定时委托</param>
    public void AddTimer(float interval, bool repeat, TimerFrameDelegate d)
    {
        if (interval < 0.0f || d == null)
        {
            return;
        }

        var data = new TimerData
        {
            Interval = interval,
            RemainTime = interval,
            Repeat = repeat,
            Destroyed = false,
            TimerHandler = d
        };

        _timerData.Add(data);
    }

    /// <summary>
    /// 移除定时
    /// </summary>
    /// <param name="d">定时委托</param>
    public void RemoveTimer(TimerFrameDelegate d)
    {
        for (var i = 0; i < _timerData.Count; i++)
        {
            if (_timerData[i].TimerHandler == d)
            {
                _timerData[i].Destroyed = true;
            }
        }
    }

    /// <summary>
    /// 添加定帧
    /// </summary>
    /// <param name="interval">间隔帧数</param>
    /// <param name="repeat">是否重复</param>
    /// <param name="d">定帧委托</param>
    public void AddFrame(uint interval, bool repeat, TimerFrameDelegate d)
    {
        if (interval == 0 || d == null)
        {
            return;
        }

        var data = new FrameData
        {
            Interval = interval,
            RemainFrame = interval,
            Repeat = repeat,
            Destroyed = false,
            FrameHandler = d
        };

        _frameData.Add(data);
    }

    /// <summary>
    /// 移除定帧
    /// </summary>
    /// <param name="d">定帧委托</param>
    public void RemoveFrame(TimerFrameDelegate d)
    {
        for (var i = 0; i < _frameData.Count; i++)
        {
            if (_frameData[i].FrameHandler == d)
            {
                _frameData[i].Destroyed = true;
            }
        }
    }

    // MonoBehaviour
    protected void Update()
    {
        // 更新
        var updateHandler = UpdateHandler;
        if (null != updateHandler)
        {
            updateHandler();
        }

        // 协同
        for (var i = 0; i < _coroutineData.Count; i++)
        {
            var data = _coroutineData[i];

            if (data.Destroyed)
            {
                continue;
            }

            if (data.Enumerator == null)
            {
                data.Enumerator = data.Handler();
            }

            if (!data.Enumerator.MoveNext())
            {
                data.Destroyed = true;
            }
        }

        for (var i = 0; i < _coroutineData.Count; )
        {
            if (_coroutineData[i].Destroyed)
            {
                _coroutineData.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }

        // 定时
        var deltaTime = Time.deltaTime;

        for (var i = 0; i < _timerData.Count; i++)
        {
            if (_timerData[i].Destroyed)
            {
                continue;
            }

            _timerData[i].RemainTime -= deltaTime;

            if (_timerData[i].RemainTime > 0.0f)
            {
                continue;
            }

            if (null != _timerData[i].TimerHandler)
            {
                _timerData[i].TimerHandler();
            }

            if (_timerData[i].Repeat)
            {
                _timerData[i].RemainTime += _timerData[i].Interval;
            }
            else
            {
                _timerData[i].Destroyed = true;
            }
        }

        for (var i = 0; i < _timerData.Count;)
        {
            if (_timerData[i].Destroyed)
            {
                _timerData.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }

        // 定帧
        for (var i = 0; i < _frameData.Count; i++)
        {
            if (_frameData[i].Destroyed)
            {
                continue;
            }

            --_frameData[i].RemainFrame;
            if (_frameData[i].RemainFrame > 0)
            {
                continue;
            }

            if (null != _frameData[i].FrameHandler)
            {
                _frameData[i].FrameHandler();
            }

            if (_frameData[i].Repeat)
            {
                _frameData[i].RemainFrame = _frameData[i].Interval;
            }
            else
            {
                _frameData[i].Destroyed = true;
            }
        }

        for (var i = 0; i < _frameData.Count; i++)
        {
            if (_frameData[i].Destroyed)
            {
                _frameData.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }
}
