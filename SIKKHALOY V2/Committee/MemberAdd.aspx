<%@ Page Title="Add Member" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="MemberAdd.aspx.cs" Inherits="EDUCATION.COM.Committee.MemberAdd" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .custom-form-row {
            display: flex;
            -ms-flex-wrap: wrap;
            flex-wrap: wrap;
            gap: 20px
        }

        .photo {
            width: 50px;
            border-radius: 5px;
        }


        #InfoWraper {
            display: none;
        }

        .student-imgs {
            height: 100px;
            width: 100px;
        }

        .avatar-upload {
            position: relative;
        }

            .avatar-upload .avatar-edit {
                position: absolute;
                right: 0;
                z-index: 1;
                top: 0;
            }

                .avatar-upload .avatar-edit input {
                    display: none;
                }

                    .avatar-upload .avatar-edit input + label {
                        display: inline-block;
                        width: 34px;
                        height: 34px;
                        padding-top: 3px;
                        margin-bottom: 0;
                        border-radius: 100%;
                        background: #FFFFFF;
                        box-shadow: 0px 2px 4px 0px rgba(0, 0, 0, 0.12);
                        cursor: pointer;
                        font-weight: normal;
                        transition: all 0.2s ease-in-out;
                        text-align: center;
                        border: 1px solid #E6E6E6;
                    }

                        .avatar-upload .avatar-edit input + label:hover {
                            background: #f1f1f1;
                            border-color: #d6d6d6;
                        }

                .avatar-upload .avatar-edit label::after {
                    content: "\f040";
                    font-family: 'FontAwesome';
                    color: #757575;
                }

        .media-body p {
            margin: 0;
            padding: 5px 10px;
            font-size: 14px;
        }

        .success_message {
            display: none;
            font-size: 80%;
            margin: 0
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">


    <a class="btn btn-success d-print-none" data-toggle="modal" data-target="#modalDonarForm">Add New Member</a>



    <a class="btn btn-info d-print-none" href="Print_MemberList.aspx">Print Member List</a>
    <a class="btn btn-dark d-print-none" href="MemberType.aspx">Add Member Type</a>




    <div class="table-responsive mt-3">

        <div class="custom-form-row">
            <div class="form-group d-print-none">
                <label>Member Type</label>
                <asp:DropDownList ID="CommitteeMemberDropDownList" required="" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId">
                    <asp:ListItem Value="%">[ All Type ]</asp:ListItem>
                </asp:DropDownList>
            </div>

            <div class="form-group d-print-none">
                <label>Name/Phone</label>
                <asp:TextBox ID="NamePhoneTextBox" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group ml-3 d-print-none" style="padding-top: 1.8rem">
                <asp:Button ID="FindButton" runat="server" CssClass="btn btn-outline-primary btn-md" Text="Find" />
            </div>
            <div class="form-group ml-3 d-print-none" style="padding-top: 1.8rem">
                <input id="PrintButton" type="button" value="Print" onclick="window.print();" class="btn btn-info" />
            </div>
        </div>

        <asp:GridView ID="MemberGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="MemberSQL" DataKeyNames="CommitteeMemberId" AllowPaging="True" PageSize="100"
            OnRowEditing="MemberGridView_RowEditing" 
            OnRowCancelingEdit="MemberGridView_RowCancelingEdit" 
            OnRowUpdating="MemberGridView_RowUpdating">
            <Columns>


                <asp:TemplateField HeaderText="Photo">
                    <ItemTemplate>
                        <img src="data:image/jpg;base64, <%# Convert.ToBase64String(string.IsNullOrEmpty(Eval("Photo").ToString())? new byte[]{}: (byte[]) Eval("Photo"))  %>" onerror="this.src='/Handeler/Default/Male.png'" class="photo" style="width: 50px" alt="<%#Eval("MemberName") %>" />
                    </ItemTemplate>
                    <EditItemTemplate>
                        <div class="form-group">
                            <img src="data:image/jpg;base64, <%# Convert.ToBase64String(string.IsNullOrEmpty(Eval("Photo").ToString())? new byte[]{}: (byte[]) Eval("Photo"))  %>" onerror="this.src='/Handeler/Default/Male.png'" class="photo" style="width: 50px; margin-bottom: 10px;" alt="<%#Eval("MemberName") %>" />
                            <br />
                            <small class="text-muted">Select new photo (optional):</small>
                            <asp:FileUpload ID="EditPhotoFileUpload" runat="server" CssClass="form-control" />
                        </div>
                    </EditItemTemplate>
                </asp:TemplateField>



                <asp:TemplateField HeaderText="Name" SortExpression="MemberName">
                    <EditItemTemplate>
                        <asp:TextBox ID="MemberNameTB" required="" runat="server" CssClass="form-control" Text='<%# Bind("MemberName") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("MemberName") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Reference By" SortExpression="MemberName">
                    <EditItemTemplate>
                        <asp:TextBox ID="ReferenceByTB" runat="server" CssClass="form-control" Text='<%# Bind("ReferenceBy") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("ReferenceBy") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Type" SortExpression="CommitteeMemberType">
                    <EditItemTemplate>
                        <asp:DropDownList ID="EditTypeDropDownList" required="" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId" SelectedValue='<%# Bind("CommitteeMemberTypeId") %>'>
                        </asp:DropDownList>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("CommitteeMemberType") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Address" SortExpression="Address">
                    <EditItemTemplate>
                        <asp:TextBox ID="AddressTB" runat="server" CssClass="form-control" Text='<%# Bind("Address") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("Address") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Phone" SortExpression="SmsNumber">
                    <EditItemTemplate>
                        <asp:TextBox ID="SmsNumberTB" required="" runat="server" CssClass="form-control" Text='<%# Bind("SmsNumber") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("SmsNumber") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Total Donation" SortExpression="TotalDonation">
                    <ItemTemplate>
                        <%#Eval("TotalDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Paid Donation" SortExpression="PaidDonation">
                    <ItemTemplate>
                        <%#Eval("PaidDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Due Donation" SortExpression="DueDonation">
                    <ItemTemplate>
                        <%#Eval("DueDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Update">
                    <EditItemTemplate>
                        <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" CommandName="Update" Text="Update" OnCommand="UpdateMember_Command" CssClass="btn btn-sm btn-success"></asp:LinkButton>
                        &nbsp;<asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel" CssClass="btn btn-sm btn-secondary"></asp:LinkButton>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit" CssClass="btn btn-sm btn-primary"></asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
        <asp:SqlDataSource ID="MemberSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            InsertCommand="INSERT INTO CommitteeMember(CommitteeMemberTypeId, RegistrationID, SchoolID, MemberName,ReferenceBy, SmsNumber, Address, Photo) VALUES (@CommitteeMemberTypeId, @RegistrationID, @SchoolID, @MemberName,@ReferenceBy, @SmsNumber, @Address, @Photo)"
            SelectCommand="SELECT CommitteeMember.CommitteeMemberId, CommitteeMemberType.CommitteeMemberType,CommitteeMemberType.CommitteeMemberTypeId, CommitteeMember.MemberName,ReferenceBy, CommitteeMember.SmsNumber
, CommitteeMember.Address, CommitteeMember.Photo, CommitteeMember.TotalDonation, CommitteeMember.PaidDonation
, CommitteeMember.DueDonation, CommitteeMember.InsertDate FROM CommitteeMember INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId WHERE (CommitteeMember.SchoolID = @SchoolID) AND CommitteeMemberType.CommitteeMemberTypeId LIKE @CommitteeMemberTypeId
    AND (CommitteeMember.SmsNumber LIKE ISNULL(@NamePhoneTextBox, '%') OR CommitteeMember.MemberName LIKE ISNULL(@NamePhoneTextBox, '%'))"
            CancelSelectOnNullParameter="False"
            UpdateCommand="UPDATE CommitteeMember SET CommitteeMemberTypeId = @CommitteeMemberTypeId, MemberName = @MemberName,ReferenceBy=@ReferenceBy, SmsNumber = @SmsNumber, Address = @Address, Photo = @Photo WHERE (CommitteeMemberId = @CommitteeMemberId)">
            <InsertParameters>
                <asp:ControlParameter ControlID="TypeDropDownList" Name="CommitteeMemberTypeId" PropertyName="SelectedValue" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:ControlParameter ControlID="MemberNameTextBox" Name="MemberName" PropertyName="Text" />
                <asp:ControlParameter ControlID="ReferenceByTextBox" Name="ReferenceBy" PropertyName="Text" />
                <asp:ControlParameter ControlID="PhoneTextBox" Name="SmsNumber" PropertyName="Text" />
                <asp:ControlParameter ControlID="AddressTextBox" Name="Address" PropertyName="Text" />
                <asp:ControlParameter ControlID="ImageFileUpload" Name="Photo" PropertyName="FileBytes" />
            </InsertParameters>

            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="CommitteeMemberDropDownList" Name="CommitteeMemberTypeId" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="NamePhoneTextBox" Name="NamePhoneTextBox" PropertyName="Text" />
            </SelectParameters>
            
            <UpdateParameters>
                <asp:Parameter Name="CommitteeMemberTypeId" Type="Int32" />
                <asp:Parameter Name="MemberName" Type="String" />
                <asp:Parameter Name="ReferenceBy" Type="String" />
                <asp:Parameter Name="SmsNumber" Type="String" />
                <asp:Parameter Name="Address" Type="String" />
                <asp:Parameter Name="Photo" Type="Object" />
                <asp:Parameter Name="CommitteeMemberId" Type="Int32" />
            </UpdateParameters>
        </asp:SqlDataSource>
        
        <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
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
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="MemberNameTextBox" ErrorMessage="Name is required" ValidationGroup="1" CssClass="EroorSummer" ID="RequiredFieldValidator6"></asp:RequiredFieldValidator>
                        <asp:TextBox ID="MemberNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Reference By</label>
                        <asp:TextBox ID="ReferenceByTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label>
                            Member Type
               <a class="ml-1" href="MemberType.aspx">Add New</a>
                        </label>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="TypeDropDownList" ErrorMessage="Type is required" ValidationGroup="1" CssClass="EroorSummer" ID="RequiredFieldValidator1"></asp:RequiredFieldValidator>
                        <asp:DropDownList ID="TypeDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId">
                            <asp:ListItem Value="">[ Select Type ]</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="form-group">
                        <label>Phone</label>
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="PhoneTextBox" ErrorMessage="Phone is required" ValidationGroup="1" CssClass="EroorSummer" ID="RequiredFieldValidator2"></asp:RequiredFieldValidator>
                        <asp:TextBox ID="PhoneTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Address</label>
                        <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label>Photo</label>
                        <asp:FileUpload ID="ImageFileUpload" runat="server" CssClass="form-control" />
                    </div>

                </div>

                <div class="modal-footer d-flex justify-content-center">
                    <asp:Button ID="AddMemberButton" runat="server" CssClass="btn btn-primary btn-md" Text="Submit" ValidationGroup="1" OnClick="AddMemberButton_Click" />
                </div>
            </div>
        </div>
    </div>




</asp:Content>
