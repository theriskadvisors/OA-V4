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
    
    public partial class AspNetStudent_Assessment
    {
        public int Id { get; set; }
        public Nullable<int> AssessmentID { get; set; }
        public string StudentID { get; set; }
        public Nullable<int> MarksGot { get; set; }
        public Nullable<bool> SubmissionStatus { get; set; }
        public string SubmittedFileName { get; set; }
        public Nullable<System.DateTime> SubmissionDate { get; set; }
    
        public virtual AspNetAssessment AspNetAssessment { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
    }
}
