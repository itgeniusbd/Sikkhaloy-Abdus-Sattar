<%@ Page Title="Manage Teacher" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Active_Deactivate_Teacher.aspx.cs" Inherits="EDUCATION.COM.ADMINISTRATION_BASIC_SETTING.Active_Deactivate_Teacher" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Manage_Teacher.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Manage Teacher</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="FindTextBox" runat="server" CssClass="form-control" placeholder="Enter search keywords here" Width="250px"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" OnClick="FindButton_Click" Text="Find" />
        </div>
    </div>

    <div class="alert alert-info">
        <asp:Label ID="CountLabel" runat="server"></asp:Label>
    </div>

    <span style="float: right; color: #ff6a00">By checked or unchecked Approved (activate/Deactivate) teacher Login access</span>
    <div class="table-responsive">
        <asp:GridView ID="TeacherGV" runat="server" AutoGenerateColumns="False" DataKeyNames="Name,UserName,Password,Phone,TeacherID" CssClass="mGrid" DataSourceID="Teacher_DetailsSQL">
            <Columns>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllCheckBox" runat="server" Text="SMS" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                    </ItemTemplate>
                    <HeaderStyle CssClass="NoPrint" />
                    <ItemStyle Width="20px" CssClass="NoPrint" />
                </asp:TemplateField>
                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                <asp:BoundField DataField="Name" HeaderText="Name" ReadOnly="True" SortExpression="Name" />
                <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                <asp:BoundField DataField="UserName" HeaderText="User Name" SortExpression="UserName" />
                <asp:BoundField DataField="Password" HeaderText="Password" SortExpression="Password" />
                <asp:TemplateField HeaderText="Approved?">
                    <ItemTemplate>
                        <asp:CheckBox ID="ApprovedCheckBox" Checked='<%#Eval("IsApproved") %>' runat="server" Text=" " OnCheckedChanged="ApprovedCheckBox_CheckedChanged" AutoPostBack="true" />
                    </ItemTemplate>
                    <HeaderStyle CssClass="NoPrint" />
                    <ItemStyle Width="20px" CssClass="NoPrint" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Locked Out?">
                    <ItemTemplate>
                        <asp:CheckBox ID="LockedOutCheckBox" runat="server" Checked='<%# Bind("IsLockedOut") %>' Text=" " OnCheckedChanged="LockedOutCheckBox_CheckedChanged" AutoPostBack="true" />
                    </ItemTemplate>
                    <HeaderStyle CssClass="NoPrint" />
                    <ItemStyle Width="20px" CssClass="NoPrint" />
                </asp:TemplateField>
                <asp:BoundField DataField="LastLoginDate" HeaderText="Last Login" SortExpression="LastLoginDate" />
            </Columns>
            <EmptyDataTemplate>
                No Record(s) Found!
            </EmptyDataTemplate>
            <PagerStyle CssClass="pgr"></PagerStyle>
        </asp:GridView>
        <asp:SqlDataSource ID="Teacher_DetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Teacher.FirstName + N' ' + Teacher.LastName AS Name, Teacher.Designation, Teacher.Phone, aspnet_Membership.Email, Registration.UserName, Registration.Validation, aspnet_Membership.IsApproved, aspnet_Membership.IsLockedOut, aspnet_Membership.CreateDate, aspnet_Membership.LastLoginDate, aspnet_Membership.LastPasswordChangedDate, AST.Password, Teacher.TeacherID, Employee_Info.ID FROM Teacher INNER JOIN Registration ON Teacher.TeacherRegistrationID = Registration.RegistrationID INNER JOIN AST ON Registration.UserName = AST.UserName INNER JOIN Employee_Info ON Teacher.EmployeeID = Employee_Info.EmployeeID LEFT OUTER JOIN aspnet_Users INNER JOIN aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId ON Registration.UserName = aspnet_Users.UserName WHERE (Registration.Category = N'Teacher') AND (Registration.SchoolID = @SchoolID) AND (Registration.Validation = N'Valid')"
            FilterExpression="Name LIKE '{0}%' or ID LIKE '{0}%' or Designation LIKE '{0}%' or Phone LIKE '{0}%' or UserName LIKE '{0}%'" CancelSelectOnNullParameter="False">
            <FilterParameters>
                <asp:ControlParameter ControlID="FindTextBox" Name="Find" PropertyName="Text" />
            </FilterParameters>
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="TeacherSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Teacher.TeacherID, Teacher.RegistrationID, Teacher.SchoolID, Teacher.Designation, Teacher.FirstName + Teacher.LastName AS Name, Teacher.FatherName, Teacher.Gender, Teacher.Age, Teacher.DateofBirth, Teacher.Religion, Teacher.Nationality, Teacher.NationalIDorPassportNO, Teacher.Address, Teacher.City, Teacher.PostalCode, Teacher.State, Teacher.Phone, Teacher.Email, Teacher.Date, Teacher.Image, Registration.Validation, Registration.UserName FROM Teacher INNER JOIN Registration ON Teacher.RegistrationID = Registration.RegistrationID WHERE (Teacher.SchoolID = @SchoolID)" UpdateCommand="UPDATE Registration SET Validation = @Validation WHERE UserName = @UserName">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
            <UpdateParameters>
                <asp:Parameter Name="Validation" />
                <asp:Parameter Name="UserName" />
            </UpdateParameters>
        </asp:SqlDataSource>
        <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any teacher" ForeColor="Red" ValidationGroup="CU"></asp:CustomValidator>
    </div>
   
    <asp:Button ID="SMS_Button" runat="server" CssClass="btn btn-primary" Text="Send Username And Password" ValidationGroup="CU" OnClick="SMS_Button_Click" />
    <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>

    <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
        <InsertParameters>
            <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
            <asp:Parameter Name="SchoolID" />
            <asp:Parameter Name="StudentID" />
            <asp:Parameter Name="TeacherID" />
            <asp:Parameter Name="EducationYearID" />
        </InsertParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            $("[id*=AllCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });
            $("[id*=SingleCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SingleCheckBox]", a).length == $("[id*=SingleCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        })

        /*--select at least one Checkbox GridView-----*/
        function Validate(d, c) {
            for (var b = document.getElementById("<%=TeacherGV.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                if ("checkbox" == b[a].type && b[a].checked) {
                    c.IsValid = !0;
                    return;
                }
            }
            c.IsValid = !1;
        };
    </script>
</asp:Content>
