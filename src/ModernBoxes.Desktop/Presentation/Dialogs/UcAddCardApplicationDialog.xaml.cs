using CommunityToolkit.Mvvm.Messaging;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using ModernBoxes.Presentation.ViewModels;
using System.Windows.Controls;

namespace ModernBoxes.Presentation.Dialogs
{
    /// <summary>UcAddCardApplicationDialog.xaml 的交互逻辑</summary>
    public partial class UcAddCardApplicationDialog : UserControl
    {
        public UcAddCardApplicationDialog()
        {
            InitializeComponent();
        }

        /// <summary>切换卡片显示状态</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CB_ChangeCardApp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            CardContentModel? cardContentModel = new CardContentModel();
            string? id = cb.Tag.ToString();
            System.Type type = cb.DataContext.GetType();
            cardContentModel.CardName = type.GetProperty("CardName").GetValue(cb.DataContext).ToString();
            cardContentModel.CardID = int.Parse(id);
            cardContentModel.CardHeight = double.Parse(type.GetProperty("CardHeight").GetValue(cb.DataContext).ToString());
            if (cb != null)
            {
                WeakReferenceMessenger.Default.Send(new CheckedCardAppMessage(int.Parse(id), (bool)cb.IsChecked));
            }
        }
    }
}