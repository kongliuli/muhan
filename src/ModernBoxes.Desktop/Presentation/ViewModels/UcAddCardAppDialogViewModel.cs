using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ModernBoxes.Presentation.ViewModels
{
    public class UcAddCardAppDialogViewModel : ObservableObject
    {
        private readonly IPersistenceProvider _persistence;

        private ObservableCollection<CardContentModel> cards = new ObservableCollection<CardContentModel>();

        public ObservableCollection<CardContentModel> CardApps
        {
            get { return cards; }
            set { cards = value; OnPropertyChanged("Cards"); }
        }

        /// <summary>
        /// 修改卡片的高度
        /// </summary>
        public RelayCommand CardHeightChange { get; }

        public UcAddCardAppDialogViewModel(IPersistenceProvider persistence)
        {
            _persistence = persistence;

            CardHeightChange = new RelayCommand((o) =>
            {
                if (o is PreviewSlider previewSlider)
                {
                    String tag = previewSlider.Tag.ToString();
                    Double value = previewSlider.Value;
                    WeakReferenceMessenger.Default.Send(new ChangeCardHeightMessage(Convert.ToInt32(tag), value));
                }
            }, x => true);

            // 关闭对话框时保存设置
            WeakReferenceMessenger.Default.Register<Boolean>(this, "ClosingDialog", (bol) => _ = SaveData(bol));
            _ = init();
        }

        private async Task init()
        {
            try
            {
                var items = await _persistence.LoadAsync<CardContentModel>("cardconfigs");
                foreach (var item in items)
                    CardApps.Add(item);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading card configs");
            }
        }

        public async Task SaveData(Boolean bol)
        {
            try
            {
                await _persistence.SaveAsync("cardconfigs", CardApps);
                WeakReferenceMessenger.Default.Send(new RefreshComponentMessage());
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error saving card configs");
            }
        }
    }
}
