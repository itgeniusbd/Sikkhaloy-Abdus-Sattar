<%@ Page Title="Attendance Schedule" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Absence_Fee_Manage.aspx.cs" Inherits="EDUCATION.COM.ATTENDANCES.Absence_Fee_Manage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="/JS/TimePicker/mdtimepicker.css" rel="stylesheet" />
    <style>
        .HC { font-weight: bold; color: #000; font-size: 14px; }
        .mGrid td .form-control { width: 90%; display: inline-block; }
        .mGrid td table td { border: none; padding: 0; font-size: 1rem; }

        .day { color: #333; }
        .stime { color: #00a12a !important; }
        .ltime { color: #F00 !important; }
        .etime { color: #000 !important; }

        .error input { color: #F00; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <h3>Attendance Schedule (Student/Employee)</h3>
            <div class="form-inline">
                <div class="form-group">
                    <asp:TextBox ID="ScheduleTextBox" placeholder="Schedule Name" runat="server" CssClass="form-control"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ControlToValidate="ScheduleTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="AS"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="StartTimeTextBox" placeholder="Start Time" runat="server" CssClass="form-control Time"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="StartTimeTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="AS"></asp:RequiredFieldValidator>
                    <input type="hidden" class="StartTime" />
                </div>
                <div class="form-group">
                    <asp:TextBox ID="LateTimeTextBox" placeholder="Late Entry Time" runat="server" CssClass="form-control Time"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="LateTimeTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="AS"></asp:RequiredFieldValidator>
                    <input type="hidden" class="LateTime" />
                </div>
                <div class="form-group">
                    <asp:TextBox ID="EndTimeTextBox" placeholder="End Time" runat="server" CssClass="form-control Time"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="EndTimeTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="AS"></asp:RequiredFieldValidator>
                    <input type="hidden" class="EndTime" />
                </div>
                <div class="form-group">
                    <asp:Button ID="Add_Schedule_Button" CausesValidation="true" OnClientClick="return timeCheck();" runat="server" CssClass="btn btn-primary" OnClick="Add_Schedule_Button_Click" Text="Add Schedule" ValidationGroup="AS" />
                    <asp:SqlDataSource ID="ScheduleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ScheduleName, ScheduleID FROM Attendance_Schedule WHERE (SchoolID = @SchoolID)" DeleteCommand="DELETE FROM Employee_Attendance_Schedule_Assign WHERE (SchoolID = @SchoolID) AND (ScheduleID = @ScheduleID)
DELETE FROM Attendance_Schedule_AssignStudent WHERE  (SchoolID = @SchoolID) AND (ScheduleID = @ScheduleID)
DELETE FROM Attendance_Schedule_Day WHERE (ScheduleID = @ScheduleID) AND (SchoolID = @SchoolID)
DELETE FROM Attendance_Schedule WHERE (ScheduleID = @ScheduleID) AND (SchoolID = @SchoolID)
"
                        InsertCommand="INSERT INTO Attendance_Schedule(SchoolID, RegistrationID, ScheduleName, LateEntryTime, StartTime, EndTime, Date) VALUES (@SchoolID, @RegistrationID, @ScheduleName, @LateEntryTime, @StartTime, @EndTime, GETDATE())" UpdateCommand="UPDATE Attendance_Schedule SET ScheduleName = @ScheduleName WHERE (ScheduleID = @ScheduleID)">
                        <DeleteParameters>
                            <asp:Parameter Name="ScheduleID" Type="Int32" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />

                            <asp:ControlParameter ControlID="ScheduleTextBox" Name="ScheduleName" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="LateTimeTextBox" Name="LateEntryTime" PropertyName="Text" />
                            <asp:ControlParameter ControlID="StartTimeTextBox" Name="StartTime" PropertyName="Text" />
                            <asp:ControlParameter ControlID="EndTimeTextBox" Name="EndTime" PropertyName="Text" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />

                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="ScheduleName" Type="String" />
                            <asp:Parameter Name="ScheduleID" Type="Int32" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                    <label id="ErrorLabel" class="EroorSummer"></label>
                    <asp:Label ID="AddSuccessLabel" runat="server" ForeColor="Green"></asp:Label>
                </div>
            </div>

            <div class="table-responsive">
                <%if (ScheduleGridView.Rows.Count > 0)
                    {%>
                <h4>
                    <span class="badge stime">Start Time</span>
                    <span class="badge ltime">Late Entry Time</span>
                    <span class="badge etime">End Time</span>
                </h4>
                <asp:GridView ID="ScheduleGridView" runat="server" AllowSorting="True" AutoGenerateColumns="False" DataKeyNames="ScheduleID" DataSourceID="ScheduleSQL" CssClass="mGrid" OnSelectedIndexChanged="ScheduleGridView_SelectedIndexChanged">
                    <Columns>
                        <asp:TemplateField HeaderText="Select Schedule">
                            <EditItemTemplate>
                                <asp:TextBox ID="ScheduleNameTextBox" runat="server" CssClass="form-control" Text='<%# Bind("ScheduleName") %>'></asp:TextBox><br />
                                <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" CommandName="Update" Text="Update"></asp:LinkButton>
                                &nbsp;<asp:LinkButton ID="ScheduleCancelLinkButton" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:LinkButton ID="ScdlLinkButton" ToolTip="Click here to edit time" runat="server" CausesValidation="False" CommandName="Select" Text='<%# Bind("ScheduleName") %>' Font-Size="13" ForeColor="#003399" Font-Bold="True" />
                                - 
                  <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit"></asp:LinkButton>
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Left" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Time">
                            <ItemTemplate>
                                <asp:DataList ID="Day_Time_DataList" runat="server" DataSourceID="Day_TimeSQL" RepeatDirection="Horizontal" RepeatLayout="Table" Width="100%">
                                    <ItemTemplate>
                                        <table style="width: 100%">
                                            <tr>
                                                <td class="day">
                                                    <asp:CheckBox ID="DayCheckBox" Checked='<%#Eval("Is_OnDay") %>' Font-Bold='<%#Eval("Is_OnDay") %>' Enabled="false" runat="server" Text='<%# Eval("Day") %>'/>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="stime">
                                                    <asp:Label ID="StartTimeLabel" ToolTip="Start Time" runat="server" Text='<%# Eval("StartTime") %>' />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="ltime">
                                                    <asp:Label ID="LateEntryTimeLabel" ToolTip="Late Entry Time" runat="server" Text='<%# Eval("LateEntryTime") %>' />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="etime">
                                                    <asp:Label ID="EndTimeLabel" ToolTip="End Time" runat="server" Text='<%# Eval("EndTime") %>' />
                                                </td>
                                            </tr>
                                        </table>
                                    </ItemTemplate>
                                </asp:DataList>
                                <asp:SqlDataSource ID="Day_TimeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Day, CONVERT(varchar(15), LateEntryTime, 100) AS LateEntryTime, CONVERT(varchar(15),  StartTime, 100) AS StartTime, CONVERT(varchar(15),EndTime, 100) AS EndTime,Is_OnDay FROM Attendance_Schedule_Day WHERE (ScheduleID = @ScheduleID)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="SdulID_HF" Name="ScheduleID" PropertyName="Value" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                                <asp:HiddenField ID="SdulID_HF" runat="server" Value='<%#Bind("ScheduleID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField ShowHeader="False">
                            <ItemTemplate>
                                <asp:LinkButton ID="ScheduleDeleteLinkButton" OnClientClick="return confirm('If you delete schedule, assign will be deleted.')" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <SelectedRowStyle BackColor="#e9e9e9" />
                </asp:GridView>
                <%} %>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Modal -->
    <div class="modal fade" id="daySchedulModal" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel"
        aria-hidden="true">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel">Timing Of Day Schedule</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body table-responsive">
                    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                        <ContentTemplate>
                            <asp:GridView ID="ModifyScheduleGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="ScheduleDayID,Day" DataSourceID="Schedule_Day_SQL">
                                <Columns>
                                    <asp:TemplateField HeaderText="On Day">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="DayCheckBox" Checked='<%#Bind("Is_OnDay") %>' runat="server" Text=" " />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Day" HeaderText="Day" ReadOnly="True" SortExpression="Day" />
                                    <asp:TemplateField HeaderText="Start Time" SortExpression="StartTime">
                                        <ItemTemplate>
                                            <div class="form-group mb-0">
                                                <asp:TextBox ID="EStartTimeTextBox" CssClass="Time form-control" runat="server" Text='<%# Bind("StartTime") %>'></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="RV1" runat="server" ControlToValidate="EStartTimeTextBox" CssClass="EroorSummer" ErrorMessage="*" SetFocusOnError="True" ValidationGroup="CD"></asp:RequiredFieldValidator>
                                                <input type="hidden" class="StartTime" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Late Entry Time" SortExpression="LateEntryTime">
                                        <ItemTemplate>
                                            <div class="form-group mb-0">
                                                <asp:TextBox ID="ELateEntryTimeTextBox" CssClass="Time form-control" runat="server" Text='<%# Bind("LateEntryTime") %>'></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="RV2" runat="server" ControlToValidate="ELateEntryTimeTextBox" CssClass="EroorSummer" ErrorMessage="*" SetFocusOnError="True" ValidationGroup="CD"></asp:RequiredFieldValidator>
                                                <input type="hidden" class="LateTime" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="End Time" SortExpression="EndTime">
                                        <ItemTemplate>
                                            <div class="form-group mb-0">
                                                <asp:TextBox ID="EEndTimeTextBox" CssClass="Time form-control" runat="server" Text='<%# Bind("EndTime") %>'></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="RV3" runat="server" ControlToValidate="EEndTimeTextBox" CssClass="EroorSummer" ErrorMessage="*" SetFocusOnError="True" ValidationGroup="CD"></asp:RequiredFieldValidator>
                                                <input type="hidden" class="EndTime" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                            <asp:SqlDataSource ID="Schedule_Day_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ScheduleDayID, ScheduleID, SchoolID, RegistrationID, Day, LateEntryTime, StartTime, EndTime, Insert_Date, Is_OnDay FROM Attendance_Schedule_Day WHERE (ScheduleID = @ScheduleID) AND (SchoolID = @SchoolID)" UpdateCommand="UPDATE Attendance_Schedule_Day SET LateEntryTime = @LateEntryTime, StartTime = @StartTime, EndTime = @EndTime,Is_OnDay = @Is_OnDay WHERE (ScheduleID = @ScheduleID) AND (Day = @Day)">
                                <SelectParameters>
                                    <asp:ControlParameter ControlID="ScheduleGridView" Name="ScheduleID" PropertyName="SelectedValue" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                                <UpdateParameters>
                                    <asp:Parameter Name="LateEntryTime" />
                                    <asp:Parameter Name="StartTime" />
                                    <asp:Parameter Name="EndTime" />
                                    <asp:ControlParameter ControlID="ScheduleGridView" Name="ScheduleID" PropertyName="SelectedValue" />
                                    <asp:Parameter Name="Day" />
                                    <asp:Parameter Name="Is_OnDay" />
                                </UpdateParameters>
                            </asp:SqlDataSource>

                            <asp:Button ID="Day_Time_UpdateButton" OnClientClick="return updateTimeCheck();" runat="server" CssClass="btn btn-primary" OnClick="Day_Time_UpdateButton_Click" Text="Update" ValidationGroup="CD" />
                            <label id="ErrorLabel2" class="EroorSummer d-block"></label>
                            <asp:Label ID="ErrLabel" runat="server" ForeColor="Green"></asp:Label>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>


    <h3 class="mt-3">Attendance Fine</h3>
    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="FineForDropDownList" runat="server" CssClass="form-control">
                <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                <asp:ListItem Value="Abs">Absent</asp:ListItem>
                <asp:ListItem>Late</asp:ListItem>
                <asp:ListItem>Bunk</asp:ListItem>
            </asp:DropDownList>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="FineForDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="1"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:TextBox ID="AmountTextBox" placeholder="Fine Amount" autocomplete="off" CssClass="form-control" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false" runat="server"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="AmountTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="Add_Fine_Button" runat="server" Text="Add Fine" CssClass="btn btn-primary" OnClick="Add_Fine_Button_Click" ValidationGroup="1" />
            <asp:SqlDataSource ID="AttendanceFineSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Attendance_Fine] WHERE [AttendanceFineID] = @AttendanceFineID" InsertCommand="IF NOT EXISTS (SELECT * FROM [Attendance_Fine]  WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND FineFor = @FineFor)
BEGIN
  INSERT INTO [Attendance_Fine] ([FineAmount], [FineFor], [SchoolID], [RegistrationID], [EducationYearID]) VALUES (@FineAmount, @FineFor, @SchoolID, @RegistrationID, @EducationYearID)
END 
ELSE
BEGIN
  UPDATE Attendance_Fine SET FineAmount = @FineAmount, FineFor = @FineFor WHERE (SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND FineFor = @FineFor)
END"
                SelectCommand="SELECT Attendance_Fine.* FROM Attendance_Fine WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)" UpdateCommand="UPDATE Attendance_Fine SET FineAmount = @FineAmount, FineFor = @FineFor WHERE (AttendanceFineID = @AttendanceFineID)">
                <DeleteParameters>
                    <asp:Parameter Name="AttendanceFineID" Type="Int32" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:ControlParameter ControlID="FineForDropDownList" Name="FineFor" PropertyName="SelectedValue" Type="String" />
                    <asp:ControlParameter ControlID="AmountTextBox" Name="FineAmount" PropertyName="Text" Type="Double" />
                </InsertParameters>
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
                <UpdateParameters>
                    <asp:Parameter Name="FineAmount" Type="Double" />
                    <asp:Parameter Name="FineFor" Type="String" />
                    <asp:Parameter Name="AttendanceFineID" Type="Int32" />
                </UpdateParameters>
            </asp:SqlDataSource>
        </div>
    </div>

    <asp:GridView ID="AFineGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="AttendanceFineID" DataSourceID="AttendanceFineSQL">
        <Columns>
            <asp:BoundField DataField="FineFor" HeaderText="Fine For" SortExpression="FineFor" />
            <asp:BoundField DataField="FineAmount" HeaderText="Fine Amount" SortExpression="FineAmount" />
            <asp:CommandField ShowEditButton="True">
                <ItemStyle Width="50px" />
            </asp:CommandField>
            <asp:TemplateField ShowHeader="False">
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are sure want to delete?')"></asp:LinkButton>
                </ItemTemplate>
                <ItemStyle Width="50px" />
            </asp:TemplateField>
        </Columns>
    </asp:GridView>


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

    <script src="/JS/TimePicker/mdtimepicker.js?v=5"></script>
    <script type="text/javascript">
        $(function () {
            $('.Time').mdtimepicker({
                theme: 'green'
            }).on('timechanged', function (e) {
                $(this).closest('.form-group').find(".StartTime").val(e.time);
                $(this).closest('.form-group').find(".LateTime").val(e.time);
                $(this).closest('.form-group').find(".EndTime").val(e.time);

                timeCheck();
                updateTimeCheck();
            });

            $.map($("[id*=ScheduleGridView] tr"), function (item, index) {
                if ($(item).find("input[type=text]").length > 0) {
                    const s = $(item).find("input[type=text][id*=EStartTimeTextBox]");
                    const l = $(item).find("input[type=text][id*=ELateEntryTimeTextBox]");
                    const e = $(item).find("input[type=text][id*=EEndTimeTextBox]");

                    const start = s.closest('.form-group').find(".StartTime").val(s.data('time'));
                    const late = l.closest('.form-group').find(".LateTime").val(l.data('time'));
                    const end = e.closest('.form-group').find(".EndTime").val(e.data('time'));
                }
            });
        });

        function timeCheck() {
            var isValid = false;
            const start = $(".StartTime").val().substr(0, 5);
            const late = $(".LateTime").val().substr(0, 5);
            const end = $(".EndTime").val().substr(0, 5);

            if (start !== '' && late !== '' && end !== '') {
                const s = new Date(`11/14/2000 ${start}`);
                const l = new Date(`11/14/2000 ${late}`);
                const e = new Date(`11/14/2000 ${end}`);

                if ((s < l) && (s < e) && (l < e)) {
                    $("#ErrorLabel").text('');
                    $("[id*=AddSuccessLabel]").text('');
                    isValid = true;
                } else {
                    $("#ErrorLabel").text("Late & End Time Must be greater than Start Time!");
                    isValid = false;
                }
            }

            return isValid;
        }

        function updateTimeCheck() {
            var isValid = true;
            $.map($("[id*=ScheduleGridView] tr"), function (item, index) {
                if ($(item).find("input[type=text]").length > 0) {
                    let s = $(item).find("[id*=EStartTimeTextBox]");
                    let l = $(item).find("[id*=ELateEntryTimeTextBox]");
                    let e = $(item).find("[id*=EEndTimeTextBox]");

                    const start = s.closest('.form-group').find(".StartTime").val().substr(0, 5);
                    const late = l.closest('.form-group').find(".LateTime").val().substr(0, 5);
                    const end = e.closest('.form-group').find(".EndTime").val().substr(0, 5);

                    if (start !== '' && late !== '' && end != '') {
                        s = new Date(`11/14/2000 ${start}`);
                        l = new Date(`11/14/2000 ${late}`);
                        e = new Date(`11/14/2000 ${end}`);

                        if ((s < l) && (s < e) && (l < e)) {
                            console.log("condition true");
                        } else {
                            isValid = false;
                        }
                    }
                }
            });

            if (!isValid) {
                $("#ErrorLabel2").text("Late & End Time Must be greater than Start Time!");
                return false;
            } else {
                $("#ErrorLabel2").text("");
                $("[id*=ErrLabel]").text('');
                return true;
            }
        }

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $('.Time').mdtimepicker({
                theme: 'green'
            }).on('timechanged', function (e) {
                $(this).closest('.form-group').find(".StartTime").val(e.time);
                $(this).closest('.form-group').find(".LateTime").val(e.time);
                $(this).closest('.form-group').find(".EndTime").val(e.time);
                timeCheck();
                updateTimeCheck();
            });

            $.map($("[id*=ScheduleGridView] tr"), function (item, index) {
                if ($(item).find("input[type=text]").length > 0) {
                    var s = $(item).find("input[type=text][id*=EStartTimeTextBox]");
                    var l = $(item).find("input[type=text][id*=ELateEntryTimeTextBox]");
                    var e = $(item).find("input[type=text][id*=EEndTimeTextBox]");

                    var start = s.closest('.form-group').find(".StartTime").val(s.data('time'));
                    var late = l.closest('.form-group').find(".LateTime").val(l.data('time'));
                    var end = e.closest('.form-group').find(".EndTime").val(e.data('time'));
                }
            });
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };

        function DayScheduleModal() {
            $("#daySchedulModal").modal('show');
        }
    </script>
</asp:Content>
