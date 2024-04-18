// Copyright since “2018” by Avail Technologies INC.  All rights reserved.  
// Filename: ConfigDeserializer.cs
// Description: Deserialize config values from XML

using System.IO;
using System.Xml.Serialization;

namespace Availtec.MyAvail.Portable.Config
{
    public static class ConfigDeserializer
    {
        #region Public Methods and Operators

        public static T Deserialize<T>(string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            return (T)xmlSerializer.Deserialize(new StringReader(xmlString));
        }

        #endregion
    }
}