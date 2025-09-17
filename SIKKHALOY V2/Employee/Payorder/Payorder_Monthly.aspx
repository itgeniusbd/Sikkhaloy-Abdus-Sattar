<%@ Page Title="Employee Monthly Payorder" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Payorder_Monthly.aspx.cs" Inherits="EDUCATION.COM.Employee.Payorder.Payorder_Monthly" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <div class="row">
        <div class="col-sm-12"><h3>Employee Monthly Payorder  <a style="float:right" href="../SetEmployee_With_PayorderName.aspx">Set Employee With Payorder Name</a></h3></div>
        
    </div>

    
    
   

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="row">
                <div class="col-sm-4 form-group">
                    <label>
                        Payorder Name <a href="#" data-toggle="modal" data-target="#myModal">Add New</a>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="PayorderNameDropDownList" CssClass="EroorSummer" ErrorMessage="Select Payorder" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                    </label>
                    <asp:DropDownList ID="PayorderNameDropDownList" runat="server" CssClass="form-control" DataSourceID="PayorderNameSQL" DataTextField="Payorder_Name" DataValueField="Employee_Payorder_NameID" AppendDataBoundItems="True" AutoPostBack="True">
                        <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-sm-4 form-group">
                    <label>Month Name</label>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="MonthNameDropDownList" CssClass="EroorSummer" ErrorMessage="Select Month" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                    <asp:DropDownList ID="MonthNameDropDownList" runat="server" CssClass="form-control" DataSourceID="MonthNameSQL" DataTextField="Month_Name" DataValueField="date" AppendDataBoundItems="True" AutoPostBack="True">
                        <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
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

                <div class="col-sm-4 form-group">
                    <%if (EmployeeListGridView.Rows.Count > 0)
                        {%>
                    
                    <asp:Button ID="UploadButton" runat="server" CssClass="btn btn-primary d-print-none mt-4" Text="Update" OnClick="SalaryUpdateButton_Click" />

                    <%}%>


                    <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-deep-orange mt-4" Text="Generate Salary" OnClick="SubmitButton_Click" ValidationGroup="1" />
                </div>

            </div>

            <asp:SqlDataSource ID="PayorderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="Emp_Salary_Monthly" SelectCommand="SELECT * FROM [Employee_Payorder]" InsertCommandType="StoredProcedure">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="PayorderNameDropDownList" Name="Employee_Payorder_NameID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="MonthNameDropDownList" DbType="Date" Name="Get_date" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="MonthNameDropDownList" Name="MonthName" PropertyName="SelectedItem.Text" Type="String" />
                    <asp:Parameter Name="EmployeeID" />
                    <asp:Parameter Direction="Output" Name="GeT_Employee_PayorderID" Type="Int32" />
                </InsertParameters>
            </asp:SqlDataSource>
            <div class="alert alert-info">                
                <label id="CountStudent"></label>
            </div>
            <div class="table-responsive">
                <asp:GridView ID="EmployeeListGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="EmployeeID,EmployeeType" DataSourceID="EmployeeListSQL" AllowSorting="True">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <ItemStyle Width="50px" />
                        </asp:TemplateField>
                        
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                        <asp:BoundField DataField="Phone" HeaderText="Mobile No." SortExpression="Phone" />
                        <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
                        <asp:BoundField DataField="EmployeeType" HeaderText="Type" SortExpression="Type" />                       
                        <asp:TemplateField HeaderText="Salary" SortExpression="Salary">
                            <ItemTemplate>
                                <asp:TextBox ID="SalaryTextBox" CssClass="form-control" runat="server" Text='<%# Bind("Salary") %>'></asp:TextBox>
                            </ItemTemplate>
                            <HeaderStyle CssClass="d-print-none" />
                            <ItemStyle CssClass="d-print-none" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="Bank_AccNo" HeaderText="Bank Acc.No" SortExpression="Bank_AccNo" />                       
                        <asp:TemplateField HeaderText="Image">
                            <ItemTemplate>
                                <img alt="" src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>" class="z-depth-1 img-fluid" style="width: 50px" />
                            </ItemTemplate>
                            <ItemStyle VerticalAlign="Middle" CssClass="Itm_Img" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="EmployeeListSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT EmployeeID, ID,Bank_AccNo, EmployeeType, Permanent_Temporary, Salary,  FirstName +' '+ LastName as Name, Designation, Phone, DeviceID FROM VW_Emp_Info WHERE (SchoolID = @SchoolID) AND (Job_Status = N'Active') AND (Employee_Payorder_NameID=@Employee_Payorder_NameID) order by ID"
                    UpdateCommand="IF NOT EXISTS (SELECT * FROM Employee_Info WHERE ID = @ID AND SchoolID = @SchoolID) 
                                    UPDATE Employee_Info SET ID = @ID WHERE (EmployeeID = @EmployeeID)"
                    InsertCommand="UPDATE Employee_Info SET EmployeeType = @EmployeeType WHERE (EmployeeID = @EmployeeID)">                    
                    <InsertParameters>
                        <asp:Parameter Name="EmployeeType" />
                        <asp:Parameter Name="EmployeeID" />
                    </InsertParameters>
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />                       
                        <asp:ControlParameter ControlID="PayorderNameDropDownList" Name="Employee_Payorder_NameID" PropertyName="SelectedValue" />
                    </SelectParameters>
                    <UpdateParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:Parameter Name="ID" />
                        <asp:Parameter Name="EmployeeID" />
                    </UpdateParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SalaryUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Employee_Info]" UpdateCommand="UPDATE Employee_Info SET Salary = @Salary WHERE (EmployeeID = @EmployeeID)">
                    <UpdateParameters>
                        <asp:Parameter Name="Salary" />
                        <asp:Parameter Name="EmployeeID" />
                    </UpdateParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="Bank_AccNoUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Employee_Info]" UpdateCommand="UPDATE Employee_Info SET Bank_AccNo = @Bank_AccNo WHERE (EmployeeID = @EmployeeID)">
                    <UpdateParameters>
                        <asp:Parameter Name="Bank_AccNo" />
                        <asp:Parameter Name="EmployeeID" />
                    </UpdateParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="Device_DataUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT DateUpdateID FROM  Attendance_Device_DataUpdateList WHERE (SchoolID = @SchoolID) AND (UpdateType = @UpdateType))
BEGIN
INSERT INTO Attendance_Device_DataUpdateList(SchoolID, RegistrationID, UpdateType, UpdateDescription) VALUES (@SchoolID, @RegistrationID, @UpdateType, @UpdateDescription)
END"
                    SelectCommand="SELECT * FROM [Attendance_Device_DataUpdateList]">
                    <InsertParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                        <asp:Parameter DefaultValue="Employee ID Change" Name="UpdateType" Type="String" />
                        <asp:Parameter DefaultValue="Employee ID chnage" Name="UpdateDescription" Type="String" />
                    </InsertParameters>
                </asp:SqlDataSource>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="PayOrderGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="Employee_PayorderID,EmployeeID" DataSourceID="EmplyoeePayOrderSQL">
                    <Columns>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" ReadOnly="True" />
                        <asp:BoundField DataField="PayorderAmount" HeaderText="Basic Salary" SortExpression="PayorderAmount" />
                        <asp:BoundField DataField="WorkingDays" HeaderText="W.D" SortExpression="WorkingDays" />
                        <asp:BoundField DataField="PerDays" HeaderText="Pre." SortExpression="PerDays" />
                        <asp:BoundField DataField="AbsDays" HeaderText="Abs." SortExpression="AbsDays" />
                        <asp:BoundField DataField="LateDays" HeaderText="Late" SortExpression="LateDays" />
                        <asp:BoundField DataField="LeaveDays" HeaderText="Leave" SortExpression="LeaveDays" />
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
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Bonus" SortExpression="Bonus">
                            <ItemTemplate>
                                <asp:HiddenField ID="Employee_PayorderID2" runat="server" Value='<%#Eval("Employee_PayorderID") %>' />
                                <asp:Repeater ID="BonusRepeater" DataSourceID="BonusSQL" runat="server">
                                    <ItemTemplate>
                                        <div class="text-left mb-1">
                                            <asp:HiddenField ID="BonusID" runat="server" Value='<%#Eval("BonusID") %>' />
                                            <%#Eval("BonusName") %>
                                            <asp:TextBox onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" ID="BonusTextBox" CssClass="form-control w-auto" runat="server" Text='<%#Eval("Bonus_Amount") %>'></asp:TextBox>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:SqlDataSource ID="BonusSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Bonus.BonusID, Employee_Bonus.BonusName, T.Bonus_Amount FROM Employee_Bonus LEFT OUTER JOIN (SELECT BonusID, Bonus_Amount FROM Employee_Bonus_Records WHERE (SchoolID = @SchoolID) AND (Employee_PayorderID = @Employee_PayorderID)) AS T ON Employee_Bonus.BonusID = T.BonusID WHERE (Employee_Bonus.SchoolID = @SchoolID)">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:ControlParameter ControlID="Employee_PayorderID2" Name="Employee_PayorderID" PropertyName="Value" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="GrossSalary" HeaderText="Gross Salary" SortExpression="GrossSalary" />
                        <asp:TemplateField HeaderText="Diduction" SortExpression="Diduction">
                            <ItemTemplate>
                                <asp:HiddenField ID="Employee_PayorderID3" runat="server" Value='<%#Eval("Employee_PayorderID") %>' />
                                <asp:Repeater ID="DiductionRepeater" DataSourceID="DiductionSQL" runat="server">
                                    <ItemTemplate>
                                        <div class="text-right mb-1">
                                            <%#Eval("DeductionName") %>: <%#Eval("Deduction_Amount") %>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:SqlDataSource ID="DiductionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Deduction_Records.Deduction_Amount, Employee_Deduction.DeductionName FROM Employee_Deduction_Records INNER JOIN Employee_Deduction ON Employee_Deduction_Records.DeductionID = Employee_Deduction.DeductionID WHERE (Employee_Deduction_Records.Employee_PayorderID = @Employee_PayorderID) AND (Employee_Deduction_Records.SchoolID = @SchoolID)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="Employee_PayorderID3" Name="Employee_PayorderID" PropertyName="Value" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Fine" SortExpression="Fine">
                            <ItemTemplate>
                                <asp:HiddenField ID="Employee_PayorderID4" runat="server" Value='<%#Eval("Employee_PayorderID") %>' />
                                <div class="text-left mb-1">
                                    Attendance Fine
                                    <asp:TextBox onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" ID="AttendanceFineTextBox" CssClass="form-control w-auto" runat="server" Text='<%#Eval("FineAmount") %>'></asp:TextBox>
                                </div>
                                <asp:Repeater ID="FineRepeater" runat="server" DataSourceID="FineSQL">
                                    <ItemTemplate>
                                        <div class="text-left mb-1">
                                            <asp:HiddenField ID="FineID" runat="server" Value='<%#Eval("FineID") %>' />
                                            <%#Eval("FineName") %>
                                            <asp:TextBox onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" ID="FineTextBox" CssClass="form-control w-auto" runat="server" Text='<%#Eval("Fine_Amount") %>'></asp:TextBox>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:SqlDataSource ID="FineSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Fine.FineID,Employee_Fine.FineName, T.Fine_Amount FROM Employee_Fine LEFT OUTER JOIN (SELECT FineID, Fine_Amount FROM Employee_Fine_Records WHERE (SchoolID = @SchoolID) AND (Employee_PayorderID = @Employee_PayorderID)) AS T ON Employee_Fine.FineID = T.FineID WHERE (Employee_Fine.SchoolID = @SchoolID)">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:ControlParameter ControlID="Employee_PayorderID4" Name="Employee_PayorderID" PropertyName="Value" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="InTotalSalary" HeaderText="Net Salary" SortExpression="InTotalSalary" />
                        <asp:TemplateField HeaderText="Delete" ShowHeader="False">
                            <ItemTemplate>
                                <asp:LinkButton ID="deleteButton" CssClass="red-text" runat="server" CausesValidation="false" CommandName="Delete"><i class="fa fa-trash" style="font-size:1.2rem" aria-hidden="true"></i></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <asp:SqlDataSource ID="EmplyoeePayOrderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Payorder_Monthly.MonthlyPayorderID, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.Designation, VW_Emp_Info.Bank_AccNo, Employee_Payorder_Monthly.WorkingDays, Employee_Payorder_Monthly.FineCountDays, Employee_Payorder_Monthly.PerDays, Employee_Payorder_Monthly.AbsDays, Employee_Payorder.PayorderAmount, Employee_Payorder.Allowance, Employee_Payorder.Bonus, Employee_Payorder.Diduction, Employee_Payorder.Fine, Employee_Payorder.InTotalSalary, VW_Emp_Info.Salary, Employee_Payorder_Monthly.LateDays, Employee_Payorder_Monthly.LeaveDays, Employee_Payorder.Employee_PayorderID, Employee_Payorder_Monthly.FineAmount, Employee_Payorder.EmployeeID, Employee_Payorder.GrossSalary FROM Employee_Payorder_Monthly INNER JOIN Employee_Payorder ON Employee_Payorder_Monthly.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN VW_Emp_Info ON Employee_Payorder.EmployeeID = VW_Emp_Info.EmployeeID WHERE (Employee_Payorder.Employee_Payorder_NameID = @Employee_Payorder_NameID) AND (Employee_Payorder_Monthly.MonthName = @MonthName) AND (Employee_Payorder.SchoolID = @SchoolID)" DeleteCommand="IF EXISTS(SELECT Employee_PayorderID FROM Employee_Payorder WHERE (Employee_PayorderID = @Employee_PayorderID) AND (PaidAmount = 0))
BEGIN
DELETE FROM Employee_Fine_Records WHERE (Employee_PayorderID = @Employee_PayorderID)
DELETE FROM Employee_Deduction_Records WHERE (Employee_PayorderID = @Employee_PayorderID) 
DELETE FROM Employee_Bonus_Records WHERE (Employee_PayorderID = @Employee_PayorderID)
DELETE FROM Employee_Allowance_Records WHERE (Employee_PayorderID = @Employee_PayorderID)
DELETE FROM Employee_Payorder_Monthly WHERE (Employee_PayorderID = @Employee_PayorderID)
DELETE FROM Employee_Payorder WHERE (Employee_PayorderID = @Employee_PayorderID)
END"
                InsertCommand="IF EXISTS(SELECT Bonus_RecordsID FROM Employee_Bonus_Records WHERE (SchoolID = @SchoolID) AND (BonusID = @BonusID) AND (Employee_PayorderID = @Employee_PayorderID))
BEGIN
IF(@Bonus_Amount &lt;&gt; '')
UPDATE Employee_Bonus_Records SET  Bonus_Amount = @Bonus_Amount WHERE (SchoolID = @SchoolID) AND (BonusID = @BonusID) AND (Employee_PayorderID = @Employee_PayorderID)
ELSE
DELETE FROM Employee_Bonus_Records WHERE (SchoolID = @SchoolID) AND (BonusID = @BonusID) AND (Employee_PayorderID = @Employee_PayorderID)
END
ELSE
BEGIN
IF(@Bonus_Amount &lt;&gt; '')
INSERT INTO Employee_Bonus_Records
                         (SchoolID, RegistrationID, BonusID, EmployeeID, Employee_PayorderID, Bonus_Amount)
VALUES             (@SchoolID, @RegistrationID, @BonusID, @EmployeeID, @Employee_PayorderID, @Bonus_Amount)
END"
                UpdateCommand="IF EXISTS(SELECT Fine_RecordsID FROM Employee_Fine_Records WHERE (SchoolID = @SchoolID) AND (FineID = @FineID) AND (Employee_PayorderID = @Employee_PayorderID))
BEGIN
IF(@Fine_Amount &lt;&gt; '')
UPDATE Employee_Fine_Records SET Fine_Amount = @Fine_Amount WHERE (SchoolID = @SchoolID) AND (FineID = @FineID) AND (Employee_PayorderID = @Employee_PayorderID)
ELSE
DELETE FROM Employee_Fine_Records WHERE (SchoolID = @SchoolID) AND (FineID = @FineID) AND (Employee_PayorderID = @Employee_PayorderID)
END 
ELSE
BEGIN
IF(@Fine_Amount &lt;&gt; '')
INSERT INTO Employee_Fine_Records
                         (SchoolID, RegistrationID, FineID, EmployeeID, Employee_PayorderID, Fine_Amount)
VALUES             (@SchoolID, @RegistrationID, @FineID, @EmployeeID, @Employee_PayorderID, @Fine_Amount)
END">
                <DeleteParameters>
                    <asp:Parameter Name="Employee_PayorderID" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:Parameter Name="BonusID" />
                    <asp:Parameter Name="Employee_PayorderID" />
                    <asp:Parameter Name="Bonus_Amount" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:Parameter Name="EmployeeID"></asp:Parameter>
                </InsertParameters>
                <SelectParameters>
                    <asp:ControlParameter ControlID="PayorderNameDropDownList" Name="Employee_Payorder_NameID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="MonthNameDropDownList" Name="MonthName" PropertyName="SelectedItem.Text" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
                <UpdateParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:Parameter Name="FineID" />
                    <asp:Parameter Name="Employee_PayorderID" />
                    <asp:Parameter Name="Fine_Amount" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:Parameter Name="EmployeeID" />
                </UpdateParameters>
            </asp:SqlDataSource>

            <asp:SqlDataSource ID="AttendanceFineUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_PayorderID FROM Employee_Payorder" UpdateCommand="UPDATE Employee_Payorder_Monthly SET FineAmount = @FineAmount WHERE (Employee_PayorderID = @Employee_PayorderID) AND (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)">
                <UpdateParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:Parameter Name="FineAmount" />
                    <asp:Parameter Name="Employee_PayorderID" />
                    <asp:Parameter Name="EmployeeID" />
                </UpdateParameters>
            </asp:SqlDataSource>

            <%if (PayOrderGridView.Rows.Count > 0)
                { %>
            <asp:Button ID="UpdateButton" runat="server" Text="Update Bonus & Fine" CssClass="btn btn-primary mt-3" OnClick="UpdateButton_Click" />
            <%} %>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Modal -->
    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog cascading-modal" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="title"><i class="fa fa-plus"></i>Add Payorder Name</h4>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body mb-0">
                    <asp:UpdatePanel ID="upnlUsers" runat="server">
                        <ContentTemplate>
                            <div class="form-inline">
                                <div class="form-group">
                                    <asp:TextBox ID="PayorderNameTextBox" runat="server" CssClass="form-control" placeholder="Payorder Name"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="PayorderNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="AA"></asp:RequiredFieldValidator>
                                </div>
                                <div class="form-group">
                                    <asp:Button ID="AddPayorderButton" runat="server" CssClass="btn btn-primary" Text="Add" ValidationGroup="AA" OnClick="AddPayorderButton_Click" />
                                </div>
                                <asp:SqlDataSource ID="PayorderNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="IF NOT EXISTS(SELECT  Employee_PayorderID FROM  Employee_Payorder WHERE (Employee_Payorder_NameID = @Employee_Payorder_NameID)) 
 BEGIN
DELETE FROM [Employee_Payorder_Name] WHERE [Employee_Payorder_NameID] = @Employee_Payorder_NameID
END"
                                    InsertCommand="IF NOT EXISTS(SELECT * FROM Employee_Payorder_Name WHERE SchoolID = @SchoolID AND Payorder_Name = @Payorder_Name)
INSERT INTO Employee_Payorder_Name(SchoolID, RegistrationID, Payorder_Name) VALUES (@SchoolID, @RegistrationID, @Payorder_Name)"
                                    SelectCommand="SELECT Employee_Payorder_NameID, SchoolID, RegistrationID, Payorder_Name, CreateDate FROM Employee_Payorder_Name WHERE (SchoolID = @SchoolID)" UpdateCommand="IF NOT EXISTS(SELECT * FROM Employee_Payorder_Name WHERE SchoolID = @SchoolID AND Payorder_Name = @Payorder_Name)
UPDATE Employee_Payorder_Name SET Payorder_Name = @Payorder_Name WHERE (Employee_Payorder_NameID = @Employee_Payorder_NameID)">
                                    <DeleteParameters>
                                        <asp:Parameter Name="Employee_Payorder_NameID" Type="Int32" />
                                    </DeleteParameters>
                                    <InsertParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                        <asp:ControlParameter ControlID="PayorderNameTextBox" Name="Payorder_Name" PropertyName="Text" Type="String" />
                                    </InsertParameters>
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                    <UpdateParameters>
                                        <asp:Parameter Name="Payorder_Name" Type="String" />
                                        <asp:Parameter Name="Employee_Payorder_NameID" Type="Int32" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </UpdateParameters>
                                </asp:SqlDataSource>
                            </div>
                            <br />
                            <asp:GridView ID="AllownceNameGridView" runat="server" AllowPaging="True" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="Employee_Payorder_NameID" DataSourceID="PayorderNameSQL">
                                <Columns>
                                    <asp:CommandField ShowEditButton="True" />
                                    <asp:BoundField DataField="Payorder_Name" HeaderText="Payorder Name" SortExpression="Payorder_Name" />
                                    <asp:BoundField DataField="CreateDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Created Date" SortExpression="CreateDate" ReadOnly="True" />
                                    <asp:CommandField ShowDeleteButton="True" />
                                </Columns>
                                <PagerStyle CssClass="pgr" />
                            </asp:GridView>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>



    <script type="text/javascript">
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            if ($("[id*=EmployeeListGridView] tr").length) {
                $("#CountStudent").text("TOTAL: " + $("[id*=EmployeeListGridView] td").closest("tr").length + " EMPLOYEE.");
            }
            $("[id*=AllIteamCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllIteamCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SingleCheckBox]", a).length == $("[id*=SingleCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });
    </script>
</asp:Content>
