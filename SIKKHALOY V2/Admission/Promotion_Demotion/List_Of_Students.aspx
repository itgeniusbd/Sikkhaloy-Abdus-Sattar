<%@ Page Title="Change Class" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="List_Of_Students.aspx.cs" Inherits="EDUCATION.COM.Admission.Promotion_Demotion.List_Of_Students" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../CSS/Student_List.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <h3>Change Class</h3>
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
            </div>

            <div class="table-responsive">
                <label id="CountStudent"></label>
                <asp:GridView ID="StudentsGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid"
                    DataKeyNames="StudentClassID,StudentID,EducationYearID">
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
                        <asp:HyperLinkField DataNavigateUrlFields="StudentID,StudentClassID"
                            DataNavigateUrlFormatString="Change_Class_And_Subjects.aspx?Student={0}&Old_Class={1}"
                            DataTextField="StudentsName" HeaderText="Select Name" />
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                        <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
                        <asp:BoundField DataField="FatherPhoneNumber" HeaderText="F.Mobile" SortExpression="FatherPhoneNumber" />
                        <asp:BoundField DataField="MotherPhoneNumber" HeaderText="M.Mobile" SortExpression="MotherPhoneNumber" />
                        <asp:BoundField DataField="GuardianPhoneNumber" HeaderText="G.Mobile" SortExpression="GuardianPhoneNumber" />
                    </Columns>
                    <PagerStyle CssClass="pgr" />
                    <SelectedRowStyle CssClass="Selected" />
                </asp:GridView>

                <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT Student.StudentID, Student.SMSPhoneNo, Student.StudentsName, Student.Gender, Student.StudentsLocalAddress, Student.MothersName, Student.FathersName, Student.FatherPhoneNumber, Student.GuardianName, StudentsClass.RollNo, Student.ID, Student.SMSPhoneNo AS Expr1, Student.MotherPhoneNumber, Student.FatherOccupation, Student.GuardianPhoneNumber, StudentsClass.StudentClassID, StudentsClass.EducationYearID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) AND (StudentsClass.Class_Status IS NULL) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END">
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
            </div>
            <br />
            <%if (StudentsGridView.Rows.Count > 0)
                { %>
            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:DropDownList ID="Re_ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="Re_ClassSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="Re_ClassDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Re_ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="Re_GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="Re_GroupSQL" DataTextField="SubjectGroup"
                        DataValueField="SubjectGroupID" OnDataBound="Re_GroupDropDownList_DataBound" OnSelectedIndexChanged="Re_GroupDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Re_GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="Re_ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="Re_SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="Re_SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="Re_SectionDropDownList_DataBound" OnSelectedIndexChanged="Re_SectionDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Re_SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="Re_ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="Re_ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="Re_ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="Re_ShiftDropDownList_DataBound" OnSelectedIndexChanged="Re_ShiftDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Re_ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="Re_ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

            </div>
            <%} %>
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
                        <asp:ControlParameter ControlID="Re_ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <asp:SqlDataSource ID="UpdateStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [StudentsClass] WHERE ([EducationYearID] = @EducationYearID)" UpdateCommand="UPDATE StudentsClass SET SectionID = @SectionID, ShiftID = @ShiftID, SubjectGroupID = @SubjectGroupID WHERE (StudentClassID = @StudentClassID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                    <UpdateParameters>
                        <asp:Parameter Name="SectionID" />
                        <asp:Parameter Name="ShiftID" />
                        <asp:Parameter Name="SubjectGroupID" />
                        <asp:Parameter Name="StudentClassID" />
                    </UpdateParameters>
                </asp:SqlDataSource>
            </div>

            <%if (StudentsGridView.Rows.Count > 0)
                { %>
            <div class="form-inline">
                <asp:CheckBox ID="KeepPayOrderCheckbox" runat="server" Text="KEAP PAY ORDER" />
                <asp:RadioButtonList ID="PDRadioButtonList" runat="server" RepeatDirection="Horizontal">
                    <asp:ListItem>Promotion</asp:ListItem>
                    <asp:ListItem>Demotion</asp:ListItem>
                </asp:RadioButtonList>

                <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Change Class" ValidationGroup="1" />
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="PDRadioButtonList" CssClass="EroorSummer" ErrorMessage="Choose an option" ValidationGroup="1"></asp:RequiredFieldValidator>

                <asp:Label ID="ErrorLabel" runat="server" ForeColor="Red"></asp:Label>
            </div>

            <asp:SqlDataSource ID="StudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS (SELECT * FROM  StudentsClass WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID))
BEGIN
INSERT INTO [StudentsClass] ([SchoolID], [RegistrationID], [StudentID], [ClassID], [SectionID], [ShiftID], [SubjectGroupID], [RollNo], [EducationYearID], [Date]) VALUES (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @ShiftID, @SubjectGroupID, @RollNo, @EducationYearID, Getdate())
END" SelectCommand="SELECT * FROM StudentsClass " UpdateCommand="UPDATE StudentsClass SET EducationYearID = @EducationYearID, New_StudentClassID = @New_StudentClassID, Promotion_Demotion_Year = @Promotion_Demotion_Year, Class_Status = @Class_Status WHERE (StudentClassID = @StudentClassID)">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:Parameter Name="StudentID" Type="Int32" />
                    <asp:ControlParameter ControlID="Re_ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:SessionParameter Name="SectionID" SessionField="SectionID" Type="String" />
                    <asp:SessionParameter Name="ShiftID" SessionField="ShiftID" Type="String" />
                    <asp:SessionParameter Name="SubjectGroupID" SessionField="GroupID" Type="String" />
                    <asp:Parameter Name="RollNo" Type="String" />
                </InsertParameters>
                <UpdateParameters>
                    <asp:Parameter DefaultValue="0" Name="EducationYearID" />
                    <asp:SessionParameter DefaultValue="" Name="New_StudentClassID" SessionField="StudentClassID" />
                    <asp:SessionParameter DefaultValue="" Name="Promotion_Demotion_Year" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="PDRadioButtonList" DefaultValue="" Name="Class_Status" PropertyName="SelectedValue" />
                    <asp:Parameter Name="StudentClassID" />
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
                    <%--<asp:SessionParameter Name="StudentClassID" SessionField="StudentClassLastID" />--%>
                    <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Old_Class" />
                </DeleteParameters>
                <UpdateParameters>
                    <asp:SessionParameter Name="New_StudentClassID" SessionField="StudentClassID" />
                    <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Old_Class" />
                </UpdateParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="UpdateIncomePayorder" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" UpdateCommand="Update Income_PayOrder Set StudentClassID=@StudentClassID,ClassID=@ClassID WHERE (SchoolID=@SchoolID and EducationYearID=@EducationYearID and StudentID=@StudentID)">

                <UpdateParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:SessionParameter Name="StudentID" SessionField="studentId" />
                    <asp:SessionParameter Name="ClassID" SessionField="NewClassID" />
                    <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassLastID" />
                </UpdateParameters>
            </asp:SqlDataSource>

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
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            if ($("[id*=StudentsGridView] tr").length) {
                $("#CountStudent").text("TOTAL: " + $("[id*=StudentsGridView] td").closest("tr").length + " STUDENT.");
            }
            $("[id*=AllIteamCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllIteamCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SingleCheckBox]", a).length == $("[id*=SingleCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });
    </script>
</asp:Content>
