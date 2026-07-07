using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ModernBoxes.Presentation.ViewModels
{
    public class OneWordViewModel : ObservableObject
    {
        private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(5) };

        private OneWordModel oneWord = new OneWordModel();

        public OneWordModel OneWord
        {
            get { return oneWord; }
            set { oneWord = value; OnPropertyChanged("OneWord"); }
        }

        public RelayCommand RefershOneWord { get; }

        public OneWordViewModel()
        {
            RefershOneWord = new RelayCommand((o) => _ = loadOneNote(), x => true);
            _ = loadOneNote();
        }

        private async Task loadOneNote()
        {
            try
            {
                var json = await _http.GetStringAsync("https://v1.hitokoto.cn/");
                var model = JsonConvert.DeserializeObject<OneWordModel>(json);
                if (model != null)
                    OneWord = model;
            }
            catch (Exception ex)
            {
                // 网络失败静默处理，保留旧内容，不打断用户
                Logger.Error(ex, "Error loading one word");
                if (string.IsNullOrEmpty(OneWord.hitokoto))
                    OneWord = new OneWordModel { hitokoto = "网络暂不可用，请稍后刷新", from = "ModernBoxes" };
            }
        }
    }
}
