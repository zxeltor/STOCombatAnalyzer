// Copyright since “2018” by Avail Technologies INC.  All rights reserved.  
// Filename: ConfigDictionary.cs
// Description: Custom Dictionary with int and ConfigType enum indexers

using System.Collections.Generic;

using Availtec.MyAvail.Portable.Enums;

namespace Availtec.MyAvail.Portable.Config
{
    public class ConfigDictionary : Dictionary<int, object>
    {
        #region Public Indexers

        public new object this[int i] => base[i];

        public object this[ConfigType i] => base[(int)i];

        #endregion

        //public void Add(ConfigType key, dynamic value)
        //{
        //    base.Add((int)key, value);
        //}
    }
}