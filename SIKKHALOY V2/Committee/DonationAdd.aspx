<%@ Page Title="Add Donation" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationAdd.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationAdd" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        #donar-info { display: flex; align-items: center; margin-top: 15px; }
        #donar-info span { border: 1px solid #a5f3b7; margin-right: 8px; padding: 3px 11px; border-radius: 5px; background-color: #e0ffe6; color: #22c147; font-weight: 500; }
        #donar-info span:last-child { cursor: pointer; border: none; border-radius: 5px; background-color: #fff; color: #ff3547; padding: 0 }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="row">
        <div class="col-md-10 col-lg-8 mx-auto">
            <div class="card card-body">
                <h2 class="font-weight-bold mb-3">Add Donation</h2>

                <div class="form-group">
                    <label>
                        Find Donar
                        <a class="ml-1 blue-text" data-toggle="modal" data-target="#modalDonarForm">Add New</a>
                    </label>
                    <asp:TextBox ID="FindDonarTextBox" autocomplete="off" runat="server" CssClass="form-control" required=""></asp:TextBox>
                    <asp:HiddenField ID="HiddenCommitteeMemberId" runat="server" />
                    <div id="donar-info"></div>
                </div>
                <div class="form-group">
                    <label>
                        Donation Category
                    <a class="ml-1" href="DonationCategory.aspx">Add New</a>
                    </label>
                    <asp:DropDownList ID="CategoryDownList" required="" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId">
                        <asp:ListItem Value="">[ Select Category ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT CommitteeDonationCategoryId, DonationCategory FROM CommitteeDonationCategory WHERE (SchoolID = @SchoolID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <label>Donation Amount</label>
                    <asp:TextBox ID="DonationAmountTextBox" onchange="setMaxPaidAmount()" min="0.01" step="0.01" type="number" runat="server" CssClass="form-control" required=""></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Descriptions</label>
                    <asp:TextBox ID="DescriptionsTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Promised Date</label>
                    <asp:TextBox ID="PromisedDateTextBox" autocomplete="off" runat="server" CssClass="form-control date-picker"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Paid Amount</label>
                    <asp:TextBox ID="PaidAmountTextBox" autocomplete="off" min="1" step="0.01" oninput="onChangePaidAmount(this)" type="number" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Paid Date</label>
                    <asp:TextBox ID="PaidDateTextBox" autocomplete="off" runat="server" CssClass="form-control date-picker" disabled=""></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Account</label>
                    <asp:DropDownList ID="AccountDropDownList" runat="server" CssClass="form-control" DataSourceID="AccountSQL" DataTextField="AccountName" DataValueField="AccountID">
                    </asp:DropDownList>

                    <asp:SqlDataSource ID="AccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AccountID,AccountName,Default_Status FROM Account WHERE (SchoolID = @SchoolID)" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="mt-4">
                    <asp:Button ID="SubmitButton" OnClientClick="return isValidForm()" OnClick="SubmitButton_Click" runat="server" CssClass="btn btn-primary m-0" Text="Submit" />

                    <asp:SqlDataSource ID="AddDonationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        InsertCommand="INSERT INTO CommitteeDonation(SchoolID, RegistrationID, CommitteeMemberId, CommitteeDonationCategoryId, Amount, Description, PromiseDate) 
                        VALUES(@SchoolID,@RegistrationID,@CommitteeMemberId,@CommitteeDonationCategoryId,@Amount,@Description,@PromiseDate); 
                        SELECT @CommitteeDonationId = SCOPE_IDENTITY();"
                        OnInserted="AddDonationSQL_Inserted"
                        DeleteCommand="DELETE FROM CommitteeDonation WHERE (CommitteeDonationId = @CommitteeDonationId) AND (PaidAmount = 0)"
                        UpdateCommand="UPDATE CommitteeDonation SET CommitteeDonationCategoryId = @CommitteeDonationCategoryId, Amount = CASE WHEN PaidAmount &gt; @Amount THEN Amount ELSE @Amount END, Description = @Description WHERE (CommitteeDonationId = @CommitteeDonationId)">

                        <DeleteParameters>
                            <asp:Parameter Name="CommitteeDonationId" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                            <asp:ControlParameter ControlID="HiddenCommitteeMemberId" Name="CommitteeMemberId" PropertyName="Value" />
                            <asp:ControlParameter ControlID="CategoryDownList" Name="CommitteeDonationCategoryId" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="DonationAmountTextBox" Name="Amount" PropertyName="Text" />
                            <asp:ControlParameter ControlID="DescriptionsTextBox" Name="Description" PropertyName="Text" />
                            <asp:ControlParameter ControlID="PromisedDateTextBox" Name="PromiseDate" PropertyName="Text" />
                            <asp:Parameter Name="CommitteeDonationId" Direction="Output" Size="100" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:Parameter Name="CommitteeMemberId" DefaultValue="%" />
                            <asp:Parameter DefaultValue="%" Name="CommitteeDonationCategoryId" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="CommitteeDonationCategoryId" />
                            <asp:Parameter Name="Amount" />
                            <asp:Parameter Name="Description" />
                            <asp:Parameter Name="CommitteeDonationId" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="ReceiptSQL" runat="server"
                        InsertCommand="INSERT INTO CommitteeMoneyReceipt (RegistrationId, SchoolId, CommitteeMemberId, EducationYearId, AccountId, CommitteeMoneyReceiptSn, PaidDate) 
                       VALUES (@RegistrationID, @SchoolID, @CommitteeMemberId, @EducationYearId, @AccountId, [dbo].[F_CommitteeMoneyReceiptSn](@SchoolID), @PaidDate);
                       SELECT @CommitteeMoneyReceiptId = SCOPE_IDENTITY();"
                        OnInserted="ReceiptSQL_Inserted" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeMoneyReceiptId FROM CommitteeMoneyReceipt">
                        <InsertParameters>
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="HiddenCommitteeMemberId" Name="CommitteeMemberId" PropertyName="Value" />
                            <asp:SessionParameter Name="EducationYearId" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="AccountDropDownList" Name="AccountId" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="PaidDateTextBox" Name="PaidDate" PropertyName="Text" />
                            <asp:Parameter Direction="Output" Name="CommitteeMoneyReceiptId" Size="50" />
                        </InsertParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="PaymentRecordSQL" runat="server"
                        InsertCommand="INSERT INTO CommitteePaymentRecord (SchoolId, RegistrationId, CommitteeDonationId, CommitteeMoneyReceiptId, PaidAmount) VALUES (@SchoolID, @RegistrationID, @CommitteeDonationId, @CommitteeMoneyReceiptId, @PaidAmount)" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteePaymentRecordId FROM CommitteePaymentRecord">
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                            <asp:Parameter Name="CommitteeDonationId" />
                            <asp:Parameter Name="CommitteeMoneyReceiptId" />
                            <asp:ControlParameter ControlID="PaidAmountTextBox" Name="PaidAmount" PropertyName="Text" />
                        </InsertParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
        </div>
    </div>

    <%--add donar--%>
    <div class="modal fade" id="modalDonarForm" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header text-center">
                    <h4 class="modal-title w-100 font-weight-bold">Add New Member</h4>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>

                <div class="modal-body mx-3">
                    <div class="form-group">
                        <label>Name</label>
                        <asp:TextBox ID="MemberNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>
                            Member Type<a class="ml-1" href="MemberType.aspx">Add New</a>
                        </label>
                        <asp:DropDownList ID="TypeDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId">
                            <asp:ListItem Value="">[ All Type ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID)">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                    <div class="form-group">
                        <label>Phone</label>
                        <asp:TextBox ID="PhoneTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Address</label>
                        <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>

                <div class="modal-footer d-flex justify-content-center">
                    <button id="btnMemberAdd" type="button" class="btn btn-default">Add</button>
                </div>
            </div>
        </div>
    </div>


    <script type="text/javascript">
        $(function () {
            //date picker
            $(".date-picker").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });


            //find donar
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
                    sessionStorage.setItem("_committee_", JSON.stringify(item));
                    showDonarInfo();

                    return item;
                }
            });
        });

        //add new donar
        const btnMemberAdd = document.getElementById("btnMemberAdd");
        btnMemberAdd.addEventListener("click", function (evt) {
            evt.preventDefault();

            const CommitteeMemberTypeId = document.getElementById("<%= TypeDropDownList.ClientID %>");
            const MemberName = document.getElementById("<%= MemberNameTextBox.ClientID %>");
            const SmsNumber = document.getElementById("<%= PhoneTextBox.ClientID %>");
            const Address = document.getElementById("<%= AddressTextBox.ClientID %>");

            if (!CommitteeMemberTypeId.value || !MemberName.value || !SmsNumber.value) {
                $.notify("Member name, type, phone required", "error");
                return;
            }

            const model = {
                CommitteeMemberTypeId: CommitteeMemberTypeId.value,
                MemberName: MemberName.value,
                SmsNumber: SmsNumber.value,
                Address: Address.value
            }

            $.ajax({
                url: "DonationAdd.aspx/DonerAddApi",
                data: JSON.stringify({ model }),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    sessionStorage.setItem("_committee_", response.d);
                    showDonarInfo();
                    $("#modalDonarForm").modal("hide");

                    MemberName.value = "";
                    SmsNumber.value = "";
                    Address.value = "";
                },
                error: function (err) {
                    console.log(err)
                }
            });
        });

        //get donar info from local store
        function getDonarInfo() {
            return JSON.parse(sessionStorage.getItem("_committee_")) || null;
        }


        //show Donar Info
        const infoHolder = document.getElementById("donar-info");

        function showDonarInfo() {
            const info = getDonarInfo();

            const findDonarTextBox = document.getElementById("<%= FindDonarTextBox.ClientID %>");
            const committeeMemberId = document.getElementById("<%= HiddenCommitteeMemberId.ClientID %>");

            if (info) {
                const { MemberName, SmsNumber, CommitteeMemberId } = info;

                findDonarTextBox.value = MemberName;
                committeeMemberId.value = CommitteeMemberId;
                infoHolder.innerHTML = `<span>${MemberName}</span><span>${SmsNumber}</span></span><span><i id="clearDonar" class="fa fa-trash"></i></span>`;
            } else {
                infoHolder.innerHTML = "";
                findDonarTextBox.value = "";
                committeeMemberId.value = "";
            }
        }

        //clear Donar
        function clearDonarInfo() {
            sessionStorage.removeItem("_committee_");
            showDonarInfo();
        }

        //click event
        document.addEventListener('click', function (e) {
            if (e.target && e.target.id == 'clearDonar') {
                clearDonarInfo();
            }
        });


        //call on page 
        showDonarInfo();


        //set max paid amount
        function setMaxPaidAmount() {
            const donationAmount = document.getElementById("<%=DonationAmountTextBox.ClientID%>");
            const paidAmount = document.getElementById("<%=PaidAmountTextBox.ClientID%>");

            paidAmount.max = donationAmount.value;
        }

        //on change paid amount
        function onChangePaidAmount(self) {
            const paidDateTextBox = document.getElementById("<%=PaidDateTextBox.ClientID%>");

            paidDateTextBox.required = !!self.value;
            paidDateTextBox.disabled = !self.value;

            if (!self.value)
                paidDateTextBox.value = "";

        }

        //check data is valid
        function isValidForm() {
            const committeeMemberId = document.getElementById("<%= HiddenCommitteeMemberId.ClientID %>");

            if (committeeMemberId.value) return true;

            infoHolder.innerHTML = `<strong class="red-text">find donar</strong>`;
            return false;
        }
    </script>
</asp:Content>
