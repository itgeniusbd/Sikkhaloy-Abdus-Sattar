<%@ Page Title="Edit Student Info" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Find_Student.aspx.cs" Inherits="EDUCATION.COM.Admission.Edit_Student_Info.Find_Student" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        #InfoWraper { display: none; }
        .student-imgs { height: 100px; width: 100px; }

        .avatar-upload { position: relative; }
        .avatar-upload .avatar-edit { position: absolute; right: 0; z-index: 1; top: 0; }
        .avatar-upload .avatar-edit input { display: none; }
        .avatar-upload .avatar-edit input + label { display: inline-block; width: 34px; height: 34px; padding-top: 3px; margin-bottom: 0; border-radius: 100%; background: #FFFFFF; box-shadow: 0px 2px 4px 0px rgba(0, 0, 0, 0.12); cursor: pointer; font-weight: normal; transition: all 0.2s ease-in-out; text-align: center; border: 1px solid #E6E6E6; }
        .avatar-upload .avatar-edit input + label:hover { background: #f1f1f1; border-color: #d6d6d6; }
        .avatar-upload .avatar-edit label::after { content: "\f040"; font-family: 'FontAwesome'; color: #757575; }

        .media-body p { margin: 0; padding: 5px 10px; font-size: 14px; }
        .success_message { display:none;font-size: 80%;margin:0}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3><span class="NoPrint">Edit Student(s) Information</span>
        <asp:Label ID="CGSSLabel" runat="server"></asp:Label>
    </h3>

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
            <asp:Button ID="IDFindButton" runat="server" CssClass="btn btn-primary" OnClick="IDFindButton_Click" Text="Go" ValidationGroup="F" />
        </div>
    </div>

    <div id="InfoWraper">
        <div class="alert alert-secondary">
            <span id="CountStudent"></span>
        </div>

        <ul class="nav nav-tabs z-depth-1">
            <li class="nav-item">
                <a class="nav-link active" data-toggle="tab" href="#panel1" role="tab" aria-expanded="true">Upload Image</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" data-toggle="tab" href="#panel2" role="tab" aria-expanded="false">Change Roll No.</a>
            </li>
        </ul>

        <div class="tab-content card">
            <div class="tab-pane fade in active show" id="panel1" role="tabpanel" aria-expanded="true">
                <div class="row">
                    <asp:Repeater ID="ImageRepeater" runat="server" DataSourceID="ShowStudentClassSQL">
                        <ItemTemplate>
                            <div class="col-lg-6">
                                <div class="card mb-3 p-3">
                                    <div class="media">
                                        <div class="pr-3">
                                            <div class="avatar-upload">
                                                <div class="avatar-edit d-print-none">
                                                    <input name="Student_Photo" id="<%# Container.ItemIndex + 1 %>" type="file" accept="image/x-png,image/jpeg" />
                                                    <label for="<%# Container.ItemIndex + 1 %>"></label>
                                                </div>
                                                <img src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="student-imgs rounded-circle z-depth-1 img-thumbnail" />
                                                <input class="ImageID" value="<%# Eval("StudentImageID") %>" type="hidden" />
                                                <p class="text-center alert-success success_message">Upload Succss!</p>
                                            </div>
                                        </div>
                                        <div class="media-body pt-2">
                                            <p class="badge mdb-color lighten-1">ID: <%#Eval("ID") %></p>
                                            <p class="badge mdb-color lighten-1 mb-2">Roll No: <%#Eval("RollNo") %></p>
                                            <p class="badge mdb-color lighten-1 mb-2">Seat No: <%#Eval("SeatNo") %></p>

                                            <span class="d-block mb-2">
                                                <i class="fa fa-user"></i>
                                                <%#Eval("StudentsName") %>
                                            </span>

                                            <small class="d-block">
                                                <a target="_blank" href="Edit_Student_information.aspx?Student=<%#Eval("StudentID") %>&Student_Class=<%#Eval("StudentClassID") %>">
                                                    <i class="fa fa-pencil-square-o"></i>
                                                    Update Info
                                                </a>
                                            </small>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>

            <div class="tab-pane fade" id="panel2" role="tabpanel" aria-expanded="false">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <div class="table-responsive">
                            <asp:GridView ID="RollNoGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" PagerStyle-CssClass="pgr"
                                DataKeyNames="StudentClassID,StudentID" CssClass="mGrid" AllowSorting="True" DataSourceID="ShowStudentClassSQL">
                                <AlternatingRowStyle CssClass="alt" />
                                <RowStyle CssClass="RowStyle" />
                                <Columns>
                                    <asp:HyperLinkField DataNavigateUrlFields="StudentID,StudentClassID" DataNavigateUrlFormatString="Edit_Student_information.aspx?Student={0}&Student_Class={1}" DataTextField="StudentsName" HeaderText="Student's Name" />
                                    <asp:BoundField DataField="FathersName" HeaderText="Father's Name" SortExpression="FathersName" />
                                    <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
                                    <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                                    <asp:TemplateField HeaderText="Roll" SortExpression="RollNo">
                                        <ItemTemplate>
                                            <span class=" d-none d-print-block"><%# Eval("RollNo") %></span>
                                            <asp:TextBox ID="RollTextBox" CssClass="form-control d-print-none" runat="server" Text='<%# Bind("RollNo") %>'></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Seat No" SortExpression="SeatNo">
                                        <ItemTemplate>
                                            <span class=" d-none d-print-block"><%# Eval("SeatNo") %></span>
                                            <asp:TextBox ID="SeatTextBox" CssClass="form-control d-print-none" runat="server" Text='<%# Bind("SeatNo") %>'></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Photo">
                                        <ItemTemplate>
                                              <img src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" style="width:60px;height:60px"/>
                                        </ItemTemplate>
                                        <ItemStyle Width="60px" />
                                    </asp:TemplateField>
                                </Columns>
                                <PagerStyle CssClass="pgr" />
                                <SelectedRowStyle CssClass="Selected" />
                            </asp:GridView>
                        </div>

                        <div class="alert alert-success d-print-none mt-4">Set Roll No By Exam</div>
                        <div class="form-inline NoPrint">
                            <div class="form-group">
                                <asp:DropDownList ID="EduYearDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="EduYearSQL" DataTextField="EducationYear" DataValueField="EducationYearID" AppendDataBoundItems="True">
                                    <asp:ListItem Value="0">[ SELECT SESSION ]</asp:ListItem>
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ControlToValidate="EduYearDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="SH"></asp:RequiredFieldValidator>
                                <asp:SqlDataSource ID="EduYearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Education_Year] WHERE ([SchoolID] = @SchoolID)">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                            <div class="form-group">
                                <asp:DropDownList ID="Roll_Class_DropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="Roll_ClassSQL" DataTextField="Class" DataValueField="ClassID">
                                    <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="Roll_ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                    SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                            <div class="form-group">
                                <asp:RadioButtonList ID="CuOrExamRadioButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="True" CssClass="form-control">
                                    <asp:ListItem Selected="True">Cumulative Exam</asp:ListItem>
                                    <asp:ListItem>Individual Exam</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>
                            <div class="form-group">
                                <asp:RadioButtonList ID="Class_Sec_RadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control">
                                    <asp:ListItem Selected="True">Class Wise</asp:ListItem>
                                    <asp:ListItem>Section Wise</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <asp:MultiView ID="ExamMultiView" runat="server">
                                <asp:View ID="Cu_ExamView" runat="server">
                                    <div class="form-group">
                                        <asp:DropDownList ID="Cu_ExamDropDownList" runat="server" DataSourceID="Cu_ExamNameSQL" DataTextField="CumulativeResultName" DataValueField="CumulativeNameID" OnDataBound="Cu_ExamDropDownList_DataBound" CssClass="form-control">
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="Cu_ExamDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="SH"></asp:RequiredFieldValidator>
                                        <asp:SqlDataSource ID="Cu_ExamNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Exam_Cumulative_Name.CumulativeResultName, Exam_Cumulative_Name.CumulativeNameID
FROM            Exam_Cumulative_Setting INNER JOIN
                         Exam_Cumulative_Name ON Exam_Cumulative_Setting.CumulativeNameID = Exam_Cumulative_Name.CumulativeNameID
WHERE        (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Setting.ClassID = @ClassID)">
                                            <SelectParameters>
                                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                                <asp:ControlParameter ControlID="EduYearDropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                                                <asp:ControlParameter ControlID="Roll_Class_DropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                            </SelectParameters>
                                        </asp:SqlDataSource>
                                    </div>
                                </asp:View>
                                <asp:View ID="ExamView" runat="server">
                                    <div class="form-group">
                                        <asp:DropDownList ID="ExamDropDownList" runat="server" DataSourceID="ExamNameSQL" DataTextField="ExamName" DataValueField="ExamID" OnDataBound="ExamDropDownList_DataBound" CssClass="form-control">
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="SH"></asp:RequiredFieldValidator>
                                        <asp:SqlDataSource ID="ExamNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName
FROM            Exam_Publish_Setting INNER JOIN
                         Exam_Name ON Exam_Publish_Setting.ExamID = Exam_Name.ExamID
WHERE        (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.ClassID = @ClassID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID)">
                                            <SelectParameters>
                                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                                <asp:ControlParameter ControlID="Roll_Class_DropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                                <asp:ControlParameter ControlID="EduYearDropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                                            </SelectParameters>
                                        </asp:SqlDataSource>
                                    </div>
                                </asp:View>
                            </asp:MultiView>
                            <div class="form-group">
                                <asp:Button ID="ShowPositionButton" runat="server" CssClass="btn btn-primary" OnClick="ShowPositionButton_Click" Text="Set Roll No" ValidationGroup="SH" />
                            </div>
                            <div class="form-group">
                                <asp:Button ID="UpdateRollButton" runat="server" CssClass="btn btn-primary" OnClick="UpdateRollButton_Click" Text="Update" />
                            </div>
                            <div class="form-group">
                                <button class="btn btn-primary" onclick="window.print()">Print</button>
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

    <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT Student.StudentID, Student.SMSPhoneNo, Student.StudentsName, Student.Gender, Student.StudentsLocalAddress, Student.MothersName, Student.FathersName, Student.FatherPhoneNumber, Student.GuardianName, StudentsClass.RollNo, StudentsClass.SeatNo, Student.ID, Student.SMSPhoneNo AS Expr1, Student.MotherPhoneNumber, Student.FatherOccupation, Student.GuardianPhoneNumber, StudentsClass.StudentClassID, Student.StudentImageID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo , '$' , '') , ',' , '') AS FLOAT) ELSE 0 END" UpdateCommand="UPDATE StudentsClass SET RollNo = @RollNo, SeatNo = @SeatNo WHERE (SchoolID = @SchoolID) AND (StudentClassID = @StudentClassID) AND (EducationYearID = @EducationYearID)">
        <SelectParameters>
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
            <asp:Parameter DefaultValue="Active" Name="Status" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
        </SelectParameters>
        <UpdateParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:Parameter Name="StudentClassID" />
            <asp:Parameter Name="RollNo" />
            <asp:Parameter Name="SeatNo" />
        </UpdateParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="ShowIDSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
        SelectCommand="SELECT Student.ID, Student.StudentsName, Student.StudentsLocalAddress, Student.MothersName, Student.FathersName, StudentsClass.RollNo, StudentsClass.SeatNo, Student.SMSPhoneNo, Student.Gender, Student.MotherPhoneNumber, Student.FatherPhoneNumber, Student.GuardianPhoneNumber, StudentsClass.StudentClassID, StudentsClass.StudentID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (Student.ID = @ID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID)">
        <SelectParameters>
            <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" Type="String" />
            <asp:Parameter DefaultValue="Active" Name="Status" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
        </SelectParameters>
    </asp:SqlDataSource>


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
            //upload image
            $('input[name=Student_Photo]').change(function (input) {
                var file = input.target.files[0];
                var prev = $(this).closest('.avatar-upload').find('.student-imgs');
                var id = $(this).closest('.avatar-upload').find('.ImageID');
                var success_msg = $(this).closest('.avatar-upload').find('.success_message');

                var Valid = ["image/jpg", "image/jpeg", "image/png"];

                if ($.inArray(file["type"], Valid) < 0) {
                    alert('Please upload file having extensions .jpeg/.jpg/.png only');
                    return false;
                }
                else {
                    canvasResize(file, {
                        width: 250,
                        quality: 70,
                        callback: function (idata) {
                            $(prev).attr('src', idata);

                            $.ajax({
                                url: "Find_Student.aspx/UpdateImage",
                                data: JSON.stringify({ 'StudentImageID': id.val(), 'Image': idata.split(",")[1] }),
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                success: function (response) {
                                    success_msg.fadeIn();
                                    setTimeout(function () {success_msg.fadeOut("slow")}, 2000);
                                },
                                error: function (xhr) {
                                    var err = JSON.parse(xhr.responseText);
                                    alert(err.message);
                                }
                            });
                        }
                    });
                }
            });

            //Autocomplete
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

            if ($("[id*=RollNoGridView] tr").length) {
                $("#CountStudent").text(`TOTAL: ${$("[id*=RollNoGridView] td").closest("tr").length} STUDENT`);
                $("#InfoWraper").show();
            }
        });
    </script>
</asp:Content>
