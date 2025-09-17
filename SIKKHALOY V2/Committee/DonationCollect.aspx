<%@ Page Title="Donation Collect" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationCollect.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationCollect" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .user-photo { flex-shrink: 0; padding-right: 1rem }
        .user-photo img { height: 160px; width: 160px; }

        /*user active status*/
        .user-activation { text-align: center; font-size: 0.9rem; font-weight: bold; }
        .active-status { color: #00C851 }
        .active-status:before { margin-right: 3px; content: '\f058'; font-family: 'fontawesome'; color: #00C851 }

        .in-active-status { color: #ff4444 }
        .in-active-status:before { margin-right: 3px; content: '\f057'; font-family: 'fontawesome'; color: #ff4444 }

        .info { width: 100%; }
        .info ul { margin: 0; padding: 0 }
        .info ul li { border-bottom: 1px solid #d6e0eb; color: #5d6772; font-size: 15px; line-height: 23px; list-style: outside none none; margin: 6px 0 0; padding-bottom: 5px; padding-left: 2px; }
        .info ul li:last-child { border-bottom: none; }


        #total-pay-amount { font-weight: bold }
        #payment-submit { display: none }
        .row-selected td { background-color: #1CAA56; color: #fff; font-weight: bold; }
        .mGrid td { padding: 0.2rem 0.5rem; font-weight: 400; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Donation Collect</h3>

    <!--find donar-->
    <div class="d-flex align-items-center">
        <div>
            <asp:TextBox ID="FindDonarTextBox" autocomplete="off" runat="server" placeholder="Find Donar" CssClass="form-control"></asp:TextBox>
            <asp:HiddenField ID="HiddenCommitteeMemberId" runat="server" />
        </div>
        <div class="ml-4">
            <asp:Button ID="FindButton" OnClientClick="return checkDonar()" OnClick="FindButton_Click" runat="server" CssClass="btn btn-grey btn-md m-0" Text="Find" />
        </div>
    </div>

    <div class="row mt-4">
        <div class="col-lg-7">
            <asp:FormView ID="MemberInfoFormView" runat="server" DataSourceID="MemberInfoSQL" RenderOuterTable="False">
                <ItemTemplate>
                    <div class="z-depth-1 p-3 mb-4">
                        <div class="d-flex flex-sm-row flex-column text-center text-sm-left">
                            <div class="user-photo">
                                <img alt="photo" src="data:image/jpg;base64, <%# Convert.ToBase64String(string.IsNullOrEmpty(Eval("Photo").ToString())? new byte[]{}: (byte[]) Eval("Photo")) %>" onerror="this.src='/Handeler/Default/Male.png'" class="img-thumbnail rounded-circle img-fluid z-depth-1" />
                                <div class="user-activation">
                                    <%# Eval("CommitteeMemberType") %>
                                </div>
                            </div>
                            <div class="info">
                                <ul>
                                    <li><strong><%# Eval("MemberName") %></strong></li>
                                    <li><b>Total Donation: </b><%# Eval("TotalDonation") %></li>
                                    <li><b>Total Paid: </b><%# Eval("PaidDonation") %></li>
                                    <li><b>Phone: </b><%# Eval("SmsNumber") %></li>
                                    <li><b>Address: </b><%# Eval("Address") %></li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="MemberInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CommitteeMember.MemberName, CommitteeMember.SmsNumber, CommitteeMember.Address, CommitteeMember.Photo, CommitteeMember.TotalDonation, CommitteeMember.PaidDonation, CommitteeMemberType.CommitteeMemberType FROM CommitteeMember INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId WHERE (CommitteeMember.SchoolID = @SchoolID) AND (CommitteeMember.CommitteeMemberId = @CommitteeMemberId)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="HiddenCommitteeMemberId" Name="CommitteeMemberId" PropertyName="Value" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="col-lg-5">
            <asp:GridView ID="PaidRecordGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="PRecordSQL" AllowPaging="True" PageSize="3">
                <Columns>
                    <asp:TemplateField HeaderText="Receipt">
                        <ItemTemplate>
                           <p class="mb-0"><%# Eval("CommitteeMoneyReceiptSn") %></p>
                            <small class="d-block"><%# Eval("PaidDate", "{0:d MMM, yyyy}") %></small>
                        </ItemTemplate>
                        <HeaderStyle HorizontalAlign="Left" />
                        <ItemStyle HorizontalAlign="Left" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Paid">
                        <ItemTemplate>
                         <%# Eval("TotalAmount") %> Tk
                        <a class="d-block" href="DonationReceipt.aspx?id=<%# Eval("CommitteeMoneyReceiptId") %>">
                            <i class="fa fa-print"></i>
                            Print
                        </a>
                        </ItemTemplate>
                        <HeaderStyle HorizontalAlign="Right" />
                        <ItemStyle HorizontalAlign="Right" />
                    </asp:TemplateField>
                </Columns>
                <PagerStyle CssClass="pgr" />
            </asp:GridView>
            <asp:SqlDataSource ID="PRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT TotalAmount, PaidDate, CommitteeMoneyReceiptId, CommitteeMoneyReceiptSn FROM CommitteeMoneyReceipt WHERE (EducationYearId = @EducationYearID) AND (SchoolId = @SchoolID) AND (CommitteeMemberId = @CommitteeMemberId) ORDER BY PaidDate DESC">
                <SelectParameters>
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="HiddenCommitteeMemberId" Name="CommitteeMemberId" PropertyName="Value" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
    </div>


    <!--donations gridview-->
    <div id="payment-container" class="my-4">
        <asp:GridView ID="DonationGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="CommitteeDonationId" DataSourceID="DonationSQL">
            <Columns>
                <asp:TemplateField HeaderText="Select">
                    <ItemTemplate>
                        <asp:CheckBox ID="DueCheckBox" CssClass="due-checkbox" runat="server" Text=" " />
                    </ItemTemplate>
                    <ItemStyle Width="100px" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Donation Category" SortExpression="Amount">
                    <ItemTemplate>
                        <%# Eval("DonationCategory") %>
                    </ItemTemplate>
                    <HeaderStyle HorizontalAlign="Left" />
                    <ItemStyle HorizontalAlign="Left" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Description" SortExpression="Description">
                    <ItemTemplate>
                        <%# Eval("Description") %>
                    </ItemTemplate>
                    <HeaderStyle HorizontalAlign="Left" />
                    <ItemStyle HorizontalAlign="Left" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Amount" SortExpression="Amount">
                    <ItemTemplate>
                        ৳<%# Eval("Amount") %>
                    </ItemTemplate>
                    <HeaderStyle HorizontalAlign="Right" />
                    <ItemStyle HorizontalAlign="Right" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Paid" SortExpression="PaidAmount">
                    <ItemTemplate>
                        ৳<%# Eval("PaidAmount") %>
                    </ItemTemplate>
                    <HeaderStyle HorizontalAlign="Right" />
                    <ItemStyle HorizontalAlign="Right" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                    <ItemTemplate>
                        ৳<%# Eval("Due") %>
                    </ItemTemplate>
                    <HeaderStyle HorizontalAlign="Right" />
                    <ItemStyle HorizontalAlign="Right" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Collect" SortExpression="Due">
                    <ItemTemplate>
                        <asp:TextBox ID="PaidAmountTextBox" type="number" step="0.01" min="0" max='<%# Eval("Due") %>' required="" Enabled="false" CssClass="form-control due-input" runat="server" Text='<%# Eval("Due") %>' autocomplete="off" />
                    </ItemTemplate>
                    <ItemStyle Width="150px" />
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="DonationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT CommitteeDonation.CommitteeDonationId, CommitteeDonation.CommitteeDonationCategoryId, CommitteeDonationCategory.DonationCategory, CommitteeDonation.Amount, CommitteeDonation.PaidAmount, CommitteeDonation.Due, CommitteeDonation.IsPaid, CommitteeDonation.Description, CommitteeDonation.InsertDate, CommitteeDonation.PromiseDate FROM CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId WHERE (CommitteeDonation.SchoolID = @SchoolID) AND (CommitteeDonation.Due > 0) AND (CommitteeDonation.CommitteeMemberId = @CommitteeMemberId)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:ControlParameter ControlID="HiddenCommitteeMemberId" Name="CommitteeMemberId" PropertyName="Value" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <!--submit button-->
    <div id="payment-submit" class="mt-4">
        <h4 id="total-pay-amount"></h4>

        <div class="form-inline">
            <div class="form-group">
                <asp:DropDownList ID="AccountDropDownList" runat="server" CssClass="form-control" DataSourceID="AccountSQL" DataTextField="AccountName" DataValueField="AccountID">
                </asp:DropDownList>
                <asp:SqlDataSource ID="AccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [AccountID], [AccountName] FROM [Account] WHERE ([SchoolID] = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="form-group mx-3">
                <asp:TextBox ID="CollectDateTextBox" placeholder="Collect Date" runat="server" autocomplete="off" CssClass="form-control date-picker"></asp:TextBox>
            </div>

            <div class="form-group">
                <asp:Button ID="CollectButton" OnClick="CollectButton_Click" runat="server" Text="Submit" OnClientClick="return validateForm()" CssClass="btn btn-success" />
            </div>
        </div>

        <asp:SqlDataSource ID="ReceiptSQL" runat="server"
            InsertCommand="INSERT INTO CommitteeMoneyReceipt (RegistrationId, SchoolId, CommitteeMemberId, EducationYearId, AccountId, CommitteeMoneyReceiptSn, PaidDate) 
                       VALUES (@RegistrationID, @SchoolID, @CommitteeMemberId, @EducationYearId, @AccountId, [dbo].[F_CommitteeMoneyReceiptSn](@SchoolID), @PaidDate);
                       SELECT @CommitteeMoneyReceiptId = SCOPE_IDENTITY();"
            OnInserted="ReceiptSQL_Inserted" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeMoneyReceiptId FROM CommitteeMoneyReceipt">
            <InsertParameters>
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearId" SessionField="Edu_Year" />
                <asp:ControlParameter ControlID="HiddenCommitteeMemberId" Name="CommitteeMemberId" PropertyName="Value" />
                <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountId" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="CollectDateTextBox" Name="PaidDate" PropertyName="Text" />
                <asp:Parameter Direction="Output" Name="CommitteeMoneyReceiptId" Size="50" />
            </InsertParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="PaymentRecordSQL" runat="server"
            InsertCommand="INSERT INTO CommitteePaymentRecord (SchoolId, RegistrationId, CommitteeDonationId, CommitteeMoneyReceiptId, PaidAmount) VALUES (@SchoolID, @RegistrationID, @CommitteeDonationId, @CommitteeMoneyReceiptId, @PaidAmount)" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteePaymentRecordId FROM CommitteePaymentRecord">
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:Parameter Name="PaidAmount" />
                <asp:Parameter Name="CommitteeDonationId" />
                <asp:Parameter Name="CommitteeMoneyReceiptId" />
            </InsertParameters>
        </asp:SqlDataSource>
    </div>


    <script type="text/javascript">
        //date picker
        $(".date-picker").datepicker({
            format: 'dd M yyyy',
            todayBtn: "linked",
            todayHighlight: true,
            autoclose: true
        }).datepicker("setDate", "0");

        //find donar
        const committeeMemberId = document.getElementById("<%= HiddenCommitteeMemberId.ClientID %>");

        //autocomplete
        $('[id*=FindDonarTextBox]').typeahead({
            displayText: function (item) {
                return `${item.MemberName}, ${item.SmsNumber}`;
            },
            afterSelect: function (item) {
                this.$element[0].value = item.MemberName
            },
            source: function (request, result) {
                $.ajax({
                    url: "/Handeler/FindDonar.asmx/FindDonarAutocomplete",
                    data: JSON.stringify({ 'prefix': request }),
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    success: function (response) { result(JSON.parse(response.d)); },
                    error: function (err) { console.log(err) }
                });
            },
            updater: function (item) {
                committeeMemberId.value = item.CommitteeMemberId;
                return item;
            }
        });


        //show payment submit area if dues
        const donationsTable = document.getElementById("<%= DonationGridView.ClientID %>");
        const paymentSubmit = document.getElementById("payment-submit");

        if (donationsTable && donationsTable.rows.length) {
            paymentSubmit.style.display = "block";
        }


        //due checkbox and input due amount
        (function () {
            //calculate total dues
            function calculateTotal() {
                const inputedDues = document.querySelectorAll(".due-input:not([disabled])");
                let total = 0;
                inputedDues.forEach((item => {
                    total += Number(item.value);
                }));

                return total;
            }

            //click checkbox and input dues
            const totalPayAmount = document.getElementById("total-pay-amount");
            const paymentTable = document.getElementById("payment-container");

            paymentTable.addEventListener("input",
                function (evt) {
                    const element = evt.target;

                    if (element.type === "checkbox") {
                        element.closest("tr").classList.toggle("row-selected");

                        const input = element.closest("tr").querySelector('.due-input');
                        input.disabled = !element.checked;
                    }

                    const total = `Total Amount: <span id="total-amount-pay">${calculateTotal()}</span> Tk`;

                    totalPayAmount.innerHTML = total;
                });
        })();

        //Uncheck due select checkbox if page reload
        const checkboxes = document.querySelectorAll("input[type='checkbox']");
        for (const checkbox of checkboxes) {
            checkbox.checked = false;
        }

        //validate form before submit pay
        function validateForm() {
            if (!committeeMemberId.value) {
                $.notify("Donar not found!", { position: "top center" });
                return false;
            }

            const isChecked = [...checkboxes].some(item => item.checked);

            if (isChecked) {
                return true;
            }

            $.notify("Select payment to pay!", { position: "top center" });
            return false;
        }
    </script>
</asp:Content>
