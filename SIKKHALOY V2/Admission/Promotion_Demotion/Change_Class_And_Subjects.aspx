<%@ Page Title="Promotion/Demotion" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Change_Class_And_Subjects.aspx.cs" Inherits="EDUCATION.COM.Admission.Promotion_Demotion.Change_Class_And_Subjects" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../CSS/Student_List.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <h3>Student Promotion/Demotion (Class & Subjects)</h3>

            <asp:FormView ID="StudentInfoFormView" runat="server" DataKeyNames="StudentID,ClassID" DataSourceID="StudentInfoSQL" Width="100%">
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
                                </ul>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CreateClass.Class, CreateSection.Section, CreateSubjectGroup.SubjectGroup, Student.StudentsName, Student.FathersName, Student.ID, Student.SMSPhoneNo, Student.StudentsName, Student.FathersName, CreateShift.Shift, StudentsClass.RollNo, Student.StudentID, Student.StudentImageID, Education_Year.EducationYear, CreateClass.ClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.StudentID = @StudentID) AND (StudentsClass.Class_Status IS NULL)">
                <SelectParameters>
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
                </SelectParameters>
            </asp:SqlDataSource>

            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged" OnDataBound="ClassDropDownList_DataBound">
                        <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) AND ClassID &lt;&gt; @ClassID ORDER BY SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="StudentInfoFormView" Name="ClassID" PropertyName="DataKey['ClassID']" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorStar" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="GroupDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE N'%' + @SectionID + N'%')">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                </div>
                <div class="form-group">
                    <asp:DropDownList ID="SectionDropDownList" runat="server" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" AutoPostBack="True" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].ShiftID LIKE N'%' + @ShiftID + N'%')">
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
                    <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].SectionID LIKE N'%' + @SectionID + N'%') AND ([Join].ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="RollNumberTextBox" placeholder="Roll Number" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="GroupGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="SubjectID,SubjectType" DataSourceID="SubjectGroupSQL" PagerStyle-CssClass="pgr" PageSize="20">
                    <Columns>
                        <asp:BoundField DataField="SubjectName" HeaderText="Subjects" SortExpression="SubjectName">
                            <HeaderStyle HorizontalAlign="Left" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Select" ShowHeader="False">
                            <ItemTemplate>
                                <asp:CheckBox ID="SubjectCheckBox" runat="server" Text=" " Checked='<%# Bind("Check") %>' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" Width="30px" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="This Subject Is">
                            <ItemTemplate>
                                <asp:RadioButtonList ID="SubjectTypeRadioButtonList" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("SubjectType") %>'>
                                    <asp:ListItem>Compulsory</asp:ListItem>
                                    <asp:ListItem>Optional</asp:ListItem>
                                </asp:RadioButtonList>
                            </ItemTemplate>
                            <ItemStyle Width="175px" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="SubjectGroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Subject.SubjectID, Subject.SubjectName, SubjectForGroup.SubjectType, CAST((CASE WHEN SubjectForGroup.SubjectType = 'Compulsory' THEN 1 ELSE 0 END)AS BIT) AS [Check] FROM Subject INNER JOIN SubjectForGroup ON Subject.SubjectID = SubjectForGroup.SubjectID WHERE (Subject.SchoolID = @SchoolID) AND (SubjectForGroup.ClassID = @ClassID) AND (SubjectForGroup.SubjectGroupID = (CASE WHEN @SubjectGroupID = '%' THEN 0 ELSE @SubjectGroupID END)) ORDER BY SubjectForGroup.SubjectType, Subject.SubjectName">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="form-inline">
                <asp:RadioButtonList ID="PDRadioButtonList" runat="server" RepeatDirection="Horizontal">
                    <asp:ListItem>Promotion</asp:ListItem>
                    <asp:ListItem>Demotion</asp:ListItem>
                </asp:RadioButtonList>

                <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Save &amp; Continue" ValidationGroup="1" />
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="PDRadioButtonList" CssClass="EroorSummer" ErrorMessage="Choose an option" ValidationGroup="1"></asp:RequiredFieldValidator>
            </div>

            <asp:SqlDataSource ID="StudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [StudentsClass] ([SchoolID], [RegistrationID], [StudentID], [ClassID], [SectionID], [ShiftID], [SubjectGroupID], [RollNo], [EducationYearID], [Date]) VALUES (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @ShiftID, @SubjectGroupID, @RollNo, @EducationYearID, Getdate())" SelectCommand="SELECT * FROM StudentsClass " UpdateCommand="UPDATE StudentsClass SET EducationYearID = @EducationYearID, New_StudentClassID = @New_StudentClassID, Promotion_Demotion_Year = @Promotion_Demotion_Year, Class_Status = @Class_Status WHERE (StudentClassID = @StudentClassID)">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" Type="Int32" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:SessionParameter Name="SectionID" SessionField="SectionID" Type="String" />
                    <asp:SessionParameter Name="ShiftID" SessionField="ShiftID" Type="String" />
                    <asp:SessionParameter Name="SubjectGroupID" SessionField="GroupID" Type="String" />
                    <asp:ControlParameter ControlID="RollNumberTextBox" Name="RollNo" PropertyName="Text" Type="String" />
                </InsertParameters>
                <UpdateParameters>
                    <asp:Parameter DefaultValue="0" Name="EducationYearID" />
                    <asp:SessionParameter DefaultValue="" Name="New_StudentClassID" SessionField="StudentClassID" />
                    <asp:SessionParameter DefaultValue="" Name="Promotion_Demotion_Year" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="PDRadioButtonList" DefaultValue="" Name="Class_Status" PropertyName="SelectedValue" />
                    <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Old_Class" />
                </UpdateParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="StudentRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                InsertCommand="INSERT INTO StudentRecord(SchoolID, RegistrationID, StudentID, StudentClassID, SubjectID, EducationYearID, Date, SubjectType) VALUES (@SchoolID, @RegistrationID, @StudentID, @StudentClassID, @SubjectID, @EducationYearID, GETDATE(), @SubjectType)" SelectCommand="SELECT * FROM [StudentRecord]">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
                    <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" Type="Int32" />
                    <asp:Parameter Name="SubjectID" Type="Int32" />
                    <asp:Parameter Name="SubjectType" />
                </InsertParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="UpdatePaymantSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM Income_PayOrder WHERE (SchoolID = @SchoolID) AND (StudentClassID = @StudentClassID) AND (PaidAmount = 0)" SelectCommand="SELECT * FROM Income_PayOrder" UpdateCommand="UPDATE Income_Discount_Record SET StudentClassID = @New_StudentClassID  WHERE  (StudentClassID = @StudentClassID)
UPDATE Income_LateFee_Change_Record SET StudentClassID = @New_StudentClassID WHERE (StudentClassID =@StudentClassID)
UPDATE Income_LateFee_Discount_Record SET StudentClassID = @New_StudentClassID WHERE (StudentClassID = @StudentClassID)
UPDATE Income_MoneyReceipt SET StudentClassID = @New_StudentClassID WHERE  (StudentClassID = @StudentClassID)
UPDATE Income_PaymentRecord SET StudentClassID = @New_StudentClassID WHERE  (StudentClassID = @StudentClassID)
UPDATE Income_PayOrder SET StudentClassID = @New_StudentClassID WHERE (StudentClassID = @StudentClassID)">
                <DeleteParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Old_Class" />                    
                </DeleteParameters>
                <UpdateParameters>
                    <asp:SessionParameter Name="New_StudentClassID" SessionField="StudentClassID" />
                    <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Old_Class" />
                </UpdateParameters>
            </asp:SqlDataSource>
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

    <script>
        function noBack() {
            window.history.forward();
        }
        noBack();
        window.onload = noBack;
        window.onpageshow = function (evt) {
            if (evt.persisted) noBack();
        }
        window.onunload = function () { void (0); }

        function DisableButton() {
            document.getElementById("<%=SubmitButton.ClientID %>").disabled = true;
        }
        window.onbeforeunload = DisableButton;
    </script>
</asp:Content>
