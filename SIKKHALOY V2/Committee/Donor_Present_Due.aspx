<%@ Page Title="Donor Present Due List" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Donor_Present_Due.aspx.cs" Inherits="EDUCATION.COM.Committee.Donor_Present_Due" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta http-equiv="content-type" content="application/xhtml+xml; charset=UTF-8" />
    <style>
        .PD_Name_Class { color: #282828; font-size: 18px; }
        .modal-body { max-height: 500px; overflow: auto; }

        .Print_ins_Name { text-align: center; margin-bottom: 10px; color: #000; padding-bottom: 5px; border-bottom: 1px solid #000; display: none; }
        #Print_InsName { font-size: 30px; }
        #P_CategoryName { font-size: 15px; }

        .info { width: 100%; }
        .info ul { margin: 0; padding: 0; }
        .info ul li { border-bottom: 1px solid #d6e0eb; color: #5d6772; font-size: 15px; line-height: 23px; list-style: outside none none; margin: 6px 0 0; padding-bottom: 5px; padding-left: 2px; }
        .info ul li:last-child { border-bottom: none; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:FormView ID="TotalDonorDueFormView" runat="server" DataSourceID="TotalDonorDueSQL" Width="100%">
        <ItemTemplate>
            <h3>Total Present Due (Overdue) In Institution: <%# Eval("TotalDue") %> Tk</h3>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="TotalDonorDueSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT ISNULL(SUM(Due), 0) AS TotalDue FROM CommitteeDonation WHERE (SchoolID = @SchoolID) AND (Due > 0) AND (PromiseDate &lt; GETDATE() OR PromiseDate IS NULL)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <div class="NoPrint form-inline">
        <div class="form-group">
            <asp:RadioButtonList ID="DueRadioButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="True" OnSelectedIndexChanged="DueRadioButtonList_SelectedIndexChanged" CssClass="form-control">
                <asp:ListItem Selected="True">Find By Donor Type</asp:ListItem>
                <asp:ListItem>Find By Member Name</asp:ListItem>
            </asp:RadioButtonList>
        </div>
    </div>

    <asp:MultiView ID="DueMultiView" runat="server">
        <asp:View ID="TypeView" runat="server">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <div class="Print_Due">
                        <div class="NoPrint form-inline">
                            <div class="form-group">
                                <asp:DropDownList ID="DonorTypeDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="DonorTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId">
                                    <asp:ListItem Value="0">[ SELECT DONOR TYPE ]</asp:ListItem>
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="DonorTypeDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="A"></asp:RequiredFieldValidator>
                                <asp:SqlDataSource ID="DonorTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CommitteeMemberType] WHERE ([SchoolID] = @SchoolID) ORDER BY CommitteeMemberType">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                            <div class="form-group">
                                <asp:DropDownList ID="DonationCategoryDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId" OnDataBound="DonationCategoryDropDownList_DataBound">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="CategorySQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                                    SelectCommand="SELECT DISTINCT CommitteeDonationCategory.DonationCategory, CommitteeDonationCategory.CommitteeDonationCategoryId
FROM CommitteeDonation INNER JOIN
     CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId INNER JOIN
     CommitteeMember ON CommitteeDonation.CommitteeMemberId = CommitteeMember.CommitteeMemberId
WHERE (CommitteeDonation.SchoolID = @SchoolID) AND (CommitteeDonation.Due > 0) 
AND (CommitteeDonation.PromiseDate &lt; GETDATE() OR CommitteeDonation.PromiseDate IS NULL)
AND (CommitteeMember.CommitteeMemberTypeId = @CommitteeMemberTypeId)
ORDER BY CommitteeDonationCategory.DonationCategory">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:ControlParameter ControlID="DonorTypeDropDownList" Name="CommitteeMemberTypeId" PropertyName="SelectedValue" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                        </div>

                        <div class="PDby_Type">
                            <label id="C_Name"></label>
                        </div>

                        <asp:GridView ID="TotalDonorDueGridView" runat="server" AutoGenerateColumns="False" DataSourceID="TotalDonorDueByTypeSQL"
                            DataKeyNames="CommitteeMemberId,MemberName,SmsNumber,Due" CssClass="mGrid" OnRowDataBound="TotalDonorDueGridView_RowDataBound" AllowSorting="True">
                            <Columns>
                                <asp:TemplateField>
                                    <HeaderTemplate>
                                        <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                                    </ItemTemplate>
                                    <HeaderStyle CssClass="NoPrint" />
                                    <ItemStyle Width="50px" CssClass="NoPrint" />
                                </asp:TemplateField>
                                <asp:BoundField DataField="MemberName" HeaderText="Donor Name" SortExpression="MemberName" />
                                <asp:BoundField DataField="CommitteeMemberType" HeaderText="Type" SortExpression="CommitteeMemberType" />
                                <asp:BoundField DataField="SmsNumber" HeaderText="Phone" SortExpression="SmsNumber" />
                                <asp:TemplateField HeaderText="Total Due" SortExpression="Due">
                                    <ItemTemplate>
                                        <asp:Label ID="CTDueLabel" runat="server" Text='<%# Bind("Due") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <HeaderStyle Font-Size="9pt" />
                            <FooterStyle CssClass="GridFooter" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="TotalDonorDueByTypeSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                            SelectCommand="SELECT CommitteeMember.CommitteeMemberId, CommitteeMember.MemberName, CommitteeMemberType.CommitteeMemberType, CommitteeMember.SmsNumber, 
SUM(CommitteeDonation.Due) AS Due
FROM CommitteeDonation 
INNER JOIN CommitteeMember ON CommitteeDonation.CommitteeMemberId = CommitteeMember.CommitteeMemberId 
INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId 
WHERE (CommitteeDonation.SchoolID = @SchoolID) AND (CommitteeDonation.Due > 0) 
AND (CommitteeDonation.PromiseDate &lt; GETDATE() OR CommitteeDonation.PromiseDate IS NULL)
AND (CommitteeMember.CommitteeMemberTypeId = @CommitteeMemberTypeId) 
AND (CommitteeDonation.CommitteeDonationCategoryId LIKE @CommitteeDonationCategoryId)
GROUP BY CommitteeMember.CommitteeMemberId, CommitteeMember.MemberName, CommitteeMemberType.CommitteeMemberType, CommitteeMember.SmsNumber
ORDER BY CommitteeMember.MemberName">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:ControlParameter ControlID="DonorTypeDropDownList" Name="CommitteeMemberTypeId" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="DonationCategoryDropDownList" Name="CommitteeDonationCategoryId" PropertyName="SelectedValue" />
                            </SelectParameters>
                        </asp:SqlDataSource>

                        <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any donor from donor list." ForeColor="Red" ValidationGroup="AD" CssClass="Class"></asp:CustomValidator>
                    </div>

                    <div class="form-inline Submit_Disable d-print-none">
                        <!-- SMS Template Info & Edit Link -->
                        <div class="form-group" style="flex: 1; margin-right: 15px;">
                            <div style="background-color: #fff3cd; border: 1px solid #ffc107; border-radius: 4px; padding: 8px 12px;">
                                <i class="fa fa-info-circle" style="color: #ff9800;"></i>
                                <small style="color: #856404; margin-right: 8px;">SMS Template: Donor Due Notification</small>
                                <a href="/SMS/SMS_Template.aspx" class="btn btn-sm btn-warning" style="padding: 2px 8px; font-size: 11px;">
                                    <i class="fa fa-edit"></i> Edit
                                </a>
                            </div>
                        </div>

                        <div class="form-group">
                            <asp:Button ID="TypeSendButton" runat="server" CssClass="btn btn-primary" OnClick="TypeSendButton_Click" Text="Send SMS" ValidationGroup="AD" />
                        </div>
                        <div class="form-group">
                            <asp:Button ID="ViewAllDueButton" runat="server" CssClass="btn btn-grey" OnClick="ViewAllDueButton_Click" Text="View" ValidationGroup="AD" />
                        </div>
                        <div class="form-group">
                            <button type="button" class="btn btn-blue-grey" onclick="window.print();">Print</button>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:View>

        <asp:View ID="NameView" runat="server">
            <div class="form-inline">
                <div class="form-group">
                    <asp:TextBox ID="MemberNameTextBox" autocomplete="off" placeholder="Enter Member Name/Phone" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find" />
                </div>
            </div>

            <asp:FormView ID="DonorInfoFormView" DataKeyNames="SmsNumber,MemberName,TotalDue" runat="server" DataSourceID="DonorDue_ByName_SQL" Width="100%">
                <ItemTemplate>
                    <div class="z-depth-1 mb-4 p-3">
                        <div class="info">
                            <ul>
                                <li>
                                    <b><%# Eval("MemberName") %></b>
                                </li>
                                <li>
                                    <b>Type:</b>
                                    <%# Eval("CommitteeMemberType") %>
                                </li>
                                <li><b>Phone:</b>
                                    <%# Eval("SmsNumber") %>
                                </li>
                                <li><b>Address:</b>
                                    <%# Eval("Address") %>
                                </li>
                                <li class="alert-secondary p-2">
                                    <b>Total Due:</b>
                                   <%#Eval("TotalDue") %>
                                    Tk
                                </li>
                            </ul>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="DonorDue_ByName_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CommitteeMember.MemberName, CommitteeMemberType.CommitteeMemberType, CommitteeMember.SmsNumber, CommitteeMember.Address, 
SUM(CommitteeDonation.Due) AS TotalDue
FROM CommitteeDonation 
INNER JOIN CommitteeMember ON CommitteeDonation.CommitteeMemberId = CommitteeMember.CommitteeMemberId 
INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId 
WHERE (CommitteeDonation.SchoolID = @SchoolID) AND (CommitteeDonation.Due > 0) 
AND (CommitteeDonation.PromiseDate &lt; GETDATE() OR CommitteeDonation.PromiseDate IS NULL)
AND (CommitteeMember.MemberName LIKE '%' + @SearchText + '%' OR CommitteeMember.SmsNumber LIKE '%' + @SearchText + '%')
GROUP BY CommitteeMember.MemberName, CommitteeMemberType.CommitteeMemberType, CommitteeMember.SmsNumber, CommitteeMember.Address">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    <asp:ControlParameter ControlID="MemberNameTextBox" Name="SearchText" PropertyName="Text" DefaultValue="" />
                </SelectParameters>
            </asp:SqlDataSource>

            <div class="table-responsive mb-2">
                <asp:GridView ID="Name_DueDetailsGridView" runat="server" AutoGenerateColumns="False" DataSourceID="Name_DueDetailsSQL" CssClass="mGrid">
                    <Columns>
                        <asp:BoundField DataField="DonationCategory" HeaderText="Category" SortExpression="DonationCategory" />
                        <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                        <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                        <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
                        <asp:BoundField DataField="Due" HeaderText="Due" SortExpression="Due" />
                        <asp:BoundField DataField="PromiseDate" HeaderText="Promise Date" SortExpression="PromiseDate" DataFormatString="{0:d MMM yyyy}" />
                    </Columns>
                    <HeaderStyle Font-Size="9pt" />
                    <FooterStyle CssClass="GridFooter" />
                </asp:GridView>
                <asp:SqlDataSource ID="Name_DueDetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT CommitteeDonationCategory.DonationCategory, CommitteeDonation.Description, CommitteeDonation.Amount, CommitteeDonation.PaidAmount, CommitteeDonation.Due, CommitteeDonation.PromiseDate
FROM CommitteeDonation 
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId 
INNER JOIN CommitteeMember ON CommitteeDonation.CommitteeMemberId = CommitteeMember.CommitteeMemberId 
WHERE (CommitteeDonation.SchoolID = @SchoolID) AND (CommitteeDonation.Due > 0) 
AND (CommitteeDonation.PromiseDate < GETDATE() OR CommitteeDonation.PromiseDate IS NULL)
AND (CommitteeMember.MemberName LIKE '%' + @SearchText + '%' OR CommitteeMember.SmsNumber LIKE '%' + @SearchText + '%')
ORDER BY CommitteeDonation.PromiseDate">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        <asp:ControlParameter ControlID="MemberNameTextBox" Name="SearchText" PropertyName="Text" DefaultValue="" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="name_hide" style="display: none;">
                <div style="background-color: #e7f3ff; border: 1px solid #2196F3; border-radius: 4px; padding: 10px 15px; margin-bottom: 15px;">
                    <i class="fa fa-info-circle" style="color: #2196F3;"></i>
                    <strong style="color: #1976D2;">SMS Template:</strong>
                    <span style="color: #555; margin-left: 10px;">
                        Donor due notification template. 
                        <small style="color: #777;">(Placeholders: {DonorName}, {TotalDue}, {DueDetails}, etc.)</small>
                    </span>
                    <a href="/SMS/SMS_Template.aspx" class="btn btn-info btn-sm ml-2" style="text-decoration: none;">
                        <i class="fa fa-edit"></i> Edit Templates
                    </a>
                </div>

                <div class="form-inline">
                    <div class="form-group">
                        <asp:Button ID="NameSendButton" runat="server" CssClass="btn btn-primary" OnClick="NameSendButton_Click" Text="Send SMS" />
                    </div>
                    <div class="form-group">
                        <button type="button" class="btn btn-primary hidden-print" onclick="window.print();">Print</button>
                        <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
                    </div>
                </div>
            </div>
        </asp:View>
    </asp:MultiView>

    <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
        InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID, CommitteeMemberId) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID, @CommitteeMemberId)" 
        SelectCommand="SELECT * FROM [SMS_OtherInfo]">
        <InsertParameters>
            <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
            <asp:Parameter Name="SchoolID" />
            <asp:Parameter Name="StudentID" />
            <asp:Parameter Name="TeacherID" />
            <asp:Parameter Name="EducationYearID" />
            <asp:Parameter Name="CommitteeMemberId" />
        </InsertParameters>
    </asp:SqlDataSource>

    <!-- Modal -->
    <div id="myModal" class="modal fade" role="dialog">
        <div class="modal-dialog modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Donor Due Details</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body" id="modalDiv">
                    <asp:UpdatePanel ID="upnlUsers" runat="server">
                        <ContentTemplate>
                            <asp:Panel ID="ExportPanel" CssClass="AllDueP" runat="server">
                                <div class="Print_ins_Name">
                                    <label id="Print_InsName"></label>
                                    <br />
                                    <label id="P_CategoryName"></label>
                                </div>
                                <asp:DataList ID="DonorDataList" runat="server" RepeatDirection="Horizontal" RepeatColumns="1" Width="100%">
                                    <ItemTemplate>
                                        <asp:Label ID="NameLabel" CssClass="PD_Name_Class" runat="server" Font-Names="Tahoma" />
                                        <asp:GridView ID="AllDueGridView" runat="server" Width="100%" AutoGenerateColumns="False" ShowFooter="True" OnRowDataBound="AllDueGridView_RowDataBound" Font-Names="Tahoma" ForeColor="#333333">
                                            <Columns>
                                                <asp:BoundField DataField="DonationCategory" HeaderText="Category" />
                                                <asp:BoundField DataField="Description" HeaderText="Description" />
                                                <asp:TemplateField HeaderText="Due">
                                                    <FooterTemplate>
                                                        <asp:Label ID="InSumLabel" runat="server"></asp:Label>
                                                    </FooterTemplate>
                                                    <ItemTemplate>
                                                        <asp:Label ID="SumAllDueLabel" runat="server" Text='<%# Bind("Due") %>' />
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>
                                            <FooterStyle BackColor="#F4F4F4" Font-Size="11pt" />
                                            <HeaderStyle BackColor="#F4F4F4" Font-Size="11pt" />
                                            <RowStyle Font-Size="11pt" />
                                        </asp:GridView>
                                        <br />
                                        <br />
                                        &nbsp;&nbsp;&nbsp;&nbsp;
                                    </ItemTemplate>
                                </asp:DataList>
                            </asp:Panel>
                        </ContentTemplate>
                        <Triggers>
                            <asp:PostBackTrigger ControlID="ExportWordButton" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
                <div class="modal-footer">
                    <asp:Button ID="ExportWordButton" runat="server" CssClass="btn btn-success" OnClick="ExportWordButton_Click" Text="Export To Word" CausesValidation="true" />
                    <button type="button" class="btn btn-primary print" onclick="Modal_Info_Prnt();">Print</button>
                    <button type="button" class="btn btn-danger" data-dismiss="modal">Close</button>
                </div>
            </div>
        </div>
    </div>


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

    <script src="/JS/Print_This.js"></script>
    <script>
        $(function () {
            $("[id*=MemberNameTextBox]").typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "/Handeler/FindDonar.asmx/FindDonarAutocomplete",
                        data: JSON.stringify({ 'prefix': request }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (response) {
                            result($.map(JSON.parse(response.d), function (item) {
                                return item.MemberName + ', ' + item.SmsNumber;
                            }));
                        }
                    });
                }
            });

            //Checkbox selected color
            $("[id*=AllIteamCheckBox]").on("click", function () {
                var isChecked = $(this).prop('checked');
                var grid = $(this).closest("table");
                
                $("input[type=checkbox]", grid).each(function () {
                    $(this).prop('checked', isChecked);
                    if (isChecked) {
                        $("td", $(this).closest("tr")).addClass("selected");
                    } else {
                        $("td", $(this).closest("tr")).removeClass("selected");
                    }
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
                var grid = $(this).closest("table");
                var allCheckbox = $("[id*=AllIteamCheckBox]", grid);
                
                if ($(this).prop('checked')) {
                    $("td", $(this).closest("tr")).addClass("selected");
                    
                    var totalCheckboxes = $("[id*=SingleCheckBox]", grid).length;
                    var checkedCheckboxes = $("[id*=SingleCheckBox]:checked", grid).length;
                    
                    if (totalCheckboxes === checkedCheckboxes) {
                        allCheckbox.prop('checked', true);
                    }
                } else {
                    $("td", $(this).closest("tr")).removeClass("selected");
                    allCheckbox.prop('checked', false);
                }
            });

            //GridView Is Empty
            if (!$('[id*=TotalDonorDueGridView] tr').length) {
                $(".Submit_Disable").hide();
                $("#C_Name").hide();
            }

            if ($('[id*=Name_DueDetailsGridView] tr').length) {
                $(".name_hide").show();
            }

            //Due Grand Total
            var DueTotal = 0;
            $("[id*=CTDueLabel]").each(function () { DueTotal = DueTotal + parseFloat($(this).text()) });

            var Category = "";
            if ($('[id*=DonationCategoryDropDownList] :selected').index() > 0) {
                Category = " For: " + $('[id*=DonationCategoryDropDownList] :selected').text();
            }

            $('#C_Name').text("Total Donor Due" + Category + " In Type " + $('[id*=DonorTypeDropDownList] :selected').text() + ": " + DueTotal + " Tk");
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (e, f) {
            //Checkbox selected color
            $("[id*=AllIteamCheckBox]").on("click", function () {
                var isChecked = $(this).prop('checked');
                var grid = $(this).closest("table");
                
                $("input[type=checkbox]", grid).each(function () {
                    $(this).prop('checked', isChecked);
                    if (isChecked) {
                        $("td", $(this).closest("tr")).addClass("selected");
                    } else {
                        $("td", $(this).closest("tr")).removeClass("selected");
                    }
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
                var grid = $(this).closest("table");
                var allCheckbox = $("[id*=AllIteamCheckBox]", grid);
                
                if ($(this).prop('checked')) {
                    $("td", $(this).closest("tr")).addClass("selected");
                    
                    var totalCheckboxes = $("[id*=SingleCheckBox]", grid).length;
                    var checkedCheckboxes = $("[id*=SingleCheckBox]:checked", grid).length;
                    
                    if (totalCheckboxes === checkedCheckboxes) {
                        allCheckbox.prop('checked', true);
                    }
                } else {
                    $("td", $(this).closest("tr")).removeClass("selected");
                    allCheckbox.prop('checked', false);
                }
            });

            //GridView Is Empty
            if (!$('[id*=TotalDonorDueGridView] tr').length) {
                $(".Submit_Disable").hide();
                $("#C_Name").hide();
            }

            //Due Grand Total
            var DueTotal = 0;
            $("[id*=CTDueLabel]").each(function () { DueTotal = DueTotal + parseFloat($(this).text()) });

            var Category = "";
            if ($('[id*=DonationCategoryDropDownList] :selected').index() > 0) {
                Category = " For: " + $('[id*=DonationCategoryDropDownList] :selected').text();
            }

            $('#C_Name').text("Total Donor Due" + Category + " In Type " + $('[id*=DonorTypeDropDownList] :selected').text() + ": " + DueTotal + " Tk");
        });

        function openModal() {
            $('#myModal').modal('show');
        }

        function Modal_Info_Prnt() {
            $(".Print_ins_Name").show();
            $("#Print_InsName").text($("#InstitutionName").text());
            $("#P_CategoryName").text("Donor Due For Type: " + $('[id*=DonorTypeDropDownList] :selected').text());

            $('#modalDiv').css({ 'height': 'auto', 'overflow': 'auto' }).removeClass('modal-body');
            $('#myModal').modal('hide');

            setTimeout(function () {
                $('#modalDiv').addClass('modal-body');
            }, 1000);

            $("#modalDiv").printThis({
                debug: false,
                importCSS: true,
                importStyle: true,
                printContainer: true,
                pageTitle: "Donor Due",
                removeInline: false,
                printDelay: 200,
                header: null,
                formValues: true
            });
        }

        /*--Select at least one Checkbox in GridView--*/
        function Validate(d, c) {
            for (var b = document.getElementById("<%=TotalDonorDueGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                if ("checkbox" == b[a].type && b[a].checked) {
                    c.IsValid = !0;
                    return;
                }
            }
            c.IsValid = !1;
        };
    </script>
</asp:Content>
