using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Sdk.Plugins;
using ModernBoxes.Core.Models;
using ModernBoxes.Desktop.Presentation;
using ModernBoxes.Infrastructure;
using ModernBoxes.Infrastructure.Plugins;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ModernBoxes.Presentation.ViewModels
{
    public class UcComponentViewModel : ObservableObject
    {
        private readonly IPersistenceProvider _persistence;

        private ObservableCollection<CardContentModel> cardContents = new ObservableCollection<CardContentModel>();

        public ObservableCollection<CardContentModel> CardContents
        {
            get { return cardContents; }
            set { cardContents = value; OnPropertyChanged("CardContents"); }
        }

        public UcComponentViewModel(IPersistenceProvider persistence)
        {
            _persistence = persistence;
            WeakReferenceMessenger.Default.Register<RefreshComponentMessage>(this, (r, m) => RefreshCardContents());
            WeakReferenceMessenger.Default.Register<ChangeCardHeightMessage>(this, (r, m) => ChangeCardHeight(m.CardId, m.Height));
            WeakReferenceMessenger.Default.Register<CheckedCardAppMessage>(this, (r, m) => CheckCardApp(m.CardID, m.IsShow));
            _ = loadCardContent();
        }

        private void CheckCardApp(int CardID, bool isShow)
        {
            var card = CardContents.FirstOrDefault(o => o.CardID == CardID);
            if (card != null)
                card.IsChecked = isShow;
        }

        private void ChangeCardHeight(int CardId, double height)
        {
            CardContentModel? cardContentModel = CardContents.FirstOrDefault(o => o.CardID == CardId);
            if (cardContentModel != null)
                cardContentModel.CardHeight = height;
        }

        private void RefreshCardContents()
        {
            CardContents.Clear();
            _ = loadCardContent();
        }

        private async Task loadCardContent()
        {
            try
            {
                var configItems = (await _persistence.LoadAsync<CardContentModel>("cardconfigs")).ToList();
                var availableCards = CardPluginLoader.GetAvailableCards();

                // 按卡片名称匹配配置，而不是按下标：插件增删或反射顺序变化时配置不会错乱
                int nextCardId = configItems.Count > 0 ? configItems.Max(c => c.CardID) + 1 : 0;
                foreach (var cardMeta in availableCards)
                {
                    var config = configItems.FirstOrDefault(c => c.CardName == cardMeta.CardName);

                    var viewModel = (ICardViewModel)App.AppHost!.Services.GetRequiredService(cardMeta.ViewModelType);
                    _ = Task.Run(async () =>
                    {
                        try { await viewModel.LoadAsync(); }
                        catch (Exception ex) { Logger.Error(ex, $"Error loading card '{cardMeta.CardName}'"); }
                    });

                    var content = new CardContentModel()
                    {
                        CardName = cardMeta.CardName,
                        IsChecked = config?.IsChecked ?? false,
                        CardID = config?.CardID ?? nextCardId++,
                        CardHeight = config?.CardHeight ?? viewModel.CardHeight,
                        CardContent = CardViewResolver.Resolve(cardMeta.CardName, cardMeta.ViewType, viewModel)
                    };

                    _ = Application.Current.Dispatcher.InvokeAsync(() => CardContents.Add(content));
                }
            }
            catch (Exception ex) { Logger.Error(ex, "Error loading card content"); }
        }
    }
}
