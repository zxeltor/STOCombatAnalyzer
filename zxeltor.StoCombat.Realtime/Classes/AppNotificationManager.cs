using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zxeltor.Types.Lib.Logging;
using zxeltor.Types.Lib.Result;

namespace zxeltor.StoCombat.Realtime.Classes
{
    public sealed class AppNotificationManager
    {
        private static AppNotificationManager instance = null;
        private static readonly object padlock = new object();

        public event EventHandler<DataGridRowContext> OnNotification; 

        private AppNotificationManager()
        {
        }

        public static AppNotificationManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new AppNotificationManager();
                    }
                    return instance;
                }
            }
        }

        public void SendNotification(object sender, string message, ResultLevel resultLevel = ResultLevel.Info)
        {
            if(OnNotification != null)
                OnNotification.Invoke(sender, new DataGridRowContext(message, resultLevel));
        }

        public void SendNotification(object sender, string message, Exception exception, ResultLevel resultLevel = ResultLevel.Error)
        {
            if (OnNotification != null)
                OnNotification.Invoke(sender, new DataGridRowContext(message, exception, resultLevel));
        }

        public void SendNotification(object sender, Exception exception, ResultLevel resultLevel = ResultLevel.Error)
        {
            if (OnNotification != null)
                OnNotification.Invoke(sender, new DataGridRowContext(exception, resultLevel));
        }
    }
}
