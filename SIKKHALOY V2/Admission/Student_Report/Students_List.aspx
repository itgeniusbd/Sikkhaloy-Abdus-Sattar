<%@ Page Title="Student List" Language="C#" MasterPageFile="~/BASIC.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Students_List.aspx.cs" Inherits="EDUCATION.COM.Admission.Student_Rerport.Students_List" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <h3>Student(s) List.
                <asp:Label ID="CGSSLabel" runat="server"></asp:Label><div style="float:right"><a href="../../Administration_Basic_Settings/StudentFoult.aspx"> All Fault Or Report</a></div>
            </h3>
            <a class="NoPrint" href="../Student_Class_Info_update.aspx">Change academic info</a>
            

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
                <div class="form-group">
                    <asp:TextBox ID="IDTextBox" placeholder="Enter ID" autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="IDTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="F"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:Button ID="IDFindButton" runat="server" CssClass="btn btn-primary" OnClick="IDFindButton_Click" Text="Submit" ValidationGroup="F" />
                </div>
            </div>

            <label id="CountStudent"></label>
            <div class="table-responsive mb-2" id="Ex-word-data">
                <h1 id="I_Name"></h1>
                <h4 id="Export-info"></h4>
                <asp:GridView ID="StudentsGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" PagerStyle-CssClass="pgr"
                    DataKeyNames="StudentClassID,StudentID" CssClass="mGrid" DataSourceID="ShowStudentClassSQL" AllowSorting="True">
                    <AlternatingRowStyle CssClass="alt" />
                    <Columns>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:HyperLinkField DataNavigateUrlFields="StudentID,StudentClassID" DataNavigateUrlFormatString="Report.aspx?Student={0}&Student_Class={1}" DataTextField="StudentsName" HeaderText="Student's Name">
                            <ItemStyle HorizontalAlign="Left" />
                        </asp:HyperLinkField>
                        <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                        <asp:BoundField DataField="DateofBirth" HeaderText="D.O.B" SortExpression="DateofBirth" DataFormatString="{0:d MMM yyyy}" />
                        <asp:BoundField DataField="Legal_Identity" HeaderText="Legal Identity No." SortExpression="Legal_Identity" />
                        <asp:BoundField DataField="BloodGroup" HeaderText="Blood Group" SortExpression="BloodGroup" />
                        <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
                        <asp:BoundField DataField="FathersName" HeaderText="Father's Name" SortExpression="FathersName">
                            <ItemStyle HorizontalAlign="Left" />
                        </asp:BoundField>
                        <asp:BoundField DataField="MothersName" HeaderText="Mother's Name" SortExpression="MothersName">
                            <ItemStyle HorizontalAlign="Left" />
                        </asp:BoundField>
                        <%--<asp:BoundField DataField="StudentsLocalAddress" HeaderText="Present Address" SortExpression="StudentsLocalAddress" />
                          <asp:BoundField DataField="StudentPermanentAddress" HeaderText="Permanent Address" SortExpression="StudentPermanentAddress" />--%>
                        <asp:BoundField DataField="FatherPhoneNumber" HeaderText="F.Mobile" SortExpression="FatherPhoneNumber" />
                        <asp:BoundField DataField="MotherPhoneNumber" HeaderText="M.Mobile" SortExpression="MotherPhoneNumber" />
                        <asp:BoundField DataField="GuardianPhoneNumber" HeaderText="G.Mobile" SortExpression="GuardianPhoneNumber" />
                    </Columns>
                    <PagerStyle CssClass="pgr" />
                </asp:GridView>
                <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT Student.StudentID, Student.SMSPhoneNo, Student.StudentsName, Student.Gender, Student.StudentsLocalAddress, Student.MothersName, Student.FathersName, Student.FatherPhoneNumber, Student.GuardianName, StudentsClass.RollNo, Student.ID, Student.MotherPhoneNumber, Student.FatherOccupation, Student.GuardianPhoneNumber, StudentsClass.StudentClassID, Student.DateofBirth,Student.Legal_Identity,Student.BloodGroup, Student.StudentPermanentAddress FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo , '$' , '') , ',' , '') AS FLOAT) ELSE 0 END">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        <asp:Parameter DefaultValue="Active" Name="Status" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="ShowIDSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, Student.StudentsLocalAddress, Student.MothersName, Student.FathersName, StudentsClass.RollNo, Student.SMSPhoneNo, Student.Gender, Student.MotherPhoneNumber, Student.FatherPhoneNumber, Student.GuardianPhoneNumber, StudentsClass.StudentClassID, StudentsClass.StudentID, Student.DateofBirth,Student.BloodGroup FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (Student.ID = @ID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID)">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" Type="String" />
                        <asp:Parameter DefaultValue="Active" Name="Status" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="is_data d-print-none" style="display: none;">
                <input id="Print" type="button" onclick="window.print()" value="Print" class="btn btn-primary" />
                <input type="button" id="ExportWord" class="btn btn-primary" value="Export To Word" />
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


    <script src="/JS/ExportWord/FileSaver.js"></script>
    <script src="/JS/ExportWord/jquery.wordexport.js"></script>

    <script type="text/javascript">
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

            if ($("[id*=StudentsGridView] tr").length) {
                $('.is_data').show();
            }
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
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

            if ($("[id*=StudentsGridView] tr").length) {
                $('.is_data').show();
            }

            $("#CountStudent").text("TOTAL: " + $("[id*=StudentsGridView] td").closest("tr").length + " STUDENT");

            $("#ExportWord").click(function () {
                $('.mGrid a').each(function () {
                    $(this).removeAttr('href');
                });

                $("#Export-info").text($("[id*=CGSSLabel]").text());
                $("#I_Name").text($("#InstitutionName").text());

                $("#Ex-word-data").wordExport("CLASS-" + $('[id*=ClassDropDownList] :selected').text());
                $("#Export-info").text('');
                $("#I_Name").text('');
            });
        });
    </script>
</asp:Content>
