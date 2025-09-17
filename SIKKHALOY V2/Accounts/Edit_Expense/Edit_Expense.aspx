<%@ Page Title="Expenditure" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Edit_Expense.aspx.cs" Inherits="EDUCATION.COM.Accounts.Edit_Expense.Edit_Expense" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Expense/Expense.css?v=1" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="Contain">
        <h3>Expenditure</h3>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="form-inline NoPrint">
                    <div class="form-group">
                        <asp:DropDownList ID="FindCategoryDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="CategoryName" DataValueField="ExpenseCategoryID">
                            <asp:ListItem Value="%">[ All Category ]</asp:ListItem>
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
                    <asp:SqlDataSource ID="ViewExpanseSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ISNULL(SUM(Amount), 0) AS TotalExp FROM Expenditure WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ExpenseCategoryID Like @ExpenseCategoryID) AND (ExpenseDate BETWEEN ISNULL(@Fdate,'1-1-1760') AND ISNULL(@TDate,'1-1-3760')) AND (ExpenseID LIKE @ExpenseID)" CancelSelectOnNullParameter="False">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="FindCategoryDropDownList" Name="ExpenseCategoryID" PropertyName="SelectedValue" DefaultValue="" />
                            <asp:ControlParameter ControlID="FormDateTextBox" DefaultValue="" Name="Fdate" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ToDateTextBox" DefaultValue="" Name="TDate" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ReceiptTextBox" DefaultValue="%" Name="ExpenseID" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <div class="table-responsive">
                    <asp:GridView ID="ExpenseGridView" runat="server" AutoGenerateColumns="False" DataSourceID="ExpenseSQL"
                        CssClass="mGrid" DataKeyNames="ExpenseID" AllowPaging="True" PageSize="80" AllowSorting="True">
                        <PagerStyle CssClass="pgr" />
                        <Columns>
                            <asp:TemplateField HeaderText="SN">
                                <ItemTemplate>
                                    <%# Container.DataItemIndex + 1 %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Category" SortExpression="CategoryName">
                                <EditItemTemplate>
                                    <asp:DropDownList ID="ExCategoryDropDownList" runat="server" AppendDataBoundItems="True"
                                        CssClass="form-control" DataSourceID="CategorySQL" DataTextField="CategoryName" DataValueField="ExpenseCategoryID" SelectedValue='<%# Bind("ExpenseCategoryID") %>' />
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("CategoryName") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Amount" SortExpression="Amount">
                                <EditItemTemplate>
                                    <asp:TextBox ID="TextBox1" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" Text='<%# Bind("Amount") %>' CssClass="form-control"></asp:TextBox>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Label2" runat="server" Text='<%# Bind("Amount") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Expense Reason" SortExpression="ExpenseFor">
                                <EditItemTemplate>
                                    <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("ExpenseFor") %>' CssClass="form-control"></asp:TextBox>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Label3" runat="server" Text='<%# Bind("ExpenseFor") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Expense Date" SortExpression="ExpenseDate" InsertVisible="False">
                                <EditItemTemplate>
                                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("ExpenseDate", "{0:d MMM yyyy}") %>'></asp:Label>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Label4" runat="server" Text='<%# Bind("ExpenseDate", "{0:d MMM yyyy}") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:CommandField ShowEditButton="True" HeaderText="Edit">
                                <HeaderStyle CssClass="d-print-none" />
                                <ItemStyle HorizontalAlign="Center" Width="50" CssClass="d-print-none" />
                            </asp:CommandField>
                            <asp:TemplateField HeaderText="Delete" ShowHeader="False">
                                <ItemTemplate>
                                    <asp:LinkButton ID="ImageButton1" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are you sure want to delete permanently?')" />
                                </ItemTemplate>
                                <HeaderStyle CssClass="d-print-none" />
                                <ItemStyle HorizontalAlign="Center" Width="50px" CssClass="d-print-none" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Receipt">
                                <ItemTemplate>
                                    <a href="../Expense/ExpenseReceipt.aspx?id=<%# Eval("ExpenseID") %>"><%# Eval("ExpenseID") %></a>
                                </ItemTemplate>
                                <HeaderStyle CssClass="d-print-none" />
                                <ItemStyle CssClass="d-print-none" />
                            </asp:TemplateField>
                        </Columns>
                        <FooterStyle CssClass="GridFooter" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="ExpenseSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        DeleteCommand="set context_info @RegistrationID
DELETE FROM [Expenditure] WHERE [ExpenseID] = @ExpenseID"
                        InsertCommand="INSERT INTO Expenditure(RegistrationID, ExpenseCategoryID, Amount, ExpenseFor, ExpenseDate, SchoolID, EducationYearID, AccountID) VALUES (@RegistrationID, @ExpenseCategoryID, @Amount, @ExpenseFor, @ExpenseDate, @SchoolID, @EducationYearID, @AccountID)"
                        SelectCommand="SELECT Expense_CategoryName.CategoryName, Expenditure.ExpenseID, Expenditure.SchoolID, Expenditure.EducationYearID, Expenditure.RegistrationID, Expenditure.ExpenseCategoryID, Expenditure.Amount, Expenditure.ExpenseFor, Expenditure.ExpenseDate FROM Expenditure INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID WHERE (Expenditure.SchoolID = @SchoolID) AND (Expenditure.EducationYearID = @EducationYearID) AND (Expenditure.ExpenseCategoryID LIKE @ExpenseCategoryID) AND (Expenditure.ExpenseDate BETWEEN ISNULL(@Fdate, '1-1-1760') AND ISNULL(@TDate, '1-1-3760')) AND (Expenditure.ExpenseID LIKE @ExpenseID) ORDER BY Expenditure.ExpenseID DESC"
                        UpdateCommand="set context_info @RegistrationID
UPDATE Expenditure SET Amount = @Amount, ExpenseFor = @ExpenseFor, ExpenseCategoryID = @ExpenseCategoryID WHERE (ExpenseID = @ExpenseID)"
                        CancelSelectOnNullParameter="False">
                        <DeleteParameters>
                            <asp:Parameter Name="ExpenseID" Type="Int32" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:ControlParameter ControlID="ExCategoryDropDownList" Name="ExpenseCategoryID" PropertyName="SelectedValue" Type="Int32" />
                            <asp:ControlParameter ControlID="AmountTextBox" Name="Amount" PropertyName="Text" Type="Double" />
                            <asp:ControlParameter ControlID="ExpenseReasonTextBox" Name="ExpenseFor" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="ExpenseDateTextBox" DbType="Date" Name="ExpenseDate" PropertyName="Text" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                            <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountID" PropertyName="SelectedValue" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="FindCategoryDropDownList" DefaultValue="" Name="ExpenseCategoryID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="FormDateTextBox" DefaultValue="" Name="Fdate" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ToDateTextBox" DefaultValue="" Name="TDate" PropertyName="Text" />
                            <asp:ControlParameter ControlID="ReceiptTextBox" DefaultValue="%" Name="ExpenseID" PropertyName="Text" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="Amount" Type="Double" />
                            <asp:Parameter Name="ExpenseFor" Type="String" />
                            <asp:Parameter Name="ExpenseCategoryID" Type="Int32" />
                            <asp:Parameter Name="ExpenseID" Type="Int32" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>

    <!--Category Modal -->
    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">Add Expense Category</div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
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
                                OnRowDeleted="ExCategoryGridView_RowDeleted" CssClass="mGrid" AllowPaging="True">
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
                                </Columns>
                            </asp:GridView>
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
                                <asp:DropDownList ID="ExCategoryDropDownList" runat="server" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="CategoryName" DataValueField="ExpenseCategoryID" AppendDataBoundItems="True">
                                    <asp:ListItem Value="0">[ SELECT CATEGORY ]</asp:ListItem>
                                </asp:DropDownList>
                                </td>
                            </div>
                            <div class="form-group">
                                <label>Amount<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="AmountTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="A"></asp:RequiredFieldValidator></label>

                                <asp:TextBox ID="AmountTextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>

                            </div>
                            <div class="form-group">
                                <label>Expense Date<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ExpenseDateTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="A"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="ExpenseDateTextBox" runat="server" autocomplete="off" CssClass="form-control Datetime" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false"></asp:TextBox>
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
                                        <asp:SessionParameter DefaultValue="" Name="SchoolID" SessionField="SchoolID" Type="Int32" />
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
            }, 2000); // 2 seconds
            return true;
        })

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
