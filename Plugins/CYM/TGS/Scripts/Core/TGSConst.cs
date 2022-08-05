using UnityEngine;


namespace CYM.TGS
{
	public static class TGSConst
	{
		public static Vector4 Vector4zero = Vector4.zero;
		public static Vector3 Vector3zero = Vector3.zero;
		public static Vector3 Vector3one = Vector3.one;
		public static Vector3 Vector3up = Vector3.up;
		public static Vector2 Vector2left = Vector2.left;
		public static Vector2 Vector2right = Vector2.right;
		public static Vector2 Vector2one = Vector2.one;
		public static Vector2 Vector2zero = Vector2.zero;
		public static Vector2 Vector2down = Vector2.down;
		public static Vector2 Vector2half = Vector2.one * 0.5f;
		public static Color ColorNull = new Color (0, 0, 0, 0);

        public static int FadeAmount = Shader.PropertyToID("_FadeAmount");
        public static int Scale = Shader.PropertyToID("_Scale");
        public static int Offset = Shader.PropertyToID("_Offset");
        public static int ZWrite = Shader.PropertyToID("_ZWrite");
        public static int SrcBlend = Shader.PropertyToID("_SrcBlend");
        public static int DstBlend = Shader.PropertyToID("_DstBlend");
        public static int Cull = Shader.PropertyToID("_Cull");
        public static int ZTest = Shader.PropertyToID("_ZTest");
        public static int StencilRef = Shader.PropertyToID("_StencilRef");
        public static int StencilComp = Shader.PropertyToID("_StencilComp");
        public static int StencilOp = Shader.PropertyToID("_StencilOp");
        public static int NearClip = Shader.PropertyToID("_NearClip");
        public static int FallOff = Shader.PropertyToID("_FallOff");
        public static int FarFadeDistance = Shader.PropertyToID("_FarFadeDistance");
        public static int FarFadeFallOff = Shader.PropertyToID("_FarFadeFallOff");
        public static int Color = Shader.PropertyToID("_Color");
        public static int Color2 = Shader.PropertyToID("_Color2");
        public static int MainTex = Shader.PropertyToID("_MainTex");
        public static int Thickness = Shader.PropertyToID("_Thickness");
        public static int CircularFadePosition = Shader.PropertyToID("_CircularFadePosition");
        public static int CircularFadeDistanceSqr = Shader.PropertyToID("_CircularFadeDistanceSqr");
        public static int CircularFadeFallOff = Shader.PropertyToID("_CircularFadeFallOff");

        public static string Truncate(string s, int length)
        {
            if (string.IsNullOrEmpty(s)) return "";
            int len = s.Length < length ? s.Length : length;
            return s.Substring(0, len);
        }
    }

}