//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SEA_Application.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EmployeeSalary
    {
        public int Id { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public Nullable<decimal> Salary { get; set; }
        public string Months { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
    
        public virtual AspNetEmployee AspNetEmployee { get; set; }
    }
}
