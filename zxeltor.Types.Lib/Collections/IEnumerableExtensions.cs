using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace zxeltor.Types.Lib.Collections
{
    public static class IEnumerableExtensions
    {
        public static SyncNotifyCollection<TSource> ToSyncNotifyCollection<TSource>(this IEnumerable<TSource> collection)
        {
            return new SyncNotifyCollection<TSource>(collection.ToList());
        }
    }
}
