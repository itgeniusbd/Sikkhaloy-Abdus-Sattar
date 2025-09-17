<%@ Page Title="Progress Report" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Progress_Report.aspx.cs" Inherits="EDUCATION.COM.Authority.Institutions.Progress_Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        #Count { text-align: center; margin-bottom: 15px; color: #fff; }
        #Count .Box1{ background: -webkit-gradient(linear,left top,right top,from(#0ac282),to(#0df3a3)); background: linear-gradient(to right,#0ac282,#0df3a3); padding: 1rem 0; }
        #Count .Box { background: -webkit-gradient(linear,left top,right top,from(#fe5d70),to(#fe909d)); background: linear-gradient(to right,#fe5d70,#fe909d); padding: 1rem 0; }
        #Count  h5 { margin: 0; }
        #Count  p { margin: 0; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Progress Report</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:RadioButtonList ID="FilterRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control" AutoPostBack="true">
                <asp:ListItem Value="%" Selected="True">All</asp:ListItem>
                <asp:ListItem Value="1">Active</asp:ListItem>
                <asp:ListItem Value="0">Inactive</asp:ListItem>
            </asp:RadioButtonList>
        </div>
    </div>

    <asp:FormView ID="Current_SutFormView" DataSourceID="Current_Sut_SQL" Width="100%" runat="server">
        <ItemTemplate>
            <div class="row" id="Count">
                <div class="col">
                    <div class="Box">
                        <h5><%# Eval("Total_Institution") %></h5>
                        <p>Institution</p>
                    </div>
                </div>
                <div class="col">
                    <div class="Box1">
                        <h5><%# Eval("ActiveStudent") %></h5>
                        <p>Active Student</p>
                    </div>
                </div>
                <div class="col">
                    <div class="Box">
                        <h5><%# Eval("Reject_Countable") %></h5>
                        <p>Reject Countable</p>
                    </div>
                </div>
                <div class="col">
                    <div class="Box1">
                        <h5><%# Eval("Reject_Uncountable") %></h5>
                        <p>Reject Uncountable</p>
                    </div>
                </div>
                <div class="col">
                    <div class="Box">
                        <h5><%# Eval("Total_Countable") %></h5>
                        <p>Total Countable</p>
                    </div>
                </div>
                <div class="col">
                    <div class="Box1">
                        <h5><%# Eval("Service_Charge") %></h5>
                        <p>Service Charge</p>
                    </div>
                </div>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="Current_Sut_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT COUNT(VW_Payment_Monthly_Stu.SchoolID) AS Total_Institution, SUM(VW_Payment_Monthly_Stu.ActiveStudent + VW_Payment_Monthly_Stu.Reject_Countable) AS Total_Countable, SUM(VW_Payment_Monthly_Stu.ActiveStudent) AS ActiveStudent, SUM(VW_Payment_Monthly_Stu.Reject_Countable) AS Reject_Countable, SUM(VW_Payment_Monthly_Stu.Reject_Uncountable) AS Reject_Uncountable, SUM(CASE WHEN SchoolInfo.Fixed = 0 THEN (VW_Payment_Monthly_Stu.ActiveStudent + VW_Payment_Monthly_Stu.Reject_Countable) * SchoolInfo.Per_Student_Rate ELSE SchoolInfo.Fixed END - SchoolInfo.Discount) AS Service_Charge FROM VW_Payment_Monthly_Stu INNER JOIN SchoolInfo ON VW_Payment_Monthly_Stu.SchoolID = SchoolInfo.SchoolID WHERE (SchoolInfo.IS_ServiceChargeActive LIKE @IS_ServiceChargeActive)">
        <SelectParameters>
            <asp:ControlParameter ControlID="FilterRadioButtonList" Name="IS_ServiceChargeActive" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>


    <asp:GridView ID="School_GridView" CssClass="mGrid table-hover" runat="server" AutoGenerateColumns="False" DataSourceID="School_DetailsSQL" AllowSorting="True">
        <Columns>
            <asp:BoundField DataField="SchoolID" HeaderText="School ID" SortExpression="SchoolID" />
            <asp:HyperLinkField SortExpression="SchoolName" DataNavigateUrlFields="SchoolID" DataNavigateUrlFormatString="Institution_Details.aspx?SchoolID={0}" DataTextField="SchoolName" HeaderText="Select" />
            <asp:BoundField DataField="ActiveStudent" HeaderText="Active Student" ReadOnly="True" SortExpression="ActiveStudent" />
            <asp:BoundField DataField="Reject_Countable" HeaderText="Rejt. Countable" ReadOnly="True" SortExpression="Reject_Countable" />
            <asp:BoundField DataField="Reject_Uncountable" HeaderText="Rejt. Uncountable" ReadOnly="True" SortExpression="Reject_Uncountable" />
            <asp:BoundField DataField="Free_SMS" HeaderText="Free SMS" SortExpression="Free_SMS" />
            <asp:BoundField DataField="Countable_Stu" HeaderText="Count Studnt" ReadOnly="True" SortExpression="Countable_Stu">
                <ItemStyle Font-Bold="True" />
            </asp:BoundField>
            <asp:BoundField DataField="Per_Student_Rate" HeaderText="Per Student" SortExpression="Per_Student_Rate" />
            <asp:BoundField DataField="Fixed" HeaderText="Fixed" SortExpression="Fixed" DataFormatString="{0:N0}">
                <ItemStyle HorizontalAlign="Right" />
            </asp:BoundField>
            <asp:BoundField DataField="Discount" HeaderText="Discount" SortExpression="Discount" DataFormatString="{0:N0}">
                <ItemStyle HorizontalAlign="Right" />
            </asp:BoundField>
            <asp:BoundField DataField="Service_Charge" HeaderText="Service Charge" ReadOnly="True" SortExpression="Service_Charge" DataFormatString="{0:N0}">
                <ItemStyle Font-Bold="True" HorizontalAlign="Right" />
            </asp:BoundField>
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="School_DetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT VW_Payment_Monthly_Stu.SchoolID, SchoolInfo.SchoolName, VW_Payment_Monthly_Stu.ActiveStudent, VW_Payment_Monthly_Stu.Reject_Countable, VW_Payment_Monthly_Stu.Reject_Uncountable, SchoolInfo.Per_Student_Rate, SchoolInfo.IS_ServiceChargeActive, SchoolInfo.Discount, SchoolInfo.Fixed, SchoolInfo.Free_SMS, VW_Payment_Monthly_Stu.ActiveStudent + VW_Payment_Monthly_Stu.Reject_Countable AS Countable_Stu, CASE WHEN SchoolInfo.Fixed = 0 THEN (VW_Payment_Monthly_Stu.ActiveStudent + VW_Payment_Monthly_Stu.Reject_Countable) * SchoolInfo.Per_Student_Rate ELSE SchoolInfo.Fixed END - SchoolInfo.Discount AS Service_Charge FROM VW_Payment_Monthly_Stu INNER JOIN SchoolInfo ON VW_Payment_Monthly_Stu.SchoolID = SchoolInfo.SchoolID WHERE (SchoolInfo.IS_ServiceChargeActive LIKE @IS_ServiceChargeActive)">
        <SelectParameters>
            <asp:ControlParameter ControlID="FilterRadioButtonList" Name="IS_ServiceChargeActive" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>
