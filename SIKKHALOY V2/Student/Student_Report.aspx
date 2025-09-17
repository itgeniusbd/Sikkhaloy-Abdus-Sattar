<%@ Page Title="My Report" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Student_Report.aspx.cs" Inherits="EDUCATION.COM.Student.Student_Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .Fault_GV { border: 1px solid #fff; }
            .Fault_GV th { border: 1px solid #fff; }
            .Fault_GV td { border: 1px solid #fff; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>My Report</h3>


            
                <div class="form-inline head-area NoPrint">
        <div class="form-group">
            <asp:TextBox ID="From_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:TextBox ID="To_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
            <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar"></i>
        </div>
        <div class="form-group">
            <asp:Button ID="Find_Button" CssClass="btn btn-primary" runat="server" Text="Submit" />
        </div>
        <div class="form-group pull-right Print">
            <a title="Print This Page" onclick="window.print();"><i class="fa fa-print" aria-hidden="true"></i></a>
        </div>
    </div>

            <asp:GridView ID="FindGridView" CssClass="mGrid" DataKeyNames="StudentFaultID" runat="server" DataSourceID="SqlDataSource1" AutoGenerateColumns="False" Width="100%" AllowPaging="True" AllowSorting="True" PageSize="30">
                <Columns>
                     
                    <asp:TemplateField HeaderText="Title" SortExpression="Fault_Title">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox3" CssClass="form-control" runat="server" Text='<%# Bind("Fault_Title") %>'></asp:TextBox>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label3" runat="server" Text='<%# Bind("Fault_Title") %>'></asp:Label>
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Left" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Report" SortExpression="Fault">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox2" CssClass="form-control" runat="server" Text='<%# Bind("Fault") %>'></asp:TextBox>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label2" runat="server" Text='<%# Bind("Fault") %>'></asp:Label>
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Left" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Date" SortExpression="Fault_Date">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control Datetime" Text='<%# Bind("Fault_Date") %>'></asp:TextBox>
                        </EditItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("Fault_Date", "{0:d MMM yyyy}") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="UserName" HeaderText="Post By" SortExpression="UserName" />
                </Columns>
                <PagerStyle CssClass="pgr" />
            </asp:GridView>
                        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                            SelectCommand=" SELECT Registration.UserName, StudentFaultID, Fault_Title, Fault,Fault_Date FROM Student_Fault Inner join Registration on Student_Fault.RegistrationId=Registration.RegistrationID 
                            WHERE (Student_Fault.SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID)AND Fault_Date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') ORDER BY Fault_Date DESC" CancelSelectOnNullParameter="False">
                            
                            
                           
                <DeleteParameters>
                    <asp:Parameter Name="StudentFaultID" Type="Int32" />
                </DeleteParameters>
                    <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
                    <%--<asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />--%>
                     <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                     <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                </SelectParameters>
          
            </asp:SqlDataSource>




    <script>
        $(function () {
            $("#_9").addClass("active");
        });

        $('.datepicker').datepicker({
            format: 'dd M yyyy',
            todayBtn: "linked",
            todayHighlight: true,
            autoclose: true
        });
    </script>
</asp:Content>
