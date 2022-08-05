using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    //数学计算通用类
    public partial class MathUtil:BaseMathUtil
    {
        #region switch direction
        public static bool IsLeft(BaseCoreMono self, BaseCoreMono target)
        {
            return Vector3.Cross(self.Forward, target.Pos).y <= 0;
        }
        public static bool IsRight(BaseCoreMono self, BaseCoreMono target)
        {
            return Vector3.Cross(self.Forward, target.Pos).y > 0;
        }
        // 是否方向相同,m==0表示2个向量垂直。m<0表示2个向量角度>90度。m>0表示2个向量角度<90度。
        public static bool IsSameDirect(Vector3 dir1, Vector3 dir2, float val = 0.5f)
        {
            return Vector3.Dot(dir1, dir2) >= val;
        }
        // 是否方向不同,m==0表示2个向量垂直。m<0表示2个向量角度>90度。m>0表示2个向量角度<90度。
        public static bool IsDiffDirect(Vector3 dir1, Vector3 dir2, float val = 0.0f)
        {
            return Vector3.Dot(dir1, dir2) <= val;
        }
        // 是否面对
        public static bool IsFace(BaseCoreMono self, BaseCoreMono target)
        {
            return Vector3.Dot(self.Forward, target.Pos - self.Pos) >= 0 ? true : false;
        }
        // 是否面对
        public static bool IsFace(BaseCoreMono self, Vector3 target)
        {
            return Vector3.Dot(self.Forward, target - self.Pos) >= 0 ? true : false;
        }
        #endregion

        #region misc
        public static float AutoSqrDistance(Vector3 a, Vector3 b)
        {
            if (IsOrthographic())
                return BaseMathUtil.XYSqrDistance(a, b);
            return BaseMathUtil.XZSqrDistance(a, b);

            bool IsOrthographic()
            {
                if (BaseGlobal.MainCamera == null)
                    return false;
                return BaseGlobal.MainCamera.orthographic;
            }
        }
        #endregion
    }
}

