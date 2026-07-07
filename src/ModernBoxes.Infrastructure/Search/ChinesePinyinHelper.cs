using System;
using System.Collections.Generic;
using System.Text;

namespace ModernBoxes.Infrastructure.Search
{
    /// <summary>
    /// ponytail: 常用汉字首字母表，覆盖搜索场景常见字；未收录字退化为原文匹配。
    /// </summary>
    public static class ChinesePinyinHelper
    {
        private static readonly Dictionary<char, char> Initials = BuildInitials();

        public static string GetInitials(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var sb = new StringBuilder(text.Length);
            foreach (var ch in text)
            {
                if (ch <= 127)
                    sb.Append(char.ToLowerInvariant(ch));
                else if (Initials.TryGetValue(ch, out var initial))
                    sb.Append(initial);
            }
            return sb.ToString();
        }

        public static bool Matches(string query, string text)
        {
            if (string.IsNullOrWhiteSpace(query) || string.IsNullOrEmpty(text))
                return false;

            var q = query.Trim();
            if (text.Contains(q, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!IsAsciiLetters(q))
                return false;

            var initials = GetInitials(text);
            return initials.Contains(q, StringComparison.OrdinalIgnoreCase);
        }

        public static bool LooksLikePinyinQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return false;
            var q = query.Trim();
            return q.Length is >= 1 and <= 4 && IsAsciiLetters(q);
        }

        private static bool IsAsciiLetters(string value)
        {
            foreach (var ch in value)
            {
                if (ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z')
                    continue;
                return false;
            }
            return value.Length > 0;
        }

        private static Dictionary<char, char> BuildInitials()
        {
            var map = new Dictionary<char, char>();
            void Add(string chars, char initial)
            {
                foreach (var ch in chars)
                    map[ch] = initial;
            }

            Add("阿啊", 'a'); Add("八吧巴把爸拔白百班般半帮包保报北被本比笔必边变便表别并病不步部", 'b');
            Add("才菜参草层查差产长常场厂车成城程吃出初处传春词此从", 'c');
            Add("大打代带单但当到道的地第点电东动都读对多", 'd');
            Add("而儿二", 'e');
            Add("发法反方房放飞分风服福父复", 'f');
            Add("该改干感刚高告哥个给根更工公功共构古故关观管光广规国过", 'g');
            Add("还海好号合和河黑很红后候话化划画话怀坏欢回会活火", 'h');
            Add("机及即集几计记际继家加价假间见建江将讲交教接街节结解界今金进近经精景九就旧居局举句具据决觉军", 'j');
            Add("开看康可课空口快块况困扩", 'k');
            Add("来兰蓝老乐类冷离李里理力历立利连联脸两亮量了料列林临零领流六龙路录论", 'l');
            Add("妈马吗买卖满忙毛么没每美门们面民名明命模目木", 'm');
            Add("那内能你年念鸟您牛农女", 'n');
            Add("哦", 'o');
            Add("怕排派盘跑配朋片票平破", 'p');
            Add("七期其奇起气千前钱强桥切亲青轻清情请秋求区去全却", 'q');
            Add("然让热人认任日容如入", 'r');
            Add("三色山商上少社设身深什神生声省胜师十时实识史使世事是手首受书数树双水说思四", 's');
            Add("他她它台太谈汤堂特题体天条听通同头图推退", 't');
            Add("外完玩晚万王网往望为位未温文问我无五物", 'w');
            Add("西息希习系细下夏先现相想向小校笑些心新信星行形型性姓兄修学", 'x');
            Add("呀压言眼阳样要也业一医已以意因音银应英影用由游有又右于鱼语元员原远月云", 'y');
            Add("再在早造则怎增展站张找这真正政知直值只指至制治中终种重周主住助注专转装准资子字自总走足组最作做坐", 'z');

            // 应用名常见字补充
            Add("微信微", 'w'); Add("信", 'x');
            Add("钉", 'd'); Add("腾讯", 't');
            Add("网网易", 'w'); Add("易", 'y');
            Add("软", 'r'); Add("件", 'j');
            Add("浏览", 'l'); Add("览", 'l');
            Add("邮", 'y'); Add("箱", 'x');
            Add("音", 'y'); Add("乐", 'l');
            Add("视", 's'); Add("频", 'p');
            Add("图", 't'); Add("片", 'p');
            Add("便", 'b'); Add("签", 'q');
            Add("文", 'w'); Add("档", 'd');
            Add("夹", 'j'); Add("夹", 'j');

            return map;
        }
    }
}
