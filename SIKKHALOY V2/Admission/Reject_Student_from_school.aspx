<%@ Page Title="TC" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Reject_Student_from_school.aspx.cs" Inherits="EDUCATION.COM.ADMISSION_REGISTER.Reject_Student_from_school" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Reject_Student.css?v=2" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <ul class="nav nav-tabs z-depth-1">
        <li class="nav-item">
            <a class="nav-link active" data-toggle="tab" href="#panel1" role="tab" aria-expanded="true">Giving TC</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" data-toggle="tab" href="#panel2" role="tab" aria-expanded="false">TC List</a>
        </li>
    </ul>

    <div class="tab-content card">
        <div class="tab-pane fade in active show" id="panel1" role="tabpanel" aria-expanded="true">
            <label>Enter Student ID to giving TC</label>
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate></ContentTemplate>
            </asp:UpdatePanel>
            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:TextBox ID="IDTextBox" autocomplete="off" placeholder="Enter Student ID" runat="server" CssClass="form-control Sid"></asp:TextBox>
                </div>

                <div class="form-group">
                    <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find" ValidationGroup="1" />
                </div>
            </div>

            <asp:FormView ID="StudentInfoFormView" runat="server" DataKeyNames="StudentID,StudentClassID,Status,ActiveTime,DeactivateTime" DataSourceID="Reject_StudentInfoSQL" Width="100%" CssClass="NoPrint" OnDataBound="StudentInfoFormView_DataBound">
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
                                    <li>
                                        <b>Session Year:</b>
                                        <%# Eval("EducationYear") %>
                                    </li>
                                    <li class="isStatus"><%# Eval("Status") %></li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>

            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:RadioButtonList ID="PayorderRadioButtonList" runat="server" CssClass="form-control" RepeatDirection="Horizontal">
                        <asp:ListItem Selected="True">Delete All Pay order</asp:ListItem>
                        <asp:ListItem>Keep current dues</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
                <div class="form-group">
                    <asp:Button ID="RejectButton" runat="server" CssClass="btn btn-primary" OnClick="RejectButton_Click" ValidationGroup="1" Text="giving TC" />
                </div>
                <div class="form-group">
                    <asp:Button ID="ActiveButton" runat="server" CssClass="btn btn-primary" OnClick="ActiveButton_Click" Text="Active" ValidationGroup="1" />
                </div>
            </div>

            <asp:SqlDataSource ID="Reject_StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, CreateSection.Section, CreateSubjectGroup.SubjectGroup, StudentsClass.ClassID, Student.StudentsName, Student.FathersName, Student.StudentID, Student.StudentImageID, Student.ID, Student.Gender, Student.DateofBirth, CreateShift.Shift, Student.Status, Student.SMSPhoneNo, StudentsClass.RollNo, StudentsClass.StudentClassID, Education_Year.EducationYear, Student.ActiveTime, Student.DeactivateTime FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.ID = @ID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.Class_Status IS NULL)" UpdateCommand="UPDATE Student SET Status = @Status, RejectedDate = GETDATE(), StudentRegistrationID = NULL, DeactivateTime = GETDATE() WHERE (ID = @ID) AND (SchoolID = @SchoolID)
UPDATE Student SET  ActiveDays = DAY(GETDATE()) WHERE (SchoolID = @SchoolID) AND (StudentID = @StudentID) AND (FORMAT(ActiveDate, 'MMM yyyy') &lt;&gt; FORMAT(GETDATE(), 'MMM yyyy')) 
UPDATE Student SET  ActiveDays = DATEDIFF(day, ActiveDate, GETDATE()) WHERE (SchoolID = @SchoolID) AND (StudentID = @StudentID) AND (FORMAT(ActiveDate, 'MMM yyyy') = FORMAT(GETDATE(), 'MMM yyyy')) AND (ISNULL(ActiveDays,0) &lt; DATEDIFF(day, ActiveDate, GETDATE()))">
                <SelectParameters>
                    <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
                <UpdateParameters>
                    <asp:Parameter DefaultValue="Rejected" Name="Status" />
                    <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:Parameter DefaultValue="" Name="StudentID" />
                </UpdateParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="PayOrderDeleteSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM Income_PayOrder
FROM            Income_PayOrder INNER JOIN
                         Student ON Income_PayOrder.StudentID = Student.StudentID
WHERE        (Income_PayOrder.PaidAmount &lt;= 0) AND (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EndDate &gt;= ISNULL(@EndDate, '1-1-1000')) AND (Student.ID = @ID)"
                SelectCommand="SELECT PayOrderID FROM Income_PayOrder" UpdateCommand="UPDATE       Income_PayOrder
SET                Is_Active = 1
FROM            Income_PayOrder INNER JOIN
                         Student ON Income_PayOrder.StudentID = Student.StudentID
WHERE        (Income_PayOrder.PaidAmount &lt;= 0) AND (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EndDate &lt;= GETDATE()) AND (Student.ID = @ID)">
                <DeleteParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:Parameter Name="EndDate" />
                    <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
                </DeleteParameters>
                <UpdateParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
                </UpdateParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="ActiveSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT StudentID, RegistrationID, SchoolID, StudentRegistrationID, StudentImageID, ID, SMSPhoneNo, StudentsName, StudentEmailAddress, Gender, DateofBirth, BloodGroup, Religion, StudentPermanentAddress, StudentsLocalAddress, PrevSchoolName, PrevClass, PrevExamYear, PrevExamGrade, MothersName, MotherOccupation, MotherPhoneNumber, FathersName, FatherOccupation, FatherPhoneNumber, GuardianName, GuardianRelationshipwithStudent, GuardianPhoneNumber, OtherDetails, Status FROM Student WHERE (ID = @ID) AND (SchoolID = @SchoolID)" UpdateCommand="UPDATE Student SET Status = @Status, ActiveTime = GETDATE(), ActiveDate = GETDATE() WHERE (ID = @ID) AND (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
                <UpdateParameters>
                    <asp:Parameter DefaultValue="Active" Name="Status" Type="String" />
                    <asp:ControlParameter ControlID="IDTextBox" DefaultValue="" Name="ID" PropertyName="Text" Type="String" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </UpdateParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="ActDeActLogSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Student_Act_Deactivate_Log(SchoolID, RegistrationID, StudentClassID, StudentID, Status, Act_Deact_Time) VALUES (@SchoolID, @RegistrationID, @StudentClassID, @StudentID, @Status, @time)" SelectCommand="SELECT * FROM [Student_Act_Deactivate_Log]">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                    <asp:Parameter Name="StudentClassID" Type="Int32" />
                    <asp:Parameter Name="StudentID" Type="Int32" />
                    <asp:Parameter Name="Status" Type="String" />
                    <asp:Parameter Name="time" />
                </InsertParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="Device_DataUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT DateUpdateID FROM  Attendance_Device_DataUpdateList WHERE (SchoolID = @SchoolID) AND (UpdateType = @UpdateType))
BEGIN
INSERT INTO Attendance_Device_DataUpdateList(SchoolID, RegistrationID, UpdateType, UpdateDescription) VALUES (@SchoolID, @RegistrationID, @UpdateType, @UpdateDescription)
END" SelectCommand="SELECT * FROM [Attendance_Device_DataUpdateList]">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                    <asp:Parameter DefaultValue="" Name="UpdateType" Type="String" />
                    <asp:Parameter DefaultValue="" Name="UpdateDescription" Type="String" />
                </InsertParameters>
            </asp:SqlDataSource>
        </div>

        <div class="tab-pane fade" id="panel2" role="tabpanel" aria-expanded="false">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <label class="NoPrint">Find Student From TC List</label>
                    <div class="form-inline NoPrint">
                        <div class="form-group">
                            <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                                <asp:ListItem Value="%">[ ALL CLASS ]</asp:ListItem>
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>

                        <div class="form-group">
                            <asp:TextBox ID="TiIDTextBox" autocomplete="off" placeholder="Enter Student ID" runat="server" CssClass="form-control Sid"></asp:TextBox>
                        </div>

                        <div class="form-group">
                            <asp:Button ID="TcFindButton" runat="server" Text="Find" CssClass="btn btn-primary" />
                        </div>
                    </div>

                    <div class="alert alert-success" style="margin-top: 20px;">
                        <asp:Label ID="CountStudentLabel" runat="server"></asp:Label>
                    </div>

                    <div class="table-responsive">
                        <asp:GridView ID="StatusGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="StudentID" DataSourceID="StatusSQL"
                            PagerStyle-CssClass="pgr" CssClass="mGrid" AllowSorting="True" AllowPaging="True" PageSize="50">
                            <AlternatingRowStyle CssClass="alt" />
                            <Columns>
                                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                                <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                                <asp:BoundField DataField="FathersName" HeaderText="Father's Name" SortExpression="FathersName" />
                                <asp:BoundField DataField="SMSPhoneNo" HeaderText="Mobile No." SortExpression="SMSPhoneNo" />
                                <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                                <asp:BoundField DataField="Group" HeaderText="Group" ReadOnly="True" SortExpression="Group" />
                                <asp:BoundField DataField="Section" HeaderText="Section" ReadOnly="True" SortExpression="Section" />
                                <asp:BoundField DataField="Shift" HeaderText="Shift" SortExpression="Shift" />
                                <asp:BoundField DataField="RejectedDate" SortExpression="RejectedDate" DataFormatString="{0:d MMM yyyy}" HeaderText="TC Date" />
                                <asp:TemplateField HeaderText="Print TC">
                                    <ItemTemplate>
                                        <a href="Print_TC.aspx?Student=<%#Eval("StudentID") %>&S_Class=<%#Eval("StudentClassID") %>">Print</a>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                No Deactivate Student Found!
                            </EmptyDataTemplate>

                            <PagerStyle CssClass="pgr"></PagerStyle>
                        </asp:GridView>
                        <asp:SqlDataSource ID="StatusSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, ISNULL(CreateSection.Section, 'No section') AS Section, ISNULL(CreateSubjectGroup.SubjectGroup, 'No Group') AS [Group], StudentsClass.ClassID, CreateShift.Shift, Student.StudentID, Student.ID, Student.SMSPhoneNo, Student.StudentsName, Student.Gender, Student.StudentPermanentAddress, Student.Status, Student.RejectedDate, Student.FathersName, StudentsClass.StudentClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.Status = @Status) AND (Student.SchoolID = @SchoolID) AND (StudentsClass.Class_Status IS NULL)  AND (StudentsClass.ClassID LIKE ISNULL(@ClassID, N'%')) AND (Student.ID LIKE ISNULL(@ID, N'%'))" UpdateCommand="UPDATE Student SET Status = @Status WHERE (ID = @ID)" OnSelected="StatusSQL_Selected" CancelSelectOnNullParameter="False">
                            <SelectParameters>
                                <asp:Parameter DefaultValue="Rejected" Name="Status" />
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:ControlParameter ControlID="ClassDropDownList" DefaultValue="" Name="ClassID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="TiIDTextBox" Name="ID" PropertyName="Text" />
                            </SelectParameters>
                            <UpdateParameters>
                                <asp:Parameter DefaultValue="Reject" Name="Status" />
                                <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
                            </UpdateParameters>
                        </asp:SqlDataSource>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>


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

    <script type="text/javascript">
        $(function () {
            $('.Sid').typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "Reject_Student_from_school.aspx/GetAllID",
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

            if ($(".isStatus").text() === "Active") {
                $(".isStatus").css("color", "green");
            }
            else {
                $(".isStatus").css("color", "red");
            }
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            if ($(".isStatus").text() === "Active") {
                $(".isStatus").css("color", "green");
            }
            else {
                $(".isStatus").css("color", "red");
            }

            $('.Sid').typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "Reject_Student_from_school.aspx/GetAllID",
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
    </script>
</asp:Content>
