using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

/// <summary>
/// 封装iOS手柄的使用
/// </summary>
public class iOSGameController : MonoBehaviour
{
#if USE_GAMECONTROLLER && !UNITY_EDITOR && UNITY_IPHONE
    /// <summary>
    /// iOS基础手柄的键位
    /// </summary>
    public enum KeyCode
    {
        KC_A,
        KC_B,
        KC_X,
        KC_Y,

        KC_Up,
        KC_Down,
        KC_Left,
        KC_Right,

        KC_LeftShoulder,
        KC_RightShoulder,
        
        KC_Count
    }

    public static bool Avialable
    {
        get
        {
            if (IsGameControllerAvialablePlugin())
            {
                if ( !Initialized )
                {
                    LastKeyState = new bool[(int)KeyCode.KC_Count];
                    KeyState = new bool[(int)KeyCode.KC_Count];

                    RefreshKeyState(LastKeyState);

                    Initialized = true;
                }

                return true;
            }

            return false;
        }   
    }

    public static bool GetKey( KeyCode code )
    {
        if ( Avialable )
        {
            return KeyState[(int)code];
        }

        return false;
    }

    public static bool GetKeyUp( KeyCode code )
    {
        if (Avialable)
        {
            if ( LastKeyState[(int)code] && !KeyState[(int)code] )
            {
                return true;
            }
        }

        return false;
    }

    public static bool GetKeyDown( KeyCode code )
    {
        if (Avialable)
        {
            if (!LastKeyState[(int)code] && KeyState[(int)code])
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasPauseEvent
    {
        get
        {
            if ( Avialable )
            {
                return HasGameControllerPauseEventPlugin();
            }

            return false;
        }
    }

    void Update()
    {
        if (Avialable)
        {
            RefreshKeyState(KeyState);
        }
    }

    void LateUpdate()
    {
        if (Avialable)
        {
            for (int i = 0; i < (int)KeyCode.KC_Count; ++i )
            {
                LastKeyState[i] = KeyState[i];
            }
            ClearGameControllerPauseEventPlugin();
        }
    }

    static void RefreshKeyState(bool[] states)
    {
        for (int i = 0; i < (int)KeyCode.KC_Count; ++i)
        {
            states[i] = IsGameControllerKeyPressedPlugin(i);
        }
    }

    static bool[] LastKeyState;
    static bool[] KeyState;
    static bool Initialized;

    [DllImport("__Internal")]
    static extern bool IsGameControllerAvialablePlugin();

    [DllImport("__Internal")]
    static extern bool IsGameControllerKeyPressedPlugin(int keyCode);

    [DllImport("__Internal")]
    static extern bool HasGameControllerPauseEventPlugin();

    [DllImport("__Internal")]
    static extern void ClearGameControllerPauseEventPlugin();
    
#endif
}
