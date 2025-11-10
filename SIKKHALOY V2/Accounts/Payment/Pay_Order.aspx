<%@ Page Title="Pay Order" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Pay_Order.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.Payment.Pay_Order" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .Role_Name { font-size: 15px; font-weight: bold; }
        .MultiRoleGV tr td { border: 1px solid #ddd; padding: 6px 0; }
        #Contain .form-control { display: inline; width: 96%; }

        .A_MR table tr td { position: relative; }
        .UAR2 table tr td { position: relative; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <h3>Pay Order For Student</h3>

    <asp:Label ID="PayorderMsgLabel" runat="server" CssClass="alert-success"></asp:Label>
    <asp:Label ID="EmptyStudentLabel" runat="server" CssClass="EroorSummer"></asp:Label>

    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" onfocus="SelectedItemCLR(this);" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ Select All students or Class ]</asp:ListItem>
                <asp:ListItem Value="-1">All Students</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT CreateClass.Class, StudentsClass.ClassID FROM StudentsClass INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = N'Active') ORDER BY StudentsClass.ClassID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group G_Show" style="display: none">
            <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup"
                DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group S_Show" style="display: none">
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="OldNewDropDownList" CssClass="form-control" runat="server" AutoPostBack="True">
                <asp:ListItem Value="%">All Student</asp:ListItem>
                <asp:ListItem Value="1">New Student</asp:ListItem>
                <asp:ListItem Value="0">Old Student</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="form-group">
            <asp:TextBox ID="IDTextBox" placeholder="Separate the ID by comma" runat="server" CssClass="form-control" TextMode="MultiLine" Height="34px"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="IDTextBox" CssClass="EroorStar" ValidationGroup="F" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="Find_ID_Button" runat="server" CssClass="btn btn-primary" ValidationGroup="F" Text="Find Student" OnClick="Find_ID_Button_Click" />
        </div>
    </div>

    <div id="Contain">
        <div class="table-responsive mb-4 Hide_S_Gv" style="display: none">
            <div class="alert alert-info">
                Select All or Specific Student
                (<asp:Label ID="TotalStudentLabel" runat="server"></asp:Label>)
            </div>
            <div style="max-height: 500px; overflow: auto;">
                <asp:GridView ID="StudentsGridView" runat="server" AutoGenerateColumns="False" PageSize="100" DataSourceID="ShowStudentClassSQL" DataKeyNames="StudentID,StudentClassID,ClassID" CssClass="mGrid" AllowSorting="True">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <ItemStyle Width="50px" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName"></asp:BoundField>
                        <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo" />
                        <asp:BoundField DataField="Section" HeaderText="Section" SortExpression="Section" />
                        <asp:BoundField DataField="FathersName" HeaderText="Father's Name" SortExpression="FathersName" />
                        <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
                    </Columns>
                    <PagerStyle CssClass="pgr" />
                </asp:GridView>
                <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="IF(@IDs &lt;&gt; '')
BEGIN 
SELECT Student.SMSPhoneNo, Student.StudentsName, Student.StudentEmailAddress, Student.Gender, Student.DateofBirth, Student.BloodGroup, Student.Religion, Student.StudentPermanentAddress, Student.StudentsLocalAddress, Student.PrevSchoolName, Student.MothersName, Student.FathersName, Student.FatherOccupation, Student.FatherPhoneNumber, StudentsClass.RollNo, Student.ID, StudentsClass.StudentID,StudentsClass.ClassID, StudentsClass.StudentClassID, CreateSection.Section FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID WHERE (Student.Status = 'Active') AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.ID IN(SELECT id FROM dbo.In_Function_Parameter(@IDs))) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS int) ELSE 0 END
END
ELSE
BEGIN
SELECT Student.SMSPhoneNo, Student.StudentsName, StudentsClass.RollNo, Student.ID, StudentsClass.StudentID, StudentsClass.ClassID, StudentsClass.StudentClassID, CreateSection.Section, CreateSubjectGroup.SubjectGroup,Student.FathersName
FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (Student.Status = 'Active') AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.Is_New LIKE @Is_New) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(StudentsClass.RollNo AS int) ELSE 0 END
END"
                    CancelSelectOnNullParameter="False" OnSelected="ShowStudentClassSQL_Selected">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="IDTextBox" DefaultValue="" Name="IDs" PropertyName="Text" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="OldNewDropDownList" Name="Is_New" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any student from student list." ForeColor="Red" ValidationGroup="A"> </asp:CustomValidator>
            </div>
        </div>

        <div class="table-responsive mb-4 A_R" style="display: none;">
            <div class="alert alert-success">Assigned Payment in Class (One Instalment)</div>
            <asp:GridView ID="One_A_RoleGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="AssignRoleID,RoleID,PayFor" DataSourceID="One_A_RoleSQL" CssClass="mGrid">
  <Columns>
                 <asp:TemplateField>
<ItemTemplate>
        <asp:CheckBox ID="A_OR_CheckBox" runat="server" Text=" " />
              </ItemTemplate>
        <ItemStyle Width="50px" />
            </asp:TemplateField>
         <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
   <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
               <asp:TemplateField HeaderText="Amount" SortExpression="Amount">
            <ItemTemplate>
          <asp:TextBox ID="AmountTextBox" runat="server" Text='<%# Bind("Amount") %>' CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
   <asp:RequiredFieldValidator ID="AmountRF" Enabled="false" runat="server" ControlToValidate="AmountTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
 </ItemTemplate>
         </asp:TemplateField>
     <asp:TemplateField HeaderText="Start Date" SortExpression="StartDate">
       <ItemTemplate>
     <asp:TextBox ID="StartDateTextBox" runat="server" Text='<%# Bind("StartDate","{0:d MMM yyyy}") %>' CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
 <asp:RequiredFieldValidator ID="StartdateRF" Enabled="false" runat="server" ControlToValidate="StartDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
      </ItemTemplate>
          </asp:TemplateField>
                  <asp:TemplateField HeaderText="End Date" SortExpression="EndDate">
          <ItemTemplate>
    <asp:TextBox ID="EndDateTextBox" runat="server" Text='<%# Bind("EndDate","{0:d MMM yyyy}") %>' CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
              <asp:RequiredFieldValidator ID="EndDateRF" Enabled="false" runat="server" ControlToValidate="EndDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
     </ItemTemplate>
         </asp:TemplateField>
        <asp:TemplateField HeaderText="Late Fee" SortExpression="LateFee">
      <ItemTemplate>
   <asp:TextBox ID="LateFeeTextBox" runat="server" Text='<%# Bind("LateFee") %>' CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
 </ItemTemplate>
           </asp:TemplateField>
  <asp:TemplateField HeaderText="Concession">
         <ItemTemplate>
      <asp:TextBox ID="DiscountTextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
       </ItemTemplate>
   </asp:TemplateField>
    </Columns>
     </asp:GridView>
   <asp:SqlDataSource ID="One_A_RoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
      SelectCommand="SELECT Income_Assign_Role.AssignRoleID, Income_Assign_Role.RoleID, Income_Roles.Role, Income_Assign_Role.PayFor, Income_Assign_Role.Amount, Income_Assign_Role.LateFee, Income_Assign_Role.StartDate, Income_Assign_Role.EndDate
FROM Income_Assign_Role 
INNER JOIN Income_Roles ON Income_Assign_Role.RoleID = Income_Roles.RoleID 
WHERE (Income_Assign_Role.ClassID = @ClassID) 
  AND (Income_Assign_Role.EducationYearID = @EducationYearID) 
  AND (Income_Assign_Role.SchoolID = @SchoolID)
  AND (Income_Roles.NumberOfPay = 1)
ORDER BY Income_Assign_Role.StartDate">
       <SelectParameters>
        <asp:Parameter Name="ClassID" Type="Int32" />
        <asp:Parameter Name="SchoolID" Type="Int32" />
        <asp:Parameter Name="EducationYearID" Type="Int32" />
     </SelectParameters>
          </asp:SqlDataSource>
        </div>
        <div class="table-responsive mb-4 A_MR" style="display: none;">
            <div class="alert alert-success">Assigned Payment in Class (Multiple Instalments)</div>
            <asp:GridView ID="Multi_A_Role_GridView" runat="server" AutoGenerateColumns="False" CssClass="MultiRoleGV" DataSourceID="Multi_A_RoleSQL" ShowHeader="False" Width="100%" DataKeyNames="RoleID" OnRowDataBound="Multi_A_Role_GridView_RowDataBound">
    <Columns>
  <asp:TemplateField>
       <ItemTemplate>
       <div class="Role_Name">
      <asp:CheckBox ID="Multi_AddCheckBox" runat="server" Text=" " />
 Add - 
    <asp:Label ID="RoleLabel" runat="server" Text='<%# Bind("Role") %>'></asp:Label>
       ( Number of Payment Instalment
   <asp:Label ID="Multi_No_PayLabel" runat="server" Text='<%# Bind("No_Of_Role") %>'></asp:Label>
)
         <asp:HiddenField ID="RoleHiddenField" runat="server" Value='<%# Eval("RoleID") %>' />
  </div>
      <div class="criteriaData" style="display: none">
  <asp:GridView ID="Input_Multi_Role_GridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" Width="100%" DataKeyNames="AssignRoleID">
      <Columns>
    <asp:TemplateField>
       <ItemTemplate>
        <asp:CheckBox ID="Input_MultiCheckBox" runat="server" Text=" " />
  </ItemTemplate>
 </asp:TemplateField>
     <asp:TemplateField HeaderText="Pay For">
       <ItemTemplate>
   <asp:Label ID="Multi_PayForLabel" runat="server" Text='<%# Eval("PayFor") %>' Visible="false"></asp:Label>
   <asp:TextBox ID="Multi_PayForTextBox" autocomplete="off" placeholder="Pay For" Text='<%# Eval("PayFor") %>' runat="server" CssClass="PayFor form-control"></asp:TextBox>
      <asp:RequiredFieldValidator ID="payForRF" Enabled="false" runat="server" ControlToValidate="Multi_PayForTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
   </ItemTemplate>
    </asp:TemplateField>
 <asp:TemplateField>
  <HeaderTemplate>
       <small>Amount Same For All</small><br />
 <asp:TextBox ID="AssignAmountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Amount Same For All" runat="server"></asp:TextBox>
  </HeaderTemplate>
  <ItemTemplate>
  <asp:TextBox ID="Multi_AmountTextBox" placeholder="Amount" Text='<%# Eval("Amount") %>' runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
   <asp:RequiredFieldValidator ID="AmountRF" Enabled="false" runat="server" ControlToValidate="Multi_AmountTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
          </ItemTemplate>
     </asp:TemplateField>
   <asp:TemplateField HeaderText="Start Date">
     <ItemTemplate>
      <asp:TextBox ID="Multi_StartDateTextBox" placeholder="Start Date" Text='<%# Eval("StartDate","{0:d MMM yyyy}") %>' runat="server" CssClass="form-control Datetime Multi_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
      <asp:RequiredFieldValidator ID="StartdateRF" Enabled="false" runat="server" ControlToValidate="Multi_StartDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
   </ItemTemplate>
 </asp:TemplateField>
  <asp:TemplateField HeaderText="End Date">
 <ItemTemplate>
 <asp:TextBox ID="Multi_EndDateTextBox" placeholder="End Date" Text='<%# Eval("EndDate","{0:d MMM yyyy}") %>' runat="server" CssClass="form-control Datetime Multi_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
    <asp:RequiredFieldValidator ID="EndDateRF" Enabled="false" runat="server" ControlToValidate="Multi_EndDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
      </ItemTemplate>
 </asp:TemplateField>
 <asp:TemplateField>
 <HeaderTemplate>
   <small>Late Fee Same For All</small><br />
  <asp:TextBox ID="AssinLFeeTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Late Fee Same For All" runat="server"></asp:TextBox>
        </HeaderTemplate>
      <ItemTemplate>
 <asp:TextBox ID="Multi_LateFeeTextBox" placeholder="Late Fee" runat="server" CssClass="form-control"></asp:TextBox>
     </ItemTemplate>
     </asp:TemplateField>
        <asp:TemplateField>
 <HeaderTemplate>
  <small>Concession Same For All</small><br />
     <asp:TextBox ID="AssConcessionTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Concession Same For All" runat="server"></asp:TextBox>
  </HeaderTemplate>
    <ItemTemplate>
   <asp:TextBox ID="Multi_DiscountTextBox" placeholder="Concession" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
      </ItemTemplate>
  </asp:TemplateField>
    </Columns>
       </asp:GridView>
      </div>
  </ItemTemplate>
     </asp:TemplateField>
    </Columns>
    </asp:GridView>
         <asp:SqlDataSource ID="Multi_A_RoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
   SelectCommand="SELECT DISTINCT Income_Roles.Role, Income_Assign_Role.RoleID, Income_Roles.NumberOfPay AS No_Of_Role
FROM Income_Assign_Role 
INNER JOIN Income_Roles ON Income_Assign_Role.RoleID = Income_Roles.RoleID 
WHERE (Income_Assign_Role.ClassID = @ClassID) 
  AND (Income_Assign_Role.EducationYearID = @EducationYearID) 
  AND (Income_Assign_Role.SchoolID = @SchoolID)
  AND (Income_Roles.NumberOfPay > 1)
ORDER BY Income_Assign_Role.RoleID">
      <SelectParameters>
      <asp:Parameter Name="ClassID" Type="Int32" />
      <asp:Parameter Name="SchoolID" Type="Int32" />
      <asp:Parameter Name="EducationYearID" Type="Int32" />
      </SelectParameters>
  </asp:SqlDataSource>
        </div>

        <%if (ClassDropDownList.SelectedIndex > 0 || StudentsGridView.Rows.Count > 0)
            { %>
        <asp:Button ID="PayOrderButton" runat="server" Text="Pay Order" CssClass="btn btn-primary" OnClick="PayOrderButton_Click" ValidationGroup="A" CausesValidation="true" />
        <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="EroorSummer" DisplayMode="List" ShowMessageBox="True" ValidationGroup="A" />
        <asp:SqlDataSource ID="PayOrderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Income_PayOrder(SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, AssignRoleID, EducationYearID, Amount, PaidAmount, LateFee, Discount, LateFee_Discount, RoleID, PayFor, StartDate, EndDate, CreatedDate) VALUES (@SchoolID, @RegistrationID, @StudentID, @ClassID, @StudentClassID, @AssignRoleID, @EducationYearID, @Amount, @PaidAmount, @LateFee, @Discount, @LateFee_Discount, @RoleID, @PayFor, @StartDate, @EndDate, GETDATE())" SelectCommand="SELECT * FROM [Income_PayOrder]">
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:Parameter Name="StudentID" Type="Int32" />
                <asp:Parameter Name="ClassID" Type="Int32" />
                <asp:Parameter Name="AssignRoleID" />
                <asp:Parameter Name="StudentClassID" Type="Int32" />
                <asp:Parameter DefaultValue="" Name="Amount" Type="Double" />
                <asp:Parameter DefaultValue="0" Name="PaidAmount" Type="Double" />
                <asp:Parameter DefaultValue="" Name="LateFee" Type="Double" />
                <asp:Parameter Name="Discount" Type="Double" />
                <asp:Parameter DefaultValue="" Name="LateFee_Discount" Type="Double" />
                <asp:Parameter DefaultValue="" Name="RoleID" Type="Int32" />
                <asp:Parameter Name="PayFor" Type="String" />
                <asp:Parameter DbType="Date" Name="StartDate" />
                <asp:Parameter DbType="Date" Name="EndDate" DefaultValue="" />
            </InsertParameters>
        </asp:SqlDataSource>
        <%}
            else
            {%>
        <p class="EroorSummer">For Any Pay Order Select All students or Spacific Class</p>
        <%} %>
    </div>


    <div class="table-responsive mb-3 UAR1" style="display: none;">
  <div class="alert alert-info">Unassigned Payment (One Instalment)</div>
        <asp:GridView ID="One_Role_GridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="Roles_1_SQL" DataKeyNames="RoleID">
  <Columns>
  <asp:TemplateField>
     <ItemTemplate>
         <asp:CheckBox ID="One_Role_CheckBox" runat="server" Text=" " />
      </ItemTemplate>
        </asp:TemplateField>
                <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
  <asp:TemplateField HeaderText="Pay For">
           <ItemTemplate>
         <asp:TextBox ID="One_PayForTextBox" runat="server" CssClass="form-control One_Role" placeholder="Pay For"></asp:TextBox>
   <asp:RequiredFieldValidator ID="payForRF" Enabled="false" runat="server" ControlToValidate="One_PayForTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
          </ItemTemplate>
        </asp:TemplateField>
                <asp:TemplateField HeaderText="Amount">
   <ItemTemplate>
            <asp:TextBox ID="One_AmountTextBox" placeholder="Amount" runat="server" CssClass="form-control One_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
     <asp:RequiredFieldValidator ID="AmountRF" Enabled="false" runat="server" ControlToValidate="One_AmountTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
         </ItemTemplate>
       </asp:TemplateField>
    <asp:TemplateField HeaderText="Start Date">
          <ItemTemplate>
     <asp:TextBox ID="One_StartDateTextBox" placeholder="Start Date" runat="server" CssClass="form-control Datetime One_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
      <asp:RequiredFieldValidator ID="StartdateRF" Enabled="false" runat="server" ControlToValidate="One_StartDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
      </ItemTemplate>
      </asp:TemplateField>
    <asp:TemplateField HeaderText="End Date">
          <ItemTemplate>
    <asp:TextBox ID="One_EndDateTextBox" placeholder="End Date" runat="server" CssClass="form-control Datetime One_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                <asp:RequiredFieldValidator ID="EndDateRF" Enabled="false" runat="server" ControlToValidate="One_EndDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
          </ItemTemplate>
           </asp:TemplateField>
             <asp:TemplateField HeaderText="Late Fee">
    <ItemTemplate>
     <asp:TextBox ID="One_LateFeeTextBox" placeholder="Late Fee" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control One_Role"></asp:TextBox>
       </ItemTemplate>
          </asp:TemplateField>
              <asp:TemplateField HeaderText="Concession">
     <ItemTemplate>
       <asp:TextBox ID="One_DiscountTextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
             </ItemTemplate>
             </asp:TemplateField>
        </Columns>
        </asp:GridView>
      <asp:SqlDataSource ID="Roles_1_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoleID, Role, NumberOfPay, Description, Date FROM Income_Roles WHERE (RoleID NOT IN (SELECT RoleID FROM Income_Assign_Role WHERE (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID))) AND (SchoolID = @SchoolID) AND (NumberOfPay = 1)">
         <SelectParameters>
     <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <div class="table-responsive mb-3 UAR2" style="display: none;">
          <div class="alert alert-danger">Unassigned Payment (Multiple Instalments)</div>
        <asp:GridView ID="Multi_R_GridView" runat="server" AutoGenerateColumns="False" DataKeyNames="RoleID,NumberOfPay" DataSourceID="Multi_R_SQL" ShowHeader="False" Width="100%" OnRowDataBound="Multi_Role_GridView_RowDataBound" CssClass="MultiRoleGV">
      <Columns>
       <asp:TemplateField>
              <ItemTemplate>
           <div class="Role_Name">
 <asp:CheckBox ID="Multi_AddCheckBox" runat="server" Text=" " />
          Add - 
    <asp:Label ID="RoleLabel" runat="server" Text='<%# Bind("Role") %>'></asp:Label>
      ( Number of Payment Instalment
              <asp:Label ID="Multi_No_PayLabel" runat="server" Text='<%# Bind("NumberOfPay") %>'></asp:Label>
       )
          </div>
       <div class="criteriaData" style="display: none">
       <asp:GridView ID="Input_Multi_Role_GridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" Width="100%">
             <Columns>
   <asp:TemplateField>
       <ItemTemplate>
   <asp:CheckBox ID="Input_MultiCheckBox" runat="server" Text=" " />
  </ItemTemplate>
   </asp:TemplateField>
             <asp:TemplateField HeaderText="Pay For">
     <ItemTemplate>
     <asp:TextBox ID="Multi_PayForTextBox" autocomplete="off" placeholder="Pay For" runat="server" CssClass="PayFor form-control"></asp:TextBox>
         <asp:RequiredFieldValidator ID="payForRF" Enabled="false" runat="server" ControlToValidate="Multi_PayForTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
     </ItemTemplate>
    </asp:TemplateField>
      <asp:TemplateField>
<HeaderTemplate>
   <small>Amount Same For All</small><br />
    <asp:TextBox ID="AssignAmountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Amount Same For All" runat="server"></asp:TextBox>
          </HeaderTemplate>
      <ItemTemplate>
  <asp:TextBox ID="Multi_AmountTextBox" placeholder="Amount" Text='<%# Eval("Amount") %>' runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
      <asp:RequiredFieldValidator ID="AmountRF" Enabled="false" runat="server" ControlToValidate="Multi_AmountTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                </ItemTemplate>
       </asp:TemplateField>
     <asp:TemplateField HeaderText="Start Date">
   <ItemTemplate>
   <asp:TextBox ID="Multi_StartDateTextBox" placeholder="Start Date" Text='<%# Bind("StartDate","{0:d MMM yyyy}") %>' runat="server" CssClass="form-control Datetime Multi_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
     <asp:RequiredFieldValidator ID="StartdateRF" Enabled="false" runat="server" ControlToValidate="Multi_StartDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
        </ItemTemplate>
</asp:TemplateField>
    <asp:TemplateField HeaderText="End Date">
 <ItemTemplate>
    <asp:TextBox ID="Multi_EndDateTextBox" placeholder="End Date" Text='<%# Bind("EndDate","{0:d MMM yyyy}") %>' runat="server" CssClass="form-control Datetime Multi_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
<asp:RequiredFieldValidator ID="EndDateRF" Enabled="false" runat="server" ControlToValidate="Multi_EndDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
    </ItemTemplate>
          </asp:TemplateField>
  <asp:TemplateField>
<HeaderTemplate>
     <small>Late Fee Same For All</small><br />
    <asp:TextBox ID="AssinLFeeTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Late Fee Same For All" runat="server"></asp:TextBox>
      </HeaderTemplate>
<ItemTemplate>
        <asp:TextBox ID="Multi_LateFeeTextBox" placeholder="Late Fee" runat="server" CssClass="form-control"></asp:TextBox>
      </ItemTemplate>
  </asp:TemplateField>

        <asp:TemplateField>
      <HeaderTemplate>
       <small>Concession Same For All</small><br />
     <asp:TextBox ID="AssConcessionTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Concession Same For All" runat="server"></asp:TextBox>
</HeaderTemplate>
   <ItemTemplate>
    <asp:TextBox ID="Multi_DiscountTextBox" placeholder="Concession" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
       </ItemTemplate>
     </asp:TemplateField>
      </Columns>
  </asp:GridView>
      </div>
  </ItemTemplate>
           </asp:TemplateField>
    </Columns>
    </asp:GridView>
         <asp:SqlDataSource ID="Multi_R_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoleID, Role, NumberOfPay, Description, Date FROM Income_Roles WHERE (RoleID NOT IN (SELECT RoleID FROM Income_Assign_Role WHERE (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID))) AND (SchoolID = @SchoolID) AND (NumberOfPay &lt;&gt; 1)">
       <SelectParameters>
 <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
     <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Years" />
  </SelectParameters>
    </asp:SqlDataSource>
  </div>

    <script type="text/javascript">
        $(function () {
     // Check for success message on page load
            var urlParams = new URLSearchParams(window.location.search);
       if (urlParams.has('success')) {
  var successCount = urlParams.get('success');
 var failCount = urlParams.get('fail') || '0';
    var message = successCount + ' Pay Order has been created successfully';
        if (failCount > 0) {
          message += ' and ' + failCount + ' Pay Order already existed';
     }
  alert(message);
        // Clean URL
window.history.replaceState({}, document.title, window.location.pathname);
 }
        
   // Handle "All" checkbox in Students GridView
        $("[id*=AllIteamCheckBox]").on("click", function () {
        var isChecked = $(this).is(':checked');
            var grid = $(this).closest('table');
         
         $("input[id*=SingleCheckBox]", grid).each(function () {
    $(this).prop('checked', isChecked);
       if (isChecked) {
         $(this).closest('tr').addClass('selected');
   } else {
  $(this).closest('tr').removeClass('selected');
   }
            });
        });

        // Handle individual student checkbox clicks
    $("[id*=SingleCheckBox]").on("click", function () {
            if ($(this).is(':checked')) {
      $(this).closest('tr').addClass('selected');
     } else {
                $(this).closest('tr').removeClass('selected');
           // Uncheck "All" checkbox if any individual checkbox is unchecked
          $("[id*=AllIteamCheckBox]").prop('checked', false);
            }

            // Check if all checkboxes are checked, then check "All" checkbox
    var grid = $(this).closest('table');
         var totalCheckboxes = $("input[id*=SingleCheckBox]", grid).length;
     var checkedCheckboxes = $("input[id*=SingleCheckBox]:checked", grid).length;
     
      if (totalCheckboxes === checkedCheckboxes) {
    $("[id*=AllIteamCheckBox]").prop('checked', true);
        }
        });
        
      if ($('[id*=SectionDropDownList] option').length > 1) {
         $('.S_Show').show();
      }
 if ($('[id*=GroupDropDownList] option').length > 1) {
      $('.G_Show').show();
   }

          $('.PayFor').on("keypress", function () {
       var tr = $(this).closest("tr");
       $(this).typeahead({
       minLength: 1,
       source: function (request, result) {
      $.ajax({
   url: "Pay_Order.aspx/GetMonth",
    data: JSON.stringify({ 'prefix': request }),
   dataType: "json",
       type: "POST",
    contentType: "application/json; charset=utf-8",
       success: function (response) {
         label = [];
             map = {};
        $.map(JSON.parse(response.d), function (item) {
      label.push(item.Month);
           map[item.Month] = item;
  });
     result(label);
    }
      });
     },
    updater: function (item) {
     $(".Datetime:eq(0)", tr).val("01 " + map[item].MonthYear);
  $(".Datetime:eq(1)", tr).val("10 " + map[item].MonthYear);
          return item;
       }
     });
  });

  $('.Datetime').datepicker({
    format: 'dd M yyyy',
  todayBtn: "linked",
      todayHighlight: true,
 autoclose: true
   });

   //Is Gridview is empty
        if ($('[id*=StudentsGridView] tr').length) {
   $(".Hide_S_Gv").show();
            }
  if ($('[id*=One_A_RoleGridView] tr').length) {
         $(".A_R").show();
  }
        if ($('[id*=Multi_A_Role_GridView] tr').length) {
     $(".A_MR").show();
 }
            if ($('[id*=One_Role_GridView] tr').length) {
   $(".UAR1").show();
       }
       if ($('[id*=Multi_R_GridView] tr').length) {
      $(".UAR2").show();
  }

   //Assign_One Role_CheckBox
    $("[id*=A_OR_CheckBox]").on("click", function () {
    ValidatorEnable($("[id*=AmountRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
  ValidatorEnable($("[id*=StartdateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
      ValidatorEnable($("[id*=EndDateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
    });

     //One Role CheckBox
 $("[id*=One_Role_CheckBox]").on("click", function () {
            ValidatorEnable($("[id*=payForRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
       ValidatorEnable($("[id*=StartdateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
  ValidatorEnable($("[id*=EndDateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
});

  //Multi Role CheckBox - Show/Hide detail section
      $("[id*=Multi_AddCheckBox]").on("click", function () {
    if ($(this).is(':checked')) {
        $(this).closest("tr").find("div.criteriaData").show("slow");
       }
             else {
       $(this).closest("tr").find("div.criteriaData").hide("slow");
      }
       });

       //Multi Role CheckBox - Check/Uncheck all child checkboxes
     $("[id*=Multi_AddCheckBox]").on("click", function () {
      var Multi_AddCheckBox = $(this);
      var grid = $(this).closest("tr").find("div.criteriaData");

     $("input[type=checkbox]", grid).each(function () {
 if (Multi_AddCheckBox.is(":checked")) {
     $(this).attr("checked", "checked");
     ValidatorEnable($("[id*=payForRF]", $(this).closest("tr"))[0], true);
             ValidatorEnable($("[id*=AmountRF]", $(this).closest("tr"))[0], true);
  ValidatorEnable($("[id*=StartdateRF]", $(this).closest("tr"))[0], true);
      ValidatorEnable($("[id*=EndDateRF]", $(this).closest("tr"))[0], true);
 }
   else {
   $(this).removeAttr("checked");
        ValidatorEnable($("[id*=payForRF]", $(this).closest("tr"))[0], false);
        ValidatorEnable($("[id*=AmountRF]", $(this).closest("tr"))[0], false);
       ValidatorEnable($("[id*=StartdateRF]", $(this).closest("tr"))[0], false);
     ValidatorEnable($("[id*=EndDateRF]", $(this).closest("tr"))[0], false);
    }
   });
   });

        $("[id*=Input_MultiCheckBox]").on("click", function () {
   ValidatorEnable($("[id*=payForRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
 ValidatorEnable($("[id*=AmountRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
     ValidatorEnable($("[id*=StartdateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
    ValidatorEnable($("[id*=EndDateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
          });

     //Assign Amount to All
     $("[id*=AssignAmountTextBox]").on("keyup", function () {
      $("[id*=Multi_AmountTextBox]", $(this).closest("tr td")).val($.trim($(this).val()));
    });

    //Assign Late Fee to All
  $("[id*=AssinLFeeTextBox]").on("keyup", function () {
         $("[id*=Multi_LateFeeTextBox]", $(this).closest("tr td")).val($.trim($(this).val()));
   });

         //Concession Fee to All
            $("[id*=AssConcessionTextBox]").on("keyup", function () {
       $("[id*=Multi_DiscountTextBox]", $(this).closest("tr td")).val($.trim($(this).val()));
});
   });

     function SelectedItemCLR(a) {
         a.options[1].style.color = "rgb(255, 106, 0)";
     };

        //Select at least one Checkbox Students GridView
    function Validate(d, c) {
       var b = document.getElementById("<%=StudentsGridView.ClientID%>");
        var a = b.getElementsByTagName("input");
       for (i = 0; i < a.length; i++) {
  if (a[i].type == "checkbox" && a[i].checked) {
  return true;
  }
        }
            alert("You do not select any student from student list.");
  return false;
        }

     function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>