<%@ Page Title="Send SMS From Contact List" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Send_SMS_To_Others.aspx.cs" Inherits="EDUCATION.COM.SMS.Send_SMS_To_Others" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .Counter_St { color: #00009b; font-size: 15px; font-weight: bold; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>

        </ContentTemplate>
    </asp:UpdatePanel>
    <h3>
        <asp:FormView ID="SMSFormView" runat="server" DataKeyNames="SMSID" DataSourceID="SMSSQL">
            <ItemTemplate>
                Saved Contact List (Remaining SMS:
                   <asp:Label ID="CountLabel" runat="server" Text='<%# Bind("SMS_Balance") %>' />)
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="SMSSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [SMS] WHERE ([SchoolID] = @SchoolID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="SelectGroupDropDownList" runat="server" CssClass="form-control" DataSourceID="AddGroupSQL" DataTextField="GroupName" DataValueField="SMS_GroupID" AutoPostBack="True" OnDataBound="SelectGroupDropDownList_DataBound">
            </asp:DropDownList>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="SelectGroupDropDownList" CssClass="EroorSummer" ErrorMessage="Select Group" InitialValue="0" ValidationGroup="SN">*</asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:TextBox ID="SearchTextBox" placeholder="Name OR Mobile No." runat="server" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="SearchButton" runat="server" CssClass="btn btn-primary" Text="Search" />
        </div>

        <div class="form-group pull-right">
            <button type="button" class="btn btn-success" data-toggle="modal" data-target="#myModal2">Add Mobile No.</button>
            <button type="button" class="btn btn-success" data-toggle="modal" data-target="#myModal">Add Group</button>
        </div>
        <div class="clearfix"></div>
    </div>

    <div class="alert alert-success">
        <asp:Label ID="IteamCountLabel" runat="server" Font-Bold="True" Font-Size="15px"></asp:Label>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="ContactListGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="SMS_NumberID" DataSourceID="Group_Phone_NumberSQL" AllowPaging="True" PageSize="200">
            <Columns>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <asp:CheckBox ID="SelectAllCheckBox" runat="server" Text=" " />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="SelectCheckBox" runat="server" Text=" " />
                    </ItemTemplate>
                    <HeaderStyle CssClass="HideCB" />
                    <ItemStyle Width="30px" CssClass="HideCB " />
                </asp:TemplateField>
                <asp:BoundField DataField="GroupName" HeaderText="Group" ReadOnly="True" SortExpression="GroupName" />
                <asp:TemplateField HeaderText="Name" SortExpression="Name">
                    <EditItemTemplate>
                        <asp:TextBox ID="TextBox1" CssClass="form-control" runat="server" Text='<%# Bind("Name") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:Label ID="Label1" runat="server" Text='<%# Bind("Name") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Mobile No." SortExpression="MobileNo">
                    <EditItemTemplate>
                        <asp:TextBox ID="UMobieTextBox" CssClass="form-control" runat="server" Text='<%# Bind("MobileNo") %>'></asp:TextBox>
                        <asp:RegularExpressionValidator ID="RV" runat="server" ControlToValidate="UMobieTextBox" CssClass="EroorStar" ErrorMessage="Invalid!" ValidationExpression="(88)?((011)|(015)|(016)|(017)|(018)|(019)|(013)|(014))\d{8,8}" ValidationGroup="UP" />
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:Label ID="MobileNoLabel" runat="server" Text='<%# Bind("MobileNo") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Address" SortExpression="Address">
                    <EditItemTemplate>
                        <asp:TextBox ID="TextBox2" CssClass="form-control" runat="server" Text='<%# Bind("Address") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:Label ID="Label2" runat="server" Text='<%# Bind("Address") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Add_Date" HeaderText="Add Date" SortExpression="Add_Date" DataFormatString="{0:d MMM yyyy}" ReadOnly="True" />
                <asp:TemplateField ShowHeader="False">
                    <EditItemTemplate>
                        <asp:LinkButton ID="LinkButton1" ValidationGroup="UP" runat="server" CausesValidation="True" CommandName="Update" Text="Update"></asp:LinkButton>
                        &nbsp;<asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit"></asp:LinkButton>
                    </ItemTemplate>
                    <HeaderStyle CssClass="HideCB" />
                    <ItemStyle Width="85px" CssClass="HideCB " />
                </asp:TemplateField>
                <asp:TemplateField ShowHeader="False">
                    <ItemTemplate>
                        <asp:LinkButton ID="DeletLinkButton" OnClientClick="return confirm('Are you sure want to delete?')" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"></asp:LinkButton>
                    </ItemTemplate>
                    <HeaderStyle CssClass="HideCB" />
                    <ItemStyle Width="30px" CssClass="HideCB " />
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                Empty Data
            </EmptyDataTemplate>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
        <asp:SqlDataSource ID="Group_Phone_NumberSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            DeleteCommand="DELETE FROM [SMS_Group_Phone_Number] WHERE [SMS_NumberID] = @SMS_NumberID"
            InsertCommand="INSERT INTO SMS_Group_Phone_Number(SchoolID, RegistrationID, SMS_GroupID, Name, MobileNo, Address, Add_Date) VALUES (@SchoolID, @RegistrationID, @SMS_GroupID, @Name, @MobileNo, @Address, GETDATE())"
            SelectCommand="SELECT SMS_Group_Phone_Number.SMS_NumberID, SMS_Group_Phone_Number.SMS_GroupID, ISNULL(SMS_Group_Phone_Number.Name, '') AS Name, SMS_Group_Phone_Number.MobileNo, SMS_Group_Phone_Number.Add_Date, SMS_Group_Phone_Number.Address, SMS_Group_Name.GroupName FROM SMS_Group_Phone_Number INNER JOIN SMS_Group_Name ON SMS_Group_Phone_Number.SMS_GroupID = SMS_Group_Name.SMS_GroupID WHERE (SMS_Group_Phone_Number.SchoolID = @SchoolID) AND (SMS_Group_Phone_Number.SMS_GroupID = @SMS_GroupID OR @SMS_GroupID = 0) ORDER BY SMS_Group_Phone_Number.SMS_GroupID"
            UpdateCommand="UPDATE SMS_Group_Phone_Number SET Name =@Name, MobileNo =@MobileNo, Address =@Address WHERE (SMS_NumberID = @SMS_NumberID)"
            OnSelected="Group_Phone_NumberSQL_Selected"
            FilterExpression="MobileNo like '%{0}%' or Name like '%{0}%'">
            <DeleteParameters>
                <asp:Parameter Name="SMS_NumberID" Type="Int32" />
            </DeleteParameters>
            <FilterParameters>
                <asp:ControlParameter ControlID="SearchTextBox" Name="Mobile" PropertyName="Text" DefaultValue="%" />

            </FilterParameters>
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:ControlParameter ControlID="GroupNameDropDownList" Name="SMS_GroupID" PropertyName="SelectedValue" Type="Int32" />
                <asp:ControlParameter ControlID="PersonNameTextBox" Name="Name" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="MobileNumberTextBox" Name="MobileNo" PropertyName="Text" />
                <asp:ControlParameter ControlID="AddressTextBox" Name="Address" PropertyName="Text" />
            </InsertParameters>
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="SelectGroupDropDownList" Name="SMS_GroupID" PropertyName="SelectedValue" />
            </SelectParameters>
            <UpdateParameters>
                <asp:Parameter Name="Name" Type="String" />
                <asp:ControlParameter ControlID="ContactListGridView" Name="MobileNo" PropertyName="SelectedDataKey[1]" Type="String" />
                <asp:Parameter Name="Address" Type="String" />
                <asp:Parameter Name="SMS_NumberID" Type="Int32" />
            </UpdateParameters>
        </asp:SqlDataSource>
        <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any contact from contact list." ForeColor="Red" ValidationGroup="SN" />
    </div>

    <%if (ContactListGridView.Rows.Count > 0)
        {%>
    <div class="row">
        <div class="col-md-4 NoPrint">
            <div class="form-group">
                <label>
                    Text Message
                  <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="SMSTextBox" CssClass="EroorSummer" ErrorMessage="Write SMS Text" ValidationGroup="SN" />
                </label>
                <asp:TextBox ID="SMSTextBox" runat="server" Height="107px" TextMode="MultiLine" CssClass="form-control"></asp:TextBox>
                <div id="sms-counter" class="Counter_St">
                    Length: <span class="length"></span>/ <span class="per_message"></span>.  Count: <span class="messages"></span>
                    SMS
                </div>
            </div>

            <div class="form-group">
                <asp:Button ID="SendSMSButton" runat="server" CssClass="btn btn-primary" Text="Send" ValidationGroup="SN" OnClick="SendSMSButton_Click" />
                <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
                <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, EducationYearID, SMS_NumberID) VALUES (@SMS_Send_ID, @SchoolID, @EducationYearID, @SMS_NumberID)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
                    <InsertParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:Parameter DbType="Guid" Name="SMS_Send_ID" />
                        <asp:Parameter Name="EducationYearID" />
                        <asp:Parameter Name="SMS_NumberID" />
                    </InsertParameters>
                </asp:SqlDataSource>
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="EroorSummer" DisplayMode="List" ValidationGroup="SN" />
            </div>
        </div>
    </div>
    <%} %>



    <!--Add Group-->
    <div id="myModal" class="modal fade" role="dialog">
        <div class="modal-dialog modal-sm">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Add New Group</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="upnlUsers" runat="server">
                        <ContentTemplate>
                            <div class="form-inline">
                                <div class="form-group">
                                    <asp:TextBox ID="GroupName" placeholder="Group Name" runat="server" CssClass="form-control" />
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="GroupName" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="G"></asp:RequiredFieldValidator>
                                </div>
                                <div class="form-group">
                                    <asp:Button ID="SaveButton" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="SaveButton_Click" ValidationGroup="G" />
                                </div>
                            </div>

                            <asp:SqlDataSource ID="AddGroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [SMS_Group_Name] WHERE [SMS_GroupID] = @SMS_GroupID" InsertCommand="INSERT INTO [SMS_Group_Name] ([RegistrationID], [SchoolID], [GroupName]) VALUES (@RegistrationID, @SchoolID, @GroupName)" SelectCommand="SELECT * FROM SMS_Group_Name WHERE (SchoolID = @SchoolID)" UpdateCommand="UPDATE SMS_Group_Name SET GroupName = @GroupName WHERE (SMS_GroupID = @SMS_GroupID)">
                                <DeleteParameters>
                                    <asp:Parameter Name="SMS_GroupID" Type="Int32" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                    <asp:ControlParameter ControlID="GroupName" Name="GroupName" PropertyName="Text" Type="String" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                                <UpdateParameters>
                                    <asp:Parameter Name="GroupName" Type="String" />
                                    <asp:Parameter Name="SMS_GroupID" Type="Int32" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                            <asp:GridView ID="GroupGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="SMS_GroupID" DataSourceID="AddGroupSQL" AllowPaging="True">
                                <Columns>
                                    <asp:BoundField DataField="GroupName" HeaderText="Group Name" SortExpression="GroupName" />
                                    <asp:CommandField ShowEditButton="True" />
                                    <asp:CommandField ShowDeleteButton="True" />
                                </Columns>
                            </asp:GridView>

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>

        </div>
    </div>

    <!--Add Number-->
    <div id="myModal2" class="modal fade" role="dialog">
        <div class="modal-dialog modal-sm">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Add Contact</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                        <ContentTemplate>

                            <div class="form-group">
                                <label>
                                    Group Name
                               <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="GroupNameDropDownList" CssClass="EroorStar" ErrorMessage="*" InitialValue="0" ValidationGroup="N"></asp:RequiredFieldValidator>
                                </label>
                                <asp:DropDownList ID="GroupNameDropDownList" runat="server" CssClass="form-control" DataSourceID="AddGroupSQL" DataTextField="GroupName" DataValueField="SMS_GroupID" OnDataBound="GroupNameDropDownList_DataBound">
                                    <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                                </asp:DropDownList>
                            </div>

                            <div class="form-group">
                                <label>
                                    Person Name
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="PersonNameTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="N"></asp:RequiredFieldValidator>
                                </label>
                                <asp:TextBox ID="PersonNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label>
                                    Mobile Number
                               <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="MobileNumberTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="N"></asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="MobileNumberTextBox" CssClass="EroorSummer" ErrorMessage="Invalid !" ValidationExpression="(88)?((011)|(015)|(016)|(017)|(018)|(019)|(013)|(014))\d{8,8}" ValidationGroup="N"></asp:RegularExpressionValidator>
                                </label>
                                <asp:TextBox ID="MobileNumberTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <label>Address</label>
                                <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control" TextMode="MultiLine"></asp:TextBox>
                            </div>

                            <div class="form-group">
                                <asp:Button ID="AddButton" runat="server" CssClass="btn btn-primary" OnClick="AddButton_Click" Text="Add To List" ValidationGroup="N" />
                                <asp:Label ID="MsgLabel" runat="server" ForeColor="#339933"></asp:Label>
                            </div>

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>

        </div>
    </div>


    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script src="/JS/SMSCount/sms_counter.min.js"></script>
    <script>
        $(function () {
            $('[id*=SMSTextBox]').countSms('#sms-counter');

            $('.textbox').focus(function () {
                $("[id*=MsgLabel]").text("");
            });

            $("[id*=SelectGroupDropDownList]").change(function () {
                $("[id*=SearchTextBox]").val('')
            });

            $("[id*=SelectAllCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SelectCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=SelectAllCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SelectCheckBox]", a).length == $("[id*=SelectCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });


        function Validate(d, c) {
            for (var b = document.getElementById("<%=ContactListGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                if ("checkbox" == b[a].type && b[a].checked) {
                    c.IsValid = !0;
                    return;
                }
            }
            c.IsValid = !1;
        };
    </script>
</asp:Content>
