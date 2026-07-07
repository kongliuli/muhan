using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Infrastructure.Plugins;
using ModernBoxes.Sdk.Plugins;
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
            set { cards = value; OnPropertyChanged(nameof(CardApps)); }
        }

        public RelayCommand CardHeightChange { get; }
        public RelayCommand MoveUpCommand { get; }
        public RelayCommand MoveDownCommand { get; }

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

            MoveUpCommand = new RelayCommand((o) => MoveCard(o as CardContentModel, -1), o => o is CardContentModel);
            MoveDownCommand = new RelayCommand((o) => MoveCard(o as CardContentModel, 1), o => o is CardContentModel);

            WeakReferenceMessenger.Default.Register<Boolean>(this, "ClosingDialog", (bol) => _ = SaveData(bol));
            _ = init();
        }

        private async Task init()
        {
            try
            {
                var configItems = (await _persistence.LoadAsync<CardContentModel>("cardconfigs")).ToList();
                var availableCards = CardPluginLoader.GetAvailableCards();
                int nextCardId = configItems.Count > 0 ? configItems.Max(c => c.CardID) + 1 : 0;
                int nextOrder = 0;

                CardApps.Clear();
                foreach (var cardMeta in availableCards.OrderBy(c => c.CardName))
                {
                    var config = configItems.FirstOrDefault(c => c.CardName == cardMeta.CardName);
                    var preview = config?.Preview ?? string.Empty;
                    if (string.IsNullOrEmpty(preview) && App.AppHost != null)
                    {
                        var vm = (ICardViewModel)App.AppHost.Services.GetRequiredService(cardMeta.ViewModelType);
                        preview = vm.Preview;
                    }

                    CardApps.Add(new CardContentModel
                    {
                        CardName = cardMeta.CardName,
                        IsChecked = config?.IsChecked ?? false,
                        CardID = config?.CardID ?? nextCardId++,
                        CardHeight = config?.CardHeight ?? 200,
                        Order = config?.Order ?? nextOrder++,
                        Preview = preview,
                    });
                }

                ResortCardApps();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading card configs");
            }
        }

        private void MoveCard(CardContentModel? card, int delta)
        {
            if (card == null)
                return;

            var index = CardApps.IndexOf(card);
            var target = index + delta;
            if (target < 0 || target >= CardApps.Count)
                return;

            var other = CardApps[target];
            (card.Order, other.Order) = (other.Order, card.Order);
            ResortCardApps();
        }

        private void ResortCardApps()
        {
            var sorted = CardApps.OrderBy(c => c.Order).ThenBy(c => c.CardID).ToList();
            CardApps.Clear();
            for (var i = 0; i < sorted.Count; i++)
            {
                sorted[i].Order = i;
                CardApps.Add(sorted[i]);
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
