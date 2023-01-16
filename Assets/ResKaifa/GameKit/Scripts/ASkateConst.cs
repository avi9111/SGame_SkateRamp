namespace ResKaifa.GameKit
{
    public class ASkateConst
    {
        public static bool UseOldDeadHandle = false;//暂时不要，死亡判断（old)有问题
        public const int SaveLastPosInterval = 6;
        public const float GravitySensive = -2f;
        public const float GravityJumpUp = 18f;
        /// <summary>
        /// //skate- 有错误;
        /// rokie    -空（暂时）；
        /// default2022 -测试通过（操作不顺）；
        /// hist    -开发中；(搞坏了。。。。）
        /// 2023    -开发中
        /// </summary>
        public const string VeclocityHandleKey = "2023";
    }
}
