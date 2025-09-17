<%@ Page Title="SMS Inbox" Language="C#" MasterPageFile="~/Basic_Teacher.Master" AutoEventWireup="true" CodeBehind="SMS_Inbox.aspx.cs" Inherits="EDUCATION.COM.Teacher.SMS_Inbox" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Admission/CSS/Report.css" rel="stylesheet" />

    <style>
.p-image img {
  height: 100px;
  width: 100px;
}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Add Student Report</h3>
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
                                        <b>(<label id="IDLabel"><%# Eval("ID") %></label>)
                                                                <%# Eval("StudentsName") %></b> 
                                                               <b> -- Father's Name:</b> <%# Eval("FathersName") %> --
                                                               <b>Phone:</b><%# Eval("SMSPhoneNo") %>
                                                                
                                    </li>

                                    <li class="alert-info">
                                        <b>Class:</b>
                                        <%# Eval("Class") %>
                                        <%# Eval("SubjectGroup",", Group: {0}") %>
                                        <%# Eval("Section",", Section: {0}") %>
                                        <%# Eval("Shift",", Shift: {0}") %>
                                        <b>Roll No:</b>
                                        <%# Eval("RollNo") %>
                                    </li>


                                </ul>
                            </div>
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
        <li class="nav-item"><a class="nav-link" href="#AddReport" data-toggle="tab" role="tab" aria-expanded="false"> Add Student Report</a></li>
        
        <li class="nav-item"><a class="nav-link" href="#Report" data-toggle="tab" role="tab" aria-expanded="false">Print Student Report</a></li>
        
        
    </ul>

    <div class="tab-content card">
        <div id="AddReport" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
            <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                <ContentTemplate>
                    <button type="button" class="btn btn-grey d-print-none btn-sm" data-toggle="modal" data-target="#FaultModal">Add Report</button>
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
                    <asp:SqlDataSource ID="FaultSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Student_Fault] WHERE [StudentFaultID] = @StudentFaultID" InsertCommand="INSERT INTO [Student_Fault] ([SchoolID], [RegistrationID], [EducationYearID], [StudentID], [StudentClassID], [Fault_Title], [Fault], [Fault_Date]) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @StudentClassID, @Fault_Title, @Fault, @Fault_Date)" SelectCommand="SELECT StudentFaultID, Fault_Title, Fault, Fault_Date FROM Student_Fault WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID) ORDER BY Fault_Date DESC" UpdateCommand="UPDATE Student_Fault SET Fault_Title = @Fault_Title, Fault = @Fault, Fault_Date = @Fault_Date WHERE (StudentFaultID = @StudentFaultID)">
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

        <div id="Report" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                <div class="form-inline head-area NoPrint">
        <div class="form-group">
            <asp:TextBox ID="From_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:TextBox ID="To_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
            <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar"></i>
        </div>
        <div class="form-group">
            <asp:Button ID="Find_Button" CssClass="btn btn-primary" runat="server" Text="Submit" />
        </div>
        <div class="form-group pull-right Print">
            <a title="Print This Page" onclick="window.print();"><i class="fa fa-print" aria-hidden="true"></i></a>
        </div>
    </div>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <asp:GridView ID="FindGridView" CssClass="mGrid" DataKeyNames="StudentFaultID" runat="server" DataSourceID="SqlDataSource1" AutoGenerateColumns="False" Width="100%" AllowPaging="True" AllowSorting="True" PageSize="30">
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

                </Columns>
                <PagerStyle CssClass="pgr" />
            </asp:GridView>
            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT StudentFaultID, Fault_Title, Fault, Fault_Date FROM Student_Fault WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID ) AND (StudentClassID = @StudentClassID) AND Fault_Date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') ORDER BY Fault_Date DESC" CancelSelectOnNullParameter="False">
                <DeleteParameters>
                    <asp:Parameter Name="StudentFaultID" Type="Int32" />
                </DeleteParameters>
                    <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />
                     <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                     <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                </SelectParameters>
          
            </asp:SqlDataSource>
        </ContentTemplate>
    </asp:UpdatePanel>
</div>
         
          
    </div>

   

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
                                <asp:TextBox ID="Fault_TextBox" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Report Date<asp:RequiredFieldValidator ControlToValidate="Fault_Date_TextBox" ValidationGroup="Fa" ID="RequiredFieldValidator4" runat="server" CssClass="EroorStar" ErrorMessage="*" /></label>
                                <asp:TextBox ID="Fault_Date_TextBox" runat="server" CssClass="form-control datepicker"></asp:TextBox>
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

    <script>
        $(function () {
            $("#_4").addClass("active");
        });

        $('.datepicker').datepicker({
            format: 'dd M yyyy',
            todayBtn: "linked",
            todayHighlight: true,
            autoclose: true
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

        function openModal() {
            $('#myModal').modal('show');
        }
    </script>
</asp:Content>
