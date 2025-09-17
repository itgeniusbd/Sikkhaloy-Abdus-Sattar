<%@ Page Title="Payment Record" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="PaymentRecord.aspx.cs" Inherits="EDUCATION.COM.Committee.PaymentRecord" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Payment Record</h3>

    <div class="d-flex align-items-center">
        <div class="form-group">
            <label>Session</label>
            <asp:DropDownList ID="SessionDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="SessionSQL" DataTextField="EducationYear" DataValueField="EducationYearID">
                <asp:ListItem Value="%">[ ALL Session ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="SessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Education_Year.EducationYearID, Education_Year.EducationYear FROM Education_Year INNER JOIN Income_MoneyReceipt ON Education_Year.EducationYearID = Income_MoneyReceipt.EducationYearID WHERE (Income_MoneyReceipt.SchoolID = @SchoolID) ORDER BY Education_Year.EducationYearID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group ml-3">
            <label>Donation Type</label>
            <asp:DropDownList ID="DonationCategoryDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="SqlCommitteeDonationCategory" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId">
                <asp:ListItem Value="%">[ ALL Type ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="SqlCommitteeDonationCategory" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeDonationCategoryId, DonationCategory FROM CommitteeDonationCategory WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="form-group ml-3">
            <label>Committee Member</label>
            <asp:DropDownList ID="CommitteeMemberDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="true" CssClass="form-control" DataSourceID="CommitteeMemberSQL" DataTextField="MemberName" DataValueField="CommitteeMemberId">
                <asp:ListItem Value="%">[ All ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="CommitteeMemberSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CommitteeMemberId, MemberName FROM CommitteeMember WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group mx-3">
            <label>From Date</label>
            <asp:TextBox ID="FormDateTextBox" runat="server" autocomplete="off" CssClass="form-control Datetime"></asp:TextBox>
        </div>
        <div class="form-group">
            <label>To Date</label>
            <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control Datetime"></asp:TextBox>
        </div>
        <div class="form-group ml-3" style="padding-top: 1.8rem">
            <asp:Button ID="FindButton" runat="server" CssClass="btn btn-outline-primary btn-md" Text="Find" />
        </div>
        <div class="form-group ml-3" style="padding-top: 1.8rem">
            <input id="PrintButton" type="button" value="Print" onclick="window.print();" class="btn btn-info" />
        </div>
    </div>

    <asp:FormView ID="TotalFormView" runat="server" DataSourceID="TotalSQL" RenderOuterTable="false">
        <ItemTemplate>
            <div class="alert alert-secondary amount">
                Total: <%#Eval("TOTAL") %> Tk
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="TotalSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT        COALESCE (SUM(CommitteeMoneyReceipt.TotalAmount), 0) AS TOTAL
FROM            CommitteeMoneyReceipt INNER JOIN
                         CommitteeMember ON CommitteeMoneyReceipt.CommitteeMemberId = CommitteeMember.CommitteeMemberId INNER JOIN
                         CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId INNER JOIN
                         Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID
WHERE        (CommitteeMoneyReceipt.SchoolId = @SchoolId) AND (CommitteeMoneyReceipt.EducationYearId LIKE @EducationYearId) AND (CAST(CommitteeMoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') 
                         AND ISNULL(@To_Date, '1-1-3000')) AND (CommitteeMoneyReceipt.CommitteeMemberId LIKE @CommitteeMemberId) AND (CommitteeMoneyReceipt.CommitteeMoneyReceiptId IN
                             (SELECT        CommitteePaymentRecord.CommitteeMoneyReceiptId
                               FROM            CommitteeDonationCategory INNER JOIN
                                                         CommitteeDonation ON CommitteeDonationCategory.CommitteeDonationCategoryId = CommitteeDonation.CommitteeDonationCategoryId INNER JOIN
                                                         CommitteePaymentRecord ON CommitteeDonation.CommitteeDonationId = CommitteePaymentRecord.CommitteeDonationId
                               WHERE        (CommitteeDonationCategory.CommitteeDonationCategoryId LIKE @CommitteeDonationCategoryId) AND (CommitteeDonationCategory.SchoolID = @SchoolId)))">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="SessionDownList" Name="EducationYearId" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="FormDateTextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="CommitteeMemberDropDownList" Name="CommitteeMemberId" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="DonationCategoryDropDownList" Name="CommitteeDonationCategoryId" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>

    <div class="table-responsive mt-2">
        <asp:GridView ID="PaymentRecordGridView" AllowSorting="True" AllowPaging="True" PageSize="50" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="CommitteeMoneyReceiptId" DataSourceID="PaymentRecordSQL">
            <Columns>
                <asp:TemplateField HeaderText="Receipt" SortExpression="CommitteeMoneyReceiptSn">
                    <ItemTemplate>
                        <a href="./DonationReceipt.aspx?id=<%# Eval("CommitteeMoneyReceiptId") %>"><%# Eval("CommitteeMoneyReceiptSn") %></a>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="MemberName" HeaderText="Member Name" SortExpression="MemberName" />
                <asp:BoundField DataField="CommitteeMemberType" HeaderText="Member Type" SortExpression="CommitteeMemberType" />
                <asp:BoundField DataField="SmsNumber" HeaderText="Sms Number" SortExpression="SmsNumber" />
                <asp:BoundField DataField="AccountName" HeaderText="Account" SortExpression="AccountName" />
                <asp:TemplateField HeaderText="Details">
                    <ItemTemplate>
                        <asp:Repeater ID="PRRepeater" runat="server" DataSourceID="PRSQL">
                            <ItemTemplate>
                                <p class="mb-0 text-right"><%# Eval("DonationCategory") +" "+Eval("Description",", {0}")+" :  "+Eval("PaidAmount") %>/-</p>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:HiddenField ID="MRidHiddenField" runat="server" Value='<%# Bind("CommitteeMoneyReceiptId")%>' />
                        <asp:SqlDataSource ID="PRSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT CommitteePaymentRecord.PaidAmount, CommitteeDonationCategory.DonationCategory, CommitteeDonation.Description, CommitteePaymentRecord.CommitteeMoneyReceiptId FROM  CommitteePaymentRecord INNER JOIN
                            CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId INNER JOIN
                            CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId WHERE (CommitteePaymentRecord.CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId)">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="MRidHiddenField" Name="CommitteeMoneyReceiptId" PropertyName="Value" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="TotalAmount" HeaderText="Total" SortExpression="TotalAmount" />
                <asp:BoundField DataField="PaidDate" HeaderText="Paid Date" SortExpression="PaidDate" DataFormatString="{0:d MMM yyyy}" />
            </Columns>
        </asp:GridView>
    </div>

    <asp:SqlDataSource ID="PaymentRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT        CommitteeMoneyReceipt.CommitteeMoneyReceiptId, CommitteeMoneyReceipt.CommitteeMemberId, CommitteeMoneyReceipt.CommitteeMoneyReceiptSn, CommitteeMoneyReceipt.TotalAmount, 
                         CommitteeMoneyReceipt.PaidDate, CommitteeMemberType.CommitteeMemberType, CommitteeMember.MemberName, CommitteeMember.SmsNumber, Account.AccountName
FROM            CommitteeMoneyReceipt INNER JOIN
                         CommitteeMember ON CommitteeMoneyReceipt.CommitteeMemberId = CommitteeMember.CommitteeMemberId INNER JOIN
                         CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId INNER JOIN
                         Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID
WHERE        (CommitteeMoneyReceipt.SchoolId = @SchoolId) AND (CommitteeMoneyReceipt.EducationYearId LIKE @EducationYearId) AND (CAST(CommitteeMoneyReceipt.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, '1-1-1000') 
                         AND ISNULL(@To_Date, '1-1-3000')) AND (CommitteeMoneyReceipt.CommitteeMemberId LIKE @CommitteeMemberId) AND (CommitteeMoneyReceipt.CommitteeMoneyReceiptId IN
                             (SELECT        CommitteePaymentRecord.CommitteeMoneyReceiptId
                               FROM            CommitteeDonationCategory INNER JOIN
                                                         CommitteeDonation ON CommitteeDonationCategory.CommitteeDonationCategoryId = CommitteeDonation.CommitteeDonationCategoryId INNER JOIN
                                                         CommitteePaymentRecord ON CommitteeDonation.CommitteeDonationId = CommitteePaymentRecord.CommitteeDonationId
                               WHERE        (CommitteeDonationCategory.CommitteeDonationCategoryId LIKE @CommitteeDonationCategoryId) AND (CommitteeDonationCategory.SchoolID = @SchoolId)))"
        CancelSelectOnNullParameter="False">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="SessionDownList" Name="EducationYearId" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="FormDateTextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="CommitteeMemberDropDownList" Name="CommitteeMemberId" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="DonationCategoryDropDownList" Name="CommitteeDonationCategoryId" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $('.Datetime').datepicker({
            format: 'dd M yyyy',
            todayBtn: "linked",
            todayHighlight: true,
            autoclose: true
        });
    </script>
</asp:Content>
