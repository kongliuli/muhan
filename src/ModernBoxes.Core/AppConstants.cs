namespace ModernBoxes.Core
{
    public static class AppConstants
    {
        /// <summary>
        /// 特殊菜单项：点击后展开卡片面板（曾因文件编码损坏散落为乱码魔法字符串）。
        /// 该值会写入用户的 MenuConfig.json，修改会导致老数据失配，勿改。
        /// </summary>
        public const string ComponentAppMenuName = "组件应用";
    }
}
