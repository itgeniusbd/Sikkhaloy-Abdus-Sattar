namespace Sikkhaloy.Shared.Menu;

public sealed class MenuTreeDto
{
    public List<MenuCategoryDto> Categories { get; set; } = [];
}

public sealed class MenuCategoryDto
{
    public int CategoryID { get; set; }
    public string Name { get; set; } = "";
    public int Sort { get; set; }
    public List<MenuSubDto> Subs { get; set; } = [];
    public List<MenuLinkDto> Links { get; set; } = [];
}

public sealed class MenuSubDto
{
    public int SubCategoryID { get; set; }
    public string Name { get; set; } = "";
    public int Sort { get; set; }
    public List<MenuLinkDto> Links { get; set; } = [];
}

public sealed class MenuLinkDto
{
    public int LinkID { get; set; }
    public string Title { get; set; } = "";
    public string PageUrl { get; set; } = "";
    public int Sort { get; set; }
    public string Route { get; set; } = "";
    public bool Ready { get; set; }
}

public static class HybridMenuRoutes
{
    public static void Apply(MenuLinkDto link)
    {
        var url = (link.PageUrl ?? "").Replace("~", "", StringComparison.Ordinal).Trim();
        var lower = url.ToLowerInvariant();

        if (lower.Contains("create_sub_admin.aspx") || lower.Contains("signup_subadmin.aspx"))
        {
            link.Route = "/basic-settings/create-sub-admin";
            link.Ready = true;
            return;
        }

        if (lower.Contains("manage_sub_admin_access.aspx"))
        {
            link.Route = "/basic-settings/page-access";
            link.Ready = true;
            return;
        }

        if (lower.Contains("active_deactivate_sub_admin.aspx"))
        {
            link.Route = "/basic-settings/sub-admin-status";
            link.Ready = true;
            return;
        }

        if (lower.Contains("specify_group_section_shift_for_classes.aspx"))
        {
            link.Route = "/basic-settings/specify-join";
            link.Ready = true;
            return;
        }

        if (lower.Contains("create_group_section_shift_for_class.aspx"))
        {
            link.Route = "/basic-settings/class-structure";
            link.Ready = true;
            return;
        }

        if (lower.Contains("create_edit_delete_subjects.aspx"))
        {
            link.Route = "/basic-settings/subjects";
            link.Ready = true;
            return;
        }

        var title = (link.Title ?? "").Trim();
        if (lower.Contains("assigning_subject_in_classes.aspx")
            || title.Equals("Assign subjects in class", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Assign Subject In Class", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Assigning in Class", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/basic-settings/assign-subjects";
            link.Ready = true;
            return;
        }

        if (lower.Contains("institution_info.aspx")
            || title.Equals("Institution Information", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Institution Info", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/basic-settings/institution";
            link.Ready = true;
            return;
        }

        if (lower.Contains("acadamic_calender.aspx")
            || lower.Contains("academic_calendar.aspx")
            || lower.Contains("add_holidays.aspx")
            || title.Equals("Academic Calendar", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/basic-settings/academic-calendar";
            link.Ready = true;
            return;
        }

        if (lower.Contains("education_year.aspx")
            || title.Equals("Create Session", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Session Year", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/basic-settings/session";
            link.Ready = true;
            return;
        }

        if (lower.Contains("create_teacher.aspx")
            || title.Equals("Add Teacher", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Signup Teacher", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/teachers/new";
            link.Ready = true;
            return;
        }

        if (lower.Contains("staff_info.aspx")
            || title.Equals("Add Staff", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Add Staff Info", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/employees/new";
            link.Ready = true;
            return;
        }

        if (lower.Contains("deactivated_employee_list.aspx")
            || title.Equals("Deactivated Employee List", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/employees/deactivated";
            link.Ready = true;
            return;
        }

        if (lower.Contains("employee_list.aspx")
            || title.Equals("Employee List", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/employees";
            link.Ready = true;
            return;
        }

        if (lower.Contains("active_deactivate_teacher.aspx")
            || title.Equals("Active/Deactivate Teacher", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Manage Teacher", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/teachers/access";
            link.Ready = true;
            return;
        }

        if (lower.Contains("teacher_allocated_subjects.aspx")
            || title.Equals("Teacher Subject Assign", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Subjects Allocated For Teacher", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/teachers/subjects";
            link.Ready = true;
            return;
        }

        if (lower.Contains("employee/id_cards.aspx")
            || lower.Contains("/id_cards.aspx")
            || title.Equals("Employee ID Card", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Employee ID Cards", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/id-cards";
            link.Ready = true;
            return;
        }

        if (lower.Contains("employee_allowance.aspx")
            || title.Equals("Allowance", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/salary/allowance";
            link.Ready = true;
            return;
        }

        if (lower.Contains("employee_deduction.aspx")
            || title.Equals("Deduction", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/salary/deduction";
            link.Ready = true;
            return;
        }

        if (lower.Contains("employee_bonus_and_fine.aspx")
            || title.Equals("Bonus & Fine", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Bonus and Fine", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/salary/bonus-fine";
            link.Ready = true;
            return;
        }

        if (lower.Contains("payorder_monthly.aspx")
            || lower.Contains("setemployee_with_payordername.aspx")
            || title.Equals("Pay Order Monthly", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Employee Monthly Payorder", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/salary/payorder";
            link.Ready = true;
            return;
        }

        if (lower.Contains("salary_sheet.aspx")
            || title.Equals("Salary Sheet", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/salary/sheet";
            link.Ready = true;
            return;
        }

        if (lower.Contains("salary_payment.aspx")
            || title.Equals("Salary Payment", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/salary/payment";
            link.Ready = true;
            return;
        }

        if (lower.Contains("salary_paiddue_report.aspx")
            || title.Equals("Paid & Due Salary", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Salary Paid Due Report", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/staff/salary/paid-due";
            link.Ready = true;
            return;
        }

        if (lower.Contains("create_student_username_password.aspx")
            || title.Equals("Signup / Userid", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Student Login Management", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-info/signup";
            link.Ready = true;
            return;
        }

        if (lower.Contains("blocked_unblocked_student_temporarily.aspx")
            || title.Equals("Blocked,Unblocked,Delete", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Blocked Unblocked Student", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-info/accounts";
            link.Ready = true;
            return;
        }

        if (lower.Contains("find_students.aspx")
            || lower.Contains("students_list.aspx")
            || lower.Contains("total_student_list.aspx")
            || title.Equals("Student List", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Class Based Student List", StringComparison.OrdinalIgnoreCase)
            || title.Equals("All Student List", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/students";
            link.Ready = true;
            link.Title = "Student List";
            return;
        }

        if (lower.Contains("all_id_cards.aspx")
            || lower.Contains("student_id_cards.aspx")
            || title.Equals("Student ID Cards", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-info/id-cards";
            link.Ready = true;
            return;
        }

        if (lower.Contains("change_student_rollno_group_section_shift.aspx")
            || title.Equals("Change Roll No,Group,Section,Shift", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Change Roll No/Group/Section/Shift", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-info/placement";
            link.Ready = true;
            return;
        }

        if (lower.Contains("change_student_subjects.aspx")
            || title.Equals("Change Subject", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Change Student's Subjects", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-info/subjects";
            link.Ready = true;
            return;
        }

        if (lower.Contains("character_certificate.aspx")
            || title.Equals("All Certificate", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Character Certificate", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-info/certificates";
            link.Ready = true;
            return;
        }

        if (lower.Contains("list_of_students.aspx")
            || lower.Contains("change_class_and_subjects.aspx")
            || title.Equals("Class Change In Current Session", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Class Change, In Current Session", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Class Change", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Change Class", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-mgmt/class-change";
            link.Ready = true;
            return;
        }

        if (lower.Contains("change_section_shift_group.aspx")
            || title.Equals("Change Section,Shift,Group", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Change Section/Shift/Group", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-mgmt/group-section-shift";
            link.Ready = true;
            return;
        }

        if (lower.Contains("studentsubjects.aspx")
            || title.Equals("Update Subjects", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Student Subjects", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-mgmt/subjects";
            link.Ready = true;
            return;
        }

        if (lower.Contains("find_student.aspx")
            || lower.Contains("edit_student_information.aspx")
            || title.Equals("Edit Information", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Edit Student Info", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-mgmt/edit";
            link.Ready = true;
            return;
        }

        if (lower.Contains("reject_student_from_school.aspx")
            || lower.Contains("print_tc.aspx")
            || title.Equals("T.C", StringComparison.OrdinalIgnoreCase)
            || title.Equals("TC", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Transfer Certificate", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-mgmt/tc";
            link.Ready = true;
            return;
        }

        if (lower.Contains("classbasednotice.aspx")
            || title.Equals("HW/Notice", StringComparison.OrdinalIgnoreCase)
            || title.Equals("HW / Notice", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Class Based Notice", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/student-mgmt/notice";
            link.Ready = true;
            return;
        }

        if (lower.Contains("admission_new_student.aspx"))
        {
            link.Route = "/students/new";
            link.Ready = true;
            return;
        }

        if (lower.Contains("re_admission/students.aspx")
            || title.Equals("Single Re-Admission", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/admission/re-admission";
            link.Ready = true;
            return;
        }

        if (lower.Contains("multiple_re_admission.aspx")
            || title.Equals("Multiple Re-Admission", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/admission/multiple-re-admission";
            link.Ready = true;
            return;
        }

        if (lower.Contains("online_admission_form.aspx")
            || title.Equals("Print Admission Form", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/admission/print-form";
            link.Ready = true;
            return;
        }

        if (lower.Contains("schedule_assignstudent.aspx")
            || (lower.Contains("attendances/") && title.Equals("RFID Number Input", StringComparison.OrdinalIgnoreCase)))
        {
            link.Route = "/attendance/student/rfid";
            link.Ready = true;
            return;
        }

        if (lower.Contains("students_attendance.aspx")
            || (title.Equals("Manual Attendance", StringComparison.OrdinalIgnoreCase)
                && !lower.Contains("employee")))
        {
            link.Route = "/attendance/student/manual";
            link.Ready = true;
            return;
        }

        if (lower.Contains("attendance_records.aspx")
            || title.Equals("Attendance Records", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/attendance/student/records";
            link.Ready = true;
            return;
        }

        if (lower.Contains("leave_for_student.aspx")
            || (title.Equals("Leave", StringComparison.OrdinalIgnoreCase) && lower.Contains("attendances/")))
        {
            link.Route = "/attendance/student/leave";
            link.Ready = true;
            return;
        }

        if (lower.Contains("attendance_fine_generate.aspx")
            || title.Equals("Attendance Fine Generate", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/attendance/student/fine";
            link.Ready = true;
            return;
        }

        if (lower.Contains("employee/attendance_schedule.aspx")
            || (lower.Contains("employee/") && title.Equals("RFID Number Input", StringComparison.OrdinalIgnoreCase)))
        {
            link.Route = "/attendance/employee/rfid";
            link.Ready = true;
            return;
        }

        if (lower.Contains("employee_leave.aspx")
            || (lower.Contains("employee/") && title.Equals("Leave", StringComparison.OrdinalIgnoreCase)))
        {
            link.Route = "/attendance/employee/leave";
            link.Ready = true;
            return;
        }

        if (lower.Contains("employee_attendance_record.aspx")
            || title.Equals("Attendance Record", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/attendance/employee/records";
            link.Ready = true;
            return;
        }

        if (lower.Contains("employee_attendance.aspx")
            || (lower.Contains("employee/") && title.Equals("Manual Attendance", StringComparison.OrdinalIgnoreCase)))
        {
            link.Route = "/attendance/employee/manual";
            link.Ready = true;
            return;
        }

        if (lower.Contains("absence_fee_manage.aspx")
            || title.Equals("Create Attendance Schedule", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Attendance Schedule", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/attendance/schedule";
            link.Ready = true;
            return;
        }

        if (lower.Contains("attendance_settings.aspx")
            || title.Equals("Attendance Settings", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/attendance/settings";
            link.Ready = true;
            return;
        }

        if (lower.Contains("leave_report.aspx")
            || title.Equals("Leave Report", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/attendance/leave-report";
            link.Ready = true;
            return;
        }

        if (lower.Contains("create_payment_roles.aspx")
            || title.Equals("Add Payment Roles", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Add Payment Role", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/roles";
            link.Ready = true;
            return;
        }

        if (lower.Contains("assign_pay_role_multi_class.aspx")
            || title.Equals("Assign Pay Role Multi Class", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Assign Payment Role By Multi Class", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/assign-roles-multi";
            link.Ready = true;
            return;
        }

        if (lower.Contains("assign_payment_roles.aspx")
            || title.Equals("Assign Payment Roles", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/assign-roles";
            link.Ready = true;
            return;
        }

        if (lower.Contains("remove_pay_order.aspx")
            || title.Equals("Remove Pay order", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Remove Pay Order", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/remove-pay-order";
            link.Ready = true;
            return;
        }

        if (lower.Contains("pay_order.aspx")
            || title.Equals("Pay Order", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/pay-order";
            link.Ready = true;
            return;
        }

        if (lower.Contains("change_payorder_date.aspx")
            || title.Equals("Change Pay order Date", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Change Payorder Date", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/change-date";
            link.Ready = true;
            return;
        }

        if (lower.Contains("payment_check_by_money_receipt.aspx")
            || title.Equals("Money Receipt Check", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/receipt-check";
            link.Ready = true;
            return;
        }

        if (lower.Contains("deposit_withdraw.aspx")
            || title.Equals("Account Management", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/cash";
            link.Ready = true;
            return;
        }

        if (lower.Contains("unpaid_money_receipt.aspx")
            || title.Equals("Unpaid Money Receipt", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/unpaid-receipt";
            link.Ready = true;
            return;
        }

        if (lower.Contains("payment_collection_by_date.aspx")
            || title.Equals("Payment Collection By Date", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/collect-by-date";
            link.Ready = true;
            return;
        }

        if (lower.Contains("payment_collection.aspx")
            || title.Equals("Payment Collection", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/collect";
            link.Ready = true;
            return;
        }

        if (lower.Contains("payment_concession_all.aspx")
            || title.Equals("Fee Concession", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/concession";
            link.Ready = true;
            return;
        }

        if (lower.Contains("others_payment.aspx")
            || title.Equals("Others Income", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Other Income", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/others";
            link.Ready = true;
            return;
        }

        if (lower.Contains("edit_expense.aspx")
            || title.Equals("Edit Expense", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/expense-edit";
            link.Ready = true;
            return;
        }

        if (lower.Contains("/expense/expense.aspx")
            || title.Equals("Expense", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Expenditure", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/expense";
            link.Ready = true;
            return;
        }

        if (lower.Contains("final_reports.aspx")
            || title.Equals("Accounts Summary", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/summary";
            link.Ready = true;
            return;
        }

        if (lower.Contains("payment_category_wise_report.aspx")
            || title.Equals("Month Based Report", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/month";
            link.Ready = true;
            return;
        }

        if (lower.Contains("reports/income.aspx")
            || title.Equals("Income Report", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Income Details", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/income";
            link.Ready = true;
            return;
        }

        if (lower.Contains("reports/expense.aspx")
            || title.Equals("Expense Report", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Expense Details", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/expense-report";
            link.Ready = true;
            return;
        }

        if (lower.Contains("present_due.aspx")
            || title.Equals("Current Due", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Current Dues", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/current-due";
            link.Ready = true;
            return;
        }

        if (lower.Contains("reports/net.aspx")
            || title.Equals("Income Expense Net", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Income & Expense Details", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/net";
            link.Ready = true;
            return;
        }

        if (lower.Contains("sessionpayorderreport.aspx")
            || title.Equals("Class Based Report", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Class based payorder report", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/session/class";
            link.Ready = true;
            return;
        }

        if (title.Equals("Session Based Report", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/session";
            link.Ready = true;
            return;
        }

        if ((lower.Contains("payorderreport.aspx") && !lower.Contains("sessionpayorder"))
            || title.Equals("Payorder Report", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/payorder";
            link.Ready = true;
            return;
        }

        if (lower.Contains("session_paid_report.aspx")
            || title.Equals("Paid Report", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Session based Paid", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/session/paid";
            link.Ready = true;
            return;
        }

        if (lower.Contains("session_due_report.aspx")
            || title.Equals("Due Report", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Session based Due", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/session/due";
            link.Ready = true;
            return;
        }

        if (lower.Contains("paidreport.aspx")
            || title.Equals("Student Paid Details", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/paid";
            link.Ready = true;
            return;
        }

        if (lower.Contains("useraccount.aspx")
            || title.Equals("My Accounts", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Accounts details by user", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/my-accounts";
            link.Ready = true;
            return;
        }

        if (lower.Contains("accountdetails.aspx")
            || title.Equals("Account Details", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Account Summary", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/account-details";
            link.Ready = true;
            return;
        }

        if (lower.Contains("account_log.aspx")
            || title.Equals("Accounts Log", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/log";
            link.Ready = true;
            return;
        }

        if (lower.Contains("session_stu_report.aspx")
            || title.Equals("Student Based Report", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Student based payorder", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/session/students";
            link.Ready = true;
            return;
        }

        if (lower.Contains("concession_report.aspx")
            || title.Equals("Concession Report", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Concession report", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/session/concession";
            link.Ready = true;
            return;
        }

        if (lower.Contains("paid_and_due_report.aspx")
            || title.Equals("Paid & Due Report", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Paid And Due Report", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/accounts/reports/session/paid-due";
            link.Ready = true;
            return;
        }

        if (lower.Contains("create_edit_delete_exam_role.aspx")
            || title.Equals("Add Sub-Exam", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Sub-Exam Name", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/sub-exams";
            link.Ready = true;
            return;
        }

        if (lower.Contains("create_edit_delete_exam.aspx")
            || title.Equals("Add Exam", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Exam Name", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/names";
            link.Ready = true;
            return;
        }

        if (lower.Contains("create_edit_delete_grading_system.aspx")
            || title.Equals("Grading System", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/grading";
            link.Ready = true;
            return;
        }

        if (lower.Contains("passmark_change.aspx")
            || title.Equals("Change Pass Marks", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Pass Marks Change", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/pass-marks";
            link.Ready = true;
            return;
        }

        if (lower.Contains("marks_distribution.aspx")
            || title.Equals("Marks Distribution", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Exam Marks Distribution", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/distribution";
            link.Ready = true;
            return;
        }

        if (lower.Contains("marks_collect_paper.aspx")
            || title.Equals("Marks Collect Paper", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/collect-paper";
            link.Ready = true;
            return;
        }

        if (lower.Contains("input_exam_marks.aspx")
            || title.Equals("Input Exam Marks", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/input";
            link.Ready = true;
            return;
        }

        if (lower.Contains("input_markcheck.aspx")
            || title.Equals("Inputted Marks Check", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Inputted Exam Marks Check", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/marks-check";
            link.Ready = true;
            return;
        }

        if (lower.Contains("exam_publish_settings.aspx")
            || title.Equals("Exam Control", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/control";
            link.Ready = true;
            return;
        }

        if (lower.Contains("cumulative_setting.aspx")
            || title.Equals("Publish Cumulative Result", StringComparison.OrdinalIgnoreCase)
            || (title.Equals("Publish Result", StringComparison.OrdinalIgnoreCase) && lower.Contains("cumulative")))
        {
            link.Route = "/exam/cumulative-publish";
            link.Ready = true;
            return;
        }

        if (lower.Contains("cumulative_result.aspx")
            || title.Equals("Cumulative Result Card", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/cumulative-result";
            link.Ready = true;
            return;
        }

        if (lower.Contains("cumulative_position.aspx")
            || title.Equals("Cumulative Merit Position", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Cumulative Position", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/cumulative-merit";
            link.Ready = true;
            return;
        }

        if (title.Equals("Cumulative Exam", StringComparison.OrdinalIgnoreCase)
            && !lower.Contains("exam_publish_settings.aspx")
            && !lower.Contains("/student/"))
        {
            link.Route = "/exam/cumulative";
            link.Ready = true;
            return;
        }

        if (lower.Contains("publish_result.aspx")
            || title.Equals("Publish Result", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/publish";
            link.Ready = true;
            return;
        }

        if (lower.Contains("individual_result_for_class.aspx")
            || title.Equals("Result Card", StringComparison.OrdinalIgnoreCase)
            || title.Equals("English Result Card", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/result-card";
            link.Ready = true;
            return;
        }

        if (lower.Contains("examposition_subject.aspx")
            || title.Equals("Merit List of Subject", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Subject Merit List", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/merit-subject";
            link.Ready = true;
            return;
        }

        if (lower.Contains("examposition.aspx")
            || title.Equals("Merit List", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Exam Position", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/merit";
            link.Ready = true;
            return;
        }

        if (lower.Contains("banglaresult.aspx")
            || lower.Contains("exmampositionbangla.aspx")
            || title.Contains("result card (bangla)", StringComparison.OrdinalIgnoreCase)
            || title.Contains("merit list (madrasa)", StringComparison.OrdinalIgnoreCase)
            || title.Equals("বাংলা রেজাল্ট কার্ড", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/merit-madrasa";
            link.Title = "Merit List (Madrasa)";
            link.Ready = true;
            return;
        }

        if (lower.Contains("analytical_smart_result.aspx")
            || title.Equals("Analytical Result", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Analytical Smart Result", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/analytical";
            link.Ready = true;
            return;
        }

        if (lower.Contains("delete_exam_and_result.aspx")
            || title.Equals("Delete Result", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Delete Exam & Result", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/delete-result";
            link.Ready = true;
            return;
        }

        if (lower.Contains("seatplan")
            || title.Equals("Seat Plan", StringComparison.OrdinalIgnoreCase)
            || title.Equals("সিট প্ল্যান", StringComparison.OrdinalIgnoreCase)
            || title.Equals("আসন বিন্যাস", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/seat-plan";
            link.Ready = true;
            return;
        }

        if (lower.Contains("admit_card")
            || lower.Contains("admitcard")
            || title.Equals("Admit Card", StringComparison.OrdinalIgnoreCase)
            || title.Equals("অ্যাডমিট কার্ড", StringComparison.OrdinalIgnoreCase)
            || title.Equals("প্রবেশপত্র", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/admit-card";
            link.Ready = true;
            return;
        }

        if (title.Equals("Exam Setting", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/exam/setting";
            link.Ready = true;
            return;
        }

        if (lower.Contains("send_sms.aspx") && !lower.Contains("others")
            || title.Equals("Send SMS", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/sms/send";
            link.Ready = true;
            return;
        }

        if (lower.Contains("sms_template.aspx")
            || title.Equals("SMS Template Management", StringComparison.OrdinalIgnoreCase)
            || title.Equals("SMS Template", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/sms/templates";
            link.Ready = true;
            return;
        }

        if (lower.Contains("send_sms_to_others.aspx")
            || title.Equals("Send SMS From Contact List", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Send SMS to Others", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/sms/contacts";
            link.Ready = true;
            return;
        }

        if (lower.Contains("sent_sms_records.aspx")
            || title.Equals("SMS Sent Records", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Sent SMS Records", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/sms/records";
            link.Ready = true;
            return;
        }

        if (lower.Contains("sms_recharge.aspx")
            || title.Equals("SMS Recharge", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/sms/recharge";
            link.Ready = true;
            return;
        }

        if (lower.Contains("create_routines_for_classes.aspx")
            || title.Equals("Create Routine", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/routine/create";
            link.Ready = true;
            return;
        }

        if (lower.Contains("assign_teacher_and_subject")
            || title.Equals("Assign Routine", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Assign Teacher", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/routine/assign";
            link.Ready = true;
            return;
        }

        if (lower.Contains("modify_routine.aspx")
            || title.Equals("Edit Routine", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Modify Routine", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/routine/edit";
            link.Ready = true;
            return;
        }

        if (lower.Contains("class_routine.aspx")
            || title.Equals("Class Routine", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/routine/class";
            link.Ready = true;
            return;
        }

        if (lower.Contains("exam_routine_bangla.aspx")
            || title.Contains("পরীক্ষার রুটিন", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Exam Routine Bangla", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/routine/exam-bn";
            link.Ready = true;
            return;
        }

        if (lower.Contains("exam_routine.aspx")
            || title.Equals("Exam Routine", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/routine/exam";
            link.Ready = true;
            return;
        }

        if (lower.Contains("memberadd.aspx")
            || title.Equals("Add Member", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/committee/members";
            link.Ready = true;
            return;
        }

        if (lower.Contains("donationcategory.aspx")
            || title.Equals("Add Donation Category", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Donation Category", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/committee/categories";
            link.Ready = true;
            return;
        }

        if (lower.Contains("donationadd.aspx")
            || title.Equals("Add Donation", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/committee/donation-add";
            link.Ready = true;
            return;
        }

        if (lower.Contains("donations.aspx")
            || title.Equals("Donations", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/committee/donations";
            link.Ready = true;
            return;
        }

        if (lower.Contains("create_donor_username_password.aspx")
            || title.Equals("Donor Login Management", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/committee/donor-login";
            link.Ready = true;
            return;
        }

        if (lower.Contains("donor_present_due.aspx")
            || title.Equals("Donor Present Due List", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/committee/donor-due";
            link.Ready = true;
            return;
        }

        if (lower.Contains("donationbulkedit.aspx")
            || title.Equals("Bulk Pay Order Edit", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/committee/donation-bulk-edit";
            link.Ready = true;
            return;
        }

        if (lower.Contains("donationpayorder.aspx")
            || title.Equals("Bulk Pay Order", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/committee/donation-pay-order";
            link.Ready = true;
            return;
        }

        if (lower.Contains("donationcollect.aspx")
            || title.Equals("Collect Donation", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Donation Collect", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/committee/collect";
            link.Ready = true;
            return;
        }

        if (lower.Contains("paymentrecord.aspx")
            || title.Equals("Payment Record", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/committee/payments";
            link.Ready = true;
            return;
        }

        if (lower.Contains("unpaidreceipt.aspx")
            || title.Equals("Unpaid Receipt", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Unpaid Money Receipt", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/committee/unpaid";
            link.Ready = true;
            return;
        }

        if (lower.Contains("due_invoice.aspx")
            || title.Equals("Due Invoice", StringComparison.OrdinalIgnoreCase)
            || title.Equals("SIKKHALOY INVOICE", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/invoice/due";
            link.Ready = true;
            return;
        }

        if (lower.Contains("invoice_list.aspx")
            || lower.Contains("paid_receipt.aspx")
            || title.Equals("Paid Invoice", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Paid Invoice List", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/invoice/paid";
            link.Ready = true;
            return;
        }

        if (lower.Contains("inventory/suppliers")
            || lower.Contains("inventory/supplier-pay")
            || title.Equals("Supplier", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Suppliers", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Supplier Payment", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Supplier Payments", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/inventory/suppliers";
            link.Ready = true;
            return;
        }

        if (lower.Contains("inventory/items")
            || title.Equals("Item Add", StringComparison.OrdinalIgnoreCase)
            || title.Equals("Add Item", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/inventory/items";
            link.Ready = true;
            return;
        }

        if (lower.Contains("inventory/purchase-report")
            || title.Equals("Purchase Report", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/inventory/purchase-report";
            link.Ready = true;
            return;
        }

        if (lower.Contains("inventory/purchase")
            || title.Equals("Purchase", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/inventory/purchase";
            link.Ready = true;
            return;
        }

        if (lower.Contains("inventory/sale-report")
            || title.Equals("Sale Report", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/inventory/sale-report";
            link.Ready = true;
            return;
        }

        if (lower.Contains("inventory/sale")
            || title.Equals("Sale", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/inventory/sale";
            link.Ready = true;
            return;
        }

        if (lower.Contains("inventory/stock")
            || title.Equals("Stock Report", StringComparison.OrdinalIgnoreCase))
        {
            link.Route = "/inventory/stock";
            link.Ready = true;
            return;
        }

        link.Route = $"/coming-soon/{link.LinkID}";
        link.Ready = false;
    }

    public static void Deduplicate(MenuTreeDto tree)
    {
        tree.Categories ??= [];
        foreach (var category in tree.Categories)
        {
            category.Links ??= [];
            category.Subs ??= [];
            category.Links.RemoveAll(IsTemporarilyHidden);
            EnsureInputExamMarks(category.Links);
            Deduplicate(category.Links);
            PutAssignBeforeSession(category.Links);
            category.Subs.RemoveAll(IsTemporarilyHiddenSub);
            foreach (var sub in category.Subs)
            {
                sub.Links ??= [];
                sub.Links.RemoveAll(IsTemporarilyHidden);
                EnsureInputExamMarks(sub.Links);
                Deduplicate(sub.Links);
                PutAssignBeforeSession(sub.Links);
            }

            category.Subs.RemoveAll(sub => sub.Links.Count == 0);
        }

        EnsureInventory(tree);
    }

    private static void EnsureInventory(MenuTreeDto tree)
    {
        var cat = tree.Categories.FirstOrDefault(c => NameIs(c.Name, "Inventory"));
        if (cat is null)
        {
            cat = new MenuCategoryDto
            {
                CategoryID = -9200,
                Name = "Inventory",
                Sort = 85,
                Links = []
            };
            var at = tree.Categories.FindIndex(c => NameIs(c.Name, "Routines"));
            if (at >= 0)
                tree.Categories.Insert(at + 1, cat);
            else
                tree.Categories.Add(cat);
        }

        cat.Links ??= [];
        MenuLinkDto[] wanted =
        [
            Link(-9201, "Item Add", "/inventory/items"),
            Link(-9208, "Supplier", "/inventory/suppliers"),
            Link(-9202, "Purchase", "/inventory/purchase"),
            Link(-9203, "Purchase Report", "/inventory/purchase-report"),
            Link(-9204, "Sale", "/inventory/sale"),
            Link(-9205, "Sale Report", "/inventory/sale-report"),
            Link(-9206, "Stock Report", "/inventory/stock")
        ];
        foreach (var link in wanted)
        {
            if (cat.Links.Any(x => SameInvLink(x, link)))
                continue;
            cat.Links.Add(link);
        }

        var leftover = cat.Links.ToList();
        cat.Links.Clear();
        foreach (var link in wanted)
        {
            var found = leftover.FirstOrDefault(x => SameInvLink(x, link));
            if (found is null) continue;
            cat.Links.Add(found);
            leftover.Remove(found);
        }
        leftover.RemoveAll(IsTemporarilyHidden);
        cat.Links.AddRange(leftover);
    }

    private static bool SameInvLink(MenuLinkDto existing, MenuLinkDto wanted) =>
        string.Equals((existing.Route ?? existing.PageUrl ?? "").Trim(), wanted.Route, StringComparison.OrdinalIgnoreCase)
        || string.Equals((existing.Title ?? "").Trim(), wanted.Title, StringComparison.OrdinalIgnoreCase);

    private static bool NameIs(string? value, string name) =>
        string.Equals((value ?? "").Trim(), name, StringComparison.OrdinalIgnoreCase);

    private static MenuLinkDto Link(int id, string title, string route) => new()
    {
        LinkID = id,
        Title = title,
        PageUrl = route,
        Route = route,
        Ready = true,
        Sort = id
    };

    private static void EnsureInputExamMarks(List<MenuLinkDto> links)
    {
        if (links.Any(link => RouteIs(link, "/exam/input")))
            return;

        var collectAt = links.FindIndex(link => RouteIs(link, "/exam/collect-paper"));
        var checkAt = links.FindIndex(link => RouteIs(link, "/exam/marks-check"));
        if (collectAt < 0 && checkAt < 0)
            return;

        var insertAt = collectAt >= 0 ? collectAt + 1 : checkAt;
        links.Insert(insertAt, new MenuLinkDto
        {
            Title = "Input Exam Marks",
            PageUrl = "Exam/Input_Exam_Marks.aspx",
            Route = "/exam/input",
            Ready = true,
            Sort = insertAt
        });
    }

    private static bool RouteIs(MenuLinkDto link, string route) =>
        (link.Route ?? "").Trim().Equals(route, StringComparison.OrdinalIgnoreCase);

    private static void PutAssignBeforeSession(List<MenuLinkDto> links)
    {
        var assignAt = links.FindIndex(link => RouteIs(link, "/basic-settings/assign-subjects"));
        var sessionAt = links.FindIndex(link => RouteIs(link, "/basic-settings/session"));
        if (assignAt < 0 || sessionAt < 0 || assignAt < sessionAt)
            return;

        var assign = links[assignAt];
        links.RemoveAt(assignAt);
        sessionAt = links.FindIndex(link => RouteIs(link, "/basic-settings/session"));
        if (sessionAt < 0)
            links.Add(assign);
        else
            links.Insert(sessionAt, assign);
    }

    private static readonly HashSet<string> HiddenSubs = new(StringComparer.OrdinalIgnoreCase)
    {
        "Weekly Exam",
        "Input Marks",
        "Edit Marks",
        "Results"
    };

    private static readonly HashSet<string> HiddenTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Weekly Exam",
        "Input Marks",
        "Edit Marks",
        "Inventory Log",
        "Supplier Payment",
        "Supplier Payments"
    };

    private static bool IsTemporarilyHiddenSub(MenuSubDto sub) =>
        HiddenSubs.Contains((sub.Name ?? "").Trim());

    private static bool IsTemporarilyHidden(MenuLinkDto link)
    {
        if (HiddenTitles.Contains((link.Title ?? "").Trim()))
            return true;
        var route = (link.Route ?? link.PageUrl ?? "").Replace('\\', '/');
        return route.Contains("/inventory/log", StringComparison.OrdinalIgnoreCase)
            || route.Contains("/inventory/supplier-pay", StringComparison.OrdinalIgnoreCase);
    }

    private static void Deduplicate(List<MenuLinkDto> links)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < links.Count;)
        {
            var route = (links[i].Route ?? "").Trim();
            if (route.Length == 0 || seen.Add(route))
            {
                i++;
                continue;
            }

            links.RemoveAt(i);
        }
    }
}
