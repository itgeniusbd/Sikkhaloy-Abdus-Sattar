<%@ Page Title="Student Report" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="EDUCATION.COM.Admission.Student_Rerport.Report" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../../Employee/CSS/Acadamic_Calender.css" rel="stylesheet" />
    <link href="../CSS/Report.css?v=12" rel="stylesheet" />

    <style>
        /*Allover report*/
        #ResultAnalysis { background-color: #f0f1f3; padding: 2.5rem 1rem 1rem; margin: -2.5rem -1rem -1rem; }
        .statistic { white-space: nowrap; overflow: hidden; padding: 20px 2px 20px 10px; margin-bottom: 15px; margin-right: 8px; }
        .has-shadow { -webkit-box-shadow: 2px 2px 2px rgba(0,0,0,0.1),-1px 0 2px rgba(0,0,0,0.05); box-shadow: 2px 2px 2px rgba(0,0,0,0.1),-1px 0 2px rgba(0,0,0,0.05); }
        .icon { width: 40px; height: 40px; line-height: 40px; text-align: center; min-width: 40px; max-width: 40px; color: #fff; border-radius: 50%; margin-right: 10px; }
        .statistic strong { font-size: 1.5em; color: #333; font-weight: 700; line-height: 1; }
        .statistic small { color: #707070; text-transform: uppercase; }
        .has-shadow { -webkit-box-shadow: 2px 2px 2px rgba(0,0,0,0.1),-1px 0 2px rgba(0,0,0,0.05); box-shadow: 2px 2px 2px rgba(0,0,0,0.1),-1px 0 2px rgba(0,0,0,0.05); }
        .icon { width: 40px; height: 40px; line-height: 40px; text-align: center; min-width: 40px; max-width: 40px; color: #fff; border-radius: 50%; margin-right: 10px; }

        .statistic2 { white-space: nowrap; overflow: hidden; padding: 20px 2px 20px 10px; margin-bottom: 15px; }
        .statistic2 strong { font-size: 1.1em; color: #333; font-weight: 700; line-height: 1; }
        .statistic2 small { color: #818181; font-size: 1rem; text-transform: uppercase; }
        @media (min-width: 992px) {
  .col-lg-3 {
    -ms-flex: 0 0 25%;
    flex: 0 0 25%;
    max-width: 100%;
  }
}
.p-image img {
  height: 140px;
  width: 140px;
}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <a href="Students_List.aspx" class="NoPrint"><< Back to List</a>

    <div class="form-inline d-print-none">
        <div class="form-group">
            <asp:TextBox ID="IDTextBox" placeholder="Enter ID" autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="IDTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="F"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="IDFindButton" runat="server" CssClass="btn btn-primary" OnClick="IDFindButton_Click" Text="Find Student" ValidationGroup="F" />
            <asp:SqlDataSource ID="ShowIDSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, Student.StudentsLocalAddress, Student.MothersName, Student.FathersName, StudentsClass.RollNo, Student.SMSPhoneNo, Student.Gender, Student.MotherPhoneNumber, Student.FatherPhoneNumber, Student.GuardianPhoneNumber, StudentsClass.StudentClassID, StudentsClass.StudentID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (Student.ID = @ID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" Type="String" />
                    <asp:Parameter DefaultValue="Active" Name="Status" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
    </div>

    <asp:FormView ID="StudentInfoFormView" runat="server" DataKeyNames="ClassID" DataSourceID="StudentInfoSQL" Width="100%">
        <ItemTemplate>
            <input id="StudentID" type="hidden" value="<%# Eval("StudentID") %>" />
            <div class="row">
                <div class="col-lg-9 col-md-8">
                    <div class="z-depth-1 mb-4 p-3">
                        <div class="d-flex flex-sm-row flex-column text-center text-sm-left">
                            <div class="p-image">
                                <img alt="No Image" src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="img-thumbnail rounded-circle z-depth-1" />
                            </div>
                            <div class="info">
                                <ul>
                                    <li>
                                        
                                 <%# Eval("StudentsName") %></b>
                                        <b>(ID: <label id="IDLabel"><%# Eval("ID") %></label>)
                                    </li>
                                    <li>
                                        <b>Father's Name:</b>
                                        <%# Eval("FathersName") %>, <b>Phone:</b>
                                        <%# Eval("SMSPhoneNo") %>
                                    </li>
                                    <li class="alert-info">
                                        <b>Class:</b>
                                        <%# Eval("Class") %>
                                         Roll No: <%# Eval("RollNo") %>
                                        <%# Eval("SubjectGroup",", Group: {0}") %>
                                        <%# Eval("Section",", Section: {0}") %>
                                        <%# Eval("Shift",", Shift: {0}") %>
                                       
                                    </li>

                                    <li class="NoPrint">
                                        <a target="_blank" href="../New_Student_Admission/Admission_Form.aspx?Student=<%# Eval("StudentID") %>&StudentClass=<%# Eval("StudentClassID") %>">Print Admission Form</a> ----
                                        <a target="_blank" href="../New_Student_Admission/Form_Bangla.aspx?Student=<%# Eval("StudentID") %>&StudentClass=<%# Eval("StudentClassID") %>">প্রিন্ট ভর্তি ফরম (বাংলা)</a>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-lg-3 col-md-4 NoPrint">
                    <div class="card mb-3">
                        <div class="card-header bg-white d-print-none">
                            <b>Guardian Info </b>
                             <a class="pull-right" href="../Edit_Student_Info/Edit_Student_information.aspx?Student=<%# Eval("StudentID") %>&Student_Class=<%# Eval("StudentClassID") %>"><i class="fa fa-pencil-square mr-1" aria-hidden="true"></i>Update </a>
                        </div>
                        <div class="py-2">
                            <img class="gimg img-thumbnail" src="/Handeler/Guardian_Photo.ashx?SID=<%# Eval("StudentImageID") %>" />
                            <div class="text-center"><strong><%# Eval("GuardianName") %></strong> <small><%# Eval("GuardianRelationshipwithStudent") %></small></div>
                            <div class="text-center">Mobile: <%# Eval("GuardianPhoneNumber") %></div>
                        </div>
                    </div>
                </div>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT Student.ID, Student.StudentsName, Student.FathersName, CreateClass.Class, StudentsClass.RollNo, CreateSection.Section, CreateSubjectGroup.SubjectGroup, CreateShift.Shift, Student.SMSPhoneNo, Student.StudentImageID, Student.StudentID, Student.SchoolID, Student.StudentEmailAddress, Student.DateofBirth, Student.BloodGroup, Student.Religion, Student.Gender, Student.StudentPermanentAddress, Student.StudentsLocalAddress, Student.PrevSchoolName, Student.PrevClass, Student.PrevExamYear, Student.PrevExamGrade, Student.MothersName, Student.MotherOccupation, Student.MotherPhoneNumber, Student.FatherOccupation, Student.FatherPhoneNumber, Student.GuardianName, Student.GuardianRelationshipwithStudent, Student.GuardianPhoneNumber, Student.OtherDetails, Student.AdmissionDate, StudentsClass.ClassID, StudentsClass.StudentClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.SchoolID = @SchoolID) AND (Student.StudentID = @StudentID) AND (StudentsClass.EducationYearID = @EducationYearID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
        </SelectParameters>
    </asp:SqlDataSource>


    <ul class="nav nav-tabs nav-justified">
        <li class="nav-item"><a class="nav-link active" href="#ResultAnalysis" data-toggle="tab" role="tab" aria-expanded="true">Result Analysis</a></li>
        <li class="nav-item"><a class="nav-link" href="#Attendance" data-toggle="tab" role="tab" aria-expanded="false">Attendance</a></li>
        <li class="nav-item"><a class="nav-link" href="#Individual_Exam" data-toggle="tab" role="tab" aria-expanded="false">Individual Result</a></li>
        <li class="nav-item"><a class="nav-link" href="#CumulativeResult" data-toggle="tab" role="tab" aria-expanded="false">Cumulative Result</a></li>
        <li class="nav-item"><a class="nav-link" href="#Subjects" data-toggle="tab" role="tab" aria-expanded="false">Subjects</a></li>
        <li class="nav-item"><a class="nav-link" href="#Accounts" data-toggle="tab" role="tab" aria-expanded="false">Accounts</a></li>
        <li class="nav-item"><a class="nav-link" href="#SMSInbox" data-toggle="tab" role="tab" aria-expanded="false">SMS Inbox</a></li>
        <li class="nav-item"><a class="nav-link" href="#Report" data-toggle="tab" role="tab" aria-expanded="false">Student Report</a></li>
    </ul>

    <div class="tab-content card">
        <div id="ResultAnalysis" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
            <div class="row no-gutters">
                <div class="col-lg-3">
                    <div class="card-header mb-2 bg-white">
                        <i class="fa fa-bar-chart mr-1" aria-hidden="true"></i>SUBJECT AVG.
                    </div>
                    <asp:FormView ID="BestSubFormView" Width="100%" DataSourceID="BestSubSQL" runat="server">
                        <ItemTemplate>
                            <div class="statistic2 d-flex align-items-center bg-white has-shadow mr-0">
                                <div class="icon bg-success"><i class="fa fa-thumbs-up"></i></div>
                                <div class="text">
                                    <strong><%# Eval("SubjectName") %></strong><br>
                                    <small><%# Eval("Top_Sub") %>%</small>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:FormView>
                    <asp:SqlDataSource ID="BestSubSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT TOP (1) ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Top_Sub, Subject.SubjectName
FROM Exam_Result_of_Subject INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID WHERE (Exam_Result_of_Subject.StudentID = @StudentID) AND (Exam_Result_of_Student.StudentPublishStatus = N'Pub')
GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Subject.SN ORDER BY Top_Sub DESC">
                        <SelectParameters>
                            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                    <asp:FormView ID="WeakFormView" Width="100%" DataSourceID="WeakSQL" runat="server">
                        <ItemTemplate>
                            <div class="statistic2 d-flex align-items-center bg-white has-shadow mr-0">
                                <div class="icon bg-danger"><i class="fa fa-thumbs-down"></i></div>
                                <div class="text">
                                    <strong><%# Eval("SubjectName") %></strong><br>
                                    <small><%# Eval("Worst_Sub") %>%</small>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:FormView>
                    <asp:SqlDataSource ID="WeakSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT TOP (1) ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Worst_Sub, Subject.SubjectName FROM Exam_Result_of_Subject INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID
WHERE (Exam_Result_of_Subject.StudentID = @StudentID) AND (Exam_Result_of_Student.StudentPublishStatus = N'Pub')
GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Subject.SN ORDER BY Worst_Sub ASC">
                        <SelectParameters>
                            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                    <div class="card mb-4">
                        <div class="card-body">
                            <div class="table-responsive">
                                <asp:Repeater ID="SubjectAvgRepeater" runat="server" DataSourceID="SubjectAvgSQL">
                                    <HeaderTemplate>
                                        <table class="table table-sm">
                                            <thead>
                                                <tr>
                                                    <th>Subject</th>
                                                    <th>Avg. Marks</th>
                                                    <th>Position</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("SubjectName") %></td>
                                            <td><%# Eval("Sub_Avg") %>%</td>
                                            <td><%# Eval("Sub_Position") %></td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </tbody>
                                    </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                                <asp:SqlDataSource ID="SubjectAvgSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Exam_Result_of_Subject.SubjectID, Subject.SN, Subject.SubjectName, ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Sub_Avg, 
AVG(CAST(Exam_Result_of_Subject.Position_InSubject_Class AS int)) AS Sub_Position FROM Exam_Result_of_Subject INNER JOIN
Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID
WHERE (Exam_Result_of_Subject.StudentID = @StudentID) AND (Exam_Result_of_Student.StudentPublishStatus = N'Pub')
GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Subject.SN ORDER BY Sub_Avg DESC">
                                    <SelectParameters>
                                        <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-lg-9 pl-2">
                    <asp:FormView ID="StdentAvgFormView" runat="server" Width="100%" DataSourceID="StudentAvgSQL">
                        <ItemTemplate>
                            <div class="row no-gutters">
                                <div class="col-lg-3">
                                    <div class="statistic d-flex align-items-center bg-white has-shadow">
                                        <div class="icon bg-danger"><i class="fa fa-list-ol"></i></div>
                                        <div class="text">
                                            <strong><%# Eval("Average_Position_Class") %></strong><br>
                                            <small>Avg. Position</small>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-3">
                                    <div class="statistic d-flex align-items-center bg-white has-shadow">
                                        <div class="icon bg-warning darken-3"><i class="fa fa-star"></i></div>
                                        <div class="text">
                                            <strong><%# Eval("Average_Point") %></strong><br>
                                            <small>Avg. Point</small>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-3">
                                    <div class="statistic d-flex align-items-center bg-white has-shadow">
                                        <div class="icon bg-primary"><i class="fa fa-pie-chart"></i></div>
                                        <div class="text">
                                            <strong><%# Eval("Average_ObtainedMarkofSubject") %>%</strong><br>
                                            <small>Avg. Mark</small>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-3">
                                    <div class="statistic d-flex align-items-center bg-white has-shadow" style="margin-right: 0;">
                                        <div class="icon bg-success"><i class="fa fa-line-chart"></i></div>
                                        <div class="text">
                                            <strong><%# Eval("Success_Percentage") %>%</strong><br>
                                            <small>Pass %</small>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:FormView>
                    <asp:SqlDataSource ID="StudentAvgSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AVG(CAST(Position_InExam_Class as int)) AS Average_Position_Class,ROUND(AVG(Student_Point),2,0) AS  Average_Point,(SELECT ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject),2,0) FROM  Exam_Result_of_Subject INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID WHERE (Exam_Result_of_Student.StudentPublishStatus = N'Pub') AND (Exam_Result_of_Student.StudentID = @StudentID)) as Average_ObtainedMarkofSubject,
(SELECT ROUND(100 * SUM(CASE WHEN t.PassStatus_Student = 'P' THEN 1 ELSE 0 END)/COUNT(t.StudentID),2,0) FROM (SELECT StudentID, PassStatus_Student FROM  Exam_Result_of_Student WHERE (StudentPublishStatus = N'Pub') AND (StudentID = @StudentID)UNION ALL SELECT StudentID, PassStatus_Student FROM  Exam_Cumulative_Student WHERE (StudentID = @StudentID)) as T) AS Success_Percentage
FROM Exam_Result_of_Student WHERE (StudentPublishStatus = N'Pub') AND (StudentID = @StudentID)">
                        <SelectParameters>
                            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                    <div class="card mb-4">
                        <div class="card-header">
                            <i class="fa fa-bar-chart mr-1" aria-hidden="true"></i>SESSION BASED EXAM REPORT
                        </div>
                        <div class="card-body">
                            <div class="table-responsive">
                                <asp:Repeater ID="SessionSuccessRepeater" runat="server" DataSourceID="SessionSuccessSQL">
                                    <HeaderTemplate>
                                        <table class="table">
                                            <thead>
                                                <tr>
                                                    <th>Session</th>
                                                    <th>Avg. Position</th>
                                                    <th>Avg. Point</th>
                                                    <th>Pass %</th>
                                                    <th>Avg. Marks</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("EducationYear") %></td>
                                            <td><%# Eval("Average_Position_Class") %></td>
                                            <td><%# Eval("Average_Point") %></td>
                                            <td><%# Eval("Success_Percentage") %>%</td>
                                            <td><%# Eval("Average_ObtainedMarkofSubject") %>%</td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </tbody>
                             </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                                <asp:SqlDataSource ID="SessionSuccessSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT T_AP.EducationYearID, Education_Year.EducationYear, T_AP.Average_Position_Class, T_AP.Average_Point, T_S.Success_Percentage, T_B.Average_ObtainedMarkofSubject
FROM (SELECT EducationYearID, AVG(CAST(Position_InExam_Class AS int)) AS Average_Position_Class, ROUND(AVG(Student_Point), 2, 0) AS Average_Point FROM Exam_Result_of_Student WHERE (StudentPublishStatus = N'Pub') AND (StudentID = @StudentID)
GROUP BY EducationYearID) AS T_AP INNER JOIN(SELECT EducationYearID, ROUND(100 * SUM(CASE WHEN t .PassStatus_Student = 'P' THEN 1 ELSE 0 END) / COUNT(StudentID), 2, 0) AS Success_Percentage
FROM (SELECT EducationYearID, StudentID, PassStatus_Student FROM Exam_Result_of_Student AS Exam_Result_of_Student_1 WHERE (StudentPublishStatus = N'Pub') AND (StudentID = @StudentID)
UNION ALL SELECT EducationYearID, StudentID, PassStatus_Student FROM Exam_Cumulative_Student WHERE (StudentID = @StudentID)) AS T
GROUP BY EducationYearID) AS T_S ON T_AP.EducationYearID = T_S.EducationYearID INNER JOIN
(SELECT Exam_Result_of_Student_2.EducationYearID, ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Average_ObtainedMarkofSubject
FROM Exam_Result_of_Subject INNER JOIN Exam_Result_of_Student AS Exam_Result_of_Student_2 ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student_2.StudentResultID WHERE (Exam_Result_of_Student_2.StudentPublishStatus = N'Pub') AND (Exam_Result_of_Student_2.StudentID = @StudentID)
GROUP BY Exam_Result_of_Student_2.EducationYearID) AS T_B ON T_AP.EducationYearID = T_B.EducationYearID INNER JOIN
Education_Year ON T_AP.EducationYearID = Education_Year.EducationYearID ORDER BY Education_Year.StartDate">
                                    <SelectParameters>
                                        <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                        </div>
                    </div>

                    <ul class="nav nav-tabs nav-justified">
                        <li class="nav-item">
                            <a class="nav-link active" data-toggle="tab" href="#panel1" role="tab">INDIVIDUAL RESULT</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" data-toggle="tab" href="#panel2" role="tab">CUMULATIVE RESULT</a>
                        </li>
                    </ul>

                    <div class="tab-content card">
                        <div class="mb-3 px-5">
                            <asp:DropDownList ID="EduYearDropDownList" runat="server" CssClass="form-control" DataSourceID="EduYearSQL" DataTextField="EducationYear" DataValueField="EducationYearID"></asp:DropDownList>
                            <asp:SqlDataSource ID="EduYearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EducationYearID, ('Session: ' + EducationYear) AS EducationYear FROM Education_Year WHERE (SchoolID = @SchoolID)">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                        <div class="tab-pane fade in show active" id="panel1" role="tabpanel">
                            <canvas id="IndividualExam"></canvas>
                        </div>

                        <div class="tab-pane fade" id="panel2" role="tabpanel">
                            <canvas id="CumilativeExam"></canvas>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div id="Attendance" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <ul class="nav nav-tabs z-depth-1">
                <li><a class="nav-link active" href="#Att_Summary" data-toggle="tab" role="tab" aria-expanded="true">Summary</a></li>
                <li><a class="nav-link" href="#Att_Monthly" data-toggle="tab" role="tab" aria-expanded="false">Details</a></li>
            </ul>
            <div class="tab-content card">
                <div id="Att_Summary" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
                    <div class="form-inline NoPrint">
                        <div class="form-group">
                            <asp:TextBox ID="FromDateTextBox" runat="server" CssClass="form-control Datetime" placeholder="From Date"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control Datetime" placeholder="To Date"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Submit" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:FormView ID="Att_Summery_FormView" runat="server" DataSourceID="SummerySQL" Width="100%">
                                <ItemTemplate>
                                    <div class="row">
                                        <div class="col-md-6">
                                            <div class="box bg-w">
                                                <h5>Working Days</h5>
                                                <h4 id="WD"><%# Eval("WorkingDay") %></h4>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="box bg-p">
                                                <h5>Present (<span id="P_percen"></span>)</h5>
                                                <h4 id="Pre"><%# Eval("Pre") %></h4>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="box bg-a">
                                                <h5>Absence (<span id="A_percen"></span>)</h5>
                                                <h4 id="Abs"><%# Eval("Abs") %></h4>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="box bg-l">
                                                <h5>Late (<span id="l_percen"></span>)</h5>
                                                <h4 id="Late"><%# Eval("Late") %></h4>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="box bg-la">
                                                <h5>Late Absence (<span id="la_percen"></span>)</h5>
                                                <h4 id="LateAbs"><%# Eval("LateAbs") %></h4>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="box bg-lv">
                                                <h5>Leave (<span id="lv_percen"></span>)</h5>
                                                <h4 id="Leave"><%# Eval("Leave") %></h4>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:FormView>
                            <asp:SqlDataSource ID="SummerySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT  dbo.F_Stu_WorkingDay(@SchoolID, @EducationYearID, (SELECT ClassID FROM  StudentsClass WHERE  (StudentClassID = @StudentClassID)), @From_Date, @To_Date)as WorkingDay,
			 dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Pre', @From_Date, @To_Date) AS Pre,
			 dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Abs', @From_Date, @To_Date) AS Abs,
			 dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Late', @From_Date, @To_Date) AS Late,
			 dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Leave', @From_Date, @To_Date) AS Leave,
			 dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Bunk', @From_Date, @To_Date)  AS Bunk,
			 dbo.F_Stu_Attendance_Summary(@SchoolID, @EducationYearID, @StudentClassID, 'Late Abs', @From_Date, @To_Date) AS LateAbs "
                                CancelSelectOnNullParameter="False">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:QueryStringParameter DefaultValue="" Name="StudentClassID" QueryStringField="Student_Class" />
                                    <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
                                    <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                        <div class="col-md-6">
                            <canvas id="myChart"></canvas>
                        </div>
                    </div>
                </div>
                <div id="Att_Monthly" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                        <ContentTemplate>
                            <div class="Pre Re_Desin">Present</div>
                            <div class="Abs Re_Desin">Absent</div>
                            <div class="Late Re_Desin">Late</div>
                            <div class="Late_Abs Re_Desin">Late Abs</div>
                            <div class="Att_Holidays Re_Desin">Holidays</div>

                            <div class="calendarWrapper">
                                <asp:Calendar ID="AttendanceCalendar" SelectionMode="None" OnDayRender="AttendanceCalendar_DayRender" runat="server" Font-Names="Tahoma" Font-Size="20px" NextMonthText="." PrevMonthText="." SelectMonthText="»" SelectWeekText="›" CellPadding="0" CssClass="myCalendar" Width="100%" FirstDayOfWeek="Saturday">
                                    <OtherMonthDayStyle ForeColor="#b0b0b0" />
                                    <DayStyle CssClass="myCalendarDay" />
                                    <DayHeaderStyle CssClass="myCalendarDayHeader" />
                                    <TodayDayStyle CssClass="myCalendarToday" />
                                    <SelectorStyle CssClass="myCalendarSelector" />
                                    <NextPrevStyle CssClass="myCalendarNextPrev" />
                                    <TitleStyle CssClass="myCalendarTitle" />
                                </asp:Calendar>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>

        <div id="Individual_Exam" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <!--Individual_Exam tab-->
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <div class="row NoPrint">
                        <div class="col-sm-4 col-md-2 form-group">
                            <label>
                                Individual Result
					   <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorSummer" ErrorMessage="Select Exam" InitialValue="0" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                            </label>

                            <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" DataSourceID="ExamNameSQl" DataTextField="ExamName" DataValueField="ExamID" AppendDataBoundItems="True" AutoPostBack="True" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
                                <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="ExamNameSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID WHERE (Exam_Name.SchoolID = @SchoolID) AND (Exam_Name.EducationYearID = @EducationYearID) AND (Exam_Result_of_Student.StudentClassID = @StudentClassID)">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>

                    <div class="table-responsive">
                        <rsweb:ReportViewer ID="ResultReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="" Height="100%" AsyncRendering="False" SizeToReportContent="True" SplitterBackColor="White">
                            <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Individual_Result.rdlc" ReportPath="Report_Individual_Result.rdlc">
                                <DataSources>
                                    <rsweb:ReportDataSource DataSourceId="ExamResultODS" Name="DataSet1" />
                                </DataSources>
                            </LocalReport>
                        </rsweb:ReportViewer>

                        <asp:ObjectDataSource ID="ExamResultODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.Profile_newTableAdapter">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" Type="Int32" />
                                <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                            </SelectParameters>
                        </asp:ObjectDataSource>
                        <asp:ObjectDataSource ID="GradingSystemODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.Exam_Grading_SystemTableAdapter">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                <asp:ControlParameter ControlID="StudentInfoFormView" Name="ClassID" PropertyName="DataKey['ClassID']" Type="Int32" />
                                <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                            </SelectParameters>
                        </asp:ObjectDataSource>
                        <asp:ObjectDataSource ID="SchoolInfoODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.SchoolInfoTableAdapter">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:ObjectDataSource>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <div id="CumulativeResult" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="form-inline">
                        <asp:DropDownList ID="Cum_ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="CumiExamSQL" DataTextField="CumulativeResultName" DataValueField="CumulativeNameID" AppendDataBoundItems="True" OnSelectedIndexChanged="Cum_ExamDropDownList_SelectedIndexChanged">
                            <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="CumiExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT CumulativeNameID, SchoolID, RegistrationID, EducationYearID, CumulativeResultName, Date FROM Exam_Cumulative_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                    <br />
                    <div class="table-responsive">
                        <rsweb:ReportViewer ID="Cu_ResultReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" ShowRefreshButton="False" WaitMessageFont-Size="14pt" AsyncRendering="False" SizeToReportContent="True" SplitterBackColor="White" DocumentMapWidth="" ZoomMode="PageWidth" Width="100%" Height="100%">
                            <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Individual_Result.rdlc" ReportPath="Exam\CumulativeResult\Cumulative_Sub_Exam.rdlc">
                                <DataSources>
                                    <rsweb:ReportDataSource DataSourceId="Cum_ExamResultODS" Name="DataSet1" />
                                </DataSources>
                            </LocalReport>
                        </rsweb:ReportViewer>
                        <asp:ObjectDataSource ID="Cum_ExamResultODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam.CumulativeResult.Cu_ExamTableAdapters.Cumi_Student_ProfileTableAdapter">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="Cum_ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" Type="Int32" />
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                                <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" Type="Int32" />
                            </SelectParameters>
                        </asp:ObjectDataSource>
                        <asp:ObjectDataSource ID="Cu_GradingSystemODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.Cumuletive_Grading_SystemTableAdapter">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                <asp:ControlParameter ControlID="StudentInfoFormView" Name="ClassID" PropertyName="DataKey['ClassID']" Type="Int32" />
                                <asp:ControlParameter ControlID="Cum_ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" Type="Int32" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                            </SelectParameters>
                        </asp:ObjectDataSource>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <div id="Subjects" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <!--Subjects-->
            <asp:BulletedList ID="BulletedList1" runat="server" DataSourceID="StudentSubjectSQL" DataTextField="Column1" DataValueField="Column1" Font-Bold="True" Font-Size="Medium" BulletStyle="Numbered">
            </asp:BulletedList>
            <asp:SqlDataSource ID="StudentSubjectSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Subject.SubjectName + IIF(SubjectType='Compulsory','',' *') FROM StudentRecord INNER JOIN Subject ON StudentRecord.SubjectID = Subject.SubjectID WHERE (StudentRecord.EducationYearID = @EducationYearID) AND (StudentRecord.SchoolID = @SchoolID) AND (StudentRecord.StudentClassID = @StudentClassID) ORDER BY Subject.SN">
                <SelectParameters>
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />
                </SelectParameters>
            </asp:SqlDataSource>
            <a id="Sub_Change">Change Subject</a>
        </div>

        <div id="Accounts" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <!--Accounts-->
            <ul class="nav nav-tabs z-depth-1">
                <li><a class="nav-link active" href="#DueAmount" data-toggle="tab" role="tab" aria-expanded="true">Total Due</a></li>
                <li><a class="nav-link" href="#PresentDue" data-toggle="tab" role="tab" aria-expanded="false">Current Due</a></li>
                <li><a class="nav-link" href="#PaidAmount" data-toggle="tab" role="tab" aria-expanded="false">Total Paid</a></li>
                <li><a class="nav-link" href="#PaidRecord" data-toggle="tab" role="tab" aria-expanded="false">Paid Record</a></li>
                <li><a class="nav-link" href="#Concession" data-toggle="tab" role="tab" aria-expanded="false">Concession</a></li>
                <li><a class="nav-link" href="#Payorder" data-toggle="tab" role="tab" aria-expanded="false">All Pay order</a></li>
            </ul>

            <div class="tab-content card">
                <a class="d-print-none" href="../../Accounts/Payment/Payment_Collection.aspx" target="_blank">Payment Collection</a>
                <div id="DueAmount" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
                    <div class="alert alert-success Accounts-p-title">
                        <label id="Total_Due"></label>
                    </div>
                    <div class="table-responsive">
                        <asp:GridView ID="DueGridView" runat="server" AutoGenerateColumns="False" DataSourceID="DueSQL" CssClass="mGrid">
                            <Columns>
                                <asp:BoundField DataField="EducationYear" HeaderText="Session" SortExpression="EducationYear" />
                                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                                <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                                <asp:BoundField DataField="StartDate" HeaderText="Start Date" SortExpression="StartDate" DataFormatString="{0:dd-MMM-yy}" />
                                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:dd-MMM-yy}"></asp:BoundField>
                                <asp:TemplateField HeaderText="Fee" SortExpression="Amount">
                                    <ItemTemplate>
                                        <asp:Label ID="TotalFeesLabel" runat="server" Text='<%# Bind("Amount") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Discount" HeaderText="Concession" SortExpression="Discount" />
                                <asp:TemplateField HeaderText="Paid" SortExpression="PaidAmount">
                                    <ItemTemplate>
                                        <asp:Label ID="Label3" runat="server" Text='<%# Bind("PaidAmount") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                                    <ItemTemplate>
                                        <asp:Label ID="TotalDueLabel" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                                <asp:BoundField DataField="LateFee_Discount" HeaderText="LF.Conc" SortExpression="LateFee_Discount" />
                                <asp:BoundField DataField="LastPaidDate" HeaderText="Last Paid Date" SortExpression="LastPaidDate" DataFormatString="{0:dd-MMM-yyyy}" />
                            </Columns>
                            <FooterStyle CssClass="GridFooter" />
                            <PagerStyle CssClass="pgr" />
                            <SelectedRowStyle CssClass="Selected" />
                            <EmptyDataTemplate>
                                No record(s) found !
                            </EmptyDataTemplate>
                        </asp:GridView>
                        <asp:SqlDataSource ID="DueSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT        Income_PayOrder.PayOrderID, Income_PayOrder.StudentID, Income_PayOrder.EducationYearID, Income_PayOrder.StudentClassID, Income_PayOrder.ClassID, CreateClass.Class, 
						 Education_Year.EducationYear, Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, 
						 Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount, 0) + ISNULL(Income_PayOrder.LateFee, 0) 
						 - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) - ISNULL(Income_PayOrder.LateFee_Discount, 0) ELSE ISNULL(Income_PayOrder.Amount, 0) 
						 - ISNULL(Income_PayOrder.Discount, 0) - ISNULL(Income_PayOrder.PaidAmount, 0) END AS Due, Income_PayOrder.RoleID, Income_PayOrder.StartDate,Income_PayOrder.LastPaidDate, Income_PayOrder.NumberOfPayment
FROM            Income_PayOrder INNER JOIN
						 Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN
						 Student ON Income_PayOrder.StudentID = Student.StudentID INNER JOIN
						 Education_Year ON Income_PayOrder.EducationYearID = Education_Year.EducationYearID INNER JOIN
						 CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID 
WHERE        (Income_PayOrder.Status = 'Due') AND (Income_PayOrder.StudentID = @StudentID) AND (Student.Status = N'Active') AND (Income_PayOrder.SchoolID = @SchoolID) 
ORDER BY Income_PayOrder.EndDate">
                            <SelectParameters>
                                <asp:QueryStringParameter DefaultValue="" Name="StudentID" QueryStringField="Student" />
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>

                <div id="PresentDue" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                    <div class="alert alert-success Accounts-p-title">
                        <label id="Current_Due"></label>
                    </div>
                    <div class="table-responsive">
                        <asp:GridView ID="PresentDueGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PresentDueeSQL" CssClass="mGrid">
                            <Columns>
                                <asp:BoundField DataField="EducationYear" HeaderText="Session" SortExpression="EducationYear" />
                                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                                <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                                <asp:BoundField DataField="PaidAmount" HeaderText="Paid " SortExpression="PaidAmount" />
                                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                                    <ItemTemplate>
                                        <asp:Label ID="PresentDueLabel" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:dd MMM yyyy}" />
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="PresentDueeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT  CreateClass.Class,Education_Year.EducationYear,Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Amount , 0) + ISNULL(LateFee , 0) - ISNULL(Discount , 0) - ISNULL(PaidAmount , 0) - ISNULL(LateFee_Discount , 0) ELSE ISNULL(Amount , 0) - ISNULL(Discount , 0) - ISNULL(PaidAmount , 0) END AS Due, Income_PayOrder.EndDate, Income_PayOrder.StudentClassID FROM Income_PayOrder INNER JOIN Education_Year ON Income_PayOrder.EducationYearID = Education_Year.EducationYearID INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID INNER JOIN CreateClass ON Income_PayOrder.ClassID = CreateClass.ClassID  WHERE (Income_PayOrder.Status = 'Due') AND (Income_PayOrder.EndDate &lt; GETDATE()) AND (Income_PayOrder.StudentID = @StudentID) ORDER BY Income_PayOrder.EndDate">
                            <SelectParameters>
                                <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>

                <div id="PaidAmount" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                    <div class="alert alert-success Accounts-p-title">
                        <label class="Total_Paid"></label>
                    </div>
                    <div class="table-responsive">
                        <asp:GridView ID="PaidGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PaidSQL"
                            AlternatingRowStyle-CssClass="alt" CssClass="mGrid" PagerStyle-CssClass="pgr">
                            <AlternatingRowStyle CssClass="alt" />
                            <RowStyle CssClass="RowStyle" />
                            <Columns>
                                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                                <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                                <asp:BoundField DataField="StartDate" HeaderText="Start Date" SortExpression="StartDate" DataFormatString="{0:dd-MMM-yy}" />
                                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:dd-MMM-yy}" />
                                <asp:TemplateField HeaderText="Fee" SortExpression="Amount">
                                    <ItemTemplate>
                                        <asp:Label ID="Label1" runat="server" Text='<%# Bind("Amount") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="Discount" HeaderText="Concession" SortExpression="Discount"></asp:BoundField>
                                <asp:TemplateField HeaderText="Paid" SortExpression="PaidAmount">
                                    <ItemTemplate>
                                        <asp:Label ID="Total_Paid_Label" runat="server" Text='<%# Bind("PaidAmount") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                                    <ItemTemplate>
                                        <asp:Label ID="Label3" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                                <asp:BoundField DataField="LateFee_Discount" HeaderText="LF.Conc" SortExpression="LateFee_Discount" />
                                <asp:BoundField DataField="LastPaidDate" HeaderText="Last Paid Date" SortExpression="LastPaidDate" DataFormatString="{0:dd-MMM-yyyy}" />
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                            <SelectedRowStyle CssClass="Selected" />
                            <EmptyDataTemplate>
                                No record(s) found !
                            </EmptyDataTemplate>
                            <FooterStyle CssClass="GridFooter" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="PaidSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount , 0) + ISNULL(Income_PayOrder.LateFee , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) - ISNULL(Income_PayOrder.LateFee_Discount , 0) ELSE ISNULL(Income_PayOrder.Amount , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) END AS Due, Income_PayOrder.LastPaidDate, Income_PayOrder.NumberOfPayment, Income_PayOrder.PayOrderID, Income_PayOrder.StudentClassID FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID WHERE (Income_PayOrder.StudentID = @StudentID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Income_PayOrder.StudentClassID = @StudentClassID) AND (Income_PayOrder.PaidAmount &lt;&gt; 0) ORDER BY Income_PayOrder.LastPaidDate DESC">
                            <SelectParameters>
                                <asp:QueryStringParameter DefaultValue="" Name="StudentID" QueryStringField="Student" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:QueryStringParameter DefaultValue="" Name="StudentClassID" QueryStringField="Student_Class" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>

                <div id="PaidRecord" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                    <div class="alert alert-success Accounts-p-title">
                        <label class="Total_Paid"></label>
                    </div>
                    <asp:UpdatePanel ID="UpdatePanel6" runat="server">
                        <ContentTemplate>
                            <div class="table-responsive">
                                <asp:GridView ID="PaidRecordGridView" runat="server" AutoGenerateColumns="False" DataSourceID="MoneyReceiptSQL" AlternatingRowStyle-CssClass="alt" CssClass="mGrid" PagerStyle-CssClass="pgr">
                                    <AlternatingRowStyle CssClass="alt" />
                                    <RowStyle CssClass="RowStyle" Font-Size="9pt" HorizontalAlign="Center" />
                                    <Columns>
                                        <asp:BoundField DataField="MoneyReceipt_SN" HeaderText="Receipt No" />
                                        <asp:BoundField DataField="PaidDate" HeaderText="Paid Date" SortExpression="PaidDate" DataFormatString="{0:dd-MMM-yy}" />
                                        <asp:BoundField DataField="TotalAmount" HeaderText="Paid" SortExpression="TotalAmount" />
                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <asp:LinkButton ID="MreceiptLinkButton" runat="server" CommandArgument='<%#Eval("MoneyReceiptID") %>' OnCommand="MreceiptLinkButton_Command" Text="Details" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                    <PagerStyle CssClass="pgr" />
                                    <SelectedRowStyle CssClass="Selected" />
                                    <EmptyDataTemplate>
                                        No record(s) found !
                                    </EmptyDataTemplate>
                                    <FooterStyle CssClass="GridFooter" />
                                </asp:GridView>
                                <asp:SqlDataSource ID="MoneyReceiptSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT PaidDate, TotalAmount, PaymentBy, MoneyReceipt_SN,MoneyReceiptID FROM Income_MoneyReceipt WHERE (StudentID = @StudentID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID)">
                                    <SelectParameters>
                                        <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" Type="Int32" />
                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                        <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>

                <div id="Concession" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                    <div class="alert alert-success Accounts-p-title">
                        <label id="Total_Concession"></label>
                    </div>
                    <div class="table-responsive">
                        <asp:GridView ID="LessPayGridView" runat="server" AutoGenerateColumns="False" DataSourceID="DiscountSQL"
                            AlternatingRowStyle-CssClass="alt" CssClass="mGrid" PagerStyle-CssClass="pgr">
                            <AlternatingRowStyle CssClass="alt" />
                            <RowStyle CssClass="RowStyle" />

                            <Columns>
                                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                                <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                                <asp:BoundField DataField="StartDate" HeaderText="Start Date" SortExpression="StartDate" DataFormatString="{0:dd-MMM-yy}" />
                                <asp:BoundField DataField="EndDate" HeaderText="End Date" SortExpression="EndDate" DataFormatString="{0:dd-MMM-yy}" />
                                <asp:BoundField DataField="Amount" HeaderText="Fee" SortExpression="Amount" />
                                <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                                <asp:TemplateField HeaderText="Concession" SortExpression="Total_Discount">
                                    <ItemTemplate>
                                        <asp:Label ID="Concession_Label" runat="server" Text='<%# Bind("Total_Discount") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                            <EmptyDataTemplate>
                                No record(s) found !
                            </EmptyDataTemplate>
                        </asp:GridView>
                        <asp:SqlDataSource ID="DiscountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.Amount, Income_PayOrder.LateFee, Income_PayOrder.Total_Discount, Income_PayOrder.StartDate, Income_PayOrder.EndDate FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID WHERE (Income_PayOrder.StudentID = @StudentID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Income_PayOrder.StudentClassID = @StudentClassID) AND (Income_PayOrder.Total_Discount &lt;&gt; 0) ORDER BY Income_PayOrder.StartDate ">
                            <SelectParameters>
                                <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" Type="Int32" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />
                            </SelectParameters>
                        </asp:SqlDataSource>

                        <%if (DisCountLateFeeGridView.Rows.Count > 0)
                            { %>
                        <br />
                        <div class="alert alert-info">Concession Late Fee Info: </div>
                        <asp:GridView ID="DisCountLateFeeGridView" runat="server" AutoGenerateColumns="False" DataSourceID="LateFeeSQL" AlternatingRowStyle-CssClass="alt" CssClass="mGrid" PagerStyle-CssClass="pgr" DataKeyNames="LateFeeDiscountID">
                            <AlternatingRowStyle CssClass="alt" />
                            <RowStyle CssClass="RowStyle" Font-Size="9pt" HorizontalAlign="Center" />
                            <Columns>
                                <asp:BoundField DataField="PreviousAmount" HeaderText="Prev. Concession" SortExpression="PreviousAmount" />
                                <asp:BoundField DataField="PostAmount" HeaderText="Post Concession" SortExpression="PostAmount" />
                                <asp:BoundField DataField="Reason" HeaderText="Reason" SortExpression="Reason" />
                                <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" DataFormatString="{0:dd-MMM-yy}" />
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                            <SelectedRowStyle CssClass="Selected" />
                            <EmptyDataTemplate>
                                No record(s) found !
                            </EmptyDataTemplate>
                            <FooterStyle CssClass="GridFooter" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="LateFeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT LateFeeDiscountID, SchoolID, EducationYearID, StudentID, StudentClassID, RegistrationID, PayOrderID, Reason, PreviousAmount, PostAmount, Date FROM Income_LateFee_Discount_Record WHERE (StudentID = @StudentID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID)">
                            <SelectParameters>
                                <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" Type="Int32" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <%} %>
                        <%if (ChargeGridView.Rows.Count > 0)
                            { %>
                        <div class="alert alert-info">Charge Late Fee Info: </div>
                        <asp:GridView ID="ChargeGridView" runat="server" AutoGenerateColumns="False" DataSourceID="ChargeLateFSQL" AlternatingRowStyle-CssClass="alt" CssClass="mGrid" PagerStyle-CssClass="pgr" DataKeyNames="LateFeeChangeID">
                            <AlternatingRowStyle CssClass="alt" />
                            <RowStyle CssClass="RowStyle" Font-Size="9pt" HorizontalAlign="Center" />
                            <Columns>
                                <asp:BoundField DataField="PreviousAmount" HeaderText="Prev. Concession" SortExpression="PreviousAmount" />
                                <asp:BoundField DataField="PostAmount" HeaderText="Post Concession" SortExpression="PostAmount" />
                                <asp:BoundField DataField="Date" DataFormatString="{0:dd-MMM-yy}" HeaderText="Date" SortExpression="Date" />
                            </Columns>
                            <HeaderStyle Font-Size="11px" />
                            <PagerStyle CssClass="pgr" />
                            <SelectedRowStyle CssClass="Selected" />
                            <EmptyDataTemplate>
                                No record(s) found !
                            </EmptyDataTemplate>
                            <FooterStyle CssClass="GridFooter" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="ChargeLateFSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT LateFeeChangeID, SchoolID, EducationYearID, StudentID, StudentClassID, RegistrationID, PayOrderID, PreviousAmount, PostAmount, Date FROM Income_LateFee_Change_Record WHERE (StudentID = @StudentID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID)">
                            <SelectParameters>
                                <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" Type="Int32" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <%} %>
                    </div>
                </div>

                <div id="Payorder" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                    <asp:FormView ID="P_SumFormView" runat="server" DataSourceID="PayOrderSummarySQL" Width="100%">
                        <ItemTemplate>
                            <div class="Accounts-p-title mb-2">
                                <span class="badge badge-primary">Total Fee: <%# Eval("TotalFee","{0:n0}") %></span>
                                <span class="badge badge-primary">Total Concession: <%# Eval("TotalDiscount","{0:n0}") %></span>
                                <span class="badge badge-primary">Total Late Fee: <%# Eval("TotalLateFee","{0:n0}") %></span>
                                <span class="badge badge-primary">Total Paid: <%# Eval("TotalPaid","{0:n0}") %></span>
                                <span class="badge badge-primary">Total Due: <%# Eval("Unpaid","{0:n0}") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:FormView>
                    <asp:SqlDataSource ID="PayOrderSummarySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SUM(Amount) AS TotalFee, 
	 SUM(LateFeeCountable) AS TotalLateFee, 
	 SUM(Total_Discount) AS TotalDiscount, 
	 SUM(ISNULL(PaidAmount, 0)) AS TotalPaid,
	 SUM(Receivable_Amount) AS Unpaid
FROM Income_PayOrder WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND StudentClassID = @StudentClassID ">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                    <div class="table-responsive">
                        <asp:GridView ID="PayOrderGridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" DataSourceID="PayOrderSQL"
                            AlternatingRowStyle-CssClass="alt" CssClass="mGrid" PagerStyle-CssClass="pgr" DataKeyNames="PayOrderID">
                            <AlternatingRowStyle CssClass="alt" />
                            <RowStyle CssClass="RowStyle" />
                            <PagerStyle CssClass="pgr" />
                            <SelectedRowStyle CssClass="Selected" />
                            <Columns>
                                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                                <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                                <asp:BoundField DataField="StartDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Start Date" SortExpression="StartDate" />
                                <asp:BoundField DataField="EndDate" DataFormatString="{0:d MMM yyyy}" HeaderText="End Date" SortExpression="EndDate" />
                                <asp:BoundField DataField="Amount" HeaderText="Fee" SortExpression="Amount" />
                                <asp:BoundField DataField="Discount" HeaderText="Concession" SortExpression="Discount" />
                                <asp:BoundField DataField="LateFee" HeaderText="Late Fee" SortExpression="LateFee" />
                                <asp:BoundField DataField="LateFee_Discount" HeaderText="L.F. Con" SortExpression="LateFee_Discount" />
                                <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                                <asp:BoundField DataField="Due" HeaderText="Due" ReadOnly="True" SortExpression="Due" />
                                <asp:BoundField DataField="LastPaidDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Last Paid Date" SortExpression="LastPaidDate" />
                            </Columns>
                            <EmptyDataTemplate>
                                No record(s) found !
                            </EmptyDataTemplate>
                            <FooterStyle CssClass="GridFooter" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="PayOrderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT Income_Roles.Role, Income_PayOrder.PayFor, Income_PayOrder.StartDate, Income_PayOrder.EndDate, Income_PayOrder.Amount, Income_PayOrder.Discount, Income_PayOrder.LateFee, Income_PayOrder.LateFee_Discount, Income_PayOrder.PaidAmount, CASE WHEN Income_PayOrder.EndDate &lt; GETDATE() - 1 THEN ISNULL(Income_PayOrder.Amount , 0) + ISNULL(Income_PayOrder.LateFee , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) - ISNULL(Income_PayOrder.LateFee_Discount , 0) ELSE ISNULL(Income_PayOrder.Amount , 0) - ISNULL(Income_PayOrder.Discount , 0) - ISNULL(Income_PayOrder.PaidAmount , 0) END AS Due, Income_PayOrder.LastPaidDate, Income_PayOrder.NumberOfPayment, Income_PayOrder.PayOrderID, Income_PayOrder.StudentClassID FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID WHERE   (Income_PayOrder.StudentID = @StudentID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND (Income_PayOrder.StudentClassID = @StudentClassID) ORDER BY Income_PayOrder.StartDate ">
                            <SelectParameters>
                                <asp:QueryStringParameter DefaultValue="" Name="StudentID" QueryStringField="Student" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:QueryStringParameter DefaultValue="" Name="StudentClassID" QueryStringField="Student_Class" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
            </div>
        </div>

        <div id="SMSInbox" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <asp:GridView ID="SMSGridView" AllowSorting="True" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="SMSRecordSQL">
                <Columns>
                    <asp:BoundField DataField="PhoneNumber" HeaderText="Phone Number" SortExpression="PhoneNumber" />
                    <asp:BoundField DataField="TextSMS" HeaderText="Text SMS" SortExpression="TextSMS" />
                    <asp:BoundField DataField="PurposeOfSMS" HeaderText="Purpose Of SMS" SortExpression="PurposeOfSMS" />
                    <asp:BoundField DataField="Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Date" SortExpression="Date">
                        <ItemStyle Width="100px" />
                    </asp:BoundField>
                </Columns>
                <EmptyDataTemplate>
                    No records
                </EmptyDataTemplate>
            </asp:GridView>
            <asp:SqlDataSource ID="SMSRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT SMS_Send_Record.TextSMS, SMS_Send_Record.PurposeOfSMS, SMS_Send_Record.Date, SMS_Send_Record.PhoneNumber FROM SMS_OtherInfo INNER JOIN SMS_Send_Record ON SMS_OtherInfo.SMS_Send_ID = SMS_Send_Record.SMS_Send_ID WHERE (SMS_OtherInfo.StudentID = @StudentID) AND (SMS_OtherInfo.SchoolID = @SchoolID) AND (SMS_OtherInfo.EducationYearID = @EducationYearID) ORDER BY SMS_Send_Record.Date DESC">
                <SelectParameters>
                    <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div id="Report" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                <ContentTemplate>
                    <button type="button" class="btn btn-grey d-print-none btn-sm" data-toggle="modal" data-target="#FaultModal">Add Report</button>
                                    <div class="form-inline head-area NoPrint">
        <div class="form-group">
            <asp:TextBox ID="From_Date_TextBox" CssClass="form-control Datetime" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:TextBox ID="To_Date_TextBox" CssClass="form-control Datetime" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
            <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar"></i>
        </div>
        <div class="form-group">
            <asp:Button ID="Find_Button" CssClass="btn btn-primary" runat="server" Text="Submit" />
        </div>
        <div class="form-group pull-right Print">
            <a title="Print This Page" onclick="window.print();"><i class="fa fa-print" aria-hidden="true"></i></a>
        </div>
    </div>
                    <asp:GridView ID="Fault_Gridview" CssClass="mGrid" DataKeyNames="StudentFaultID" runat="server" DataSourceID="FaultSQL" AutoGenerateColumns="False" Width="100%" AllowPaging="True" AllowSorting="True" PageSize="30">
                        <Columns>
                            <asp:TemplateField HeaderText="Title" SortExpression="Fault_Title">
                                <EditItemTemplate>
                                    <asp:TextBox ID="TextBox3" CssClass="form-control" runat="server" Text='<%# Bind("Fault_Title") %>'></asp:TextBox>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Label3" runat="server" Text='<%# Bind("Fault_Title") %>'></asp:Label>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Left" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Report" SortExpression="Fault">
                                <EditItemTemplate>
                                    <asp:TextBox ID="TextBox2" CssClass="form-control" runat="server" Text='<%# Bind("Fault") %>'></asp:TextBox>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Label2" runat="server" Text='<%# Bind("Fault") %>'></asp:Label>
                                </ItemTemplate>
                                <ItemStyle HorizontalAlign="Left" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Date" SortExpression="Fault_Date">
                                <EditItemTemplate>
                                    <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control Datetime" Text='<%# Bind("Fault_Date") %>'></asp:TextBox>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("Fault_Date", "{0:d MMM yyyy}") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="UserName" HeaderText="Added By" SortExpression="UserName" />
                            <asp:TemplateField HeaderText="Edit/Delete">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkEdit" runat="server" CssClass="blue-text d-print-none" CommandName="edit">
                                        <i class="fa fa-pencil-square-o" aria-hidden="true"></i>
                                    </asp:LinkButton>
                                    <span class="d-print-none">|</span>
                                    <asp:LinkButton ID="lnkDelete" OnClientClick="return confirm('Are you sure want to delete?')" CssClass="red-text d-print-none" runat="server" CommandName="delete">
                                        <i class="fa fa-trash" aria-hidden="true"></i>
                                    </asp:LinkButton>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:Button ID="lnkUpdate" CssClass="btn btn-success btn-sm" runat="server" CommandName="update" Text="Update" />
                                    <asp:Button ID="lnkCancel" CssClass="btn btn-default btn-sm" runat="server" CommandName="cancel" Text="Cancel" />
                                </EditItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                        <PagerStyle CssClass="pgr" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="FaultSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Student_Fault] WHERE [StudentFaultID] = @StudentFaultID" InsertCommand="INSERT INTO [Student_Fault] ([SchoolID], [RegistrationID], [EducationYearID], [StudentID], [StudentClassID], [Fault_Title], [Fault], [Fault_Date]) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @StudentClassID, @Fault_Title, @Fault, @Fault_Date)" SelectCommand=" SELECT Registration.UserName, StudentFaultID, Fault_Title, Fault,Fault_Date FROM Student_Fault Inner join Registration on Student_Fault.RegistrationId=Registration.RegistrationID 
WHERE (Student_Fault.SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID)AND Fault_Date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') ORDER BY Fault_Date DESC" UpdateCommand="UPDATE Student_Fault SET Fault_Title = @Fault_Title, Fault = @Fault, Fault_Date = @Fault_Date WHERE (StudentFaultID = @StudentFaultID)" CancelSelectOnNullParameter="False">
                        <DeleteParameters>
                            <asp:Parameter Name="StudentFaultID" Type="Int32" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" Type="Int32" />
                            <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" Type="Int32" />
                            <asp:ControlParameter ControlID="Fault_Title_TextBox" Name="Fault_Title" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="Fault_TextBox" Name="Fault" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="Fault_Date_TextBox" DbType="Date" Name="Fault_Date" PropertyName="Text" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />
                       <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                     <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="Fault_Title" Type="String" />
                            <asp:Parameter Name="Fault" Type="String" />
                            <asp:Parameter DbType="Date" Name="Fault_Date" />
                            <asp:Parameter Name="StudentFaultID" Type="Int32" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <button class="btn btn-primary d-print-none mt-3" onclick="window.print()" type="button">Print</button>

    <!-- Modal fault -->
    <div class="modal fade" id="FaultModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog cascading-modal" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="title">Add Student Report</h4>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body mb-0">
                    <asp:UpdatePanel ID="UpdatePanel5" runat="server">
                        <ContentTemplate>
                            <div class="form-group">
                                <label>Report Title<asp:RequiredFieldValidator ControlToValidate="Fault_Title_TextBox" ValidationGroup="Fa" ID="RequiredFieldValidator1" runat="server" CssClass="EroorStar" ErrorMessage="*" /></label>
                                <asp:TextBox ID="Fault_Title_TextBox" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Report<asp:RequiredFieldValidator ControlToValidate="Fault_TextBox" ValidationGroup="Fa" ID="RequiredFieldValidator3" runat="server" CssClass="EroorStar" ErrorMessage="*" /></label>
                                <asp:TextBox ID="Fault_TextBox" runat="server" CssClass="form-control" TextMode="MultiLine"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Report Date<asp:RequiredFieldValidator ControlToValidate="Fault_Date_TextBox" ValidationGroup="Fa" ID="RequiredFieldValidator4" runat="server" CssClass="EroorStar" ErrorMessage="*" /></label>
                                <asp:TextBox ID="Fault_Date_TextBox" runat="server" CssClass="form-control Datetime"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <asp:Button ValidationGroup="Fa" ID="Fault_Add_Button" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="Fault_Add_Button_Click" />
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>

    <!--Money receipt-->
    <asp:UpdatePanel ID="UpdatePanel9" runat="server">
        <ContentTemplate>
            <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
                <div class="modal-dialog cascading-modal" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h4 class="title">Money Receipt Details</h4>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        </div>
                        <div class="modal-body mb-0">
                            <asp:GridView ID="AllPaidRGridView" runat="server" AutoGenerateColumns="False" DataSourceID="AllPayRecordSQL"
                                CssClass="mGrid" PagerStyle-CssClass="pgr"
                                Width="100%" AllowPaging="True" PageSize="32">
                                <Columns>
                                    <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                                    <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                                    <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                                    <asp:BoundField DataField="PaidDate" HeaderText="Paid Date" SortExpression="PaidDate" DataFormatString="{0:dd-MMM-yyyy}" />
                                </Columns>
                                <PagerSettings Mode="NumericFirstLast" PageButtonCount="5" LastPageText="Last" FirstPageText="First" NextPageText="Next" PreviousPageText="Previous" />
                                <PagerStyle CssClass="pgr" />
                                <SelectedRowStyle CssClass="Selected" />
                                <FooterStyle CssClass="GridFooter" />
                            </asp:GridView>
                            <asp:SqlDataSource ID="AllPayRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                SelectCommand="SELECT Income_PaymentRecord.PayOrderID, Income_PaymentRecord.PaidAmount, Income_PaymentRecord.PayFor + ' (' + Education_Year.EducationYear + ')' AS PayFor, Income_PaymentRecord.PaidDate, Income_Roles.Role FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN Education_Year ON Income_PaymentRecord.EducationYearID = Education_Year.EducationYearID WHERE (Income_PaymentRecord.MoneyReceiptID = @MoneyReceiptID)">
                                <SelectParameters>
                                    <asp:Parameter Name="MoneyReceiptID" Type="Int32" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>

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
        $(function () {
            //chart
            var ctx = document.getElementById("myChart");
            var wd = $("#WD").text();
            var pre = $("#Pre").text();
            var abs = $("#Abs").text();
            var Late = $("#Late").text();
            var LateAbs = $("#LateAbs").text();
            var Leave = $("#Leave").text();

            $("#P_percen").text(Math.round((parseFloat(pre) * 100) / parseFloat(wd)).toFixed() + "%");
            $("#A_percen").text(Math.round((parseFloat(abs) * 100) / parseFloat(wd)).toFixed() + "%");
            $("#l_percen").text(Math.round((parseFloat(Late) * 100) / parseFloat(wd)).toFixed() + "%");
            $("#la_percen").text(Math.round((parseFloat(LateAbs) * 100) / parseFloat(wd)).toFixed() + "%");
            $("#lv_percen").text(Math.round((parseFloat(Leave) * 100) / parseFloat(wd)).toFixed() + "%");

            var myChart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: ["Working Days", "Present", "Absence", "Late", "Late Absence", "Leave"],
                    datasets: [{
                        label: 'Attendance',
                        data: [wd, pre, abs, Late, LateAbs, Leave],
                        backgroundColor: [
							'rgba(54, 162, 235, 0.2)',
							'rgba(6,215,156, 0.2)',
							'rgba(239,83,80, 0.2)',
							'rgba(255, 206, 86, 0.2)',
							'rgba(153, 102, 255, 0.2)',
							'rgba(255, 159, 64, 0.2)'
                        ],
                        borderColor: [
							'rgba(54, 162, 235, 1)',
							'rgba(6,215,156,1)',
							'rgba(239,83,80, 1)',
							'rgba(255, 206, 86, 1)',
							'rgba(153, 102, 255, 1)',
							'rgba(255, 159, 64, 1)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {

                }
            });

            //Find ID
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

            //Total Due
            var Total_Due = 0;
            $("[id*=TotalDueLabel]").each(function () {
                Total_Due = Total_Due + parseFloat($(this).html());
            });
            $("#Total_Due").html("Total Due: " + Total_Due.toString() + " Tk");

            //Current Due
            var Current_Due = 0;
            $("[id*=PresentDueLabel]").each(function () {
                Current_Due = Current_Due + parseFloat($(this).html());
            });
            $("#Current_Due").html("Total Current Due: " + Current_Due.toString() + " Tk");

            //Total_Paid
            var Total_Paid = 0;
            $("[id*=Total_Paid_Label]").each(function () {
                Total_Paid = Total_Paid + parseFloat($(this).html());
            });
            $(".Total_Paid").html("Total Paid: " + Total_Paid.toString() + " Tk");

            //Total_Concession
            var Total_Concession = 0;
            $("[id*=Concession_Label]").each(function () {
                Total_Concession = Total_Concession + parseFloat($(this).html());
            });
            $("#Total_Concession").html("Total Concession: " + Total_Concession.toString() + " Tk");

            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            //Exam analysis
            //Chart
            var sy = $("[id*=Session_DropDownList] option:selected").val();
            $('[id*=EduYearDropDownList] option[value="' + sy + '"]').attr('selected', true);
            ClassChart();

        });


        $("[id*=EduYearDropDownList]").on("change", function () {
            ClassChart();
        });

        var myChart;
        var myChart2;
        function ClassChart() {
            var Pdata = {};
            Pdata.EducationYearID =$("[id*=EduYearDropDownList] option:selected").val();
            Pdata.StudentID = $("#StudentID").val();

            //Individual
            $.ajax({
                type: "POST",
                url: "Report.aspx/Get_Exam_GradePoint",
                data: JSON.stringify(Pdata),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var Ch_data = r.d;
                    console.log(r.d);
                    var footerLine = Ch_data[2];

                    if (myChart) {
                        myChart.destroy();
                    }

                    var ctx = document.getElementById("IndividualExam").getContext('2d');
                    myChart = new Chart(ctx, {
                        type: 'bar',
                        data: {
                            labels: Ch_data[0],
                            datasets: [{
                                type: 'line',
                                borderColor: ['rgba(0, 200, 81, .8)'],
                                borderWidth: 5,
                                borderDash: [5, 5],
                                fill: false,
                                data: Ch_data[1]
                            },
                            {
                                data: Ch_data[1],
                                backgroundColor: [
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',
                                    'rgba(155,187,88,.3)',
                                    'rgba(35,191,170,.3)',
                                    'rgba(128,100,161,.3)',
                                    'rgba(74,172,197,.3)',
                                    'rgba(247,150,71,.3)',
                                    'rgba(127,96,132,.3)',
                                    'rgba(119,160,51,.3)',
                                    'rgba(51,85,139,.3)',
                                    'rgba(229,149,102,.3)',
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',

                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',
                                    'rgba(155,187,88,.3)',
                                    'rgba(35,191,170,.3)',
                                    'rgba(128,100,161,.3)',
                                    'rgba(74,172,197,.3)',
                                    'rgba(247,150,71,.3)',
                                    'rgba(127,96,132,.3)',
                                    'rgba(119,160,51,.3)',
                                    'rgba(51,85,139,.3)',
                                    'rgba(229,149,102,.3)',
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)'
                                ],
                                borderColor: [
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',
                                    'rgba(155,187,88,.5)',
                                    'rgba(35,191,170,.5)',
                                    'rgba(128,100,161,.5)',
                                    'rgba(74,172,197,.5)',
                                    'rgba(247,150,71,.5)',
                                    'rgba(127,96,132,.5)',
                                    'rgba(119,160,51,.5)',
                                    'rgba(51,85,139,.5)',
                                    'rgba(229,149,102,.5)',
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',

                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',
                                    'rgba(155,187,88,.5)',
                                    'rgba(35,191,170,.5)',
                                    'rgba(128,100,161,.5)',
                                    'rgba(74,172,197,.5)',
                                    'rgba(247,150,71,.5)',
                                    'rgba(127,96,132,.5)',
                                    'rgba(119,160,51,.5)',
                                    'rgba(51,85,139,.5)',
                                    'rgba(229,149,102,.5)',
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)'
                                ],
                                borderWidth: 1
                            }]
                        },
                        options: {
                            scales: {
                                yAxes: [{
                                    ticks: {
                                        beginAtZero: true,
                                        max: 5.5
                                    }
                                }],
                                xAxes: [{
                                    ticks: {
                                        autoSkip: false
                                    },
                                    maxBarThickness: 100,
                                }]
                            },
                            legend: {
                                display: false
                            },
                            tooltips: {
                                enabled: true,
                                mode: 'single',
                                callbacks: {
                                    beforeFooter: function (tooltipItems, data) {
                                        return 'Grade: ' + footerLine[tooltipItems[0].index];
                                    }
                                }
                            }
                        }
                    });
                },
                failure: function (r) {
                    alert(r.d);
                },
                error: function (r) {
                    alert(r.d);
                }
            });

            //Cumilative
            $.ajax({
                type: "POST",
                url: "Report.aspx/Get_CumilativeExam",
                data: JSON.stringify(Pdata),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var Ch_data = r.d;
                    console.log(r.d);
                    var footerLine = Ch_data[2];

                    if (myChart2) {
                        myChart2.destroy();
                    }

                    var ctx = document.getElementById("CumilativeExam").getContext('2d');
                    myChart2 = new Chart(ctx, {
                        type: 'bar',
                        data: {
                            labels: Ch_data[0],
                            datasets: [{
                                type: 'line',
                                borderColor: ['rgba(0, 200, 81, .8)'],
                                borderWidth: 5,
                                borderDash: [5, 5],
                                fill: false,
                                data: Ch_data[1]
                            },
                            {
                                data: Ch_data[1],
                                backgroundColor: [
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',
                                    'rgba(155,187,88,.3)',
                                    'rgba(35,191,170,.3)',
                                    'rgba(128,100,161,.3)',
                                    'rgba(74,172,197,.3)',
                                    'rgba(247,150,71,.3)',
                                    'rgba(127,96,132,.3)',
                                    'rgba(119,160,51,.3)',
                                    'rgba(51,85,139,.3)',
                                    'rgba(229,149,102,.3)',
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',

                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',
                                    'rgba(155,187,88,.3)',
                                    'rgba(35,191,170,.3)',
                                    'rgba(128,100,161,.3)',
                                    'rgba(74,172,197,.3)',
                                    'rgba(247,150,71,.3)',
                                    'rgba(127,96,132,.3)',
                                    'rgba(119,160,51,.3)',
                                    'rgba(51,85,139,.3)',
                                    'rgba(229,149,102,.3)',
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)'
                                ],
                                borderColor: [
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',
                                    'rgba(155,187,88,.5)',
                                    'rgba(35,191,170,.5)',
                                    'rgba(128,100,161,.5)',
                                    'rgba(74,172,197,.5)',
                                    'rgba(247,150,71,.5)',
                                    'rgba(127,96,132,.5)',
                                    'rgba(119,160,51,.5)',
                                    'rgba(51,85,139,.5)',
                                    'rgba(229,149,102,.5)',
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',

                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',
                                    'rgba(155,187,88,.5)',
                                    'rgba(35,191,170,.5)',
                                    'rgba(128,100,161,.5)',
                                    'rgba(74,172,197,.5)',
                                    'rgba(247,150,71,.5)',
                                    'rgba(127,96,132,.5)',
                                    'rgba(119,160,51,.5)',
                                    'rgba(51,85,139,.5)',
                                    'rgba(229,149,102,.5)',
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)'
                                ],
                                borderWidth: 1
                            }]
                        },
                        options: {
                            scales: {
                                yAxes: [{
                                    ticks: {
                                        beginAtZero: true,
                                        max: 5.5
                                    }
                                }],
                                xAxes: [{
                                    ticks: {
                                        autoSkip: false
                                    },
                                    maxBarThickness: 100,
                                }]
                            },
                            legend: {
                                display: false
                            },
                            tooltips: {
                                enabled: true,
                                mode: 'single',
                                callbacks: {
                                    beforeFooter: function (tooltipItems, data) {
                                        return 'Grade: ' + footerLine[tooltipItems[0].index];
                                    }
                                }
                            }
                        }
                    });
                },
                failure: function (r) {
                    alert(r.d);
                },
                error: function (r) {
                    alert(r.d);
                }
            });
        }


        // Define a plugin to provide data labels
        Chart.plugins.register({
            afterDatasetsDraw: function (chart) {
                var ctx = chart.ctx;

                chart.data.datasets.forEach(function (dataset, i) {
                    var meta = chart.getDatasetMeta(i);
                    if (!meta.hidden) {
                        meta.data.forEach(function (element, index) {
                            // Draw the text in black, with the specified font
                            ctx.fillStyle = 'rgb(0, 0, 0)';

                            var fontSize = 16;
                            var fontStyle = 'normal';
                            var fontFamily = 'Helvetica Neue';
                            ctx.font = Chart.helpers.fontString(fontSize, fontStyle, fontFamily);

                            // Just naively convert to string for now
                            var dataString = dataset.data[index].toString();

                            // Make sure alignment settings are correct
                            ctx.textAlign = 'center';
                            ctx.textBaseline = 'middle';

                            var padding = 3;
                            var position = element.tooltipPosition();
                            ctx.fillText(dataString, position.x, position.y - (fontSize / 2) - padding);
                        });
                    }
                });
            }
        });



        //Update Pannel
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

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

        function openModal() {
            $('#myModal').modal('show');
        }

        //Set Query string. --Subject--
        $("#Sub_Change").click(function () {
            var id = $("#IDLabel").text();
            window.open("../Change_Student_Subjects.aspx?id=" + id + "", '_blank');
        });
    </script>
</asp:Content>
