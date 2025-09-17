<%@ Page Title="Payment Success" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Success.aspx.cs" Inherits="EDUCATION.COM.Student.OnlinePayment.Payment_Success"  %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <style>


    </style>
    <h3>Accounts</h3>

    <!--Money receipt-->
<%--    <asp:UpdatePanel ID="UpdatePanel9" runat="server">
        <ContentTemplate>
            <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
                <div class="modal-dialog cascading-modal" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h4 class="title">Money Receipt Details</h4>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        </div>
                        <div class="modal-body mb-0">
                            <asp:GridView ID="AllPaidRGridView" runat="server" AutoGenerateColumns="False" DataSourceID="AllPayRecordSQL" CssClass="mGrid">
                                <Columns>
                                    <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                                    <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                                    <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                                    <asp:BoundField DataField="PaidDate" HeaderText="Paid Date" SortExpression="PaidDate" DataFormatString="{0:dd-MMM-yyyy}" />
                                </Columns>
                            </asp:GridView>
                            <asp:SqlDataSource ID="AllPayRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                SelectCommand="SELECT Income_PaymentRecord.PayOrderID, Income_PaymentRecord.PaidAmount, Income_PaymentRecord.PayFor, Income_PaymentRecord.PaidDate, Income_Roles.Role FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID WHERE (Income_PaymentRecord.MoneyReceiptID = @MoneyReceiptID)">
                                <SelectParameters>
                                    <asp:Parameter Name="MoneyReceiptID" Type="Int32" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>

                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>--%>

        <!--submit button-->
    <div id="payment-submit" class="mt-4">
        <div class="form-inline">
            <div class="form-group">
                <asp:Button ID="PayButton" runat="server" Text="Submit Payment"  CssClass="btn btn-primary" />
            </div>
        </div>
    </div>




    <script>
        $(function () {

        });

    </script>
</asp:Content>
