<%@ Page Title="Send SMS" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Send_SMS.aspx.cs" Inherits="EDUCATION.COM.SMS.Send_SMS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .Counter_St { color: #00009b; font-size: 15px; font-weight: bold; }
        .ID_tb {height:38px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>
        <asp:FormView ID="SMSFormView" runat="server" DataKeyNames="SMSID" DataSourceID="SMSSQL">
            <ItemTemplate>
                Send SMS to Mobile (Remaining SMS:
                   <asp:Label ID="CountLabel" runat="server" Text='<%# Bind("SMS_Balance") %>' />)
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="SMSSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [SMS] WHERE ([SchoolID] = @SchoolID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </h3>

    <div class="mb-4">
        <asp:RadioButtonList ID="SelectRadioButtonList" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal" AutoPostBack="True" CssClass="Radio-button">
            <asp:ListItem Selected="True">Send SMS selected students</asp:ListItem>
            <asp:ListItem>Send SMS All students</asp:ListItem>
            <asp:ListItem>Send Single Or Multiple SMS</asp:ListItem>
            <asp:ListItem>Send SMS Teachers</asp:ListItem>
        </asp:RadioButtonList>
    </div>


    <asp:MultiView ID="SMSMultiView" runat="server">
        <asp:View ID="Selected_Students_View" runat="server">
            <div class="form-inline spacing">
                <div class="form-group">
                    <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
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
                    <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
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
                    <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="IDTextBox" runat="server" CssClass="form-control ID_tb" placeholder="Separate the ID by comma" TextMode="MultiLine"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="IDFindButton" runat="server" Text="Find" CssClass="btn btn-primary" OnClick="IDFindButton_Click" ValidationGroup="ID" />
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" ControlToValidate="IDTextBox" CssClass="EroorStar" ErrorMessage="Enter ID" ValidationGroup="ID"></asp:RequiredFieldValidator>
                </div>
                <asp:SqlDataSource ID="FindIDSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT StudentsClass.EducationYearID, StudentsClass.Date,StudentsClass.RollNo, Student.ID, Student.StudentsName, Student.SMSPhoneNo, Student.Gender, Student.DateofBirth,Student.BloodGroup, Student.Religion, Student.StudentID, CreateClass.Class FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (StudentsClass.EducationYearID = @Session) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.Status = 'Active') AND (Student.ID IN(SELECT  id from [dbo].[In_Function_Parameter] (@IDs)))">
                    <SelectParameters>
                        <asp:SessionParameter Name="Session" SessionField="Edu_Year" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="IDTextBox" Name="IDs" PropertyName="Text" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="AllStudentsGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="StudentID,SMSPhoneNo" CssClass="mGrid">
                    <Columns>
                        <asp:TemplateField HeaderText="Select">
                            <HeaderTemplate>
                                <asp:CheckBox ID="SelectAllCheckBox" runat="server" Text=" " />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SelectCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo" />
                        <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                        <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                        <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                        <asp:BoundField DataField="Religion" HeaderText="Religion" SortExpression="Religion" />
                        <asp:BoundField DataField="SMSPhoneNo" HeaderText="Phone Number" SortExpression="SMSPhoneNo" />

                    </Columns>
                    <EmptyDataTemplate>
                        Empty
                    </EmptyDataTemplate>
                    <PagerStyle CssClass="pgr"></PagerStyle>
                    <RowStyle CssClass="RowStyle" />
                </asp:GridView>
                <asp:SqlDataSource ID="AllStudentsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT StudentsClass.EducationYearID, StudentsClass.Date, Student.ID, Student.StudentsName, Student.SMSPhoneNo, Student.Gender, Student.DateofBirth, Student.BloodGroup, Student.Religion, Student.StudentID, Student.StudentEmailAddress, CreateClass.Class, StudentsClass.RollNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (StudentsClass.SectionID LIKE  @SectionID) AND (StudentsClass.SubjectGroupID LIKE  @SubjectGroupID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.ShiftID LIKE  @ShiftID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) ORDER BY StudentsClass.RollNo">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        <asp:Parameter DefaultValue="Active" Name="Status" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </asp:View>
         <asp:View ID="AllStudentView" runat="server"></asp:View>
        <asp:View ID="Single_SMS_View" runat="server">
            <div class="row">
                <div class="col-lg-4 col-sm-6">
                    <div class="form-group">
                        <asp:TextBox ID="SingleMobileNoTextBox" runat="server" CssClass="form-control"  placeholder="Enter Mobile Number"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ControlToValidate="SingleMobileNoTextBox" CssClass="EroorSummer" ErrorMessage="Enter mobile no." ValidationGroup="1"></asp:RequiredFieldValidator>
                        <%--<asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="SingleMobileNoTextBox" CssClass="EroorSummer" ErrorMessage="Invalid Mobile number" ValidationExpression="^(?=\d+ \d+#)[\d ]{12}#\d{5}|\d{11}|\d{1,12}#\d{1,5}$" ValidationGroup="1"></asp:RegularExpressionValidator>--%>
                    </div>
                </div>
            </div>
        </asp:View>

        <asp:View ID="Teachers_View" runat="server">
            <div class="table-responsive">
                <asp:GridView ID="AllTeachersGridView" runat="server" AutoGenerateColumns="False" DataSourceID="AllTeachersSQL" CssClass="mGrid" DataKeyNames="EmployeeID,Phone" PageSize="100">
                    <Columns>
                        <asp:TemplateField HeaderText="Select">
                            <HeaderTemplate>
                                <asp:CheckBox ID="SelectAllCheckBox" runat="server" Text=" " />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SelectCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                        <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
                        <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                    </Columns>
                    <EmptyDataTemplate>
                        Empty
                    </EmptyDataTemplate>
                    <PagerStyle CssClass="pgr"></PagerStyle>
                    <RowStyle CssClass="RowStyle" />
                </asp:GridView>
                <asp:SqlDataSource ID="AllTeachersSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EmployeeID, ID,  FirstName +' '+ LastName as Name, Designation, Phone FROM VW_Emp_Info WHERE (SchoolID = @SchoolID) AND (Job_Status = N'Active')">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </asp:View>
    </asp:MultiView>

    <div class="row">
        <div class="col-lg-4 col-sm-6">
            <div class="form-group">
                <span style="color: #ff0000">N.B: Don't copy sms text from ms word/excel file</span>
                <asp:TextBox ID="SMSTextBox" runat="server" CssClass="form-control" Height="90px" TextMode="MultiLine" onfocus="empty();" onkeyup="countChar();" placeholder="SMS Text Write here"></asp:TextBox>
                <div id="sms-counter" class="Counter_St">
                    Length: <span class="length"></span>/ <span class="per_message"></span>.  
                      Count: <span class="messages"></span>
                    SMS
                </div>

                <asp:Button ID="SMSButton" runat="server" Text="Send SMS" ValidationGroup="1" CssClass="btn btn-primary" OnClick="SMSButton_Click" />
                <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="SMSTextBox" CssClass="EroorSummer" ErrorMessage="Write SMS" ValidationGroup="1"></asp:RequiredFieldValidator>
                <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
            </div>
            <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
                <InsertParameters>
                    <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
                    <asp:Parameter Name="SchoolID" />
                    <asp:Parameter Name="StudentID" />
                    <asp:Parameter Name="TeacherID" />
                    <asp:Parameter Name="EducationYearID" />
                </InsertParameters>
            </asp:SqlDataSource>
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

    <script src="/JS/SMSCount/sms_counter.min.js"></script>
    <script type="text/javascript">
        $(function () {
            $('[id*=SMSTextBox]').countSms('#sms-counter');

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

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
