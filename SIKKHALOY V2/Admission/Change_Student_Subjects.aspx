<%@ Page Title="Change Student's Subjects" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Change_Student_Subjects.aspx.cs" Inherits="EDUCATION.COM.Admission.Change_Student_Subjects" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Student_List.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Change Student's Subjects</h3>

    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
                   <div class="form-inline">
                <div class="form-group">
                    <asp:TextBox ID="IDTextBox" autocomplete="off" placeholder="Enter Student ID" runat="server" CssClass="form-control"></asp:TextBox>
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
                SelectCommand="SELECT CreateClass.Class,CreateSection.Section, CreateSubjectGroup.SubjectGroup, Student.StudentsName, Student.FathersName, Student.ID, Student.SMSPhoneNo, Student.Gender, Student.DateofBirth, Student.BloodGroup, Student.Religion, CreateShift.Shift, StudentsClass.RollNo, Student.MothersName, Student.FatherPhoneNumber, Student.StudentID, Student.StudentImageID, StudentsClass.ClassID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.SubjectGroupID, StudentsClass.StudentClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.ID = @ID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.Status = @Status)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
                    <asp:Parameter DefaultValue="Active" Name="Status" />
                </SelectParameters>
            </asp:SqlDataSource>

            <%if (StudentInfoFormView.DataItemCount > 0)
            { %>
            <div class="table-responsive">
                <div class="alert-info">Choose subject and subject type</div>
                <asp:GridView ID="GroupGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="SubjectID" DataSourceID="SubjectGroupSQL"
                    CssClass="mGrid" PagerStyle-CssClass="pgr" AlternatingRowStyle-CssClass="alt" PageSize="20">
                    <AlternatingRowStyle CssClass="alt" />
                    <RowStyle CssClass="RowStyle" />
                    <Columns>
                        <asp:BoundField DataField="SubjectName" HeaderText="Subjects" SortExpression="SubjectName" />
                        <asp:TemplateField ShowHeader="False" HeaderText="Select">
                            <ItemTemplate>
                                <asp:CheckBox ID="SubjectCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" Width="30px" />
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Subject Type">
                            <ItemTemplate>
                                <asp:RadioButtonList ID="SubjectTypeRadioButtonList" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem Selected="True">Compulsory</asp:ListItem>
                                    <asp:ListItem>Optional</asp:ListItem>
                                </asp:RadioButtonList>
                            </ItemTemplate>
                            <ItemStyle Width="175px" />
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        Empty
                    </EmptyDataTemplate>
                    <PagerStyle CssClass="pgr" />
                    <SelectedRowStyle CssClass="Selected" />
                </asp:GridView>

                <br />
                <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Change Subjects" ValidationGroup="2" />
                <asp:SqlDataSource ID="SubjectGroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM Subject WHERE (SchoolID = @SchoolID) ORDER BY SubjectName">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <asp:SqlDataSource ID="StudentSubjectRecordsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO StudentRecord(StudentID, RegistrationID, SchoolID, StudentClassID, SubjectID, EducationYearID, SubjectType, Date) VALUES (@StudentID, @RegistrationID, @SchoolID, @StudentClassID, @SubjectID, @EducationYearID, @SubjectType, GETDATE())" SelectCommand="SELECT * FROM StudentRecord" DeleteCommand="DELETE FROM StudentRecord WHERE (StudentClassID = @StudentClassID) AND (SchoolID = @SchoolID)">
                    <DeleteParameters>
                        <asp:Parameter Name="StudentClassID" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </DeleteParameters>
                    <InsertParameters>
                        <asp:Parameter Name="StudentID" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                        <asp:Parameter Name="StudentClassID" />
                        <asp:Parameter Name="SubjectID" Type="Int32" />
                        <asp:Parameter Name="SubjectType" Type="String" />
                    </InsertParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="UpdateStudentRecordIDSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT StudentRecordID, StudentID, RegistrationID, SchoolID, StudentClassID, SubjectID, EducationYearID, SubjectType, Date FROM StudentRecord" UpdateCommand="UPDATE
Exam_Obtain_Marks
SET 
     Exam_Obtain_Marks.StudentRecordID =  StudentRecord.StudentRecordID
FROM            
   Exam_Obtain_Marks
INNER JOIN
   StudentRecord
ON 
StudentRecord.EducationYearID = Exam_Obtain_Marks.EducationYearID AND
StudentRecord.SubjectID = Exam_Obtain_Marks.SubjectID AND 
StudentRecord.StudentID = Exam_Obtain_Marks.StudentID AND 
StudentRecord.SchoolID = Exam_Obtain_Marks.SchoolID
WHERE (StudentRecord.StudentClassID = @StudentClassID)

UPDATE  
Exam_Result_of_Subject
SET    
 Exam_Result_of_Subject.StudentRecordID = StudentRecord.StudentRecordID
FROM          
  StudentRecord 
INNER JOIN
 Exam_Result_of_Subject 
ON 
StudentRecord.StudentID = Exam_Result_of_Subject.StudentID AND 
StudentRecord.EducationYearID = Exam_Result_of_Subject.EducationYearID AND 
StudentRecord.SubjectID = Exam_Result_of_Subject.SubjectID AND 
StudentRecord.SchoolID = Exam_Result_of_Subject.SchoolID
WHERE (StudentRecord.StudentClassID = @StudentClassID)">
                    <UpdateParameters>
                        <asp:Parameter Name="StudentClassID" />
                    </UpdateParameters>
                </asp:SqlDataSource>

            </div>
            <%} %>
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
        $(function () {
            //Query String
            if (document.location.search.length) {
                function GetQueryString(name) {
                    var sPageURL = window.location.search.substring(1);
                    var sURLVariables = sPageURL.split('&');
                    for (var i = 0; i < sURLVariables.length; i++) {
                        var sParameterName = sURLVariables[i].split('=');
                        if (sParameterName[0] == name) {
                            return sParameterName[1];
                        }
                    }
                }

                $("#<%=IDTextBox.ClientID%>").val(GetQueryString('id'));
                $("#<%=IDFindButton.ClientID%>").click();
            }

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


        function DisableButton() {
            document.getElementById("<%=SubmitButton.ClientID %>").disabled = !0;
        }
        window.onbeforeunload = DisableButton;

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

            //Add or Remove CheckBox Selected Color
            $("[id*=SubjectCheckBox]").on("click", function () {
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected")) : ($("td", $(this).closest("tr")).removeClass("selected"), $($(this).closest("tr")).removeClass("selected"));
            });
        })


    </script>
</asp:Content>
