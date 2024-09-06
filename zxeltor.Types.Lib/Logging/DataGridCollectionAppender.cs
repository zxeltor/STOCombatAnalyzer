using log4net.Appender;
using log4net.Core;

namespace zxeltor.Types.Lib.Logging
{
    public class DataGridCollectionAppender : AppenderSkeleton
    {
        public ICollection<DataGridRowContext>? LogGridRowDataCollection { get; set; }

        public DataGridCollectionAppender(string appenderName, ICollection<DataGridRowContext> collection)
        {
            this.Name = appenderName;
            this.LogGridRowDataCollection = collection;
            this.Threshold = Level.Debug;
        }

        #region Overrides of AppenderSkeleton

        /// <inheritdoc />
        protected override void Append(LoggingEvent loggingEvent)
        {
            this.LogGridRowDataCollection?.Add(new DataGridRowContext(loggingEvent.RenderedMessage));
        }

        #endregion
    }
}
