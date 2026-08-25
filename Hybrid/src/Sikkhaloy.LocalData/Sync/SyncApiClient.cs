using System.Net.Http.Json;
using System.Text.Json;
using Sikkhaloy.Shared.Accounts;
using Sikkhaloy.Shared.Access;
using Sikkhaloy.Shared.Attendance;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Calendar;
using Sikkhaloy.Shared.Classes;
using Sikkhaloy.Shared.Employees;
using Sikkhaloy.Shared.Exam;
using Sikkhaloy.Shared.Institution;
using Sikkhaloy.Shared.Menu;
using Sikkhaloy.Shared.Routine;
using Sikkhaloy.Shared.Committee;
using Sikkhaloy.Shared.Invoice;
using Sikkhaloy.Shared.Authority;
using Sikkhaloy.Shared.Sms;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Subjects;
using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.LocalData.Sync;

public interface ISyncApiClient
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<bool> PingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PublicInstituteDto>> GetPublicInstitutesAsync(CancellationToken cancellationToken = default);
    Task<PublicStatsDto> GetPublicStatsAsync(CancellationToken cancellationToken = default);
    Task<PublicContactResult> SendPublicContactAsync(PublicContactRequest request, CancellationToken cancellationToken = default);
    Task<PushResponse> PushAsync(string accessToken, PushRequest request, CancellationToken cancellationToken = default);
    Task<PullResponse> PullAsync(string accessToken, long since, CancellationToken cancellationToken = default);
    Task<StudentIdCheckResult> CheckStudentIdAsync(string accessToken, string studentCode, int? exceptServerId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SchoolClassDto>> GetClassesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<ClassStructureDto> GetClassStructureAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EducationYearDto>> GetYearsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<EducationYearResult> CreateYearAsync(string accessToken, SaveEducationYearRequest request, CancellationToken cancellationToken = default);
    Task<EducationYearResult> UpdateYearAsync(string accessToken, int yearId, SaveEducationYearRequest request, CancellationToken cancellationToken = default);
    Task<EducationYearResult> DeleteYearAsync(string accessToken, int yearId, CancellationToken cancellationToken = default);
    Task<OfficeProfileDto> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AdminInfoDto?> GetAdminInfoAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<ProfileResult> SaveAdminInfoAsync(string accessToken, AdminInfoDto request, CancellationToken cancellationToken = default);
    Task<ProfileResult> ChangePasswordAsync(string accessToken, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<MenuTreeDto> GetMenuAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<LoginResponse> SwitchYearAsync(string accessToken, int educationYearId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubAdminDto>> GetSubAdminsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<PageAccessDto> GetPageAccessAsync(string accessToken, string userName, CancellationToken cancellationToken = default);
    Task<SavePageAccessResult> SavePageAccessAsync(string accessToken, SavePageAccessRequest request, CancellationToken cancellationToken = default);
    Task<CreateSubAdminResult> CreateSubAdminAsync(string accessToken, CreateSubAdminRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubAdminAccountDto>> GetSubAdminAccountsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<SubAdminStatusResult> SetSubAdminApprovedAsync(string accessToken, SetSubAdminApprovedRequest request, CancellationToken cancellationToken = default);
    Task<SubAdminStatusResult> UnlockSubAdminAsync(string accessToken, UnlockSubAdminRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubjectDto>> GetSubjectsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<SubjectResult> CreateSubjectAsync(string accessToken, SaveSubjectRequest request, CancellationToken cancellationToken = default);
    Task<SubjectResult> UpdateSubjectAsync(string accessToken, int subjectId, SaveSubjectRequest request, CancellationToken cancellationToken = default);
    Task<SubjectResult> DeleteSubjectAsync(string accessToken, int subjectId, CancellationToken cancellationToken = default);
    Task<SubjectResult> SaveSubjectSerialsAsync(string accessToken, SaveSubjectSerialsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClassSubjectRowDto>> GetClassSubjectsAsync(string accessToken, int classId, int groupId, CancellationToken cancellationToken = default);
    Task<SubjectAssignResult> SaveClassSubjectsAsync(string accessToken, SaveClassSubjectsRequest request, CancellationToken cancellationToken = default);
    Task<SubjectAssignResult> ClearClassSubjectsAsync(string accessToken, int classId, int groupId, CancellationToken cancellationToken = default);
    Task<InstitutionDto> GetInstitutionAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<InstitutionResult> SaveInstitutionAsync(string accessToken, InstitutionDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HolidayDto>> GetHolidaysAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<HolidayResult> AddWeeklyHolidaysAsync(string accessToken, WeeklyHolidayRequest request, CancellationToken cancellationToken = default);
    Task<HolidayResult> AddRangeHolidaysAsync(string accessToken, RangeHolidayRequest request, CancellationToken cancellationToken = default);
    Task<HolidayResult> AddHolidayAsync(string accessToken, SaveHolidayRequest request, CancellationToken cancellationToken = default);
    Task<HolidayResult> UpdateHolidayAsync(string accessToken, int holidayId, SaveHolidayRequest request, CancellationToken cancellationToken = default);
    Task<HolidayResult> DeleteHolidayAsync(string accessToken, int holidayId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeListDto>> GetEmployeesAsync(
        string accessToken, string? type, string? status, string? query, CancellationToken cancellationToken = default);
    Task<EmployeeResult> CreateTeacherAsync(string accessToken, CreateTeacherRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeResult> CreateStaffAsync(string accessToken, CreateStaffRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeResult> UpdateEmployeeAsync(string accessToken, int employeeId, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeResult> SetEmployeeStatusAsync(string accessToken, int employeeId, SetJobStatusRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeIdCardDto>> GetEmployeeIdCardsAsync(
        string accessToken, string? type, string? query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeacherAccountDto>> GetTeacherAccountsAsync(
        string accessToken, string? query, CancellationToken cancellationToken = default);
    Task<TeacherAccountResult> SetTeacherApprovedAsync(
        string accessToken, SetTeacherApprovedRequest request, CancellationToken cancellationToken = default);
    Task<TeacherAccountResult> UnlockTeacherAsync(
        string accessToken, UnlockTeacherRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeacherPickDto>> GetActiveTeachersAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TeacherSubjectRowDto>> GetTeacherSubjectsAsync(
        string accessToken, int teacherId, int classId, CancellationToken cancellationToken = default);
    Task<EmployeeResult> ToggleTeacherSubjectAsync(
        string accessToken, int teacherId, ToggleTeacherSubjectRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalaryNameDto>> GetSalaryNamesAsync(string accessToken, string kind, CancellationToken cancellationToken = default);
    Task<SalaryResult> CreateSalaryNameAsync(string accessToken, string kind, SaveSalaryNameRequest request, CancellationToken cancellationToken = default);
    Task<SalaryResult> UpdateSalaryNameAsync(string accessToken, string kind, int id, SaveSalaryNameRequest request, CancellationToken cancellationToken = default);
    Task<SalaryResult> DeleteSalaryNameAsync(string accessToken, string kind, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalaryAssignRowDto>> GetSalaryAssignAsync(
        string accessToken, string kind, int nameId, string? type, CancellationToken cancellationToken = default);
    Task<SalaryResult> SaveSalaryAssignAsync(
        string accessToken, string kind, SaveSalaryAssignRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayorderEmployeeDto>> GetPayorderEmployeesAsync(
        string accessToken, string? type, CancellationToken cancellationToken = default);
    Task<SalaryResult> AssignPayorderAsync(string accessToken, AssignPayorderRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalaryMonthDto>> GetSalaryMonthsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<SalaryResult> GenerateSalaryAsync(string accessToken, GenerateSalaryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlyPayorderDto>> GetMonthlyPayordersAsync(
        string accessToken, int payorderNameId, string monthName, string? type, CancellationToken cancellationToken = default);
    Task<SalaryResult> UpdateBonusFineAsync(string accessToken, UpdateBonusFineRequest request, CancellationToken cancellationToken = default);
    Task<SalaryResult> DeleteMonthlyPayorderAsync(string accessToken, int employeePayorderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountOptionDto>> GetSalaryAccountsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<SalaryResult> PaySalaryAsync(string accessToken, PaySalaryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaidRecordDto>> GetPaidRecordsAsync(
        string accessToken, int employeeId, int employeePayorderId, CancellationToken cancellationToken = default);
    Task<SalaryResult> DeletePaidRecordAsync(string accessToken, int recordId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaidDueRowDto>> GetPaidDueAsync(string accessToken, IReadOnlyList<int> payorderNameIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReAdmissionCandidateDto>> GetReAdmissionCandidatesAsync(
        string accessToken, int yearId, int classId, int sectionId, int groupId, int shiftId, CancellationToken cancellationToken = default);
    Task<ReAdmissionAssignDto> GetReAdmissionAssignAsync(
        string accessToken, int studentId, int fromYearId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReAdmissionSubjectDto>> GetReAdmissionSubjectsAsync(
        string accessToken, int classId, int groupId, CancellationToken cancellationToken = default);
    Task<ReAdmissionResult> FinishReAdmissionAsync(
        string accessToken, ReAdmissionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReAdmissionExamDto>> GetReAdmissionExamsAsync(
        string accessToken, int yearId, int classId, bool cumulative, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReAdmissionPositionDto>> GetReAdmissionPositionsAsync(
        string accessToken, int yearId, int classId, int examId, bool cumulative, bool sectionWise, CancellationToken cancellationToken = default);
    Task<BulkReAdmissionResult> FinishBulkReAdmissionAsync(
        string accessToken, BulkReAdmissionRequest request, CancellationToken cancellationToken = default);
    Task<StudentSignupListsDto> GetStudentSignupAsync(
        string accessToken, int classId, int groupId, int sectionId, int shiftId, string? studentId,
        CancellationToken cancellationToken = default);
    Task<StudentInfoResult> CreateStudentUsersAsync(
        string accessToken, CreateStudentUsersRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentAccountDto>> GetStudentAccountsAsync(
        string accessToken, int classId, int groupId, int sectionId, int shiftId, string? studentId,
        CancellationToken cancellationToken = default);
    Task<StudentAccountResult> SetStudentApprovedAsync(
        string accessToken, SetStudentApprovedRequest request, CancellationToken cancellationToken = default);
    Task<StudentAccountResult> UnlockStudentAsync(
        string accessToken, UnlockStudentRequest request, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> DeleteStudentAccountAsync(
        string accessToken, DeleteStudentAccountRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentIdCardDto>> GetStudentIdCardsAsync(
        string accessToken, int classId, int groupId, int sectionId, int shiftId, string? ids,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentPhotoDto>> GetStudentPhotosAsync(
        string accessToken, CancellationToken cancellationToken = default);
    Task<StudentReportDto> GetStudentReportAsync(
        string accessToken, string studentId, CancellationToken cancellationToken = default);
    Task<StudentPlacementDto?> GetStudentPlacementAsync(
        string accessToken, string studentId, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> SaveStudentPlacementAsync(
        string accessToken, SaveStudentPlacementRequest request, CancellationToken cancellationToken = default);
    Task<StudentSubjectsDto> GetStudentSubjectsAsync(
        string accessToken, string studentId, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> SaveStudentSubjectsAsync(
        string accessToken, SaveStudentSubjectsRequest request, CancellationToken cancellationToken = default);
    Task<StudentPlacementDto?> GetStudentCertificateAsync(
        string accessToken, string studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SmStudentRowDto>> GetSmStudentsAsync(
        string accessToken, int classId, int groupId, int sectionId, int shiftId, string? studentId, int? subjectId,
        CancellationToken cancellationToken = default);
    Task<StudentPlacementDto?> GetSmClassChangeStudentAsync(
        string accessToken, int studentId, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> ChangeClassAsync(
        string accessToken, ChangeClassRequest request, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> SaveSmPlacementAsync(
        string accessToken, BulkPlacementRequest request, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> SaveSmOneSubjectAsync(
        string accessToken, SaveOneSubjectRequest request, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> SaveSmClassSubjectsAsync(
        string accessToken, ReplaceClassSubjectsRequest request, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> SaveSmRollsAsync(
        string accessToken, SaveRollSeatRequest request, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> SaveStudentPhotoAsync(
        string accessToken, SaveStudentPhotoRequest request, CancellationToken cancellationToken = default);
    Task<TcStudentDto?> FindTcStudentAsync(
        string accessToken, string studentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TcStudentDto>> ListTcStudentsAsync(
        string accessToken, int classId, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> GiveTcAsync(
        string accessToken, GiveTcRequest request, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> ActivateTcAsync(
        string accessToken, ActivateTcRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NoticeDto>> GetNoticesAsync(
        string accessToken, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> SaveNoticeAsync(
        string accessToken, SaveNoticeRequest request, CancellationToken cancellationToken = default);
    Task<StudentInfoResult> DeleteNoticesAsync(
        string accessToken, DeleteNoticesRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceScheduleDto>> GetAttendanceSchedulesAsync(
        string accessToken, CancellationToken cancellationToken = default);
    Task<AttendanceResult> CreateAttendanceScheduleAsync(
        string accessToken, SaveScheduleRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceResult> RenameAttendanceScheduleAsync(
        string accessToken, int scheduleId, SaveScheduleRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceResult> DeleteAttendanceScheduleAsync(
        string accessToken, int scheduleId, CancellationToken cancellationToken = default);
    Task<AttendanceResult> SaveAttendanceScheduleDaysAsync(
        string accessToken, SaveScheduleDaysRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceSettingsDto> GetAttendanceSettingsAsync(
        string accessToken, CancellationToken cancellationToken = default);
    Task<AttendanceResult> SaveAttendanceSettingsAsync(
        string accessToken, AttendanceSettingsDto request, CancellationToken cancellationToken = default);
    Task<AttendanceDownloadResult> DownloadAttendanceAppAsync(
        string accessToken, CancellationToken cancellationToken = default);
    Task<AttendanceDownloadResult> DownloadAttendancePhotosAsync(
        string accessToken, CancellationToken cancellationToken = default);
    Task<AttendanceDownloadResult> DownloadAttendanceUsersAsync(
        string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentRfidRowDto>> GetStudentRfidAsync(
        string accessToken, int scheduleId, int classId, int groupId, int sectionId, int shiftId,
        CancellationToken cancellationToken = default);
    Task<AttendanceResult> SaveStudentRfidAsync(
        string accessToken, SaveStudentRfidRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeRfidRowDto>> GetEmployeeRfidAsync(
        string accessToken, int scheduleId, string? type, CancellationToken cancellationToken = default);
    Task<AttendanceResult> SaveEmployeeRfidAsync(
        string accessToken, SaveEmployeeRfidRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentManualRowDto>> GetStudentManualAsync(
        string accessToken, int scheduleId, int classId, int groupId, int sectionId, int shiftId, DateTime date,
        CancellationToken cancellationToken = default);
    Task<AttendanceResult> SaveStudentManualAsync(
        string accessToken, SaveStudentManualRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeManualRowDto>> GetEmployeeManualAsync(
        string accessToken, int scheduleId, string? type, DateTime date, CancellationToken cancellationToken = default);
    Task<AttendanceResult> SaveEmployeeManualAsync(
        string accessToken, SaveEmployeeManualRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentAttendanceRecordDto>> GetStudentAttendanceRecordsAsync(
        string accessToken, string? status, int classId, int groupId, int sectionId, int shiftId, int scheduleId,
        DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentAttendanceSummaryDto>> GetStudentAttendanceSummaryAsync(
        string accessToken, int classId, int groupId, int sectionId, int shiftId, int scheduleId,
        DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeAttendanceRecordDto>> GetEmployeeAttendanceRecordsAsync(
        string accessToken, string? type, string? status, int scheduleId, int employeeId, DateTime from, DateTime to,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeAttendanceSummaryDto>> GetEmployeeAttendanceSummaryAsync(
        string accessToken, string? type, int scheduleId, int employeeId, DateTime from, DateTime to,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetAttendanceLeaveTypesAsync(
        string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceLeaveTypeDto>> GetAttendanceLeaveTypeRowsAsync(
        string accessToken, CancellationToken cancellationToken = default);
    Task<AttendanceResult> AddAttendanceLeaveTypeAsync(
        string accessToken, SaveLeaveTypeRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceResult> DeleteAttendanceLeaveTypeAsync(
        string accessToken, int leaveTypeId, CancellationToken cancellationToken = default);
    Task<StudentLeavePersonDto?> FindStudentLeaveAsync(
        string accessToken, string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentLeaveSuggestDto>> SuggestStudentLeaveAsync(
        string accessToken, string query, CancellationToken cancellationToken = default);
    Task<StudentLeavePrintDto?> GetStudentLeavePrintAsync(
        string accessToken, int leaveId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentLeaveRowDto>> GetStudentLeavesAsync(
        string accessToken, int studentId, CancellationToken cancellationToken = default);
    Task<AttendanceResult> SaveStudentLeaveAsync(
        string accessToken, SaveStudentLeaveRequest request, CancellationToken cancellationToken = default);
    Task<AttendanceResult> DeleteStudentLeaveAsync(
        string accessToken, int leaveId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeLeavePickDto>> GetEmployeeLeavePicksAsync(
        string accessToken, string? type, string? query, CancellationToken cancellationToken = default);
    Task<AttendanceResult> SaveEmployeeLeaveAsync(
        string accessToken, SaveEmployeeLeaveRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaveReportRowDto>> GetLeaveReportAsync(
        string accessToken, string? type, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceMonthDto>> GetAttendanceFineMonthsAsync(
        string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceFineRowDto>> GenerateAttendanceFineAsync(
        string accessToken, GenerateFineRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentRoleDto>> GetPaymentRolesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AccountsResult> CreatePaymentRoleAsync(string accessToken, SavePaymentRoleRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> UpdatePaymentRoleAsync(string accessToken, int id, SavePaymentRoleRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> DeletePaymentRoleAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AssignedRoleDto>> GetAssignedRolesAsync(string accessToken, int classId, int roleId, CancellationToken cancellationToken = default);
    Task<AccountsResult> AssignPaymentRoleAsync(string accessToken, SaveAssignedRoleRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> BulkAssignRolesAsync(string accessToken, BulkAssignRoleRequest request, CancellationToken cancellationToken = default);
    Task<AssignableRolesDto> GetAssignableRolesAsync(string accessToken, IReadOnlyList<int> classIds, CancellationToken cancellationToken = default);
    Task<AccountsResult> UpdateAssignedRoleAsync(string accessToken, UpdateAssignedRoleRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> DeleteAssignedRoleAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PayOrderStudentDto>> GetPayOrderStudentsAsync(string accessToken, int classId, CancellationToken cancellationToken = default);
    Task<AccountsResult> CreatePayOrdersAsync(string accessToken, CreatePayOrdersRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UnpaidPayOrderDto>> GetUnpaidPayOrdersAsync(string accessToken, int classId, int roleId, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<AccountsResult> RemovePayOrdersAsync(string accessToken, RemovePayOrderRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> ChangePayOrderDateAsync(string accessToken, ChangePayOrderDateRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashAccountDto>> GetCashAccountsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AccountsResult> CreateCashAccountAsync(string accessToken, SaveCashAccountRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> SetDefaultCashAccountAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<AccountsResult> DepositCashAsync(string accessToken, AccountMoveRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> WithdrawCashAsync(string accessToken, AccountMoveRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountMoveDto>> GetCashDepositsAsync(string accessToken, int accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountMoveDto>> GetCashWithdrawsAsync(string accessToken, int accountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeeSuggestDto>> SuggestFeeStudentsAsync(string accessToken, string query, CancellationToken cancellationToken = default);
    Task<FeeStudentBundleDto> GetFeeStudentBundleAsync(string accessToken, string id, CancellationToken cancellationToken = default);
    Task<AccountsResult> CollectPaymentAsync(string accessToken, CollectPaymentRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> AddMorePayOrderAsync(string accessToken, AddMorePayOrderRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> SaveConcessionAsync(string accessToken, SaveConcessionRequest request, CancellationToken cancellationToken = default);
    Task<ReceiptDetailDto?> GetMoneyReceiptAsync(string accessToken, string receiptNo, CancellationToken cancellationToken = default);
    Task<AccountsResult> UpdatePrintedReceiptAsync(string accessToken, PrintedReceiptRequest request, CancellationToken cancellationToken = default);
    Task<PaymentSmsSettingDto> GetPaymentSmsSettingAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AccountsResult> SavePaymentSmsSettingAsync(string accessToken, bool active, CancellationToken cancellationToken = default);
    Task<AccountsResult> SendReceiptSmsAsync(string accessToken, int moneyReceiptId, CancellationToken cancellationToken = default);
    Task<AccountsResult> UnpaidMoneyReceiptAsync(string accessToken, int moneyReceiptId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExtraIncomeCategoryDto>> GetExtraIncomeCategoriesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AccountsResult> CreateExtraIncomeCategoryAsync(string accessToken, string name, CancellationToken cancellationToken = default);
    Task<AccountsResult> UpdateExtraIncomeCategoryAsync(string accessToken, int id, string name, CancellationToken cancellationToken = default);
    Task<AccountsResult> DeleteExtraIncomeCategoryAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<ExtraIncomeListDto> GetExtraIncomeAsync(string accessToken, int categoryId, DateTime? from, DateTime? to, string? receiptNo = null, CancellationToken cancellationToken = default);
    Task<AccountsResult> CreateExtraIncomeAsync(string accessToken, SaveExtraIncomeRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> UpdateExtraIncomeAsync(string accessToken, SaveExtraIncomeRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> DeleteExtraIncomeAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<ExtraIncomeDto?> GetExtraIncomeOneAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseCategoryDto>> GetExpenseCategoriesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AccountsResult> CreateExpenseCategoryAsync(string accessToken, string name, CancellationToken cancellationToken = default);
    Task<AccountsResult> UpdateExpenseCategoryAsync(string accessToken, int id, string name, CancellationToken cancellationToken = default);
    Task<AccountsResult> DeleteExpenseCategoryAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseSubCategoryDto>> GetExpenseSubCategoriesAsync(string accessToken, int categoryId, CancellationToken cancellationToken = default);
    Task<AccountsResult> CreateExpenseSubCategoryAsync(string accessToken, int categoryId, string name, CancellationToken cancellationToken = default);
    Task<AccountsResult> UpdateExpenseSubCategoryAsync(string accessToken, int id, string name, CancellationToken cancellationToken = default);
    Task<AccountsResult> DeleteExpenseSubCategoryAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<ExpenseListDto> GetExpenseAsync(string accessToken, int categoryId, int subCategoryId, DateTime? from, DateTime? to, string? receiptNo = null, CancellationToken cancellationToken = default);
    Task<AccountsResult> CreateExpenseAsync(string accessToken, SaveExpenseRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> UpdateExpenseAsync(string accessToken, SaveExpenseRequest request, CancellationToken cancellationToken = default);
    Task<AccountsResult> DeleteExpenseAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<ExpenseDto?> GetExpenseOneAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<AccountsSummaryDto> GetAccountsSummaryAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<MonthBasedDto> GetMonthBasedReportAsync(string accessToken, DateTime? from, DateTime? to, int classId, string? roleIds, string? sectionId = null, bool students = false, bool money = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NameAmountDto>> GetMonthBasedRolesAsync(string accessToken, int classId, CancellationToken cancellationToken = default);
    Task<IncomeExpenseReportDto> GetIncomeReportAsync(string accessToken, DateTime? from, DateTime? to, string? category, CancellationToken cancellationToken = default);
    Task<IncomeExpenseReportDto> GetExpenseReportAsync(string accessToken, DateTime? from, DateTime? to, string? category, CancellationToken cancellationToken = default);
    Task<NetReportDto> GetNetReportAsync(string accessToken, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<CurrentDueDto> GetCurrentDueAsync(string accessToken, int classId, string? sectionId, string? roleId, string? id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NameAmountDto>> GetDueRolesAsync(string accessToken, int classId, CancellationToken cancellationToken = default);
    Task<CurrentDueStudentDetailDto> GetDueDetailsAsync(string accessToken, string id, string? roleId, CancellationToken cancellationToken = default);
    Task<AccountsResult> SendDueSmsAsync(string accessToken, DueSmsRequest request, CancellationToken cancellationToken = default);
    Task<PayorderReportDto> GetPayorderReportAsync(string accessToken, DateTime? from, DateTime? to, int roleId, CancellationToken cancellationToken = default);
    Task<PaidDetailsDto> GetPaidDetailsAsync(string accessToken, string? yearId, int classId, string? groupId, string? sectionId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<MyAccountsDto> GetMyAccountsAsync(string accessToken, int regId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountDetailDto>> GetAccountDetailsAsync(string accessToken, string? accountId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<AccountsLogDto> GetAccountsLogAsync(string accessToken, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NameAmountDto>> GetReportIncomeCategoriesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NameAmountDto>> GetReportExpenseCategoriesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NameAmountDto>> GetReportSectionsAsync(string accessToken, int classId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NameAmountDto>> GetReportGroupsAsync(string accessToken, int classId, CancellationToken cancellationToken = default);
    Task<SessionFilterDto> GetSessionFiltersAsync(string accessToken, int yearId, int classId, string? roleId, string? kind, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<SessionClassReportDto> GetSessionClassReportAsync(string accessToken, int yearId, DateTime? from, DateTime? to, int classId, int roleId, CancellationToken cancellationToken = default);
    Task<SessionStudentReportDto> GetSessionStudentsAsync(string accessToken, int yearId, int classId, string? sectionId, string? roleId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<SessionStudentReportDto> GetSessionPaidAsync(string accessToken, int yearId, int classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<SessionStudentReportDto> GetSessionDueAsync(string accessToken, int yearId, int classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<SessionStudentReportDto> GetSessionConcessionAsync(string accessToken, int yearId, int classId, string? sectionId, string? roleId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<SessionPaidDueDto> GetSessionPaidDueAsync(string accessToken, string? status, string? classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);

    Task<DashboardOverviewDto> GetDashboardOverviewAsync(string accessToken, CancellationToken cancellationToken = default);

    Task<ExamFilterDto> GetExamFiltersAsync(string accessToken, string? kind, int classId = 0, int examId = 0, string? groupId = null, string? sectionId = null, string? shiftId = null, int subjectId = 0, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamNameDto>> GetExamNamesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<ExamResult> CreateExamNameAsync(string accessToken, SaveExamNameRequest request, CancellationToken cancellationToken = default);
    Task<ExamResult> UpdateExamNameAsync(string accessToken, int examId, SaveExamNameRequest request, CancellationToken cancellationToken = default);
    Task<ExamResult> DeleteExamNameAsync(string accessToken, int examId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubExamDto>> GetSubExamsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<ExamResult> CreateSubExamAsync(string accessToken, SaveSubExamRequest request, CancellationToken cancellationToken = default);
    Task<ExamResult> UpdateSubExamAsync(string accessToken, int id, SaveSubExamRequest request, CancellationToken cancellationToken = default);
    Task<ExamResult> DeleteSubExamAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GradeSystemDto>> GetExamGradingAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<ExamResult> CreateExamGradingAsync(string accessToken, SaveGradeSystemRequest request, CancellationToken cancellationToken = default);
    Task<ExamResult> RenameExamGradingAsync(string accessToken, int id, SaveGradeSystemRequest request, CancellationToken cancellationToken = default);
    Task<ExamResult> UpdateExamGradeCommentAsync(string accessToken, int gradingId, SaveGradeCommentRequest request, CancellationToken cancellationToken = default);
    Task<ExamResult> DeleteExamGradingAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PassMarkRowDto>> GetExamPassMarksAsync(string accessToken, int classId, int examId, int subExamId, CancellationToken cancellationToken = default);
    Task<ExamResult> SaveExamPassMarksAsync(string accessToken, SavePassMarksRequest request, CancellationToken cancellationToken = default);
    Task<DistSheetDto> GetExamDistributionAsync(string accessToken, int classId, int examId, CancellationToken cancellationToken = default);
    Task<ExamResult> SaveExamDistributionAsync(string accessToken, SaveDistributionRequest request, CancellationToken cancellationToken = default);
    Task<ExamResult> CopyExamDistributionAsync(string accessToken, CopyDistributionRequest request, CancellationToken cancellationToken = default);
    Task<CollectPaperDto> GetExamCollectPaperAsync(string accessToken, int examId, int classId, int subjectId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken = default);
    Task<InputSheetDto> GetExamInputSheetAsync(string accessToken, int examId, int classId, int subjectId, int subExamId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken = default);
    Task<ExamResult> SaveExamInputMarksAsync(string accessToken, SaveInputMarksRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MarksCheckRowDto>> GetExamMarksCheckAsync(string accessToken, int classId, int examId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamControlRowDto>> GetExamControlAsync(string accessToken, int examId, bool cumulative, CancellationToken cancellationToken = default);
    Task<ExamResult> SaveExamControlAsync(string accessToken, SaveExamControlRequest request, CancellationToken cancellationToken = default);
    Task<ExamPublishSettingDto> GetExamPublishSettingAsync(string accessToken, int classId, int examId, CancellationToken cancellationToken = default);
    Task<ExamResult> PublishExamResultAsync(string accessToken, ExamPublishRequest request, CancellationToken cancellationToken = default);
    Task<ExamResult> DeleteExamResultAsync(string accessToken, ExamDeleteResultRequest request, CancellationToken cancellationToken = default);
    Task<ExamMeritListDto> GetExamMeritAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? passStatus, CancellationToken cancellationToken = default);
    Task<ExamMeritListDto> GetExamMeritSubjectAsync(string accessToken, int classId, int examId, int subjectId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken = default);
    Task<ExamResultCardSheetDto> GetExamResultCardsAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, CancellationToken cancellationToken = default);
    Task<ExamAnalyticalDto> GetExamAnalyticalAsync(string accessToken, int classId, int examId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamOptionDto>> GetCumulativeExamNamesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<ExamResult> CreateCumulativeExamNameAsync(string accessToken, SaveCumulativeNameRequest request, CancellationToken cancellationToken = default);
    Task<ExamResult> UpdateCumulativeExamNameAsync(string accessToken, int id, SaveCumulativeNameRequest request, CancellationToken cancellationToken = default);
    Task<CumulativePublishSettingDto> GetCumulativePublishSettingAsync(string accessToken, int classId, int examId, CancellationToken cancellationToken = default);
    Task<ExamResult> PublishCumulativeResultAsync(string accessToken, CumulativePublishRequest request, CancellationToken cancellationToken = default);
    Task<ExamMeritListDto> GetCumulativeMeritAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken = default);
    Task<CumulativeResultCardSheetDto> GetCumulativeResultCardsAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, CancellationToken cancellationToken = default);
    Task<ExamSeatPlanSheetDto> GetExamSeatPlanAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, string? classIds = null, CancellationToken cancellationToken = default);
    Task<ExamResult> RandomizeExamSeatsAsync(string accessToken, RandomSeatRequest request, CancellationToken cancellationToken = default);
    Task<ExamAdmitCardSheetDto> GetExamAdmitCardsAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, string? paymentStatus, CancellationToken cancellationToken = default);
    Task<ExamResult> SaveExamAdmitSignAsync(string accessToken, SaveExamSignRequest request, CancellationToken cancellationToken = default);

    Task<SmsBalanceDto> GetSmsBalanceAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SmsStudentDto>> GetSmsStudentsAsync(string accessToken, int classId, int groupId, int sectionId, int shiftId, string? ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SmsTeacherDto>> GetSmsTeachersAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<SmsResult> SendOfficeSmsAsync(string accessToken, SendOfficeSmsRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SmsGroupDto>> GetSmsGroupsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<SmsResult> SaveSmsGroupAsync(string accessToken, SaveSmsGroupRequest request, CancellationToken cancellationToken = default);
    Task<SmsResult> DeleteSmsGroupAsync(string accessToken, int groupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SmsContactDto>> GetSmsContactsAsync(string accessToken, int groupId, string? search, CancellationToken cancellationToken = default);
    Task<SmsResult> SaveSmsContactAsync(string accessToken, SaveSmsContactRequest request, CancellationToken cancellationToken = default);
    Task<SmsResult> DeleteSmsContactAsync(string accessToken, int numberId, CancellationToken cancellationToken = default);
    Task<SmsRecordsDto> GetSmsRecordsAsync(string accessToken, DateTime? from, DateTime? to, string? search, CancellationToken cancellationToken = default);
    Task<SmsRechargePageDto> GetSmsRechargeAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<SmsResult> StartSmsRechargeAsync(string accessToken, SmsRechargeRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoutineNameDto>> GetRoutineNamesAsync(string accessToken, bool unusedOnly, CancellationToken cancellationToken = default);
    Task<RoutineResult> SaveRoutineNameAsync(string accessToken, SaveRoutineNameRequest request, CancellationToken cancellationToken = default);
    Task<RoutineResult> DeleteRoutineNameAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<RoutineResult> CreateClassRoutineAsync(string accessToken, CreateClassRoutineRequest request, CancellationToken cancellationToken = default);
    Task<ClassRoutineSheetDto> GetRoutineAssignAsync(string accessToken, int classId, int groupId, int sectionId, int shiftId, int routineInfoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoutineOptionDto>> GetRoutineTeachersAsync(string accessToken, int classId, int subjectId, string day, string start, string end, int exceptRoutineInfoId, CancellationToken cancellationToken = default);
    Task<RoutineResult> AssignClassRoutineAsync(string accessToken, AssignRoutineRequest request, CancellationToken cancellationToken = default);
    Task<ClassRoutineSheetDto> GetRoutineViewAsync(string accessToken, int classId, int groupId, int sectionId, int shiftId, int routineInfoId, bool edit, CancellationToken cancellationToken = default);
    Task<RoutineResult> UpdateClassRoutineAsync(string accessToken, AssignRoutineRequest request, CancellationToken cancellationToken = default);
    Task<RoutineResult> DeleteClassRoutineAsync(string accessToken, AssignRoutineRequest request, CancellationToken cancellationToken = default);
    Task<ExamRoutineSheetDto> GetExamRoutineAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoutineOptionDto>> GetExamRoutineSubjectsAsync(string accessToken, int classId, CancellationToken cancellationToken = default);
    Task<RoutineResult> SaveExamRoutineAsync(string accessToken, SaveExamRoutineRequest request, CancellationToken cancellationToken = default);
    Task<RoutineResult> DeleteExamRoutineAsync(string accessToken, int id, CancellationToken cancellationToken = default);

    Task<CommitteeLookupsDto> GetCommitteeLookupsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommitteeMemberTypeDto>> GetCommitteeTypesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<CommitteeResult> SaveCommitteeTypeAsync(string accessToken, SaveCommitteeMemberTypeRequest request, CancellationToken cancellationToken = default);
    Task<CommitteeResult> DeleteCommitteeTypeAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommitteeMemberDto>> GetCommitteeMembersAsync(string accessToken, int typeId, string? q, CancellationToken cancellationToken = default);
    Task<CommitteeResult> SaveCommitteeMemberAsync(string accessToken, SaveCommitteeMemberRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DonationCategoryDto>> GetDonationCategoriesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<CommitteeResult> SaveDonationCategoryAsync(string accessToken, SaveDonationCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CommitteeResult> DeleteDonationCategoryAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DonorSuggestDto>> SuggestDonorsAsync(string accessToken, string? q, CancellationToken cancellationToken = default);
    Task<CommitteeResult> AddDonationAsync(string accessToken, AddDonationRequest request, CancellationToken cancellationToken = default);
    Task<DonationListDto> GetDonationsAsync(string accessToken, int memberId, int categoryId, string? paid, CancellationToken cancellationToken = default);
    Task<CommitteeResult> UpdateDonationAsync(string accessToken, UpdateDonationRequest request, CancellationToken cancellationToken = default);
    Task<CommitteeResult> DeleteDonationAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<CollectPageDto> GetCollectDonationAsync(string accessToken, int memberId, CancellationToken cancellationToken = default);
    Task<CommitteeResult> CollectDonationAsync(string accessToken, CollectDonationRequest request, CancellationToken cancellationToken = default);
    Task<PaymentRecordListDto> GetCommitteePaymentsAsync(string accessToken, int yearId, int categoryId, int memberId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<UnpaidReceiptDto> GetUnpaidReceiptAsync(string accessToken, string? sn, CancellationToken cancellationToken = default);
    Task<CommitteeResult> UnpaidReceiptAsync(string accessToken, string sn, CancellationToken cancellationToken = default);
    Task<DonationReceiptDto> GetDonationReceiptAsync(string accessToken, int id, CancellationToken cancellationToken = default);

    Task<DueInvoiceDto> GetDueInvoiceAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<SubscriptionStatusDto> GetSubscriptionStatusAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<InvoiceResult> PayDueInvoiceAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<PaidInvoiceListDto> GetPaidInvoicesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<PaidInvoiceReceiptDto> GetPaidInvoiceReceiptAsync(string accessToken, int id, CancellationToken cancellationToken = default);

    Task<AuthorityDashboardDto> GetAuthorityDashboardAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthorityDashboardDto> GetAuthorityInstitutionsAsync(
        string accessToken, string? q, string? validation, string? live, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<LoginResponse> EnterAuthoritySchoolAsync(string accessToken, int schoolId, int educationYearId = 0, CancellationToken cancellationToken = default);
    Task<InstitutionDetailsDto> GetAuthorityInstitutionDetailsAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SaveAuthorityInstitutionYearsAsync(string accessToken, SaveInstitutionYearsRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> RechargeAuthorityInstitutionSmsAsync(string accessToken, InstSmsRechargeRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SaveAuthorityDueNoticeAsync(string accessToken, InstDueNoticeRequest request, CancellationToken cancellationToken = default);
    Task<InstStudentFindDto> FindAuthorityInstitutionStudentAsync(string accessToken, int schoolId, string id, CancellationToken cancellationToken = default);
    Task<AuthorityResult> DeleteAuthorityInstitutionStudentAsync(string accessToken, InstIdRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> ChangeAuthorityInstitutionStudentIdAsync(string accessToken, InstChangeIdRequest request, CancellationToken cancellationToken = default);
    Task<InstReceiptDto> FindAuthorityInstitutionReceiptAsync(string accessToken, int schoolId, string sn, CancellationToken cancellationToken = default);
    Task<AuthorityResult> DeleteAuthorityInstitutionReceiptAsync(string accessToken, InstReceiptRequest request, CancellationToken cancellationToken = default);

    Task<SignupLookupsDto> GetAuthoritySignupLookupsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthorityResult> CreateAuthorityUserAsync(string accessToken, SignupUserRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> CreateAuthorityInstitutionAsync(string accessToken, SignupInstitutionRequest request, CancellationToken cancellationToken = default);
    Task<UserInfoListDto> GetAuthorityUserInfoAsync(string accessToken, string? q, string? validation, string? password, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SchoolUserDto>> GetAuthoritySchoolUsersAsync(string accessToken, int schoolId, string? category, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SetAuthorityApprovedAsync(string accessToken, SetApprovedRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> UnlockAuthorityUserAsync(string accessToken, UnlockUserRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TestimonialRowDto>> GetAuthorityTestimonialsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SaveAuthorityTestimonialAsync(string accessToken, SaveTestimonialRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SetAuthorityTestimonialShowAsync(string accessToken, SetTestimonialShowRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResetSchoolOptionDto>> GetAuthorityResetSchoolsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResetYearOptionDto>> GetAuthorityResetYearsAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default);
    Task<ResetPreviewDto> PreviewAuthorityResetAsync(string accessToken, int schoolId, string mode, int educationYearId, CancellationToken cancellationToken = default);
    Task<ResetPreviewDto> PreviewAuthorityResetImagesAsync(string accessToken, int schoolId, IReadOnlyList<int> yearIds, CancellationToken cancellationToken = default);
    Task<ResetProgressDto> GetAuthorityResetProgressAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default);
    Task<AuthorityResult> StartAuthorityResetAsync(string accessToken, ResetExecuteRequest request, CancellationToken cancellationToken = default);
    Task<ResetPreviewDto> DeleteAuthorityResetImagesAsync(string accessToken, ResetImageRequest request, CancellationToken cancellationToken = default);
    Task<AttSignupPageDto> GetAuthorityAttendanceAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthorityResult> RegisterAuthorityAttendanceAsync(string accessToken, AttRegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SetAuthorityAttendancePasswordAsync(string accessToken, AttPasswordRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SetAuthorityAttendanceActiveAsync(string accessToken, AttActiveRequest request, CancellationToken cancellationToken = default);
    Task<SmsSettingPageDto> GetAuthoritySmsSettingAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SaveAuthoritySmsSettingAsync(string accessToken, SaveSmsSettingRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SmsSenderRowDto>> GetAuthoritySmsRecordsAsync(string accessToken, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<SmsFailedPageDto> GetAuthorityFailedSmsAsync(string accessToken, DateTime? from, DateTime? to, string? reason, int schoolId, CancellationToken cancellationToken = default);
    Task<ClientSmsPageDto> GetAuthorityClientSmsAsync(string accessToken, string? q, string? validation, CancellationToken cancellationToken = default);
    Task<SendClientSmsResult> SendAuthorityClientSmsAsync(string accessToken, SendClientSmsRequest request, CancellationToken cancellationToken = default);
    Task<AuthAccountsPageDto> GetAuthorityAccountsAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthProgressPageDto> GetAuthorityProgressAsync(string accessToken, string? filter, CancellationToken cancellationToken = default);
    Task<AuthCollectPageDto> GetAuthorityCollectionAsync(string accessToken, int categoryId, string? month, CancellationToken cancellationToken = default);
    Task<AuthManagePageDto> GetAuthorityManageAsync(string accessToken, string? q, string? validation, string? payment, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SaveAuthorityManageAsync(string accessToken, AuthManageSaveRequest request, CancellationToken cancellationToken = default);
    Task<AuthCreatePageDto> GetAuthorityCreateInvoiceAsync(string accessToken, string? month, int otherSchoolId, string? smsFrom, string? smsTo, string? smsQ, CancellationToken cancellationToken = default);
    Task<AuthorityResult> GenerateAuthorityStudentCountAsync(string accessToken, AuthGenerateCountRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> AutoGenerateAuthorityInvoiceAsync(string accessToken, AuthGenerateCountRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> EnableAuthorityInvoiceJobAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthorityResult> CreateAuthorityServiceInvoicesAsync(string accessToken, AuthCreateServiceRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> AddAuthorityInvoiceCategoryAsync(string accessToken, AuthAddCategoryRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> CreateAuthorityOtherInvoiceAsync(string accessToken, AuthCreateOtherRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> DeleteAuthorityInvoiceAsync(string accessToken, int invoiceId, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SetAuthorityGraceAsync(string accessToken, AuthGraceRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> ClearAuthorityGraceAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default);
    Task<AuthPaidPageDto> GetAuthorityPaidInvoiceAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default);
    Task<AuthorityResult> PayAuthorityInvoicesAsync(string accessToken, AuthPayInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<AuthPrintPageDto> GetAuthorityPrintInvoiceAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default);
    Task<AuthPayPrintDto> GetAuthorityPayPrintAsync(string accessToken, int schoolId, string ids, CancellationToken cancellationToken = default);
    Task<AuthReceiptPrintDto> GetAuthorityReceiptPrintAsync(string accessToken, int receiptId, CancellationToken cancellationToken = default);
    Task<AuthOnlinePayPageDto> GetAuthorityOnlinePayAsync(string accessToken, string? type, int schoolId, string? method, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<AuthLinkTreeDto> GetAuthorityLinksAsync(string accessToken, int categoryId, int subId, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SaveAuthorityLinkCategoryAsync(string accessToken, AuthLinkNameSaveRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> DeleteAuthorityLinkCategoryAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SaveAuthorityLinkSubAsync(string accessToken, AuthLinkNameSaveRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> DeleteAuthorityLinkSubAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SaveAuthorityLinkPageAsync(string accessToken, AuthLinkPageSaveRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> DeleteAuthorityLinkPageAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<AuthRoleListDto> GetAuthorityRolesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthorityResult> CreateAuthorityRoleAsync(string accessToken, AuthRoleSaveRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> DeleteAuthorityRoleAsync(string accessToken, AuthRoleSaveRequest request, CancellationToken cancellationToken = default);
    Task<AuthReferralPageDto> GetAuthorityReferralAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SaveAuthorityReferrerAsync(string accessToken, AuthReferrerSaveRequest request, CancellationToken cancellationToken = default);
    Task<AuthSchoolSearchPageDto> SearchAuthorityReferralSchoolsAsync(string accessToken, string? q, int refId, CancellationToken cancellationToken = default);
    Task<AuthorityResult> AssignAuthoritySchoolAsync(string accessToken, AuthAssignSchoolRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> UpdateAuthorityAssignAsync(string accessToken, AuthAssignUpdateRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> DeleteAuthorityAssignAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<AuthCommissionPageDto> GetAuthorityCommissionAsync(string accessToken, int refId, DateTime? from, DateTime? to, string? status, int detailId, CancellationToken cancellationToken = default);
    Task<AuthorityResult> PayAuthorityCommissionAsync(string accessToken, AuthCommissionPayRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> CreateAuthoritySubAsync(string accessToken, AuthSubSignupRequest request, CancellationToken cancellationToken = default);
    Task<AuthAccessPageDto> GetAuthorityPageAccessAsync(string accessToken, string? userName, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SaveAuthorityPageAccessAsync(string accessToken, AuthAccessSaveRequest request, CancellationToken cancellationToken = default);
    Task<AuthProfileDto> GetAuthorityProfileAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<ProfileResult> SaveAuthorityProfileAsync(string accessToken, AuthProfileDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuthNoticeDto>> GetAdminNoticesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuthNoticeDto>> GetAuthorityNoticesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthorityResult> SaveAuthorityNoticeAsync(string accessToken, AuthNoticeSaveRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> DeleteAuthorityNoticeAsync(string accessToken, int id, CancellationToken cancellationToken = default);
    Task<AuthUnreadDto> GetAuthorityUnreadAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthMessagePageDto> GetAuthorityMessagesAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthorityResult> ReadAuthorityMessageAsync(string accessToken, AuthMessageReadRequest request, CancellationToken cancellationToken = default);
    Task<AuthorityResult> DeleteAuthorityContactAsync(string accessToken, int id, CancellationToken cancellationToken = default);
}

public sealed class SyncApiClient : ISyncApiClient
{
    public const string HttpClientName = "SikkhaloySync";

    public static string NormalizeBaseUrl(string? url)
    {
        url = string.IsNullOrWhiteSpace(url) ? "http://127.0.0.1:5135/" : url.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return "http://127.0.0.1:5135/";

        var builder = new UriBuilder(uri);
        if (string.Equals(builder.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            builder.Host = "127.0.0.1";
        if (!builder.Path.EndsWith('/'))
            builder.Path += "/";
        return builder.Uri.ToString();
    }

    public static SocketsHttpHandler CreateHandler() => new()
    {
        UseProxy = false,
        ConnectTimeout = TimeSpan.FromSeconds(5)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpFactory;

    public SyncApiClient(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await Http().PostAsJsonAsync("api/auth/login", request, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, cancellationToken);
        if (payload is null)
        {
            return new LoginResponse
            {
                Succeeded = false,
                Error = $"সার্ভার উত্তর পড়া যায়নি ({(int)response.StatusCode})।"
            };
        }

        return payload;
    }

    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(4));
            using var response = await Http().GetAsync("api/health", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<PublicInstituteDto>> GetPublicInstitutesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(8));
            var http = Http();
            using var response = await http.GetAsync("api/public/institutes", cts.Token);
            if (!response.IsSuccessStatusCode)
                return [];

            var list = await response.Content.ReadFromJsonAsync<List<PublicInstituteDto>>(JsonOptions, cts.Token) ?? [];
            var baseUrl = http.BaseAddress?.ToString() ?? "http://127.0.0.1:5135/";
            foreach (var item in list)
            {
                if (item.HasLogo)
                    item.LogoUrl = $"{baseUrl}api/public/institutes/{item.SchoolID}/logo";
            }

            return list;
        }
        catch (Exception)
        {
            return [];
        }
    }

    public async Task<PublicStatsDto> GetPublicStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(8));
            using var response = await Http().GetAsync("api/public/stats", cts.Token);
            if (!response.IsSuccessStatusCode)
                return new PublicStatsDto();

            return await response.Content.ReadFromJsonAsync<PublicStatsDto>(JsonOptions, cts.Token)
                   ?? new PublicStatsDto();
        }
        catch (Exception)
        {
            return new PublicStatsDto();
        }
    }

    public async Task<PublicContactResult> SendPublicContactAsync(PublicContactRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(12));
            using var response = await Http().PostAsJsonAsync("api/public/contact", request, cts.Token);
            var result = await response.Content.ReadFromJsonAsync<PublicContactResult>(JsonOptions, cts.Token);
            if (result is not null)
                return result;

            return new PublicContactResult { Succeeded = false, Error = "home.pop.fail" };
        }
        catch (Exception)
        {
            return new PublicContactResult { Succeeded = false, Error = "home.pop.fail" };
        }
    }

    public async Task<PushResponse> PushAsync(string accessToken, PushRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/push")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await Http().SendAsync(message, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<PushResponse>(JsonOptions, cancellationToken)
               ?? new PushResponse();
    }

    public async Task<PullResponse> PullAsync(string accessToken, long since, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/pull?since={since}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await Http().SendAsync(message, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<PullResponse>(JsonOptions, cancellationToken)
               ?? new PullResponse();
    }

    public async Task<StudentIdCheckResult> CheckStudentIdAsync(
        string accessToken, string studentCode, int? exceptServerId = null, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/student-id?code={Uri.EscapeDataString(studentCode)}";
        if (exceptServerId is > 0)
            url += $"&exceptServerId={exceptServerId.Value}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StudentIdCheckResult>(JsonOptions, cancellationToken)
               ?? new StudentIdCheckResult();
    }

    public async Task<IReadOnlyList<SchoolClassDto>> GetClassesAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/classes");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SchoolClassDto>>(JsonOptions, cancellationToken)
               ?? [];
    }

    public async Task<ClassStructureDto> GetClassStructureAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/class-structure");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ClassStructureDto>(JsonOptions, cancellationToken)
               ?? new ClassStructureDto();
    }

    public async Task<IReadOnlyList<EducationYearDto>> GetYearsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/years");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<EducationYearDto>>(JsonOptions, cancellationToken)
               ?? [];
    }

    public async Task<EducationYearResult> CreateYearAsync(
        string accessToken, SaveEducationYearRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/years")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadYearResultAsync(response, cancellationToken);
    }

    public async Task<EducationYearResult> UpdateYearAsync(
        string accessToken, int yearId, SaveEducationYearRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/sync/years/{yearId}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadYearResultAsync(response, cancellationToken);
    }

    public async Task<EducationYearResult> DeleteYearAsync(
        string accessToken, int yearId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/sync/years/{yearId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadYearResultAsync(response, cancellationToken);
    }

    public async Task<OfficeProfileDto> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/profile");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OfficeProfileDto>(JsonOptions, cancellationToken)
               ?? new OfficeProfileDto();
    }

    public async Task<AdminInfoDto?> GetAdminInfoAsync(
        string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/profile/admin");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload) || payload == "null")
            return null;
        return JsonSerializer.Deserialize<AdminInfoDto>(payload, JsonOptions);
    }

    public async Task<ProfileResult> SaveAdminInfoAsync(
        string accessToken, AdminInfoDto request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/profile/admin")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ProfileResult>(JsonOptions, cancellationToken)
               ?? new ProfileResult { Error = "profile.needOnline" };
    }

    public async Task<ProfileResult> ChangePasswordAsync(
        string accessToken, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/profile/password")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ProfileResult>(JsonOptions, cancellationToken)
               ?? new ProfileResult { Error = "profile.needOnline" };
    }

    public async Task<MenuTreeDto> GetMenuAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/menu");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MenuTreeDto>(JsonOptions, cancellationToken)
               ?? new MenuTreeDto();
    }

    public async Task<LoginResponse> SwitchYearAsync(string accessToken, int educationYearId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/auth/switch-year")
        {
            Content = JsonContent.Create(new SwitchYearRequest { EducationYearID = educationYearId })
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, cancellationToken);
        return payload ?? new LoginResponse { Succeeded = false, Error = "login.failed" };
    }

    public async Task<IReadOnlyList<SubAdminDto>> GetSubAdminsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/sub-admins");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SubAdminDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<PageAccessDto> GetPageAccessAsync(string accessToken, string userName, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/page-access?userName={Uri.EscapeDataString(userName)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PageAccessDto>(JsonOptions, cancellationToken) ?? new PageAccessDto();
    }

    public async Task<SavePageAccessResult> SavePageAccessAsync(string accessToken, SavePageAccessRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/page-access")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new SavePageAccessResult { Succeeded = false, Error = "Save failed." };
        return await response.Content.ReadFromJsonAsync<SavePageAccessResult>(JsonOptions, cancellationToken)
               ?? new SavePageAccessResult { Succeeded = false, Error = "Save failed." };
    }

    public async Task<CreateSubAdminResult> CreateSubAdminAsync(string accessToken, CreateSubAdminRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/sub-admins")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new CreateSubAdminResult { Succeeded = false, Error = "sub.failed" };
        return await response.Content.ReadFromJsonAsync<CreateSubAdminResult>(JsonOptions, cancellationToken)
               ?? new CreateSubAdminResult { Succeeded = false, Error = "sub.failed" };
    }

    public async Task<IReadOnlyList<SubAdminAccountDto>> GetSubAdminAccountsAsync(
        string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/sub-admin-accounts");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SubAdminAccountDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<SubAdminStatusResult> SetSubAdminApprovedAsync(
        string accessToken, SetSubAdminApprovedRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/sub-admins/approved")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new SubAdminStatusResult { Succeeded = false, Error = "subact.failed" };
        return await response.Content.ReadFromJsonAsync<SubAdminStatusResult>(JsonOptions, cancellationToken)
               ?? new SubAdminStatusResult { Succeeded = false, Error = "subact.failed" };
    }

    public async Task<SubAdminStatusResult> UnlockSubAdminAsync(
        string accessToken, UnlockSubAdminRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/sub-admins/unlock")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new SubAdminStatusResult { Succeeded = false, Error = "subact.failed" };
        return await response.Content.ReadFromJsonAsync<SubAdminStatusResult>(JsonOptions, cancellationToken)
               ?? new SubAdminStatusResult { Succeeded = false, Error = "subact.failed" };
    }

    public async Task<IReadOnlyList<SubjectDto>> GetSubjectsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/subjects");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SubjectDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<SubjectResult> CreateSubjectAsync(
        string accessToken, SaveSubjectRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/subjects")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSubjectResultAsync(response, cancellationToken);
    }

    public async Task<SubjectResult> UpdateSubjectAsync(
        string accessToken, int subjectId, SaveSubjectRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/sync/subjects/{subjectId}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSubjectResultAsync(response, cancellationToken);
    }

    public async Task<SubjectResult> DeleteSubjectAsync(
        string accessToken, int subjectId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/sync/subjects/{subjectId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSubjectResultAsync(response, cancellationToken);
    }

    public async Task<SubjectResult> SaveSubjectSerialsAsync(
        string accessToken, SaveSubjectSerialsRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/subjects/serials")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSubjectResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<ClassSubjectRowDto>> GetClassSubjectsAsync(
        string accessToken, int classId, int groupId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/class-subjects?classId={classId}&groupId={groupId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ClassSubjectRowDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<SubjectAssignResult> SaveClassSubjectsAsync(
        string accessToken, SaveClassSubjectsRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/class-subjects")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new SubjectAssignResult { Succeeded = false, Error = "asgn.failed" };
        return await response.Content.ReadFromJsonAsync<SubjectAssignResult>(JsonOptions, cancellationToken)
               ?? new SubjectAssignResult { Succeeded = false, Error = "asgn.failed" };
    }

    public async Task<SubjectAssignResult> ClearClassSubjectsAsync(
        string accessToken, int classId, int groupId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/sync/class-subjects?classId={classId}&groupId={groupId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new SubjectAssignResult { Succeeded = false, Error = "asgn.failed" };
        return await response.Content.ReadFromJsonAsync<SubjectAssignResult>(JsonOptions, cancellationToken)
               ?? new SubjectAssignResult { Succeeded = false, Error = "asgn.failed" };
    }

    public async Task<InstitutionDto> GetInstitutionAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/institution");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<InstitutionDto>(JsonOptions, cancellationToken)
               ?? new InstitutionDto();
    }

    public async Task<InstitutionResult> SaveInstitutionAsync(
        string accessToken, InstitutionDto request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, "api/sync/institution")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new InstitutionResult { Succeeded = false, Error = "inst.failed" };
        return await response.Content.ReadFromJsonAsync<InstitutionResult>(JsonOptions, cancellationToken)
               ?? new InstitutionResult { Succeeded = false, Error = "inst.failed" };
    }

    public async Task<IReadOnlyList<HolidayDto>> GetHolidaysAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/holidays");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<HolidayDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<HolidayResult> AddWeeklyHolidaysAsync(
        string accessToken, WeeklyHolidayRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/holidays/weekly")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadHolidayResultAsync(response, cancellationToken);
    }

    public async Task<HolidayResult> AddRangeHolidaysAsync(
        string accessToken, RangeHolidayRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/holidays/range")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadHolidayResultAsync(response, cancellationToken);
    }

    public async Task<HolidayResult> AddHolidayAsync(
        string accessToken, SaveHolidayRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/holidays")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadHolidayResultAsync(response, cancellationToken);
    }

    public async Task<HolidayResult> UpdateHolidayAsync(
        string accessToken, int holidayId, SaveHolidayRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/sync/holidays/{holidayId}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadHolidayResultAsync(response, cancellationToken);
    }

    public async Task<HolidayResult> DeleteHolidayAsync(
        string accessToken, int holidayId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/sync/holidays/{holidayId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadHolidayResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<ReAdmissionCandidateDto>> GetReAdmissionCandidatesAsync(
        string accessToken, int yearId, int classId, int sectionId, int groupId, int shiftId, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/re-admission/candidates?yearId={yearId}&classId={classId}&sectionId={sectionId}&groupId={groupId}&shiftId={shiftId}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ReAdmissionCandidateDto>>(JsonOptions, cancellationToken)
               ?? [];
    }

    public async Task<ReAdmissionAssignDto> GetReAdmissionAssignAsync(
        string accessToken, int studentId, int fromYearId, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/re-admission/assign?studentId={studentId}&fromYearId={fromYearId}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ReAdmissionAssignDto>(JsonOptions, cancellationToken)
               ?? new ReAdmissionAssignDto { Error = "readm.failed" };
    }

    public async Task<IReadOnlyList<ReAdmissionSubjectDto>> GetReAdmissionSubjectsAsync(
        string accessToken, int classId, int groupId, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/re-admission/subjects?classId={classId}&groupId={groupId}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ReAdmissionSubjectDto>>(JsonOptions, cancellationToken)
               ?? [];
    }

    public async Task<ReAdmissionResult> FinishReAdmissionAsync(
        string accessToken, ReAdmissionRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/re-admission")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new ReAdmissionResult { Succeeded = false, Error = "readm.failed" };
        return await response.Content.ReadFromJsonAsync<ReAdmissionResult>(JsonOptions, cancellationToken)
               ?? new ReAdmissionResult { Succeeded = false, Error = "readm.failed" };
    }

    public async Task<IReadOnlyList<ReAdmissionExamDto>> GetReAdmissionExamsAsync(
        string accessToken, int yearId, int classId, bool cumulative, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/re-admission/exams?yearId={yearId}&classId={classId}&cumulative={cumulative}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ReAdmissionExamDto>>(JsonOptions, cancellationToken)
               ?? [];
    }

    public async Task<IReadOnlyList<ReAdmissionPositionDto>> GetReAdmissionPositionsAsync(
        string accessToken, int yearId, int classId, int examId, bool cumulative, bool sectionWise, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/re-admission/positions?yearId={yearId}&classId={classId}&examId={examId}&cumulative={cumulative}&sectionWise={sectionWise}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ReAdmissionPositionDto>>(JsonOptions, cancellationToken)
               ?? [];
    }

    public async Task<BulkReAdmissionResult> FinishBulkReAdmissionAsync(
        string accessToken, BulkReAdmissionRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/re-admission/bulk")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new BulkReAdmissionResult { Succeeded = false, Error = "readm.failed" };
        return await response.Content.ReadFromJsonAsync<BulkReAdmissionResult>(JsonOptions, cancellationToken)
               ?? new BulkReAdmissionResult { Succeeded = false, Error = "readm.failed" };
    }

    public async Task<IReadOnlyList<EmployeeListDto>> GetEmployeesAsync(
        string accessToken, string? type, string? status, string? query, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/employees?type={Uri.EscapeDataString(type ?? "")}&status={Uri.EscapeDataString(status ?? "")}&q={Uri.EscapeDataString(query ?? "")}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<EmployeeListDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<EmployeeResult> CreateTeacherAsync(
        string accessToken, CreateTeacherRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/employees/teachers")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadEmployeeResultAsync(response, cancellationToken);
    }

    public async Task<EmployeeResult> CreateStaffAsync(
        string accessToken, CreateStaffRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/employees/staff")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadEmployeeResultAsync(response, cancellationToken);
    }

    public async Task<EmployeeResult> UpdateEmployeeAsync(
        string accessToken, int employeeId, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/sync/employees/{employeeId}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadEmployeeResultAsync(response, cancellationToken);
    }

    public async Task<EmployeeResult> SetEmployeeStatusAsync(
        string accessToken, int employeeId, SetJobStatusRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/sync/employees/{employeeId}/status")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadEmployeeResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeeIdCardDto>> GetEmployeeIdCardsAsync(
        string accessToken, string? type, string? query, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/employees/id-cards?type={Uri.EscapeDataString(type ?? "")}&q={Uri.EscapeDataString(query ?? "")}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<EmployeeIdCardDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<TeacherAccountDto>> GetTeacherAccountsAsync(
        string accessToken, string? query, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/teachers/accounts?q={Uri.EscapeDataString(query ?? "")}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TeacherAccountDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<TeacherAccountResult> SetTeacherApprovedAsync(
        string accessToken, SetTeacherApprovedRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/teachers/approved")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new TeacherAccountResult { Succeeded = false, Error = "emp.failed" };
        return await response.Content.ReadFromJsonAsync<TeacherAccountResult>(JsonOptions, cancellationToken)
               ?? new TeacherAccountResult { Succeeded = false, Error = "emp.failed" };
    }

    public async Task<TeacherAccountResult> UnlockTeacherAsync(
        string accessToken, UnlockTeacherRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/teachers/unlock")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new TeacherAccountResult { Succeeded = false, Error = "emp.failed" };
        return await response.Content.ReadFromJsonAsync<TeacherAccountResult>(JsonOptions, cancellationToken)
               ?? new TeacherAccountResult { Succeeded = false, Error = "emp.failed" };
    }

    public async Task<IReadOnlyList<TeacherPickDto>> GetActiveTeachersAsync(
        string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/teachers");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TeacherPickDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<TeacherSubjectRowDto>> GetTeacherSubjectsAsync(
        string accessToken, int teacherId, int classId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/teachers/{teacherId}/subjects?classId={classId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TeacherSubjectRowDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<EmployeeResult> ToggleTeacherSubjectAsync(
        string accessToken, int teacherId, ToggleTeacherSubjectRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/sync/teachers/{teacherId}/subjects")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadEmployeeResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<SalaryNameDto>> GetSalaryNamesAsync(
        string accessToken, string kind, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/salary/names?kind={Uri.EscapeDataString(kind)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SalaryNameDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<SalaryResult> CreateSalaryNameAsync(
        string accessToken, string kind, SaveSalaryNameRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/sync/salary/names?kind={Uri.EscapeDataString(kind)}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSalaryResultAsync(response, cancellationToken);
    }

    public async Task<SalaryResult> UpdateSalaryNameAsync(
        string accessToken, string kind, int id, SaveSalaryNameRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/sync/salary/names/{id}?kind={Uri.EscapeDataString(kind)}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSalaryResultAsync(response, cancellationToken);
    }

    public async Task<SalaryResult> DeleteSalaryNameAsync(
        string accessToken, string kind, int id, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/sync/salary/names/{id}?kind={Uri.EscapeDataString(kind)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSalaryResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<SalaryAssignRowDto>> GetSalaryAssignAsync(
        string accessToken, string kind, int nameId, string? type, CancellationToken cancellationToken = default)
    {
        var qs = $"kind={Uri.EscapeDataString(kind)}&nameId={nameId}&type={Uri.EscapeDataString(type ?? "%")}";
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/salary/assign?{qs}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SalaryAssignRowDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<SalaryResult> SaveSalaryAssignAsync(
        string accessToken, string kind, SaveSalaryAssignRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/sync/salary/assign?kind={Uri.EscapeDataString(kind)}")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSalaryResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<PayorderEmployeeDto>> GetPayorderEmployeesAsync(
        string accessToken, string? type, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/salary/payorder-employees?type={Uri.EscapeDataString(type ?? "%")}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<PayorderEmployeeDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<SalaryResult> AssignPayorderAsync(
        string accessToken, AssignPayorderRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/salary/payorder-employees")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSalaryResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<SalaryMonthDto>> GetSalaryMonthsAsync(
        string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/salary/months");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SalaryMonthDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<SalaryResult> GenerateSalaryAsync(
        string accessToken, GenerateSalaryRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/salary/generate")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSalaryResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<MonthlyPayorderDto>> GetMonthlyPayordersAsync(
        string accessToken, int payorderNameId, string monthName, string? type, CancellationToken cancellationToken = default)
    {
        var qs = $"payorderNameId={payorderNameId}&monthName={Uri.EscapeDataString(monthName ?? "")}&type={Uri.EscapeDataString(type ?? "%")}";
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/salary/monthly?{qs}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MonthlyPayorderDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<SalaryResult> UpdateBonusFineAsync(
        string accessToken, UpdateBonusFineRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/salary/bonus-fine")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSalaryResultAsync(response, cancellationToken);
    }

    public async Task<SalaryResult> DeleteMonthlyPayorderAsync(
        string accessToken, int employeePayorderId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/sync/salary/monthly/{employeePayorderId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSalaryResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<AccountOptionDto>> GetSalaryAccountsAsync(
        string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/salary/accounts");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<AccountOptionDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<SalaryResult> PaySalaryAsync(
        string accessToken, PaySalaryRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/salary/pay")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSalaryResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<PaidRecordDto>> GetPaidRecordsAsync(
        string accessToken, int employeeId, int employeePayorderId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Get, $"api/sync/salary/paid-records?employeeId={employeeId}&employeePayorderId={employeePayorderId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<PaidRecordDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<SalaryResult> DeletePaidRecordAsync(
        string accessToken, int recordId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/sync/salary/paid-records/{recordId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadSalaryResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<PaidDueRowDto>> GetPaidDueAsync(
        string accessToken, IReadOnlyList<int> payorderNameIds, CancellationToken cancellationToken = default)
    {
        var ids = payorderNameIds is { Count: > 0 } ? string.Join(",", payorderNameIds) : "";
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/salary/paid-due?ids={Uri.EscapeDataString(ids)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<PaidDueRowDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<StudentSignupListsDto> GetStudentSignupAsync(
        string accessToken, int classId, int groupId, int sectionId, int shiftId, string? studentId,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/student-info/signup?classId={classId}&groupId={groupId}&sectionId={sectionId}&shiftId={shiftId}&id={Uri.EscapeDataString(studentId ?? "")}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StudentSignupListsDto>(JsonOptions, cancellationToken)
               ?? new StudentSignupListsDto();
    }

    public async Task<StudentInfoResult> CreateStudentUsersAsync(
        string accessToken, CreateStudentUsersRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/student-info/signup")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadStudentInfoResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentAccountDto>> GetStudentAccountsAsync(
        string accessToken, int classId, int groupId, int sectionId, int shiftId, string? studentId,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/student-info/accounts?classId={classId}&groupId={groupId}&sectionId={sectionId}&shiftId={shiftId}&id={Uri.EscapeDataString(studentId ?? "")}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<StudentAccountDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<StudentAccountResult> SetStudentApprovedAsync(
        string accessToken, SetStudentApprovedRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/student-info/accounts/approved")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new StudentAccountResult { Succeeded = false, Error = "si.failed" };
        return await response.Content.ReadFromJsonAsync<StudentAccountResult>(JsonOptions, cancellationToken)
               ?? new StudentAccountResult { Succeeded = false, Error = "si.failed" };
    }

    public async Task<StudentAccountResult> UnlockStudentAsync(
        string accessToken, UnlockStudentRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/student-info/accounts/unlock")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new StudentAccountResult { Succeeded = false, Error = "si.failed" };
        return await response.Content.ReadFromJsonAsync<StudentAccountResult>(JsonOptions, cancellationToken)
               ?? new StudentAccountResult { Succeeded = false, Error = "si.failed" };
    }

    public async Task<StudentInfoResult> DeleteStudentAccountAsync(
        string accessToken, DeleteStudentAccountRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/student-info/accounts/delete")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadStudentInfoResultAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentIdCardDto>> GetStudentIdCardsAsync(
        string accessToken, int classId, int groupId, int sectionId, int shiftId, string? ids,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/student-info/id-cards?classId={classId}&groupId={groupId}&sectionId={sectionId}&shiftId={shiftId}&ids={Uri.EscapeDataString(ids ?? "")}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<StudentIdCardDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<StudentPhotoDto>> GetStudentPhotosAsync(
        string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/student-info/photos");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<StudentPhotoDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<StudentReportDto> GetStudentReportAsync(
        string accessToken, string studentId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get,
            $"api/sync/student-info/report?id={Uri.EscapeDataString(studentId)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StudentReportDto>(JsonOptions, cancellationToken)
               ?? new StudentReportDto();
    }

    public async Task<StudentPlacementDto?> GetStudentPlacementAsync(
        string accessToken, string studentId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/student-info/placement?id={Uri.EscapeDataString(studentId)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return null;
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload) || payload == "null")
            return null;
        return JsonSerializer.Deserialize<StudentPlacementDto>(payload, JsonOptions);
    }

    public async Task<StudentInfoResult> SaveStudentPlacementAsync(
        string accessToken, SaveStudentPlacementRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/student-info/placement")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadStudentInfoResultAsync(response, cancellationToken);
    }

    public async Task<StudentSubjectsDto> GetStudentSubjectsAsync(
        string accessToken, string studentId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/student-info/subjects?id={Uri.EscapeDataString(studentId)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StudentSubjectsDto>(JsonOptions, cancellationToken)
               ?? new StudentSubjectsDto();
    }

    public async Task<StudentInfoResult> SaveStudentSubjectsAsync(
        string accessToken, SaveStudentSubjectsRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/student-info/subjects")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadStudentInfoResultAsync(response, cancellationToken);
    }

    public async Task<StudentPlacementDto?> GetStudentCertificateAsync(
        string accessToken, string studentId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/student-info/certificate?id={Uri.EscapeDataString(studentId)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return null;
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload) || payload == "null")
            return null;
        return JsonSerializer.Deserialize<StudentPlacementDto>(payload, JsonOptions);
    }

    public async Task<IReadOnlyList<SmStudentRowDto>> GetSmStudentsAsync(
        string accessToken, int classId, int groupId, int sectionId, int shiftId, string? studentId, int? subjectId,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/student-mgmt/students?classId={classId}&groupId={groupId}&sectionId={sectionId}&shiftId={shiftId}&id={Uri.EscapeDataString(studentId ?? "")}&subjectId={subjectId ?? 0}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SmStudentRowDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public async Task<StudentPlacementDto?> GetSmClassChangeStudentAsync(
        string accessToken, int studentId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/student-mgmt/class-change?studentId={studentId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return null;
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload) || payload == "null")
            return null;
        return JsonSerializer.Deserialize<StudentPlacementDto>(payload, JsonOptions);
    }

    public Task<StudentInfoResult> ChangeClassAsync(
        string accessToken, ChangeClassRequest request, CancellationToken cancellationToken = default) =>
        PostStudentInfoAsync(accessToken, "api/sync/student-mgmt/class-change", request, cancellationToken);

    public Task<StudentInfoResult> SaveSmPlacementAsync(
        string accessToken, BulkPlacementRequest request, CancellationToken cancellationToken = default) =>
        PostStudentInfoAsync(accessToken, "api/sync/student-mgmt/group-section-shift", request, cancellationToken);

    public Task<StudentInfoResult> SaveSmOneSubjectAsync(
        string accessToken, SaveOneSubjectRequest request, CancellationToken cancellationToken = default) =>
        PostStudentInfoAsync(accessToken, "api/sync/student-mgmt/subject-students", request, cancellationToken);

    public Task<StudentInfoResult> SaveSmClassSubjectsAsync(
        string accessToken, ReplaceClassSubjectsRequest request, CancellationToken cancellationToken = default) =>
        PostStudentInfoAsync(accessToken, "api/sync/student-mgmt/class-subjects", request, cancellationToken);

    public Task<StudentInfoResult> SaveSmRollsAsync(
        string accessToken, SaveRollSeatRequest request, CancellationToken cancellationToken = default) =>
        PostStudentInfoAsync(accessToken, "api/sync/student-mgmt/rolls", request, cancellationToken);

    public Task<StudentInfoResult> SaveStudentPhotoAsync(
        string accessToken, SaveStudentPhotoRequest request, CancellationToken cancellationToken = default) =>
        PostStudentInfoAsync(accessToken, "api/sync/student-mgmt/photo", request, cancellationToken);

    public async Task<TcStudentDto?> FindTcStudentAsync(
        string accessToken, string studentId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/student-mgmt/tc?id={Uri.EscapeDataString(studentId)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return null;
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload) || payload == "null")
            return null;
        return JsonSerializer.Deserialize<TcStudentDto>(payload, JsonOptions);
    }

    public async Task<IReadOnlyList<TcStudentDto>> ListTcStudentsAsync(
        string accessToken, int classId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/student-mgmt/tc/list?classId={classId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TcStudentDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public Task<StudentInfoResult> GiveTcAsync(
        string accessToken, GiveTcRequest request, CancellationToken cancellationToken = default) =>
        PostStudentInfoAsync(accessToken, "api/sync/student-mgmt/tc/give", request, cancellationToken);

    public Task<StudentInfoResult> ActivateTcAsync(
        string accessToken, ActivateTcRequest request, CancellationToken cancellationToken = default) =>
        PostStudentInfoAsync(accessToken, "api/sync/student-mgmt/tc/activate", request, cancellationToken);

    public async Task<IReadOnlyList<NoticeDto>> GetNoticesAsync(
        string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/student-mgmt/notices");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<NoticeDto>>(JsonOptions, cancellationToken) ?? [];
    }

    public Task<StudentInfoResult> SaveNoticeAsync(
        string accessToken, SaveNoticeRequest request, CancellationToken cancellationToken = default) =>
        PostStudentInfoAsync(accessToken, "api/sync/student-mgmt/notices", request, cancellationToken);

    public Task<StudentInfoResult> DeleteNoticesAsync(
        string accessToken, DeleteNoticesRequest request, CancellationToken cancellationToken = default) =>
        PostStudentInfoAsync(accessToken, "api/sync/student-mgmt/notices/delete", request, cancellationToken);

    public Task<IReadOnlyList<AttendanceScheduleDto>> GetAttendanceSchedulesAsync(
        string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<AttendanceScheduleDto>(accessToken, "api/sync/attendance/schedules", cancellationToken);

    public Task<AttendanceResult> CreateAttendanceScheduleAsync(
        string accessToken, SaveScheduleRequest request, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, "api/sync/attendance/schedules", request, cancellationToken);

    public Task<AttendanceResult> RenameAttendanceScheduleAsync(
        string accessToken, int scheduleId, SaveScheduleRequest request, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, $"api/sync/attendance/schedules/{scheduleId}", request, cancellationToken);

    public Task<AttendanceResult> DeleteAttendanceScheduleAsync(
        string accessToken, int scheduleId, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, $"api/sync/attendance/schedules/{scheduleId}/delete", new { }, cancellationToken);

    public Task<AttendanceResult> SaveAttendanceScheduleDaysAsync(
        string accessToken, SaveScheduleDaysRequest request, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, "api/sync/attendance/schedules/days", request, cancellationToken);

    public async Task<AttendanceSettingsDto> GetAttendanceSettingsAsync(
        string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/attendance/settings");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AttendanceSettingsDto>(JsonOptions, cancellationToken)
               ?? new AttendanceSettingsDto();
    }

    public Task<AttendanceResult> SaveAttendanceSettingsAsync(
        string accessToken, AttendanceSettingsDto request, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, "api/sync/attendance/settings", request, cancellationToken);

    public Task<AttendanceDownloadResult> DownloadAttendanceAppAsync(
        string accessToken, CancellationToken cancellationToken = default) =>
        DownloadAttendanceFileAsync(accessToken, "api/sync/attendance/settings/app", "att.noInstaller", cancellationToken);

    public Task<AttendanceDownloadResult> DownloadAttendancePhotosAsync(
        string accessToken, CancellationToken cancellationToken = default) =>
        DownloadAttendanceFileAsync(accessToken, "api/sync/attendance/settings/photos.zip", "att.noPhotos", cancellationToken);

    public Task<AttendanceDownloadResult> DownloadAttendanceUsersAsync(
        string accessToken, CancellationToken cancellationToken = default) =>
        DownloadAttendanceFileAsync(accessToken, "api/sync/attendance/settings/users.csv", "att.dlFailed", cancellationToken);

    public Task<IReadOnlyList<StudentRfidRowDto>> GetStudentRfidAsync(
        string accessToken, int scheduleId, int classId, int groupId, int sectionId, int shiftId,
        CancellationToken cancellationToken = default) =>
        GetListAsync<StudentRfidRowDto>(
            accessToken,
            $"api/sync/attendance/student/rfid?scheduleId={scheduleId}&classId={classId}&groupId={groupId}&sectionId={sectionId}&shiftId={shiftId}",
            cancellationToken);

    public Task<AttendanceResult> SaveStudentRfidAsync(
        string accessToken, SaveStudentRfidRequest request, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, "api/sync/attendance/student/rfid", request, cancellationToken);

    public Task<IReadOnlyList<EmployeeRfidRowDto>> GetEmployeeRfidAsync(
        string accessToken, int scheduleId, string? type, CancellationToken cancellationToken = default) =>
        GetListAsync<EmployeeRfidRowDto>(
            accessToken,
            $"api/sync/attendance/employee/rfid?scheduleId={scheduleId}&type={Uri.EscapeDataString(type ?? "%")}",
            cancellationToken);

    public Task<AttendanceResult> SaveEmployeeRfidAsync(
        string accessToken, SaveEmployeeRfidRequest request, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, "api/sync/attendance/employee/rfid", request, cancellationToken);

    public Task<IReadOnlyList<StudentManualRowDto>> GetStudentManualAsync(
        string accessToken, int scheduleId, int classId, int groupId, int sectionId, int shiftId, DateTime date,
        CancellationToken cancellationToken = default) =>
        GetListAsync<StudentManualRowDto>(
            accessToken,
            $"api/sync/attendance/student/manual?scheduleId={scheduleId}&classId={classId}&groupId={groupId}&sectionId={sectionId}&shiftId={shiftId}&date={date:yyyy-MM-dd}",
            cancellationToken);

    public Task<AttendanceResult> SaveStudentManualAsync(
        string accessToken, SaveStudentManualRequest request, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, "api/sync/attendance/student/manual", request, cancellationToken);

    public Task<IReadOnlyList<EmployeeManualRowDto>> GetEmployeeManualAsync(
        string accessToken, int scheduleId, string? type, DateTime date, CancellationToken cancellationToken = default) =>
        GetListAsync<EmployeeManualRowDto>(
            accessToken,
            $"api/sync/attendance/employee/manual?scheduleId={scheduleId}&type={Uri.EscapeDataString(type ?? "%")}&date={date:yyyy-MM-dd}",
            cancellationToken);

    public Task<AttendanceResult> SaveEmployeeManualAsync(
        string accessToken, SaveEmployeeManualRequest request, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, "api/sync/attendance/employee/manual", request, cancellationToken);

    public Task<IReadOnlyList<StudentAttendanceRecordDto>> GetStudentAttendanceRecordsAsync(
        string accessToken, string? status, int classId, int groupId, int sectionId, int shiftId, int scheduleId,
        DateTime from, DateTime to, CancellationToken cancellationToken = default) =>
        GetListAsync<StudentAttendanceRecordDto>(
            accessToken,
            $"api/sync/attendance/student/records?status={Uri.EscapeDataString(status ?? "%")}&classId={classId}&groupId={groupId}&sectionId={sectionId}&shiftId={shiftId}&scheduleId={scheduleId}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}",
            cancellationToken);

    public Task<IReadOnlyList<StudentAttendanceSummaryDto>> GetStudentAttendanceSummaryAsync(
        string accessToken, int classId, int groupId, int sectionId, int shiftId, int scheduleId,
        DateTime from, DateTime to, CancellationToken cancellationToken = default) =>
        GetListAsync<StudentAttendanceSummaryDto>(
            accessToken,
            $"api/sync/attendance/student/summary?classId={classId}&groupId={groupId}&sectionId={sectionId}&shiftId={shiftId}&scheduleId={scheduleId}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}",
            cancellationToken);

    public Task<IReadOnlyList<EmployeeAttendanceRecordDto>> GetEmployeeAttendanceRecordsAsync(
        string accessToken, string? type, string? status, int scheduleId, int employeeId, DateTime from, DateTime to,
        CancellationToken cancellationToken = default) =>
        GetListAsync<EmployeeAttendanceRecordDto>(
            accessToken,
            $"api/sync/attendance/employee/records?type={Uri.EscapeDataString(type ?? "%")}&status={Uri.EscapeDataString(status ?? "%")}&scheduleId={scheduleId}&employeeId={employeeId}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}",
            cancellationToken);

    public Task<IReadOnlyList<EmployeeAttendanceSummaryDto>> GetEmployeeAttendanceSummaryAsync(
        string accessToken, string? type, int scheduleId, int employeeId, DateTime from, DateTime to,
        CancellationToken cancellationToken = default) =>
        GetListAsync<EmployeeAttendanceSummaryDto>(
            accessToken,
            $"api/sync/attendance/employee/summary?type={Uri.EscapeDataString(type ?? "%")}&scheduleId={scheduleId}&employeeId={employeeId}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}",
            cancellationToken);

    public Task<IReadOnlyList<string>> GetAttendanceLeaveTypesAsync(
        string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<string>(accessToken, "api/sync/attendance/leave-types", cancellationToken);

    public Task<IReadOnlyList<AttendanceLeaveTypeDto>> GetAttendanceLeaveTypeRowsAsync(
        string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<AttendanceLeaveTypeDto>(accessToken, "api/sync/attendance/leave-types/rows", cancellationToken);

    public Task<AttendanceResult> AddAttendanceLeaveTypeAsync(
        string accessToken, SaveLeaveTypeRequest request, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, "api/sync/attendance/leave-types", request, cancellationToken);

    public Task<AttendanceResult> DeleteAttendanceLeaveTypeAsync(
        string accessToken, int leaveTypeId, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, $"api/sync/attendance/leave-types/{leaveTypeId}/delete", new { }, cancellationToken);

    public async Task<StudentLeavePersonDto?> FindStudentLeaveAsync(
        string accessToken, string id, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Get, $"api/sync/attendance/student/leave/find?id={Uri.EscapeDataString(id ?? "")}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return null;
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload) || payload == "null")
            return null;
        return JsonSerializer.Deserialize<StudentLeavePersonDto>(payload, JsonOptions);
    }

    public Task<IReadOnlyList<StudentLeaveSuggestDto>> SuggestStudentLeaveAsync(
        string accessToken, string query, CancellationToken cancellationToken = default) =>
        GetListAsync<StudentLeaveSuggestDto>(
            accessToken, $"api/sync/attendance/student/leave/suggest?q={Uri.EscapeDataString(query ?? "")}", cancellationToken);

    public async Task<StudentLeavePrintDto?> GetStudentLeavePrintAsync(
        string accessToken, int leaveId, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Get, $"api/sync/attendance/student/leave/print/{leaveId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            return null;
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(payload) || payload == "null")
            return null;
        return JsonSerializer.Deserialize<StudentLeavePrintDto>(payload, JsonOptions);
    }

    public Task<IReadOnlyList<StudentLeaveRowDto>> GetStudentLeavesAsync(
        string accessToken, int studentId, CancellationToken cancellationToken = default) =>
        GetListAsync<StudentLeaveRowDto>(
            accessToken, $"api/sync/attendance/student/leave?studentId={studentId}", cancellationToken);

    public Task<AttendanceResult> SaveStudentLeaveAsync(
        string accessToken, SaveStudentLeaveRequest request, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, "api/sync/attendance/student/leave", request, cancellationToken);

    public Task<AttendanceResult> DeleteStudentLeaveAsync(
        string accessToken, int leaveId, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, $"api/sync/attendance/student/leave/{leaveId}/delete", new { }, cancellationToken);

    public Task<IReadOnlyList<EmployeeLeavePickDto>> GetEmployeeLeavePicksAsync(
        string accessToken, string? type, string? query, CancellationToken cancellationToken = default) =>
        GetListAsync<EmployeeLeavePickDto>(
            accessToken,
            $"api/sync/attendance/employee/leave/picks?type={Uri.EscapeDataString(type ?? "%")}&q={Uri.EscapeDataString(query ?? "")}",
            cancellationToken);

    public Task<AttendanceResult> SaveEmployeeLeaveAsync(
        string accessToken, SaveEmployeeLeaveRequest request, CancellationToken cancellationToken = default) =>
        PostAttendanceAsync(accessToken, "api/sync/attendance/employee/leave", request, cancellationToken);

    public Task<IReadOnlyList<LeaveReportRowDto>> GetLeaveReportAsync(
        string accessToken, string? type, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/attendance/leave-report?type={Uri.EscapeDataString(type ?? "Student")}";
        if (from.HasValue)
            url += $"&from={from:yyyy-MM-dd}";
        if (to.HasValue)
            url += $"&to={to:yyyy-MM-dd}";
        return GetListAsync<LeaveReportRowDto>(accessToken, url, cancellationToken);
    }

    public Task<IReadOnlyList<AttendanceMonthDto>> GetAttendanceFineMonthsAsync(
        string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<AttendanceMonthDto>(accessToken, "api/sync/attendance/fine/months", cancellationToken);

    public Task<IReadOnlyList<AttendanceFineRowDto>> GenerateAttendanceFineAsync(
        string accessToken, GenerateFineRequest request, CancellationToken cancellationToken = default) =>
        GetListPostAsync<AttendanceFineRowDto>(accessToken, "api/sync/attendance/fine", request, cancellationToken);

    public Task<IReadOnlyList<PaymentRoleDto>> GetPaymentRolesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<PaymentRoleDto>(accessToken, "api/sync/accounts/roles", cancellationToken);

    public Task<AccountsResult> CreatePaymentRoleAsync(string accessToken, SavePaymentRoleRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/roles", request, cancellationToken);

    public Task<AccountsResult> UpdatePaymentRoleAsync(string accessToken, int id, SavePaymentRoleRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, $"api/sync/accounts/roles/{id}", request, cancellationToken);

    public Task<AccountsResult> DeletePaymentRoleAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/roles/delete", new AccountsIdRequest { Id = id }, cancellationToken);

    public Task<IReadOnlyList<AssignedRoleDto>> GetAssignedRolesAsync(string accessToken, int classId, int roleId, CancellationToken cancellationToken = default) =>
        GetListAsync<AssignedRoleDto>(accessToken, $"api/sync/accounts/assigned?classId={classId}&roleId={roleId}", cancellationToken);

    public Task<AccountsResult> AssignPaymentRoleAsync(string accessToken, SaveAssignedRoleRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/assigned", request, cancellationToken);

    public Task<AccountsResult> BulkAssignRolesAsync(string accessToken, BulkAssignRoleRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/assigned/bulk", request, cancellationToken);

    public async Task<AssignableRolesDto> GetAssignableRolesAsync(
        string accessToken, IReadOnlyList<int> classIds, CancellationToken cancellationToken = default)
    {
        var qs = string.Join(",", classIds.Where(x => x > 0).Distinct());
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/accounts/assigned/available?classIds={Uri.EscapeDataString(qs)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AssignableRolesDto>(JsonOptions, cancellationToken) ?? new AssignableRolesDto();
    }

    public Task<AccountsResult> UpdateAssignedRoleAsync(string accessToken, UpdateAssignedRoleRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/assigned/update", request, cancellationToken);

    public Task<AccountsResult> DeleteAssignedRoleAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, $"api/sync/accounts/assigned/{id}/delete", new { }, cancellationToken);

    public Task<IReadOnlyList<PayOrderStudentDto>> GetPayOrderStudentsAsync(string accessToken, int classId, CancellationToken cancellationToken = default) =>
        GetListAsync<PayOrderStudentDto>(accessToken, $"api/sync/accounts/payorder/students?classId={classId}", cancellationToken);

    public Task<AccountsResult> CreatePayOrdersAsync(string accessToken, CreatePayOrdersRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/payorder", request, cancellationToken);

    public Task<IReadOnlyList<UnpaidPayOrderDto>> GetUnpaidPayOrdersAsync(string accessToken, int classId, int roleId, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var qs = $"classId={classId}&roleId={roleId}";
        if (endDate is not null)
            qs += $"&endDate={endDate:yyyy-MM-dd}";
        return GetListAsync<UnpaidPayOrderDto>(accessToken, $"api/sync/accounts/payorder/unpaid?{qs}", cancellationToken);
    }

    public Task<AccountsResult> RemovePayOrdersAsync(string accessToken, RemovePayOrderRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/payorder/remove", request, cancellationToken);

    public Task<AccountsResult> ChangePayOrderDateAsync(string accessToken, ChangePayOrderDateRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/payorder/dates", request, cancellationToken);

    public Task<IReadOnlyList<CashAccountDto>> GetCashAccountsAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<CashAccountDto>(accessToken, "api/sync/accounts/cash", cancellationToken);

    public Task<AccountsResult> CreateCashAccountAsync(string accessToken, SaveCashAccountRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/cash", request, cancellationToken);

    public Task<AccountsResult> SetDefaultCashAccountAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, $"api/sync/accounts/cash/{id}/default", new { }, cancellationToken);

    public Task<AccountsResult> DepositCashAsync(string accessToken, AccountMoveRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/cash/deposit", request, cancellationToken);

    public Task<AccountsResult> WithdrawCashAsync(string accessToken, AccountMoveRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/cash/withdraw", request, cancellationToken);

    public Task<IReadOnlyList<AccountMoveDto>> GetCashDepositsAsync(string accessToken, int accountId, CancellationToken cancellationToken = default) =>
        GetListAsync<AccountMoveDto>(accessToken, $"api/sync/accounts/cash/{accountId}/deposits", cancellationToken);

    public Task<IReadOnlyList<AccountMoveDto>> GetCashWithdrawsAsync(string accessToken, int accountId, CancellationToken cancellationToken = default) =>
        GetListAsync<AccountMoveDto>(accessToken, $"api/sync/accounts/cash/{accountId}/withdraws", cancellationToken);

    public Task<IReadOnlyList<FeeSuggestDto>> SuggestFeeStudentsAsync(string accessToken, string query, CancellationToken cancellationToken = default) =>
        GetListAsync<FeeSuggestDto>(accessToken, $"api/sync/accounts/students/suggest?q={Uri.EscapeDataString(query ?? "")}", cancellationToken);

    public async Task<FeeStudentBundleDto> GetFeeStudentBundleAsync(string accessToken, string id, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/accounts/students/bundle?id={Uri.EscapeDataString(id ?? "")}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FeeStudentBundleDto>(JsonOptions, cancellationToken) ?? new FeeStudentBundleDto();
    }

    public Task<AccountsResult> CollectPaymentAsync(string accessToken, CollectPaymentRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/collect", request, cancellationToken);

    public Task<AccountsResult> AddMorePayOrderAsync(string accessToken, AddMorePayOrderRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/add-more", request, cancellationToken);

    public Task<AccountsResult> SaveConcessionAsync(string accessToken, SaveConcessionRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/concession", request, cancellationToken);

    public async Task<ReceiptDetailDto?> GetMoneyReceiptAsync(string accessToken, string receiptNo, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/accounts/receipt?no={Uri.EscapeDataString(receiptNo ?? "")}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ReceiptDetailDto>(JsonOptions, cancellationToken);
    }

    public Task<AccountsResult> UpdatePrintedReceiptAsync(string accessToken, PrintedReceiptRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/receipt/printed", request, cancellationToken);

    public async Task<PaymentSmsSettingDto> GetPaymentSmsSettingAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "api/sync/accounts/sms");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PaymentSmsSettingDto>(JsonOptions, cancellationToken)
               ?? new PaymentSmsSettingDto();
    }

    public Task<AccountsResult> SavePaymentSmsSettingAsync(string accessToken, bool active, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/sms", new PaymentSmsSettingDto { Active = active }, cancellationToken);

    public Task<AccountsResult> SendReceiptSmsAsync(string accessToken, int moneyReceiptId, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, $"api/sync/accounts/receipt/{moneyReceiptId}/sms", new { }, cancellationToken);

    public Task<AccountsResult> UnpaidMoneyReceiptAsync(string accessToken, int moneyReceiptId, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, $"api/sync/accounts/receipt/{moneyReceiptId}/unpaid", new { }, cancellationToken);

    public Task<IReadOnlyList<ExtraIncomeCategoryDto>> GetExtraIncomeCategoriesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<ExtraIncomeCategoryDto>(accessToken, "api/sync/accounts/extra/categories", cancellationToken);

    public Task<AccountsResult> CreateExtraIncomeCategoryAsync(string accessToken, string name, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/extra/categories", new SaveExtraCategoryRequest { Name = name }, cancellationToken);

    public Task<AccountsResult> UpdateExtraIncomeCategoryAsync(string accessToken, int id, string name, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/extra/categories/update", new SaveExtraCategoryRequest { ExtraIncomeCategoryID = id, Name = name }, cancellationToken);

    public Task<AccountsResult> DeleteExtraIncomeCategoryAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, $"api/sync/accounts/extra/categories/{id}/delete", new { }, cancellationToken);

    public async Task<ExtraIncomeListDto> GetExtraIncomeAsync(string accessToken, int categoryId, DateTime? from, DateTime? to, string? receiptNo = null, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/accounts/extra?categoryId={categoryId}";
        if (from is not null) url += $"&from={from:yyyy-MM-dd}";
        if (to is not null) url += $"&to={to:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(receiptNo)) url += $"&receiptNo={Uri.EscapeDataString(receiptNo.Trim())}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ExtraIncomeListDto>(JsonOptions, cancellationToken) ?? new ExtraIncomeListDto();
    }

    public Task<AccountsResult> CreateExtraIncomeAsync(string accessToken, SaveExtraIncomeRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/extra", request, cancellationToken);

    public Task<AccountsResult> UpdateExtraIncomeAsync(string accessToken, SaveExtraIncomeRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/extra/update", request, cancellationToken);

    public Task<AccountsResult> DeleteExtraIncomeAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, $"api/sync/accounts/extra/{id}/delete", new { }, cancellationToken);

    public async Task<ExtraIncomeDto?> GetExtraIncomeOneAsync(string accessToken, int id, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/accounts/extra/{id}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ExtraIncomeDto>(JsonOptions, cancellationToken);
    }

    public Task<IReadOnlyList<ExpenseCategoryDto>> GetExpenseCategoriesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<ExpenseCategoryDto>(accessToken, "api/sync/accounts/expense/categories", cancellationToken);

    public Task<AccountsResult> CreateExpenseCategoryAsync(string accessToken, string name, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/expense/categories", new SaveExpenseCategoryRequest { Name = name }, cancellationToken);

    public Task<AccountsResult> UpdateExpenseCategoryAsync(string accessToken, int id, string name, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/expense/categories/update", new SaveExpenseCategoryRequest { ExpenseCategoryID = id, Name = name }, cancellationToken);

    public Task<AccountsResult> DeleteExpenseCategoryAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, $"api/sync/accounts/expense/categories/{id}/delete", new { }, cancellationToken);

    public Task<IReadOnlyList<ExpenseSubCategoryDto>> GetExpenseSubCategoriesAsync(string accessToken, int categoryId, CancellationToken cancellationToken = default) =>
        GetListAsync<ExpenseSubCategoryDto>(accessToken, $"api/sync/accounts/expense/subcategories?categoryId={categoryId}", cancellationToken);

    public Task<AccountsResult> CreateExpenseSubCategoryAsync(string accessToken, int categoryId, string name, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/expense/subcategories", new SaveExpenseSubCategoryRequest { ExpenseCategoryID = categoryId, Name = name }, cancellationToken);

    public Task<AccountsResult> UpdateExpenseSubCategoryAsync(string accessToken, int id, string name, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/expense/subcategories/update", new SaveExpenseSubCategoryRequest { ExpenseSubCategoryID = id, Name = name }, cancellationToken);

    public Task<AccountsResult> DeleteExpenseSubCategoryAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, $"api/sync/accounts/expense/subcategories/{id}/delete", new { }, cancellationToken);

    public async Task<ExpenseListDto> GetExpenseAsync(string accessToken, int categoryId, int subCategoryId, DateTime? from, DateTime? to, string? receiptNo = null, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/accounts/expense?categoryId={categoryId}&subCategoryId={subCategoryId}";
        if (from is not null) url += $"&from={from:yyyy-MM-dd}";
        if (to is not null) url += $"&to={to:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(receiptNo)) url += $"&receiptNo={Uri.EscapeDataString(receiptNo.Trim())}";
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ExpenseListDto>(JsonOptions, cancellationToken) ?? new ExpenseListDto();
    }

    public Task<AccountsResult> CreateExpenseAsync(string accessToken, SaveExpenseRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/expense", request, cancellationToken);

    public Task<AccountsResult> UpdateExpenseAsync(string accessToken, SaveExpenseRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/expense/update", request, cancellationToken);

    public Task<AccountsResult> DeleteExpenseAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, $"api/sync/accounts/expense/{id}/delete", new { }, cancellationToken);

    public Task<AccountsSummaryDto> GetAccountsSummaryAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<AccountsSummaryDto>(accessToken, "api/sync/accounts/reports/summary", cancellationToken);

    public Task<MonthBasedDto> GetMonthBasedReportAsync(string accessToken, DateTime? from, DateTime? to, int classId, string? roleIds, string? sectionId = null, bool students = false, bool money = true, CancellationToken cancellationToken = default) =>
        GetItemAsync<MonthBasedDto>(accessToken, $"api/sync/accounts/reports/month?classId={classId}&students={students}&money={money}{QDates(from, to)}{Q("roleIds", roleIds)}{Q("sectionId", sectionId)}", cancellationToken);

    public Task<IReadOnlyList<NameAmountDto>> GetMonthBasedRolesAsync(string accessToken, int classId, CancellationToken cancellationToken = default) =>
        GetListAsync<NameAmountDto>(accessToken, $"api/sync/accounts/reports/month-roles?classId={classId}", cancellationToken);

    public Task<IncomeExpenseReportDto> GetIncomeReportAsync(string accessToken, DateTime? from, DateTime? to, string? category, CancellationToken cancellationToken = default) =>
        GetItemAsync<IncomeExpenseReportDto>(accessToken, $"api/sync/accounts/reports/income?category={Uri.EscapeDataString(category ?? "%")}{QDates(from, to)}", cancellationToken);

    public Task<IncomeExpenseReportDto> GetExpenseReportAsync(string accessToken, DateTime? from, DateTime? to, string? category, CancellationToken cancellationToken = default) =>
        GetItemAsync<IncomeExpenseReportDto>(accessToken, $"api/sync/accounts/reports/expense?category={Uri.EscapeDataString(category ?? "%")}{QDates(from, to)}", cancellationToken);

    public Task<NetReportDto> GetNetReportAsync(string accessToken, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<NetReportDto>(accessToken, $"api/sync/accounts/reports/net?{QDates(from, to).TrimStart('&')}", cancellationToken);

    public Task<CurrentDueDto> GetCurrentDueAsync(string accessToken, int classId, string? sectionId, string? roleId, string? id, CancellationToken cancellationToken = default) =>
        GetItemAsync<CurrentDueDto>(accessToken, $"api/sync/accounts/reports/due?classId={classId}{Q("sectionId", sectionId)}{Q("roleId", roleId)}{Q("id", id)}", cancellationToken);

    public Task<IReadOnlyList<NameAmountDto>> GetDueRolesAsync(string accessToken, int classId, CancellationToken cancellationToken = default) =>
        GetListAsync<NameAmountDto>(accessToken, $"api/sync/accounts/reports/due-roles?classId={classId}", cancellationToken);

    public Task<CurrentDueStudentDetailDto> GetDueDetailsAsync(string accessToken, string id, string? roleId, CancellationToken cancellationToken = default) =>
        GetItemAsync<CurrentDueStudentDetailDto>(accessToken, $"api/sync/accounts/reports/due-details?id={Uri.EscapeDataString(id ?? "")}{Q("roleId", roleId)}", cancellationToken);

    public Task<AccountsResult> SendDueSmsAsync(string accessToken, DueSmsRequest request, CancellationToken cancellationToken = default) =>
        PostAccountsAsync(accessToken, "api/sync/accounts/reports/due-sms", request, cancellationToken);

    public Task<PayorderReportDto> GetPayorderReportAsync(string accessToken, DateTime? from, DateTime? to, int roleId, CancellationToken cancellationToken = default) =>
        GetItemAsync<PayorderReportDto>(accessToken, $"api/sync/accounts/reports/payorder?roleId={roleId}{QDates(from, to)}", cancellationToken);

    public Task<PaidDetailsDto> GetPaidDetailsAsync(string accessToken, string? yearId, int classId, string? groupId, string? sectionId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<PaidDetailsDto>(accessToken, $"api/sync/accounts/reports/paid?classId={classId}{Q("yearId", yearId)}{Q("groupId", groupId)}{Q("sectionId", sectionId)}{QDates(from, to)}", cancellationToken);

    public Task<MyAccountsDto> GetMyAccountsAsync(string accessToken, int regId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<MyAccountsDto>(accessToken, $"api/sync/accounts/reports/my?regId={regId}{QDates(from, to)}", cancellationToken);

    public Task<IReadOnlyList<AccountDetailDto>> GetAccountDetailsAsync(string accessToken, string? accountId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetListAsync<AccountDetailDto>(accessToken, $"api/sync/accounts/reports/account?accountId={Uri.EscapeDataString(accountId ?? "%")}{QDates(from, to)}", cancellationToken);

    public Task<AccountsLogDto> GetAccountsLogAsync(string accessToken, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<AccountsLogDto>(accessToken, $"api/sync/accounts/reports/log?{QDates(from, to).TrimStart('&')}", cancellationToken);

    public Task<IReadOnlyList<NameAmountDto>> GetReportIncomeCategoriesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<NameAmountDto>(accessToken, "api/sync/accounts/reports/income-categories", cancellationToken);

    public Task<IReadOnlyList<NameAmountDto>> GetReportExpenseCategoriesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<NameAmountDto>(accessToken, "api/sync/accounts/reports/expense-categories", cancellationToken);

    public Task<IReadOnlyList<NameAmountDto>> GetReportSectionsAsync(string accessToken, int classId, CancellationToken cancellationToken = default) =>
        GetListAsync<NameAmountDto>(accessToken, $"api/sync/accounts/reports/sections?classId={classId}", cancellationToken);

    public Task<IReadOnlyList<NameAmountDto>> GetReportGroupsAsync(string accessToken, int classId, CancellationToken cancellationToken = default) =>
        GetListAsync<NameAmountDto>(accessToken, $"api/sync/accounts/reports/groups?classId={classId}", cancellationToken);

    public Task<SessionFilterDto> GetSessionFiltersAsync(string accessToken, int yearId, int classId, string? roleId, string? kind, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<SessionFilterDto>(accessToken, $"api/sync/accounts/reports/session/filters?yearId={yearId}&classId={classId}{Q("roleId", roleId)}{Q("kind", kind)}{QDates(from, to)}", cancellationToken);

    public Task<SessionClassReportDto> GetSessionClassReportAsync(string accessToken, int yearId, DateTime? from, DateTime? to, int classId, int roleId, CancellationToken cancellationToken = default) =>
        GetItemAsync<SessionClassReportDto>(accessToken, $"api/sync/accounts/reports/session/class?yearId={yearId}&classId={classId}&roleId={roleId}{QDates(from, to)}", cancellationToken);

    public Task<SessionStudentReportDto> GetSessionStudentsAsync(string accessToken, int yearId, int classId, string? sectionId, string? roleId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<SessionStudentReportDto>(accessToken, $"api/sync/accounts/reports/session/students?yearId={yearId}&classId={classId}{Q("sectionId", sectionId)}{Q("roleId", roleId)}{QDates(from, to)}", cancellationToken);

    public Task<SessionStudentReportDto> GetSessionPaidAsync(string accessToken, int yearId, int classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<SessionStudentReportDto>(accessToken, $"api/sync/accounts/reports/session/paid?yearId={yearId}&classId={classId}{Q("sectionId", sectionId)}{Q("roleId", roleId)}{Q("payFor", payFor)}{QDates(from, to)}", cancellationToken);

    public Task<SessionStudentReportDto> GetSessionDueAsync(string accessToken, int yearId, int classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<SessionStudentReportDto>(accessToken, $"api/sync/accounts/reports/session/due?yearId={yearId}&classId={classId}{Q("sectionId", sectionId)}{Q("roleId", roleId)}{Q("payFor", payFor)}{QDates(from, to)}", cancellationToken);

    public Task<SessionStudentReportDto> GetSessionConcessionAsync(string accessToken, int yearId, int classId, string? sectionId, string? roleId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<SessionStudentReportDto>(accessToken, $"api/sync/accounts/reports/session/concession?yearId={yearId}&classId={classId}{Q("sectionId", sectionId)}{Q("roleId", roleId)}{QDates(from, to)}", cancellationToken);

    public Task<SessionPaidDueDto> GetSessionPaidDueAsync(string accessToken, string? status, string? classId, string? sectionId, string? roleId, string? payFor, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<SessionPaidDueDto>(accessToken, $"api/sync/accounts/reports/session/paid-due?{QDates(from, to).TrimStart('&')}{Q("status", status)}{Q("classId", classId)}{Q("sectionId", sectionId)}{Q("roleId", roleId)}{Q("payFor", payFor)}", cancellationToken);

    public Task<DashboardOverviewDto> GetDashboardOverviewAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<DashboardOverviewDto>(accessToken, "api/sync/dashboard/overview", cancellationToken);

    public Task<ExamFilterDto> GetExamFiltersAsync(string accessToken, string? kind, int classId = 0, int examId = 0, string? groupId = null, string? sectionId = null, string? shiftId = null, int subjectId = 0, CancellationToken cancellationToken = default) =>
        GetItemAsync<ExamFilterDto>(accessToken, $"api/sync/exam/filters?kind={Uri.EscapeDataString(kind ?? "")}&classId={classId}&examId={examId}&subjectId={subjectId}{Q("groupId", groupId)}{Q("sectionId", sectionId)}{Q("shiftId", shiftId)}", cancellationToken);

    public Task<IReadOnlyList<ExamNameDto>> GetExamNamesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<ExamNameDto>(accessToken, "api/sync/exam/names", cancellationToken);

    public Task<ExamResult> CreateExamNameAsync(string accessToken, SaveExamNameRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/names", request, cancellationToken);

    public Task<ExamResult> UpdateExamNameAsync(string accessToken, int examId, SaveExamNameRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, $"api/sync/exam/names/{examId}", request, cancellationToken);

    public Task<ExamResult> DeleteExamNameAsync(string accessToken, int examId, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, $"api/sync/exam/names/{examId}/delete", new { }, cancellationToken);

    public Task<IReadOnlyList<SubExamDto>> GetSubExamsAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<SubExamDto>(accessToken, "api/sync/exam/sub-exams", cancellationToken);

    public Task<ExamResult> CreateSubExamAsync(string accessToken, SaveSubExamRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/sub-exams", request, cancellationToken);

    public Task<ExamResult> UpdateSubExamAsync(string accessToken, int id, SaveSubExamRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, $"api/sync/exam/sub-exams/{id}", request, cancellationToken);

    public Task<ExamResult> DeleteSubExamAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, $"api/sync/exam/sub-exams/{id}/delete", new { }, cancellationToken);

    public Task<IReadOnlyList<GradeSystemDto>> GetExamGradingAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<GradeSystemDto>(accessToken, "api/sync/exam/grading", cancellationToken);

    public Task<ExamResult> CreateExamGradingAsync(string accessToken, SaveGradeSystemRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/grading", request, cancellationToken);

    public Task<ExamResult> RenameExamGradingAsync(string accessToken, int id, SaveGradeSystemRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, $"api/sync/exam/grading/{id}", request, cancellationToken);

    public Task<ExamResult> UpdateExamGradeCommentAsync(string accessToken, int gradingId, SaveGradeCommentRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, $"api/sync/exam/grading/{gradingId}/comment", request, cancellationToken);

    public Task<ExamResult> DeleteExamGradingAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, $"api/sync/exam/grading/{id}/delete", new { }, cancellationToken);

    public Task<IReadOnlyList<PassMarkRowDto>> GetExamPassMarksAsync(string accessToken, int classId, int examId, int subExamId, CancellationToken cancellationToken = default) =>
        GetListAsync<PassMarkRowDto>(accessToken, $"api/sync/exam/pass-marks?classId={classId}&examId={examId}&subExamId={subExamId}", cancellationToken);

    public Task<ExamResult> SaveExamPassMarksAsync(string accessToken, SavePassMarksRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/pass-marks", request, cancellationToken);

    public Task<DistSheetDto> GetExamDistributionAsync(string accessToken, int classId, int examId, CancellationToken cancellationToken = default) =>
        GetItemAsync<DistSheetDto>(accessToken, $"api/sync/exam/distribution?classId={classId}&examId={examId}", cancellationToken);

    public Task<ExamResult> SaveExamDistributionAsync(string accessToken, SaveDistributionRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/distribution", request, cancellationToken);

    public Task<ExamResult> CopyExamDistributionAsync(string accessToken, CopyDistributionRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/distribution/copy", request, cancellationToken);

    public Task<CollectPaperDto> GetExamCollectPaperAsync(string accessToken, int examId, int classId, int subjectId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken = default) =>
        GetItemAsync<CollectPaperDto>(accessToken, $"api/sync/exam/collect-paper?examId={examId}&classId={classId}&subjectId={subjectId}{Q("groupId", groupId)}{Q("sectionId", sectionId)}{Q("shiftId", shiftId)}", cancellationToken);

    public Task<InputSheetDto> GetExamInputSheetAsync(string accessToken, int examId, int classId, int subjectId, int subExamId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken = default) =>
        GetItemAsync<InputSheetDto>(accessToken, $"api/sync/exam/input?examId={examId}&classId={classId}&subjectId={subjectId}&subExamId={subExamId}{Q("groupId", groupId)}{Q("sectionId", sectionId)}{Q("shiftId", shiftId)}", cancellationToken);

    public Task<ExamResult> SaveExamInputMarksAsync(string accessToken, SaveInputMarksRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/input", request, cancellationToken);

    public Task<IReadOnlyList<MarksCheckRowDto>> GetExamMarksCheckAsync(string accessToken, int classId, int examId, CancellationToken cancellationToken = default) =>
        GetListAsync<MarksCheckRowDto>(accessToken, $"api/sync/exam/marks-check?classId={classId}&examId={examId}", cancellationToken);

    public Task<IReadOnlyList<ExamControlRowDto>> GetExamControlAsync(string accessToken, int examId, bool cumulative, CancellationToken cancellationToken = default) =>
        GetListAsync<ExamControlRowDto>(accessToken, $"api/sync/exam/control?examId={examId}&cumulative={cumulative}", cancellationToken);

    public Task<ExamResult> SaveExamControlAsync(string accessToken, SaveExamControlRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/control", request, cancellationToken);

    public Task<ExamPublishSettingDto> GetExamPublishSettingAsync(string accessToken, int classId, int examId, CancellationToken cancellationToken = default) =>
        GetItemAsync<ExamPublishSettingDto>(accessToken, $"api/sync/exam/publish?classId={classId}&examId={examId}", cancellationToken);

    public Task<ExamResult> PublishExamResultAsync(string accessToken, ExamPublishRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/publish", request, cancellationToken);

    public Task<ExamResult> DeleteExamResultAsync(string accessToken, ExamDeleteResultRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/delete-result", request, cancellationToken);

    public Task<ExamMeritListDto> GetExamMeritAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? passStatus, CancellationToken cancellationToken = default) =>
        GetItemAsync<ExamMeritListDto>(accessToken, $"api/sync/exam/merit?classId={classId}&examId={examId}{Q("groupId", groupId)}{Q("sectionId", sectionId)}{Q("shiftId", shiftId)}{Q("passStatus", passStatus)}", cancellationToken);

    public Task<ExamMeritListDto> GetExamMeritSubjectAsync(string accessToken, int classId, int examId, int subjectId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken = default) =>
        GetItemAsync<ExamMeritListDto>(accessToken, $"api/sync/exam/merit-subject?classId={classId}&examId={examId}&subjectId={subjectId}{Q("groupId", groupId)}{Q("sectionId", sectionId)}{Q("shiftId", shiftId)}", cancellationToken);

    public Task<ExamResultCardSheetDto> GetExamResultCardsAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, CancellationToken cancellationToken = default) =>
        GetItemAsync<ExamResultCardSheetDto>(accessToken, $"api/sync/exam/result-cards?classId={classId}&examId={examId}{Q("groupId", groupId)}{Q("sectionId", sectionId)}{Q("shiftId", shiftId)}{Q("studentIds", studentIds)}", cancellationToken);

    public Task<ExamAnalyticalDto> GetExamAnalyticalAsync(string accessToken, int classId, int examId, CancellationToken cancellationToken = default) =>
        GetItemAsync<ExamAnalyticalDto>(accessToken, $"api/sync/exam/analytical?classId={classId}&examId={examId}", cancellationToken);

    public Task<IReadOnlyList<ExamOptionDto>> GetCumulativeExamNamesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<ExamOptionDto>(accessToken, "api/sync/exam/cumulative/names", cancellationToken);

    public Task<ExamResult> CreateCumulativeExamNameAsync(string accessToken, SaveCumulativeNameRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/cumulative/names", request, cancellationToken);

    public Task<ExamResult> UpdateCumulativeExamNameAsync(string accessToken, int id, SaveCumulativeNameRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, $"api/sync/exam/cumulative/names/{id}", request, cancellationToken);

    public Task<CumulativePublishSettingDto> GetCumulativePublishSettingAsync(string accessToken, int classId, int examId, CancellationToken cancellationToken = default) =>
        GetItemAsync<CumulativePublishSettingDto>(accessToken, $"api/sync/exam/cumulative/publish?classId={classId}&examId={examId}", cancellationToken);

    public Task<ExamResult> PublishCumulativeResultAsync(string accessToken, CumulativePublishRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/cumulative/publish", request, cancellationToken);

    public Task<ExamMeritListDto> GetCumulativeMeritAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, CancellationToken cancellationToken = default) =>
        GetItemAsync<ExamMeritListDto>(accessToken, $"api/sync/exam/cumulative/merit?classId={classId}&examId={examId}{Q("groupId", groupId)}{Q("sectionId", sectionId)}{Q("shiftId", shiftId)}", cancellationToken);

    public Task<CumulativeResultCardSheetDto> GetCumulativeResultCardsAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, CancellationToken cancellationToken = default) =>
        GetItemAsync<CumulativeResultCardSheetDto>(accessToken, $"api/sync/exam/cumulative/result-cards?classId={classId}&examId={examId}{Q("groupId", groupId)}{Q("sectionId", sectionId)}{Q("shiftId", shiftId)}{Q("studentIds", studentIds)}", cancellationToken);

    public Task<ExamSeatPlanSheetDto> GetExamSeatPlanAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, string? classIds = null, CancellationToken cancellationToken = default) =>
        GetItemAsync<ExamSeatPlanSheetDto>(accessToken, $"api/sync/exam/seat-plan?classId={classId}&examId={examId}{Q("groupId", groupId)}{Q("sectionId", sectionId)}{Q("shiftId", shiftId)}{Q("studentIds", studentIds)}{Q("classIds", classIds)}", cancellationToken);

    public Task<ExamResult> RandomizeExamSeatsAsync(string accessToken, RandomSeatRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/seat-plan/random", request, cancellationToken);

    public Task<ExamAdmitCardSheetDto> GetExamAdmitCardsAsync(string accessToken, int classId, int examId, string? groupId, string? sectionId, string? shiftId, string? studentIds, string? paymentStatus, CancellationToken cancellationToken = default) =>
        GetItemAsync<ExamAdmitCardSheetDto>(accessToken, $"api/sync/exam/admit-cards?classId={classId}&examId={examId}{Q("groupId", groupId)}{Q("sectionId", sectionId)}{Q("shiftId", shiftId)}{Q("studentIds", studentIds)}{Q("paymentStatus", paymentStatus)}", cancellationToken);

    public Task<ExamResult> SaveExamAdmitSignAsync(string accessToken, SaveExamSignRequest request, CancellationToken cancellationToken = default) =>
        PostExamAsync(accessToken, "api/sync/exam/admit-sign", request, cancellationToken);

    public Task<SmsBalanceDto> GetSmsBalanceAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<SmsBalanceDto>(accessToken, "api/sync/sms/balance", cancellationToken);

    public Task<IReadOnlyList<SmsStudentDto>> GetSmsStudentsAsync(string accessToken, int classId, int groupId, int sectionId, int shiftId, string? ids, CancellationToken cancellationToken = default) =>
        GetListAsync<SmsStudentDto>(accessToken, $"api/sync/sms/students?classId={classId}&groupId={groupId}&sectionId={sectionId}&shiftId={shiftId}{Q("ids", ids)}", cancellationToken);

    public Task<IReadOnlyList<SmsTeacherDto>> GetSmsTeachersAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<SmsTeacherDto>(accessToken, "api/sync/sms/teachers", cancellationToken);

    public Task<SmsResult> SendOfficeSmsAsync(string accessToken, SendOfficeSmsRequest request, CancellationToken cancellationToken = default) =>
        PostSmsAsync(accessToken, "api/sync/sms/send", request, cancellationToken);

    public Task<IReadOnlyList<SmsGroupDto>> GetSmsGroupsAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<SmsGroupDto>(accessToken, "api/sync/sms/groups", cancellationToken);

    public Task<SmsResult> SaveSmsGroupAsync(string accessToken, SaveSmsGroupRequest request, CancellationToken cancellationToken = default) =>
        PostSmsAsync(accessToken, "api/sync/sms/groups", request, cancellationToken);

    public Task<SmsResult> DeleteSmsGroupAsync(string accessToken, int groupId, CancellationToken cancellationToken = default) =>
        PostSmsAsync(accessToken, $"api/sync/sms/groups/{groupId}/delete", new { }, cancellationToken);

    public Task<IReadOnlyList<SmsContactDto>> GetSmsContactsAsync(string accessToken, int groupId, string? search, CancellationToken cancellationToken = default) =>
        GetListAsync<SmsContactDto>(accessToken, $"api/sync/sms/contacts?groupId={groupId}{Q("q", search)}", cancellationToken);

    public Task<SmsResult> SaveSmsContactAsync(string accessToken, SaveSmsContactRequest request, CancellationToken cancellationToken = default) =>
        PostSmsAsync(accessToken, "api/sync/sms/contacts", request, cancellationToken);

    public Task<SmsResult> DeleteSmsContactAsync(string accessToken, int numberId, CancellationToken cancellationToken = default) =>
        PostSmsAsync(accessToken, $"api/sync/sms/contacts/{numberId}/delete", new { }, cancellationToken);

    public Task<SmsRecordsDto> GetSmsRecordsAsync(string accessToken, DateTime? from, DateTime? to, string? search, CancellationToken cancellationToken = default) =>
        GetItemAsync<SmsRecordsDto>(accessToken, $"api/sync/sms/records?{(QDates(from, to) + Q("q", search)).TrimStart('&')}", cancellationToken);

    public Task<SmsRechargePageDto> GetSmsRechargeAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<SmsRechargePageDto>(accessToken, "api/sync/sms/recharge", cancellationToken);

    public Task<SmsResult> StartSmsRechargeAsync(string accessToken, SmsRechargeRequest request, CancellationToken cancellationToken = default) =>
        PostSmsAsync(accessToken, "api/sync/sms/recharge", request, cancellationToken);

    public Task<IReadOnlyList<RoutineNameDto>> GetRoutineNamesAsync(string accessToken, bool unusedOnly, CancellationToken cancellationToken = default) =>
        GetListAsync<RoutineNameDto>(accessToken, $"api/sync/routine/names?unusedOnly={unusedOnly}", cancellationToken);

    public Task<RoutineResult> SaveRoutineNameAsync(string accessToken, SaveRoutineNameRequest request, CancellationToken cancellationToken = default) =>
        PostRoutineAsync(accessToken, "api/sync/routine/names", request, cancellationToken);

    public Task<RoutineResult> DeleteRoutineNameAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostRoutineAsync(accessToken, $"api/sync/routine/names/{id}/delete", new { }, cancellationToken);

    public Task<RoutineResult> CreateClassRoutineAsync(string accessToken, CreateClassRoutineRequest request, CancellationToken cancellationToken = default) =>
        PostRoutineAsync(accessToken, "api/sync/routine/create", request, cancellationToken);

    public Task<ClassRoutineSheetDto> GetRoutineAssignAsync(string accessToken, int classId, int groupId, int sectionId, int shiftId, int routineInfoId, CancellationToken cancellationToken = default) =>
        GetItemAsync<ClassRoutineSheetDto>(accessToken, $"api/sync/routine/assign?classId={classId}&groupId={groupId}&sectionId={sectionId}&shiftId={shiftId}&routineInfoId={routineInfoId}", cancellationToken);

    public Task<IReadOnlyList<RoutineOptionDto>> GetRoutineTeachersAsync(string accessToken, int classId, int subjectId, string day, string start, string end, int exceptRoutineInfoId, CancellationToken cancellationToken = default) =>
        GetListAsync<RoutineOptionDto>(accessToken, $"api/sync/routine/teachers?classId={classId}&subjectId={subjectId}&day={Uri.EscapeDataString(day ?? "")}&start={Uri.EscapeDataString(start ?? "")}&end={Uri.EscapeDataString(end ?? "")}&exceptRoutineInfoId={exceptRoutineInfoId}", cancellationToken);

    public Task<RoutineResult> AssignClassRoutineAsync(string accessToken, AssignRoutineRequest request, CancellationToken cancellationToken = default) =>
        PostRoutineAsync(accessToken, "api/sync/routine/assign", request, cancellationToken);

    public Task<ClassRoutineSheetDto> GetRoutineViewAsync(string accessToken, int classId, int groupId, int sectionId, int shiftId, int routineInfoId, bool edit, CancellationToken cancellationToken = default) =>
        GetItemAsync<ClassRoutineSheetDto>(accessToken, $"api/sync/routine/view?classId={classId}&groupId={groupId}&sectionId={sectionId}&shiftId={shiftId}&routineInfoId={routineInfoId}&edit={edit}", cancellationToken);

    public Task<RoutineResult> UpdateClassRoutineAsync(string accessToken, AssignRoutineRequest request, CancellationToken cancellationToken = default) =>
        PostRoutineAsync(accessToken, "api/sync/routine/update", request, cancellationToken);

    public Task<RoutineResult> DeleteClassRoutineAsync(string accessToken, AssignRoutineRequest request, CancellationToken cancellationToken = default) =>
        PostRoutineAsync(accessToken, "api/sync/routine/delete-class", request, cancellationToken);

    public Task<ExamRoutineSheetDto> GetExamRoutineAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        GetItemAsync<ExamRoutineSheetDto>(accessToken, $"api/sync/routine/exam?id={id}", cancellationToken);

    public Task<IReadOnlyList<RoutineOptionDto>> GetExamRoutineSubjectsAsync(string accessToken, int classId, CancellationToken cancellationToken = default) =>
        GetListAsync<RoutineOptionDto>(accessToken, $"api/sync/routine/exam/subjects?classId={classId}", cancellationToken);

    public Task<RoutineResult> SaveExamRoutineAsync(string accessToken, SaveExamRoutineRequest request, CancellationToken cancellationToken = default) =>
        PostRoutineAsync(accessToken, "api/sync/routine/exam", request, cancellationToken);

    public Task<RoutineResult> DeleteExamRoutineAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostRoutineAsync(accessToken, $"api/sync/routine/exam/{id}/delete", new { }, cancellationToken);

    public Task<CommitteeLookupsDto> GetCommitteeLookupsAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<CommitteeLookupsDto>(accessToken, "api/sync/committee/lookups", cancellationToken);

    public Task<IReadOnlyList<CommitteeMemberTypeDto>> GetCommitteeTypesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<CommitteeMemberTypeDto>(accessToken, "api/sync/committee/types", cancellationToken);

    public Task<CommitteeResult> SaveCommitteeTypeAsync(string accessToken, SaveCommitteeMemberTypeRequest request, CancellationToken cancellationToken = default) =>
        PostCommitteeAsync(accessToken, "api/sync/committee/types", request, cancellationToken);

    public Task<CommitteeResult> DeleteCommitteeTypeAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostCommitteeAsync(accessToken, $"api/sync/committee/types/{id}/delete", new { }, cancellationToken);

    public Task<IReadOnlyList<CommitteeMemberDto>> GetCommitteeMembersAsync(string accessToken, int typeId, string? q, CancellationToken cancellationToken = default) =>
        GetListAsync<CommitteeMemberDto>(accessToken, $"api/sync/committee/members?typeId={typeId}{Q("q", q)}", cancellationToken);

    public Task<CommitteeResult> SaveCommitteeMemberAsync(string accessToken, SaveCommitteeMemberRequest request, CancellationToken cancellationToken = default) =>
        PostCommitteeAsync(accessToken, "api/sync/committee/members", request, cancellationToken);

    public Task<IReadOnlyList<DonationCategoryDto>> GetDonationCategoriesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<DonationCategoryDto>(accessToken, "api/sync/committee/categories", cancellationToken);

    public Task<CommitteeResult> SaveDonationCategoryAsync(string accessToken, SaveDonationCategoryRequest request, CancellationToken cancellationToken = default) =>
        PostCommitteeAsync(accessToken, "api/sync/committee/categories", request, cancellationToken);

    public Task<CommitteeResult> DeleteDonationCategoryAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostCommitteeAsync(accessToken, $"api/sync/committee/categories/{id}/delete", new { }, cancellationToken);

    public Task<IReadOnlyList<DonorSuggestDto>> SuggestDonorsAsync(string accessToken, string? q, CancellationToken cancellationToken = default) =>
        GetListAsync<DonorSuggestDto>(accessToken, $"api/sync/committee/donors?q={Uri.EscapeDataString(q ?? "")}", cancellationToken);

    public Task<CommitteeResult> AddDonationAsync(string accessToken, AddDonationRequest request, CancellationToken cancellationToken = default) =>
        PostCommitteeAsync(accessToken, "api/sync/committee/donations", request, cancellationToken);

    public Task<DonationListDto> GetDonationsAsync(string accessToken, int memberId, int categoryId, string? paid, CancellationToken cancellationToken = default) =>
        GetItemAsync<DonationListDto>(accessToken, $"api/sync/committee/donations?memberId={memberId}&categoryId={categoryId}{Q("paid", paid)}", cancellationToken);

    public Task<CommitteeResult> UpdateDonationAsync(string accessToken, UpdateDonationRequest request, CancellationToken cancellationToken = default) =>
        PostCommitteeAsync(accessToken, "api/sync/committee/donations/update", request, cancellationToken);

    public Task<CommitteeResult> DeleteDonationAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostCommitteeAsync(accessToken, $"api/sync/committee/donations/{id}/delete", new { }, cancellationToken);

    public Task<CollectPageDto> GetCollectDonationAsync(string accessToken, int memberId, CancellationToken cancellationToken = default) =>
        GetItemAsync<CollectPageDto>(accessToken, $"api/sync/committee/collect?memberId={memberId}", cancellationToken);

    public Task<CommitteeResult> CollectDonationAsync(string accessToken, CollectDonationRequest request, CancellationToken cancellationToken = default) =>
        PostCommitteeAsync(accessToken, "api/sync/committee/collect", request, cancellationToken);

    public Task<PaymentRecordListDto> GetCommitteePaymentsAsync(string accessToken, int yearId, int categoryId, int memberId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<PaymentRecordListDto>(accessToken, $"api/sync/committee/payments?yearId={yearId}&categoryId={categoryId}&memberId={memberId}{QDates(from, to)}", cancellationToken);

    public Task<UnpaidReceiptDto> GetUnpaidReceiptAsync(string accessToken, string? sn, CancellationToken cancellationToken = default) =>
        GetItemAsync<UnpaidReceiptDto>(accessToken, $"api/sync/committee/unpaid?sn={Uri.EscapeDataString(sn ?? "")}", cancellationToken);

    public Task<CommitteeResult> UnpaidReceiptAsync(string accessToken, string sn, CancellationToken cancellationToken = default) =>
        PostCommitteeAsync(accessToken, "api/sync/committee/unpaid", new UnpaidReceiptRequest { Sn = sn }, cancellationToken);

    public Task<DonationReceiptDto> GetDonationReceiptAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        GetItemAsync<DonationReceiptDto>(accessToken, $"api/sync/committee/receipt/{id}", cancellationToken);

    public Task<DueInvoiceDto> GetDueInvoiceAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<DueInvoiceDto>(accessToken, "api/sync/invoice/due", cancellationToken);

    public Task<SubscriptionStatusDto> GetSubscriptionStatusAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<SubscriptionStatusDto>(accessToken, "api/sync/invoice/status", cancellationToken);

    public Task<InvoiceResult> PayDueInvoiceAsync(string accessToken, CancellationToken cancellationToken = default) =>
        PostInvoiceAsync(accessToken, "api/sync/invoice/pay", new { }, cancellationToken);

    public Task<PaidInvoiceListDto> GetPaidInvoicesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<PaidInvoiceListDto>(accessToken, "api/sync/invoice/paid", cancellationToken);

    public Task<PaidInvoiceReceiptDto> GetPaidInvoiceReceiptAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        GetItemAsync<PaidInvoiceReceiptDto>(accessToken, $"api/sync/invoice/receipt/{id}", cancellationToken);

    public Task<AuthorityDashboardDto> GetAuthorityDashboardAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthorityDashboardDto>(accessToken, "api/sync/authority/dashboard", cancellationToken);

    public Task<AuthorityDashboardDto> GetAuthorityInstitutionsAsync(
        string accessToken, string? q, string? validation, string? live, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthorityDashboardDto>(
            accessToken,
            $"api/sync/authority/institutions?q={Uri.EscapeDataString(q ?? "")}{Q("validation", validation)}{Q("live", live)}{QDates(from, to)}",
            cancellationToken);

    public async Task<LoginResponse> EnterAuthoritySchoolAsync(string accessToken, int schoolId, int educationYearId = 0, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/auth/enter-school")
        {
            Content = JsonContent.Create(new EnterSchoolRequest { SchoolID = schoolId, EducationYearID = educationYearId })
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions, cancellationToken);
        return payload ?? new LoginResponse { Succeeded = false, Error = "auth.fail" };
    }

    public Task<InstitutionDetailsDto> GetAuthorityInstitutionDetailsAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default) =>
        GetItemAsync<InstitutionDetailsDto>(accessToken, $"api/sync/authority/institutions/{schoolId}", cancellationToken);

    public Task<AuthorityResult> SaveAuthorityInstitutionYearsAsync(string accessToken, SaveInstitutionYearsRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/institutions/years", request, cancellationToken);

    public Task<AuthorityResult> RechargeAuthorityInstitutionSmsAsync(string accessToken, InstSmsRechargeRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/institutions/sms-recharge", request, cancellationToken);

    public Task<AuthorityResult> SaveAuthorityDueNoticeAsync(string accessToken, InstDueNoticeRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/institutions/due-notice", request, cancellationToken);

    public Task<InstStudentFindDto> FindAuthorityInstitutionStudentAsync(string accessToken, int schoolId, string id, CancellationToken cancellationToken = default) =>
        GetItemAsync<InstStudentFindDto>(accessToken, $"api/sync/authority/institutions/{schoolId}/student?q={Uri.EscapeDataString(id ?? "")}", cancellationToken);

    public Task<AuthorityResult> DeleteAuthorityInstitutionStudentAsync(string accessToken, InstIdRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/institutions/delete-student", request, cancellationToken);

    public Task<AuthorityResult> ChangeAuthorityInstitutionStudentIdAsync(string accessToken, InstChangeIdRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/institutions/change-student-id", request, cancellationToken);

    public Task<InstReceiptDto> FindAuthorityInstitutionReceiptAsync(string accessToken, int schoolId, string sn, CancellationToken cancellationToken = default) =>
        GetItemAsync<InstReceiptDto>(accessToken, $"api/sync/authority/institutions/{schoolId}/receipt?sn={Uri.EscapeDataString(sn ?? "")}", cancellationToken);

    public Task<AuthorityResult> DeleteAuthorityInstitutionReceiptAsync(string accessToken, InstReceiptRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/institutions/delete-receipt", request, cancellationToken);

    public Task<SignupLookupsDto> GetAuthoritySignupLookupsAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<SignupLookupsDto>(accessToken, "api/sync/authority/signup/lookups", cancellationToken);

    public Task<AuthorityResult> CreateAuthorityUserAsync(string accessToken, SignupUserRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/signup/user", request, cancellationToken);

    public Task<AuthorityResult> CreateAuthorityInstitutionAsync(string accessToken, SignupInstitutionRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/signup/institution", request, cancellationToken);

    public Task<UserInfoListDto> GetAuthorityUserInfoAsync(string accessToken, string? q, string? validation, string? password, CancellationToken cancellationToken = default) =>
        GetItemAsync<UserInfoListDto>(
            accessToken,
            $"api/sync/authority/user-info?q={Uri.EscapeDataString(q ?? "")}{Q("validation", validation)}{Q("password", password)}",
            cancellationToken);

    public Task<IReadOnlyList<SchoolUserDto>> GetAuthoritySchoolUsersAsync(string accessToken, int schoolId, string? category, CancellationToken cancellationToken = default) =>
        GetListAsync<SchoolUserDto>(
            accessToken,
            $"api/sync/authority/user-info/users?schoolId={schoolId}{Q("category", category)}",
            cancellationToken);

    public Task<AuthorityResult> SetAuthorityApprovedAsync(string accessToken, SetApprovedRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/user-info/approve", request, cancellationToken);

    public Task<AuthorityResult> UnlockAuthorityUserAsync(string accessToken, UnlockUserRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/user-info/unlock", request, cancellationToken);

    public Task<IReadOnlyList<TestimonialRowDto>> GetAuthorityTestimonialsAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<TestimonialRowDto>(accessToken, "api/sync/authority/testimonials", cancellationToken);

    public Task<AuthorityResult> SaveAuthorityTestimonialAsync(string accessToken, SaveTestimonialRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/testimonials", request, cancellationToken);

    public Task<AuthorityResult> SetAuthorityTestimonialShowAsync(string accessToken, SetTestimonialShowRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/testimonials/show", request, cancellationToken);

    public Task<IReadOnlyList<ResetSchoolOptionDto>> GetAuthorityResetSchoolsAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<ResetSchoolOptionDto>(accessToken, "api/sync/authority/reset/schools", cancellationToken);

    public Task<IReadOnlyList<ResetYearOptionDto>> GetAuthorityResetYearsAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default) =>
        GetListAsync<ResetYearOptionDto>(accessToken, $"api/sync/authority/reset/years?schoolId={schoolId}", cancellationToken);

    public Task<ResetPreviewDto> PreviewAuthorityResetAsync(string accessToken, int schoolId, string mode, int educationYearId, CancellationToken cancellationToken = default) =>
        GetItemAsync<ResetPreviewDto>(
            accessToken,
            $"api/sync/authority/reset/preview?schoolId={schoolId}&mode={Uri.EscapeDataString(mode)}&educationYearId={educationYearId}",
            cancellationToken);

    public Task<ResetPreviewDto> PreviewAuthorityResetImagesAsync(string accessToken, int schoolId, IReadOnlyList<int> yearIds, CancellationToken cancellationToken = default) =>
        GetItemAsync<ResetPreviewDto>(
            accessToken,
            $"api/sync/authority/reset/image-preview?schoolId={schoolId}&yearIds={Uri.EscapeDataString(string.Join(",", yearIds ?? []))}",
            cancellationToken);

    public Task<ResetProgressDto> GetAuthorityResetProgressAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default) =>
        GetItemAsync<ResetProgressDto>(accessToken, $"api/sync/authority/reset/progress?schoolId={schoolId}", cancellationToken);

    public Task<AuthorityResult> StartAuthorityResetAsync(string accessToken, ResetExecuteRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/reset/execute", request, cancellationToken);

    public async Task<ResetPreviewDto> DeleteAuthorityResetImagesAsync(string accessToken, ResetImageRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/authority/reset/delete-images")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ResetPreviewDto>(JsonOptions, cancellationToken)
               ?? new ResetPreviewDto { Ok = false, Message = "ab.failed" };
    }

    public Task<AttSignupPageDto> GetAuthorityAttendanceAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<AttSignupPageDto>(accessToken, "api/sync/authority/attendance", cancellationToken);

    public Task<AuthorityResult> RegisterAuthorityAttendanceAsync(string accessToken, AttRegisterRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/attendance/register", request, cancellationToken);

    public Task<AuthorityResult> SetAuthorityAttendancePasswordAsync(string accessToken, AttPasswordRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/attendance/password", request, cancellationToken);

    public Task<AuthorityResult> SetAuthorityAttendanceActiveAsync(string accessToken, AttActiveRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/attendance/active", request, cancellationToken);

    public Task<SmsSettingPageDto> GetAuthoritySmsSettingAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<SmsSettingPageDto>(accessToken, "api/sync/authority/sms-setting", cancellationToken);

    public Task<AuthorityResult> SaveAuthoritySmsSettingAsync(string accessToken, SaveSmsSettingRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/sms-setting", request, cancellationToken);

    public Task<IReadOnlyList<SmsSenderRowDto>> GetAuthoritySmsRecordsAsync(string accessToken, DateTime? from, DateTime? to, CancellationToken cancellationToken = default) =>
        GetListAsync<SmsSenderRowDto>(
            accessToken,
            $"api/sync/authority/sms-setting/records?{(QDates(from, to).TrimStart('&'))}",
            cancellationToken);

    public Task<SmsFailedPageDto> GetAuthorityFailedSmsAsync(string accessToken, DateTime? from, DateTime? to, string? reason, int schoolId, CancellationToken cancellationToken = default) =>
        GetItemAsync<SmsFailedPageDto>(
            accessToken,
            $"api/sync/authority/sms-setting/failed?{(QDates(from, to) + Q("reason", reason) + Q("schoolId", schoolId.ToString())).TrimStart('&')}",
            cancellationToken);

    public Task<ClientSmsPageDto> GetAuthorityClientSmsAsync(string accessToken, string? q, string? validation, CancellationToken cancellationToken = default) =>
        GetItemAsync<ClientSmsPageDto>(
            accessToken,
            $"api/sync/authority/client-sms?q={Uri.EscapeDataString(q ?? "")}{Q("validation", validation)}",
            cancellationToken);

    public async Task<SendClientSmsResult> SendAuthorityClientSmsAsync(string accessToken, SendClientSmsRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/authority/client-sms")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            var parsed = JsonSerializer.Deserialize<SendClientSmsResult>(body, JsonOptions);
            if (parsed is not null)
                return parsed;
        }
        catch (JsonException)
        {
        }
        return new SendClientSmsResult { Error = response.IsSuccessStatusCode ? "ab.smsFail" : body };
    }

    public Task<AuthAccountsPageDto> GetAuthorityAccountsAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthAccountsPageDto>(accessToken, "api/sync/authority/accounts", cancellationToken);

    public Task<AuthProgressPageDto> GetAuthorityProgressAsync(string accessToken, string? filter, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthProgressPageDto>(accessToken, $"api/sync/authority/progress?filter={Uri.EscapeDataString(filter ?? "%")}", cancellationToken);

    public Task<AuthCollectPageDto> GetAuthorityCollectionAsync(string accessToken, int categoryId, string? month, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthCollectPageDto>(accessToken, $"api/sync/authority/collection?categoryId={categoryId}{Q("month", month)}", cancellationToken);

    public Task<AuthManagePageDto> GetAuthorityManageAsync(string accessToken, string? q, string? validation, string? payment, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthManagePageDto>(
            accessToken,
            $"api/sync/authority/manage?q={Uri.EscapeDataString(q ?? "")}{Q("validation", validation)}{Q("payment", payment)}",
            cancellationToken);

    public Task<AuthorityResult> SaveAuthorityManageAsync(string accessToken, AuthManageSaveRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/manage", request, cancellationToken);

    public Task<AuthCreatePageDto> GetAuthorityCreateInvoiceAsync(
        string accessToken, string? month, int otherSchoolId, string? smsFrom, string? smsTo, string? smsQ, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthCreatePageDto>(
            accessToken,
            $"api/sync/authority/invoice/create?month={Uri.EscapeDataString(month ?? "")}&otherSchoolId={otherSchoolId}{Q("smsFrom", smsFrom)}{Q("smsTo", smsTo)}{Q("smsQ", smsQ)}",
            cancellationToken);

    public Task<AuthorityResult> GenerateAuthorityStudentCountAsync(string accessToken, AuthGenerateCountRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/invoice/generate-count", request, cancellationToken);

    public Task<AuthorityResult> AutoGenerateAuthorityInvoiceAsync(string accessToken, AuthGenerateCountRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/invoice/auto-generate", request, cancellationToken);

    public Task<AuthorityResult> EnableAuthorityInvoiceJobAsync(string accessToken, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/invoice/enable-job", new AuthIdRequest(), cancellationToken);

    public Task<AuthorityResult> CreateAuthorityServiceInvoicesAsync(string accessToken, AuthCreateServiceRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/invoice/service", request, cancellationToken);

    public Task<AuthorityResult> AddAuthorityInvoiceCategoryAsync(string accessToken, AuthAddCategoryRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/invoice/category", request, cancellationToken);

    public Task<AuthorityResult> CreateAuthorityOtherInvoiceAsync(string accessToken, AuthCreateOtherRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/invoice/other", request, cancellationToken);

    public Task<AuthorityResult> DeleteAuthorityInvoiceAsync(string accessToken, int invoiceId, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/invoice/other/delete", new AuthIdRequest { Id = invoiceId }, cancellationToken);

    public Task<AuthorityResult> SetAuthorityGraceAsync(string accessToken, AuthGraceRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/invoice/grace", request, cancellationToken);

    public Task<AuthorityResult> ClearAuthorityGraceAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/invoice/grace/clear", new AuthGraceRequest { SchoolID = schoolId }, cancellationToken);

    public Task<AuthPaidPageDto> GetAuthorityPaidInvoiceAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthPaidPageDto>(accessToken, $"api/sync/authority/invoice/paid?schoolId={schoolId}", cancellationToken);

    public Task<AuthorityResult> PayAuthorityInvoicesAsync(string accessToken, AuthPayInvoiceRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/invoice/pay", request, cancellationToken);

    public Task<AuthPrintPageDto> GetAuthorityPrintInvoiceAsync(string accessToken, int schoolId, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthPrintPageDto>(accessToken, $"api/sync/authority/invoice/print?schoolId={schoolId}", cancellationToken);

    public Task<AuthPayPrintDto> GetAuthorityPayPrintAsync(string accessToken, int schoolId, string ids, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthPayPrintDto>(accessToken, $"api/sync/authority/invoice/print/pay?schoolId={schoolId}&ids={Uri.EscapeDataString(ids)}", cancellationToken);

    public Task<AuthReceiptPrintDto> GetAuthorityReceiptPrintAsync(string accessToken, int receiptId, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthReceiptPrintDto>(accessToken, $"api/sync/authority/invoice/print/receipt?receiptId={receiptId}", cancellationToken);

    public Task<AuthOnlinePayPageDto> GetAuthorityOnlinePayAsync(
        string accessToken, string? type, int schoolId, string? method, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/authority/online-pay?type={Uri.EscapeDataString(type ?? "All")}&schoolId={schoolId}{Q("method", method)}";
        if (from is not null) url += $"&from={from:yyyy-MM-dd}";
        if (to is not null) url += $"&to={to:yyyy-MM-dd}";
        return GetItemAsync<AuthOnlinePayPageDto>(accessToken, url, cancellationToken);
    }

    public Task<AuthLinkTreeDto> GetAuthorityLinksAsync(string accessToken, int categoryId, int subId, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthLinkTreeDto>(accessToken, $"api/sync/authority/links?categoryId={categoryId}&subId={subId}", cancellationToken);

    public Task<AuthorityResult> SaveAuthorityLinkCategoryAsync(string accessToken, AuthLinkNameSaveRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/links/category", request, cancellationToken);

    public Task<AuthorityResult> DeleteAuthorityLinkCategoryAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/links/category/delete", new AuthIdRequest { Id = id }, cancellationToken);

    public Task<AuthorityResult> SaveAuthorityLinkSubAsync(string accessToken, AuthLinkNameSaveRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/links/sub", request, cancellationToken);

    public Task<AuthorityResult> DeleteAuthorityLinkSubAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/links/sub/delete", new AuthIdRequest { Id = id }, cancellationToken);

    public Task<AuthorityResult> SaveAuthorityLinkPageAsync(string accessToken, AuthLinkPageSaveRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/links/page", request, cancellationToken);

    public Task<AuthorityResult> DeleteAuthorityLinkPageAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/links/page/delete", new AuthIdRequest { Id = id }, cancellationToken);

    public Task<AuthRoleListDto> GetAuthorityRolesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthRoleListDto>(accessToken, "api/sync/authority/roles", cancellationToken);

    public Task<AuthorityResult> CreateAuthorityRoleAsync(string accessToken, AuthRoleSaveRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/roles", request, cancellationToken);

    public Task<AuthorityResult> DeleteAuthorityRoleAsync(string accessToken, AuthRoleSaveRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/roles/delete", request, cancellationToken);

    public Task<AuthReferralPageDto> GetAuthorityReferralAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthReferralPageDto>(accessToken, $"api/sync/authority/reference?id={id}", cancellationToken);

    public Task<AuthorityResult> SaveAuthorityReferrerAsync(string accessToken, AuthReferrerSaveRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/reference", request, cancellationToken);

    public Task<AuthSchoolSearchPageDto> SearchAuthorityReferralSchoolsAsync(string accessToken, string? q, int refId, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthSchoolSearchPageDto>(accessToken, $"api/sync/authority/reference/schools?q={Uri.EscapeDataString(q ?? "")}&refId={refId}", cancellationToken);

    public Task<AuthorityResult> AssignAuthoritySchoolAsync(string accessToken, AuthAssignSchoolRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/reference/assign", request, cancellationToken);

    public Task<AuthorityResult> UpdateAuthorityAssignAsync(string accessToken, AuthAssignUpdateRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/reference/assign/update", request, cancellationToken);

    public Task<AuthorityResult> DeleteAuthorityAssignAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/reference/assign/delete", new AuthIdRequest { Id = id }, cancellationToken);

    public Task<AuthCommissionPageDto> GetAuthorityCommissionAsync(
        string accessToken, int refId, DateTime? from, DateTime? to, string? status, int detailId, CancellationToken cancellationToken = default)
    {
        var url = $"api/sync/authority/commission?refId={refId}&status={Uri.EscapeDataString(status ?? "")}&detailId={detailId}";
        if (from is not null) url += $"&from={from:yyyy-MM-dd}";
        if (to is not null) url += $"&to={to:yyyy-MM-dd}";
        return GetItemAsync<AuthCommissionPageDto>(accessToken, url, cancellationToken);
    }

    public Task<AuthorityResult> PayAuthorityCommissionAsync(string accessToken, AuthCommissionPayRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/commission/pay", request, cancellationToken);

    public Task<AuthorityResult> CreateAuthoritySubAsync(string accessToken, AuthSubSignupRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/sub-authority", request, cancellationToken);

    public Task<AuthAccessPageDto> GetAuthorityPageAccessAsync(string accessToken, string? userName, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthAccessPageDto>(accessToken, $"api/sync/authority/page-access?userName={Uri.EscapeDataString(userName ?? "")}", cancellationToken);

    public Task<AuthorityResult> SaveAuthorityPageAccessAsync(string accessToken, AuthAccessSaveRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/page-access", request, cancellationToken);

    public Task<AuthProfileDto> GetAuthorityProfileAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthProfileDto>(accessToken, "api/sync/authority/profile", cancellationToken);

    public async Task<ProfileResult> SaveAuthorityProfileAsync(string accessToken, AuthProfileDto request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "api/sync/authority/profile")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ProfileResult>(JsonOptions, cancellationToken)
               ?? new ProfileResult { Error = "profile.needOnline" };
    }

    public Task<IReadOnlyList<AuthNoticeDto>> GetAdminNoticesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<AuthNoticeDto>(accessToken, "api/sync/admin-notices", cancellationToken);

    public Task<IReadOnlyList<AuthNoticeDto>> GetAuthorityNoticesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetListAsync<AuthNoticeDto>(accessToken, "api/sync/authority/notices", cancellationToken);

    public Task<AuthorityResult> SaveAuthorityNoticeAsync(string accessToken, AuthNoticeSaveRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/notices", request, cancellationToken);

    public Task<AuthorityResult> DeleteAuthorityNoticeAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/notices/delete", new AuthNoticeIdRequest { Id = id }, cancellationToken);

    public Task<AuthUnreadDto> GetAuthorityUnreadAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthUnreadDto>(accessToken, "api/sync/authority/messages/unread", cancellationToken);

    public Task<AuthMessagePageDto> GetAuthorityMessagesAsync(string accessToken, CancellationToken cancellationToken = default) =>
        GetItemAsync<AuthMessagePageDto>(accessToken, "api/sync/authority/messages", cancellationToken);

    public Task<AuthorityResult> ReadAuthorityMessageAsync(string accessToken, AuthMessageReadRequest request, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/messages/read", request, cancellationToken);

    public Task<AuthorityResult> DeleteAuthorityContactAsync(string accessToken, int id, CancellationToken cancellationToken = default) =>
        PostAuthAsync(accessToken, "api/sync/authority/messages/delete-contact", new AuthNoticeIdRequest { Id = id }, cancellationToken);

    private async Task<AuthorityResult> PostAuthAsync<T>(
        string accessToken, string url, T request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            var parsed = JsonSerializer.Deserialize<AuthorityResult>(body, JsonOptions);
            if (parsed is not null)
            {
                if (!response.IsSuccessStatusCode)
                    parsed.Succeeded = false;
                return parsed;
            }
        }
        catch (JsonException)
        {
        }
        return new AuthorityResult { Error = response.IsSuccessStatusCode ? "ab.failed" : body };
    }

    private async Task<InvoiceResult> PostInvoiceAsync<T>(
        string accessToken, string url, T request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            var parsed = JsonSerializer.Deserialize<InvoiceResult>(body, JsonOptions);
            if (parsed is not null)
            {
                if (!response.IsSuccessStatusCode)
                    parsed.Succeeded = false;
                return parsed;
            }
        }
        catch (JsonException)
        {
        }
        return new InvoiceResult { Error = response.IsSuccessStatusCode ? "inv.payFail" : body };
    }

    private async Task<CommitteeResult> PostCommitteeAsync<T>(
        string accessToken, string url, T request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            var parsed = JsonSerializer.Deserialize<CommitteeResult>(body, JsonOptions);
            if (parsed is not null)
            {
                if (!response.IsSuccessStatusCode)
                    parsed.Succeeded = false;
                return parsed;
            }
        }
        catch (JsonException)
        {
        }
        return new CommitteeResult { Error = response.IsSuccessStatusCode ? "cm.fail" : body };
    }

    private async Task<RoutineResult> PostRoutineAsync<T>(
        string accessToken, string url, T request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        try
        {
            var parsed = JsonSerializer.Deserialize<RoutineResult>(body, JsonOptions);
            if (parsed is not null)
            {
                if (!response.IsSuccessStatusCode)
                    parsed.Succeeded = false;
                return parsed;
            }
        }
        catch (JsonException)
        {
        }
        return new RoutineResult { Error = response.IsSuccessStatusCode ? "rt.fail" : body };
    }

    private async Task<SmsResult> PostSmsAsync<T>(
        string accessToken, string url, T request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        SmsResult? parsed = null;
        try
        {
            parsed = JsonSerializer.Deserialize<SmsResult>(body, JsonOptions);
        }
        catch (JsonException)
        {
        }
        if (parsed is not null)
        {
            if (!response.IsSuccessStatusCode)
            {
                parsed.Succeeded = false;
                if (string.IsNullOrWhiteSpace(parsed.Error))
                    parsed.Error = string.IsNullOrWhiteSpace(parsed.Message) ? "sms.fail" : parsed.Message;
            }
            return parsed;
        }
        return new SmsResult
        {
            Succeeded = false,
            Error = response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(body) ? "sms.fail" : body.Trim()
        };
    }

    public async Task<ExpenseDto?> GetExpenseOneAsync(string accessToken, int id, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, $"api/sync/accounts/expense/{id}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ExpenseDto>(JsonOptions, cancellationToken);
    }

    private async Task<AccountsResult> PostAccountsAsync<T>(
        string accessToken, string url, T request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        try
        {
            var parsed = await response.Content.ReadFromJsonAsync<AccountsResult>(JsonOptions, cancellationToken);
            if (parsed is not null)
                return parsed;
        }
        catch (JsonException)
        {
        }
        return new AccountsResult { Error = "acc.failed" };
    }

    private static string QDates(DateTime? from, DateTime? to)
    {
        var q = "";
        if (from is not null) q += $"&from={from:yyyy-MM-dd}";
        if (to is not null) q += $"&to={to:yyyy-MM-dd}";
        return q;
    }

    private static string Q(string name, string? value) =>
        string.IsNullOrWhiteSpace(value) ? "" : $"&{name}={Uri.EscapeDataString(value.Trim())}";

    private async Task<T> GetItemAsync<T>(string accessToken, string url, CancellationToken cancellationToken) where T : new()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(ExtractApiError(body) ?? $"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).");
        }
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken) ?? new T();
    }

    private static string? ExtractApiError(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String)
            {
                var detail = err.GetString();
                if (!string.IsNullOrWhiteSpace(detail)) return detail;
            }
        }
        catch (JsonException)
        {
        }
        return null;
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(
        string accessToken, string url, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, cancellationToken) ?? [];
    }

    private async Task<IReadOnlyList<T>> GetListPostAsync<T>(
        string accessToken, string url, object request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions, cancellationToken) ?? [];
    }

    private async Task<ExamResult> PostExamAsync<T>(
        string accessToken, string url, T request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String)
                {
                    var detail = err.GetString();
                    if (!string.IsNullOrWhiteSpace(detail))
                        return new ExamResult { Succeeded = false, Error = detail };
                }
            }
            catch (JsonException)
            {
            }
            return new ExamResult { Succeeded = false, Error = "exam.failed" };
        }
        return JsonSerializer.Deserialize<ExamResult>(body, JsonOptions)
               ?? new ExamResult { Succeeded = false, Error = "exam.failed" };
    }

    private async Task<AttendanceResult> PostAttendanceAsync<T>(
        string accessToken, string url, T request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return new AttendanceResult { Succeeded = false, Error = "att.failed" };
        return await response.Content.ReadFromJsonAsync<AttendanceResult>(JsonOptions, cancellationToken)
               ?? new AttendanceResult { Succeeded = false, Error = "att.failed" };
    }

    private async Task<AttendanceDownloadResult> DownloadAttendanceFileAsync(
        string accessToken, string url, string notFoundError, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return new AttendanceDownloadResult { Error = notFoundError };
        if (!response.IsSuccessStatusCode)
            return new AttendanceDownloadResult { Error = "att.dlFailed" };
        var name = response.Content.Headers.ContentDisposition?.FileNameStar
                   ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
                   ?? Path.GetFileName(url);
        return new AttendanceDownloadResult
        {
            Succeeded = true,
            FileName = string.IsNullOrWhiteSpace(name) ? "download" : name,
            ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream",
            Content = await response.Content.ReadAsByteArrayAsync(cancellationToken)
        };
    }

    private async Task<StudentInfoResult> PostStudentInfoAsync<T>(
        string accessToken, string url, T body, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Http().SendAsync(message, cancellationToken);
        return await ReadStudentInfoResultAsync(response, cancellationToken);
    }

    private async Task<StudentInfoResult> ReadStudentInfoResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            return new StudentInfoResult { Succeeded = false, Error = "si.failed" };
        return await response.Content.ReadFromJsonAsync<StudentInfoResult>(JsonOptions, cancellationToken)
               ?? new StudentInfoResult { Succeeded = false, Error = "si.failed" };
    }

    private async Task<SalaryResult> ReadSalaryResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            return new SalaryResult { Succeeded = false, Error = "sal.failed" };
        return await response.Content.ReadFromJsonAsync<SalaryResult>(JsonOptions, cancellationToken)
               ?? new SalaryResult { Succeeded = false, Error = "sal.failed" };
    }

    private async Task<EmployeeResult> ReadEmployeeResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            return new EmployeeResult { Succeeded = false, Error = "emp.failed" };
        return await response.Content.ReadFromJsonAsync<EmployeeResult>(JsonOptions, cancellationToken)
               ?? new EmployeeResult { Succeeded = false, Error = "emp.failed" };
    }

    private async Task<EducationYearResult> ReadYearResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            return new EducationYearResult { Succeeded = false, Error = "sess.failed" };
        return await response.Content.ReadFromJsonAsync<EducationYearResult>(JsonOptions, cancellationToken)
               ?? new EducationYearResult { Succeeded = false, Error = "sess.failed" };
    }

    private async Task<HolidayResult> ReadHolidayResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            return new HolidayResult { Succeeded = false, Error = "cal.failed" };
        return await response.Content.ReadFromJsonAsync<HolidayResult>(JsonOptions, cancellationToken)
               ?? new HolidayResult { Succeeded = false, Error = "cal.failed" };
    }

    private async Task<SubjectResult> ReadSubjectResultAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            return new SubjectResult { Succeeded = false, Error = "subj.failed" };
        return await response.Content.ReadFromJsonAsync<SubjectResult>(JsonOptions, cancellationToken)
               ?? new SubjectResult { Succeeded = false, Error = "subj.failed" };
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var detail = TryReadError(body);
        throw new HttpRequestException(
            string.IsNullOrWhiteSpace(detail)
                ? $"Sync API returned {(int)response.StatusCode} ({response.ReasonPhrase})."
                : detail,
            null,
            response.StatusCode);
    }

    private static string? TryReadError(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
                return error.GetString();
            if (doc.RootElement.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                return title.GetString();
        }
        catch (JsonException)
        {
        }

        var trimmed = body.Trim();
        return trimmed.Length is > 0 and < 400 ? trimmed : null;
    }

    private HttpClient Http() => _httpFactory.CreateClient(HttpClientName);
}
