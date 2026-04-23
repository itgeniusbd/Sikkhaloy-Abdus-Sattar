<%@ Page Title="Expenditure" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Expense.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.Expense.Expense" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="Expense.css?v=1" rel="stylesheet" />
    <style>
        /* Category modal grid compact */
        #myModal .mGrid td, #myModal .mGrid th { padding: 4px 8px; font-size: 13px; vertical-align: middle; }
        #myModal .mGrid td a, #myModal .mGrid td span { font-size: 13px; }
        #myModal .modal-body { max-height: 75vh; overflow-y: auto; padding: 10px 15px; }
        #myModal .modal-header { padding: 8px 15px; }
        #myModal .form-inline { margin-bottom: 8px; }
        /* Sub-cat modal compact */
        #subCatModal .mGrid td, #subCatModal .mGrid th { padding: 4px 8px; font-size: 13px; vertical-align: middle; }
        #subCatModal .modal-body { max-height: 65vh; overflow-y: auto; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="Contain">
        <h3>Expenditure</h3>

        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="form-inline NoPrint">
                    <div class="form-group">
                        <asp:DropDownList ID="FindCategoryDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="CategoryName" DataValueField="ExpenseCategoryID" AutoPostBack="True" OnSelectedIndexChanged="FindCategoryDropDownList_SelectedIndexChanged">
                            <asp:ListItem Value="%">[ All Category ]</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="form-group">
                        <asp:DropDownList ID="FindSubCategoryDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control">
                            <asp:ListItem Value="%">[ All Sub-Category ]</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="form-group">
                        <asp:TextBox ID="FormDateTextBox" placeholder="From Date" runat="server" autocomplete="off" CssClass="form-control Datetime" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <asp:TextBox ID="ToDateTextBox" placeholder="To Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control Datetime"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:TextBox ID="ReceiptTextBox" placeholder="Receipt No." autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:Button ID="FindButton" runat="server" CssClass="btn btn-blue-grey" Text="Find" />
                    </div>

                    <div class="form-group pull-right">
                        <button type="button" class="btn btn-deep-orange" data-toggle="modal" data-target="#myModal2">Add Expense</button>
                        <button type="button" class="btn btn-success" data-toggle="modal" data-target="#myModal">Add New Category</button>
                    </div>
                    <div class="clearfix"></div>
                </div>

                <div class="alert alert-success">
                    <asp:FormView ID="Total_FormView" runat="server" DataSourceID="ViewExpanseSQL">
                        <ItemTemplate>
                            <h4 class="TotalEx">
                                <label class="Date"></label>
                                Total <%# Eval("TotalExp","{0:N0}") %> Tk.</h4>
                        </ItemTemplate>
                    </asp:FormView>

                    <asp:SqlDataSource ID="ViewExpanseSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ISNULL(SUM(Amount), 0) AS TotalExp FROM Expenditure WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ExpenseCategoryID LIKE @ExpenseCategoryID) AND (ISNULL(CAST(ExpenseSubCategoryID AS VARCHAR),'%') LIKE @ExpenseSubCategoryID) AND (ExpenseDate BETWEEN ISNULL(@Fdate, '1-1-1760') AND ISNULL(@TDate, '1-1-3760')) AND (ExpenseID LIKE @ExpenseID)" CancelSelectOnNullParameter="False">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="FindCategoryDropDownList" Name="ExpenseCategoryID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="FindSubCategoryDropDownList" DefaultValue="%" Name="ExpenseSubCategoryID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="FormDateTextBox" DefaultValue="" Name="Fdate" PropertyName="Text" DbType="Date" />
                            <asp:ControlParameter ControlID="ToDateTextBox" DefaultValue="" Name="TDate" PropertyName="Text" DbType="Date" />
                            <asp:ControlParameter ControlID="ReceiptTextBox" DefaultValue="%" Name="ExpenseID" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <div class="table-responsive">
                    <asp:GridView ID="ExpenseGridView" runat="server" AutoGenerateColumns="False" DataSourceID="ExpenseSQL"
                        AlternatingRowStyle-CssClass="alt" PagerStyle-CssClass="pgr" DataKeyNames="ExpenseID" CssClass="mGrid" AllowPaging="True" PageSize="80" AllowSorting="True">
                        <AlternatingRowStyle CssClass="alt" />
                        <RowStyle CssClass="RowStyle" />
                        <PagerStyle CssClass="pgr" />
                        <SelectedRowStyle CssClass="Selected" />
                        <Columns>
                            <asp:TemplateField HeaderText="SN">
                                <ItemTemplate>
                                    <%# Container.DataItemIndex + 1 %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="CategoryName" HeaderText="Category" SortExpression="CategoryName" />
                            <asp:BoundField DataField="SubCategoryName" HeaderText="Sub-Category" SortExpression="SubCategoryName" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                            <asp:BoundField DataField="ExpenseFor" HeaderText="Expense Reason" SortExpression="ExpenseFor" />
                            <asp:BoundField DataField="ExpenseDate" HeaderText="Expense Date" SortExpression="ExpenseDate" ReadOnly="True" DataFormatString="{0:d MMM yyyy}" />
                            <asp:TemplateField HeaderText="Receipt">
                                <ItemTemplate>
                                    <a href="ExpenseReceipt.aspx?id=<%# Eval("ExpenseID") %>"><%# Eval("ExpenseID") %></a>
                                </ItemTemplate>
                                 <HeaderStyle CssClass="d-print-none" />
                            <ItemStyle CssClass="d-print-none" />
                            </asp:TemplateField>
                        </Columns>
                        <FooterStyle CssClass="GridFooter" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="ExpenseSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        InsertCommand="INSERT INTO Expenditure(RegistrationID, ExpenseCategoryID, ExpenseSubCategoryID, Amount, ExpenseFor, ExpenseDate, SchoolID, EducationYearID, AccountID) VALUES (@RegistrationID, @ExpenseCategoryID, NULLIF(@ExpenseSubCategoryID,''), @Amount, @ExpenseFor, Getdate(), @SchoolID, @EducationYearID, @AccountID)"
                        SelectCommand="SELECT Expense_CategoryName.CategoryName, ISNULL(sc.SubCategoryName,'') AS SubCategoryName, Expenditure.ExpenseID, Expenditure.SchoolID, Expenditure.EducationYearID, Expenditure.RegistrationID, Expenditure.ExpenseCategoryID, Expenditure.ExpenseSubCategoryID, Expenditure.Amount, Expenditure.ExpenseFor, Expenditure.ExpenseDate FROM Expenditure INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID LEFT JOIN Expense_SubCategory sc ON Expenditure.ExpenseSubCategoryID = sc.ExpenseSubCategoryID WHERE (Expenditure.SchoolID = @SchoolID) AND (Expenditure.EducationYearID = @EducationYearID) AND (Expenditure.ExpenseCategoryID LIKE @ExpenseCategoryID) AND (ISNULL(CAST(Expenditure.ExpenseSubCategoryID AS VARCHAR),'%') LIKE @ExpenseSubCategoryID) AND (Expenditure.ExpenseDate BETWEEN ISNULL(@Fdate, '1-1-1760') AND ISNULL(@TDate, '1-1-3760')) AND (Expenditure.ExpenseID LIKE @ExpenseID) ORDER BY Expenditure.ExpenseID DESC" CancelSelectOnNullParameter="False">
                        <InsertParameters>
                            <asp:ControlParameter ControlID="ExCategoryDropDownList" Name="ExpenseCategoryID" PropertyName="SelectedValue" Type="Int32" />
                            <asp:ControlParameter ControlID="ExSubCategoryDropDownList" Name="ExpenseSubCategoryID" PropertyName="SelectedValue" Type="String" />
                            <asp:ControlParameter ControlID="AmountTextBox" Name="Amount" PropertyName="Text" Type="Double" />
                            <asp:ControlParameter ControlID="ExpenseReasonTextBox" Name="ExpenseFor" PropertyName="Text" Type="String" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                            <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountID" PropertyName="SelectedValue" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="FindCategoryDropDownList" DefaultValue="%" Name="ExpenseCategoryID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="FindSubCategoryDropDownList" DefaultValue="%" Name="ExpenseSubCategoryID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="FormDateTextBox" DefaultValue="" Name="Fdate" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ToDateTextBox" DefaultValue="" Name="TDate" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ReceiptTextBox" DefaultValue="%" Name="ExpenseID" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <!--Category Modal -->
    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">Add Expense Category</div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close" onclick="$('[id$=SubPanelOpenFlag]').val('0'); $('#subCatModal').modal('hide');"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="upnlUsers" runat="server">
                        <ContentTemplate>
                            <div class="form-inline">
                                <div class="form-group">
                                    <asp:TextBox placeholder="Category Name" ID="CategoryNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="CategoryNameTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="ADD"></asp:RequiredFieldValidator>
                                </div>
                                <div class="form-group">
                                    <asp:Button ID="AddCategoryButton" runat="server" CssClass="btn btn-primary" OnClick="AddCategoryButton_Click" Text="Add" ValidationGroup="ADD" />
                                </div>
                            </div>

                            <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Expense_CategoryName] WHERE [ExpenseCategoryID] = @ExpenseCategoryID" InsertCommand=" IF NOT EXISTS ( SELECT  * FROM [Expense_CategoryName] WHERE (SchoolID = @SchoolID) AND (CategoryName = @CategoryName))
INSERT INTO Expense_CategoryName(CategoryName, RegistrationID, SchoolID) VALUES (LTRIM(RTRIM(@CategoryName)), @RegistrationID, @SchoolID)"
                                SelectCommand="SELECT ExpenseCategoryID, SchoolID, RegistrationID, CategoryName FROM Expense_CategoryName WHERE (SchoolID = @SchoolID)" UpdateCommand=" IF NOT EXISTS ( SELECT  * FROM [Expense_CategoryName] WHERE (SchoolID = @SchoolID) AND (CategoryName = @CategoryName))
UPDATE [Expense_CategoryName] SET [CategoryName] = LTRIM(RTRIM(@CategoryName)) WHERE [ExpenseCategoryID] = @ExpenseCategoryID">
                                <DeleteParameters>
                                    <asp:Parameter Name="ExpenseCategoryID" Type="Int32" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:ControlParameter ControlID="CategoryNameTextBox" Name="CategoryName" PropertyName="Text" Type="String" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                                <UpdateParameters>
                                    <asp:Parameter Name="CategoryName" Type="String" />
                                    <asp:Parameter Name="ExpenseCategoryID" Type="Int32" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                            <asp:GridView ID="ExCategoryGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="ExpenseCategoryID" DataSourceID="CategorySQL"
                                OnRowDeleted="ExCategoryGridView_RowDeleted" OnRowCommand="ExCategoryGridView_RowCommand" CssClass="mGrid" AllowPaging="True">
                                <PagerStyle CssClass="pgr" />
                                <Columns>
                                    <asp:TemplateField HeaderText="Category" SortExpression="CategoryName">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("CategoryName") %>' CssClass="textbox"></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("CategoryName") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:CommandField ShowEditButton="True" UpdateText="Save" HeaderText="Edit">
                                        <ItemStyle Width="60px" />
                                    </asp:CommandField>
                                    <asp:TemplateField ShowHeader="False" HeaderText="Delete">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are you sure want to delete?')"></asp:LinkButton>
                                        </ItemTemplate>
                                        <ItemStyle Width="50px" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Sub">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="ManageSubCatBtn" runat="server"
                                                Text='<i class="fa fa-list"></i> Sub'
                                                CommandName="ManageSub"
                                                CommandArgument='<%# Eval("ExpenseCategoryID") + "|" + Eval("CategoryName") %>'
                                                CssClass="btn btn-xs btn-info"
                                                style="white-space:nowrap;"></asp:LinkButton>
                                        </ItemTemplate>
                                        <ItemStyle Width="55px" />
                                        <HeaderStyle Width="55px" />
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>

                            <!-- Sub-Category hidden data (server-side state) -->
                            <asp:HiddenField ID="SelectedCategoryIDHidden" runat="server" />
                            <asp:HiddenField ID="SubPanelOpenFlag" runat="server" Value="0" />
                            <asp:Label ID="SelectedCategoryLabel" runat="server" style="display:none;"></asp:Label>

                            <!-- Sub-Category SqlDataSource -->
                            <asp:SqlDataSource ID="SubCategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                DeleteCommand="DELETE FROM Expense_SubCategory WHERE ExpenseSubCategoryID = @ExpenseSubCategoryID"
                                UpdateCommand="UPDATE Expense_SubCategory SET SubCategoryName = LTRIM(RTRIM(@SubCategoryName)) WHERE ExpenseSubCategoryID = @ExpenseSubCategoryID"
                                SelectCommand="SELECT ExpenseSubCategoryID, SubCategoryName FROM Expense_SubCategory WHERE ExpenseCategoryID = @ExpenseCategoryID AND SchoolID = @SchoolID ORDER BY ExpenseSubCategoryID">
                                <DeleteParameters>
                                    <asp:Parameter Name="ExpenseSubCategoryID" Type="Int32" />
                                </DeleteParameters>
                                <UpdateParameters>
                                    <asp:Parameter Name="SubCategoryName" Type="String" />
                                    <asp:Parameter Name="ExpenseSubCategoryID" Type="Int32" />
                                </UpdateParameters>
                                <SelectParameters>
                                    <asp:ControlParameter ControlID="SelectedCategoryIDHidden" Name="ExpenseCategoryID" PropertyName="Value" Type="Int32" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                            </asp:SqlDataSource>

                            <!-- Sub-Category Modal (nested, centered) -->
                            <asp:Panel ID="SubCategoryPanel" runat="server" Visible="True">
                            <div class="modal fade" id="subCatModal" tabindex="-1" role="dialog" aria-hidden="true" style="z-index:1060;">
                                <div class="modal-dialog modal-dialog-centered" role="document" style="max-width:500px;">
                                    <div class="modal-content">
                                        <div class="modal-header" style="background:#17a2b8; color:#fff; padding:10px 15px;">
                                            <h6 class="modal-title mb-0">
                                                <i class="fa fa-list mr-1"></i>
                                                Sub-Categories of: <strong><asp:Label ID="SubCatTitleLabel" runat="server"></asp:Label></strong>
                                            </h6>
                                            <asp:LinkButton ID="CloseSubPanelBtn" runat="server" Text="&times;" CssClass="close" style="color:#fff; font-size:20px; line-height:1;" OnClick="CloseSubPanelBtn_Click"></asp:LinkButton>
                                        </div>
                                        <div class="modal-body" style="padding:15px;">
                                            <div class="input-group mb-3">
                                                <asp:TextBox placeholder="Sub-Category Name" ID="SubCategoryNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                                <div class="input-group-append">
                                                    <asp:Button ID="AddSubCategoryButton" runat="server" CssClass="btn btn-primary" OnClick="AddSubCategoryButton_Click" Text="Add" ValidationGroup="ADDSUB" />
                                                </div>
                                            </div>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="SubCategoryNameTextBox" CssClass="text-danger" ErrorMessage="Sub-Category Name is required." ValidationGroup="ADDSUB" Display="Dynamic"></asp:RequiredFieldValidator>
                                            <asp:GridView ID="SubCategoryGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="ExpenseSubCategoryID" DataSourceID="SubCategorySQL"
                                                OnRowDeleted="SubCategoryGridView_RowDeleted" CssClass="mGrid" AllowPaging="True" Width="100%">
                                                <PagerStyle CssClass="pgr" />
                                                <Columns>
                                                    <asp:TemplateField HeaderText="Sub-Category">
                                                        <EditItemTemplate>
                                                            <asp:TextBox ID="SubCatEditTextBox" runat="server" Text='<%# Bind("SubCategoryName") %>' CssClass="form-control form-control-sm"></asp:TextBox>
                                                        </EditItemTemplate>
                                                        <ItemTemplate>
                                                            <asp:Label ID="SubCatLabel" runat="server" Text='<%# Bind("SubCategoryName") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:CommandField ShowEditButton="True" UpdateText="Save" HeaderText="Edit">
                                                        <ItemStyle Width="50px" />
                                                    </asp:CommandField>
                                                    <asp:TemplateField ShowHeader="False" HeaderText="Del">
                                                        <ItemTemplate>
                                                            <asp:LinkButton ID="DelSubCatBtn" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are you sure?')" CssClass="text-danger"></asp:LinkButton>
                                                        </ItemTemplate>
                                                        <ItemStyle Width="50px" />
                                                    </asp:TemplateField>
                                                </Columns>
                                            </asp:GridView>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            </asp:Panel>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>

    <!-- Expense Modal -->
    <div class="modal fade" id="myModal2" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">Add Expense</div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                        <ContentTemplate>
                            <div class="form-group">
                                <label>
                                    Category
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="ExCategoryDropDownList" CssClass="EroorSummer" ErrorMessage="Select Category" InitialValue="0" ValidationGroup="A">*</asp:RequiredFieldValidator></label>
                                <asp:DropDownList ID="ExCategoryDropDownList" runat="server" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="CategoryName" DataValueField="ExpenseCategoryID" AppendDataBoundItems="True" AutoPostBack="True" OnSelectedIndexChanged="ExCategoryDropDownList_SelectedIndexChanged">
                                    <asp:ListItem Value="0">[ SELECT CATEGORY ]</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="form-group">
                                <label>Sub-Category <small class="text-muted">(optional)</small></label>
                                <asp:DropDownList ID="ExSubCategoryDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True">
                                    <asp:ListItem Value="">[ No Sub-Category ]</asp:ListItem>
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SubCategoryEntrySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                    SelectCommand="SELECT ExpenseSubCategoryID, SubCategoryName FROM Expense_SubCategory WHERE ExpenseCategoryID = @ExpenseCategoryID AND SchoolID = @SchoolID ORDER BY ExpenseSubCategoryID">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="ExCategoryDropDownList" Name="ExpenseCategoryID" PropertyName="SelectedValue" Type="Int32" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                            <div class="form-group">
                                <label>Amount<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="AmountTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="A"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="AmountTextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Expense Reason</label>
                                <asp:TextBox ID="ExpenseReasonTextBox" runat="server" CssClass="form-control" TextMode="MultiLine"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Expense From<asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="AccountDropDownList" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="A"></asp:RequiredFieldValidator></label>
                                <asp:DropDownList ID="AccountDropDownList" runat="server" CssClass="form-control" DataSourceID="AccountSQL" DataTextField="AccountName" DataValueField="AccountID">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="AccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AccountID,AccountName  +Format(AccountBalance,' (##,###.## tk)')  as AccountName FROM [Account] WHERE ([SchoolID] = @SchoolID) AND (AccountBalance &lt;&gt; 0)">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>

                            <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Submit" ValidationGroup="A" />
                            <label id="ErMsg"></label>
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
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true,
                endDate: '+0d'
            });

            //get date in label
            var from = $("[id*=FormDateTextBox]").val();
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
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true,
                endDate: '+0d'
            });

            // Re-open modals after postback based on flag
            var flag = $('[id$=SubPanelOpenFlag]').val();
            if (flag === '1') {
                $('#myModal').modal('show');
                setTimeout(function () { $('#subCatModal').modal('show'); }, 300);
            } else if (flag === '2') {
                $('#subCatModal').modal('hide');
                $('#myModal').modal('show');
            } else {
                // flag=0: all modals closed — clean up any stale backdrop
                cleanupModals();
            }

            //get date in label
            var from = $("[id*=FormDateTextBox]").val();
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
        })

        function Success() {
            var e = $('#ErMsg');
            e.text("Expense Inserted Successfully!!");
            e.fadeIn();
            e.queue(function () { setTimeout(function () { e.dequeue(); }, 3000); });
            e.fadeOut('slow');
        }

        //Disable the submit button after clicking
        $("form").submit(function () {
            $("[id$=SubmitButton]").attr("disabled", true);
            setTimeout(function () {
                $("[id$=SubmitButton]").prop('disabled', false);
            }, 3000); // 3 seconds
            return true;
        })

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };

        // Clean up Bootstrap modal backdrop and body class
        function cleanupModals() {
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open');
            $('body').css('padding-right', '');
        }

        // When both modals are fully hidden, clean up backdrop
        $('#myModal').on('hidden.bs.modal', function () {
            if (!$('#subCatModal').hasClass('in') && !$('#subCatModal').hasClass('show')) {
                cleanupModals();
            }
        });
        $('#subCatModal').on('hidden.bs.modal', function () {
            if (!$('#myModal').hasClass('in') && !$('#myModal').hasClass('show')) {
                cleanupModals();
            }
        });
    </script>
</asp:Content>
