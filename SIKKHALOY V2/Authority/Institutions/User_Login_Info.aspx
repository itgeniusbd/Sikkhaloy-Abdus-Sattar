<%@ Page Title="User Login Info" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="User_Login_Info.aspx.cs" Inherits="EDUCATION.COM.Authority.Institutions.User_Login_Info" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>User Login Info</h3>
    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="FromDateTextBox" placeholder="From Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
        </div>
        <div class="form-group mx-2">
            <asp:TextBox ID="ToDateTextBox" placeholder="To Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:RadioButtonList ID="UserRadioButtonList" CssClass="form-control" runat="server" RepeatDirection="Horizontal">
                <asp:ListItem Selected="True">Student</asp:ListItem>
                <asp:ListItem>Teacher</asp:ListItem>
            </asp:RadioButtonList>
        </div>
        <div class="form-group">
            <asp:Button ID="SearchButton" runat="server" Text="Search" CssClass="btn btn-primary" />
        </div>
    </div>

    <asp:GridView ID="UserGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataSourceID="UserSQL" AllowSorting="True" ShowFooter="True">
        <Columns>
            <asp:BoundField DataField="SchoolName" HeaderText="Institution" SortExpression="SchoolName" />
            <asp:TemplateField HeaderText="Total Login" SortExpression="Login_Student_No">
                <ItemTemplate>
                    <span class="LoginUser"><%# Eval("Login_Student_No") %></span>
                </ItemTemplate>
                <FooterTemplate>
                    <label id="Gtotal"></label>
                </FooterTemplate>
            </asp:TemplateField>
        </Columns>
        <FooterStyle CssClass="GridFooter" />
    </asp:GridView>
    <asp:SqlDataSource ID="UserSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SchoolInfo.SchoolName, COUNT(Registration.UserName) AS Login_Student_No FROM aspnet_Users INNER JOIN Registration ON aspnet_Users.UserName = Registration.UserName INNER JOIN SchoolInfo ON Registration.SchoolID = SchoolInfo.SchoolID WHERE (Registration.Category = @Category) AND (aspnet_Users.LastActivityDate BETWEEN ISNULL(@Sdate, '1-1-2000') AND ISNULL(@Edate, '1-1-3000')) GROUP BY SchoolInfo.SchoolName ORDER BY Login_Student_No DESC" CancelSelectOnNullParameter="False">
        <SelectParameters>
            <asp:ControlParameter ControlID="UserRadioButtonList" Name="Category" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="FromDateTextBox" Name="Sdate" PropertyName="Text" />
            <asp:ControlParameter ControlID="ToDateTextBox" Name="Edate" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            var total = 0;
            $($(".LoginUser")).each(function () {
                if (!isNaN(parseInt($(this).text()))) {
                    total += parseInt($(this).text());
                }
            });
            $("#Gtotal").html("Total: " + total);
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
