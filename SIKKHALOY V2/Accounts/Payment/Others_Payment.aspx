<%@ Page Title="Others Payment" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Others_Payment.aspx.cs" Inherits="EDUCATION.COM.Accounts.Payment.Others_Payment" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Expanse.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Others Payment Details</h3>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:DropDownList ID="FindCategoryDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="Extra_Income_CategoryName" DataValueField="Extra_IncomeCategoryID">
                        <asp:ListItem Value="%">[ SELECT CATEGORY ]</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="FormDateTextBox" placeholder="From Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="ToDateTextBox" placeholder="To Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="ReceiptTextBox" placeholder="Receipt No." autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="FindButton" runat="server" Text="Find" CssClass="btn btn-brown" OnClick="FindButton_Click" />
                </div>
                <div class="form-group pull-right">
                    <button type="button" class="btn btn-success" data-toggle="modal" data-target="#myModal2">Add Income</button>
                    <button type="button" class="btn btn-deep-orange" data-toggle="modal" data-target="#myModal">Add New Category</button>
                </div>
                <div class="clearfix"></div>
            </div>

            <div class="alert alert-success">
                <asp:Label ID="AmountLabel" runat="server" Font-Bold="True" Font-Size="Large"></asp:Label>
                <asp:SqlDataSource ID="ViewIncomeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT ISNULL(SUM(Extra_IncomeAmount), 0) AS Amount FROM Extra_Income WHERE (SchoolID = @SchoolID) AND (Extra_IncomeCategoryID LIKE @Extra_IncomeCategoryID) AND (Extra_IncomeDate BETWEEN ISNULL(@Fdate, '1-1-1000') AND ISNULL(@TDate, '1-1-3000')) AND (EducationYearID = @EducationYearID) AND (Extra_IncomeID LIKE @Extra_IncomeID)" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>" CancelSelectOnNullParameter="False">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="FindCategoryDropDownList" Name="Extra_IncomeCategoryID" PropertyName="SelectedValue" DefaultValue="" />
                        <asp:ControlParameter ControlID="FormDateTextBox" DefaultValue="" Name="Fdate" PropertyName="Text" />
                        <asp:ControlParameter ControlID="ToDateTextBox" DefaultValue="" Name="TDate" PropertyName="Text" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ReceiptTextBox" DefaultValue="%" Name="Extra_IncomeID" PropertyName="Text" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="ExtraIncomeGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="Extra_IncomeID" DataSourceID="ExtraIncomeSQL" AllowPaging="True" PageSize="20" AllowSorting="True">
                    <Columns>
                        <asp:BoundField DataField="Extra_Income_CategoryName" HeaderText="Category" ReadOnly="True" SortExpression="Extra_Income_CategoryName" />
                        <asp:TemplateField HeaderText="Details" SortExpression="Extra_IncomeFor">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox2" CssClass="form-control" runat="server" Text='<%# Bind("Extra_IncomeFor") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label2" runat="server" Text='<%# Bind("Extra_IncomeFor") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Amount" SortExpression="Extra_IncomeAmount">
                            <EditItemTemplate>
                                <asp:TextBox ID="TextBox1" CssClass="form-control" runat="server" Text='<%# Bind("Extra_IncomeAmount") %>'></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Bind("Extra_IncomeAmount") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Extra_IncomeDate" HeaderText="Date" SortExpression="Extra_IncomeDate" DataFormatString="{0:d MMM yyyy}" ReadOnly="True" />
                        <asp:TemplateField HeaderText="Edit">
                            <EditItemTemplate>
                                <asp:LinkButton ID="UpdateLinkButton" runat="server" CausesValidation="True" CommandName="Update" Text="Updete"></asp:LinkButton>
                                &nbsp;<asp:LinkButton ID="CancelLinkButton" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:LinkButton ID="EditLinkButton" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit"></asp:LinkButton>
                            </ItemTemplate>
                            <HeaderStyle CssClass="d-print-none" />
                            <ItemStyle Width="70px" CssClass="d-print-none" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Delete">
                            <ItemTemplate>
                                <asp:LinkButton ID="DeleteLinkButton" runat="server" CausesValidation="False" OnClientClick="return confirm('Are You Sure Want To Delete?')" CommandName="Delete" Text="Delete"></asp:LinkButton>
                            </ItemTemplate>
                            <HeaderStyle CssClass="d-print-none" />
                            <ItemStyle Width="40px" CssClass="d-print-none" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Receipt">
                            <ItemTemplate>
                                <a href="./Others_Payment_Receipt.aspx?id=<%# Eval("Extra_IncomeID") %>"><%# Eval("Extra_IncomeID") %></a>
                            </ItemTemplate>
                             <HeaderStyle CssClass="d-print-none" />
                            <ItemStyle CssClass="d-print-none" />
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        Empty
                    </EmptyDataTemplate>
                    <PagerStyle CssClass="pgr" />
                </asp:GridView>
                <asp:SqlDataSource ID="ExtraIncomeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    DeleteCommand="SET context_info @RegistrationID DELETE FROM [Extra_Income] WHERE [Extra_IncomeID] = @Extra_IncomeID"
                    InsertCommand="INSERT INTO Extra_Income(SchoolID, RegistrationID, Extra_IncomeCategoryID, Extra_IncomeAmount, Extra_IncomeFor, AccountID, EducationYearID, Extra_IncomeDate) VALUES (@SchoolID, @RegistrationID, @Extra_IncomeCategoryID, @Extra_IncomeAmount, @Extra_IncomeFor, @AccountID, @EducationYearID, @Extra_IncomeDate)"
                    SelectCommand="SELECT Extra_Income.Extra_IncomeAmount, Extra_Income.Extra_IncomeFor, Extra_Income.Extra_IncomeDate, Extra_IncomeCategory.Extra_Income_CategoryName, Extra_Income.Extra_IncomeID, Extra_Income.EducationYearID FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID WHERE (Extra_Income.SchoolID = @SchoolID) AND (Extra_Income.Extra_IncomeCategoryID LIKE @Extra_IncomeCategoryID) AND (Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@Fdate, '1-1-1000') AND ISNULL(@TDate, '1-1-3000')) AND (Extra_Income.EducationYearID = @EducationYearID) AND (Extra_Income.Extra_IncomeID LIKE @Extra_IncomeID) ORDER BY Extra_Income.Insert_Date DESC"
                    UpdateCommand="SET context_info @RegistrationID UPDATE Extra_Income SET Extra_IncomeAmount = @Extra_IncomeAmount, Extra_IncomeFor = @Extra_IncomeFor WHERE (Extra_IncomeID = @Extra_IncomeID)" CancelSelectOnNullParameter="False">
                    <DeleteParameters>
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                        <asp:Parameter Name="Extra_IncomeID" Type="Int32" />
                    </DeleteParameters>
                    <InsertParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                        <asp:ControlParameter ControlID="CategoryDropDownList" Name="Extra_IncomeCategoryID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:ControlParameter ControlID="AmountTextBox" Name="Extra_IncomeAmount" PropertyName="Text" Type="Double" />
                        <asp:ControlParameter ControlID="IncomeForTextBox" Name="Extra_IncomeFor" PropertyName="Text" Type="String" />
                        <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountID" PropertyName="SelectedValue" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="Payment_Date_TextBox" Name="Extra_IncomeDate" PropertyName="Text" />
                    </InsertParameters>
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="FindCategoryDropDownList" Name="Extra_IncomeCategoryID" PropertyName="SelectedValue" DefaultValue="" />
                        <asp:ControlParameter ControlID="FormDateTextBox" DefaultValue="" Name="Fdate" PropertyName="Text" />
                        <asp:ControlParameter ControlID="ToDateTextBox" DefaultValue="" Name="TDate" PropertyName="Text" />
                        <asp:SessionParameter DefaultValue="" Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ReceiptTextBox" DefaultValue="%" Name="Extra_IncomeID" PropertyName="Text" />
                    </SelectParameters>
                    <UpdateParameters>
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                        <asp:Parameter Name="Extra_IncomeAmount" Type="Double" />
                        <asp:Parameter Name="Extra_IncomeFor" Type="String" />
                        <asp:Parameter Name="Extra_IncomeID" Type="Int32" />
                    </UpdateParameters>
                </asp:SqlDataSource>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>


    <!-- Modal Add Category -->
    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">Add Category</div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="upnlUsers" runat="server">
                        <ContentTemplate>
                            <div class="form-inline">
                                <div class="form-group">
                                    <asp:TextBox ID="NewCategoryNameTextBox" placeholder="Category Name" runat="server" CssClass="form-control" />
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="NewCategoryNameTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="G"></asp:RequiredFieldValidator>
                                </div>
                                <div class="form-group">
                                    <asp:Button ID="SaveButton" runat="server" CssClass="btn btn-primary" OnClick="SaveButton_Click" Text="Save" ValidationGroup="G" />
                                </div>
                            </div>

                            <asp:GridView ID="AllCategory" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="Extra_IncomeCategoryID" DataSourceID="NewCategorySQL" AllowPaging="True">
                                <Columns>
                                    <asp:BoundField DataField="Extra_Income_CategoryName" HeaderText="Category" SortExpression="Extra_Income_CategoryName" />
                                    <asp:TemplateField>
                                        <EditItemTemplate>
                                            <asp:LinkButton ID="UpdateLinkButton" runat="server" CausesValidation="True" CommandName="Update" Text="Updete"></asp:LinkButton>
                                            &nbsp;<asp:LinkButton ID="CancelLinkButton" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:LinkButton ID="EditLinkButton" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit"></asp:LinkButton>
                                        </ItemTemplate>
                                        <HeaderStyle CssClass="No_Print" />
                                        <ItemStyle Width="100px" CssClass="No_Print" />
                                    </asp:TemplateField>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:LinkButton ID="DeleteLinkButton" runat="server" CausesValidation="False" OnClientClick="return confirm('Are You Sure Want To Delete?')" CommandName="Delete" Text="Delete"></asp:LinkButton>
                                        </ItemTemplate>
                                        <HeaderStyle CssClass="No_Print" />
                                        <ItemStyle Width="40px" CssClass="No_Print" />
                                    </asp:TemplateField>
                                </Columns>
                                <PagerStyle CssClass="pgr" />
                            </asp:GridView>
                            <asp:SqlDataSource ID="NewCategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Extra_IncomeCategory] WHERE [Extra_IncomeCategoryID] = @Extra_IncomeCategoryID" InsertCommand=" IF NOT EXISTS ( SELECT  * FROM [Extra_IncomeCategory] WHERE (SchoolID = @SchoolID) AND ([Extra_Income_CategoryName]= @Extra_Income_CategoryName))

INSERT INTO [Extra_IncomeCategory] ([SchoolID], [RegistrationID], [Extra_Income_CategoryName]) VALUES (@SchoolID, @RegistrationID, LTRIM(RTRIM(@Extra_Income_CategoryName)))"
                                SelectCommand="SELECT * FROM [Extra_IncomeCategory] WHERE ([SchoolID] = @SchoolID)" UpdateCommand="UPDATE Extra_IncomeCategory SET Extra_Income_CategoryName = LTRIM(RTRIM(@Extra_Income_CategoryName)) WHERE (Extra_IncomeCategoryID = @Extra_IncomeCategoryID)" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                                <DeleteParameters>
                                    <asp:Parameter Name="Extra_IncomeCategoryID" Type="Int32" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                    <asp:ControlParameter ControlID="NewCategoryNameTextBox" Name="Extra_Income_CategoryName" PropertyName="Text" Type="String" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                </SelectParameters>
                                <UpdateParameters>
                                    <asp:Parameter Name="Extra_Income_CategoryName" Type="String" />
                                    <asp:Parameter Name="Extra_IncomeCategoryID" Type="Int32" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>

    <!-- Modal Add Income -->
    <div class="modal fade" id="myModal2" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">Add Income</div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                        <ContentTemplate>
                            <table style="width: 100%">
                                <tr>
                                    <td>Category
                                 <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="CategoryDropDownList" CssClass="EroorSummer" ErrorMessage="আয়ের ধরণ সিলেক্ট করুন" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:DropDownList ID="CategoryDropDownList" runat="server" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="Extra_Income_CategoryName" DataValueField="Extra_IncomeCategoryID" OnDataBound="CategoryDropDownList_DataBound">
                                        </asp:DropDownList>
                                        <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Extra_IncomeCategory] WHERE ([SchoolID] = @SchoolID)" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                                            <SelectParameters>
                                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                            </SelectParameters>
                                        </asp:SqlDataSource>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Amount&nbsp;<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="AmountTextBox" CssClass="EroorSummer" ErrorMessage="Required" ValidationGroup="1">*</asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:TextBox ID="AmountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control" placeholder="Amount"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Payment Add Date
                                 <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="Payment_Date_TextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:TextBox ID="Payment_Date_TextBox" runat="server" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control Datetime"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Details</td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:TextBox ID="IncomeForTextBox" runat="server" CssClass="form-control" placeholder="Details" TextMode="MultiLine"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Account
                              <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="AccountDropDownList" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td>

                                        <asp:DropDownList ID="AccountDropDownList" runat="server" CssClass="form-control" DataSourceID="AccountSQL" DataTextField="AccountName" DataValueField="AccountID">
                                        </asp:DropDownList>

                                        <asp:SqlDataSource ID="AccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AccountID,AccountName  FROM Account WHERE (SchoolID = @SchoolID)" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                                            <SelectParameters>
                                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                            </SelectParameters>
                                        </asp:SqlDataSource>

                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <label id="ErMsg" class="SuccessMessage"></label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="SubmitButton_Click" ValidationGroup="1" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>&nbsp;</td>
                                </tr>
                            </table>
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
                autoclose: true
            });
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
        function EndRequestHandler(sender, args) {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        }

        function Success() {
            var e = $('#ErMsg');
            e.text("Income Added Successfully");
            e.fadeIn();
            e.queue(function () { setTimeout(function () { e.dequeue(); }, 3000); });
            e.fadeOut('slow');
        }

        //Disable the submit button after clicking
        $("form").submit(function () {
            $(".btn btn-primary").attr("disabled", true);
            setTimeout(function () {
                $(".btn btn-primary").prop('disabled', false);
            }, 2000); // 2 seconds
            return true;
        })

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
