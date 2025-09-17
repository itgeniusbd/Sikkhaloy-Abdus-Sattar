<%@ Page Title="Sent SMS Records" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Sent_SMS_Records.aspx.cs" Inherits="EDUCATION.COM.SMS.Sent_SMS_Records" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .range_inputs { display:none;}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:FormView ID="SMSFormView" runat="server" DataKeyNames="SMSID" DataSourceID="SMSSQL" Width="100%" CssClass="NoPrint">
        <ItemTemplate>
            <h3>Sent SMS Records <small>Remaining SMS: <%#Eval("SMS_Balance") %></small></h3>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="SMSSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [SMS] WHERE ([SchoolID] = @SchoolID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <div class="NoPrint form-inline">
        <div class="form-group">
            <asp:TextBox ID="FromDateTextBox" runat="server" CssClass="form-control Datetime" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control Datetime" placeholder="To Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar"></i>
        </div>
        <div class="form-group">
            <asp:TextBox ID="Phone_Purpose_TextBox" placeholder="Mobile No, Purpose" runat="server" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find" ValidationGroup="1" />
        </div>
    </div>

    <asp:FormView ID="FormView1" runat="server" DataSourceID="CountSQL" Width="100%">
        <ItemTemplate>
            <div class="alert alert-primary">
                <span class="Date"></span>
                Total Sent SMS: <%# Eval("Total_SENT") %>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="CountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ISNULL(SUM(SMS_Send_Record.SMSCount),0) AS Total_SENT FROM SMS_Send_Record INNER JOIN SMS_OtherInfo ON SMS_Send_Record.SMS_Send_ID = SMS_OtherInfo.SMS_Send_ID WHERE (SMS_OtherInfo.SchoolID = @SchoolID)  AND (SMS_Send_Record.PurposeOfSMS like @PurposeOfSMS) AND (CAST(SMS_Send_Record.Date AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))" CancelSelectOnNullParameter="False">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <%-- <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" /> --%>
            <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="Phone_Purpose_TextBox" DefaultValue="%" Name="PurposeOfSMS" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>

    <div class="table-responsive">
        <asp:GridView ID="SMSRecordsGridView" runat="server" AutoGenerateColumns="False" ShowHeaderWhenEmpty="True" EmptyDataText="No records Found!"
            DataSourceID="SMSRecordsSQL" AllowPaging="True" AllowSorting="True" PageSize="50" CssClass="mGrid">
            <Columns>
                <asp:BoundField DataField="PhoneNumber" HeaderText="Phone No" SortExpression="PhoneNumber" />
                <asp:BoundField DataField="TextSMS" HeaderText="Text SMS" SortExpression="TextSMS" >
                <ItemStyle HorizontalAlign="Left" />
                </asp:BoundField>
                <asp:BoundField DataField="TextCount" HeaderText="Text Count" SortExpression="TextCount" />
                <asp:BoundField DataField="SMSCount" HeaderText="SMS Count" SortExpression="SMSCount" />
                <asp:BoundField DataField="PurposeOfSMS" HeaderText="Purpose Of SMS" SortExpression="PurposeOfSMS" />
                <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date"
                    DataFormatString="{0:dd-MMM-yy (h:mm tt)}">
                    <HeaderStyle Width="150px" />
                </asp:BoundField>
            </Columns>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
        <asp:SqlDataSource ID="SMSRecordsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT SMS_Send_Record.SMS_Send_ID, SMS_Send_Record.PhoneNumber, SMS_Send_Record.TextSMS, SMS_Send_Record.TextCount, SMS_Send_Record.SMSCount, SMS_Send_Record.PurposeOfSMS, SMS_Send_Record.SMS_Response, SMS_Send_Record.Status, SMS_Send_Record.Date FROM SMS_Send_Record INNER JOIN SMS_OtherInfo ON SMS_Send_Record.SMS_Send_ID = SMS_OtherInfo.SMS_Send_ID WHERE (SMS_OtherInfo.SchoolID = @SchoolID) AND (CAST(SMS_Send_Record.Date AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) ORDER BY SMS_Send_Record.Date DESC"
            FilterExpression="PhoneNumber LIKE '{0}%'or PurposeOfSMS LIKE '{0}%'" CancelSelectOnNullParameter="False">
            <FilterParameters>
                <asp:ControlParameter ControlID="Phone_Purpose_TextBox" Name="Phone_Purpose" PropertyName="Text" />
            </FilterParameters>
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <input type="button" value="Print" class="btn btn-grey btn-sm NoPrint" onclick="window.print()"/>

    <script>
        $(function () {
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

            $(".Date").text(Brases1 + B + A + from + tt + TODate + Brases2);

            //Date range picker
            function cb(start, end) {
                $('[id*=FromDateTextBox]').val(start.format('D MMMM YYYY'));
                $('[id*=ToDateTextBox]').val(end.format('D MMMM YYYY'));

                $("[id*=FindButton]").trigger("click");
            }

            $('#PickDate').daterangepicker({
                autoApply: true,
                showCustomRangeLabel: false,
                ranges: {
                    'Today': [moment(), moment()],
                    'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                    'Last 7 Days': [moment().subtract(6, 'days'), moment()],
                    'Last 30 Days': [moment().subtract(29, 'days'), moment()],
                    'This Month': [moment().startOf('month'), moment().endOf('month')],
                    'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
                    'This Year': [moment().startOf('year'), moment().endOf('year')]
                }
            }, cb);

//            cb(start, end);
        });
    </script>
</asp:Content>
