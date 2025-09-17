<%@ Page Title="Account Summary" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="AccountDetails.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.AccountDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .range_inputs { display: none; }
    </style>
    <link href="CSS/Account_Details.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Account Summary <small class="Date"></small></h3>
    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="AccountDropDownList" runat="server" CssClass="form-control" DataSourceID="AccountListSQL" DataTextField="AccountName" DataValueField="AccountID" AppendDataBoundItems="True" AutoPostBack="True">
                <asp:ListItem Value="%">All Account</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="AccountListSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [AccountID], [AccountName] FROM [Account] WHERE ([SchoolID] = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:TextBox ID="From_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:TextBox ID="To_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
            <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar"></i>
        </div>
        <div class="form-group">
            <asp:Button ID="Find_Button" CssClass="btn btn-primary" runat="server" Text="Find" />
        </div>
        <div class="form-group d-print-none">
            <a title="Print This Page" onclick="window.print();"><i class="fa fa-print"></i></a>
        </div>
    </div>

    <asp:Repeater ID="AccountRepeater" runat="server" DataSourceID="AccountSQL">
        <ItemTemplate>
            <div id="b-Section">
                <asp:HiddenField ID="AccountID_HF" runat="server" Value='<%# Eval("AccountID") %>' />
                <div class="row">
                    <div class="col-lg-4">
                        <div class="user-grid Account-bg clearfix">
                            <div class="pull-left acc-name">
                                <i class="fa fa-file-text"></i>
                                <%# Eval("AccountName") %>
                            </div>

                            <div class="pull-right acc-name">
                                <div class="find-date">
                                    CURRENT BALANCE
                                </div>
                                <div>
                                    <i class="fa fa-money"></i>
                                    <%# Eval("AccountBalance","{0:n0}") %> tk
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-lg-4">
                        <div class="user-grid In_Out-bg clearfix">
                            <div class="pull-left">
                                <div class="headline">
                                    <i class="fa fa-plus-circle"></i>
                                    Total Add
                                </div>

                                <div class="value">
                                    <i class="fa fa-caret-up"></i>
                                    <%# Eval("Total_In","{0:n0}") %> tk
                                </div>
                            </div>
                            <div class="pull-right">
                                <div class="headline">
                                    <i class="fa fa-minus-circle"></i>
                                    Total Subtract
                                </div>

                                <div class="value">
                                    <i class="fa fa-caret-down"></i>
                                    <%# Eval("Total_Ex","{0:n0}") %> tk
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="col-lg-4">
                        <div class="user-grid Bef-after-bg clearfix">
                            <div class="pull-left">
                                <div class="headline">
                                    <i class="fa fa-caret-left"></i>
                                    Opening balance
                           <p class="From_date"></p>
                                </div>
                                <div class="value">
                                    <i class="fa fa-money"></i>
                                    <%# Eval("Balance_Before","{0:n0}") %> tk
                                </div>
                            </div>
                            <div class="pull-right">
                                <div class="headline">
                                    <i class="fa fa-caret-right"></i>
                                    Closing balance
                           <p class="To_date"></p>
                                </div>
                                <div class="value" style="text-align: right;">
                                    <i class="fa fa-money"></i>
                                    <%# Eval("Balance_After","{0:n0}") %> tk
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <section>
                    <div class="box b-add">
                        <i class="fa fa-plus-circle"></i>
                        Add Balance
                    </div>
                    <asp:GridView ID="Add_B_GridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" DataSourceID="Add_B_SQL" CssClass="mGrid">
                        <Columns>
                            <asp:BoundField DataField="SubCategory" HeaderText="Category" SortExpression="SubCategory" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" SortExpression="Amount" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="Add_B_SQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT   SubCategory, SUM(Amount) AS Amount FROM Account_Log  WHERE (SchoolID = @SchoolID) AND (AccountID = @AccountID) AND Add_Subtraction = 'Add' AND (In_Ex_type = 'In') AND
Insert_Date between ISNULL(@From_Date, '1-1-1000') and ISNULL(@To_Date,'1-1-3000') GROUP BY   SubCategory">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="AccountID_HF" Name="AccountID" PropertyName="Value" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </section>
                <section>
                    <div class="box b-add-adjt">
                        <i class="fa fa-exchange"></i>
                        Add Balance by Adjustment
                    </div>
                    <asp:GridView ID="Add_B_Adj_GridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" DataSourceID="Add_B_Adj_SQL" CssClass="mGrid">
                        <Columns>
                            <asp:BoundField DataField="SubCategory" HeaderText="Category" SortExpression="SubCategory" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" SortExpression="Amount" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="Add_B_Adj_SQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SubCategory,SUM(Amount) AS Amount FROM Account_Log  WHERE (SchoolID = @SchoolID) AND (AccountID = @AccountID) AND Add_Subtraction = 'Add' AND (In_Ex_type &lt;&gt; 'In') AND
Insert_Date between ISNULL(@From_Date, '1-1-1000') and ISNULL(@To_Date,'1-1-3000') GROUP BY SubCategory">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="AccountID_HF" Name="AccountID" PropertyName="Value" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </section>
                <section>
                    <div class="box b-subs">
                        <i class="fa fa-minus-circle"></i>
                        Subtract Balance
                    </div>
                    <asp:GridView ID="Sub_B_GridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" DataSourceID="Sub_B_SQL" CssClass="mGrid">
                        <Columns>
                            <asp:BoundField DataField="SubCategory" HeaderText="Category" SortExpression="SubCategory" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" SortExpression="Amount" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="Sub_B_SQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SubCategory,SUM(Amount) AS Amount FROM  Account_Log  WHERE (SchoolID = @SchoolID) AND (AccountID = @AccountID) AND Add_Subtraction = 'Subtraction' AND (In_Ex_type = 'Ex') AND
Insert_Date between ISNULL(@From_Date, '1-1-1000') and ISNULL(@To_Date,'1-1-3000') GROUP BY SubCategory">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="AccountID_HF" Name="AccountID" PropertyName="Value" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </section>
                <section>
                    <div class="box b-subs-adjt">
                        <i class="fa fa-exchange"></i>
                        Subtract Balance by Adjustment
                    </div>
                    <asp:GridView ID="Sub_B_Adj_GridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" DataSourceID="Sub_B_Adj_SQL" CssClass="mGrid">
                        <Columns>
                            <asp:BoundField DataField="SubCategory" HeaderText="Category" SortExpression="SubCategory" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" SortExpression="Amount" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="Sub_B_Adj_SQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SubCategory,SUM(Amount) AS Amount FROM  Account_Log 
WHERE (SchoolID = @SchoolID) AND (AccountID = @AccountID) AND Add_Subtraction = 'Subtraction'  AND (In_Ex_type &lt;&gt; 'Ex') AND
Insert_Date between ISNULL(@From_Date, '1-1-1000') and ISNULL(@To_Date,'1-1-3000') GROUP BY   SubCategory">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="AccountID_HF" Name="AccountID" PropertyName="Value" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </section>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <asp:SqlDataSource ID="AccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * from (Select AccountID,AccountName, AccountBalance,ISNULL((SELECT SUM(Amount) FROM  Account_Log WHERE  (SchoolID = @SchoolID) AND (AccountID = Account.AccountID) AND Add_Subtraction = 'Add' and Insert_Date between ISNULL(@From_Date, '1-1-1000') and ISNULL(@To_Date,'1-1-3000')),0) as Total_In, ISNULL((SELECT SUM(Amount) FROM  Account_Log WHERE  (SchoolID = @SchoolID) AND (AccountID = Account.AccountID) AND  Add_Subtraction = 'Subtraction' and Insert_Date between ISNULL(@From_Date, '1-1-1000') and ISNULL(@To_Date,'1-1-3000')),0) as Total_Ex,(SELECT top(1) Balance_Before FROM Account_Log WHERE (SchoolID = @SchoolID) and (AccountID = Account.AccountID) and  Insert_Date between ISNULL(@From_Date, '1-1-1000') and ISNULL(@To_Date,'1-1-3000') ORDER BY Insert_Date, Insert_Time) AS Balance_Before,(SELECT top(1) Balance_After FROM Account_Log WHERE (SchoolID = @SchoolID) and (AccountID = Account.AccountID) and Insert_Date between ISNULL(@From_Date, '1-1-1000') and ISNULL(@To_Date,'1-1-3000') ORDER BY Insert_Date DESC, Insert_Time DESC) as Balance_After FROM Account WHERE(SchoolID = @SchoolID) AND AccountID LIKE @AccountID) as All_Account where ([Total_Ex] &lt;&gt; 0 or [Total_In] &lt;&gt; 0)

"
        CancelSelectOnNullParameter="False">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountID" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>


    <script>
        $(function () {
            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });


            //Show Title box
            if ($('.mGrid tr').length) {
                $('.mGrid').closest("section").find('.box').show();
            }



            //get date in label
            var from = $("[id*=From_Date_TextBox]").val();
            var To = $("[id*=To_Date_TextBox]").val();

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

            //Balance before after date//
            if (from != "") {
                $('.From_date').text(from);
            }

            if (To != "") {
                $('.To_date').text(To);
            }

            //Date range picker
            function cb(start, end) {
                $('[id*=From_Date_TextBox]').val(start.format('DD MMM YYYY'));
                $('[id*=To_Date_TextBox]').val(end.format('DD MMM YYYY'));

                $("[id*=Find_Button]").trigger("click");
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

            cb(start, end);
        });
    </script>
</asp:Content>
