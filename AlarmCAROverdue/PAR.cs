//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AlarmCAROverdue
{
    using System;
    using System.Collections.Generic;
    
    public partial class PAR
    {
        public System.DateTime entry_date { get; set; }
        public int plant_id { get; set; }
        public int qc_reject_id { get; set; }
        public string report_no { get; set; }
        public string found { get; set; }
        public string root_cause { get; set; }
        public int responsible { get; set; }
        public int qty_product { get; set; }
        public int qty_reject { get; set; }
        public string ipqc { get; set; }
        public System.DateTime issued_date { get; set; }
        public System.DateTime reply_due_date { get; set; }
        public Nullable<System.DateTime> reply_date { get; set; }
        public Nullable<System.DateTime> respond_date { get; set; }
        public Nullable<System.DateTime> effective_date { get; set; }
        public string attach { get; set; }
        public Nullable<System.DateTime> issue_dt { get; set; }
    }
}
