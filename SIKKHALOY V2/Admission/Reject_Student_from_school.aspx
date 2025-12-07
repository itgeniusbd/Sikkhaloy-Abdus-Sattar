<%@ Page Title="TC" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Reject_Student_from_school.aspx.cs" Inherits="EDUCATION.COM.ADMISSION_REGISTER.Reject_Student_from_school" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Reject_Student.css?v=11" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <a href="../Accounts/Payment/Pay_Order.aspx">Go To Pay Order</a>
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
                    <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find" ValidationGroup="1" OnClick="FindButton_Click" />
                </div>
            </div>

            <!-- Hidden fields to store student info for postback -->
    <input type="hidden" id="hdnStudentID" runat="server" />
    <input type="hidden" id="hdnStudentClassID" runat="server" />
    <input type="hidden" id="hdnStudentIDText" runat="server" />
    <input type="hidden" id="hdnDeactivateTime" runat="server" />

<asp:UpdatePanel ID="UpdatePanelStudentInfo" runat="server" UpdateMode="Conditional">
<ContentTemplate>
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
   <asp:Button ID="ActiveButton" runat="server" CssClass="btn btn-success btn-lg" Text="? Active Student" OnClick="ActiveButton_Click" />
     </div>
       </div>
</ContentTemplate>
<Triggers>
<asp:AsyncPostBackTrigger ControlID="ConfirmActiveButton" EventName="Click" />
<asp:AsyncPostBackTrigger ControlID="FindButton" EventName="Click" />
</Triggers>
</asp:UpdatePanel>

   <!-- Active Student Form - Wrapped in UpdatePanel for better postback -->
<asp:UpdatePanel ID="UpdatePanelActiveStudent" runat="server" UpdateMode="Conditional">
 <ContentTemplate>
         <asp:Panel ID="ActiveStudentPanel" runat="server" Visible="false" CssClass="card mt-3 NoPrint">
    <div class="card-header bg-success text-white">
     <h5>? Active Student - Select Class & Session</h5>
  </div>
         <div class="card-body">
     <div class="row">
        <div class="col-md-3">
<label>Select Class <span class="text-danger">*</span></label>
   <asp:DropDownList ID="ActiveClassDropDown" runat="server" CssClass="form-control" 
   DataSourceID="ActiveClassSQL" DataTextField="Class" DataValueField="ClassID" 
     AppendDataBoundItems="true" AutoPostBack="true" 
OnSelectedIndexChanged="ActiveClassDropDown_SelectedIndexChanged">
 <asp:ListItem Value="0">[ Select Class ]</asp:ListItem>
 </asp:DropDownList>
   <asp:SqlDataSource ID="ActiveClassSQL" runat="server" 
        ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
       SelectCommand="SELECT ClassID, Class FROM CreateClass WHERE SchoolID = @SchoolID ORDER BY SN">
 <SelectParameters>
   <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
          </SelectParameters>
  </asp:SqlDataSource>
   </div>

     <div class="col-md-2" id="divSection" runat="server">
     <label>Section</label>
<asp:DropDownList ID="ActiveSectionDropDown" runat="server" CssClass="form-control" 
      DataSourceID="ActiveSectionSQL" DataTextField="Section" DataValueField="SectionID" 
   AppendDataBoundItems="true">
   <asp:ListItem Value="0">[ No Section ]</asp:ListItem>
  </asp:DropDownList>
  <asp:SqlDataSource ID="ActiveSectionSQL" runat="server" 
   ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
 SelectCommand="SELECT SectionID, Section FROM CreateSection WHERE SchoolID = @SchoolID AND ClassID = @ClassID ORDER BY Section"
      OnSelecting="ActiveSectionSQL_Selecting">
    <SelectParameters>
   <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
 <asp:ControlParameter Name="ClassID" ControlID="ActiveClassDropDown" PropertyName="SelectedValue" DefaultValue="0" />
  </SelectParameters>
   </asp:SqlDataSource>
       </div>

 <div class="col-md-2" id="divGroup" runat="server">
      <label>Group</label>
     <asp:DropDownList ID="ActiveGroupDropDown" runat="server" CssClass="form-control" 
      DataSourceID="ActiveGroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" 
  AppendDataBoundItems="true">
         <asp:ListItem Value="0">[ No Group ]</asp:ListItem>
     </asp:DropDownList>
       <asp:SqlDataSource ID="ActiveGroupSQL" runat="server" 
    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
  SelectCommand="SELECT SubjectGroupID, SubjectGroup FROM CreateSubjectGroup WHERE SchoolID = @SchoolID AND ClassID = @ClassID ORDER BY SubjectGroup"
   OnSelecting="ActiveGroupSQL_Selecting">
        <SelectParameters>
 <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
  <asp:ControlParameter ControlID="ActiveClassDropDown" Name="ClassID" PropertyName="SelectedValue" DefaultValue="0" />
  </SelectParameters>
        </asp:SqlDataSource>
  </div>

 <div class="col-md-2" id="divShift" runat="server">
 <label>Shift</label>
   <asp:DropDownList ID="ActiveShiftDropDown" runat="server" CssClass="form-control" 
    DataSourceID="ActiveShiftSQL" DataTextField="Shift" DataValueField="ShiftID" 
 AppendDataBoundItems="true">
<asp:ListItem Value="0">[ No Shift ]</asp:ListItem>
 </asp:DropDownList>
    <asp:SqlDataSource ID="ActiveShiftSQL" runat="server" 
     ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
     SelectCommand="SELECT ShiftID, Shift FROM CreateShift WHERE SchoolID = @SchoolID ORDER BY Shift">
      <SelectParameters>
 <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
       </SelectParameters>
</asp:SqlDataSource>
      </div>

<div class="col-md-3">
<label>Session Year <span class="text-danger">*</span></label>
     <asp:DropDownList ID="ActiveSessionDropDown" runat="server" CssClass="form-control" 
     DataSourceID="ActiveSessionSQL" DataTextField="EducationYear" DataValueField="EducationYearID" 
    AppendDataBoundItems="true">
         <asp:ListItem Value="0">[ Select Session ]</asp:ListItem>
   </asp:DropDownList>
     <asp:SqlDataSource ID="ActiveSessionSQL" runat="server" 
  ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
 SelectCommand="SELECT EducationYearID, EducationYear FROM Education_Year WHERE SchoolID = @SchoolID ORDER BY EducationYearID DESC">
   <SelectParameters>
   <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>
     </div>
     </div>

   <div class="alert alert-info mt-3">
    <strong>?? Note:</strong> Student will be activated in the selected class and session.
      </div>

   <div class="mt-3">
       <asp:Button ID="ConfirmActiveButton" runat="server" CssClass="btn btn-success btn-lg" 
   Text="? Confirm & Active Student" OnClick="ConfirmActiveButton_Click" 
   ValidationGroup="ActiveStudent" UseSubmitBehavior="true" />
      <asp:Button ID="CancelActiveButton" runat="server" CssClass="btn btn-secondary ml-2" 
   Text="Cancel" OnClick="CancelActiveButton_Click" CausesValidation="false" />
   <asp:RequiredFieldValidator ID="rfvClass" runat="server" 
  ControlToValidate="ActiveClassDropDown" InitialValue="0" 
   ErrorMessage="Please select a class!" CssClass="text-danger ml-2" 
     Display="Dynamic" ValidationGroup="ActiveStudent" />
  <asp:RequiredFieldValidator ID="rfvSession" runat="server" 
      ControlToValidate="ActiveSessionDropDown" InitialValue="0" 
   ErrorMessage="Please select a session!" CssClass="text-danger ml-2" 
    Display="Dynamic" ValidationGroup="ActiveStudent" />
  </div>
</div>
        </asp:Panel>
  </ContentTemplate>
   <Triggers>
   <asp:AsyncPostBackTrigger ControlID="ActiveButton" EventName="Click" />
   <asp:AsyncPostBackTrigger ControlID="ConfirmActiveButton" EventName="Click" />
  <asp:AsyncPostBackTrigger ControlID="CancelActiveButton" EventName="Click" />
       <asp:AsyncPostBackTrigger ControlID="ActiveClassDropDown" EventName="SelectedIndexChanged" />
 </Triggers>
</asp:UpdatePanel>
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
                            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
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
                                <asp:BoundField DataField="FatherPhoneNumber" HeaderText="Father's Phone" SortExpression="FatherPhoneNumber" />
                                <asp:BoundField DataField="MotherPhoneNumber" HeaderText="Mother's Phone" SortExpression="MotherPhoneNumber" />
                                <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
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
                        <asp:SqlDataSource ID="StatusSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, ISNULL(CreateSection.Section, 'No section') AS Section, ISNULL(CreateSubjectGroup.SubjectGroup, 'No Group') AS [Group], StudentsClass.ClassID, CreateShift.Shift, Student.StudentID, Student.ID, Student.SMSPhoneNo, Student.FatherPhoneNumber, Student.MotherPhoneNumber, Student.StudentsName, Student.Gender, Student.StudentPermanentAddress, Student.Status, Student.RejectedDate, Student.FathersName, StudentsClass.StudentClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.Status = @Status) AND (Student.SchoolID = @SchoolID) AND (StudentsClass.Class_Status IS NULL)  AND (StudentsClass.ClassID LIKE ISNULL(@ClassID, N'%')) AND (Student.ID LIKE ISNULL(@ID, N'%'))" UpdateCommand="UPDATE Student SET Status = @Status WHERE (ID = @ID)" OnSelected="StatusSQL_Selected" CancelSelectOnNullParameter="False">
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
    <div class="progress_BG"></div>
    <div class="progress">
       <img src="../CSS/loading.gif" alt="Loading..." /><br />
 <b>Loading...</b>
      </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script type="text/javascript">
  function initializeTypeahead() {
    if (typeof $.fn.typeahead !== 'undefined') {
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
     try {
      var data = JSON.parse(response.d);
          result($.map(data, function (item) {
          return item;
    }));
     } catch (e) {
console.error('Error parsing typeahead response:', e);
         }
   },
    error: function (xhr, status, error) {
       console.error('Typeahead AJAX error:', error);
     }
    });
   }
 });
    } else {
    console.warn('Typeahead plugin not loaded');
         }
        }

 $(function () {
      initializeTypeahead();

      console.log('Page loaded');
   console.log('Student search textbox:', $('[id$="IDTextBox"]').val());

   if ($(".isStatus").text() === "Active") {
      $(".isStatus").css("color", "green");
      } else {
       $(".isStatus").css("color", "red");
 }

// Store FormView DataKeys in hidden fields after page load
     var $formView = $('[id$="StudentInfoFormView"]');
 if ($formView.length > 0 && $formView.find('.info').length > 0) {
   // Extract data from FormView's rendered HTML
    var studentIDText = $formView.find('.info ul li:first b').text().match(/\(([^)]+)\)/);
   if (studentIDText && studentIDText[1]) {
     $('[id$="hdnStudentIDText"]').val(studentIDText[1]);
     console.log('Stored Student ID Text:', studentIDText[1]);
  }
 }
     });
</script>
</asp:Content>
