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
    
    public partial class GetStudentSubjects_Result
    {
        public int Id { get; set; }
        public string SubjectName { get; set; }
        public Nullable<int> ClassID { get; set; }
        public string TeacherID { get; set; }
        public string Status { get; set; }
        public Nullable<bool> IsManadatory { get; set; }
        public Nullable<int> Points { get; set; }
        public string CourseType { get; set; }
        public string SubjectGroup { get; set; }
        public string StudentID { get; set; }
    }
}
