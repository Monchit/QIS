//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MvcQIS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Group
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string MrgEmpId { get; set; }
        public string DeptId { get; set; }
        public Nullable<System.DateTime> OracleLastUpdate { get; set; }
        public Nullable<System.DateTime> LastUpdate { get; set; }
        public Nullable<bool> AutoUpdate { get; set; }
        public Nullable<bool> IsObsolete { get; set; }
        public Nullable<System.DateTime> ObsoleteDate { get; set; }
    }
}
