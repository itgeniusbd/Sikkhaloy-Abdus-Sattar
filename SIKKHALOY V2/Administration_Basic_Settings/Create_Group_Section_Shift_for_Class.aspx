<%@ Page Title="Add Class/Group/Section/Shift" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Create_Group_Section_Shift_for_Class.aspx.cs" Inherits="EDUCATION.COM.ADMINISTRATION_BASICSETTING.Create_Group_Section_Shift_for_Class" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .form-inline .btn { padding: .84rem 2.14rem; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Add Group/Section/Shift for Class
        <label id="Dis_Cls" class="alert-success"></label>
        <asp:Label ID="QMsgLabel" runat="server" CssClass="EroorSummer"></asp:Label>
    </h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID">
                <asp:ListItem Value="0">[ SELECT CLASS]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
            <asp:Label ID="ClErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
        </div>
        <div class="form-group">
            <a style="padding: 0.55rem 2.14rem;" data-toggle="modal" data-target="#myModal" role="button" class="btn btn-elegant waves-effect waves-light">Add New Class <i class="fa fa-plus-circle ml-2"></i></a>
        </div>
    </div>

    <div class="card my-4">
        <div class="card-body">
            <div class="form-inline mb-3">
                <div class="md-form">
                    <asp:TextBox ID="SubjectGroupTextBox" placeholder="Enter Group Name" runat="server" CssClass="form-control EnableText"></asp:TextBox>
                </div>
                <asp:Button ID="GroupButton" runat="server" OnClick="GroupButton_Click" Text="Add Group" CssClass="btn btn-primary EnableSubmit" ValidationGroup="G" />
                <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="SubjectGroupTextBox" CssClass="EroorSummer" ErrorMessage="Enter Group" ValidationGroup="G"></asp:RequiredFieldValidator>
            </div>

            <asp:GridView ID="GroupGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="SubjectGroupID" DataSourceID="SubjectGroupSQL" CssClass="mGrid">
                <Columns>
                    <asp:TemplateField HeaderText="Group" SortExpression="SubjectGroup">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("SubjectGroup") %>' CssClass="textbox"></asp:TextBox>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("SubjectGroup") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowEditButton="True">
                        <ItemStyle HorizontalAlign="Center" Width="50px" />
                    </asp:CommandField>
                    <asp:TemplateField HeaderText="Delete">
                        <ItemTemplate>
                            <asp:LinkButton ID="DImB" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('This Data Will be Deleted Permanently From Everywhere \n Are You sure want to delete?')" />
                        </ItemTemplate>
                        <ItemStyle Width="60px" />
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="SubjectGroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                DeleteCommand="UPDATE StudentsClass SET SubjectGroupID = 0 WHERE  (SubjectGroupID = @SubjectGroupID)
DELETE FROM [CreateSubjectGroup] WHERE ( [SubjectGroupID] = @SubjectGroupID)
DELETE FROM [Join] WHERE (SubjectGroupID = @SubjectGroupID)
DELETE FROM SubjectForGroup WHERE  (ClassID = @ClassID) AND (SubjectGroupID = @SubjectGroupID)"
                InsertCommand="INSERT INTO [CreateSubjectGroup] ([RegistrationID], [SchoolID], [ClassID], [SubjectGroup]) VALUES (@RegistrationID, @SchoolID, @ClassID, @SubjectGroup)" SelectCommand="SELECT SubjectGroupID, RegistrationID, SchoolID, ClassID, SubjectGroup FROM CreateSubjectGroup WHERE (ClassID = @ClassID) AND (SchoolID = @SchoolID)" UpdateCommand="UPDATE [CreateSubjectGroup] SET [SubjectGroup] = @SubjectGroup WHERE [SubjectGroupID] = @SubjectGroupID">
                <DeleteParameters>
                    <asp:Parameter Name="SubjectGroupID" Type="Int32" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:ControlParameter ControlID="SubjectGroupTextBox" Name="SubjectGroup" PropertyName="Text" Type="String" />
                </InsertParameters>
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
                <UpdateParameters>
                    <asp:Parameter Name="SubjectGroup" Type="String" />
                    <asp:Parameter Name="SubjectGroupID" Type="Int32" />
                </UpdateParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="GroupJoinSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Join] WHERE [JoinID] = @JoinID" InsertCommand="INSERT INTO [Join] (RegistrationID, SchoolID, ClassID, SectionID, SubjectGroupID, ShiftID) VALUES (@RegistrationID, @SchoolID, @ClassID, @SectionID, @SubjectGroupID, @ShiftID)" SelectCommand="SELECT * FROM [Join]" UpdateCommand="UPDATE [Join] SET RegistrationID = @RegistrationID, SchoolID = @SchoolID, ClassID = @ClassID, SectionID = @SectionID, SubjectGroupID = @SubjectGroupID WHERE (JoinID = @JoinID)">
                <DeleteParameters>
                    <asp:Parameter Name="JoinID" Type="Int32" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:Parameter DefaultValue="0" Name="SectionID" Type="String" />
                    <asp:Parameter DefaultValue="" Name="SubjectGroupID" Type="String" />
                    <asp:Parameter DefaultValue="0" Name="ShiftID" />
                </InsertParameters>
                <UpdateParameters>
                    <asp:Parameter Name="RegistrationID" Type="Int32" />
                    <asp:Parameter Name="SchoolID" Type="Int32" />
                    <asp:Parameter Name="ClassID" Type="Int32" />
                    <asp:Parameter Name="SectionID" Type="String" />
                    <asp:Parameter Name="SubjectGroupID" Type="String" />
                    <asp:Parameter Name="JoinID" Type="Int32" />
                </UpdateParameters>
            </asp:SqlDataSource>
        </div>
    </div>

    <div class="card my-4">
        <div class="card-body">
            <div class="form-inline mb-3">
                <div class="md-form">
                    <asp:TextBox ID="SectionTextBox" runat="server" placeholder="Enter Section Name" CssClass="form-control EnableText"></asp:TextBox>
                </div>
                <asp:Button ID="SectionButton" runat="server" OnClick="SectionButton_Click" Text="Add Section" CssClass="btn btn-primary EnableSubmit" ValidationGroup="Se" />
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="SectionTextBox" CssClass="EroorSummer" ErrorMessage="Enter Section" ValidationGroup="Se"></asp:RequiredFieldValidator>
            </div>

            <asp:GridView ID="SectionGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="SectionID" DataSourceID="SectionSQL" CssClass="mGrid">
                <Columns>
                    <asp:TemplateField HeaderText="Section" SortExpression="Section">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Section") %>' CssClass="textbox"></asp:TextBox>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("Section") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowEditButton="True">
                        <ItemStyle HorizontalAlign="Center" Width="50px" />
                    </asp:CommandField>
                    <asp:TemplateField HeaderText="Delete">
                        <ItemTemplate>
                            <asp:LinkButton ID="DImB" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('This Data Will be Deleted Permanently From Everywhere \n Are You sure want to delete?')" />
                        </ItemTemplate>
                        <ItemStyle Width="60px" />
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="UPDATE StudentsClass SET SectionID = 0 WHERE  (SectionID = @SectionID)
DELETE FROM [CreateSection] WHERE [SectionID] = @SectionID
DELETE FROM [Join] WHERE (SectionID = @SectionID)"
                InsertCommand="INSERT INTO [CreateSection] ([SchoolID], [RegistrationID], [ClassID], [Section]) VALUES (@SchoolID, @RegistrationID, @ClassID, @Section)" SelectCommand="SELECT * FROM [CreateSection] WHERE (ClassID = @ClassID) AND (SchoolID = @SchoolID)" UpdateCommand="UPDATE [CreateSection] SET [Section] = @Section WHERE [SectionID] = @SectionID">
                <DeleteParameters>
                    <asp:Parameter Name="SectionID" Type="Int32" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:ControlParameter ControlID="SectionTextBox" Name="Section" PropertyName="Text" Type="String" />
                </InsertParameters>
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
                <UpdateParameters>
                    <asp:Parameter Name="Section" Type="String" />
                    <asp:Parameter Name="SectionID" Type="Int32" />
                </UpdateParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="SectionJoinSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Join] WHERE [JoinID] = @JoinID" InsertCommand="INSERT INTO [Join] (RegistrationID, SchoolID, ClassID, SectionID, SubjectGroupID, ShiftID) VALUES (@RegistrationID, @SchoolID, @ClassID, @SectionID, @SubjectGroupID, @ShiftID)" SelectCommand="SELECT * FROM [Join]" UpdateCommand="UPDATE [Join] SET RegistrationID = @RegistrationID, SchoolID = @SchoolID, ClassID = @ClassID, SectionID = @SectionID, SubjectGroupID = @SubjectGroupID WHERE (JoinID = @JoinID)">
                <DeleteParameters>
                    <asp:Parameter Name="JoinID" Type="Int32" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:Parameter DefaultValue="" Name="SectionID" Type="String" />
                    <asp:Parameter DefaultValue="0" Name="SubjectGroupID" Type="String" />
                    <asp:Parameter DefaultValue="0" Name="ShiftID" />
                </InsertParameters>
                <UpdateParameters>
                    <asp:Parameter Name="RegistrationID" Type="Int32" />
                    <asp:Parameter Name="SchoolID" Type="Int32" />
                    <asp:Parameter Name="ClassID" Type="Int32" />
                    <asp:Parameter Name="SectionID" Type="String" />
                    <asp:Parameter Name="SubjectGroupID" Type="String" />
                    <asp:Parameter Name="JoinID" Type="Int32" />
                </UpdateParameters>
            </asp:SqlDataSource>
        </div>
    </div>

    <div class="card my-4">
        <div class="card-body">
            <div class="form-inline mb-3">
                <div class="md-form">
                    <asp:TextBox ID="ShiftTextBox" runat="server" placeholder="Enter Shift Name" CssClass="form-control EnableText"></asp:TextBox>
                </div>
                <asp:Button ID="ShiftButton" runat="server" OnClick="ShiftButton_Click" Text="Add Shift" CssClass="btn btn-primary EnableSubmit" ValidationGroup="Sh" />
                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="ShiftTextBox" CssClass="EroorSummer" ErrorMessage="Enter Shift" ValidationGroup="Sh"></asp:RequiredFieldValidator>
            </div>
            <asp:GridView ID="ShiftGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="ShiftID" DataSourceID="ShiftSQL" CssClass="mGrid">
                <Columns>
                    <asp:TemplateField HeaderText="Shift" SortExpression="Shift">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Shift") %>' CssClass="textbox"></asp:TextBox>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("Shift") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowEditButton="True">
                        <ItemStyle HorizontalAlign="Center" Width="50px" />
                    </asp:CommandField>
                    <asp:TemplateField HeaderText="Delete">
                        <ItemTemplate>
                            <asp:LinkButton ID="DImB" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('This Data Will be Deleted Permanently From Everywhere \n Are You sure want to delete?')" />
                        </ItemTemplate>
                        <ItemStyle Width="60px" />
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="UPDATE [StudentsClass] SET [ShiftID] = 0 WHERE [ShiftID] = @ShiftID
DELETE FROM [CreateShift] WHERE [ShiftID] = @ShiftID
DELETE FROM [Join] WHERE (ShiftID = @ShiftID)"
                InsertCommand="INSERT INTO [CreateShift] ([RegistrationID], [SchoolID], [ClassID], [Shift]) VALUES (@RegistrationID, @SchoolID, @ClassID, @Shift)" SelectCommand="SELECT * FROM [CreateShift] WHERE (ClassID = @ClassID) AND (SchoolID = @SchoolID)
"
                UpdateCommand="UPDATE [CreateShift] SET [Shift] = @Shift WHERE [ShiftID] = @ShiftID">
                <DeleteParameters>
                    <asp:Parameter Name="ShiftID" Type="Int32" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:ControlParameter ControlID="ShiftTextBox" Name="Shift" PropertyName="Text" Type="String" />
                </InsertParameters>
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
                <UpdateParameters>
                    <asp:Parameter Name="Shift" Type="String" />
                    <asp:Parameter Name="ShiftID" Type="Int32" />
                </UpdateParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="ShiftJoinSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Join] WHERE [JoinID] = @JoinID" InsertCommand="INSERT INTO [Join] ([RegistrationID], [SchoolID], [ClassID], [SectionID], [ShiftID], [SubjectGroupID]) VALUES (@RegistrationID, @SchoolID, @ClassID, @SectionID, @ShiftID, @SubjectGroupID)" SelectCommand="SELECT * FROM [Join]" UpdateCommand="UPDATE [Join] SET [RegistrationID] = @RegistrationID, [SchoolID] = @SchoolID, [ClassID] = @ClassID, [SectionID] = @SectionID, [ShiftID] = @ShiftID, [SubjectGroupID] = @SubjectGroupID WHERE [JoinID] = @JoinID">
                <DeleteParameters>
                    <asp:Parameter Name="JoinID" Type="Int32" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:Parameter DefaultValue="0" Name="SectionID" Type="String" />
                    <asp:Parameter DefaultValue="" Name="ShiftID" Type="String" />
                    <asp:Parameter DefaultValue="0" Name="SubjectGroupID" Type="String" />
                </InsertParameters>
                <UpdateParameters>
                    <asp:Parameter Name="RegistrationID" Type="Int32" />
                    <asp:Parameter Name="SchoolID" Type="Int32" />
                    <asp:Parameter Name="ClassID" Type="Int32" />
                    <asp:Parameter Name="SectionID" Type="String" />
                    <asp:Parameter Name="ShiftID" Type="String" />
                    <asp:Parameter Name="SubjectGroupID" Type="String" />
                    <asp:Parameter Name="JoinID" Type="Int32" />
                </UpdateParameters>
            </asp:SqlDataSource>
        </div>
    </div>


    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">Add New Class</div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="upnlUsers" runat="server">
                        <ContentTemplate>
                            <div class="Add-section">
                                <div class="form-inline">
                                    <div class="form-group">
                                        <asp:TextBox ID="ClassnameTextBox" placeholder="Enter Class Name" runat="server" CssClass="form-control"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ClassnameTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="form-group">
                                        <asp:TextBox ID="SN_TextBox" placeholder="Enter SN" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <asp:Button ID="ClassAddButton" runat="server" CssClass="btn btn-primary" OnClick="ClassAddButton_Click" Text="Add Class" ValidationGroup="1" />
                                        <label id="S_Msg" class="SuccessMessage"></label>
                                    </div>
                                </div>
                            </div>

                            <div class="table-responsive">
                                <asp:GridView ID="ClassGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="ClassID" DataSourceID="CreateClassSQL" OnRowDeleted="ClassGridView_RowDeleted" CssClass="mGrid" PageSize="12">
                                    <Columns>
                                        <asp:TemplateField>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="EditClassTextBox" runat="server" CssClass="textbox" Text='<%#Bind("Class") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="Label1" runat="server" Text='<%#Bind("Class") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>

                                        <asp:TemplateField HeaderText="Class Serial">
                                            <ItemTemplate>
                                                <asp:TextBox ID="SN_TextBox" Text='<%# Bind("SN") %>' runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)"></asp:TextBox>
                                            </ItemTemplate>
                                            <ItemStyle Width="100px" />
                                        </asp:TemplateField>

                                        <asp:CommandField ShowEditButton="True" HeaderText="Edit">
                                            <ItemStyle Width="50" />
                                        </asp:CommandField>
                                        <asp:TemplateField HeaderText="Delete" ShowHeader="False">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are You sure want to delete?')"></asp:LinkButton>
                                            </ItemTemplate>
                                            <ItemStyle Width="60px" />
                                        </asp:TemplateField>
                                    </Columns>
                                    <PagerStyle CssClass="pgr" />
                                </asp:GridView>
                            </div>

                            <asp:Button ID="SN_Button" runat="server" Text="Update Class Serial" CssClass="btn btn-success" OnClick="SN_Button_Click" />
                            <asp:SqlDataSource ID="CreateClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [CreateClass] WHERE [ClassID] = @ClassID" InsertCommand="IF NOT EXISTS(SELECT * FROM [CreateClass] WHERE (SchoolID = @SchoolID AND Class = @Class))
INSERT INTO [CreateClass] ([SchoolID], [RegistrationID], [Class],[SN]) VALUES (@SchoolID, @RegistrationID, @Class ,@SN)"
                                SelectCommand="SELECT ClassID, SchoolID, RegistrationID, Class,SN FROM CreateClass WHERE (SchoolID = @SchoolID) order by SN" UpdateCommand="IF NOT EXISTS(SELECT * FROM [CreateClass] WHERE (SchoolID = @SchoolID AND Class = @Class))
UPDATE [CreateClass] SET [Class] = @Class WHERE [ClassID] = @ClassID">
                                <DeleteParameters>
                                    <asp:Parameter Name="ClassID" Type="Int32" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                    <asp:ControlParameter ControlID="ClassnameTextBox" Name="Class" PropertyName="Text" Type="String" />
                                    <asp:ControlParameter ControlID="SN_TextBox" Name="SN" PropertyName="Text" Type="String" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                                <UpdateParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:Parameter Name="Class" Type="String" />
                                    <asp:Parameter Name="ClassID" Type="Int32" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="UpdateSN_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass]" UpdateCommand="UPDATE CreateClass SET SN = @SN WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID)">
                                <UpdateParameters>
                                    <asp:Parameter Name="SN" />
                                    <asp:Parameter Name="ClassID" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
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
                <img src="../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script>
        $(function () {
            $(".EnableSubmit").prop("disabled", !0);
            $(".EnableText").prop("disabled", !0);

            if ($('[id*=ClassDropDownList] :selected').val() != 0) {
                $(".EnableSubmit").prop("disabled", !1);
                $(".EnableText").prop("disabled", !1);
                $("#Dis_Cls").text($('[id*=ClassDropDownList] :selected').text());
            }
            else {
                $(".EnableSubmit").prop("disabled", !0);
                $(".EnableText").prop("disabled", !0);
            }
        });

        function Success() {
            var e = $('#S_Msg');
            e.text("Class Inserted Successfully!!");
            e.fadeIn();
            e.queue(function () { setTimeout(function () { e.dequeue(); }, 3000); });
            e.fadeOut('slow');
        }

    </script>
</asp:Content>
