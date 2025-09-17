<%@ Page Title="Salary Sheet" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Salary_Sheet.aspx.cs" Inherits="EDUCATION.COM.Employee.Salary_Sheet" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        @media print {
            h3 { font-size: 1.5rem; text-align: center; }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <h3>Salary Sheet<label id="Month"></label></h3>

            <div class="form-inline d-print-none">
                <div class="form-group">
                    <asp:DropDownList ID="PayorderNameDropDownList" runat="server" CssClass="form-control" DataSourceID="PayorderNameSQL" DataTextField="Payorder_Name" DataValueField="Employee_Payorder_NameID" AppendDataBoundItems="True" AutoPostBack="True">
                        <asp:ListItem Value="0">[ PAYORDER NAME ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="PayorderNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT Employee_Payorder_NameID, SchoolID, RegistrationID, Payorder_Name, CreateDate FROM Employee_Payorder_Name WHERE (SchoolID = @SchoolID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group mx-1">
                    <asp:DropDownList ID="MonthNameDropDownList" AutoPostBack="true" runat="server" CssClass="form-control" DataSourceID="MonthNameSQL" DataTextField="Month_Name" DataValueField="date" AppendDataBoundItems="True">
                        <asp:ListItem Value="0">[ SELECT MONTH ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="MonthNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="with months (date) AS (SELECT StartDate FROM  Education_Year WHERE (EducationYearID = @EducationYearID)
UNION ALL SELECT DATEADD(month,1,date) from months where DATEADD(month,1,date)&lt;= (SELECT EndDate FROM  Education_Year WHERE (EducationYearID = @EducationYearID)))
select  FORMAT(Date,'MMM yyyy') as Month_Name, date from months">
                        <SelectParameters>
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:RadioButtonList CssClass="form-control" ID="EmpTypeRadioButtonList" runat="server" AutoPostBack="True" RepeatDirection="Horizontal">
                        <asp:ListItem Selected="True" Value="%">All Employee</asp:ListItem>
                        <asp:ListItem>Teacher</asp:ListItem>
                        <asp:ListItem>Staff</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
                <div class="form-group">
                    <button type="button" onclick="window.print();" class="btn stylish-color-dark">Print</button>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="EmployeeGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="MonthlyPayorderID,Employee_PayorderID" DataSourceID="EmplyoeePayOrderSQL" OnRowDataBound="EmployeeGridView_RowDataBound" ShowFooter="True" AllowSorting="True">
                    <Columns>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" ReadOnly="True">
                            <ItemStyle HorizontalAlign="Left" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Salary" SortExpression="PayorderAmount">
                            <ItemTemplate>
                                <span class="PayorderAmount"><%# Eval("PayorderAmount") %></span>
                            </ItemTemplate>
                            <FooterTemplate>
                                <span class="GTPayorderAmount"></span>
                            </FooterTemplate>
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="Bank_AccNo" HeaderText="Bank Acc." SortExpression="Bank_AccNo" />
                        <asp:BoundField DataField="WorkingDays" HeaderText="W.D" SortExpression="WorkingDays" />
                        <asp:BoundField DataField="FineCountDays" HeaderText="Fine Count" SortExpression="FineCountDays" />
                        <asp:TemplateField HeaderText="Allowance" SortExpression="Allowance">
                            <ItemTemplate>
                                <asp:HiddenField ID="Employee_PayorderID" runat="server" Value='<%#Eval("Employee_PayorderID") %>' />
                                <asp:Repeater ID="AllowanceRepeater" runat="server" DataSourceID="AllowanceSQL">
                                    <ItemTemplate>
                                        <div class="text-right mb-1">
                                            <%#Eval("AllowanceName") %>: <%#Eval("AllowanceAmount") %>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:SqlDataSource ID="AllowanceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Allowance.AllowanceName, Employee_Allowance_Records.AllowanceAmount FROM Employee_Allowance_Records INNER JOIN Employee_Allowance ON Employee_Allowance_Records.AllowanceID = Employee_Allowance.AllowanceID WHERE (Employee_Allowance_Records.Employee_PayorderID = @Employee_PayorderID) AND (Employee_Allowance_Records.SchoolID = @SchoolID)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="Employee_PayorderID" Name="Employee_PayorderID" PropertyName="Value" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                                <input type="hidden" class="Allowance" value="<%# Eval("Allowance") %>"></input>
                            </ItemTemplate>
                            <FooterTemplate>
                                <label class="GTAllowance"></label>
                            </FooterTemplate>
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Bonus" SortExpression="Bonus">
                            <ItemTemplate>
                                <asp:HiddenField ID="Employee_PayorderID2" runat="server" Value='<%#Eval("Employee_PayorderID") %>' />
                                <asp:Repeater ID="BonusRepeater" DataSourceID="BonusSQL" runat="server">
                                    <ItemTemplate>
                                        <div class="text-right mb-1">
                                            <%#Eval("BonusName") %>: <%#Eval("Bonus_Amount") %>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:SqlDataSource ID="BonusSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        Employee_Bonus_Records.Bonus_Amount, Employee_Bonus.BonusName
FROM            Employee_Bonus_Records INNER JOIN
                         Employee_Bonus ON Employee_Bonus_Records.BonusID = Employee_Bonus.BonusID
WHERE        (Employee_Bonus_Records.Employee_PayorderID = @Employee_PayorderID)">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:ControlParameter ControlID="Employee_PayorderID2" Name="Employee_PayorderID" PropertyName="Value" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                                <input type="hidden" class="Bonus" value="<%# Eval("Bonus") %>"></input>
                            </ItemTemplate>
                            <FooterTemplate>
                                <label class="GTBonus"></label>
                            </FooterTemplate>
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Gross Salary" SortExpression="GrossSalary">
                            <ItemTemplate>
                                <span class="GrossSalary"><%# Eval("GrossSalary") %></span>
                            </ItemTemplate>
                            <FooterTemplate>
                                <label class="GTGrossSalary"></label>
                            </FooterTemplate>
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Deduction" SortExpression="Diduction">
                            <ItemTemplate>
                                <span class="Diduction"><%# Eval("Diduction") %></span>
                            </ItemTemplate>
                            <FooterTemplate>
                                <label class="GTDiduction"></label>
                            </FooterTemplate>
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Fine" SortExpression="Fine">
                            <ItemTemplate>
                                <span class="Fine"><%# Eval("Fine") %></span>
                            </ItemTemplate>
                            <FooterTemplate>
                                <label class="GTFine"></label>
                            </FooterTemplate>
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Net Salary" SortExpression="InTotalSalary">
                            <ItemTemplate>
                                <span class="InTotalSalary"><%# Eval("InTotalSalary") %></span>
                            </ItemTemplate>
                            <FooterTemplate>
                                <label class="GTInTotalSalary"></label>
                            </FooterTemplate>
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Signature">
                            <ItemStyle Width="120px" />
                        </asp:TemplateField>
                    </Columns>
                    <FooterStyle CssClass="GridFooter" />
                </asp:GridView>

                <asp:SqlDataSource ID="EmplyoeePayOrderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Payorder_Monthly.MonthlyPayorderID, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.Designation, VW_Emp_Info.Bank_AccNo, Employee_Payorder_Monthly.WorkingDays, Employee_Payorder.PayorderAmount, Employee_Payorder.Allowance, Employee_Payorder.Bonus, Employee_Payorder.Diduction, Employee_Payorder.Fine, Employee_Payorder.InTotalSalary, Employee_Payorder.Employee_PayorderID, Employee_Payorder.EmployeeID, Employee_Payorder.GrossSalary, Employee_Payorder_Monthly.FineCountDays FROM Employee_Payorder_Monthly INNER JOIN Employee_Payorder ON Employee_Payorder_Monthly.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN VW_Emp_Info ON Employee_Payorder.EmployeeID = VW_Emp_Info.EmployeeID WHERE (Employee_Payorder.Employee_Payorder_NameID = @Employee_Payorder_NameID) AND (Employee_Payorder_Monthly.MonthName = @MonthName) AND (Employee_Payorder.SchoolID = @SchoolID) AND (VW_Emp_Info.EmployeeType LIKE @EmployeeType)">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="PayorderNameDropDownList" Name="Employee_Payorder_NameID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="MonthNameDropDownList" Name="MonthName" PropertyName="SelectedItem.Text" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <script>
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            if ($('[id*=MonthNameDropDownList] :selected').index() > 0) {
                $("#Month").text(": " + $('[id*=MonthNameDropDownList] :selected').text());
            }

            var allowance = 0;
            $(".Allowance").each(function () {
                allowance = allowance + parseFloat($(this).val());
            });
            $(".GTAllowance").html("৳" + allowance);

            var bonus = 0;
            $(".Bonus").each(function () {
                bonus = bonus + parseFloat($(this).val());
            });
            $(".GTBonus").html("৳" + bonus);

            var grossSalary = 0;
            $(".GrossSalary").each(function () {
                grossSalary = grossSalary + parseFloat($(this).html());
            });
            $(".GTGrossSalary").html("৳" + grossSalary);


            var deduction = 0;
            $(".Diduction").each(function () {
                deduction = deduction + parseFloat($(this).html());
            });
            $(".GTDiduction").html("৳" + deduction);


            var fine = 0;
            $(".Fine").each(function () {
                fine = fine + parseFloat($(this).html());
            });
            $(".GTFine").html("৳" + fine);


            var inTotalSalary = 0;
            $(".InTotalSalary").each(function () {
                inTotalSalary = inTotalSalary + parseFloat($(this).html());
            });
            $(".GTInTotalSalary").html("৳" + inTotalSalary);
        });
    </script>
</asp:Content>
