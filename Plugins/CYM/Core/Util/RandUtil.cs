using System.Collections.Generic;
using UnityEngine;
using URandom;
using Normalization = UnityRandom.Normalization;

namespace CYM
{
    public partial class RandUtil: BaseRandUtil
    {
        private static UnityRandom URAND;
        [RuntimeInitializeOnLoadMethod]
        static void InitRandom()
        {
            URAND = new UnityRandom(System.DateTime.Now.Millisecond);
            Random.InitState(System.DateTime.Now.Millisecond);
        }

        #region normal
        public static void RandForwardY(BaseUnit unit, Vector3 upwards)
        {
            if (upwards == Vector3.up)
                unit.Trans.eulerAngles = new Vector3(unit.Trans.eulerAngles.x, Range(0.0f, 360.0f), unit.Trans.eulerAngles.z);
            else if (upwards == Vector3.forward)
                unit.Trans.eulerAngles = new Vector3(unit.Trans.eulerAngles.x, unit.Trans.eulerAngles.y, Range(0.0f, 360.0f));
            else if (upwards == Vector3.right)
                unit.Trans.eulerAngles = new Vector3(Range(0.0f, 360.0f), unit.Trans.eulerAngles.y, unit.Trans.eulerAngles.z);
        }
        //随机年龄
        public static int RandAge(List<AgeRange> range)
        { 
            return RandUtil.RangeInt(GameConfig.Ins.AgeRangeData[range.Rand()]);
        }
        #endregion

        #region urand
        // VALUE Return a Float 0 - 1
        public static float Value() => URAND.Value();
        // VALUE Return a Float 0 - 1
        public static float Value(Normalization n, float t) => URAND.Value(n, t);
        // POISSON Return a Float
        public static float Possion(float lambda) => URAND.Possion(lambda);
        // EXPONENTIAL Return a Float
        public static float Exponential(float lambda) => URAND.Exponential(lambda);
        // GAMMA Return a Float
        public static float Gamma(float order) => URAND.Gamma(order);
        // POINT IN A SQUARE Return a Vector2
        public static Vector2 PointInASquare() => URAND.PointInASquare();
        // POINT IN A SQUARE Return a Vector2
        public static Vector2 PointInASquare(Normalization n, float t) => URAND.PointInASquare(n, t);
        // RANDOM POINT IN A CIRCLE centered at 0
        // FROM http://mathworld.wolfram.com/CirclePointPicking.html
        // Take a number between 0 and 2PI and move to Cartesian Coordinates
        public static Vector2 PointInACircle() => URAND.PointInACircle();
        // RANDOM POINT IN A CIRCLE centered at 0
        // FROM http://mathworld.wolfram.com/CirclePointPicking.html
        // Take a number between 0 and 2PI and move to Cartesian Coordinates
        public Vector2 PointInACircle(Normalization n, float t) => URAND.PointInACircle(n, t);
        // RANDOM POINT in a DISK
        // FROM http://mathworld.wolfram.com/DiskPointPicking.html
        public Vector2 PointInADisk() => URAND.PointInADisk();
        // RANDOM POINT in a DISK
        // FROM http://mathworld.wolfram.com/DiskPointPicking.html
        public Vector2 PointInADisk(Normalization n, float t) => URAND.PointInADisk(n, t);
        // RANDOM POINT IN A CUBE. Return a Vector3
        public Vector3 PointInACube() => URAND.PointInACube();
        // RANDOM POINT IN A CUBE. Return a Vector3
        public Vector3 PointInACube(Normalization n, float t) => URAND.PointInACube(n, t);
        // RANDOM POINT ON A CUBE. Return a Vector3
        public Vector3 PointOnACube() => URAND.PointOnACube();
        // RANDOM POINT ON A CUBE. Return a Vector3
        public Vector3 PointOnACube(Normalization n, float t) => URAND.PointOnACube(n, t);
        // RANDOM POINT ON A SPHERE. Return a Vector3
        public Vector3 PointOnASphere() => URAND.PointOnASphere();
        // RANDOM POINT ON A SPHERE. Return a Vector3
        public Vector3 PointOnASphere(Normalization n, float t) => URAND.PointOnASphere(n, t);
        // RANDOM POINT IN A SPHERE. Return a Vector3
        public Vector3 PointInASphere() => URAND.PointInASphere();
        // RANDOM POINT IN A SPHERE. Return a Vector3
        public Vector3 PointInASphere(Normalization n, float t) => URAND.PointInASphere(n, t);
        // RANDOM POINT IN A CAP. Return a Vector3 
        // TODO: see RandomSphere GetPointOnCap(float spotAngle, ref NPack.MersenneTwister _rand, Quaternion orientation)
        public Vector3 PointOnCap(float spotAngle) => URAND.PointOnCap(spotAngle);
        public Vector3 PointOnCap(float spotAngle, Normalization n, float t) => URAND.PointOnCap(spotAngle, n, t);
        // RANDOM POINT IN A RING on a SPHERE. Return a Vector3 
        // TODO: see RandomSphere public static Vector3 GetPointOnRing(float innerSpotAngle, float outerSpotAngle, ref NPack.MersenneTwister _rand, Quaternion orientation)
        public Vector3 PointOnRing(float innerAngle, float outerAngle) => URAND.PointOnRing(innerAngle, outerAngle);
        public Vector3 PointOnRing(float innerAngle, float outerAngle, Normalization n, float t) => URAND.PointOnRing(innerAngle, outerAngle, n, t);
        // RANDOM RAINBOW COLOR
        public Color Rainbow() => URAND.Rainbow();
        // RANDOM RAINBOW COLOR
        public Color Rainbow(Normalization n, float t) => URAND.Rainbow(n, t);
        // RANDOM DICES
        public DiceRoll RollDice(int size, DiceRoll.DiceType type) => URAND.RollDice(size, type);
        // START a FLOAT SHUFFLE BAG
        // Note the a value can be shuffled with himself
        public ShuffleBagCollection<float> ShuffleBag(float[] values) => URAND.ShuffleBag(values);
        // START a WIGHTED FLOAT SHUFFLE BAG, the trick is the it is added many times
        // Note the a value can be shuffled with himself
        public ShuffleBagCollection<float> ShuffleBag(Dictionary<float, int> dict) => URAND.ShuffleBag(dict);
        #endregion
    }
}