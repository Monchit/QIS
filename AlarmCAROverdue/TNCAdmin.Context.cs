﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TNC_ADMINEntities : DbContext
    {
        public TNC_ADMINEntities()
            : base("name=TNC_ADMINEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<tnc_costcentercode> tnc_costcentercode { get; set; }
        public DbSet<tnc_dept_master> tnc_dept_master { get; set; }
        public DbSet<tnc_group_master> tnc_group_master { get; set; }
        public DbSet<tnc_plant_master> tnc_plant_master { get; set; }
        public DbSet<tnc_user> tnc_user { get; set; }
        public DbSet<V_Employee_Info> V_Employee_Info { get; set; }
        public DbSet<View_Organization> View_Organization { get; set; }
    }
}
