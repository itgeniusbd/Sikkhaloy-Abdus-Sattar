<%@ Page Title="Change Roll No/Group/Section/Shift" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Change_Student_RollNo_Group_Section_Shift.aspx.cs" Inherits="EDUCATION.COM.Admission.Change_Student_RollNo_Group_Section_Shift" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Student_List.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <h3>Change Roll No/Group/Section/Shift</h3>
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <div class="form-inline">
                <div class="form-group">
                    <asp:TextBox ID="IDTextBox" placeholder="Enter Student ID" autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="IDTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="F"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:Button ID="IDFindButton" runat="server" CssClass="btn btn-primary" Text="GO" ValidationGroup="F" OnClick="IDFindButton_Click" />
                </div>
            </div>

            <asp:FormView ID="StudentInfoFormView" Width="100%" runat="server" DataKeyNames="StudentID,ClassID,SectionID,ShiftID,SubjectGroupID,StudentClassID,RollNo" DataSourceID="StudentInfoSQL">
                <ItemTemplate>
                    <div class="z-depth-1 mb-4 p-3">
                        <div class="d-flex flex-sm-row flex-column text-center text-sm-left">
                            <div class="p-image">
                                <img alt="No Image" src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="img-thumbnail rounded-circle z-depth-1" />
                            </div>
                            <div class="info">
                                <ul>
                                    <li>
                                        <b>(<%# Eval("ID") %>)
                                        <%# Eval("StudentsName") %></b>
                                    </li>
                                    <li>
                                        <b>Father's Name:</b>
                                        <%# Eval("FathersName") %>
                                    </li>
                                    <li class="alert-info">
                                        <b>Class:</b>
                                        <%# Eval("Class") %>
                                        <%# Eval("SubjectGroup",", Group: {0}") %>
                                        <%# Eval("Section",", Section: {0}") %>
                                        <%# Eval("Shift",", Shift: {0}") %>
                                    </li>
                                    <li><b>Roll No:</b>
                                        <%# Eval("RollNo") %>
                                    </li>
                                    <li><b>Phone:</b>
                                        <%# Eval("SMSPhoneNo") %>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CreateClass.Class,CreateSection.Section,CreateSubjectGroup.SubjectGroup, Student.StudentsName, Student.FathersName, Student.ID, Student.SMSPhoneNo, Student.Gender, Student.DateofBirth, Student.BloodGroup, Student.Religion,CreateShift.Shift, StudentsClass.RollNo, Student.MothersName, Student.FatherPhoneNumber, Student.StudentID, Student.StudentImageID, StudentsClass.ClassID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID, StudentsClass.StudentClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.ID = @ID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.Status = @Status)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
                    <asp:Parameter DefaultValue="Active" Name="Status" />
                </SelectParameters>
            </asp:SqlDataSource>

            <%if (StudentInfoFormView.DataItemCount > 0)
                { %>
            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                        <SelectParameters>
                            <asp:Parameter DefaultValue="" Name="ClassID" />
                            <asp:Parameter DefaultValue="" Name="SectionID" />
                            <asp:Parameter DefaultValue="" Name="ShiftID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="GRequiredFieldValidator" runat="server" ControlToValidate="GroupDropDownList" CssClass="EroorStar" ErrorMessage="Select group" InitialValue="%" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
                        <SelectParameters>
                            <asp:Parameter Name="ClassID" />
                            <asp:Parameter Name="SubjectGroupID" />
                            <asp:Parameter Name="ShiftID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="SeRequiredFieldValidator" runat="server" ControlToValidate="SectionDropDownList" CssClass="EroorStar" ErrorMessage="Select section" InitialValue="%" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:Parameter Name="SubjectGroupID" />
                            <asp:Parameter Name="SectionID" />
                            <asp:Parameter Name="ClassID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="ShRequiredFieldValidator" runat="server" ControlToValidate="ShiftDropDownList" CssClass="EroorStar" ErrorMessage="Select shift" InitialValue="%" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="RollNumberTextBox" placeholder="Roll No" runat="server" CssClass="form-control"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RollRequiredFieldValidator" runat="server" ControlToValidate="RollNumberTextBox" CssClass="EroorStar" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:Button ID="UpdateButton" runat="server" CssClass="btn btn-primary" Text="Update" ValidationGroup="1" OnClick="UpdateButton_Click" />
                </div>
            </div>
            <%} %>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="/CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script type="text/javascript">
        $(document).ready(function () {
            $('[id*=IDTextBox]').typeahead({
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

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $('[id*=IDTextBox]').typeahead({
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
        })
    </script>
</asp:Content>
