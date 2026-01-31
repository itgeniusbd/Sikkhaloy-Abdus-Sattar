<%@ Page Title="Donation Receipt" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationReceipt.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationReceipt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .info { text-align: center; color: #000; font-weight: 400; margin-bottom: 6px; }
        .mGrid td { padding: 3px; border: 1px solid #a0a0a0; }
        .grid-footer td { font-weight: bold; }
        .received-by-user-container { text-align: center; color: #333; font-size: 11px; margin-top: 5px; }

        /* Payment Receipt Header */
        .payment-receipt-header {
            text-align: center;
            font-size: 18px;
            font-weight: bold;
            color: #333;
            margin: 0px 0 5px 0;
            padding: 3px 0;
            border-top: 1px solid #333;
            border-bottom: 1px solid #333;
            letter-spacing: 1px;
            text-transform: uppercase;
        }

        /* Two Column Layout for Receipt Info */
        .receipt-info-container {
            display: flex;
            justify-content: space-between;
            margin: 5px 0 10px 0;
            padding: 1px;
            border: 1px solid #ddd;
            text-align: left;
        }

        .receipt-info-left {
            flex: 1;
            padding: 5px 15px;
            border-right: 1px solid #ddd;
        }

        .receipt-info-right {
            flex: 1;
            padding: 5px 15px;
        }

        .receipt-info-left p, .receipt-info-right p {
            margin: 5px 0;
            font-size: 12px;
            border-bottom: 1px solid #eee;
            color: #000 !important;
            padding-bottom: 2px;
        }

        .receipt-info-left p:last-child, .receipt-info-right p:last-child {
            border-bottom: none;
        }

        .receipt-info-container strong {
            font-weight: bold;
            color: #000 !important;
        }

        @page { margin: 0 13.3rem !important; }

        @media print {
            .logo-waper { display: none; }
            #header { margin-bottom: 0px !important; padding-top: 0px !important; border-bottom: none !important; }

            /*for black and white page*/
            .bg-main { background-color: #fff; color: #000; box-shadow: none !important }
            #InstitutionName { font-weight: bold; color: #000 !important; }
            
            .payment-receipt-header {
                font-size: 16px;
                border-top: 1px solid #000;
                border-bottom: 1px solid #000;
            }
            .receipt-info-container {
                border: 1px solid #000;
            }
            .receipt-info-left {
                border-right: 1px solid #000;
            }
        }
    </style>
    <!--add dynamic css for printing-->
    <style type="text/css" media="print" id="print-content"></style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <a class="d-print-none" href="DonationCollect.aspx"><< Back To</a>

    <div class="payment-receipt-header">Donation Receipt</div>

    <asp:FormView ID="InfoFormView" runat="server" DataKeyNames="MemberName,TotalAmount,SmsNumber,CommitteeMoneyReceiptSn" DataSourceID="InfoSQL" RenderOuterTable="False">
        <ItemTemplate>
            <div class="receipt-info-container dynamic-font-size">
                <div class="receipt-info-left">
                    <p><strong>Donor:</strong> <%# Eval("MemberName") %> (<%# Eval("CommitteeMemberType") %>)</p>
                    <p><strong>Mobile:</strong> <%# Eval("SmsNumber") %></p>
                    <p><strong>Receipt No:</strong> <%# Eval("CommitteeMoneyReceiptSn") %></p>
                </div>
                <div class="receipt-info-right">
                    <p><strong>Date:</strong> <%# Eval("PaidDate","{0:d MMM yyyy}") %></p>
                    <p><strong>Payment Method:</strong> <%# Eval("AccountName") %></p>
                    <p><strong>Address:</strong> <%# Eval("Address") %></p>
                </div>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="InfoSQL" runat="server" 
        ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CommitteeMoneyReceipt.CommitteeMoneyReceiptId, CommitteeMoneyReceipt.CommitteeMemberId, CommitteeMoneyReceipt.AccountId, CommitteeMoneyReceipt.CommitteeMoneyReceiptSn, 
                         CommitteeMoneyReceipt.TotalAmount, CommitteeMoneyReceipt.PaidDate, CommitteeMember.MemberName, CommitteeMember.SmsNumber, CommitteeMember.Address, CommitteeMemberType.CommitteeMemberType, 
                         Account.AccountName, Education_Year.EducationYear, Admin.FirstName, Admin.LastName
        FROM CommitteeMoneyReceipt 
        INNER JOIN CommitteeMember ON CommitteeMoneyReceipt.CommitteeMemberId = CommitteeMember.CommitteeMemberId 
        LEFT JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId 
        LEFT JOIN Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID 
        LEFT JOIN Education_Year ON CommitteeMoneyReceipt.EducationYearId = Education_Year.EducationYearID 
        LEFT JOIN Admin ON CommitteeMoneyReceipt.RegistrationId = Admin.RegistrationID
        WHERE (CommitteeMoneyReceipt.SchoolId = @SchoolId) AND (CommitteeMoneyReceipt.CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" Type="Int32" />
            <asp:QueryStringParameter Name="CommitteeMoneyReceiptId" QueryStringField="id" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>


    <asp:GridView ID="PaymentGridView" runat="server" AutoGenerateColumns="False" DataSourceID="PaymentSQL" CssClass="mGrid" Font-Bold="False" RowStyle-CssClass="Rows">
        <Columns>
            <asp:BoundField DataField="DonationCategory" HeaderText="Category" SortExpression="DonationCategory" />
            <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
            <asp:TemplateField HeaderText="Paid Amount" SortExpression="PaidAmount">
                <ItemTemplate>
                    <label class="paid-amount">
                        <%# Eval("PaidAmount") %> TK
                    </label>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <RowStyle CssClass="Rows" />
    </asp:GridView>
    <asp:SqlDataSource ID="PaymentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CommitteePaymentRecord.SchoolId, CommitteePaymentRecord.PaidAmount, CommitteeDonationCategory.DonationCategory, CommitteeDonation.Description FROM CommitteePaymentRecord INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId WHERE (CommitteePaymentRecord.SchoolId = @SchoolId) AND (CommitteePaymentRecord.CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolId" SessionField="SchoolID" Type="Int32" />
            <asp:QueryStringParameter Name="CommitteeMoneyReceiptId" QueryStringField="id" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

    <div class="text-right mt-2 dynamic-font-size">
        <label class="m-0" style="white-space: nowrap">
            <strong id="total-paid"></strong>
            TK
        </label>
        <p class="m-0" id="amount-in-word" style="color: #666;"></p>
    </div>

    <div id="CurrentDueSection" style="margin-top: 20px;">
        <h5 style="border-bottom: 1px solid #333; display: inline-block; font-size: 14px; font-weight: bold; margin-bottom: 5px;">Current Due</h5>
        <asp:GridView ID="UnpaidGridView" runat="server" AutoGenerateColumns="False" DataSourceID="UnpaidSQL" CssClass="mGrid" Font-Bold="False" RowStyle-CssClass="Rows">
            <Columns>
                <asp:BoundField DataField="DonationCategory" HeaderText="Category" />
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <asp:TemplateField HeaderText="Due Amount">
                    <ItemTemplate>
                        <label class="due-amount">
                            <%# Eval("Due") %> TK
                        </label>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <RowStyle CssClass="Rows" />
        </asp:GridView>
        <asp:SqlDataSource ID="UnpaidSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT CommitteeDonationCategory.DonationCategory, CommitteeDonation.Description, CommitteeDonation.Due FROM CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId WHERE (CommitteeDonation.SchoolID = @SchoolID) AND (CommitteeDonation.Due > 0) AND (CommitteeDonation.PromiseDate < CAST(GETDATE() AS DATE)) AND (CommitteeDonation.CommitteeMemberId = (SELECT TOP 1 CommitteeMemberId FROM CommitteeMoneyReceipt WHERE CommitteeMoneyReceiptId = @CommitteeMoneyReceiptId))">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:QueryStringParameter Name="CommitteeMoneyReceiptId" QueryStringField="id" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>


    <p class="mt-3" style="font-style: italic;">আল্লাহ আপনার দান কবুল করুন, দুনিয়া ও আখেরাতে উত্তম বিনিময় দান করুন। </p>

    <asp:FormView ID="FooterFormView" runat="server" DataSourceID="InfoSQL" RenderOuterTable="False">
        <ItemTemplate>
            <div class="received-by-user-container">
                (© Sikkhaloy.com) Received By: <%# Eval("FirstName") %> <%# Eval("LastName") %>
            </div>
        </ItemTemplate>
    </asp:FormView>


    <div class="d-print-none my-4 card">
        <div class="card-header">
            <h4 class="card-title mb-0">
                <i class="fa fa-print"></i>
                Print Options
            </h4>
        </div>
        <div class="card-body">
            <div class="d-flex align-items-center">
                <div>
                    <input id="checkboxInstitution" type="checkbox" />
                    <label for="checkboxInstitution">Hide Institution Name</label>
                </div>
                <div class="ml-3">
                    <input id="checkboxDueDetails" type="checkbox" />
                    <label for="checkboxDueDetails">Hide Current Due</label>
                </div>
            </div>

            <div class="d-flex align-items-center mt-3">
                <div>
                    <label for="inputTopSpace">Page Space From Top (px)</label>
                    <input id="inputTopSpace" min="0" type="number" class="form-control" />
                </div>
                <div class="ml-3">
                    <label for="inputFontSize">Font Size (px)</label>
                    <input id="inputFontSize" min="10" max="20" type="number" class="form-control" />
                </div>
            </div>
        </div>
        <div class="card-footer">
            <input id="PrintButton" type="button" value="Print" onclick="window.print();" class="btn btn-info" />
        </div>
    </div>

    <div class="d-print-none mt-4">
        <asp:FormView ID="SMSFormView" runat="server" DataKeyNames="SMSID" DataSourceID="SMS_OtherInfoSQL" RenderOuterTable="false">
            <ItemTemplate>
                <div>Remaining SMS: <%# Eval("SMS_Balance") %></div>
            </ItemTemplate>
        </asp:FormView>

        <asp:Button ID="SMSButton" runat="server" Text="Send SMS" CssClass="btn btn-primary" OnClick="SMSButton_Click" />
        <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>

        <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT * FROM [SMS] WHERE ([SchoolID] = @SchoolID)"
            InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID, CommitteeMemberId) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID, @CommitteeMemberId)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
            <InsertParameters>
                <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
                <asp:Parameter Name="SchoolID" />
                <asp:Parameter Name="StudentID" />
                <asp:Parameter Name="TeacherID" />
                <asp:Parameter Name="EducationYearID" />
                <asp:Parameter Name="CommitteeMemberId" />
            </InsertParameters>
        </asp:SqlDataSource>
    </div>


    <!--Amount in word js-->
    <script src="../../JS/amount-in-word.js"></script>
    <script>
        $(function () {
            //remove donation add saved record
            sessionStorage.removeItem("_committee_");

            //show total in gridview footer
            let total = 0;
            $(".paid-amount").each(function () {
                total += parseFloat($(this).text())
            });

            $("#total-paid").text(`Total: ${total}`);
            const inWord = number2text(total);
            document.getElementById("amount-in-word").textContent = inWord;

            // Hide Current Due section if empty
            if ($('#<%= UnpaidGridView.ClientID %> tr').length <= 1) {
                $("#CurrentDueSection").hide();
                $(".ml-3:has(#checkboxDueDetails)").hide();
            }
        });


        //print options
        let printingOptions = {
            isInstitutionName: false,
            topSpace: 0,
            fontSize: 11
        };

        const stores = {
            set: function () {
                localStorage.setItem('receipt-printing', JSON.stringify(printingOptions));
            },
            get: function () {
                const data = localStorage.getItem("receipt-printing");

                if (data) printingOptions = JSON.parse(data);
            }
        }

        const printContent = document.getElementById("print-content");
        const checkboxInstitution = document.getElementById("checkboxInstitution");
        const checkboxDueDetails = document.getElementById("checkboxDueDetails");
        const currentDueSection = document.getElementById("CurrentDueSection");
        const header = document.getElementById("header");
        const institutionInfo = document.querySelector(".bg-main");

        const inputTopSpace = document.getElementById("inputTopSpace");
        const inputFontSize = document.getElementById("inputFontSize");

        //institution name show/hide checkbox
        checkboxInstitution.addEventListener("change", function () {
            printingOptions.isInstitutionName = this.checked;

            stores.set();
            bindPrintOption();
        });

        //due details show/hide checkbox
        checkboxDueDetails.addEventListener("change", function () {
            if (this.checked) {
                currentDueSection.classList.add("d-print-none");
            } else {
                currentDueSection.classList.remove("d-print-none");
            }
        });

        //input top space
        inputTopSpace.addEventListener("input", function () {
            printingOptions.topSpace = +this.value

            stores.set();
            bindPrintOption();
        });

        //input font size
        inputFontSize.addEventListener("input", function () {
            const min = +this.min;
            const size = +this.value;

            if (min < size) {
                printingOptions.fontSize = size;

                stores.set();
                bindPrintOption();
            }
        });

        //bind selected options
        function bindPrintOption() {
            stores.get();

            //institution show hide
            checkboxInstitution.checked = printingOptions.isInstitutionName;
            printingOptions.isInstitutionName ? institutionInfo.classList.add("d-print-none") : institutionInfo.classList.remove("d-print-none");

            //space from top
            inputTopSpace.value = printingOptions.topSpace;

            //font size
            inputFontSize.value = printingOptions.fontSize;
            printContent.textContent = `
                 #InstitutionName { font-size: ${printingOptions.fontSize + 4}px}
                 .dynamic-font-size {font-size: ${printingOptions.fontSize + 1}px !important}
                 #header { padding-top: ${printingOptions.topSpace}px}
                .InsInfo p { font-size: ${printingOptions.fontSize + 1}px}
                .mGrid th { font-size: ${printingOptions.fontSize}px}
                .mGrid td { font-size: ${printingOptions.fontSize}px}`;
        }

        bindPrintOption();
    </script>
</asp:Content>
