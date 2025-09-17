<%@ Page Title="Block/Unblock/Locked out/Delete Student" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Blocked_Unblocked_Student_Temporarily.aspx.cs" Inherits="EDUCATION.COM.ADMISSION_REGISTER.Blocked_Unblocked_Student_Temporarily" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Block/Unblock/Locked out/Delete Student</h3>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup"
                        DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="IDTextBox" placeholder="Enter ID" autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="IDTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="F"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:Button ID="IDFindButton" runat="server" CssClass="btn btn-primary" OnClick="IDFindButton_Click" Text="Find" ValidationGroup="F" />
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="StudentGridView" runat="server" AutoGenerateColumns="False" AllowPaging="True"
                    PagerStyle-CssClass="pgr" DataKeyNames="UserName" CssClass="mGrid" PageSize="40">
                    <Columns>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                        <asp:BoundField DataField="SMSPhoneNo" HeaderText="Phone" SortExpression="SMSPhoneNo" />
                        <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                        <asp:BoundField DataField="UserName" HeaderText="Userid" SortExpression="UserName" />
                        <asp:BoundField DataField="Validation" HeaderText="Validation" SortExpression="Validation" />
                        <asp:TemplateField HeaderText="Approved?">
                            <ItemTemplate>
                                <asp:CheckBox ID="ApprovedCheckBox" runat="server" Checked='<%# Bind("IsApproved") %>' Text=" " OnCheckedChanged="ApprovedCheckBox_CheckedChanged" AutoPostBack="true" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Locked Out?">
                            <ItemTemplate>
                                <asp:CheckBox ID="LockedOutCheckBox" runat="server" Checked='<%# Bind("IsLockedOut") %>' Text=" " OnCheckedChanged="LockedOutCheckBox_CheckedChanged" AutoPostBack="true" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="CreateDate" HeaderText="Create Date" SortExpression="CreateDate" />
                        <asp:BoundField DataField="LastLoginDate" HeaderText="Last Login" SortExpression="LastLoginDate" />
                        <asp:BoundField DataField="LastPasswordChangedDate" HeaderText="Last Password Changed" SortExpression="LastPasswordChangedDate" />
                        <asp:TemplateField HeaderText="Delete">
                            <ItemTemplate>
                                <asp:LinkButton ID="DeleteLinkButton" runat="server" CommandName='<%#Eval("UserName") %>' CommandArgument='<%#Eval("RegistrationID") %>' OnCommand="DeleteLinkButton_Command" OnClientClick="return confirm('This User will be delete permanently. Are you sure?')">Delete</asp:LinkButton>
                            </ItemTemplate>
                            <HeaderStyle CssClass="NoPrint" />
                            <ItemStyle CssClass="NoPrint" />
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        No Record(s) Found!
                    </EmptyDataTemplate>
                    <PagerStyle CssClass="pgr"></PagerStyle>
                </asp:GridView>
                <asp:SqlDataSource ID="StudentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, aspnet_Membership.Email, Registration.UserName, Registration.Validation, aspnet_Membership.IsApproved, aspnet_Membership.IsLockedOut, aspnet_Membership.CreateDate, aspnet_Membership.LastLoginDate, aspnet_Membership.LastPasswordChangedDate, Student.SMSPhoneNo, Registration.RegistrationID FROM aspnet_Users INNER JOIN aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId INNER JOIN Registration ON aspnet_Users.UserName = Registration.UserName INNER JOIN Student ON Registration.RegistrationID = Student.StudentRegistrationID INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (Registration.Category = N'Student') AND (Registration.SchoolID = @SchoolID) AND (Student.Status = 'Active') AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = 'Active') AND (StudentsClass.EducationYearID = @EducationYearID)">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="Student_ID_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, aspnet_Membership.Email, Registration.UserName, Registration.Validation, aspnet_Membership.IsApproved, aspnet_Membership.IsLockedOut, aspnet_Membership.CreateDate, aspnet_Membership.LastLoginDate, aspnet_Membership.LastPasswordChangedDate, Student.SMSPhoneNo, Registration.RegistrationID FROM aspnet_Users INNER JOIN aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId INNER JOIN Registration ON aspnet_Users.UserName = Registration.UserName INNER JOIN Student ON Registration.RegistrationID = Student.StudentRegistrationID WHERE (Registration.Category = N'Student') AND (Registration.SchoolID = @SchoolID) AND (Student.Status = 'Active') AND (Student.ID = @ID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="LITSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM AST WHERE (UserName = @UserName)
DELETE FROM Registration WHERE (RegistrationID = @RegistrationID)
UPDATE       Student SET  StudentRegistrationID = NULL
WHERE        (StudentRegistrationID = @RegistrationID)"
                    SelectCommand="SELECT * FROM [AST]">
                    <DeleteParameters>
                        <asp:Parameter Name="UserName" />
                        <asp:Parameter Name="RegistrationID" />
                    </DeleteParameters>
                </asp:SqlDataSource>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script>
        $(document).ready(function () {
            $('[id*=IDTextBox]').typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "/Handeler/Student_IDs.asmx/GetStudentID",
                        data: JSON.stringify({ 'ids': request }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (response) {
                            result($.map(JSON.parse(response.d), function (item) {
                                return item;
                            }));
                        }
                    });
                }
            });

            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, e) {
                $('[id*=IDTextBox]').typeahead({
                    minLength: 1,
                    source: function (request, result) {
                        $.ajax({
                            url: "/Handeler/Student_IDs.asmx/GetStudentID",
                            data: JSON.stringify({ 'ids': request }),
                            dataType: "json",
                            type: "POST",
                            contentType: "application/json; charset=utf-8",
                            success: function (response) {
                                result($.map(JSON.parse(response.d), function (item) {
                                    return item;
                                }));
                            }
                        });
                    }
                });
            });
        });
    </script>
</asp:Content>
