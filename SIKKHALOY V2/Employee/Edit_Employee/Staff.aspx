<%@ Page Title="Update Staff Info" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Staff.aspx.cs" Inherits="EDUCATION.COM.Employee.Edit_Employee.Staff" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <a href="../Employee_List.aspx">Back To Employee List</a>

    <asp:FormView ID="BasicInfoFormView" runat="server" DataKeyNames="StaffID" DataSourceID="BasicInfoSQL" DefaultMode="Edit" Width="100%">
        <EditItemTemplate>
            <div class="row mb-4">
                <div class="col-lg-8">
                    <div class="card">
                        <div class="card-body">
                            <h3>Update Staff Info</h3>

                            <div class="form-group">
                                <label>First Name</label>
                                <asp:TextBox ID="FirstNameTextBox" runat="server" Text='<%# Bind("FirstName") %>' CssClass="form-control" />
                            </div>

                            <div class="form-group">
                                <label>Last Name</label>
                                <asp:TextBox ID="LastNameTextBox" runat="server" Text='<%# Bind("LastName") %>' CssClass="form-control" />
                            </div>

                            <div class="form-group">
                                <label>Gender</label>
                                <asp:RadioButtonList ID="GenderRadioButtonList" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("Gender") %>'>
                                    <asp:ListItem>Male</asp:ListItem>
                                    <asp:ListItem>Female</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <div class="form-group">
                                <label>Father&#39;s Name</label>
                                <asp:TextBox ID="FatherNameTextBox" runat="server" Text='<%# Bind("FatherName") %>' CssClass="form-control" />
                            </div>

                            <div class="form-group">
                                <label>Designation</label>
                                <asp:TextBox ID="DesignationTextBox" runat="server" Text='<%# Bind("Designation") %>' CssClass="form-control" />
                            </div>
                            <div class="form-group">
                                <label>Religion</label>
                                <asp:DropDownList ID="ReligionDropDownList" runat="server" CssClass="form-control" SelectedValue='<%# Bind("Religion") %>'>
                                    <asp:ListItem>Islam</asp:ListItem>
                                    <asp:ListItem>Hinduism</asp:ListItem>
                                    <asp:ListItem>Buddhism</asp:ListItem>
                                    <asp:ListItem>Christianity</asp:ListItem>
                                </asp:DropDownList>
                            </div>

                            <div class="form-group">
                                <label>Date of Birth</label>
                                <asp:TextBox ID="DateofBirthTextBox" runat="server" Text='<%# Bind("DateofBirth") %>' CssClass="form-control" />
                            </div>

                            <div class="form-group">
                                <label>Address</label>
                                <asp:TextBox ID="AddressTextBox" runat="server" Text='<%# Bind("Address") %>' CssClass="form-control" />
                            </div>

                            <div class="form-group">
                                <label>Phone</label>
                                <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="PhoneTextBox" CssClass="EroorSummer" ErrorMessage="Phone Number Not Valid" SetFocusOnError="True" ValidationExpression="(88)?((011)|(015)|(016)|(017)|(018)|(019)|(013)|(014))\d{8,8}" ValidationGroup="1"></asp:RegularExpressionValidator>
                                <asp:TextBox ID="PhoneTextBox" runat="server" Text='<%# Bind("Phone") %>' CssClass="form-control" />
                            </div>

                            <div class="form-group">
                                <asp:LinkButton ID="UpdateButton" runat="server" CausesValidation="True" CommandName="Update" CssClass="btn btn-primary" Text="Update" ValidationGroup="1" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </EditItemTemplate>
    </asp:FormView>

    <asp:SqlDataSource ID="BasicInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM Staff_Info WHERE (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)" UpdateCommand="UPDATE Staff_Info SET FirstName = @FirstName, LastName = @LastName, Gender = @Gender, FatherName = @FatherName, Designation = @Designation, DateofBirth = @DateofBirth, Religion = @Religion, Address = @Address, Phone = @Phone WHERE (StaffID = @StaffID)">
        <SelectParameters>
            <asp:QueryStringParameter Name="EmployeeID" QueryStringField="Emp" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="FirstName" />
            <asp:Parameter Name="LastName" />
            <asp:Parameter Name="Gender" />
            <asp:Parameter Name="FatherName" />
            <asp:Parameter Name="Designation" />
            <asp:Parameter Name="DateofBirth" />
            <asp:Parameter Name="Religion" />
            <asp:Parameter Name="Address" />
            <asp:Parameter Name="Phone" />
            <asp:Parameter Name="StaffID" />
        </UpdateParameters>
    </asp:SqlDataSource>


    <asp:FormView ID="JobInfoFormView" runat="server" DataKeyNames="EmployeeID" DataSourceID="JobInfoSQL" DefaultMode="Edit" Width="100%">
        <EditItemTemplate>
            <div class="row">
                <div class="col-lg-8">
                    <div class="card">
                        <div class="card-body">
                            <h3>Update Staff Job Info</h3>

                            <div class="form-group">
                                <label>Job Type</label>
                                <asp:RadioButtonList ID="JobTypeRadioButtonList" CssClass="form-control" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("Permanent_Temporary") %>'>
                                    <asp:ListItem>Permanent</asp:ListItem>
                                    <asp:ListItem>Temporary</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <div class="form-group" style="display: none;">
                                <label>Salary Type</label>
                                <asp:RadioButtonList ID="SalaryTypeRadioButtonList" CssClass="form-control" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("Work_Time_Basis") %>'>
                                    <asp:ListItem>Work Basis</asp:ListItem>
                                    <asp:ListItem>Time Basis</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <div class="form-group" id="DR" style="display: none;">
                                <asp:RadioButtonList ID="JobperiodRadioButtonList" runat="server" CssClass="form-control" RepeatDirection="Horizontal" SelectedValue='<%# Bind("Time_Basis_Type") %>'>
                                    <asp:ListItem>Monthly</asp:ListItem>
                                    <asp:ListItem>Weekly</asp:ListItem>
                                    <asp:ListItem>Daily</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <div class="form-group">
                                <label>Salary</label>
                                <asp:TextBox ID="SalaryTextBox" runat="server" autocomplete="off" CssClass="form-control" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false" placeholder="Total Salary" Text='<%# Bind("Salary") %>'></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label>Abs Deduction</label>
                                <asp:RadioButtonList ID="Abs_DeductedRadioButtonList" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("IS_Abs_Deducted") %>'>
                                    <asp:ListItem Value="True">Yes</asp:ListItem>
                                    <asp:ListItem Value="False">No</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <div class="form-group" id="AHD" style="display: none">
                                <label>
                                    Deduction Amount
                                    <asp:RegularExpressionValidator ControlToValidate="Abs_DeductedAmount_TextBox" SetFocusOnError="true" ValidationExpression="[1-9][0-9]*" ID="Rx2" runat="server" ErrorMessage="0 Not allow" CssClass="EroorSummer" /></label>
                                <asp:TextBox ID="Abs_DeductedAmount_TextBox" CssClass="form-control" runat="server" autocomplete="off" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false" placeholder="Amount" Text='<%# Bind("Abs_Deduction") %>'></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label>Late Count As Abs</label>
                                <asp:RadioButtonList ID="Late_Count_As_AbsRadioButtonList" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("IS_Late_Count_As_Abs") %>'>
                                    <asp:ListItem Value="True">Yes</asp:ListItem>
                                    <asp:ListItem Value="False">No</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <div class="form-group" id="LateDay" style="display: none">
                                <label>
                                    Number of days
                                    <asp:RegularExpressionValidator ControlToValidate="LateDaysTextBox" SetFocusOnError="true" ValidationExpression="[1-9][0-9]*" ID="Rx" runat="server" ErrorMessage="0 Not allow" CssClass="EroorSummer" /></label>
                                <asp:TextBox ID="LateDaysTextBox" Text='<%# Bind("Late_Days") %>' autocomplete="off" CssClass="form-control" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false" runat="server" placeholder="Number of late days"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <asp:Button ID="UpdateButton" CssClass="btn btn-primary" runat="server" CommandName="Update" Text="Update" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </EditItemTemplate>
    </asp:FormView>

    <asp:SqlDataSource ID="JobInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM Employee_Info WHERE (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)" UpdateCommand="UPDATE Employee_Info SET Permanent_Temporary = @Permanent_Temporary, Work_Time_Basis = @Work_Time_Basis, Time_Basis_Type = @Time_Basis_Type,Employee_Payorder_NameID=@Employee_Payorder_NameID, Salary = @Salary, IS_Abs_Deducted = @IS_Abs_Deducted, Abs_Deduction = @Abs_Deduction, IS_Late_Count_As_Abs = @IS_Late_Count_As_Abs, Late_Days = @Late_Days WHERE (EmployeeID = @EmployeeID)">
        <SelectParameters>
            <asp:QueryStringParameter Name="EmployeeID" QueryStringField="Emp" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <%--<asp:ControlParameter ControlID="PayorderNameDropDownList" Name="Employee_Payorder_NameID" PropertyName="SelectedValue" />--%>
        </SelectParameters>
        <UpdateParameters>
            <asp:QueryStringParameter Name="EmployeeID" QueryStringField="Emp" />
            <asp:Parameter Name="Permanent_Temporary" />
            <asp:Parameter Name="Work_Time_Basis" />
            <asp:Parameter Name="Time_Basis_Type" />
            <asp:Parameter Name="Salary" />
            <asp:Parameter Name="IS_Abs_Deducted" />
            <asp:Parameter Name="Abs_Deduction" />
            <asp:Parameter Name="IS_Late_Count_As_Abs" />
            <asp:Parameter Name="Late_Days" />
            <asp:Parameter Name="Employee_Payorder_NameID" />
            <%--<asp:ControlParameter ControlID="PayorderNameDropDownList" Name="Employee_Payorder_NameID" PropertyName="SelectedValue" />--%>


        </UpdateParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="PayorderNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Payorder_NameID, SchoolID, RegistrationID, Payorder_Name, CreateDate FROM Employee_Payorder_Name WHERE (SchoolID = @SchoolID)">



        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>

    </asp:SqlDataSource>

    <asp:Button ID="DeactivateButton" runat="server" Text="Deactivate Staff" CssClass="btn bg-danger" OnClientClick="return confirm('Are You Sure Want To Deactivate?')" OnClick="DeactivateButton_Click" />
    <asp:SqlDataSource ID="DeactivateEmpSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EmployeeID, SchoolID, RegistrationID, ID, RFID, DeviceID, EmployeeType, Permanent_Temporary, Work_Time_Basis, Time_Basis_Type, Salary, IS_Abs_Deducted, Abs_Deduction, IS_Late_Count_As_Abs, Job_Status, CreateDate FROM Employee_Info WHERE (SchoolID = @SchoolID)" UpdateCommand="UPDATE Employee_Info SET Job_Status = 'Deactivate' WHERE (EmployeeID = @EmployeeID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
        <UpdateParameters>
            <asp:QueryStringParameter Name="EmployeeID" QueryStringField="Emp" Type="Int32" />
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
            <asp:Parameter DefaultValue="Deactivate Employee" Name="UpdateType" Type="String" />
            <asp:Parameter DefaultValue="Employee Deactivate" Name="UpdateDescription" Type="String" />
        </InsertParameters>
    </asp:SqlDataSource>


    <script src="/JS/DateMask.js"></script>
    <script>
        $(function () {
            //$("#body_JobInfoFormView_SalaryTypeRadioButtonList_0").click(function () { $('#DR').hide('slow') })
            //$("#body_JobInfoFormView_SalaryTypeRadioButtonList_1").click(function () { $('#DR').show('slow') })

            //if ($("[id*=SalaryTypeRadioButtonList]").find(":checked").val() == "Work Basis") {
            //    $('#DR').hide();
            //} else {
            //    $('#DR').show();
            //}

            $("[id*=DateofBirthTextBox]").mask("99/99/9999", { placeholder: 'dd/mm/yyyy' });

            //=Abs_Deducted
            var Rx2 = $("[id*=Rx2]");

            if ($("[id*=Abs_DeductedRadioButtonList]").find(":checked").val() == "True") {
                $('#AHD').show();
                $("[id*=Abs_DeductedAmount_TextBox]").attr("required", "required");
                ValidatorEnable(Rx2[0], 1);
            } else {
                $('#AHD').hide();
                ValidatorEnable(Rx2[0], 0);
            }

            $("#body_JobInfoFormView_Abs_DeductedRadioButtonList_0").click(function () {
                $('#AHD').show('slow');
                $("[id*=Abs_DeductedAmount_TextBox]").attr("required", "required");
                ValidatorEnable(Rx2[0], 1);
            });

            $("#body_JobInfoFormView_Abs_DeductedRadioButtonList_1").click(function () {
                $('#AHD').hide('slow');
                $("[id*=Abs_DeductedAmount_TextBox]").removeAttr("required");
                ValidatorEnable(Rx2[0], 0);
            });

            //Late_Count_As_Abs
            $("#body_JobInfoFormView_Late_Count_As_AbsRadioButtonList_0").click(function () {
                $('#LateDay').show('slow');
                $("[id*=LateDaysTextBox]").attr("required", "required");
            });
            $("#body_JobInfoFormView_Late_Count_As_AbsRadioButtonList_1").click(function () {
                $('#LateDay').hide('slow');
                $("[id*=LateDaysTextBox]").removeAttr("required");

            });

            if ($("[id*=Late_Count_As_AbsRadioButtonList]").find(":checked").val() == "True") {
                $('#LateDay').show();
                $("[id*=LateDaysTextBox]").attr("required", "required");
            } else {
                $('#LateDay').hide();
            }
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
