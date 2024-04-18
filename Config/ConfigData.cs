// Copyright since “2018” by Avail Technologies INC.  All rights reserved.  
// Filename: ConfigData.cs
// Description: “Purpose of class and code in this file including descriptions of important interfaces usage details, and data structures”

namespace Availtec.MyAvail.Portable.Config
{
    public class ConfigData
    {
        #region Constructors and Destructors

        public ConfigData()
        {
        }

        public ConfigData(string value, string defaultValue, string typeName)
        {
            this.Value = value;
            this.DefaultValue = defaultValue;
            this.TypeName = typeName;
        }

        public ConfigData(int id, string name, string description, string value, string defaultValue, string typeName, int? groupId = null, string securityFunctionUniqueName = null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.Value = value;
            this.DefaultValue = defaultValue;
            this.TypeName = typeName;
            this.GroupId = groupId;
            this.SecurityFunctionUniqueName = securityFunctionUniqueName;
        }

        #endregion

        #region Public Properties

        public string DefaultValue { get; set; }

        public string Description { get; set; }

        public int? GroupId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string SecurityFunctionUniqueName { get; set; }

        public string TypeName { get; set; }

        public string Value { get; set; }

        #endregion
    }
}