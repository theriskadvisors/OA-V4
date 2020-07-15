﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class SEA_DatabaseEntities : DbContext
    {
        public SEA_DatabaseEntities()
            : base("name=SEA_DatabaseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Aspnet_Employee_Session> Aspnet_Employee_Session { get; set; }
        public virtual DbSet<AspNetAbsent_Student> AspNetAbsent_Student { get; set; }
        public virtual DbSet<AspNetAnnouncement> AspNetAnnouncements { get; set; }
        public virtual DbSet<AspNetAnnouncement_Subject> AspNetAnnouncement_Subject { get; set; }
        public virtual DbSet<AspNetAssessment> AspNetAssessments { get; set; }
        public virtual DbSet<AspNetAssessment_Question> AspNetAssessment_Question { get; set; }
        public virtual DbSet<AspNetAssessment_Questions_Category> AspNetAssessment_Questions_Category { get; set; }
        public virtual DbSet<AspNetAssessment_Topic> AspNetAssessment_Topic { get; set; }
        public virtual DbSet<AspNetAttendance> AspNetAttendances { get; set; }
        public virtual DbSet<AspNetCatalog> AspNetCatalogs { get; set; }
        public virtual DbSet<AspNetChapter> AspNetChapters { get; set; }
        public virtual DbSet<AspNetClass> AspNetClasses { get; set; }
        public virtual DbSet<AspNetClass_FeeType> AspNetClass_FeeType { get; set; }
        public virtual DbSet<AspnetComment_Head> AspnetComment_Head { get; set; }
        public virtual DbSet<AspnetComment> AspnetComments { get; set; }
        public virtual DbSet<AspNetCurriculum> AspNetCurriculums { get; set; }
        public virtual DbSet<AspNetDurationType> AspNetDurationTypes { get; set; }
        public virtual DbSet<AspNetEmp_Auto_Absent> AspNetEmp_Auto_Absent { get; set; }
        public virtual DbSet<AspNetEmp_Auto_Attendance> AspNetEmp_Auto_Attendance { get; set; }
        public virtual DbSet<AspNetEmployee> AspNetEmployees { get; set; }
        public virtual DbSet<AspNetEmployee_Attendance> AspNetEmployee_Attendance { get; set; }
        public virtual DbSet<AspNetEmployeeAttendance> AspNetEmployeeAttendances { get; set; }
        public virtual DbSet<AspNetFeeChallan> AspNetFeeChallans { get; set; }
        public virtual DbSet<AspNetFeedBackForm> AspNetFeedBackForms { get; set; }
        public virtual DbSet<AspNetFeeType> AspNetFeeTypes { get; set; }
        public virtual DbSet<AspNetFinanceLedger> AspNetFinanceLedgers { get; set; }
        public virtual DbSet<AspNetFinanceLedgerType> AspNetFinanceLedgerTypes { get; set; }
        public virtual DbSet<AspNetFinanceMonth> AspNetFinanceMonths { get; set; }
        public virtual DbSet<AspNetFinancePeriod> AspNetFinancePeriods { get; set; }
        public virtual DbSet<AspNetHoliday_Calendar_Notification> AspNetHoliday_Calendar_Notification { get; set; }
        public virtual DbSet<AspNetHomework> AspNetHomeworks { get; set; }
        public virtual DbSet<AspnetLesson> AspnetLessons { get; set; }
        public virtual DbSet<AspNetLessonPlan> AspNetLessonPlans { get; set; }
        public virtual DbSet<AspNetLessonPlan_Topic> AspNetLessonPlan_Topic { get; set; }
        public virtual DbSet<AspNetLessonPlanBreakdown> AspNetLessonPlanBreakdowns { get; set; }
        public virtual DbSet<AspNetLessonPlanBreakdownHeading> AspNetLessonPlanBreakdownHeadings { get; set; }
        public virtual DbSet<AspNetLog> AspNetLogs { get; set; }
        public virtual DbSet<AspNetLoginTime> AspNetLoginTimes { get; set; }
        public virtual DbSet<AspNetMessage_Receiver> AspNetMessage_Receiver { get; set; }
        public virtual DbSet<AspNetMessage> AspNetMessages { get; set; }
        public virtual DbSet<AspNetNote> AspNetNotes { get; set; }
        public virtual DbSet<AspNetNotesOrder> AspNetNotesOrders { get; set; }
        public virtual DbSet<AspNetNotification> AspNetNotifications { get; set; }
        public virtual DbSet<AspNetNotification_User> AspNetNotification_User { get; set; }
        public virtual DbSet<AspnetOption> AspnetOptions { get; set; }
        public virtual DbSet<AspNetOrder> AspNetOrders { get; set; }
        public virtual DbSet<AspNetParent> AspNetParents { get; set; }
        public virtual DbSet<AspNetParent_Child> AspNetParent_Child { get; set; }
        public virtual DbSet<AspNetParent_Notification> AspNetParent_Notification { get; set; }
        public virtual DbSet<AspNetParent_Session> AspNetParent_Session { get; set; }
        public virtual DbSet<AspNetParentTeacherMeeting> AspNetParentTeacherMeetings { get; set; }
        public virtual DbSet<AspNetProject> AspNetProjects { get; set; }
        public virtual DbSet<AspNetPTM_ParentFeedback> AspNetPTM_ParentFeedback { get; set; }
        public virtual DbSet<AspNetPTM_TeacherFeedback> AspNetPTM_TeacherFeedback { get; set; }
        public virtual DbSet<AspNetPTMAttendance> AspNetPTMAttendances { get; set; }
        public virtual DbSet<AspNetPTMFormRole> AspNetPTMFormRoles { get; set; }
        public virtual DbSet<AspNetPushNotification> AspNetPushNotifications { get; set; }
        public virtual DbSet<AspnetQuestion> AspnetQuestions { get; set; }
        public virtual DbSet<AspnetQuiz> AspnetQuizs { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetRoom> AspNetRooms { get; set; }
        public virtual DbSet<AspNetSalary> AspNetSalaries { get; set; }
        public virtual DbSet<AspNetSalaryDetail> AspNetSalaryDetails { get; set; }
        public virtual DbSet<AspNetSession> AspNetSessions { get; set; }
        public virtual DbSet<AspNetStudent> AspNetStudents { get; set; }
        public virtual DbSet<AspNetStudent_Announcement> AspNetStudent_Announcement { get; set; }
        public virtual DbSet<AspNetStudent_Assessment> AspNetStudent_Assessment { get; set; }
        public virtual DbSet<AspNetStudent_Attendance> AspNetStudent_Attendance { get; set; }
        public virtual DbSet<AspNetStudent_AutoAttendance> AspNetStudent_AutoAttendance { get; set; }
        public virtual DbSet<AspNetStudent_Fine> AspNetStudent_Fine { get; set; }
        public virtual DbSet<AspNetStudent_HomeWork> AspNetStudent_HomeWork { get; set; }
        public virtual DbSet<AspNetStudent_Notification> AspNetStudent_Notification { get; set; }
        public virtual DbSet<AspNetStudent_Payment> AspNetStudent_Payment { get; set; }
        public virtual DbSet<AspNetStudent_PaymentDetail> AspNetStudent_PaymentDetail { get; set; }
        public virtual DbSet<AspNetStudent_Project> AspNetStudent_Project { get; set; }
        public virtual DbSet<AspNetStudent_Session_class> AspNetStudent_Session_class { get; set; }
        public virtual DbSet<AspNetStudent_Subject> AspNetStudent_Subject { get; set; }
        public virtual DbSet<AspNetStudent_Term_Assessment> AspNetStudent_Term_Assessment { get; set; }
        public virtual DbSet<AspNetStudent_Term_Assessments_Answers> AspNetStudent_Term_Assessments_Answers { get; set; }
        public virtual DbSet<AspnetStudentAssignment> AspnetStudentAssignments { get; set; }
        public virtual DbSet<AspnetStudentAssignmentSubmission> AspnetStudentAssignmentSubmissions { get; set; }
        public virtual DbSet<AspnetStudentAttachment> AspnetStudentAttachments { get; set; }
        public virtual DbSet<AspnetStudentLink> AspnetStudentLinks { get; set; }
        public virtual DbSet<AspNetStudentPerformanceReport> AspNetStudentPerformanceReports { get; set; }
        public virtual DbSet<AspNetSubject_Catalog> AspNetSubject_Catalog { get; set; }
        public virtual DbSet<AspNetSubject_Homework> AspNetSubject_Homework { get; set; }
        public virtual DbSet<AspNetSubject> AspNetSubjects { get; set; }
        public virtual DbSet<AspnetSubjectTopic> AspnetSubjectTopics { get; set; }
        public virtual DbSet<AspNetTeacher> AspNetTeachers { get; set; }
        public virtual DbSet<AspNetTeacher_Session> AspNetTeacher_Session { get; set; }
        public virtual DbSet<AspNetTeacherSubject> AspNetTeacherSubjects { get; set; }
        public virtual DbSet<AspNetTerm> AspNetTerms { get; set; }
        public virtual DbSet<AspNetTime_Setting> AspNetTime_Setting { get; set; }
        public virtual DbSet<AspNetTimeslot> AspNetTimeslots { get; set; }
        public virtual DbSet<AspNetTimeTable> AspNetTimeTables { get; set; }
        public virtual DbSet<AspNetTopic> AspNetTopics { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<AspNetUsers_Session> AspNetUsers_Session { get; set; }
        public virtual DbSet<AspNetVirtualRole> AspNetVirtualRoles { get; set; }
        public virtual DbSet<Comment_Head> Comment_Head { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Employee_SalaryIncrement> Employee_SalaryIncrement { get; set; }
        public virtual DbSet<Employee_SalaryRecord> Employee_SalaryRecord { get; set; }
        public virtual DbSet<EmployeeAdvanceSalary> EmployeeAdvanceSalaries { get; set; }
        public virtual DbSet<EmployeePenalty> EmployeePenalties { get; set; }
        public virtual DbSet<EmployeePenaltyType> EmployeePenaltyTypes { get; set; }
        public virtual DbSet<EmployeeSalary> EmployeeSalaries { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<FeeDateSetting> FeeDateSettings { get; set; }
        public virtual DbSet<FeeDiscount> FeeDiscounts { get; set; }
        public virtual DbSet<GenericSubject> GenericSubjects { get; set; }
        public virtual DbSet<Ledger> Ledgers { get; set; }
        public virtual DbSet<LedgerGroup> LedgerGroups { get; set; }
        public virtual DbSet<LedgerHead> LedgerHeads { get; set; }
        public virtual DbSet<Lesson_Session> Lesson_Session { get; set; }
        public virtual DbSet<Month_Multiplier> Month_Multiplier { get; set; }
        public virtual DbSet<NonRecurringCharge> NonRecurringCharges { get; set; }
        public virtual DbSet<NonRecurringFeeMultiplier> NonRecurringFeeMultipliers { get; set; }
        public virtual DbSet<PenaltyFee> PenaltyFees { get; set; }
        public virtual DbSet<Quiz_Topic_Questions> Quiz_Topic_Questions { get; set; }
        public virtual DbSet<ruffdata> ruffdatas { get; set; }
        public virtual DbSet<Student_ChallanForm> Student_ChallanForm { get; set; }
        public virtual DbSet<Student_GenericSubjects> Student_GenericSubjects { get; set; }
        public virtual DbSet<Student_Quiz_Scoring> Student_Quiz_Scoring { get; set; }
        public virtual DbSet<StudentChallanForm> StudentChallanForms { get; set; }
        public virtual DbSet<StudentDiscount> StudentDiscounts { get; set; }
        public virtual DbSet<StudentFeeMonth> StudentFeeMonths { get; set; }
        public virtual DbSet<StudentLessonTracking> StudentLessonTrackings { get; set; }
        public virtual DbSet<StudentPenalty> StudentPenalties { get; set; }
        public virtual DbSet<StudentPenaltyType> StudentPenaltyTypes { get; set; }
        public virtual DbSet<StudentRecurringFee> StudentRecurringFees { get; set; }
        public virtual DbSet<Teacher_GenericSubjects> Teacher_GenericSubjects { get; set; }
        public virtual DbSet<TeacherPenaltyRecord> TeacherPenaltyRecords { get; set; }
        public virtual DbSet<ToDoList> ToDoLists { get; set; }
        public virtual DbSet<TokenAuthentication> TokenAuthentications { get; set; }
        public virtual DbSet<Voucher> Vouchers { get; set; }
        public virtual DbSet<VoucherRecord> VoucherRecords { get; set; }
        public virtual DbSet<AggregatedCounter> AggregatedCounters { get; set; }
        public virtual DbSet<Counter> Counters { get; set; }
        public virtual DbSet<Hash> Hashes { get; set; }
        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<JobParameter> JobParameters { get; set; }
        public virtual DbSet<JobQueue> JobQueues { get; set; }
        public virtual DbSet<List> Lists { get; set; }
        public virtual DbSet<Schema> Schemata { get; set; }
        public virtual DbSet<Server> Servers { get; set; }
        public virtual DbSet<Set> Sets { get; set; }
        public virtual DbSet<State> States { get; set; }
    
        public virtual ObjectResult<DefaulterStudents_Result> DefaulterStudents()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<DefaulterStudents_Result>("DefaulterStudents");
        }
    
        public virtual ObjectResult<GetAccountantListingData_Result> GetAccountantListingData(string sessionId)
        {
            var sessionIdParameter = sessionId != null ?
                new ObjectParameter("SessionId", sessionId) :
                new ObjectParameter("SessionId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAccountantListingData_Result>("GetAccountantListingData", sessionIdParameter);
        }
    
        public virtual ObjectResult<GetAllParents_Result> GetAllParents(string userID, string sessionID)
        {
            var userIDParameter = userID != null ?
                new ObjectParameter("UserID", userID) :
                new ObjectParameter("UserID", typeof(string));
    
            var sessionIDParameter = sessionID != null ?
                new ObjectParameter("SessionID", sessionID) :
                new ObjectParameter("SessionID", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllParents_Result>("GetAllParents", userIDParameter, sessionIDParameter);
        }
    
        public virtual ObjectResult<GetAllSubjects_Result> GetAllSubjects()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllSubjects_Result>("GetAllSubjects");
        }
    
        public virtual ObjectResult<GetAllTeachers_Result> GetAllTeachers()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAllTeachers_Result>("GetAllTeachers");
        }
    
        public virtual ObjectResult<GetPendingStudentsList_Result> GetPendingStudentsList(string sessionID)
        {
            var sessionIDParameter = sessionID != null ?
                new ObjectParameter("SessionID", sessionID) :
                new ObjectParameter("SessionID", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetPendingStudentsList_Result>("GetPendingStudentsList", sessionIDParameter);
        }
    
        public virtual ObjectResult<Nullable<int>> GetRollNumbers()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("GetRollNumbers");
        }
    
        public virtual ObjectResult<GetStudentBasicData_Result> GetStudentBasicData(string studentId)
        {
            var studentIdParameter = studentId != null ?
                new ObjectParameter("StudentId", studentId) :
                new ObjectParameter("StudentId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetStudentBasicData_Result>("GetStudentBasicData", studentIdParameter);
        }
    
        public virtual ObjectResult<GetStudentSessionSubjectAssessment_Result> GetStudentSessionSubjectAssessment(string studentId, Nullable<int> sessionId, Nullable<int> subjectId)
        {
            var studentIdParameter = studentId != null ?
                new ObjectParameter("StudentId", studentId) :
                new ObjectParameter("StudentId", typeof(string));
    
            var sessionIdParameter = sessionId.HasValue ?
                new ObjectParameter("SessionId", sessionId) :
                new ObjectParameter("SessionId", typeof(int));
    
            var subjectIdParameter = subjectId.HasValue ?
                new ObjectParameter("SubjectId", subjectId) :
                new ObjectParameter("SubjectId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetStudentSessionSubjectAssessment_Result>("GetStudentSessionSubjectAssessment", studentIdParameter, sessionIdParameter, subjectIdParameter);
        }
    
        public virtual ObjectResult<GetStudentSubjects_Result> GetStudentSubjects(string studentId)
        {
            var studentIdParameter = studentId != null ?
                new ObjectParameter("StudentId", studentId) :
                new ObjectParameter("StudentId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetStudentSubjects_Result>("GetStudentSubjects", studentIdParameter);
        }
    
        public virtual ObjectResult<GetStudentTermSubjectAssessment_Result> GetStudentTermSubjectAssessment(string studentId, Nullable<int> termId, Nullable<int> studentSubjectId)
        {
            var studentIdParameter = studentId != null ?
                new ObjectParameter("StudentId", studentId) :
                new ObjectParameter("StudentId", typeof(string));
    
            var termIdParameter = termId.HasValue ?
                new ObjectParameter("TermId", termId) :
                new ObjectParameter("TermId", typeof(int));
    
            var studentSubjectIdParameter = studentSubjectId.HasValue ?
                new ObjectParameter("StudentSubjectId", studentSubjectId) :
                new ObjectParameter("StudentSubjectId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetStudentTermSubjectAssessment_Result>("GetStudentTermSubjectAssessment", studentIdParameter, termIdParameter, studentSubjectIdParameter);
        }
    
        public virtual ObjectResult<GetSubjctAssessmentQuestions_Result> GetSubjctAssessmentQuestions(Nullable<int> subjectId)
        {
            var subjectIdParameter = subjectId.HasValue ?
                new ObjectParameter("SubjectId", subjectId) :
                new ObjectParameter("SubjectId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetSubjctAssessmentQuestions_Result>("GetSubjctAssessmentQuestions", subjectIdParameter);
        }
    
        public virtual ObjectResult<string> GetUserRoleById(string userID)
        {
            var userIDParameter = userID != null ?
                new ObjectParameter("UserID", userID) :
                new ObjectParameter("UserID", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("GetUserRoleById", userIDParameter);
        }
    
        public virtual int sp_MSforeach_worker(string command1, string replacechar, string command2, string command3, Nullable<int> worker_type)
        {
            var command1Parameter = command1 != null ?
                new ObjectParameter("command1", command1) :
                new ObjectParameter("command1", typeof(string));
    
            var replacecharParameter = replacechar != null ?
                new ObjectParameter("replacechar", replacechar) :
                new ObjectParameter("replacechar", typeof(string));
    
            var command2Parameter = command2 != null ?
                new ObjectParameter("command2", command2) :
                new ObjectParameter("command2", typeof(string));
    
            var command3Parameter = command3 != null ?
                new ObjectParameter("command3", command3) :
                new ObjectParameter("command3", typeof(string));
    
            var worker_typeParameter = worker_type.HasValue ?
                new ObjectParameter("worker_type", worker_type) :
                new ObjectParameter("worker_type", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_MSforeach_worker", command1Parameter, replacecharParameter, command2Parameter, command3Parameter, worker_typeParameter);
        }
    
        public virtual int sp_MSforeachtable(string command1, string replacechar, string command2, string command3, string whereand, string precommand, string postcommand)
        {
            var command1Parameter = command1 != null ?
                new ObjectParameter("command1", command1) :
                new ObjectParameter("command1", typeof(string));
    
            var replacecharParameter = replacechar != null ?
                new ObjectParameter("replacechar", replacechar) :
                new ObjectParameter("replacechar", typeof(string));
    
            var command2Parameter = command2 != null ?
                new ObjectParameter("command2", command2) :
                new ObjectParameter("command2", typeof(string));
    
            var command3Parameter = command3 != null ?
                new ObjectParameter("command3", command3) :
                new ObjectParameter("command3", typeof(string));
    
            var whereandParameter = whereand != null ?
                new ObjectParameter("whereand", whereand) :
                new ObjectParameter("whereand", typeof(string));
    
            var precommandParameter = precommand != null ?
                new ObjectParameter("precommand", precommand) :
                new ObjectParameter("precommand", typeof(string));
    
            var postcommandParameter = postcommand != null ?
                new ObjectParameter("postcommand", postcommand) :
                new ObjectParameter("postcommand", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_MSforeachtable", command1Parameter, replacecharParameter, command2Parameter, command3Parameter, whereandParameter, precommandParameter, postcommandParameter);
        }
    
        public virtual ObjectResult<StudentOrderDetails_Result> StudentOrderDetails()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<StudentOrderDetails_Result>("StudentOrderDetails");
        }
    
        public virtual ObjectResult<TodayDebitList_Result> TodayDebitList()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<TodayDebitList_Result>("TodayDebitList");
        }
    
        public virtual ObjectResult<TodayExpense_Result> TodayExpense()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<TodayExpense_Result>("TodayExpense");
        }
    
        public virtual ObjectResult<TodayExpenseList_Result> TodayExpenseList()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<TodayExpenseList_Result>("TodayExpenseList");
        }
    
        public virtual ObjectResult<TodayExpensesList_Result> TodayExpensesList()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<TodayExpensesList_Result>("TodayExpensesList");
        }
    
        public virtual ObjectResult<AllDefaulterStudents_Result> AllDefaulterStudents()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<AllDefaulterStudents_Result>("AllDefaulterStudents");
        }
    }
}