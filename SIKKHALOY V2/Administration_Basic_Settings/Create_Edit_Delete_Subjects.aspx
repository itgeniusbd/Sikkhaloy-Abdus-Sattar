<%@ Page Title="Add All Subjects" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Create_Edit_Delete_Subjects.aspx.cs" Inherits="EDUCATION.COM.ADMINISTRATION_BASIC_SETTING.Create_Edit_Delete_Subjects" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <h3>Add All Subjects of Institution</h3>

            <div class="form-inline">
                <div class="form-group">
                    <asp:TextBox ID="SubjectNameTextBox" runat="server" CssClass="form-control" placeholder="Subject Name"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="SubjectNameTextBox" CssClass="EroorSummer" ErrorMessage="Enter Subject Name" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:Button ID="SubjectButton" runat="server" CssClass="btn btn-primary" Text="Add Subject" OnClick="AddSubjectButton_Click" ValidationGroup="1" />
                </div>
                <div class="form-group ml-auto">
                    <a href="Assigning_subject_in_classes.aspx">Assign Subject In Class</a>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="SubjectGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="SubjectID" DataSourceID="SubjectSQL" CssClass="mGrid" PageSize="30">
                    <Columns>
                        <asp:TemplateField ShowHeader="False" HeaderText="Delete">
                            <ItemTemplate>
                                <asp:LinkButton ID="lButton1" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are You Sure want to delete permanently this Subject? ')" />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" Width="50px" />
                        </asp:TemplateField>

                        <asp:CommandField ShowEditButton="True" HeaderText="Edit">
                            <ItemStyle HorizontalAlign="Center" Width="50px" />
                        </asp:CommandField>
                        <asp:TemplateField HeaderText="Academic Subject List" SortExpression="SubjectName">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("SubjectName") %>' CssClass="form-control"></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Bind("SubjectName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Subject Serial">
                            <ItemTemplate>
                                <asp:TextBox ID="SN_TextBox" Text='<%# Bind("SN") %>' runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)"></asp:TextBox>
                            </ItemTemplate>
                            <ItemStyle Width="100px" />
                        </asp:TemplateField>
                    </Columns>

                    <EmptyDataTemplate>
                        Empty
                    </EmptyDataTemplate>

                    <PagerStyle CssClass="pgr" />
                    <SelectedRowStyle CssClass="Selected" />
                </asp:GridView>
                <asp:SqlDataSource ID="SubjectSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="IF NOT EXISTS (SELECT * FROM StudentRecord WHERE [SubjectID] = @SubjectID AND SchoolID = @SchoolID)
DELETE FROM [Subject] WHERE [SubjectID] = @SubjectID"
                    SelectCommand="SELECT SubjectID, SchoolID, RegistrationID, SubjectName, SN FROM Subject WHERE (SchoolID = @SchoolID) ORDER BY ISNULL(SN, 9999), SubjectName" UpdateCommand="IF NOT EXISTS (SELECT * FROM Subject WHERE SubjectName = @SubjectName AND SchoolID = @SchoolID)
UPDATE [Subject] SET [SubjectName] = @SubjectName WHERE [SubjectID] = @SubjectID"
                    InsertCommand="IF NOT EXISTS (SELECT * FROM Subject WHERE SubjectName = @SubjectName AND SchoolID = @SchoolID)
INSERT INTO Subject(SubjectName, Date, RegistrationID, SchoolID) VALUES (@SubjectName, GETDATE(), @RegistrationID, @SchoolID)">
                    <DeleteParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:Parameter Name="SubjectID" Type="Int32" />
                    </DeleteParameters>

                    <InsertParameters>
                        <asp:ControlParameter ControlID="SubjectNameTextBox" Name="SubjectName" PropertyName="Text" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    </InsertParameters>
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>

                    <UpdateParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:Parameter Name="SubjectID" Type="Int32" />
                        <asp:Parameter Name="SubjectName" Type="String" />
                    </UpdateParameters>
                </asp:SqlDataSource>

                <div class="form-group pull-right">
                    <asp:SqlDataSource ID="SNSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Subject]" UpdateCommand="UPDATE Subject SET SN = @SN WHERE (SchoolID = @SchoolID) AND (SubjectID = @SubjectID)">
                        <UpdateParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:Parameter Name="SubjectID" />
                            <asp:Parameter Name="SN" Type="Int32" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                    <asp:Button ID="SN_Button" runat="server" Text="Update Subject Serial" CssClass="btn btn-success" OnClick="SN_Button_Click" />
                </div>
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
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
