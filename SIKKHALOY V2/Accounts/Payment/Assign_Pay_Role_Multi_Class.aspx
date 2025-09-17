<%@ Page Title="Assign Pay Role Multi Class" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Assign_Pay_Role_Multi_Class.aspx.cs" Inherits="EDUCATION.COM.Accounts.Payment.Assign_Pay_Role_Multi_Class" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .Role_Name { font-size: 15px; font-weight: bold; }
        .MultiRoleGV tr td { border: 1px solid #ddd; padding: 6px 0; }
        #Contain .form-control { display: inline; width: 96%; }
        #Mtime table tr td { position: relative; }
        .UAR2 table tr td { position: relative; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div id="Contain">
        <h3>Assign Payment Installment In All Roles</h3>
        <a href="Assign_Payment_Roles.aspx"><< Back</a>

        <div class="row mb-4">
            <div class="col">
                <asp:CheckBoxList ID="ClassCheckBoxList" CssClass="DefaultCheckBoxList" runat="server" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" RepeatDirection="Horizontal" RepeatLayout="Flow">
                </asp:CheckBoxList>
                <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:Button ID="FindButton" runat="server" Text="Submit" CssClass="btn btn-blue-grey btn-sm" OnClick="FindButton_Click" />
            </div>
        </div>

        <div id="1time" class="table-responsive mb-4" style="display: none;">
            <div class="alert alert-secondary">Assign One Installment</div>
            <asp:GridView ID="One_Role_GridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="Roles_1_SQL" DataKeyNames="RoleID">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:CheckBox ID="One_Role_CheckBox" runat="server" Text=" " />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                    <asp:TemplateField HeaderText="Pay For">
                        <ItemTemplate>
                            <asp:TextBox ID="One_PayForTextBox" runat="server" CssClass="form-control One_Role" placeholder="Pay For"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="payForRF" Enabled="false" runat="server" ControlToValidate="One_PayForTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Amount">
                        <ItemTemplate>
                            <asp:TextBox ID="One_AmountTextBox" placeholder="Amount" runat="server" CssClass="form-control One_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Start Date">
                        <ItemTemplate>
                            <asp:TextBox ID="One_StartDateTextBox" placeholder="Start Date" runat="server" CssClass="form-control Datetime One_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="StartdateRF" Enabled="false" runat="server" ControlToValidate="One_StartDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="End Date">
                        <ItemTemplate>
                            <asp:TextBox ID="One_EndDateTextBox" placeholder="End Date" runat="server" CssClass="form-control Datetime One_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="EndDateRF" Enabled="false" runat="server" ControlToValidate="One_EndDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Late Fee">
                        <ItemTemplate>
                            <asp:TextBox ID="One_LateFeeTextBox" placeholder="Late Fee" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control One_Role"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="Roles_1_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoleID, Role, NumberOfPay FROM Income_Roles
WHERE (SchoolID = @SchoolID) AND   (NumberOfPay = 1) AND
RoleID NOT IN (SELECT RoleID FROM Income_Assign_Role WHERE ClassID in (select id from [dbo].[In_Function_Parameter](@ClassIDs)) AND EducationYearID = @EducationYearID)">
                <SelectParameters>
                    <asp:Parameter Name="ClassIDs" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div id="Mtime" class="table-responsive mb-4" style="display: none;">
            <div class="alert alert-primary">Assign Multiple Installment</div>
            <asp:GridView ID="Multi_Role_GridView" runat="server" AutoGenerateColumns="False" DataKeyNames="RoleID,NumberOfPay" DataSourceID="OtherRolesSQL" ShowHeader="False" Width="100%" OnRowDataBound="Multi_Role_GridView_RowDataBound" CssClass="MultiRoleGV">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <div class="Role_Name">
                                <asp:CheckBox ID="Multi_AddCheckBox" runat="server" Text=" " />
                                Add -
                                <asp:Label ID="RoleLabel" runat="server" Text='<%# Bind("Role") %>'></asp:Label>
                                ( Number of Payment Instalment 
                                        <asp:Label ID="Multi_No_PayLabel" runat="server" Text='<%# Bind("NumberOfPay") %>'></asp:Label>)
                            </div>

                            <div class="criteriaData" style="display: none">
                                <asp:GridView ID="Input_Multi_Role_GridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" Width="100%">
                                    <Columns>
                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="Input_MultiCheckBox" runat="server" Text=" " />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Pay For">
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_PayForTextBox" placeholder="Pay For" runat="server" CssClass="form-control PayFor"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="payForRF" Enabled="false" runat="server" ControlToValidate="Multi_PayForTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                                <small>Amount Same For All</small><br />
                                                <asp:TextBox ID="AssignAmountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Amount Same For All" runat="server"></asp:TextBox>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_AmountTextBox" placeholder="Amount" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Start Date">
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_StartDateTextBox" placeholder="Start Date" runat="server" CssClass="form-control Datetime Multi_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="StartdateRF" Enabled="false" runat="server" ControlToValidate="Multi_StartDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="End Date">
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_EndDateTextBox" placeholder="End Date" runat="server" CssClass="form-control Datetime Multi_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="EndDateRF" Enabled="false" runat="server" ControlToValidate="Multi_EndDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                                <small>Late Fee Same For All</small><br />
                                                <asp:TextBox ID="AssinLFeeTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Late Fee Same For All" runat="server"></asp:TextBox>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_LateFeeTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" placeholder="Late Fee" runat="server" CssClass="form-control"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="OtherRolesSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoleID, Role, NumberOfPay FROM Income_Roles
WHERE (SchoolID = @SchoolID) AND   (NumberOfPay &lt;&gt; 1) AND
RoleID NOT IN (SELECT RoleID FROM Income_Assign_Role WHERE ClassID in (select id from [dbo].[In_Function_Parameter](@ClassIDs)) AND EducationYearID = @EducationYearID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:Parameter Name="ClassIDs" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <%if (One_Role_GridView.Rows.Count > 0 || Multi_Role_GridView.Rows.Count > 0)
            {%>
        <asp:Button ID="RoleButton" runat="server" Text="Assign" OnClick="RoleButton_Click" CssClass="btn btn-primary" ValidationGroup="A" />
        <asp:SqlDataSource ID="AssignRoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Income_Assign_Role(SchoolID, RegistrationID, RoleID, ClassID, EducationYearID, PayFor, Amount, LateFee, StartDate, EndDate, Date) VALUES (@SchoolID, @RegistrationID, @RoleID, @ClassID, @EducationYearID, @PayFor, @Amount, @LateFee, @StartDate, @EndDate, GETDATE())" OldValuesParameterFormatString="original_{0}" SelectCommand="SELECT AssignRoleID, RegistrationID, SchoolID, RoleID, ClassID, PayFor, Amount, LateFee, StartDate, EndDate, EducationYearID, Date FROM Income_Assign_Role WHERE (SchoolID = @SchoolID)">
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:Parameter Name="RoleID" Type="Int32" />
                <asp:Parameter Name="ClassID" Type="Int32" />
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
        <%}%>
    </div>

    <script type="text/javascript">
        $(function () {
            $('.PayFor').on("keypress", function () {
                var tr = $(this).closest("tr");
                $(this).typeahead({
                    minLength: 1,
                    source: function (request, result) {
                        $.ajax({
                            url: "Assign_Pay_Role_Multi_Class.aspx/GetMonth",
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

            if ($('[id*=One_Role_GridView] tr').length) {
                $("#1time").show();
            }

            if ($('[id*=Multi_Role_GridView] tr').length) {
                $("#Mtime").show();
            }

            //One Role CheckBox
            $("[id*=One_Role_CheckBox]").on("click", function () {
                ValidatorEnable($("[id*=payForRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=StartdateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=EndDateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
            });

            //Multi Role CheckBox
            $("[id*=Multi_AddCheckBox]").on("click", function () {
                if ($(this).is(':checked')) {
                    $(this).closest("tr").find("div.criteriaData").show("shlow");
                }
                else {
                    $(this).closest("tr").find("div.criteriaData").hide("shlow");
                }
            });

            $("[id*=Multi_AddCheckBox]").on("click", function () {
                var Multi_AddCheckBox = $(this);
                var grid = $(this).closest("tr").find("div.criteriaData");

                $("input[type=checkbox]", grid).each(function () {
                    if (Multi_AddCheckBox.is(":checked")) {
                        $(this).attr("checked", "checked");
                        ValidatorEnable($("[id*=payForRF]", $(this).closest("tr"))[0], true);
                        ValidatorEnable($("[id*=StartdateRF]", $(this).closest("tr"))[0], true);
                        ValidatorEnable($("[id*=EndDateRF]", $(this).closest("tr"))[0], true);
                    }
                    else {
                        $(this).removeAttr("checked");
                        ValidatorEnable($("[id*=payForRF]", $(this).closest("tr"))[0], false);
                        ValidatorEnable($("[id*=StartdateRF]", $(this).closest("tr"))[0], false);
                        ValidatorEnable($("[id*=EndDateRF]", $(this).closest("tr"))[0], false);
                    }
                });
            });

            $("[id*=Input_MultiCheckBox]").on("click", function () {
                ValidatorEnable($("[id*=payForRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=StartdateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=EndDateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
            });

            //Assign Amount to All
            $("[id*=AssignAmountTextBox]").on("keyup", function () {
                $("[id*=Multi_AmountTextBox]", $(this).closest("tr td")).val($.trim($(this).val()));
            });

            //Assign Late Fee to All
            $("[id*=AssinLFeeTextBox]").on("keyup", function () {
                $("[id*=Multi_LateFeeTextBox]", $(this).closest("tr td")).val($.trim($(this).val()));
            });

            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
