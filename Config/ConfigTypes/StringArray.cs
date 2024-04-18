// Copyright since “2018” by Avail Technologies INC.  All rights reserved.  
// Filename: StringArray.cs
// Description:  Config data type to store string arrays

using System.Xml.Serialization;

namespace Availtec.MyAvail.Portable.Config.ConfigTypes
{
    [XmlRoot("Items")]
    public class StringArray
    {
        #region Public Properties

        [XmlElement("Item")]
        public string[] Items { get; set; }

        #endregion
    }
}