<%@ Page Title="Add Staff Info" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Staff_Info.aspx.cs" Inherits="EDUCATION.COM.Employee.Staff_Info" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <h3>Add Staff Info</h3>

    <div class="row">
        <div class="col-lg-8">
            <div class="card">
                <div class="card-body">
                    <div class="form-group">
                        <label>First Name<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="FirstNameTextBox" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="AS"></asp:RequiredFieldValidator></label>
                        <asp:TextBox ID="FirstNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>
                            Last Name
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="LastNameTextBox" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="AS"></asp:RequiredFieldValidator></label>
                        <asp:TextBox ID="LastNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Gender</label>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="GenderRadioButtonList" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="AS"></asp:RequiredFieldValidator>
                        <asp:RadioButtonList ID="GenderRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control">
                            <asp:ListItem>Male</asp:ListItem>
                            <asp:ListItem>Female</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div class="form-group">
                        <label>Father&#39;s Name</label>
                        <asp:TextBox ID="FatherNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>
                            Designation
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="DesignationTextBox" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="AS"></asp:RequiredFieldValidator></label>
                        <asp:TextBox ID="DesignationTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Religion</label>
                        <asp:DropDownList ID="ReligionDropDownList" runat="server" CssClass="form-control">
                            <asp:ListItem>Islam</asp:ListItem>
                            <asp:ListItem>Hinduism</asp:ListItem>
                            <asp:ListItem>Buddhism</asp:ListItem>
                            <asp:ListItem>Christianity</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="form-group">
                        <label>Phone</label>
                        <asp:TextBox ID="PhoneTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Address</label>
                        <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Job Type</label>
                        <asp:RadioButtonList ID="JobTypeRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control">
                            <asp:ListItem Selected="True">Permanent</asp:ListItem>
                            <asp:ListItem>Temporary</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div class="form-group">
                        <label>Image</label>
                        <asp:FileUpload ID="ImageFileUpload" runat="server" CssClass="form-control" />
                    </div>

                    <div class="form-group" style="display: none;">
                        <label>Salary Type</label>
                        <asp:RadioButtonList ID="SalaryTypeRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control">
                            <asp:ListItem>Work Basis</asp:ListItem>
                            <asp:ListItem Selected="True">Time Basis</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div class="form-group" id="DR" style="display: none;">
                        <label>Salary For</label>
                        <asp:RadioButtonList ID="JobperiodRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control">
                            <asp:ListItem Selected="True">Monthly</asp:ListItem>
                            <asp:ListItem>Weekly</asp:ListItem>
                            <asp:ListItem>Daily</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div class="form-group">
                        <label>Salary</label>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="SalaryTextBox" ErrorMessage="Input Salary" ValidationGroup="AS" CssClass="EroorSummer" ID="RequiredFieldValidator6"></asp:RequiredFieldValidator>
                        <asp:TextBox ID="SalaryTextBox" placeholder="Total Salary" runat="server" autocomplete="off" CssClass="form-control" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label>Abs Deduction</label>
                        <asp:RadioButtonList ID="Abs_DeductedRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control">
                            <asp:ListItem Value="True">Yes</asp:ListItem>
                            <asp:ListItem Selected="True" Value="False">No</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div class="form-group SHD" style="display: none">
                        <label>Amount</label>
                        <asp:TextBox ID="Abs_DeductedAmount_TextBox" placeholder="Deduction Amount" runat="server" autocomplete="off" CssClass="form-control" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Late Count As Abs</label>
                        <asp:RadioButtonList ID="Late_Count_As_AbsRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control">
                            <asp:ListItem Value="True">Yes</asp:ListItem>
                            <asp:ListItem Selected="True" Value="False">No</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div class="form-group" id="LateDay" style="display: none">
                        <label>Number of days</label>
                        <asp:TextBox ID="LateDaysTextBox" autocomplete="off" CssClass="form-control" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false" runat="server" placeholder="Number of late days"></asp:TextBox>
                    </div>

                    <asp:Button ID="AddStaffButton" runat="server" CssClass="btn btn-primary" Text="Submit" ValidationGroup="AS" OnClick="AddStaffButton_Click" />
                    <asp:SqlDataSource ID="Employee_InfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Employee_Info(SchoolID, RegistrationID, ID, EmployeeType, Permanent_Temporary, Work_Time_Basis, Time_Basis_Type, Salary, IS_Abs_Deducted, Abs_Deduction, IS_Late_Count_As_Abs,Late_Days, Job_Status) VALUES (@SchoolID, @RegistrationID, dbo.Employee_Staff_ID(@StaffID), @EmployeeType, @Permanent_Temporary, @Work_Time_Basis, @Time_Basis_Type, @Salary, @IS_Abs_Deducted, @Abs_Deduction, @IS_Late_Count_As_Abs,@Late_Days, @Job_Status) UPDATE  Staff_Info SET  EmployeeID = (SELECT SCOPE_IDENTITY()) WHERE (StaffID = @StaffID)"
                        SelectCommand="SELECT * FROM [Employee_Info]">
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                            <asp:Parameter DefaultValue="Staff" Name="EmployeeType" Type="String" />
                            <asp:ControlParameter ControlID="JobTypeRadioButtonList" Name="Permanent_Temporary" PropertyName="SelectedValue" Type="String" />
                            <asp:ControlParameter ControlID="SalaryTypeRadioButtonList" Name="Work_Time_Basis" PropertyName="SelectedValue" Type="String" />
                            <asp:ControlParameter ControlID="JobperiodRadioButtonList" Name="Time_Basis_Type" PropertyName="SelectedValue" Type="String" />
                            <asp:ControlParameter ControlID="SalaryTextBox" Name="Salary" PropertyName="Text" Type="Double" />
                            <asp:ControlParameter ControlID="Abs_DeductedRadioButtonList" Name="IS_Abs_Deducted" PropertyName="SelectedValue" Type="Boolean" />
                            <asp:ControlParameter ControlID="Abs_DeductedAmount_TextBox" Name="Abs_Deduction" PropertyName="Text" Type="Double" />
                            <asp:ControlParameter ControlID="Late_Count_As_AbsRadioButtonList" Name="IS_Late_Count_As_Abs" PropertyName="SelectedValue" Type="Boolean" />
                            <asp:Parameter DefaultValue="Active" Name="Job_Status" Type="String" />
                            <asp:ControlParameter ControlID="LateDaysTextBox" Name="Late_Days" PropertyName="Text" />
                            <asp:Parameter DefaultValue="" Name="StaffID" />
                        </InsertParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="Staff_InfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Staff_Info(SchoolID, RegistrationID, FirstName, LastName, Gender, FatherName, Designation, Address, Phone, Image, Staff_SN, Religion) VALUES (@SchoolID, @RegistrationID, @FirstName, @LastName, @Gender, @FatherName, @Designation, @Address, @Phone, @Image, dbo.Staff_SerialNumber(@SchoolID), @Religion)" SelectCommand="SELECT Staff_Info.FirstName, Staff_Info.LastName, Staff_Info.FirstName + ' ' + Staff_Info.LastName AS Name, Staff_Info.Gender, Staff_Info.FatherName, Staff_Info.Designation, Staff_Info.DateofBirth, Staff_Info.Religion, Staff_Info.NationalIDorPassportNO, Staff_Info.Address, Staff_Info.Phone, Staff_Info.Staff_SN, Employee_Info.ID, Employee_Info.EmployeeType, Employee_Info.Permanent_Temporary, Employee_Info.Salary, Staff_Info.StaffID, Staff_Info.EmployeeID FROM Staff_Info INNER JOIN Employee_Info ON Staff_Info.EmployeeID = Employee_Info.EmployeeID WHERE (Staff_Info.SchoolID = @SchoolID)">
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                            <asp:ControlParameter ControlID="FirstNameTextBox" Name="FirstName" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="LastNameTextBox" Name="LastName" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="GenderRadioButtonList" Name="Gender" PropertyName="SelectedValue" Type="String" />
                            <asp:ControlParameter ControlID="FatherNameTextBox" Name="FatherName" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="DesignationTextBox" Name="Designation" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="AddressTextBox" Name="Address" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="PhoneTextBox" Name="Phone" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="ImageFileUpload" Name="Image" PropertyName="FileBytes" />
                            <asp:ControlParameter ControlID="ReligionDropDownList" Name="Religion" PropertyName="SelectedValue" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                    <asp:SqlDataSource ID="Device_DataUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT DateUpdateID FROM  Attendance_Device_DataUpdateList WHERE (SchoolID = @SchoolID) AND (UpdateType = @UpdateType))
BEGIN
INSERT INTO Attendance_Device_DataUpdateList(SchoolID, RegistrationID, UpdateType, UpdateDescription) VALUES (@SchoolID, @RegistrationID, @UpdateType, @UpdateDescription)
END"
                        SelectCommand="SELECT * FROM [Attendance_Device_DataUpdateList]">
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                            <asp:Parameter DefaultValue="New Employee" Name="UpdateType" Type="String" />
                            <asp:Parameter DefaultValue="Add new Staff" Name="UpdateDescription" Type="String" />
                        </InsertParameters>
                    </asp:SqlDataSource>

                </div>
            </div>
        </div>
    </div>

    <div class="table-responsive mt-5">
        <asp:GridView ID="StaffGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="Staff_InfoSQL" DataKeyNames="StaffID" AllowPaging="True">
            <Columns>
                <asp:TemplateField HeaderText="ID" SortExpression="ID">
                    <ItemTemplate>
                        <%#Eval("ID") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Name" SortExpression="FirstName">
                    <ItemTemplate>
                        <%#Eval("Name") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Father's Name" SortExpression="FatherName">
                    <ItemTemplate>
                        <%#Eval("FatherName") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Designation" SortExpression="Designation">
                    <ItemTemplate>
                        <%#Eval("Designation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Phone" SortExpression="Phone">
                    <ItemTemplate>
                        <%#Eval("Phone") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Salary" SortExpression="Salary">
                    <ItemTemplate>
                        <%#Eval("Salary") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Edit">
                    <ItemTemplate>
                        <a target="_blank" href="Edit_Employee/Staff.aspx?Emp=<%# Eval("EmployeeID") %>">Update Info</a>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
    </div>

    <script>
        $(function () {
            //$("#body_SalaryTypeRadioButtonList_1").click(function () { $('#DR').show('slow') })
            //$("#body_SalaryTypeRadioButtonList_0").click(function () { $('#DR').hide('slow') })

            $("#body_Abs_DeductedRadioButtonList_0").click(function () {
                $('.SHD').show('slow');
                $("[id*=Abs_DeductedAmount_TextBox]").attr("required", "required");
            });
            $("#body_Abs_DeductedRadioButtonList_1").click(function () {
                $('.SHD').hide('slow');
                $("[id*=Abs_DeductedAmount_TextBox]").removeAttr("required");
            });

            $("#body_Late_Count_As_AbsRadioButtonList_0").click(function () {
                $('#LateDay').show('slow');
                $("[id*=LateDaysTextBox]").attr("required", "required");
            });
            $("#body_Late_Count_As_AbsRadioButtonList_1").click(function () {
                $('#LateDay').hide('slow');
                $("[id*=LateDaysTextBox]").removeAttr("required");
            });
        })
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
