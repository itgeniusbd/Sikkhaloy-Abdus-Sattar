<%@ Page Title="Specify Group/Section/Shift" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Specify_group_section_Shift_for_classes.aspx.cs" Inherits="EDUCATION.COM.ADMINISTRATION_BASICSETTING.Specify_group_section_Shift_for_classes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <h3>Specify Group/Section/Shift For Class
              <small class="form-text text-muted">Join each other (Group/Section/Shift) which is contain in class</small>
            </h3>

            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged" Width="200px">
                <asp:ListItem Value="0">[SELECT CLASS]</asp:ListItem>
            </asp:DropDownList>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorSummer" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1"></asp:RequiredFieldValidator>
            <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>

            <div class="d-flex justify-content-start mb-2">
                <asp:GridView ID="GroupGridView" runat="server" DataKeyNames="SubjectGroupID" AutoGenerateColumns="False" DataSourceID="GroupSQL" CssClass="mGrid" Width="100%">
                    <Columns>
                        <asp:BoundField DataField="SubjectGroup" HeaderText="Group" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:CheckBox ID="GroupCheckBox" runat="server" AutoPostBack="True" OnCheckedChanged="GroupCheckBox_CheckedChanged" Text=" " />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateSubjectGroup] WHERE ([ClassID] = @ClassID)">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <asp:GridView ID="SectionGridView" runat="server" DataKeyNames="SectionID" AutoGenerateColumns="False" DataSourceID="SectionSQL" CssClass="mGrid" Width="100%">
                    <Columns>
                        <asp:BoundField DataField="Section" HeaderText="Section" SortExpression="Section" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:CheckBox ID="SectionCheckBox" runat="server" AutoPostBack="True" OnCheckedChanged="GroupCheckBox_CheckedChanged" Text=" " />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateSection] WHERE ([ClassID] = @ClassID)">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <asp:GridView ID="ShiftGridView" runat="server" DataKeyNames="ShiftID" AutoGenerateColumns="False" DataSourceID="ShiftSQL" CssClass="mGrid" Width="100%">
                    <Columns>
                        <asp:BoundField DataField="Shift" HeaderText="Shift" SortExpression="Shift" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:CheckBox ID="ShiftCheckBox" runat="server" AutoPostBack="True" OnCheckedChanged="GroupCheckBox_CheckedChanged" Text=" " />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateShift] WHERE ([ClassID] = @ClassID)">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>


            <asp:Button ID="JoinButton" runat="server" CssClass="btn btn-primary" OnClick="JoinButton_Click" Text="Submit" ValidationGroup="1" />
            <asp:Label ID="ExistLabel" runat="server" CssClass="EroorSummer"></asp:Label>
            <asp:Label ID="MassageLabel" runat="server" CssClass="EroorSummer"></asp:Label>
            <asp:SqlDataSource ID="JoinSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [Join] (RegistrationID, SchoolID, ClassID, SectionID, SubjectGroupID, ShiftID) VALUES (@RegistrationID, @SchoolID, @ClassID, @SectionID, @SubjectGroupID, @ShiftID)" SelectCommand="SELECT * FROM [Join]">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:Parameter Name="SectionID" Type="String" />
                    <asp:Parameter Name="SubjectGroupID" Type="String" />
                    <asp:Parameter Name="ShiftID" />
                </InsertParameters>
            </asp:SqlDataSource>

            <div class="mt-2">
                <asp:GridView ID="ShowJoinGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="JoinID" DataSourceID="ShowJoinSQL" PagerStyle-CssClass="pgr" Width="100%">
                    <AlternatingRowStyle CssClass="alt" />
                    <Columns>
                        <asp:BoundField DataField="SubjectGroup" HeaderText="Group" SortExpression="SubjectGroup" />
                        <asp:BoundField DataField="Section" HeaderText="Section" SortExpression="Section" />
                        <asp:BoundField DataField="Shift" HeaderText="Shift" SortExpression="Shift" />
                        <asp:TemplateField ShowHeader="False">
                            <ItemTemplate>
                                <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are You Sure want to delete permanently?')"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pgr" />
                    <RowStyle CssClass="RowStyle" />
                </asp:GridView>
                <asp:SqlDataSource ID="ShowJoinSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Join] WHERE (JoinID = @JoinID)" SelectCommand="SELECT CreateSection.Section, CreateSubjectGroup.SubjectGroup, [Join].JoinID, CreateShift.Shift FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].ClassID = @ClassID)">
                    <DeleteParameters>
                        <asp:ControlParameter ControlID="SeGrGridView" Name="JoinID" PropertyName="SelectedValue" />
                    </DeleteParameters>
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <asp:GridView ID="ScShGridView" runat="server" AutoGenerateColumns="False" DataSourceID="ShowJoinSecShiftSQL"
                    CssClass="mGrid" PagerStyle-CssClass="pgr" AlternatingRowStyle-CssClass="alt" DataKeyNames="JoinID" Width="100%">
                    <AlternatingRowStyle CssClass="alt" />
                    <Columns>
                        <asp:BoundField DataField="Section" HeaderText="Section" SortExpression="Section" />
                        <asp:BoundField DataField="Shift" HeaderText="Shift" SortExpression="Shift" />
                        <asp:TemplateField ShowHeader="False">
                            <ItemTemplate>
                                <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are You Sure want to delete permanently?')"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pgr"></PagerStyle>
                    <RowStyle CssClass="RowStyle" />
                </asp:GridView>
                <asp:SqlDataSource ID="ShowJoinSecShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateSection.Section, CreateShift.Shift, [Join].JoinID FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].ClassID = @ClassID)" DeleteCommand="DELETE FROM [Join] WHERE (JoinID = @JoinID)">
                    <DeleteParameters>
                        <asp:ControlParameter ControlID="ScShGridView" Name="JoinID" PropertyName="SelectedValue" />
                    </DeleteParameters>
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <asp:GridView ID="ShGrGridView" runat="server" AutoGenerateColumns="False" DataSourceID="ShowJoinShiftGroupSQL"
                    CssClass="mGrid" PagerStyle-CssClass="pgr" AlternatingRowStyle-CssClass="alt" DataKeyNames="JoinID" Width="100%">
                    <AlternatingRowStyle CssClass="alt" />
                    <Columns>
                        <asp:BoundField DataField="SubjectGroup" HeaderText="Group" SortExpression="SubjectGroup" />
                        <asp:BoundField DataField="Shift" HeaderText="Shift" SortExpression="Shift" />
                        <asp:TemplateField ShowHeader="False">
                            <ItemTemplate>
                                <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are You Sure want to delete permanently?')"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pgr"></PagerStyle>
                    <RowStyle CssClass="RowStyle" />
                </asp:GridView>
                <asp:SqlDataSource ID="ShowJoinShiftGroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateShift.Shift, CreateSubjectGroup.SubjectGroup, [Join].JoinID FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID)" DeleteCommand="DELETE FROM [Join] WHERE (JoinID = @JoinID)">
                    <DeleteParameters>
                        <asp:ControlParameter ControlID="ShGrGridView" Name="JoinID" PropertyName="SelectedValue" />
                    </DeleteParameters>
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <asp:GridView ID="SeGrGridView" runat="server" AutoGenerateColumns="False" DataSourceID="ShowJoinSecGruopSQL"
                    CssClass="mGrid" PagerStyle-CssClass="pgr" AlternatingRowStyle-CssClass="alt" DataKeyNames="JoinID" Width="100%">
                    <AlternatingRowStyle CssClass="alt" />
                    <Columns>
                        <asp:BoundField DataField="SubjectGroup" HeaderText="Group" SortExpression="SubjectGroup" />
                        <asp:BoundField DataField="Section" HeaderText="Section" SortExpression="Section" />
                        <asp:TemplateField ShowHeader="False">
                            <ItemTemplate>
                                <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are You Sure want to delete permanently?')"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>

                    <PagerStyle CssClass="pgr"></PagerStyle>
                    <RowStyle CssClass="RowStyle" />
                </asp:GridView>
                <asp:SqlDataSource ID="ShowJoinSecGruopSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateSection.Section, CreateSubjectGroup.SubjectGroup, [Join].JoinID FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID)" DeleteCommand="DELETE FROM [Join] WHERE (JoinID = @JoinID)">
                    <DeleteParameters>
                        <asp:ControlParameter ControlID="SeGrGridView" Name="JoinID" PropertyName="SelectedValue" />
                    </DeleteParameters>
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    </SelectParameters>
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
        $(function () {
            $('[id*=JoinButton]').hide();
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (e, f) {
                $('[id*=JoinButton]').hide();
                if ($('[id*=GroupGridView] tr').length > 0 || $('[id*=SectionGridView] tr').length > 0 || $('[id*=ShiftGridView] tr').length > 0) {
                    $('[id*=JoinButton]').show();
                }
            });
        });
    </script>
</asp:Content>
