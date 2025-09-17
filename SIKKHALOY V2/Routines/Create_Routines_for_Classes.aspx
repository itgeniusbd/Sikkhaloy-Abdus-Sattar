<%@ Page Title="Create Routine" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Create_Routines_for_Classes.aspx.cs" Inherits="EDUCATION.COM.ROUTINES.Create_Routines_for_Classes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/RoutineInfo.css" rel="stylesheet" />
    <link href="/JS/TimePicker/mdtimepicker.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Routine Time And Period</h3>

    <div class="row mb-3">
        <div class="col-lg-8">
            <div class="card">
                <div class="card-body">
                    <div class="md-form mt-0">
                        <a class="blue-text" data-toggle="modal" data-target="#RoutineModal"><i class="fa fa-plus-circle mr-1"></i>Create/Delete Routine Name</a>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="RoutineNameDropDownList" CssClass="EroorStar" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                        <asp:DropDownList ID="RoutineNameDropDownList" CssClass="form-control" runat="server" AppendDataBoundItems="True" DataSourceID="SelectRoutineInfoSQL" DataTextField="RoutineSpecification" DataValueField="RoutineInfoID">
                            <asp:ListItem Value="0">[ SELECT ROUTINE NAME ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="SelectRoutineInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoutineInfoID, RoutineSpecification FROM RoutineInfo WHERE(SchoolID = @SchoolID) AND (RoutineInfoID NOT IN (SELECT RoutineInfoID FROM RoutineDay WHERE (SchoolID = @SchoolID)))">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>

                    <asp:CheckBoxList ID="WeekNameCheckBoxList" runat="server" RepeatDirection="Horizontal" CssClass="checkbox">
                        <asp:ListItem>Sat</asp:ListItem>
                        <asp:ListItem>Sun</asp:ListItem>
                        <asp:ListItem>Mon</asp:ListItem>
                        <asp:ListItem>Tue</asp:ListItem>
                        <asp:ListItem>Wed</asp:ListItem>
                        <asp:ListItem>Thu</asp:ListItem>
                        <asp:ListItem>Fri</asp:ListItem>
                    </asp:CheckBoxList>

                    <div class="md-form m-0">
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="RoutinePeriodTextBox" CssClass="EroorStar" ErrorMessage="Enter  Routine Period" ValidationGroup="1">*</asp:RequiredFieldValidator>
                        <asp:TextBox ID="RoutinePeriodTextBox" placeholder="Period Name" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="md-form m-0">
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="StartTimeTextBox" CssClass="EroorStar" ErrorMessage="Enter start time" ValidationGroup="1">*</asp:RequiredFieldValidator>
                        <asp:TextBox ID="StartTimeTextBox" runat="server" CssClass="form-control tp" placeholder="Start Time"></asp:TextBox>
                    </div>
                    <div class="md-form mt-0">
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="RoutinePeriodTextBox" CssClass="EroorStar" ErrorMessage="Enter end time" ValidationGroup="1">*</asp:RequiredFieldValidator>
                        <asp:TextBox ID="EndTimeTextBox" runat="server" CssClass="form-control tp" placeholder="End Time"></asp:TextBox>
                    </div>
                     <asp:CheckBox ID="OffTime_CheckBox" Text="Make this period as off time" runat="server" ForeColor="Red" />
                    <div class="md-form mt-0">
                        <asp:Button ID="AddButton" runat="server" OnClick="AddToCartButton_Click" Text="Add To List" CssClass="btn btn-amber" ValidationGroup="1" />
                    </div>

                    <asp:GridView ID="RoutineGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid">
                        <Columns>
                            <asp:BoundField DataField="RoutinePeriod" HeaderText="Routine Period" SortExpression="RoutinePeriod" />
                            <asp:BoundField DataField="StartTime" HeaderText="Start Time" SortExpression="StartTime" />
                            <asp:BoundField DataField="EndTime" HeaderText="End Time" SortExpression="EndTime" />
                            <asp:BoundField DataField="Duration" HeaderText="Duration" SortExpression="Duration" />
                            <asp:TemplateField HeaderText="Off Time">
                                <ItemTemplate>
                                    <asp:CheckBox ID="Show_OffTime_CheckBox" Checked='<%#Eval("OffTime") %>' Text=" " runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ShowHeader="False" HeaderText="Delete">
                                <ItemTemplate>
                                    <asp:LinkButton ID="LinkButton1" runat="server" OnClick="RowDelete" Text="Delete"></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <%if (RoutineGridView.Rows.Count > 0)
                        {%>
                    <asp:Button ID="CompleteButton" runat="server" OnClick="CompleteButton_Click" Text="Save &amp; continue" CssClass="btn btn-primary" />
                    <%} %>
                </div>
            </div>
        </div>
    </div>

    <asp:SqlDataSource ID="RoutineDaySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO RoutineDay(RoutineInfoID, SchoolID, RegistrationID, Day) VALUES (@RoutineInfoID, @SchoolID, @RegistrationID, @Day)" SelectCommand="SELECT * FROM [RoutineDay]">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:ControlParameter ControlID="RoutineNameDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" Type="Int32" />
            <asp:Parameter Name="Day" Type="String" />
        </InsertParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="RoutineTimeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO RoutineTime(RoutineInfoID, SchoolID, RegistrationID, RoutinePeriod, StartTime, EndTime, Duration, Is_OffTime) VALUES (@RoutineInfoID, @SchoolID, @RegistrationID, @RoutinePeriod, @StartTime, @EndTime, @Duration, @Is_OffTime)" SelectCommand="SELECT * FROM [RoutineTime]">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:ControlParameter ControlID="RoutineNameDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" Type="Int32" />
            <asp:Parameter Name="RoutinePeriod" Type="String" />
            <asp:Parameter Name="StartTime" />
            <asp:Parameter Name="EndTime" />
            <asp:Parameter Name="Duration" Type="String" />
            <asp:Parameter Name="Is_OffTime" />
        </InsertParameters>
    </asp:SqlDataSource>

    <!--Routine Modal -->
    <div class="modal fade" id="RoutineModal" tabindex="-1" role="dialog" aria-labelledby="RoutineModalLabel" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">Add Routine Name</div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                        <ContentTemplate>
                            <div class="form-inline">
                                <div class="md-form">
                                    <asp:TextBox ID="RoutinenameTextBox" runat="server" CssClass="form-control" placeholder="Routine Name"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="RoutinenameTextBox" CssClass="EroorStar" ValidationGroup="R">*</asp:RequiredFieldValidator>
                                </div>
                                <asp:Button ID="RoutineNameButton" ValidationGroup="R" runat="server" Text="Add" CssClass="btn btn-dark-green" OnClick="RoutineNameButton_Click" />
                            </div>
                            <asp:GridView ID="RnameGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="RoutineInfoID" DataSourceID="RoutineInfoSQL">
                                <Columns>
                                    <asp:CommandField ShowEditButton="True" />
                                    <asp:BoundField DataField="RoutineSpecification" HeaderText="Routine Name" SortExpression="RoutineSpecification" />
                                    <asp:TemplateField HeaderText="Routine Delete">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Warning!! Routine will be Deleted Permanently!!')"></asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                            <asp:SqlDataSource ID="RoutineInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO RoutineInfo(SchoolID, RegistrationID, RoutineSpecification, Date) VALUES (@SchoolID, @RegistrationID, @RoutineSpecification, GETDATE())" SelectCommand="SELECT RoutineInfoID, RoutineSpecification FROM RoutineInfo WHERE(SchoolID = @SchoolID)" DeleteCommand="DELETE FROM RoutineDay WHERE (SchoolID = @SchoolID) AND (RoutineInfoID = @RoutineInfoID)
DELETE FROM RoutineForClass  WHERE (SchoolID = @SchoolID) AND (RoutineInfoID = @RoutineInfoID)
DELETE FROM RoutineInfo WHERE (SchoolID = @SchoolID) AND (RoutineInfoID = @RoutineInfoID)
DELETE FROM RoutineTime WHERE (SchoolID = @SchoolID) AND (RoutineInfoID = @RoutineInfoID)">
                                <DeleteParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:Parameter Name="RoutineInfoID" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                    <asp:ControlParameter ControlID="RoutinenameTextBox" Name="RoutineSpecification" PropertyName="Text" Type="String" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
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


    <script src="/JS/TimePicker/mdtimepicker.js?v=4"></script>
    <script type="text/javascript">
        $(function () {
            $('.tp').mdtimepicker({
                theme: 'green',
                readOnly: false
            });
        });
    </script>
</asp:Content>
