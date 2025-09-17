<%@ Page Title="New Pay Order" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="New_Payorder.aspx.cs" Inherits="EDUCATION.COM.Admission.Re_Admission.New_Payorder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .Role_Name { font-size: 15px; font-weight: bold; }
        .MultiRoleGV tr td { border: 1px solid #ddd; padding: 6px 0; }
        #Contain .form-control { display: inline; width: 96%; }
        .A_MR table tr td{position:relative; }
        .UAR2 table tr td{position:relative; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div id="Contain">
        <h3 class="mb-2">Assign payment</h3>

        <asp:FormView ID="StudentBasicFormView" runat="server" DataSourceID="StudentBasucSQL" Width="100%">
            <ItemTemplate>
                <h4 class="mb-3">
                    <span class="badge pink">ID: <%# Eval("ID") %></span>
                    <span class="badge pink">Name: <%#Eval("StudentsName") %></span>
                    <span class="badge pink">Class: <%# Eval("Class") %></span>
                    <span class="badge pink">Roll No:<%#Eval("RollNo") %></span>
                </h4>
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="StudentBasucSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, CreateClass.Class, StudentsClass.RollNo FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (StudentsClass.StudentID = @StudentID) AND (StudentsClass.SchoolID = @SchoolID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
            </SelectParameters>
        </asp:SqlDataSource>

        <div class="table-responsive mb-4">
            <div class="alert alert-success">Saved payment role of this Class</div>
            <asp:GridView ID="One_A_RoleGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="AssignRoleID,RoleID,PayFor" DataSourceID="One_A_RoleSQL" CssClass="mGrid">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:CheckBox ID="A_OR_CheckBox" runat="server" Text=" " />
                        </ItemTemplate>
                        <ItemStyle Width="50px" />
                    </asp:TemplateField>
                    <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                    <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                    <asp:TemplateField HeaderText="Amount" SortExpression="Amount">
                        <ItemTemplate>
                            <asp:TextBox ID="AmountTextBox" runat="server" Text='<%# Bind("Amount") %>' CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="AmountRF" Enabled="false" runat="server" ControlToValidate="AmountTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Start Date" SortExpression="StartDate">
                        <ItemTemplate>
                            <asp:TextBox ID="StartDateTextBox" runat="server" Text='<%# Bind("StartDate","{0:d MMM yyyy}") %>' CssClass="Datetime form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="StartdateRF" Enabled="false" runat="server" ControlToValidate="StartDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="End Date" SortExpression="EndDate">
                        <ItemTemplate>
                            <asp:TextBox ID="EndDateTextBox" runat="server" Text='<%# Bind("EndDate","{0:d MMM yyyy}") %>' CssClass="Datetime form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="EndDateRF" Enabled="false" runat="server" ControlToValidate="EndDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Late Fee" SortExpression="LateFee">
                        <ItemTemplate>
                            <asp:TextBox ID="LateFeeTextBox" runat="server" Text='<%# Bind("LateFee") %>' CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Concession">
                        <ItemTemplate>
                            <asp:TextBox ID="DiscountTextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="One_A_RoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT * from (SELECT Income_Roles.Role, I_Role.AssignRoleID, I_Role.RegistrationID, I_Role.SchoolID, I_Role.RoleID, I_Role.ClassID, I_Role.PayFor,I_Role.Amount,I_Role.LateFee, I_Role.StartDate,I_Role.EndDate, I_Role.EducationYearID, 
(SELECT COUNT(*) AS No_Of_Role FROM Income_Assign_Role WHERE (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (SchoolID = @SchoolID) AND (RoleID = I_Role.RoleID))  AS No_Of_Role
FROM Income_Assign_Role as I_Role INNER JOIN Income_Roles ON I_Role.RoleID = Income_Roles.RoleID WHERE (I_Role.ClassID = @ClassID) AND (I_Role.EducationYearID = @EducationYearID) AND (I_Role.SchoolID = @SchoolID)) as Roles where No_Of_Role = 1 ORDER BY StartDate">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:QueryStringParameter Name="EducationYearID" QueryStringField="Year" />
                    <asp:QueryStringParameter Name="ClassID" QueryStringField="Class" Type="Int32" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="table-responsive mb-4">
            <asp:GridView ID="Multi_A_Role_GridView" runat="server" AutoGenerateColumns="False" CssClass="MultiRoleGV" DataSourceID="Multi_A_RoleSQL" ShowHeader="False" Width="100%" DataKeyNames="RoleID">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <div class="Role_Name">
                                <asp:CheckBox ID="Multi_AddCheckBox" runat="server" Text=" " />
                                Add - 
                           <asp:Label ID="RoleLabel" runat="server" Text='<%# Bind("Role") %>'></asp:Label>
                                (Instalment
                                <asp:Label ID="Multi_No_PayLabel" runat="server" Text='<%# Bind("No_Of_Role") %>'></asp:Label>)
                           <asp:HiddenField ID="RoleHiddenField" runat="server" Value='<%# Eval("RoleID") %>' />
                            </div>
                            <div class="criteriaData" style="display: none">
                                <asp:GridView ID="Input_Multi_Role_GridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" Width="100%" DataSourceID="Multi_Role_SQL" DataKeyNames="AssignRoleID">
                                    <Columns>
                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="Input_MultiCheckBox" runat="server" Text=" " />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Pay For">
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_PayForTextBox" placeholder="Pay For" Text='<%# Bind("PayFor") %>' runat="server" CssClass="form-control PayFor"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="payForRF" Enabled="false" runat="server" ControlToValidate="Multi_PayForTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                                <asp:TextBox ID="AssignAmountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Amount Same For All" runat="server"></asp:TextBox>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_AmountTextBox" placeholder="Amount" Text='<%# Bind("Amount") %>' runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="AmountRF" Enabled="false" runat="server" ControlToValidate="Multi_AmountTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Start Date">
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_StartDateTextBox" placeholder="Start Date" Text='<%# Bind("StartDate","{0:d MMM yyyy}") %>' runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="StartdateRF" Enabled="false" runat="server" ControlToValidate="Multi_StartDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="End Date">
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_EndDateTextBox" placeholder="End Date" Text='<%# Bind("EndDate","{0:d MMM yyyy}") %>' runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="EndDateRF" Enabled="false" runat="server" ControlToValidate="Multi_EndDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                                <asp:TextBox ID="AssinLFeeTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Late Fee Same For All" runat="server"></asp:TextBox>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_LateFeeTextBox" placeholder="Late Fee" Text='<%# Bind("LateFee") %>' runat="server" CssClass="form-control"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                                <asp:TextBox ID="AssConcessionTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Concession Same For All" runat="server"></asp:TextBox>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_DiscountTextBox" placeholder="Concession" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                                <asp:SqlDataSource ID="Multi_Role_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Income_Roles.Role, Income_Assign_Role.AssignRoleID, Income_Assign_Role.RegistrationID, Income_Assign_Role.SchoolID, Income_Assign_Role.ClassID, Income_Assign_Role.PayFor, 
                         Income_Assign_Role.Amount, Income_Assign_Role.LateFee, Income_Assign_Role.StartDate, Income_Assign_Role.EndDate, Income_Assign_Role.EducationYearID, Income_Assign_Role.RoleID
FROM Income_Assign_Role INNER JOIN Income_Roles ON Income_Assign_Role.RoleID = Income_Roles.RoleID WHERE (Income_Assign_Role.ClassID = @ClassID) AND (Income_Assign_Role.EducationYearID = @EducationYearID) AND (Income_Assign_Role.SchoolID = @SchoolID) AND (Income_Assign_Role.RoleID = @RoleID) ORDER BY Income_Assign_Role.StartDate">
                                    <SelectParameters>
                                        <asp:QueryStringParameter Name="ClassID" QueryStringField="Class" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:QueryStringParameter Name="EducationYearID" QueryStringField="Year" />
                                        <asp:ControlParameter ControlID="RoleHiddenField" Name="RoleID" PropertyName="Value" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="Multi_A_RoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT distinct * from (SELECT Income_Roles.Role, I_Role.RoleID,
(SELECT COUNT(*) AS No_Of_Role FROM Income_Assign_Role WHERE (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (SchoolID = @SchoolID) AND (RoleID = I_Role.RoleID))  AS No_Of_Role
FROM Income_Assign_Role as I_Role INNER JOIN Income_Roles ON I_Role.RoleID = Income_Roles.RoleID WHERE (I_Role.ClassID = @ClassID) AND (I_Role.EducationYearID = @EducationYearID) AND (I_Role.SchoolID = @SchoolID)) as Roles where No_Of_Role &lt;&gt; 1">
                <SelectParameters>
                    <asp:QueryStringParameter Name="ClassID" QueryStringField="Class" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:QueryStringParameter Name="EducationYearID" QueryStringField="Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="table-responsive mb-4 A_MR">
            <div class="alert alert-info">Unsaved payment role</div>
            <asp:GridView ID="One_Role_GridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="Roles_1_SQL" DataKeyNames="RoleID">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:CheckBox ID="One_Role_CheckBox" runat="server" Text=" " />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Role" HeaderText="Role" SortExpression="Role" />
                    <asp:TemplateField HeaderText="Pay For">
                        <ItemTemplate>
                            <asp:TextBox ID="One_PayForTextBox" runat="server" CssClass="form-control One_Role" placeholder="Pay For"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="payForRF" Enabled="false" runat="server" ControlToValidate="One_PayForTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Amount">
                        <ItemTemplate>
                            <asp:TextBox ID="One_AmountTextBox" placeholder="Amount" runat="server" CssClass="form-control One_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="AmountRF" Enabled="false" runat="server" ControlToValidate="One_AmountTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Start Date">
                        <ItemTemplate>
                            <asp:TextBox ID="One_StartDateTextBox" placeholder="Start Date" runat="server" CssClass="form-control Datetime One_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="StartdateRF" Enabled="false" runat="server" ControlToValidate="One_StartDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="End Date">
                        <ItemTemplate>
                            <asp:TextBox ID="One_EndDateTextBox" placeholder="End Date" runat="server" CssClass="form-control Datetime One_Role" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="EndDateRF" Enabled="false" runat="server" ControlToValidate="One_EndDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Late Fee">
                        <ItemTemplate>
                            <asp:TextBox ID="One_LateFeeTextBox" placeholder="Late Fee" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control One_Role"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Concession">
                        <ItemTemplate>
                            <asp:TextBox ID="One_DiscountTextBox" placeholder="Concession" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="Roles_1_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoleID, Role, NumberOfPay, Description, Date FROM Income_Roles WHERE (RoleID NOT IN (SELECT RoleID FROM Income_Assign_Role WHERE (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID))) AND (SchoolID = @SchoolID) AND (NumberOfPay = 1)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:QueryStringParameter Name="EducationYearID" QueryStringField="Year" />
                    <asp:QueryStringParameter Name="ClassID" QueryStringField="Class" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="table-responsive mb-4 UAR2">
            <asp:GridView ID="Multi_R_GridView" runat="server" AutoGenerateColumns="False" DataKeyNames="RoleID,NumberOfPay" DataSourceID="Multi_R_SQL" ShowHeader="False" Width="100%" OnRowDataBound="Multi_Role_GridView_RowDataBound" CssClass="MultiRoleGV">
                <Columns>
                    <asp:TemplateField>
                        <ItemTemplate>
                            <div class="Role_Name">
                                <asp:CheckBox ID="Multi_AddCheckBox" runat="server" Text=" " />
                                Add - 
                           <asp:Label ID="RoleLabel" runat="server" Text='<%# Bind("Role") %>'></asp:Label>
                                (Instalment
                           <asp:Label ID="Multi_No_PayLabel" runat="server" Text='<%# Bind("NumberOfPay") %>'></asp:Label>)
                            </div>
                            <div class="criteriaData" style="display: none">
                                <asp:GridView ID="Input_Multi_Role_GridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" Width="100%">
                                    <Columns>
                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <asp:CheckBox ID="Input_MultiCheckBox" runat="server" Text=" " />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Pay For">
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_PayForTextBox" placeholder="Pay For" runat="server" CssClass="form-control PayFor"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="payForRF" Enabled="false" runat="server" ControlToValidate="Multi_PayForTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                                <asp:TextBox ID="AssignAmountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Amount Same For All" runat="server"></asp:TextBox>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_AmountTextBox" placeholder="Amount" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="AmountRF" Enabled="false" runat="server" ControlToValidate="Multi_AmountTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Start Date">
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_StartDateTextBox" placeholder="Start Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="StartdateRF" Enabled="false" runat="server" ControlToValidate="Multi_StartDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="End Date">
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_EndDateTextBox" placeholder="End Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="EndDateRF" Enabled="false" runat="server" ControlToValidate="Multi_EndDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                                <asp:TextBox ID="AssinLFeeTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Late Fee Same For All" runat="server"></asp:TextBox>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_LateFeeTextBox" placeholder="Late Fee" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
                                            <HeaderTemplate>
                                                <asp:TextBox ID="AssConcessionTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Concession Same For All" runat="server"></asp:TextBox>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <asp:TextBox ID="Multi_DiscountTextBox" placeholder="Concession" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="Multi_R_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoleID, Role, NumberOfPay, Description, Date FROM Income_Roles WHERE (RoleID NOT IN (SELECT RoleID FROM Income_Assign_Role WHERE (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID))) AND (SchoolID = @SchoolID) AND (NumberOfPay &lt;&gt; 1)">
                <SelectParameters>
                    <asp:QueryStringParameter Name="ClassID" QueryStringField="Class" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:QueryStringParameter Name="EducationYearID" QueryStringField="Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="form-group">
            <asp:Button ID="PayOrderButton" runat="server" Text="Assign Payorder" CssClass="btn btn-primary" OnClick="PayOrderButton_Click" ValidationGroup="A" />
            <asp:SqlDataSource ID="PayOrderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Income_PayOrder(SchoolID, RegistrationID, StudentID, ClassID, StudentClassID, AssignRoleID, EducationYearID, Amount, PaidAmount, LateFee, Discount, LateFee_Discount, RoleID, PayFor, StartDate, EndDate, CreatedDate) VALUES (@SchoolID, @RegistrationID, @StudentID, @ClassID, @StudentClassID, @AssignRoleID, @EducationYearID, @Amount, @PaidAmount, @LateFee, @Discount, @LateFee_Discount, @RoleID, @PayFor, @StartDate, @EndDate, GETDATE())" SelectCommand="SELECT * FROM [Income_PayOrder]">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" Type="Int32" />
                    <asp:QueryStringParameter Name="ClassID" QueryStringField="Class" Type="Int32" />
                    <asp:QueryStringParameter Name="StudentClassID" QueryStringField="StudentClass" Type="Int32" />
                    <asp:QueryStringParameter Name="EducationYearID" QueryStringField="Year" />
                    <asp:Parameter Name="AssignRoleID" />
                    <asp:Parameter DefaultValue="" Name="Amount" Type="Double" />
                    <asp:Parameter DefaultValue="0" Name="PaidAmount" Type="Double" />
                    <asp:Parameter DefaultValue="" Name="LateFee" Type="Double" />
                    <asp:Parameter Name="Discount" Type="Double" />
                    <asp:Parameter DefaultValue="" Name="LateFee_Discount" Type="Double" />
                    <asp:Parameter Name="RoleID" Type="Int32" />
                    <asp:Parameter Name="PayFor" Type="String" />
                    <asp:Parameter DbType="Date" Name="StartDate" />
                    <asp:Parameter DbType="Date" Name="EndDate" DefaultValue="" />
                </InsertParameters>
            </asp:SqlDataSource>
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


    <script type="text/javascript">
        $(function () {
            $('.PayFor').on("keypress", function () {
                var tr = $(this).closest("tr");
                $(this).typeahead({
                    minLength: 1,
                    source: function (request, result) {
                        $.ajax({
                            url: "New_Payorder.aspx/GetMonth",
                            data: JSON.stringify({ 'prefix': request }),
                            dataType: "json",
                            type: "POST",
                            contentType: "application/json; charset=utf-8",
                            success: function (response) {
                                label = [];
                                map = {};
                                $.map(JSON.parse(response.d), function (item) {
                                    label.push(item.Month);
                                    map[item.Month] = item;
                                });
                                result(label);
                            }
                        });
                    },
                    updater: function (item) {
                        $(".Datetime:eq(0)", tr).val("01 " + map[item].MonthYear);
                        $(".Datetime:eq(1)", tr).val("10 " + map[item].MonthYear);
                        return item;
                    }
                });
            });

            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            //Assign_One Role_CheckBox
            $("[id*=A_OR_CheckBox]").on("click", function () {
                ValidatorEnable($("[id*=AmountRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=StartdateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=EndDateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
            });

            //One Role CheckBox
            $("[id*=One_Role_CheckBox]").on("click", function () {
                ValidatorEnable($("[id*=payForRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=AmountRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=StartdateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=EndDateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
            });

            //Multi Role CheckBox
            $("[id*=Multi_AddCheckBox]").on("click", function () {
                var Multi_AddCheckBox = $(this);
                var grid = $(this).closest("tr").find("div.criteriaData");

                $("input[type=checkbox]", grid).each(function () {
                    if (Multi_AddCheckBox.is(":checked")) {
                        $(this).attr("checked", "checked");
                        ValidatorEnable($("[id*=payForRF]", $(this).closest("tr"))[0], true);
                        ValidatorEnable($("[id*=AmountRF]", $(this).closest("tr"))[0], true);
                        ValidatorEnable($("[id*=StartdateRF]", $(this).closest("tr"))[0], true);
                        ValidatorEnable($("[id*=EndDateRF]", $(this).closest("tr"))[0], true);
                    }
                    else {
                        $(this).removeAttr("checked");
                        ValidatorEnable($("[id*=payForRF]", $(this).closest("tr"))[0], false);
                        ValidatorEnable($("[id*=AmountRF]", $(this).closest("tr"))[0], false);
                        ValidatorEnable($("[id*=StartdateRF]", $(this).closest("tr"))[0], false);
                        ValidatorEnable($("[id*=EndDateRF]", $(this).closest("tr"))[0], false);
                    }
                });
            });

            $("[id*=Input_MultiCheckBox]").on("click", function () {
                ValidatorEnable($("[id*=payForRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=AmountRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=StartdateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
                ValidatorEnable($("[id*=EndDateRF]", $("td", $(this).closest("tr")))[0], $(this).is(":checked"));
            });

            //Show Hide Multi Role CheckBox
            $("[id*=Multi_AddCheckBox]").on("click", function () {
                if ($(this).is(':checked')) {
                    $(this).closest("tr").find("div.criteriaData").show("shlow");
                }
                else {
                    $(this).closest("tr").find("div.criteriaData").hide("shlow");
                }
            });

            //Assign Amount to All
            $("[id*=AssignAmountTextBox]").on("keyup", function () {
                $("[id*=Multi_AmountTextBox]", $(this).closest("tr td")).val($.trim($(this).val()));
            });
            //Assign Late Fee to All
            $("[id*=AssinLFeeTextBox]").on("keyup", function () {
                $("[id*=Multi_LateFeeTextBox]", $(this).closest("tr td")).val($.trim($(this).val()));
            });
            //Concession Fee to All
            $("[id*=AssConcessionTextBox]").on("keyup", function () {
                $("[id*=Multi_DiscountTextBox]", $(this).closest("tr td")).val($.trim($(this).val()));
            });
        });

        function noBack() {
            window.history.forward();
        }
        noBack();
        window.onload = noBack;
        window.onpageshow = function (evt) {
            if (evt.persisted) noBack();
        }
        window.onunload = function () { void (0); }

        function DisableButton() {
            document.getElementById("<%=PayOrderButton.ClientID %>").disabled = true;
        }
        window.onbeforeunload = DisableButton;

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
