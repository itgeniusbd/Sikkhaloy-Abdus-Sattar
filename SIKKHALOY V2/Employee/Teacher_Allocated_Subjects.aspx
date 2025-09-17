<%@ Page Title="Subjects Allocated For Teacher" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Teacher_Allocated_Subjects.aspx.cs" Inherits="EDUCATION.COM.Employee.Teacher_Allocated_Subjects" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid th { border: none; background-color: #fff; color: #333; text-align: left;}
        .mGrid td { padding:.5rem 0.2rem; border: 1px solid #dee2e6; border-right: none;}
        .added { color:#09aa33;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Subjects Allocated For Teacher (Add/Modify)</h3>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="TeacherDropDownList" runat="server" CssClass="form-control" DataSourceID="TeacherSQL" DataTextField="Name" DataValueField="TeacherID" AppendDataBoundItems="True" AutoPostBack="True">
                        <asp:ListItem Value="0">[ SELECT TEACHER ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="TeacherSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Teacher.TeacherID, Teacher.FirstName + ' ' + Teacher.LastName AS Name FROM Teacher INNER JOIN Employee_Info ON Teacher.EmployeeID = Employee_Info.EmployeeID WHERE (Teacher.SchoolID = @SchoolID) AND (Employee_Info.Job_Status = N'Active')">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="TeacherDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="AS"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="AS"></asp:RequiredFieldValidator>
                </div>
            </div>

            <div class="table-responsive mb-3">
                <asp:GridView ID="SubjectGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="SubjectID" DataSourceID="SubjectSQL">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <em>Checked to add subject and unchecked to remove subject</em>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SubjectCheckBox" Checked='<%#Bind("IS_Exsit") %>' runat="server" Text='<%# Eval("SubjectName") %>' AutoPostBack="True" OnCheckedChanged="SubjectCheckBox_CheckedChanged" />
                            </ItemTemplate>
                            <ItemStyle Width="50px" HorizontalAlign="Left" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="SubjectSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Subject.SubjectID, Subject.SubjectName, Subject.SN, CAST(CASE WHEN T_S.SubjectID IS NULL THEN 0 ELSE 1 END AS BIT) AS IS_Exsit FROM Subject INNER JOIN SubjectForGroup ON Subject.SubjectID = SubjectForGroup.SubjectID LEFT OUTER JOIN (SELECT SubjectID FROM TecherSubject WHERE (SchoolID = @SchoolID) AND (TeacherID = @TeacherID) AND (ClassID = @ClassID)) AS T_S ON Subject.SubjectID = T_S.SubjectID WHERE (Subject.SchoolID = @SchoolID) AND (SubjectForGroup.ClassID = @ClassID) ORDER BY Subject.SN, Subject.SubjectName">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="TeacherDropDownList" Name="TeacherID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="TeacherSubjectSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO TecherSubject(SchoolID, RegistrationID, TeacherID, SubjectID, date, ClassID) VALUES (@SchoolID, @RegistrationID, @TeacherID, @SubjectID, GETDATE(), @ClassID)" SelectCommand="SELECT * FROM [TecherSubject]" DeleteCommand="DELETE FROM TecherSubject WHERE (SchoolID = @SchoolID) AND (TeacherID = @TeacherID) AND (ClassID = @ClassID) AND (SubjectID = @SubjectID)">
                    <DeleteParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="TeacherDropDownList" Name="TeacherID" PropertyName="SelectedValue" />
                        <asp:Parameter Name="SubjectID" />
                    </DeleteParameters>
                    <InsertParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="TeacherDropDownList" Name="TeacherID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:Parameter Name="SubjectID" Type="Int32" />
                    </InsertParameters>
                </asp:SqlDataSource>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

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

    <script>
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            if ($('[id*=SubjectGridView] tr').length) {
                $("[id*=SubjectCheckBox]").each(function () {
                    $(this).is(":checked") ? ($("td", $(this).closest("tr")).append('<em class="added">Added <i class="fa fa-check"></i></em>')) : ($("td", $(this).closest("tr")));
                });
            }
        })
    </script>
</asp:Content>
