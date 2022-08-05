//------------------------------------------------------------------------------
// IMUIWaterMarker.cs
// Copyright 2020 2020/12/24 
// Created by CYM on 2020/12/24
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using RapidGUI;
using Sirenix.OdinInspector;

namespace CYM
{
	public enum WaterMarkerPos
	{
		LeftTop = 0,
		RightTop,
		RightBot,
		LeftBot,
		Center,
		Top,
	}
	[HideMonoScript]
	public class IMUIWaterMarker : MonoBehaviour
	{
		[HideInInspector]
		public Color tintColor = Color.red;
		[HideInInspector]
		public WaterMarkerPos position = WaterMarkerPos.Top;
		[HideInInspector]
		public Vector2 offset = new Vector2(5,5);
		[HideInInspector]
		[Range(0, 5f)]
		public float fadeInTime = 0.5f;
		static string WaterMarkerStr;

		private static bool show = false;
		private static float timer;
		private static Rect logoRect;

		// Update is called once per frame
		void OnGUI()
		{
			GUI.depth = -1;
			if (show)
			{
				GUI.color = tintColor;
				AdjustLogo();
				GUI.Label(logoRect, new GUIContent(WaterMarkerStr), RGUIStyle.warningLabel);
			}
		}

		public static void Show(string str)
		{
			WaterMarkerStr = str;
			show = true;
			timer = Time.time;
		}

		public static void Hide()
		{
			show = false;
		}

		public void ChangePosition(WaterMarkerPos newPosition)
		{
			position = newPosition;
		}

		private void AdjustLogo()
		{

			float newLogoWidth = 500;
			float newLogoHeight = 40;
			switch (position)
			{
				case WaterMarkerPos.LeftBot:
					logoRect = new Rect(offset.x,
									(Screen.height - newLogoHeight) - (offset.y),
									newLogoWidth, newLogoHeight);
					break;
				case WaterMarkerPos.LeftTop:
					logoRect = new Rect(offset.x,
									offset.y,
									newLogoWidth, newLogoHeight);
					break;
				case WaterMarkerPos.RightBot:
					logoRect = new Rect((Screen.width - newLogoWidth) - (offset.x),
									(Screen.height - newLogoHeight) - (offset.y),
									newLogoWidth, newLogoHeight);
					break;
				case WaterMarkerPos.RightTop:
					logoRect = new Rect((Screen.width - newLogoWidth) - (offset.x),
										(offset.y ),
										newLogoWidth, newLogoHeight);
					break;
				case WaterMarkerPos.Center:
					logoRect = new Rect((Screen.width * 0.5f) - (newLogoWidth * 0.5f),
									(Screen.height * 0.5f) - (newLogoHeight * 0.5f),
									newLogoWidth, newLogoHeight);
					break;
				case WaterMarkerPos.Top:
					logoRect = new Rect((Screen.width * 0.5f) - (newLogoWidth * 0.5f),
									newLogoHeight + offset.y,
									newLogoWidth, newLogoHeight);
					break;
			}
		}
	}
}