<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="SetEmployee_With_PayorderName.aspx.cs" Inherits="EDUCATION.COM.Employee.SetEmployee_With_PayorderName" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Employee Monthly Payorder</h3>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="row">
                <div class="col-sm-4 form-group">
                    <label>
                        Payorder Name <a href="#" data-toggle="modal" data-target="#myModal">Add New</a>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="PayorderNameDropDownList" CssClass="EroorSummer" ErrorMessage="Select Payorder" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                    </label>
                    <asp:DropDownList ID="PayorderNameDropDownList" runat="server" CssClass="form-control" DataSourceID="PayorderNameSQL" DataTextField="Payorder_Name" DataValueField="Employee_Payorder_NameID" AppendDataBoundItems="True" AutoPostBack="True">
                        <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                    </asp:DropDownList>
                </div>                

                

            </div>

            <asp:SqlDataSource ID="PayorderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="Emp_Salary_Monthly" SelectCommand="SELECT * FROM [Employee_Payorder]" InsertCommandType="StoredProcedure">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="PayorderNameDropDownList" Name="Employee_Payorder_NameID" PropertyName="SelectedValue" />
                    <%--<asp:ControlParameter ControlID="MonthNameDropDownList" DbType="Date" Name="Get_date" PropertyName="SelectedValue" />--%>
                    <%--<asp:ControlParameter ControlID="MonthNameDropDownList" Name="MonthName" PropertyName="SelectedItem.Text" Type="String" />--%>
                    <asp:Parameter Name="EmployeeID" />
                    <asp:Parameter Direction="Output" Name="GeT_Employee_PayorderID" Type="Int32" />
                </InsertParameters>
            </asp:SqlDataSource>
            <div class="alert alert-info">                
                <label id="CountStudent"></label>
            </div>
            <div class="table-responsive">
                <asp:GridView ID="EmployeeListGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="EmployeeID,EmployeeType" DataSourceID="EmployeeListSQL" AllowSorting="True">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <ItemStyle Width="50px" />
                        </asp:TemplateField>
                        
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                        <asp:BoundField DataField="Phone" HeaderText="Mobile No." SortExpression="Phone" />
                        <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
                        <asp:BoundField DataField="EmployeeType" HeaderText="Type" SortExpression="Type" />                       
                        <asp:BoundField DataField="Payorder_Name" HeaderText="Payorder Name" SortExpression="Payorder_Name" />
                        <asp:BoundField DataField="Bank_AccNo" HeaderText="Bank Acc.No" SortExpression="Bank_AccNo" />                       
                        <asp:TemplateField HeaderText="Image">
                            <ItemTemplate>
                                <img alt="" src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>" class="z-depth-1 img-fluid" style="width: 50px" />
                            </ItemTemplate>
                            <ItemStyle VerticalAlign="Middle" CssClass="Itm_Img" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="EmployeeListSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT EmployeeID, ID,Bank_AccNo, EmployeeType, Permanent_Temporary,P.Payorder_Name,  FirstName +' '+ LastName as Name, Designation, Phone, DeviceID FROM VW_Emp_Info E Left jOin Employee_Payorder_Name P ON E.Employee_Payorder_NameID=P.Employee_Payorder_NameID  WHERE (E.SchoolID = @SchoolID) AND (E.Job_Status = N'Active') order by ID"
                    UpdateCommand="UPDATE Employee_Info SET Employee_Payorder_NameID = @Employee_Payorder_NameID WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />                       
                        <asp:ControlParameter ControlID="PayorderNameDropDownList" Name="Employee_Payorder_NameID" PropertyName="SelectedValue" />
                    </SelectParameters>
                    <UpdateParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />                        
                        <asp:Parameter Name="EmployeeID" />
                        <asp:ControlParameter ControlID="PayorderNameDropDownList" Name="Employee_Payorder_NameID" PropertyName="SelectedValue" />
                    </UpdateParameters>
                </asp:SqlDataSource>
                
                
                
                    
                
            </div>

            <div class="col-sm-4 form-group">
    <%if (EmployeeListGridView.Rows.Count > 0)
        {%>
    <br />
    <asp:Button ID="UploadButton" runat="server" CssClass="btn btn-primary d-print-none mt-4" Text="Update" OnClick="PayorderNameUpdateButton_Click" />

    <%}%>
</div>

            
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Modal -->
    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog cascading-modal" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="title"><i class="fa fa-plus"></i>Add Payorder Name</h4>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body mb-0">
                    <asp:UpdatePanel ID="upnlUsers" runat="server">
                        <ContentTemplate>
                            <div class="form-inline">
                                <div class="form-group">
                                    <asp:TextBox ID="PayorderNameTextBox" runat="server" CssClass="form-control" placeholder="Payorder Name"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="PayorderNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="AA"></asp:RequiredFieldValidator>
                                </div>
                                <div class="form-group">
                                    <asp:Button ID="AddPayorderButton" runat="server" CssClass="btn btn-primary" Text="Add" ValidationGroup="AA" />
                                </div>
                                <asp:SqlDataSource ID="PayorderNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="IF NOT EXISTS(SELECT  Employee_PayorderID FROM  Employee_Payorder WHERE (Employee_Payorder_NameID = @Employee_Payorder_NameID)) 
 BEGIN
DELETE FROM [Employee_Payorder_Name] WHERE [Employee_Payorder_NameID] = @Employee_Payorder_NameID
END"
                                    InsertCommand="IF NOT EXISTS(SELECT * FROM Employee_Payorder_Name WHERE SchoolID = @SchoolID AND Payorder_Name = @Payorder_Name)
INSERT INTO Employee_Payorder_Name(SchoolID, RegistrationID, Payorder_Name) VALUES (@SchoolID, @RegistrationID, @Payorder_Name)"
                                    SelectCommand="SELECT Employee_Payorder_NameID, SchoolID, RegistrationID, Payorder_Name, CreateDate FROM Employee_Payorder_Name WHERE (SchoolID = @SchoolID)" UpdateCommand="IF NOT EXISTS(SELECT * FROM Employee_Payorder_Name WHERE SchoolID = @SchoolID AND Payorder_Name = @Payorder_Name)
UPDATE Employee_Payorder_Name SET Payorder_Name = @Payorder_Name WHERE (Employee_Payorder_NameID = @Employee_Payorder_NameID)">
                                    <DeleteParameters>
                                        <asp:Parameter Name="Employee_Payorder_NameID" Type="Int32" />
                                    </DeleteParameters>
                                    <InsertParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                        <asp:ControlParameter ControlID="PayorderNameTextBox" Name="Payorder_Name" PropertyName="Text" Type="String" />
                                    </InsertParameters>
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                    <UpdateParameters>
                                        <asp:Parameter Name="Payorder_Name" Type="String" />
                                        <asp:Parameter Name="Employee_Payorder_NameID" Type="Int32" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </UpdateParameters>
                                </asp:SqlDataSource>
                            </div>
                            <br />
                            <asp:GridView ID="AllownceNameGridView" runat="server" AllowPaging="True" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="Employee_Payorder_NameID" DataSourceID="PayorderNameSQL">
                                <Columns>
                                    <asp:CommandField ShowEditButton="True" />
                                    <asp:BoundField DataField="Payorder_Name" HeaderText="Payorder Name" SortExpression="Payorder_Name" />
                                    <asp:BoundField DataField="CreateDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Created Date" SortExpression="CreateDate" ReadOnly="True" />
                                    <asp:CommandField ShowDeleteButton="True" />
                                </Columns>
                                <PagerStyle CssClass="pgr" />
                            </asp:GridView>
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
                <img src="../../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>



    <script type="text/javascript">
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            if ($("[id*=EmployeeListGridView] tr").length) {
                $("#CountStudent").text("TOTAL: " + $("[id*=EmployeeListGridView] td").closest("tr").length + " EMPLOYEE.");
            }
            $("[id*=AllIteamCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllIteamCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SingleCheckBox]", a).length == $("[id*=SingleCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
            
        });

        $(document).ready(function () {
            if ($("[id*=EmployeeListGridView] tr").length) {
                $("#CountStudent").text("TOTAL: " + $("[id*=EmployeeListGridView] td").closest("tr").length + " EMPLOYEE.");
            }
            $("[id*=AllIteamCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllIteamCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SingleCheckBox]", a).length == $("[id*=SingleCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });


    </script>
</asp:Content>