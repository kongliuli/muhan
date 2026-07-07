using ModernBoxes.Core.Interfaces;
using ModernBoxes.Sdk.Plugins;
using ModernBoxes.Core.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ModernBoxes.Cards;

[CardExport("一言", Author = "ModernBoxes", Version = "3.0", Description = "一言卡片-随机获取一句话")]
public class OneWordCardViewModel : CardBase<OneWordModel>
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(5) };
    private static readonly string _cachePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ModernBoxes",
        "OneWordCache.json");

    public OneWordCardViewModel() : base(new OneWordModel())
    {
        CardID = "oneword";
        CardHeight = 150;
        Preview = "pack://application:,,,/Resource/image/previews/onenote.png";
        IsChecked = false;
    }

    public override async Task LoadAsync()
    {
        // 先展示缓存，弱网时不至于空白，然后后台刷新
        var cached = LoadCache();
        if (cached != null)
            CardContent = cached;
        await FetchHitokoto();
    }

    public override async Task RefreshAsync() => await FetchHitokoto();

    private async Task FetchHitokoto()
    {
        try
        {
            var json = await _http.GetStringAsync("https://v1.hitokoto.cn/");
            var model = JsonConvert.DeserializeObject<OneWordModel>(json);
            if (model == null)
            {
                if (CardContent is not OneWordModel)
                    CardContent = FallbackContent("解析失败");
                return;
            }
            CardContent = model;
            SaveCache(json);
        }
        catch (Exception)
        {
            // 已有缓存内容时静默失败，保留上一条
            if (CardContent is not OneWordModel m || string.IsNullOrEmpty(m.hitokoto))
                CardContent = FallbackContent("网络暂不可用，请稍后刷新");
        }
    }

    private static OneWordModel? LoadCache()
    {
        try
        {
            if (!File.Exists(_cachePath))
                return null;
            return JsonConvert.DeserializeObject<OneWordModel>(File.ReadAllText(_cachePath));
        }
        catch { return null; }
    }

    private static void SaveCache(string json)
    {
        try { File.WriteAllText(_cachePath, json); }
        catch { /* ponytail: 缓存写失败不影响功能 */ }
    }

    private static OneWordModel FallbackContent(string hint) =>
        new() { hitokoto = hint, from = "ModernBoxes" };
}
