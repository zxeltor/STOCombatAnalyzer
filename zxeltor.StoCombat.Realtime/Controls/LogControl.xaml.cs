using log4net.Repository.Hierarchy;
using log4net;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using zxeltor.Types.Lib.Collections;
using zxeltor.Types.Lib.Result;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using zxeltor.Types.Lib.Helpers;
using zxeltor.Types.Lib.Logging;
using zxeltor.StoCombat.Realtime.Classes;


namespace zxeltor.StoCombat.Realtime.Controls
{
    /// <summary>
    /// Interaction logic for LogControl.xaml
    /// </summary>
    public partial class LogControl : UserControl
    {
        private static readonly ILog _log = log4net.LogManager.GetLogger(typeof(LogControl));

        public LogControlDataContext MyContext { get; }

        public LogControl()
        {
            InitializeComponent();
            DataContext = MyContext = new LogControlDataContext();

            AppNotificationManager.Instance.OnNotification += InstanceOnOnNotification;
        }
        ~LogControl()
        {
            AppNotificationManager.Instance.OnNotification -= InstanceOnOnNotification;
        }

        private void InstanceOnOnNotification(object? sender, DataGridRowContext e)
        {
            this.Dispatcher.Invoke( () => MyContext.LogGridRows.Add(e));
        }

        public void AddLog(string message, ResultLevel resultLevel = ResultLevel.Info)
        {
            this.MyContext.AddLogGridRow(new DataGridRowContext(message, resultLevel));
        }

        public void AddLog(string message, Exception exception, ResultLevel resultLevel = ResultLevel.Error)
        {
            this.MyContext.AddLogGridRow(new DataGridRowContext(message, exception, resultLevel));
        }

        public void AddLog(Exception exception, ResultLevel resultLevel = ResultLevel.Error)
        {
            this.MyContext.AddLogGridRow(new DataGridRowContext(exception, resultLevel));
        }

        private void UiButtonClearLog_OnClick(object sender, RoutedEventArgs e)
        {
            this.MyContext.Clear();
        }
    }

    public class LogControlDataContext : INotifyPropertyChanged
    {
        public void AddLogGridRow(DataGridRowContext context)
        {
            this.LogGridRows.Add(context);
        }

        public void Clear()
        {
            this.LogGridRows.Clear();
        }

        public SyncNotifyCollection<DataGridRowContext> LogGridRows { get; } = [];

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
