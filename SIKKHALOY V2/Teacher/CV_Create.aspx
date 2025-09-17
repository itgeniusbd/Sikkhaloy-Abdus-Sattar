<%@ Page Title="Create CV" Language="C#" MasterPageFile="~/Basic_Teacher.Master" AutoEventWireup="true" CodeBehind="CV_Create.aspx.cs" Inherits="EDUCATION.COM.Teacher.CV_Create" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Create CV</h3>
    <a href="Update_Info.aspx" target="_blank" class="btn btn-success">Update Personal Info</a>
    <hr />

    <div class="card mb-5">
        <div class="card-header">Add Career Objective</div>
        <div class="card-body">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="form-group">
                        <label>
                            Career Objective
                    <asp:RequiredFieldValidator ControlToValidate="CareerObjectiveTextBox" ValidationGroup="C" ID="RequiredFieldValidator11" runat="server" CssClass="EroorStar" ErrorMessage="*"></asp:RequiredFieldValidator></label>
                        <asp:TextBox placeholder="Career Objective" TextMode="MultiLine" Rows="3" CssClass="form-control" ID="CareerObjectiveTextBox" runat="server"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:Button ID="ObjectiveButton" ValidationGroup="C" runat="server" Text="Save" CssClass="btn btn-info" OnClick="ObjectiveButton_Click" />
                    </div>

                    <asp:GridView ID="ObjectiveGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="CareerObjectiveID" DataSourceID="ObjectiveSQL">
                        <Columns>
                            <asp:CommandField ShowEditButton="True" />
                            <asp:TemplateField HeaderText="Career Objective" SortExpression="CareerObjective">
                                <EditItemTemplate>
                                    <asp:TextBox ID="TextBox1" CssClass="form-control" Rows="3" runat="server" Text='<%# Bind("CareerObjective") %>'></asp:TextBox>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("CareerObjective") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:CommandField ShowDeleteButton="True" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="ObjectiveSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Teacher_Career_Objective] WHERE [CareerObjectiveID] = @CareerObjectiveID" InsertCommand="INSERT INTO [Teacher_Career_Objective] ([SchoolID], [TeacherID], [CareerObjective]) VALUES (@SchoolID, @TeacherID, @CareerObjective)" SelectCommand="SELECT CareerObjectiveID, SchoolID, TeacherID, CareerObjective FROM Teacher_Career_Objective WHERE (SchoolID = @SchoolID) AND (TeacherID = @TeacherID)" UpdateCommand="UPDATE Teacher_Career_Objective SET CareerObjective = @CareerObjective WHERE (CareerObjectiveID = @CareerObjectiveID)">
                        <DeleteParameters>
                            <asp:Parameter Name="CareerObjectiveID" Type="Int32" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
                            <asp:ControlParameter ControlID="CareerObjectiveTextBox" Name="CareerObjective" PropertyName="Text" Type="String" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="CareerObjective" Type="String" />
                            <asp:Parameter Name="CareerObjectiveID" Type="Int32" />
                        </UpdateParameters>
                    </asp:SqlDataSource>

                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <div class="card mb-5">
        <div class="card-header">Add Education Info</div>
        <div class="card-body">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:TextBox placeholder="Institution Name" CssClass="form-control" ID="InstitutionTextBox" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="InstitutionTextBox" ValidationGroup="E" ID="RequiredFieldValidator1" runat="server" CssClass="EroorStar" ErrorMessage="*"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox placeholder="Exam Name" CssClass="form-control" ID="ExamNameTextBox" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="ExamNameTextBox" ValidationGroup="E" ID="RequiredFieldValidator3" runat="server" CssClass="EroorStar" ErrorMessage="*"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox placeholder="Exam Year" CssClass="form-control" ID="ExamYearTextBox" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="ExamYearTextBox" ValidationGroup="E" ID="RequiredFieldValidator4" runat="server" CssClass="EroorStar" ErrorMessage="*"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox placeholder="Result" CssClass="form-control" ID="ResultTextBox" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="ResultTextBox" ValidationGroup="E" ID="RequiredFieldValidator2" runat="server" CssClass="EroorStar" ErrorMessage="*"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:Button ID="EducationButton" ValidationGroup="E" runat="server" Text="Save" CssClass="btn btn-info" OnClick="EducationButton_Click" />
                        </div>
                    </div>

                    <asp:GridView ID="EducationGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="TeacherEducationID" DataSourceID="EducationSQL">
                        <Columns>
                            <asp:CommandField ShowEditButton="True" />
                            <asp:BoundField DataField="InstitutionName" HeaderText="Institution Name" SortExpression="InstitutionName" />
                            <asp:BoundField DataField="ExamName" HeaderText="Exam Name" SortExpression="ExamName" />
                            <asp:BoundField DataField="ExamYear" HeaderText="Exam Year" SortExpression="ExamYear" />
                            <asp:BoundField DataField="Result" HeaderText="Result" SortExpression="Result" />
                            <asp:CommandField ShowDeleteButton="True" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="EducationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Teacher_EducationInfo] WHERE [TeacherEducationID] = @TeacherEducationID" InsertCommand="INSERT INTO [Teacher_EducationInfo] ([SchoolID], [TeacherID], [InstitutionName], [ExamName], [ExamYear], [Result]) VALUES (@SchoolID, @TeacherID, @InstitutionName, @ExamName, @ExamYear, @Result)" SelectCommand="SELECT TeacherEducationID, SchoolID, TeacherID, InstitutionName, ExamName, ExamYear, Result FROM Teacher_EducationInfo WHERE (SchoolID = @SchoolID) AND (TeacherID = @TeacherID)" UpdateCommand="UPDATE Teacher_EducationInfo SET InstitutionName = @InstitutionName, ExamName = @ExamName, ExamYear = @ExamYear, Result = @Result WHERE (TeacherEducationID = @TeacherEducationID)">
                        <DeleteParameters>
                            <asp:Parameter Name="TeacherEducationID" Type="Int32" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
                            <asp:ControlParameter ControlID="InstitutionTextBox" Name="InstitutionName" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="ExamNameTextBox" Name="ExamName" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="ExamYearTextBox" Name="ExamYear" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="ResultTextBox" Name="Result" PropertyName="Text" Type="String" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="InstitutionName" Type="String" />
                            <asp:Parameter Name="ExamName" Type="String" />
                            <asp:Parameter Name="ExamYear" Type="String" />
                            <asp:Parameter Name="Result" Type="String" />
                            <asp:Parameter Name="TeacherEducationID" Type="Int32" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <div class="card mb-5">
        <div class="card-header">Add Job Experience Info</div>
        <div class="card-body">
            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <ContentTemplate>
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:TextBox placeholder="Institution Name" CssClass="form-control" ID="JobInstitutionTextBox" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="JobInstitutionTextBox" ValidationGroup="J" ID="RequiredFieldValidator5" runat="server" CssClass="EroorStar" ErrorMessage="*"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox placeholder="Designation" CssClass="form-control" ID="DesignationTextBox" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="DesignationTextBox" ValidationGroup="J" ID="RequiredFieldValidator6" runat="server" CssClass="EroorStar" ErrorMessage="*"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox placeholder="Job Type" CssClass="form-control" ID="JobTypeTextBox" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="JobTypeTextBox" ValidationGroup="J" ID="RequiredFieldValidator7" runat="server" CssClass="EroorStar" ErrorMessage="*"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox placeholder="Year of job start and end" CssClass="form-control" ID="JobYearTextBox" runat="server"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <asp:TextBox placeholder="Address" CssClass="form-control" ID="AddressTextBox" runat="server"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <asp:TextBox placeholder="Phone" CssClass="form-control" ID="PhoneTextBox" runat="server"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <asp:TextBox placeholder="Email" CssClass="form-control" ID="EmailTextBox" runat="server"></asp:TextBox>
                        </div>

                        <div class="form-group">
                            <asp:Button ID="Job_Button" ValidationGroup="J" runat="server" Text="Save" CssClass="btn btn-info" OnClick="Job_Button_Click" />
                        </div>
                    </div>

                    <asp:GridView ID="JobGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="TeacherJobID" DataSourceID="JobSQL">
                        <Columns>
                            <asp:CommandField ShowEditButton="True" />
                            <asp:BoundField DataField="InstitutionName" HeaderText="Institution" SortExpression="InstitutionName" />
                            <asp:BoundField DataField="Position" HeaderText="Designation" SortExpression="Position" />
                            <asp:BoundField DataField="JobType" HeaderText="Job Type" SortExpression="JobType" />
                            <asp:BoundField DataField="JobYear" HeaderText="Job Year" SortExpression="JobYear" />
                            <asp:BoundField DataField="Address" HeaderText="Address" SortExpression="Address" />
                            <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                            <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                            <asp:CommandField ShowDeleteButton="True" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="JobSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Teacher_JobInfo] WHERE [TeacherJobID] = @TeacherJobID" InsertCommand="INSERT INTO Teacher_JobInfo(SchoolID, TeacherID, InstitutionName, Position, JobType, JobYear, Address, Phone, Email) VALUES (@SchoolID, @TeacherID, @InstitutionName, @Position, @JobType, @JobYear, @Address, @Phone, @Email)" SelectCommand="SELECT TeacherJobID, SchoolID, TeacherID, InstitutionName, Position, Responsibilitie, JobType, JobStatus, JobYear, Address, Phone, Email FROM Teacher_JobInfo WHERE (SchoolID = @SchoolID) AND (TeacherID = @TeacherID)" UpdateCommand="UPDATE Teacher_JobInfo SET InstitutionName = @InstitutionName, Position = @Position, JobType = @JobType, JobYear = @JobYear, Address = @Address, Phone = @Phone, Email = @Email WHERE (TeacherJobID = @TeacherJobID)">
                        <DeleteParameters>
                            <asp:Parameter Name="TeacherJobID" Type="Int32" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
                            <asp:ControlParameter ControlID="JobInstitutionTextBox" Name="InstitutionName" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="DesignationTextBox" Name="Position" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="JobTypeTextBox" Name="JobType" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="JobYearTextBox" Name="JobYear" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="AddressTextBox" Name="Address" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="PhoneTextBox" Name="Phone" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="EmailTextBox" Name="Email" PropertyName="Text" Type="String" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="InstitutionName" Type="String" />
                            <asp:Parameter Name="Position" Type="String" />
                            <asp:Parameter Name="JobType" Type="String" />
                            <asp:Parameter Name="JobYear" Type="String" />
                            <asp:Parameter Name="Address" Type="String" />
                            <asp:Parameter Name="Phone" Type="String" />
                            <asp:Parameter Name="Email" Type="String" />
                            <asp:Parameter Name="TeacherJobID" Type="Int32" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <div class="card mb-5">
        <div class="card-header">Add Skill</div>
        <div class="card-body">
            <asp:UpdatePanel ID="UpdatePanel4" runat="server">
                <ContentTemplate>
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:TextBox placeholder="Skill Name" CssClass="form-control" ID="SkillTextBox" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="SkillTextBox" ValidationGroup="S" ID="RequiredFieldValidator8" runat="server" CssClass="EroorStar" ErrorMessage="*"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox placeholder="Description" CssClass="form-control" ID="DescriptionTextBox" runat="server"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <asp:Button ID="SkillButton" ValidationGroup="S" runat="server" Text="Save" CssClass="btn btn-info" OnClick="SkillButton_Click" />
                        </div>
                    </div>

                    <asp:GridView ID="SkillGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="TeacherSkillID" DataSourceID="SkillSQL">
                        <Columns>
                            <asp:CommandField ShowEditButton="True" />
                            <asp:BoundField DataField="SkilName" HeaderText="Skil Name" SortExpression="SkilName" />
                            <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <asp:CommandField ShowDeleteButton="True" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="SkillSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Teacher_Skill] WHERE [TeacherSkillID] = @TeacherSkillID" InsertCommand="INSERT INTO [Teacher_Skill] ([SchoolID], [TeacherID], [SkilName], [Description]) VALUES (@SchoolID, @TeacherID, @SkilName, @Description)" SelectCommand="SELECT TeacherSkillID, SchoolID, TeacherID, SkilName, Description FROM Teacher_Skill WHERE (SchoolID = @SchoolID) AND (TeacherID = @TeacherID)" UpdateCommand="UPDATE Teacher_Skill SET SkilName = @SkilName, Description = @Description WHERE (TeacherSkillID = @TeacherSkillID)">
                        <DeleteParameters>
                            <asp:Parameter Name="TeacherSkillID" Type="Int32" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
                            <asp:ControlParameter ControlID="SkillTextBox" Name="SkilName" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="DescriptionTextBox" Name="Description" PropertyName="Text" Type="String" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="SkilName" Type="String" />
                            <asp:Parameter Name="Description" Type="String" />
                            <asp:Parameter Name="TeacherSkillID" Type="Int32" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <div class="card">
        <div class="card-header">Add Language</div>
        <div class="card-body">
            <asp:UpdatePanel ID="UpdatePanel5" runat="server">
                <ContentTemplate>
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:TextBox placeholder="Language Name" CssClass="form-control" ID="LanguageNameTextBox" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="LanguageNameTextBox" ValidationGroup="L" ID="RequiredFieldValidator9" runat="server" CssClass="EroorStar" ErrorMessage="*"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox placeholder="Level" CssClass="form-control" ID="LevelTextBox" runat="server"></asp:TextBox>
                            <asp:RequiredFieldValidator ControlToValidate="LevelTextBox" ValidationGroup="L" ID="RequiredFieldValidator10" runat="server" CssClass="EroorStar" ErrorMessage="*"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:Button ID="LanguageButton" ValidationGroup="L" runat="server" Text="Save" CssClass="btn btn-info" OnClick="LanguageButton_Click" />
                        </div>
                    </div>

                    <asp:GridView ID="LanguageGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="TeacherLanguageID" DataSourceID="LanguageSQL">
                        <Columns>
                            <asp:CommandField ShowEditButton="True" />
                            <asp:BoundField DataField="LanguageName" HeaderText="Language Name" SortExpression="LanguageName" />
                            <asp:BoundField DataField="Level" HeaderText="Level" SortExpression="Level" />
                            <asp:CommandField ShowDeleteButton="True" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="LanguageSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Teacher_Language] WHERE [TeacherLanguageID] = @TeacherLanguageID" InsertCommand="INSERT INTO [Teacher_Language] ([SchoolID], [TeacherID], [LanguageName], [Level]) VALUES (@SchoolID, @TeacherID, @LanguageName, @Level)" SelectCommand="SELECT * FROM [Teacher_Language] WHERE (([SchoolID] = @SchoolID) AND ([TeacherID] = @TeacherID))" UpdateCommand="UPDATE Teacher_Language SET LanguageName = @LanguageName, Level = @Level WHERE (TeacherLanguageID = @TeacherLanguageID)">
                        <DeleteParameters>
                            <asp:Parameter Name="TeacherLanguageID" Type="Int32" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
                            <asp:ControlParameter ControlID="LanguageNameTextBox" Name="LanguageName" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="LevelTextBox" Name="Level" PropertyName="Text" Type="String" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" Type="Int32" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="LanguageName" Type="String" />
                            <asp:Parameter Name="Level" Type="String" />
                            <asp:Parameter Name="TeacherLanguageID" Type="Int32" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                </ContentTemplate>
            </asp:UpdatePanel>
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
            $("#_5").addClass("active");
        });
    </script>
</asp:Content>
