<%@ Page Title="Change Group/Section/Shift" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Change_Section_Shift_Group.aspx.cs" Inherits="EDUCATION.COM.Admission.Section_Shift_Group_Change_ofStudent.Change_Section_Shift_Group" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <div class="Contain">
                <h3>Change Group/Section/Shift</h3>

                <div class="form-inline NoPrint">
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
                </div>

                <div class="table-responsive">
                    <label id="CountStudent"></label>
                    <asp:GridView ID="StudentsGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" PagerStyle-CssClass="pgr" PageSize="60" CssClass="mGrid" DataKeyNames="StudentID,StudentClassID" DataSourceID="ShowStudentClassSQL" AllowSorting="True">
                        <AlternatingRowStyle CssClass="alt" />
                        <RowStyle CssClass="RowStyle" />
                        <Columns>
                            <asp:TemplateField HeaderText="Select">
                                <HeaderTemplate>
                                    <asp:CheckBox ID="SelectAllCheckBox" runat="server" Text=" " />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="SelectCheckBox" runat="server" Text=" " />
                                </ItemTemplate>
                                <HeaderStyle CssClass="NoPrint" />
                                <ItemStyle CssClass="NoPrint" />
                            </asp:TemplateField>
                            <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                            <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                            <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                            <asp:BoundField DataField="FathersName" HeaderText="Father's Name" SortExpression="FathersName" />
                            <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                            <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                            <asp:BoundField DataField="Section" HeaderText="Section" ReadOnly="True" SortExpression="Section" />
                            <asp:BoundField DataField="Group" HeaderText="Group" ReadOnly="True" SortExpression="Group" />
                            <asp:BoundField DataField="Shift" HeaderText="Shift" ReadOnly="True" SortExpression="Shift" />
                            <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
                        </Columns>
                        <PagerStyle CssClass="pgr" />
                        <SelectedRowStyle CssClass="Selected" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT Student.StudentID, StudentsClass.StudentClassID, StudentsClass.ClassID, Student.ID, Student.StudentsName, Student.FathersName, Student.Gender, CreateClass.Class, ISNULL(CreateSection.Section, N' -- ') AS Section, ISNULL(CreateSubjectGroup.SubjectGroup, N' -- ') AS [Group], ISNULL(CreateShift.Shift, N' -- ') AS Shift, Student.SMSPhoneNo, StudentsClass.RollNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (StudentsClass.ClassID = @ClassID) AND (Student.Status = N'Active') AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="UpdateStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [StudentsClass] WHERE ([EducationYearID] = @EducationYearID)" UpdateCommand="UPDATE StudentsClass SET SectionID = @SectionID, ShiftID = @ShiftID, SubjectGroupID = @SubjectGroupID WHERE (StudentClassID = @StudentClassID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="SectionID" />
                            <asp:Parameter Name="ShiftID" />
                            <asp:Parameter Name="SubjectGroupID" />
                            <asp:Parameter Name="StudentClassID" />
                        </UpdateParameters>
                    </asp:SqlDataSource>

                    <%if (StudentsGridView.Rows.Count > 0)
                        { %>
                    <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any student from student list." ForeColor="Red" ValidationGroup="1"></asp:CustomValidator>
                    <br />
                    <asp:Button ID="ChangeButton" runat="server" Text="Submit" CssClass="btn btn-primary" OnClick="ChangeButton_Click" ValidationGroup="1" />
                    <%} %>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

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
        /*--select at least one Checkbox Students GridView-----*/
        function Validate(d, c) {
            for (var b = document.getElementById("<%=StudentsGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                if ("checkbox" == b[a].type && b[a].checked) {
                    c.IsValid = !0;
                    return;
                }
            }
            c.IsValid = !1;
        };

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $("#CountStudent").text("TOTAL: " + $("[id*=StudentsGridView] td").closest("tr").length + " STUDENT.");

            $("[id*=SelectAllCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SelectCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=SelectAllCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SelectCheckBox]", a).length == $("[id*=SelectCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });
    </script>
</asp:Content>
