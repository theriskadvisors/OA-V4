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
    
    public partial class AspnetLesson
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AspnetLesson()
        {
            this.AspnetComment_Head = new HashSet<AspnetComment_Head>();
            this.AspnetQuestions = new HashSet<AspnetQuestion>();
            this.AspnetStudentAssignments = new HashSet<AspnetStudentAssignment>();
            this.AspnetStudentAssignmentSubmissions = new HashSet<AspnetStudentAssignmentSubmission>();
            this.AspnetStudentAttachments = new HashSet<AspnetStudentAttachment>();
            this.AspnetStudentLinks = new HashSet<AspnetStudentLink>();
            this.Comment_Head = new HashSet<Comment_Head>();
            this.Events = new HashSet<Event>();
            this.Lesson_Session = new HashSet<Lesson_Session>();
            this.StudentLessonTrackings = new HashSet<StudentLessonTracking>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Video_Url { get; set; }
        public string Description { get; set; }
        public Nullable<int> TopicId { get; set; }
        public Nullable<System.TimeSpan> Duration { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<decimal> DurationMinutes { get; set; }
        public string EncryptedID { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> OrderBy { get; set; }
        public string MeetingLink { get; set; }
        public string LessonIMG { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspnetComment_Head> AspnetComment_Head { get; set; }
        public virtual AspnetSubjectTopic AspnetSubjectTopic { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspnetQuestion> AspnetQuestions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspnetStudentAssignment> AspnetStudentAssignments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspnetStudentAssignmentSubmission> AspnetStudentAssignmentSubmissions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspnetStudentAttachment> AspnetStudentAttachments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspnetStudentLink> AspnetStudentLinks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comment_Head> Comment_Head { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Event> Events { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Lesson_Session> Lesson_Session { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StudentLessonTracking> StudentLessonTrackings { get; set; }
    }
}
