<%@ Page Title="Assign Payment Role" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Assign_Payment_Roles.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.Payment.Assign_Payment_Roles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        #Contain .form-control { display: inline; width: 97%; }
         .UAR2 table tr td{position:relative; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div id="Contain">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <h3>Assign Payment Installment In a Role</h3>
                <a href="Assign_Pay_Role_Multi_Class.aspx">Assign Payment Role By Multi Class >></a>

                <div class="form-inline">
                    <div class="form-group">
                        <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" AutoPostBack="True" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                            <asp:ListItem Value="0"> [ SELECT CLASS ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <asp:RequiredFieldValidator ID="cRf" InitialValue="0" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorStar" ValidationGroup="A">*</asp:RequiredFieldValidator>
                    </div>
                    <div class="form-group">
                        <asp:DropDownList ID="RoleDropDownList" CssClass="form-control" runat="server" DataSourceID="RoleSQL" DataTextField="Role" DataValueField="RoleID_No" OnSelectedIndexChanged="RoleDropDownList_SelectedIndexChanged" AutoPostBack="True" AppendDataBoundItems="True">
                            <asp:ListItem Value="0">[ SELECT ROLE ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="RoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Role + ' (' + CAST(NumberOfPay AS nvarchar(50)) + ')' AS Role, CAST(RoleID AS nvarchar(50)) + ',' + CAST(NumberOfPay AS nvarchar(50)) AS RoleID_No FROM Income_Roles WHERE (SchoolID = @SchoolID)">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" InitialValue="0" runat="server" ControlToValidate="RoleDropDownList" CssClass="EroorStar" ValidationGroup="A">*</asp:RequiredFieldValidator>
                    </div>
                </div>

                <%if (IncludRoleGridView.Rows.Count > 0)
                { %>
                <div class="alert alert-info">Total Payment Installment of This Role</div>
                <div class="table-responsive mb-3 UAR2">
                    <asp:GridView ID="IncludRoleGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid">
                        <Columns>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:CheckBox ID="ActiveCheckBox" runat="server" AutoPostBack="True" Checked="True" OnCheckedChanged="ActiveCheckBox_CheckedChanged" Text=" " />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Pay For">
                                <ItemTemplate>
                                    <asp:TextBox ID="PayForTextBox" autocomplete="off" runat="server" CssClass="form-control PayFor"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="payForRF" runat="server" ControlToValidate="PayForTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Amount">
                                <ItemTemplate>
                                    <asp:TextBox ID="AmountTextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Start Date">
                                <ItemTemplate>
                                    <asp:TextBox ID="StartDateTextBox" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="StartdateRF" runat="server" ControlToValidate="StartDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="End Date">
                                <ItemTemplate>
                                    <asp:TextBox ID="EndDateTextBox" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="EndDateRF" runat="server" ControlToValidate="EndDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Late Fee">
                                <ItemTemplate>
                                    <asp:TextBox ID="LateFeeTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control"></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="AssignRoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Income_Assign_Role(SchoolID, RegistrationID, RoleID, ClassID, EducationYearID, PayFor, Amount, LateFee, StartDate, EndDate, Date) VALUES (@SchoolID, @RegistrationID, @RoleID, @ClassID, @EducationYearID, @PayFor, @Amount, @LateFee, @StartDate, @EndDate, GETDATE())" OldValuesParameterFormatString="original_{0}" SelectCommand="SELECT AssignRoleID, RegistrationID, SchoolID, RoleID, ClassID, PayFor, Amount, LateFee, StartDate, EndDate, EducationYearID, Date FROM Income_Assign_Role WHERE (SchoolID = @SchoolID)">
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:Parameter Name="RoleID" />
                            <asp:Parameter Name="PayFor" Type="String" />
                            <asp:Parameter Name="Amount" Type="Double" />
                            <asp:Parameter Name="LateFee" Type="Double" />
                            <asp:Parameter DbType="Date" Name="StartDate" />
                            <asp:Parameter DbType="Date" Name="EndDate" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <asp:Button ID="RoleButton" runat="server" Text="Submit" OnClick="RoleButton_Click" CssClass="btn btn-primary" ValidationGroup="A" />
                <%} %>

                <%if (AssignedGridView.Rows.Count > 0)
                {%>
                <div class="alert alert-primary mt-3">Already Assigned Role of This Class</div>
                <asp:GridView ID="AssignedGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="AssignRoleID" DataSourceID="AssignedRoleSQL" OnRowDeleted="AssignedGridView_RowDeleted" CssClass="mGrid">
                    <Columns>
                        <asp:TemplateField HeaderText="Pay For" SortExpression="PayFor">
                            <EditItemTemplate>
                                <asp:TextBox ID="EPayforTextBox" runat="server" Text='<%# Bind("PayFor") %>' CssClass="form-control"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="EPayforTextBox" CssClass="EroorStar" ErrorMessage="!" SetFocusOnError="True"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="PayforLabel" runat="server" Text='<%# Bind("PayFor") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Amount" SortExpression="Amount">
                            <EditItemTemplate>
                                <asp:TextBox ID="EAmountTextBox" runat="server" Text='<%# Bind("Amount") %>' CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label3" runat="server" Text='<%# Bind("Amount") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Late Fee" SortExpression="LateFee">
                            <EditItemTemplate>
                                <asp:TextBox ID="ElfeeTextBox" runat="server" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" Text='<%# Bind("LateFee") %>' CssClass="form-control"></asp:TextBox>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label4" runat="server" Text='<%# Bind("LateFee") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Start Date" SortExpression="StartDate">
                            <EditItemTemplate>
                                <asp:TextBox ID="EStDateTextBox" runat="server" Text='<%# Bind("StartDate","{0:d MMM yyyy}") %>' CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="EStDateTextBox" CssClass="EroorStar" ErrorMessage="!" SetFocusOnError="True"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Bind("StartDate", "{0:d MMM yyyy}") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="End Date" SortExpression="EndDate">
                            <EditItemTemplate>
                                <asp:TextBox ID="EEdateTextBox" runat="server" Text='<%# Bind("EndDate","{0:d MMM yyyy}") %>' CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="EEdateTextBox" CssClass="EroorStar" ErrorMessage="!" SetFocusOnError="True"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label2" runat="server" Text='<%# Bind("EndDate", "{0:d MMM yyyy}") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:CommandField ShowEditButton="True" />
                        <asp:TemplateField ShowHeader="False">
                            <ItemTemplate>
                                <asp:LinkButton ID="DeleteLinkButton" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are you sure want to delete?')"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="AssignedRoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Income_Assign_Role] WHERE [AssignRoleID] = @AssignRoleID" SelectCommand="SELECT AssignRoleID, RegistrationID, RoleID, ClassID, EducationYearID, PayFor, Amount, LateFee, StartDate, EndDate, Date FROM Income_Assign_Role WHERE (ClassID = @ClassID) AND (RoleID = @RoleID) AND (EducationYearID = @EducationYearID)" UpdateCommand="UPDATE Income_Assign_Role SET Amount = @Amount, LateFee = @LateFee, StartDate = @StartDate, EndDate = @EndDate, PayFor = @PayFor WHERE (AssignRoleID = @AssignRoleID)">
                    <DeleteParameters>
                        <asp:Parameter Name="AssignRoleID" Type="Int32" />
                    </DeleteParameters>
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:Parameter Name="RoleID" Type="Int32" />
                    </SelectParameters>
                    <UpdateParameters>
                        <asp:Parameter Name="Amount" Type="Double" />
                        <asp:Parameter Name="LateFee" Type="Double" />
                        <asp:Parameter DbType="Date" Name="StartDate" />
                        <asp:Parameter DbType="Date" Name="EndDate" />
                        <asp:Parameter Name="PayFor" />
                        <asp:Parameter Name="AssignRoleID" Type="Int32" />
                    </UpdateParameters>
                </asp:SqlDataSource>
                <%} %>
            </ContentTemplate>
        </asp:UpdatePanel>
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
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            $('.PayFor').on("keypress", function () {
                var tr = $(this).closest("tr");
                $(this).typeahead({
                    minLength: 1,
                    source: function (request, result) {
                        $.ajax({
                            url: "Assign_Payment_Roles.aspx/GetMonth",
                            data: JSON.stringify({ 'prefix': request }),
                            dataType: "json",
                            type: "POST",
                            contentType: "application/json; charset=utf-8",
                            success: function (response) {
                                label = [];
                                map = {};
                                $.map(JSON.parse(response.d), function (item) {
                                    label.push(item.Month);
                                    map[item.Month] = item;
                                });
                                result(label);
                            }
                        });
                    },
                    updater: function (item) {
                        $(".Datetime:eq(0)", tr).val("01 " + map[item].MonthYear);
                        $(".Datetime:eq(1)", tr).val("10 " + map[item].MonthYear);
                        return item;
                    }
                });
            });
        });
    </script>
</asp:Content>
