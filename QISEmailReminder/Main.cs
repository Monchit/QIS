//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QISEmailReminder
{
    using System;
    using System.Collections.Generic;
    
    public partial class Main
    {
        public Main()
        {
            this.InspectionLot = new HashSet<InspectionLot>();
            this.QCReject = new HashSet<QCReject>();
        }
    
        public System.DateTime entry_date { get; set; }
        public int plant_id { get; set; }
        public Nullable<System.DateTime> issue_dt { get; set; }
    
        public virtual ICollection<InspectionLot> InspectionLot { get; set; }
        public virtual Master_Plant Master_Plant { get; set; }
        public virtual ICollection<QCReject> QCReject { get; set; }
    }
}
