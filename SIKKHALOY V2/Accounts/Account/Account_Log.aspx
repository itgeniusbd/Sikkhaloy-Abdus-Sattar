<%@ Page Title="Accounts Log" Language="C#" MasterPageFile="~/BASIC.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Account_Log.aspx.cs" Inherits="EDUCATION.COM.Accounts.Account.Account_Log1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta http-equiv="content-type" content="application/xhtml+xml; charset=UTF-8" />
    <link href="CSS/Accounts_Log.css?v=1" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div id="Summary" runat="server" class="ACC_Log">
        <div class="Expo_Name">
            <asp:Label ID="Insti_NameLabel" runat="server"></asp:Label>
        </div>

        <h3>Accounts Log:
         <label class="Date"></label>
            <asp:Label ID="DisDateLabel" runat="server"></asp:Label>
            <asp:HiddenField ID="GetDateHF" runat="server" />
            <span class="NoPrint" runat="server" id="Def_Hide">(By Default Show Today's insert Report)</span></h3>

        <div class="Find_Date form-inline NoPrint" runat="server" id="HideDate">
            <div class="form-group">
                <asp:TextBox ID="FromDateTextBox" runat="server" CssClass="form-control Datetime"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control Datetime"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:Button ID="FindButton" runat="server" Text="Find" CssClass="btn btn-primary" />
            </div>
        </div>
    </div>


    <ul class="nav nav-tabs z-depth-1">
        <li class="nav-item">
            <a class="nav-link active" data-toggle="tab" href="#panel1" role="tab" aria-expanded="true">Income Log</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" data-toggle="tab" href="#panel2" role="tab" aria-expanded="false">Expense Log</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" data-toggle="tab" href="#panel3" role="tab" aria-expanded="false">Adjustment</a>
        </li>
    </ul>

    <div class="tab-content card">
        <div class="tab-pane fade in active show" id="panel1" role="tabpanel" aria-expanded="true">
            <div id="Cash_In" runat="server" class="ACC_Log">
                <div class="Left_Con" runat="server" id="Sum_total">

                    <asp:FormView ID="Total_IN_FormView" runat="server" DataSourceID="Total_Cash_IN_SQL" Width="100%">
                        <ItemTemplate>
                            <div class="In">
                                Income Log :(Total
               <asp:Label ID="Total_INLabel" runat="server" Text='<%# Bind("Total_IN") %>' />
                                /- )
                            </div>
                        </ItemTemplate>
                    </asp:FormView>
                    <asp:SqlDataSource ID="Total_Cash_IN_SQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ISNULL(SUM(Amount),0) AS Total_IN FROM Account_Log WHERE  (SchoolID = @SchoolID) AND In_Ex_type='In' AND Insert_Up_De = 'In' AND Insert_Date between ISNULL(@From_Date, '1-1-1000') and ISNULL(@To_Date,'1-1-3000') " ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>


                <asp:DataList ID="IN_ClassOrOtherCategoryDataList" runat="server" DataSourceID="IN_ClassOrOtherCategorySQL" Width="100%" RepeatDirection="Horizontal" RepeatLayout="Flow">
                    <ItemTemplate>
                        <div class="A_Name Sub_In">
                            <asp:Label ID="ClassOrOtherCategoryLabel" runat="server" Text='<%# Eval("ClassOrOtherCategory") %>' />
                            (<asp:Label ID="TotalLabel" runat="server" Text='<%# Eval("Total","{0:n}") %>' />
                            Tk)
                        </div>
                        <asp:GridView ID="InLogGridView" runat="server" AutoGenerateColumns="False" DataSourceID="In_SubCategorySQL" CssClass="mGrid" AllowPaging="True" PageSize="50" PagerStyle-CssClass="pgr" AllowSorting="True">
                            <Columns>
                                <asp:BoundField DataField="Log_SN" HeaderText="SN" SortExpression="Log_SN" />
                                <asp:BoundField DataField="SubCategory" HeaderText="Category" SortExpression="SubCategory" />
                                <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                                <asp:BoundField DataField="Details" HeaderText="Details" SortExpression="Details" />
                                <asp:BoundField DataField="UserName" HeaderText="User Name" SortExpression="UserName">
                                    <HeaderStyle Width="85px" />
                                </asp:BoundField>
                                <asp:BoundField DataField="Insert_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Insert Date" SortExpression="Insert_Date" />
                                <asp:BoundField DataField="Activity_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Date" SortExpression="Activity_Date" />
                                <asp:BoundField DataField="Insert_Time" HeaderText="Time" SortExpression="Insert_Time" />
                            </Columns>
                        </asp:GridView>
                        <asp:SqlDataSource ID="In_SubCategorySQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Account_Log.Log_SN, Account_Log.SubCategory, Account_Log.Amount, Account_Log.Details, Registration.UserName, Account_Log.Insert_Date,Account_Log.Activity_Date, CONVERT(varchar(15),Account_Log.Insert_Time,100) AS Insert_Time FROM Account_Log INNER JOIN
 Registration ON Account_Log.RegistrationID = Registration.RegistrationID WHERE (Account_Log.SchoolID = @SchoolID) AND (Account_Log.ClassOrOtherCategory = @ClassOrOtherCategory) AND (Account_Log.Add_Subtraction = N'Add') AND (Account_Log.ClassOrOtherCategory not like '%Updated%' AND Account_Log.ClassOrOtherCategory not  like '%Deleted%')AND 
(Account_Log.Insert_Date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))"
                            ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:ControlParameter ControlID="ClassOrOtherCategoryLabel" Name="ClassOrOtherCategory" PropertyName="Text" />
                                <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                                <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </ItemTemplate>
                </asp:DataList>
                <asp:SqlDataSource ID="IN_ClassOrOtherCategorySQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT  ClassOrOtherCategory, ISNULL(SUM(Amount),0) AS Total FROM Account_Log 
WHERE (SchoolID = @SchoolID) AND In_Ex_type='In' AND Insert_Up_De = 'In' AND (Insert_Date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) 
GROUP BY ClassOrOtherCategory ORDER BY ClassOrOtherCategory"
                    ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                        <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <br />
            <asp:Button ID="In_Export_Button" runat="server" Text="Export To Word" OnClick="In_Export_Button_Click" CssClass="btn btn-primary" />
        </div>

        <div class="tab-pane fade" id="panel2" role="tabpanel" aria-expanded="false">
            <div id="Cash_Out" runat="server" class="ACC_Log">
                <asp:FormView ID="Cash_Out_FormView" runat="server" DataSourceID="Total_Cash_Out_SQL">
                    <ItemTemplate>
                        <div class="Out">
                            Expense Log: (Total
               <asp:Label ID="Total_OutLabel" runat="server" Text='<%# Bind("Total_Out") %>' />
                            /-)
                        </div>
                    </ItemTemplate>
                </asp:FormView>
                <asp:SqlDataSource ID="Total_Cash_Out_SQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ISNULL(SUM(Amount),0) AS Total_Out FROM Account_Log WHERE (SchoolID = @SchoolID) AND In_Ex_type='Ex' AND Insert_Up_De = 'In' AND Insert_Date between ISNULL(@From_Date, '1-1-1000') and ISNULL(@To_Date,'1-1-3000') " ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                        <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />

                    </SelectParameters>
                </asp:SqlDataSource>

                <asp:DataList ID="OCDataList" runat="server" DataSourceID="Out_ClassOrOtherCategorySQL" Width="100%" RepeatDirection="Horizontal" RepeatLayout="Flow">
                    <ItemTemplate>
                        <div class="A_Name Sub_Out">
                            <asp:Label ID="ClassOrOtherCategoryLabel" runat="server" Text='<%# Eval("ClassOrOtherCategory") %>' />
                            (<asp:Label ID="TotalLabel" runat="server" Text='<%# Eval("Total","{0:n}") %>' />
                            Tk)
                        </div>
                        <asp:GridView ID="InLogGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="Out_SubCategory_SQL" AllowPaging="true" PageSize="50" PagerStyle-CssClass="pgr" AllowSorting="true">
                            <Columns>
                                <asp:BoundField DataField="Log_SN" HeaderText="SN" SortExpression="Log_SN" />
                                <asp:BoundField DataField="SubCategory" HeaderText="Category" SortExpression="SubCategory" />
                                <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                                <asp:BoundField DataField="Details" HeaderText="Details" SortExpression="Details" />
                                <asp:BoundField DataField="UserName" HeaderText="User Name" SortExpression="UserName">
                                    <HeaderStyle Width="85px" />
                                </asp:BoundField>
                                <asp:BoundField DataField="Insert_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Insert Date" SortExpression="Insert_Date" />
                                <asp:BoundField DataField="Activity_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Date" SortExpression="Activity_Date" />
                                <asp:BoundField DataField="Insert_Time" HeaderText="Time" SortExpression="Insert_Time" />
                            </Columns>
                        </asp:GridView>
                        <asp:SqlDataSource ID="Out_SubCategory_SQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Account_Log.Log_SN, Account_Log.SubCategory, Account_Log.Amount, Account_Log.Details, Registration.UserName, Account_Log.Insert_Date,Account_Log.Activity_Date, CONVERT(varchar(15),Account_Log.Insert_Time,100) AS Insert_Time FROM Account_Log INNER JOIN
Registration ON Account_Log.RegistrationID = Registration.RegistrationID WHERE (Account_Log.SchoolID = @SchoolID) AND (Account_Log.ClassOrOtherCategory = @ClassOrOtherCategory) AND (Account_Log.Add_Subtraction = N'Subtraction')
AND  (Account_Log.ClassOrOtherCategory not  like '%Updated%' AND Account_Log.ClassOrOtherCategory  not like '%Deleted%')  AND (Account_Log.Insert_Date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) "
                            ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:ControlParameter ControlID="ClassOrOtherCategoryLabel" Name="ClassOrOtherCategory" PropertyName="Text" />
                                <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                                <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />

                            </SelectParameters>
                        </asp:SqlDataSource>
                    </ItemTemplate>
                </asp:DataList>
                <asp:SqlDataSource ID="Out_ClassOrOtherCategorySQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT  ClassOrOtherCategory, SUM(Amount) AS Total FROM Account_Log WHERE (SchoolID = @SchoolID) AND In_Ex_type='Ex' AND Insert_Up_De = 'In' AND (Insert_Date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))  
GROUP BY ClassOrOtherCategory ORDER BY ClassOrOtherCategory "
                    ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                        <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />

                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
            <br />
            <asp:Button ID="Out_Export_Button" runat="server" Text="Export To Word" CssClass="btn btn-primary" OnClick="Out_Export_Button_Click" />
        </div>

        <div class="tab-pane fade" id="panel3" role="tabpanel" aria-expanded="false">
            <asp:FormView ID="IN_Deleted_FormView" runat="server" DataSourceID="In_Deleted_Amount_SQL">
                <ItemTemplate>
                    <div class="Adjsmnt In_Adjs">
                        Adjustment Log: (Total 
               <asp:Label ID="Amount_In_By_deleteLabel" runat="server" Text='<%# Bind("Amount_In_By_delete") %>' />
                        /-)
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="In_Deleted_Amount_SQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ISNULL(SUM(Amount),0) AS Amount_In_By_delete FROM Account_Log WHERE (SchoolID = @SchoolID) AND Insert_Up_De &lt;&gt; 'In' AND (Insert_Date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) "
                ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                    <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />

                </SelectParameters>
            </asp:SqlDataSource>

            <asp:DataList ID="IN_DeletedDataList" runat="server" DataSourceID="IN_Deleted_ClassOrOtherCategorySQL" Width="100%" RepeatDirection="Horizontal" RepeatLayout="Flow">
                <ItemTemplate>
                    <div class="A_Name Sub_In">
                        <asp:Label ID="ClassOrOtherCategoryLabel" runat="server" Text='<%# Eval("ClassOrOtherCategory") %>' />
                        (<asp:Label ID="TotalLabel" runat="server" Text='<%# Eval("Total","{0:n}") %>' />
                        Tk)
                    </div>

                    <asp:GridView ID="InLogGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="IN_Deleted_SubCategorySQL" AllowPaging="true" PageSize="50" PagerStyle-CssClass="pgr" AllowSorting="true">
                        <Columns>
                            <asp:BoundField DataField="Log_SN" HeaderText="SN" SortExpression="Log_SN" />
                            <asp:BoundField DataField="SubCategory" HeaderText="Category" SortExpression="SubCategory" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                            <asp:BoundField DataField="Details" HeaderText="Details" SortExpression="Details" />
                            <asp:BoundField DataField="UserName" HeaderText="User Name" SortExpression="UserName">
                                <HeaderStyle Width="85px" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Insert_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Insert Date" SortExpression="Insert_Date" />
                            <asp:BoundField DataField="Activity_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Date" SortExpression="Activity_Date" />
                            <asp:BoundField DataField="Insert_Time" HeaderText="Time" SortExpression="Insert_Time" />
                        </Columns>
                        <FooterStyle HorizontalAlign="Center" />
                        <HeaderStyle BackColor="#F4F4F4" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="IN_Deleted_SubCategorySQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Account_Log.Log_SN, Account_Log.SubCategory, Account_Log.Amount, Account_Log.Details, Registration.UserName, Account_Log.Insert_Date, Account_Log.Activity_Date, CONVERT(varchar(15),Account_Log.Insert_Time,100) AS Insert_Time FROM Account_Log INNER JOIN
Registration ON Account_Log.RegistrationID = Registration.RegistrationID WHERE (Account_Log.SchoolID = @SchoolID) AND (Account_Log.ClassOrOtherCategory = @ClassOrOtherCategory) AND
Insert_Up_De &lt;&gt; 'In' AND (Account_Log.Insert_Date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) "
                        ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="ClassOrOtherCategoryLabel" Name="ClassOrOtherCategory" PropertyName="Text" />
                            <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />

                        </SelectParameters>
                    </asp:SqlDataSource>
                </ItemTemplate>
            </asp:DataList>
            <asp:SqlDataSource ID="IN_Deleted_ClassOrOtherCategorySQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT  ClassOrOtherCategory, ISNULL(SUM(Amount),0) AS Total FROM Account_Log WHERE (SchoolID = @SchoolID)  AND Insert_Up_De &lt;&gt; 'In'  AND (Insert_Date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) 
GROUP BY ClassOrOtherCategory ORDER BY ClassOrOtherCategory"
                ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                    <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />

                </SelectParameters>
            </asp:SqlDataSource>
        </div>
    </div>

    <script type="text/javascript">
        $(function () {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            if ($('[id*=IN_DeletedDataList] tr').length) {
                $(".In_Adjs").show();
            }
            if ($('[id*=DOCDataList] tr').length) {
                $(".Out_Adjs").show();
            }


            //get date in label
            var from = $("[id*=FromDateTextBox]").val();
            var To = $("[id*=ToDateTextBox]").val();

            $("#txtDate").val();

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
            var DT = $(".Date").text();
            $('#<%=GetDateHF.ClientID%>').val(DT);
        });
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
