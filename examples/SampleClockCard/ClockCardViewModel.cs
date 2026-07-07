using ModernBoxes.Core.Interfaces;
using ModernBoxes.Sdk.Plugins;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SampleClockCard;

[CardExport("示例时钟", Author = "muhan", Version = "1.0", Description = "插件示例：显示当前时间", ViewType = typeof(ClockCardView))]
public class ClockCardViewModel : CardBase<object>
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

    public ClockCardViewModel() : base(new object())
    {
        CardID = "sample-clock";
        CardHeight = 120;
        CardName = "示例时钟";
        CardContent = DateTime.Now.ToString("HH:mm:ss");
        _timer.Tick += (_, _) => CardContent = DateTime.Now.ToString("HH:mm:ss");
    }

    public override Task LoadAsync()
    {
        _timer.Start();
        return Task.CompletedTask;
    }

    public override Task RefreshAsync() => LoadAsync();
}

public class ClockCardView : UserControl
{
    public ClockCardView()
    {
        Content = new TextBlock
        {
            FontSize = 28,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
        };
        DataContextChanged += (_, _) =>
        {
            if (Content is TextBlock block)
                block.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("CardContent"));
        };
    }
}
