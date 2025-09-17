<%@ Page Title="Change Payorder Date" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Change_Payorder_Date.aspx.cs" Inherits="EDUCATION.COM.Accounts.Payment.Change_Payorder_Date" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .SuccessMsg { font-size: 15px; color: #32b000; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div id="Contain">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <h3>Change Payorder Date</h3>

                <div class="form-inline">
                    <div class="form-group">
                        <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID">
                            <asp:ListItem Value="0"> [ SELECT CLASS ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                    <div class="form-group">
                        <asp:DropDownList ID="RoleDropDownList" CssClass="form-control" runat="server" DataSourceID="RoleSQL" DataTextField="Role" DataValueField="RoleID" AutoPostBack="True" AppendDataBoundItems="True">
                            <asp:ListItem Value="0">[ SELECT ROLE ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="RoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Role + ' (' + CAST(NumberOfPay AS nvarchar(50)) + ')' AS Role , RoleID  FROM Income_Roles WHERE (SchoolID = @SchoolID)">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" InitialValue="0" runat="server" ControlToValidate="RoleDropDownList" CssClass="EroorStar" ValidationGroup="A">*</asp:RequiredFieldValidator>
                    </div>
                </div>

                <div class="table-responsive pShow" style="display: none;">
                    <div class="alert alert-primary">Payment Role Details</div>
                    <asp:GridView ID="AssignedGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="AssignRoleID,RoleID,PayFor" DataSourceID="AssignedRoleSQL" CssClass="mGrid">
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                                </ItemTemplate>
                                <ItemStyle Width="50px" />
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Pay For" SortExpression="PayFor">
                                <ItemTemplate>
                                    <asp:Label ID="PayforLabel" runat="server" Text='<%# Bind("PayFor") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Amount" SortExpression="Amount">
                                <ItemTemplate>
                                    <asp:Label ID="Label3" runat="server" Text='<%# Bind("Amount") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Late Fee" SortExpression="LateFee">
                                <ItemTemplate>
                                    <asp:Label ID="Label4" runat="server" Text='<%# Bind("LateFee") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Start Date" SortExpression="StartDate">
                                <ItemTemplate>
                                    <asp:TextBox ID="EStDateTextBox" runat="server" Text='<%# Bind("StartDate","{0:d MMM yyyy}") %>' CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="End Date" SortExpression="EndDate">
                                <ItemTemplate>
                                    <asp:TextBox ID="EEdateTextBox" runat="server" Text='<%# Bind("EndDate","{0:d MMM yyyy}") %>' CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="AssignedRoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AssignRoleID, RegistrationID, RoleID, ClassID, EducationYearID, PayFor, Amount, LateFee, StartDate, EndDate, Date FROM Income_Assign_Role WHERE (ClassID = @ClassID) AND (RoleID = @RoleID) AND (EducationYearID = @EducationYearID)" UpdateCommand="UPDATE Income_Assign_Role SET StartDate = @StartDate, EndDate = @EndDate WHERE (AssignRoleID = @AssignRoleID)
UPDATE Income_PayOrder SET StartDate = @StartDate, EndDate = @EndDate WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (RoleID = @RoleID) AND (ClassID = @ClassID) AND (PayFor = @PayFor)
UPDATE  Income_PayOrder SET Is_LateFeeAdded =0 WHERE (EndDate &gt;= GETDATE()) AND (Is_LateFeeAdded = 1) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (RoleID = @RoleID) AND (ClassID = @ClassID) AND (PayFor = @PayFor)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="RoleDropDownList" Name="RoleID" PropertyName="SelectedValue" Type="Int32" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="RoleDropDownList" Name="RoleID" PropertyName="SelectedValue" />
                            <asp:Parameter Name="AssignRoleID" />
                            <asp:Parameter DbType="Date" Name="StartDate" />
                            <asp:Parameter DbType="Date" Name="EndDate" />
                            <asp:Parameter Name="PayFor" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                    <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any Payment role." ForeColor="Red" ValidationGroup="A"> </asp:CustomValidator>
                    <br />
                    <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Change Date" ValidationGroup="A" OnClick="SubmitButton_Click" />
                    <label id="SuccMsg" class="SuccessMsg"></label>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
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
    </div>

    <script type="text/javascript">
        $(function () {
            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {

            if ($("[id*=AssignedGridView] tr").length) {
                $(".pShow").show();
            }

            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            $("[id*=AllIteamCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });
            $("[id*=SingleCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=chkHeader]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=chkRow]", a).length == $("[id*=chkRow]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });

        /*--select at least one Checkbox in GridView-----*/
        function Validate(d, c) {
            for (var b = document.getElementById("<%=AssignedGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                if ("checkbox" == b[a].type && b[a].checked) {
                    c.IsValid = !0;
                    return;
                }
            }
            c.IsValid = !1;
        };

        function Success() {
            var e = $('#SuccMsg');
            e.text("Date Changed successfully!!");
            e.fadeIn();
            e.queue(function () { setTimeout(function () { e.dequeue() }, 3000) });
            e.fadeOut('slow');
        }

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
