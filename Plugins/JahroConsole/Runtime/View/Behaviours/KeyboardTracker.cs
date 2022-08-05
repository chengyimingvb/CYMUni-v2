using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JahroConsole.View
{
    internal class KeyboardTracker : MonoBehaviour
    {

        public Action OnTildaPressed = delegate{};

        public Action OnEscPressed = delegate{};

        public Action SwitchToTextMode = delegate{};

        public Action SwitchToVisualMode = delegate{};

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                OnTildaPressed();
            }   
            else if (Input.GetKeyDown(KeyCode.Escape) && Application.isMobilePlatform)
            {
                OnEscPressed();   
            }
            else if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Alpha1))
            {
                SwitchToTextMode();
            }
            else if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Alpha2))
            {
                SwitchToVisualMode();
            }
        }

        public static float GetSoftKeyboardHeight()
        {

            if (Application.isEditor)
            {
                return 0;//0.565f; //Default value for editor
            }


#if UNITY_ANDROID
            using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
                using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
                {
                    View.Call("getWindowVisibleDisplayFrame", rect);
                    return (float)(Screen.height - rect.Call<int>("height")) / Screen.height;
                }
            }
#else
        return (float)TouchScreenKeyboard.area.height / Screen.height;
#endif
        }
    }
}