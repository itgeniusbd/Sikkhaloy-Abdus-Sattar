<%@ Page Title="Add Holidays" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Add_Holidays.aspx.cs" Inherits="EDUCATION.COM.Employee.Add_Holidays" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Acadamic_Calender.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div id="H_Day">
        <h3>Holidays Management</h3>
        <a href="Acadamic_Calender.aspx">Academic Calender</a><br />

        <div class="card mb-4">
            <div class="card-header">
                Weekly Holiday(s)
            </div>
            <div class="card-body">
                <em class="text-success">Select Start Date - End Date of Your Session And Select Day Of Weekly Holiday</em>

                <div class="form-group">
                    <label>
                        Start Date
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="StartDateTextBox" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="W"></asp:RequiredFieldValidator></label>
                    <asp:TextBox ID="StartDateTextBox" runat="server" CssClass="Datetime form-control" placeholder="Start Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>
                        End Date
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="EndDateTextBox" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="W"></asp:RequiredFieldValidator></label>
                    <asp:TextBox ID="EndDateTextBox" runat="server" CssClass="Datetime form-control" placeholder="End Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Day</label>
                    <asp:CheckBoxList ID="WeekCheckBoxList" runat="server" RepeatDirection="Horizontal" CssClass="form-control">
                        <asp:ListItem Value="Saturday">Sat</asp:ListItem>
                        <asp:ListItem Value="Sunday">Sun</asp:ListItem>
                        <asp:ListItem Value="Monday">Mon</asp:ListItem>
                        <asp:ListItem Value="Tuesday">Tue</asp:ListItem>
                        <asp:ListItem Value="Wednesday">Wed</asp:ListItem>
                        <asp:ListItem Value="Thursday">Thu</asp:ListItem>
                        <asp:ListItem Value="Friday">Fri</asp:ListItem>
                    </asp:CheckBoxList>
                </div>
                <div class="form-group">
                    <asp:Button ID="WeeklyButton" runat="server" Text="Add Weekly Holiday" CssClass="btn btn-info" OnClick="WeeklyButton_Click" ValidationGroup="W" />
                </div>
            </div>
        </div>

        <div class="card mb-4">
            <div class="card-header">
                Multiple Holidays
            </div>

            <div class="card-body">
                <em class="text-success">Provide Holiday Name And Select From Date - To Date</em>
                <div class="form-inline">
                    <div class="form-group">
                        <asp:TextBox ID="Multi_HoliNameTextBox" placeholder="Holiday Name" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group mx-2">
                        <asp:TextBox ID="Multi_FromDateTextBox" placeholder="From Date" runat="server" CssClass="Datetime form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:TextBox ID="Multi_ToDateTextBox" placeholder="To Date" runat="server" CssClass="Datetime form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:Button ID="Multi_HolidayButton" runat="server" CssClass="btn btn-success" OnClick="Multi_HolidayButton_Click" Text="Save" />
                    </div>
                </div>
            </div>
        </div>

        <div class="card mb-4">
            <div class="card-header">
                Single Holiday(s)
            </div>

            <div class="card-body">
                 <em class="text-success">Make Holiday List By Providing Holiday Name And Holiday Date And Click Save To Finish</em>
                <div class="form-inline">
                    <div class="form-group">
                        <asp:TextBox ID="HolidayNameTextBox" placeholder="Holiday Name" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group mx-2">
                        <asp:TextBox ID="HolidateDateTextBox" placeholder="Date" runat="server" CssClass="Datetime form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <input id="ChartButton" type="button" value="Add To List" class="btn btn-danger" />
                    </div>
                </div>

                <asp:GridView ID="HolidaysGridview" CssClass="mGrid" runat="server" AutoGenerateColumns="False">
                    <Columns>
                        <asp:TemplateField HeaderText="Name">
                            <ItemTemplate>
                                <%# Eval("HolidayName") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Date">
                            <ItemTemplate>
                                <%# Eval("Date")%>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <a id="Del">Delete</a>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

                <div class="form-group">
                    <br />
                    <asp:Button ID="SaveButton" runat="server" CssClass="btn btn-danger" OnClick="SaveButton_Click" Text="Save" />
                </div>
            </div>
        </div>

        <asp:SqlDataSource ID="HolidaySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand=" IF NOT EXISTS(SELECT * FROM Employee_Holiday WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (HolidayDate = @HolidayDate))
 BEGIN
INSERT INTO Employee_Holiday(SchoolID, RegistrationID, EducationYearID, HolidayName, HolidayDate) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @HolidayName, @HolidayDate)
END"
            SelectCommand="SELECT * FROM [Employee_Holiday]" DeleteCommand="DELETE FROM Employee_Holiday WHERE (HolidayID = @HolidayID)" UpdateCommand=" IF NOT EXISTS(SELECT * FROM Employee_Holiday WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (HolidayDate = @HolidayDate))
 BEGIN
UPDATE Employee_Holiday SET HolidayName = @HolidayName, HolidayDate = @HolidayDate WHERE (HolidayID = @HolidayID)
END">
            <DeleteParameters>
                <asp:Parameter Name="HolidayID" />
            </DeleteParameters>
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                <asp:Parameter DbType="Date" Name="HolidayDate" />
                <asp:Parameter Name="HolidayName" Type="String" />
            </InsertParameters>
            <UpdateParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:Parameter Name="HolidayID" />
                <asp:Parameter Name="HolidayDate" />
                <asp:Parameter Name="HolidayName" />
            </UpdateParameters>
        </asp:SqlDataSource>


        <asp:UpdatePanel ID="ContainUpdatecard" runat="server">
            <ContentTemplate>
                <div class="calendarWrapper">
                    <b>Click On Date To Edit Or Delete Holiday</b>
                    <asp:Calendar ID="HolidayCalendar" OnDayRender="HolidayCalendar_DayRender" runat="server" NextMonthText="." PrevMonthText="." SelectMonthText="»" SelectWeekText="›" CellPadding="0" CssClass="myCalendar" Width="100%" FirstDayOfWeek="Saturday">
                        <DayStyle CssClass="myCalendarDay" />
                        <DayHeaderStyle CssClass="myCalendarDayHeader" ForeColor="#2d3338" />
                        <SelectedDayStyle CssClass="myCalendarSelector" BackColor="#FE5815" />
                        <TodayDayStyle CssClass="myCalendarToday" />
                        <SelectorStyle CssClass="myCalendarSelector" />
                        <NextPrevStyle CssClass="myCalendarNextPrev" />
                        <TitleStyle CssClass="myCalendarTitle" />
                    </asp:Calendar>
                </div>
                <asp:FormView ID="EdtFormView" runat="server" DataSourceID="EditHolidaySQL" DataKeyNames="HolidayID" DefaultMode="Edit" Width="100%">
                    <EditItemTemplate>
                        <div class="H_Date">
                            <asp:Label ID="HolidayDateLabel" runat="server" Text='<%# Bind("HolidayDate", "{0:d MMM yyyy}") %>' />
                        </div>

                        <asp:TextBox ID="HolidayNameTextBox" TextMode="MultiLine" CssClass="form-control" runat="server" Text='<%# Bind("HolidayName") %>' />

                        <asp:LinkButton ID="UpdateButton" CssClass="btn btn-primary" runat="server" CausesValidation="True" CommandName="Update" Text="Update" />
                        <asp:LinkButton ID="DeleteButton" runat="server" CssClass="btn btn-primary" CausesValidation="False" CommandName="Delete" OnClientClick="return confirm('Holiday Will be deleted Permanently')" Text="Delete" />
                    </EditItemTemplate>
                </asp:FormView>
                <asp:SqlDataSource ID="EditHolidaySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT HolidayID, SchoolID, RegistrationID, EducationYearID, HolidayName, HolidayDate, CreateDate FROM Employee_Holiday WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (HolidayDate = @HolidayDate)" DeleteCommand="DELETE FROM [Employee_Holiday] WHERE [HolidayID] = @HolidayID" UpdateCommand="UPDATE Employee_Holiday SET HolidayName = @HolidayName, HolidayDate = @HolidayDate WHERE (HolidayID = @HolidayID)">
                    <DeleteParameters>
                        <asp:Parameter Name="HolidayID" Type="Int32" />
                    </DeleteParameters>
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="HolidayCalendar" Name="HolidayDate" PropertyName="SelectedDate" />
                    </SelectParameters>
                    <UpdateParameters>
                        <asp:Parameter Name="HolidayName" Type="String" />
                        <asp:Parameter DbType="Date" Name="HolidayDate" />
                        <asp:Parameter Name="HolidayID" Type="Int32" />
                    </UpdateParameters>
                </asp:SqlDataSource>
            </ContentTemplate>
        </asp:UpdatePanel>
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
            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            if ($("[id*=HolidaysListGridView] tr").length)
                $("#Show").show();


            $("[id*=ChartButton]").click(function () {
                //Reference the GridView.
                var gridView = $("[id*=HolidaysGridview]");

                //Reference the first row.
                var row = gridView.find("tr").eq(1);

                //Check if row is dummy, if yes then remove.
                if ($.trim(row.find("td").eq(0).html()) == "") {
                    row.remove();
                }

                //Clone the reference first row.
                row = row.clone(true);

                //Add first cell.
                var HolidayName = $("[id*=HolidayNameTextBox]");
                if (HolidayName.val() != "")
                    SetValue(row, 0, "HolidayName", HolidayName);

                //Add second cell.
                var HolidateDate = $("[id*=HolidateDateTextBox]");
                if (HolidateDate.val() != "")
                    SetValue(row, 1, "Date", HolidateDate);

                //Add the row to the GridView.
                if (HolidayName.val() != "" && HolidateDate.val() != "") {
                    gridView.append(row);
                    HolidayName.val("");
                    HolidateDate.val("");
                }

                return false;
            });

            function SetValue(row, index, name, textbox) {
                //Reference the Cell and set the value.
                row.find("td").eq(index).html(textbox.val());

                //Create and add a Hidden Field to send value to server. 
                var input = $("<input type = 'hidden' />");
                input.prop("name", name);
                input.val(textbox.val());
                row.find("td").eq(index).append(input);
            }

            $("#Del").live("click", function () {
                var row = $(this).closest("tr");
                if ($("[id*=HolidaysGridview] tr").length <= 2) {
                    $.trim(row.find("td").eq(0).html(""));
                    $.trim(row.find("td").eq(1).html(""));
                }
                else {
                    row.remove();
                }
            })
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
