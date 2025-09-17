<%@ Page Title="Account With Deposit/Withdraw" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Deposit_Withdraw.aspx.cs" Inherits="EDUCATION.COM.Accounts.Account.Deposit_Withdraw" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .Hide { display:none;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Account With Deposit/Withdraw</h3>

    <asp:UpdatePanel ID="UpdatePanel4" runat="server">
        <ContentTemplate>
            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="AccountDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="AccountSQL" DataTextField="AccountName" DataValueField="AccountID" OnDataBound="AccountDropDownList_DataBound">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="AccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [AccountID],  AccountName FROM [Account] WHERE ([SchoolID] = @SchoolID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <a data-toggle="modal" class="btn btn-brown" data-target="#myModal">Add/Manage Account</a>
                </div>
            </div>

            <asp:FormView ID="ABFormView" runat="server" DataKeyNames="AccountID" DataSourceID="AccountBalanceSQL" Width="100%">
                <ItemTemplate>
                    <div class="alert alert-info">
                        <h5 class="mb-0">Total Balance In <%#Eval("AccountName") %></h5>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="AccountBalanceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AccountName + ' (' + CAST(CAST(AccountBalance AS DECIMAL(38,2)) AS VARCHAR(50)) + ')' AS AccountName, AccountID FROM Account WHERE (SchoolID = @SchoolID) AND (AccountID = @AccountID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </ContentTemplate>
    </asp:UpdatePanel>

    <ul class="nav nav-tabs z-depth-1">
        <li class="nav-item"><a class="nav-link active" href="#Deposit" data-toggle="tab" role="tab" aria-expanded="true">Deposit</a></li>
        <li class="nav-item"><a class="nav-link" href="#Withdraw" data-toggle="tab" role="tab" aria-expanded="false">Withdraw</a></li>
    </ul>

    <div class="tab-content card">
        <div id="Deposit" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="alert alert-success Hide">
                        <span id="DAccName"></span>
                    </div>
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:TextBox ID="AccountIN_AmountTextBox" placeholder="Deposit Amount" runat="server" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="AccountIN_AmountTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="D"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox ID="Deposit_Date_TextBox" placeholder="Deposit Date" runat="server" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control Datetime"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="Deposit_Date_TextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="D"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox ID="IN_DetailsTextBox" placeholder="Details" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <asp:SqlDataSource ID="DepositSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                InsertCommand="INSERT INTO AccountIN_Record(AccountID, SchoolID, RegistrationID, AccountIN_Amount, IN_Details, EducationYearID, AccountIN_Date) VALUES (@AccountID, @SchoolID, @RegistrationID, @AccountIN_Amount, @IN_Details, @EducationYearID, @AccountIN_Date)"
                                SelectCommand="SELECT AccountIN_ID, AccountID, SchoolID, RegistrationID, EducationYearID, AccountIN_Amount, IN_Details, AccountIN_Date, Insert_Date FROM AccountIN_Record WHERE (SchoolID = @SchoolID) AND (AccountID = @AccountID) AND (EducationYearID = @EducationYearID)">

                                <InsertParameters>
                                    <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountID" PropertyName="SelectedValue" Type="Int32" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                    <asp:ControlParameter ControlID="AccountIN_AmountTextBox" Name="AccountIN_Amount" PropertyName="Text" Type="Double" />
                                    <asp:ControlParameter ControlID="IN_DetailsTextBox" Name="IN_Details" PropertyName="Text" Type="String" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="Deposit_Date_TextBox" Name="AccountIN_Date" PropertyName="Text" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountID" PropertyName="SelectedValue" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                            <asp:Button ID="DepositButton" runat="server" Text="Deposit" CssClass="btn btn-primary Hide" OnClick="DepositButton_Click" ValidationGroup="D" />
                            <asp:Label ID="DELabel" runat="server" CssClass="EroorSummer"></asp:Label>
                        </div>
                    </div>

                    <asp:GridView ID="DepositGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="AccountIN_ID" DataSourceID="DepositSQL">
                        <Columns>
                            <asp:BoundField DataField="AccountIN_Amount" HeaderText="Deposit Amount" SortExpression="AccountIN_Amount" />
                            <asp:BoundField DataField="AccountIN_Date" HeaderText="Deposit Date" SortExpression="AccountIN_Date" DataFormatString="{0:d MMM yyyy}" />
                            <asp:BoundField DataField="IN_Details" HeaderText="Details" SortExpression="IN_Details" />
                        </Columns>
                    </asp:GridView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <div id="Withdraw" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <div class="alert alert-danger Hide">
                        <span id="WAccName"></span>
                    </div>
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:TextBox ID="AccountOUT_AmountTextBox" placeholder="Withdraw Amount" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="AccountOUT_AmountTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="W"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox ID="Withdraw_Date_TextBox" placeholder="Withdraw Date" runat="server" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control Datetime"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="Withdraw_Date_TextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="W"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:TextBox ID="Out_DetailsTextBox" placeholder="Details" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <asp:Label ID="WELabel" runat="server" CssClass="EroorSummer"></asp:Label>
                            <asp:SqlDataSource ID="WithdrawSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                InsertCommand="INSERT INTO AccountOUT_Record(AccountID, SchoolID, RegistrationID, AccountOUT_Amount, Out_Details, EducationYearID, AccountOUT_Date) VALUES (@AccountID, @SchoolID, @RegistrationID, @AccountOUT_Amount, @Out_Details, @EducationYearID, @AccountOUT_Date)"
                                SelectCommand="SELECT AccountOUT_ID, AccountID, SchoolID, RegistrationID, EducationYearID, AccountOUT_Amount, Out_Details, AccountOUT_Date, Insert_Date FROM AccountOUT_Record WHERE (SchoolID = @SchoolID) AND (AccountID = @AccountID) AND (EducationYearID = @EducationYearID)">
                                <InsertParameters>
                                    <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountID" PropertyName="SelectedValue" Type="Int32" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                    <asp:ControlParameter ControlID="AccountOUT_AmountTextBox" Name="AccountOUT_Amount" PropertyName="Text" Type="Double" />
                                    <asp:ControlParameter ControlID="Out_DetailsTextBox" Name="Out_Details" PropertyName="Text" Type="String" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="Withdraw_Date_TextBox" Name="AccountOUT_Date" PropertyName="Text" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountID" PropertyName="SelectedValue" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                            <asp:Button ID="WithdrawButton" runat="server" Text="Withdraw" CssClass="btn btn-primary Hide" OnClick="WithdrawButton_Click" ValidationGroup="W" />
                        </div>
                    </div>

                    <asp:GridView ID="WithdrawGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="AccountOUT_ID" DataSourceID="WithdrawSQL">
                        <Columns>
                            <asp:BoundField DataField="AccountOUT_Amount" HeaderText="Withdraw Amount" SortExpression="AccountOUT_Amount" />
                            <asp:BoundField DataField="AccountOUT_Date" HeaderText="Withdraw Date" SortExpression="AccountOUT_Date" DataFormatString="{0:d MMM yyyy}" />
                            <asp:BoundField DataField="Out_Details" HeaderText="Details" SortExpression="Out_Details" />
                        </Columns>
                    </asp:GridView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <!-- Modal -->
    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg cascading-modal" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="title">Manage Account</h4>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body mb-0">
                    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                        <ContentTemplate>
                            <div class="table-responsive">
                                <div class="form-inline">
                                    <div class="form-group">
                                        <asp:TextBox ID="AccountNameTextBox" runat="server" CssClass="form-control" placeholder="Account Name"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="AccountNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="I"></asp:RequiredFieldValidator>
                                    </div>
                                    <div class="form-group">
                                        <asp:Button ID="AddAccountButton" runat="server" CssClass="btn btn-primary" OnClick="AddAccountButton_Click" Text="Add" ValidationGroup="I" />
                                        <asp:Label ID="ErrLabel" runat="server" CssClass="EroorSummer"></asp:Label>
                                    </div>
                                </div>

                                <asp:GridView ID="AccountNameGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="AccountID" DataSourceID="AccountNameSQL">
                                    <Columns>
                                        <asp:TemplateField HeaderText="Account">
                                            <ItemTemplate>
                                                <asp:Label ID="Label2" runat="server" Text='<%# Eval("AccountName") %>' />
                                                (<asp:Label ID="Label1" runat="server" Text='<%# Bind("AccountBalance") %>' />)
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="AnTextBox" CssClass="textbox" runat="server" Text='<%# Bind("AccountName") %>'></asp:TextBox>
                                            </EditItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="Total_IN" HeaderText="Deposit" SortExpression="Total_IN" ReadOnly="true" />
                                        <asp:BoundField DataField="Total_OUT" HeaderText="Withdraw" SortExpression="Total_OUT" ReadOnly="true" />
                                        <asp:BoundField DataField="Total_Income" HeaderText="Income" SortExpression="Total_Income" ReadOnly="true" />
                                        <asp:BoundField DataField="Total_Expense" HeaderText="Expense" SortExpression="Total_Expense" ReadOnly="true" />
                                        <asp:BoundField DataField="Deleted_Income" HeaderText="Del. Inc." SortExpression="Deleted_Income" ReadOnly="true" />
                                        <asp:BoundField DataField="Deleted_Expense" HeaderText="Del. Exp." SortExpression="Deleted_Expense" ReadOnly="true" />
                                        <asp:TemplateField HeaderText="Default" SortExpression="Default_Status">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="DStatusCheckBox" runat="server" Text=" " Checked='<%# Bind("Default_Status") %>' AutoPostBack="True" OnCheckedChanged="DStatusCheckBox_CheckedChanged" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:CommandField ShowEditButton="True" />
                                        <asp:TemplateField ShowHeader="False">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="LinkButton1" runat="server" OnClientClick="return confirm('Are you sure want to delete !!')" CausesValidation="False" CommandName="Delete" Text="Delete"></asp:LinkButton>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                                <asp:SqlDataSource ID="AccountNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand=" IF NOT EXISTS (SELECT * FROM Account_Log WHERE  [AccountID] = @AccountID)
DELETE FROM [Account] WHERE [AccountID] = @AccountID"
                                    InsertCommand="IF NOT EXISTS(SELECT * FROM  Account WHERE SchoolID = @SchoolID AND AccountName = @AccountName)
INSERT INTO Account(AccountName, RegistrationID, SchoolID) VALUES (@AccountName, @RegistrationID, @SchoolID)
ELSE
SET @ERROR = @AccountName + '  Already Exists'"
                                    SelectCommand="SELECT * FROM Account WHERE (SchoolID = @SchoolID)" UpdateCommand="UPDATE Account SET AccountName = @AccountName WHERE (AccountID = @AccountID)" OnInserted="AccountNameSQL_Inserted" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                                    <DeleteParameters>
                                        <asp:Parameter Name="AccountID" Type="Int32" />
                                    </DeleteParameters>
                                    <InsertParameters>
                                        <asp:ControlParameter ControlID="AccountNameTextBox" Name="AccountName" PropertyName="Text" Type="String" />
                                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                        <asp:Parameter Direction="Output" Name="ERROR" Size="256" />
                                    </InsertParameters>
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                    <UpdateParameters>
                                        <asp:Parameter Name="AccountID" Type="Int32" />
                                        <asp:Parameter Name="AccountName" />
                                    </UpdateParameters>
                                </asp:SqlDataSource>
                                <asp:SqlDataSource ID="DefaultAccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Account]" UpdateCommand="UPDATE Account SET Default_Status = 1 WHERE (AccountID = @AccountID) AND (SchoolID=@SchoolID) 
UPDATE Account SET Default_Status = 0 WHERE (AccountID &lt;&gt;@AccountID) AND (SchoolID=@SchoolID)"
                                    ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                                    <UpdateParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:Parameter Name="AccountID" />
                                    </UpdateParameters>
                                </asp:SqlDataSource>
                            </div>
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
                <img src="../../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>


    <script type="text/javascript">
        $(function () {
            if ($('[id*=AccountDropDownList]')[0].selectedIndex > 0) {
                $(".Hide").show();
            }

            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (e, f) {
            if ($('[id*=AccountDropDownList]')[0].selectedIndex > 0) {
                $("#DAccName").text("Deposit In " + $('[id*=AccountDropDownList] :selected').text());
                $("#WAccName").text("Withdraw From " + $('[id*=AccountDropDownList] :selected').text());
                $(".Hide").show();
            }
            else {
                $("#DAccName").text("");
                $("#WAccName").text("");
            }

            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
