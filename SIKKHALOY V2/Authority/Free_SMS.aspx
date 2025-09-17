<%@ Page Title="Manage Institution" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Free_SMS.aspx.cs" Inherits="EDUCATION.COM.Authority.Free_SMS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .Invaid_Ins td { color: #ff2b2b; }
        .Invaid_Ins td a { color: #ff2b2b; }
    </style>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="body" runat="server">
    <h3>Manage Institution</h3>
    <div class="form-inline">
        <div class="md-form">
            <asp:TextBox ID="SearchTextBox" placeholder="Institution, Username" CssClass="form-control" runat="server"></asp:TextBox>
        </div>
        <div class="md-form">
            <asp:Button ID="FIndButton" runat="server" Text="Find" CssClass="btn btn-primary btn-sm" />
        </div>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="SchoolGridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" DataKeyNames="SchoolID" DataSourceID="InstitutionSQL" CssClass="mGrid">
            <Columns>
                <asp:BoundField DataField="School_SN" HeaderText="SN" SortExpression="School_SN" />
                <asp:BoundField DataField="SchoolID" HeaderText="School ID" SortExpression="SchoolID" InsertVisible="False" ReadOnly="True" />
                <asp:BoundField DataField="SchoolName" HeaderText="Name" SortExpression="SchoolName" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:TemplateField HeaderText="Validation">
                    <ItemTemplate>
                        <asp:CheckBox ID="Validation_CheckBox" Checked='<%#Bind("Validation") %>' Text=" " runat="server" />
                        <input type="hidden" class="IS_Valid" value="<%#Eval("Validation") %>" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Payment Active">
                    <ItemTemplate>
                        <asp:CheckBox ID="Payment_Active_CheckBox" Checked='<%#Bind("IS_ServiceChargeActive") %>' Text=" " runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Per Student" SortExpression="Per_Student_Rate">
                    <ItemTemplate>
                        <asp:TextBox ID="Per_Student_TextBox" ToolTip="Per Student" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" runat="server" Text='<%# Bind("Per_Student_Rate") %>'></asp:TextBox>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Free SMS" SortExpression="Free_SMS">
                    <ItemTemplate>
                        <asp:TextBox ID="Free_SMS_TextBox" ToolTip="Free SMS" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" runat="server" Text='<%# Bind("Free_SMS") %>'></asp:TextBox>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Discount" SortExpression="Discount">
                    <ItemTemplate>
                        <asp:TextBox ID="Discount_TextBox" ToolTip="Discount" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" runat="server" Text='<%# Bind("Discount") %>'></asp:TextBox>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Fixed" SortExpression="Fixed">
                    <ItemTemplate>
                        <asp:TextBox ID="Fixed_TextBox" ToolTip="Fixed" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" runat="server" Text='<%# Bind("Fixed") %>'></asp:TextBox>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="InstitutionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Per_Student_Rate, School_SN, SchoolID, SchoolName, Date, Address, Phone, Free_SMS, Fixed, Discount, IS_ServiceChargeActive, CAST(CASE WHEN Validation = 'Valid' THEN 1 ELSE 0 END AS BIT) AS Validation, UserName FROM SchoolInfo AS Sch ORDER BY School_SN" UpdateCommand="UPDATE SchoolInfo SET Free_SMS = @Free_SMS, Discount = @Discount, Fixed = @Fixed, IS_ServiceChargeActive = @IS_ServiceChargeActive, Validation = @Validation, Per_Student_Rate=@Per_Student_Rate WHERE (SchoolID = @SchoolID)"
            FilterExpression="SchoolName like '{0}%' OR UserName like '{0}%'">
            <FilterParameters>
                <asp:ControlParameter ControlID="SearchTextBox" Name="Find" PropertyName="Text" />
            </FilterParameters>
            <UpdateParameters>
                <asp:Parameter Name="Free_SMS" />
                <asp:Parameter Name="Discount" />
                <asp:Parameter Name="Fixed" />
                <asp:Parameter Name="SchoolID" />
                <asp:Parameter Name="IS_ServiceChargeActive" />
                <asp:Parameter Name="Validation" />
                <asp:Parameter Name="Per_Student_Rate" />
            </UpdateParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="DeviceActiveInactiveSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AttendanceSettingID FROM Attendance_Device_Setting " UpdateCommand="UPDATE  Attendance_Device_Setting SET  IsActive = @IsActive WHERE (SchoolID = @SchoolID)">
            <UpdateParameters>
                <asp:Parameter Name="IsActive" />
                <asp:Parameter Name="SchoolID" />
            </UpdateParameters>
        </asp:SqlDataSource>
    </div>
    <div class="form-group">
        <br />
        <asp:Button ID="UpdateButton" runat="server" Text="Update" CssClass="btn btn-primary" OnClick="UpdateButton_Click" />
    </div>

    <script>
        $(function () {
            $('.mGrid tr').each(function () {
                if ($(this).find('.IS_Valid').val() === "False") {
                    $(this).addClass("Invaid_Ins");
                }
            });
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
