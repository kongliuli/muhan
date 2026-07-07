using Microsoft.Extensions.Hosting;
using ModernBoxes.Core.Enums;
using ModernBoxes.Desktop.Windows;
using ModernBoxes.Infrastructure;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ModernBoxes.Desktop.HostedServices
{
    public sealed class EdgeHoverHostedService : IHostedService
    {
        private EdgeSentinelWindow? _sentinel;
        private DispatcherTimer? _enterTimer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WeakReferenceMessenger.Default.Register<HoverConfigChangedMessage>(this, (_, _) => RefreshSentinel());
                RefreshSentinel();
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Application.Current.Dispatcher.Invoke(DisposeSentinel);
            WeakReferenceMessenger.Default.Unregister<HoverConfigChangedMessage>(this);
            return Task.CompletedTask;
        }

        private void RefreshSentinel()
        {
            DisposeSentinel();
            if (!IsHoverEnabled())
                return;

            var leftSide = GetCardSide() == CommentLayout.left;
            _sentinel = new EdgeSentinelWindow();
            _sentinel.EdgeEnter += OnEdgeEnter;
            _sentinel.EdgeLeave += OnEdgeLeave;
            _sentinel.Reposition(leftSide);
            _sentinel.Show();
        }

        private void OnEdgeEnter()
        {
            _enterTimer?.Stop();
            _enterTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _enterTimer.Tick += (_, _) =>
            {
                _enterTimer.Stop();
                ActivateHover();
            };
            _enterTimer.Start();
        }

        private void OnEdgeLeave()
        {
            _enterTimer?.Stop();
        }

        private void ActivateHover()
        {
            if (Application.Current.MainWindow is not MainWindow main)
                return;

            main.EnsureVisibleForHover();
            main.ExpandCardPanelFromHover();
        }

        private static bool IsHoverEnabled()
        {
            var hoverStr = ConfigHelper.getConfig("IsHover");
            return bool.TryParse(hoverStr, out var enabled) && enabled;
        }

        /// <summary>悬停侧与卡片布局侧一致，左右只生效一边。</summary>
        private static CommentLayout GetCardSide()
        {
            var layoutStr = ConfigHelper.GetComponentLayout();
            if (string.IsNullOrEmpty(layoutStr))
                return CommentLayout.right;
            return Enum.TryParse<CommentLayout>(layoutStr, out var layout) ? layout : CommentLayout.right;
        }

        private void DisposeSentinel()
        {
            _enterTimer?.Stop();
            if (_sentinel == null)
                return;
            _sentinel.EdgeEnter -= OnEdgeEnter;
            _sentinel.EdgeLeave -= OnEdgeLeave;
            _sentinel.Close();
            _sentinel = null;
        }
    }

    public sealed class HoverConfigChangedMessage;
}
