namespace Sikkhaloy.Shared.Employees;

public sealed class SalaryNameDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime? Created { get; set; }
}

public sealed class SaveSalaryNameRequest
{
    public string Name { get; set; } = "";
}

public sealed class SalaryAssignRowDto
{
    public int EmployeeID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string EmployeeType { get; set; } = "";
    public string? Phone { get; set; }
    public decimal Salary { get; set; }
    public bool Assigned { get; set; }
    public decimal Amount { get; set; }
    public string FixedOrPercentage { get; set; } = "Fixed";
}

public sealed class SaveSalaryAssignItem
{
    public int EmployeeID { get; set; }
    public bool Assigned { get; set; }
    public decimal Amount { get; set; }
    public string FixedOrPercentage { get; set; } = "Fixed";
}

public sealed class SaveSalaryAssignRequest
{
    public int NameId { get; set; }
    public List<SaveSalaryAssignItem> Items { get; set; } = [];
}

public sealed class PayorderEmployeeDto
{
    public int EmployeeID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string EmployeeType { get; set; } = "";
    public string? Phone { get; set; }
    public decimal Salary { get; set; }
    public string? BankAccNo { get; set; }
    public int PayorderNameId { get; set; }
    public string? PayorderName { get; set; }
}

public sealed class AssignPayorderRequest
{
    public int PayorderNameId { get; set; }
    public List<int> EmployeeIDs { get; set; } = [];
}

public sealed class SalaryMonthDto
{
    public string Name { get; set; } = "";
    public DateTime Date { get; set; }
}

public sealed class GenerateSalaryRequest
{
    public int PayorderNameId { get; set; }
    public DateTime MonthDate { get; set; }
    public string MonthName { get; set; } = "";
    public List<int> EmployeeIDs { get; set; } = [];
}

public sealed class DeleteMonthlyPayordersRequest
{
    public List<int> EmployeePayorderIds { get; set; } = [];
}

public sealed class SalaryLineDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
}

public sealed class MonthlyPayorderDto
{
    public int EmployeePayorderID { get; set; }
    public int MonthlyPayorderID { get; set; }
    public int EmployeeID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Designation { get; set; } = "";
    public string? BankAccNo { get; set; }
    public string EmployeeType { get; set; } = "";
    public decimal PayorderAmount { get; set; }
    public int WorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsDays { get; set; }
    public int LateDays { get; set; }
    public int LeaveDays { get; set; }
    public int FineCountDays { get; set; }
    public decimal Allowance { get; set; }
    public decimal Bonus { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal Deduction { get; set; }
    public decimal Fine { get; set; }
    public decimal AttendanceFine { get; set; }
    public decimal NetSalary { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Due { get; set; }
    public string PaidStatus { get; set; } = "";
    public string? Phone { get; set; }
    public List<SalaryLineDto> Allowances { get; set; } = [];
    public List<SalaryLineDto> Bonuses { get; set; } = [];
    public List<SalaryLineDto> Deductions { get; set; } = [];
    public List<SalaryLineDto> Fines { get; set; } = [];
}

public sealed class UpdateBonusFineItem
{
    public int EmployeePayorderID { get; set; }
    public int EmployeeID { get; set; }
    public decimal AttendanceFine { get; set; }
    public List<SalaryLineDto> Bonuses { get; set; } = [];
    public List<SalaryLineDto> Fines { get; set; } = [];
}

public sealed class UpdateBonusFineRequest
{
    public List<UpdateBonusFineItem> Items { get; set; } = [];
}

public sealed class AccountOptionDto
{
    public int AccountID { get; set; }
    public string Name { get; set; } = "";
    public decimal Balance { get; set; }
}

public sealed class PaySalaryItem
{
    public int EmployeePayorderID { get; set; }
    public int EmployeeID { get; set; }
    public decimal Amount { get; set; }
    public string Name { get; set; } = "";
}

public sealed class PaySalaryRequest
{
    public int AccountID { get; set; }
    public DateTime PaidDate { get; set; }
    public string MonthName { get; set; } = "";
    public List<PaySalaryItem> Items { get; set; } = [];
}

public sealed class PaidRecordDto
{
    public int RecordID { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? AccountName { get; set; }
}

public sealed class PaidDueRowDto
{
    public int EmployeeID { get; set; }
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string MonthName { get; set; } = "";
    public DateTime MonthStartDate { get; set; }
    public decimal Paid { get; set; }
    public decimal Due { get; set; }
}

public sealed class SalaryResult
{
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public int Id { get; set; }
    public int Count { get; set; }
}
