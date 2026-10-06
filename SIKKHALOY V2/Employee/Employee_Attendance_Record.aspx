<%@ Page Title="Employee Attendance Record" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Employee_Attendance_Record.aspx.cs" Inherits="EDUCATION.COM.Employee.Employee_Attendance_Record" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Employee_Attendance_Record.css?v=3" rel="stylesheet" />
    <style>
        /* multi-schedule picker */
        .sch-ms { position: relative; display: inline-block; vertical-align: middle; }
        .sch-ms .sch-ms-btn { min-width: 200px; max-width: 320px; text-align: left; cursor: pointer; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; padding-right: 28px; position: relative; }
        .sch-ms .sch-ms-btn:after { content: ""; position: absolute; right: 10px; top: 50%; margin-top: -2px; border-left: 5px solid transparent; border-right: 5px solid transparent; border-top: 5px solid #666; }
        .sch-ms-panel { display: none; position: absolute; z-index: 1050; left: 0; top: 100%; margin-top: 2px; min-width: 100%; width: max-content; max-width: 90vw; background: #fff; border: 1px solid #ccc; border-radius: 4px; box-shadow: 0 4px 12px rgba(0,0,0,.18); padding: 8px 12px; text-align: left; }
        .sch-ms.open .sch-ms-panel { display: block; }
        .sch-ms-tools { display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #eee; padding-bottom: 6px; margin-bottom: 6px; font-size: 13px; }
        .sch-ms-tools a { color: #0d6efd; }
        .sch-ms-count { color: #777; padding: 0 12px; }
        .sch-ms-list { display: block; position: relative; max-height: 260px; overflow-y: auto; }
        .sch-ms-list br { display: none; }
        .sch-ms-list label { display: block !important; margin: 3px 0; white-space: nowrap; cursor: pointer; }
        .sch-ms-foot { border-top: 1px solid #eee; padding-top: 6px; margin-top: 6px; text-align: right; }
        @media print { .sch-ms { display: none !important; } }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Employee Attendance Record
      <label class="Date"></label>
      <label class="SchedulePrint"></label>
    </h3>
    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
        <ContentTemplate>
            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:RadioButtonList ID="EmpTypeRadioButtonList" runat="server" AutoPostBack="True" RepeatDirection="Horizontal" CssClass="Rb form-control">
                        <asp:ListItem Selected="True" Value="%">All Employee</asp:ListItem>
                        <asp:ListItem>Teacher</asp:ListItem>
                        <asp:ListItem>Staff</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
                <div class="form-group">
                    <div class="sch-ms">
                        <button type="button" class="form-control sch-ms-btn" onclick="schMsToggle(this)"><span class="sch-ms-text">[ ALL SCHEDULE ]</span></button>
                        <div class="sch-ms-panel">
                            <div class="sch-ms-tools">
                                <a href="#" onclick="return schMsAll(this, true);">Select all</a>
                                <span class="sch-ms-count"></span>
                                <a href="#" onclick="return schMsAll(this, false);">Clear</a>
                            </div>
                            <asp:CheckBoxList ID="ScheduleCheckBoxList" runat="server" CssClass="sch-ms-list" RepeatLayout="Flow" DataSourceID="ScheduleSQL" DataTextField="ScheduleName" DataValueField="ScheduleID">
                            </asp:CheckBoxList>
                            <div class="sch-ms-foot">
                                <button type="button" class="btn btn-sm btn-primary m-0" onclick="schMsClose()">OK</button>
                            </div>
                        </div>
                    </div>
                    <asp:SqlDataSource ID="ScheduleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ScheduleID, ScheduleName FROM Attendance_Schedule WHERE (SchoolID = @SchoolID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="FromDateTextBox" runat="server" CssClass="form-control Datetime" placeholder="From Date"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control Datetime" placeholder="To Date"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Submit" />
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <ul class="nav nav-tabs z-depth-1">
        <li class="nav-item"><a class="nav-link active" href="#tab1" data-toggle="tab" role="tab" aria-expanded="true">Attendance Record</a></li>
        <li class="nav-item"><a class="nav-link" href="#tab2" data-toggle="tab" role="tab" aria-expanded="false">Attendance Summary</a></li>
    </ul>

    <div class="tab-content card">
        <div id="tab1" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <div class="form-inline NoPrint">
                        <div class="form-group">
                            <asp:DropDownList ID="EmployeeDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="Emp_SQL" DataTextField="Name" DataValueField="EmployeeID" OnDataBound="EmployeeDropDownList_DataBound">
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="Emp_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EmployeeID,FirstName + ' ' + LastName AS Name FROM VW_Emp_Info WHERE (SchoolID = @SchoolID) AND (Job_Status = N'Active') AND (EmployeeType LIKE @EmployeeType)">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                        <div class="form-group">
                            <asp:DropDownList ID="AttenDropDownList" runat="server" CssClass="form-control" AutoPostBack="True">
                                <asp:ListItem Value="%">All Attendance</asp:ListItem>
                                <asp:ListItem Value="Pre">Present</asp:ListItem>
                                <asp:ListItem>Late</asp:ListItem>
                                <asp:ListItem>Late Abs</asp:ListItem>
                                <asp:ListItem Value="Abs">Absence</asp:ListItem>
                                <asp:ListItem>Leave</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>

                    <label class="Emp_Att"></label>
                    <asp:Label ID="RecordLabel" runat="server"></asp:Label>
                    <asp:DataList ID="Atten_Status_DataList" CssClass="pull-right" runat="server" DataSourceID="Att_Status_SQL" RepeatDirection="Horizontal">
                        <ItemTemplate>
                            <div class="Atten_Count">
                                <asp:Label ID="AttendanceStatusLabel" runat="server" Text='<%# Bind("AttendanceStatus") %>' />
                                <asp:Label ID="Days_Count_Label" runat="server" Text='<%# Bind("Total") %>' />
                                day(s)
                            </div>
                        </ItemTemplate>
                    </asp:DataList>
                    <asp:SqlDataSource ID="Att_Status_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT  AttendanceStatus, COUNT(EmployeeID) AS Total,  CASE WHEN AttendanceStatus = 'Pre' THEN 1 WHEN AttendanceStatus = 'Late' THEN 2 WHEN AttendanceStatus = 'Late Abs' THEN 3 WHEN AttendanceStatus = 'Abs' THEN 4 WHEN AttendanceStatus = 'Leave' THEN 5 END AS Ascending
FROM  Employee_Attendance_Record WHERE (SchoolID = @SchoolID) AND (AttendanceDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND (EmployeeID = @EmployeeID) AND (ISNULL(@ScheduleIDs, N'') = N'' OR CHARINDEX(N',' + CAST(ISNULL(Attendance_ScheduleID, 0) AS nvarchar(12)) + N',', @ScheduleIDs) > 0) GROUP BY AttendanceStatus,CASE WHEN AttendanceStatus = 'Pre' THEN 1 WHEN AttendanceStatus = 'Late' THEN 2 WHEN AttendanceStatus = 'Late Abs' THEN 3 WHEN AttendanceStatus = 'Abs' THEN 4 WHEN AttendanceStatus = 'Leave' THEN 5 END
ORDER BY Ascending" CancelSelectOnNullParameter="False">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="EmployeeDropDownList" Name="EmployeeID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                            <asp:Parameter Name="ScheduleIDs" DefaultValue="" ConvertEmptyStringToNull="False" Type="String" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                    <div class="table-responsive">
                        <asp:GridView ID="EmployeeAttRecordGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="Employee_Attendance_RecordID" DataSourceID="EmployeeAttendanceSQL" AllowPaging="True" PageSize="500" AllowSorting="True" OnRowDataBound="EmployeeAttRecordGridView_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
                                <asp:BoundField DataField="EmployeeType" HeaderText="Type" SortExpression="EmployeeType" />
                                <asp:BoundField DataField="AttendanceStatus" HeaderText="Attendance" SortExpression="AttendanceStatus" />
                                <asp:BoundField DataField="AttendanceDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Attendance Date" SortExpression="AttendanceDate" />
                                <asp:BoundField DataField="EntryTime" HeaderText="Entry Time" SortExpression="EntryTime" />
                                <asp:BoundField DataField="ExitTime" HeaderText="Exit Time" SortExpression="ExitTime" />
                                <asp:BoundField DataField="ExitStatus" HeaderText="Exit Status" SortExpression="ExitStatus" />
                            </Columns>
                            <EmptyDataTemplate>
                                No Record !
                            </EmptyDataTemplate>
                            <PagerStyle CssClass="pgr" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="EmployeeAttendanceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT Employee_Attendance_Record.Employee_Attendance_RecordID, Employee_Attendance_Record.SchoolID, Employee_Attendance_Record.AttendanceStatus, Employee_Attendance_Record.AttendanceDate, CASE WHEN Employee_Attendance_Record.EntryTime IS NULL OR Employee_Attendance_Record.EntryTime = '00:00:00' THEN '' ELSE CONVERT (varchar(15), Employee_Attendance_Record.EntryTime, 100) END AS EntryTime, CASE WHEN Employee_Attendance_Record.ExitTime IS NULL OR Employee_Attendance_Record.ExitTime = '00:00:00' THEN '' ELSE CONVERT (varchar(15), Employee_Attendance_Record.ExitTime, 100) END AS ExitTime, Employee_Attendance_Record.CreatedDate, Employee_Attendance_Record.ExitStatus, VW_Emp_Info.ID, VW_Emp_Info.EmployeeType, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.Designation FROM Employee_Attendance_Record INNER JOIN VW_Emp_Info ON Employee_Attendance_Record.EmployeeID = VW_Emp_Info.EmployeeID WHERE (Employee_Attendance_Record.SchoolID = @SchoolID) AND (VW_Emp_Info.EmployeeType LIKE @EmployeeType) AND (Employee_Attendance_Record.AttendanceStatus LIKE @AttendanceStatus) AND (Employee_Attendance_Record.AttendanceDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND ((Employee_Attendance_Record.EmployeeID = @EmployeeID) OR (@EmployeeID = 0)) AND (ISNULL(@ScheduleIDs, N'') = N'' OR CHARINDEX(N',' + CAST(ISNULL(Employee_Attendance_Record.Attendance_ScheduleID, 0) AS nvarchar(12)) + N',', @ScheduleIDs) > 0) ORDER BY Name, Employee_Attendance_Record.AttendanceDate"
                            CancelSelectOnNullParameter="False" OnSelected="EmployeeAttendanceSQL_Selected">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                <asp:ControlParameter ControlID="EmployeeDropDownList" Name="EmployeeID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="AttenDropDownList" Name="AttendanceStatus" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                                <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                                <asp:Parameter Name="ScheduleIDs" DefaultValue="" ConvertEmptyStringToNull="False" Type="String" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <div id="tab2" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="table-responsive">
                        <asp:GridView ID="Employee_Att_Summary_GridView" OnRowDataBound="Employee_Att_Summary_GridView_RowDataBound" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="Employee_Att_Summary_SQL" AllowSorting="True">
                            <Columns>
                                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                                <asp:BoundField DataField="Name" HeaderText="Name" ReadOnly="True" SortExpression="Name" />
                                <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
                                <asp:BoundField DataField="EmployeeType" HeaderText="Type" SortExpression="EmployeeType" />
                                <asp:BoundField DataField="Pre" HeaderText="Pre" ReadOnly="True" SortExpression="Pre" />
                                <asp:BoundField DataField="Late" HeaderText="Late" ReadOnly="True" SortExpression="Late" />
                                <asp:BoundField DataField="Late Abs" HeaderText="Late Abs" ReadOnly="True" SortExpression="Late Abs" />
                                <asp:BoundField DataField="Abs" HeaderText="Abs" ReadOnly="True" SortExpression="Abs" />
                                <asp:BoundField DataField="Leave" HeaderText="Leave" ReadOnly="True" SortExpression="Leave" />
                            </Columns>
                            <EmptyDataTemplate>
                                No Record !
                            </EmptyDataTemplate>
                        </asp:GridView>
                        <asp:SqlDataSource CancelSelectOnNullParameter="False" ID="Employee_Att_Summary_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT *,
(SELECT COUNT(EmployeeID) FROM Employee_Attendance_Record WHERE (SchoolID = @SchoolID) AND (AttendanceDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND AttendanceStatus = 'Pre' AND EmployeeID = t.EmployeeID AND (ISNULL(@ScheduleIDs, N'') = N'' OR CHARINDEX(N',' + CAST(ISNULL(Attendance_ScheduleID, 0) AS nvarchar(12)) + N',', @ScheduleIDs) > 0))  AS Pre,
(SELECT COUNT(EmployeeID) FROM Employee_Attendance_Record WHERE (SchoolID = @SchoolID) AND (AttendanceDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND AttendanceStatus = 'Late' AND EmployeeID = t.EmployeeID AND (ISNULL(@ScheduleIDs, N'') = N'' OR CHARINDEX(N',' + CAST(ISNULL(Attendance_ScheduleID, 0) AS nvarchar(12)) + N',', @ScheduleIDs) > 0))  AS Late,
(SELECT COUNT(EmployeeID) FROM Employee_Attendance_Record WHERE (SchoolID = @SchoolID) AND (AttendanceDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND AttendanceStatus = 'Late Abs' AND EmployeeID = t.EmployeeID AND (ISNULL(@ScheduleIDs, N'') = N'' OR CHARINDEX(N',' + CAST(ISNULL(Attendance_ScheduleID, 0) AS nvarchar(12)) + N',', @ScheduleIDs) > 0))  AS [Late Abs],
(SELECT COUNT(EmployeeID) FROM Employee_Attendance_Record WHERE (SchoolID = @SchoolID) AND (AttendanceDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND AttendanceStatus = 'Abs' AND EmployeeID = t.EmployeeID AND (ISNULL(@ScheduleIDs, N'') = N'' OR CHARINDEX(N',' + CAST(ISNULL(Attendance_ScheduleID, 0) AS nvarchar(12)) + N',', @ScheduleIDs) > 0))  AS [Abs],
(SELECT COUNT(EmployeeID) FROM Employee_Attendance_Record WHERE (SchoolID = @SchoolID) AND (AttendanceDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND AttendanceStatus = 'Leave' AND EmployeeID = t.EmployeeID AND (ISNULL(@ScheduleIDs, N'') = N'' OR CHARINDEX(N',' + CAST(ISNULL(Attendance_ScheduleID, 0) AS nvarchar(12)) + N',', @ScheduleIDs) > 0))  AS Leave
FROM (SELECT DISTINCT VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.ID, VW_Emp_Info.Designation, VW_Emp_Info.EmployeeType, VW_Emp_Info.EmployeeID
FROM Employee_Attendance_Record INNER JOIN VW_Emp_Info ON Employee_Attendance_Record.EmployeeID = VW_Emp_Info.EmployeeID
WHERE (Employee_Attendance_Record.AttendanceDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND (VW_Emp_Info.Job_Status = N'Active') AND (VW_Emp_Info.EmployeeType LIKE @EmployeeType) AND (VW_Emp_Info.SchoolID = @SchoolID) AND (ISNULL(@ScheduleIDs, N'') = N'' OR CHARINDEX(N',' + CAST(ISNULL(Employee_Attendance_Record.Attendance_ScheduleID, 0) AS nvarchar(12)) + N',', @ScheduleIDs) > 0)) AS T">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                                <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />

                                <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
                                <asp:Parameter Name="ScheduleIDs" DefaultValue="" ConvertEmptyStringToNull="False" Type="String" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <button type="button" class="btn btn-primary d-print-none" onclick="window.print()">Print</button>

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


    <script type="text/javascript">
        // multi-schedule picker: none / all ticked = all schedules
        function schMsBoxes($r) { return $r.find('.sch-ms-list input[type=checkbox]'); }
        function schMsState($r) {
            var boxes = schMsBoxes($r), on = boxes.filter(':checked');
            if (on.length === boxes.length) return '';
            return on.map(function () { return this.value; }).get().join(',');
        }
        function schMsNames() {
            var $r = $('.sch-ms').first(), boxes = schMsBoxes($r), on = boxes.filter(':checked');
            if (on.length === 0 || on.length === boxes.length) return [];
            return on.map(function () { return $.trim($r.find('label[for="' + this.id + '"]').text()); }).get();
        }
        function schMsUpdate() {
            $('.sch-ms').each(function () {
                var $r = $(this), boxes = schMsBoxes($r), on = boxes.filter(':checked'), t;
                if (on.length === 0 || on.length === boxes.length) t = '[ ALL SCHEDULE ]';
                else if (on.length === 1) t = $.trim($r.find('label[for="' + on[0].id + '"]').text());
                else t = on.length + ' schedules selected';
                $r.find('.sch-ms-text').text(t);
                $r.find('.sch-ms-btn').attr('title', schMsNames().join(', '));
                $r.find('.sch-ms-count').text(on.length + ' / ' + boxes.length);
            });
        }
        function schMsToggle(btn) {
            var $r = $(btn).closest('.sch-ms');
            if ($r.hasClass('open')) { schMsClose(); return; }
            $r.addClass('open').data('start', schMsState($r));
        }
        function schMsAll(a, on) {
            schMsBoxes($(a).closest('.sch-ms')).prop('checked', on);
            schMsUpdate();
            return false;
        }
        function schMsClose() {
            var changed = false;
            $('.sch-ms.open').each(function () {
                var $r = $(this);
                $r.removeClass('open');
                if (schMsState($r) !== $r.data('start')) changed = true;
            });
            if (changed) {
                var b = document.querySelector('[id$="SubmitButton"]');
                if (b) b.click();
            }
        }
        $(document).on('change', '.sch-ms-list input[type=checkbox]', schMsUpdate);
        $(document).on('mousedown touchstart', function (e) {
            if ($('.sch-ms.open').length && !$(e.target).closest('.sch-ms').length) schMsClose();
        });
        $(document).on('keydown', function (e) { if (e.keyCode === 27 && $('.sch-ms.open').length) schMsClose(); });
        $(function () { schMsUpdate(); });
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () { schMsUpdate(); });
    </script>
    <script type="text/javascript">
        $(function () {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            })

            //get date in label
            var from = $("[id*=FromDateTextBox]").val();
            var To = $("[id*=ToDateTextBox]").val();

            var tt;
            var Brases1 = "";
            var Brases2 = "";
            var A = "";
            var B = "";
            var TODate = "";

            if (To == "" || from == "" || To == "" && from == "") {
                tt = "";
                A = "";
                B = "";
            }
            else {
                tt = " To ";
                Brases1 = "(";
                Brases2 = ")";
            }

            if (To == "" && from == "") { Brases1 = ""; }

            if (To == from) {
                TODate = "";
                tt = "";
                var Brases1 = "";
                var Brases2 = "";
            }
            else { TODate = To; }

            if (from == "" && To != "") {
                B = " Before ";
            }

            if (To == "" && from != "") {
                A = " After ";
            }

            if (from != "" && To != "") {
                A = "";
                B = "";
            }

            var Employee = "";
            if ($('[id*=EmployeeDropDownList] :selected').index() > 0) {
                Employee = $('[id*=EmployeeDropDownList] :selected').text() + ".";
            }

            var schNames = schMsNames();
            var Schedule = schNames.length ? " Schedule: " + schNames.join(", ") : "";

            $(".Date").text(Brases1 + B + A + from + tt + TODate + Brases2);
            $(".SchedulePrint").text(Schedule);
            $(".Emp_Att").text(Employee);
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            //get date in label
            var from = $("[id*=FromDateTextBox]").val();
            var To = $("[id*=ToDateTextBox]").val();

            var tt;
            var Brases1 = "";
            var Brases2 = "";
            var A = "";
            var B = "";
            var TODate = "";

            if (To == "" || from == "" || To == "" && from == "") {
                tt = "";
                A = "";
                B = "";
            }
            else {
                tt = " To ";
                Brases1 = "(";
                Brases2 = ")";
            }

            if (To == "" && from == "") { Brases1 = ""; }

            if (To == from) {
                TODate = "";
                tt = "";
                var Brases1 = "";
                var Brases2 = "";
            }
            else { TODate = To; }

            if (from == "" && To != "") {
                B = " Before ";
            }

            if (To == "" && from != "") {
                A = " After ";
            }

            if (from != "" && To != "") {
                A = "";
                B = "";
            }

            var Employee = "";
            if ($('[id*=EmployeeDropDownList] :selected').index() > 0) {
                Employee = $('[id*=EmployeeDropDownList] :selected').text() + ".";
            }

            var schNames = schMsNames();
            var Schedule = schNames.length ? " Schedule: " + schNames.join(", ") : "";

            $(".Date").text(Brases1 + B + A + from + tt + TODate + Brases2);
            $(".SchedulePrint").text(Schedule);
            $(".Emp_Att").text(Employee);
        })
    </script>
</asp:Content>
