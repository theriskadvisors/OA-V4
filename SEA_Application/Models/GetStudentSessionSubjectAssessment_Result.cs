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
    
    public partial class GetStudentSessionSubjectAssessment_Result
    {
        public int Id { get; set; }
        public Nullable<int> STAID { get; set; }
        public string Question { get; set; }
        public string Catageory { get; set; }
        public string FirstTermGrade { get; set; }
        public string SecondTermGrade { get; set; }
        public string ThirdTermGrade { get; set; }
        public Nullable<int> SessionId { get; set; }
        public string Answer { get; set; }
    }
}
