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
    
    public partial class TT_MAIL_WIP
    {
        public long ID { get; set; }
        public int ProgramID { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Title { get; set; }
        public string HTMLBody { get; set; }
        public short Flag { get; set; }
    }
}
