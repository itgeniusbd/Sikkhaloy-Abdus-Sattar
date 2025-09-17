<%@ Page Title="Employee List" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Employee_List.aspx.cs" Inherits="EDUCATION.COM.Employee.Employee_List" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Emp_List.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Employee List</h3>
    <a class="NoPrint" href="Edit_Employee/Deactivated_Employee_List.aspx">Deactivated Employee List</a>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:RadioButtonList CssClass="form-control" ID="EmpTypeRadioButtonList" runat="server" AutoPostBack="True" RepeatLayout="Flow" RepeatDirection="Horizontal" OnSelectedIndexChanged="EmpTypeRadioButtonList_SelectedIndexChanged">
                <asp:ListItem Selected="True" Value="%">All Employee</asp:ListItem>
                <asp:ListItem>Teacher</asp:ListItem>
                <asp:ListItem>Staff</asp:ListItem>
            </asp:RadioButtonList>
        </div>
        <div class="form-group mx-2">
            <asp:TextBox ID="FindTextBox" runat="server" placeholder="Enter Search Keyword" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find" OnClick="FindButton_Click" />
            <input type="button" value="Print" class="btn btn-dark-green" onclick="window.print();" />
        </div>
    </div>

    <div class="alert alert-info">
        <asp:Label ID="CountLabel" runat="server"></asp:Label>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="EmployeeGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="EmployeeID,EmployeeType" DataSourceID="EmployeeSQL" AllowSorting="True">
            <Columns>
                <asp:TemplateField HeaderText="Edit/Deactivate">
                    <ItemTemplate>
                        <asp:LinkButton ID="EditLinkButton" runat="server" OnCommand="EditLinkButton_Command" CommandName='<%#Eval("EmployeeID") %>' CommandArgument='<%#Eval("EmployeeType") %>'>Edit/Deactivate</asp:LinkButton>
                    </ItemTemplate>
                    <HeaderStyle CssClass="d-print-none" />
                    <ItemStyle CssClass="d-print-none" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="ID" SortExpression="ID">
                    <ItemTemplate>
                        <asp:TextBox ID="Emp_ID_TextBox" CssClass="form-control d-print-none" runat="server" Text='<%# Bind("ID") %>'></asp:TextBox>
                        <span class="d-print-block d-none"><%#Eval("ID") %></span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="FatherName" HeaderText="Father's Name" SortExpression="FatherName" />
                 <asp:BoundField DataField="Phone" HeaderText="Mobile No." SortExpression="Phone" />
                <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
                <asp:TemplateField HeaderText="Emp.Type" SortExpression="EmployeeType">
                    <ItemTemplate>
                        <asp:TextBox ID="EmployeeTypeTextBox" CssClass="form-control" runat="server" Text='<%# Bind("EmployeeType") %>'></asp:TextBox>
                    </ItemTemplate>
                    <HeaderStyle CssClass="d-print-none" />
                    <ItemStyle CssClass="d-print-none" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Salary" SortExpression="Salary">
                    <ItemTemplate>
                        <asp:TextBox ID="SalaryTextBox" CssClass="form-control" runat="server" Text='<%# Bind("Salary") %>'></asp:TextBox>
                    </ItemTemplate>
                    <HeaderStyle CssClass="d-print-none" />
                    <ItemStyle CssClass="d-print-none" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Bank Acc. No.">
                     <ItemTemplate>
                        <asp:TextBox ID="AccNoTextBox" CssClass="form-control d-print-none" runat="server" Text='<%# Bind("Bank_AccNo") %>'></asp:TextBox>
                         <span class="d-print-block d-none"><%#Eval("Bank_AccNo") %></span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Image">
                    <ItemTemplate>
                        <img alt="" src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>" class="z-depth-1 img-fluid" style="width:50px"/>
                        <div class="NoPrint">
                            <asp:FileUpload ID="ImgFileUpload" runat="server" />
                        </div>
                    </ItemTemplate>
                    <ItemStyle VerticalAlign="Middle" CssClass="Itm_Img" />
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT EmployeeID, ID,Bank_AccNo, EmployeeType, Permanent_Temporary, Salary,  FirstName +' '+ LastName as Name,FatherName, Designation, Phone, DeviceID FROM VW_Emp_Info WHERE (SchoolID = @SchoolID) AND (Job_Status = N'Active') AND (EmployeeType LIKE @EmployeeType) order by ID"
            FilterExpression="ID LIKE '{0}%' or Name LIKE '{0}%' or Designation LIKE '{0}%' or Phone LIKE '{0}%'" UpdateCommand="IF NOT EXISTS (SELECT * FROM Employee_Info WHERE ID = @ID AND SchoolID = @SchoolID) 
UPDATE Employee_Info SET ID = @ID WHERE (EmployeeID = @EmployeeID)"
            InsertCommand="UPDATE Employee_Info SET EmployeeType = @EmployeeType WHERE (EmployeeID = @EmployeeID)">
            <FilterParameters>
                <asp:ControlParameter ControlID="FindTextBox" Name="Find" PropertyName="Text" />
            </FilterParameters>
            <InsertParameters>
                <asp:Parameter Name="EmployeeType" />
                <asp:Parameter Name="EmployeeID" />
            </InsertParameters>
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
            </SelectParameters>
            <UpdateParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:Parameter Name="ID" />
                <asp:Parameter Name="EmployeeID" />
            </UpdateParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="SalaryUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Employee_Info]" UpdateCommand="UPDATE Employee_Info SET Salary = @Salary WHERE (EmployeeID = @EmployeeID)">
            <UpdateParameters>
                <asp:Parameter Name="Salary" />
                <asp:Parameter Name="EmployeeID" />
            </UpdateParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="Bank_AccNoUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Employee_Info]" UpdateCommand="UPDATE Employee_Info SET Bank_AccNo = @Bank_AccNo WHERE (EmployeeID = @EmployeeID)">
            <UpdateParameters>
                <asp:Parameter Name="Bank_AccNo" />
                <asp:Parameter Name="EmployeeID" />
            </UpdateParameters>
        </asp:SqlDataSource>

            <asp:SqlDataSource ID="Device_DataUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT DateUpdateID FROM  Attendance_Device_DataUpdateList WHERE (SchoolID = @SchoolID) AND (UpdateType = @UpdateType))
BEGIN
INSERT INTO Attendance_Device_DataUpdateList(SchoolID, RegistrationID, UpdateType, UpdateDescription) VALUES (@SchoolID, @RegistrationID, @UpdateType, @UpdateDescription)
END" SelectCommand="SELECT * FROM [Attendance_Device_DataUpdateList]">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                    <asp:Parameter DefaultValue="Employee ID Change" Name="UpdateType" Type="String" />
                    <asp:Parameter DefaultValue="Employee ID chnage" Name="UpdateDescription" Type="String" />
                </InsertParameters>
            </asp:SqlDataSource>

    </div>

    <%if (EmployeeGridView.Rows.Count > 0)
        {%>
    <br />
    <asp:Button ID="UploadButton" runat="server" CssClass="btn btn-primary d-print-none" OnClick="UploadButton_Click" Text="Update" />
    <%}%>


    <script src="/JS/Fileuploader/jquery.nicefileinput.min.js"></script>
    <script type="text/javascript">

        $(document).ready(function () {
            $("input[type=file]").nicefileinput();
        })
    </script>
</asp:Content>
