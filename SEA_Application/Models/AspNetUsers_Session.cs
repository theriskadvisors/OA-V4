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
    
    public partial class AspNetUsers_Session
    {
        public int Id { get; set; }
        public string UserID { get; set; }
        public Nullable<int> SessionID { get; set; }
    
        public virtual AspNetSession AspNetSession { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
    }
}
