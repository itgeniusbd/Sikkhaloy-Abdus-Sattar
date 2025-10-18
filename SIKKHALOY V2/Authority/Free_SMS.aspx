<%@ Page Title="Manage Institution" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Free_SMS.aspx.cs" Inherits="EDUCATION.COM.Authority.Free_SMS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid { text-align: left; }
        .Invaid_Ins td { color: #ff2b2b; }
        .Invaid_Ins td a { color: #ff2b2b; }
        
        /* Enhanced Search Panel Styles */
        .search-filters {
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            border: 1px solid #dee2e6;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 25px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        
        .search-filters h6 {
            color: #495057;
            font-weight: 600;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
        }
        
        .search-filters h6 i {
            margin-right: 8px;
            color: #007bff;
        }

        /* Enhanced Form Controls */
        .search-row {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            align-items: end;
            margin-bottom: 15px;
        }
        
        .search-col {
            flex: 1;
            min-width: 180px;
        }
        
        .search-col-auto {
            flex: 0 0 auto;
            min-width: 200px;
        }

        /* Enhanced Input Styles */
        .form-control {
            border: 2px solid #e9ecef;
            border-radius: 8px;
            padding: 8px 15px;
            font-size: 14px;
            transition: all 0.3s ease;
            background-color: #fff;
        }
        
        .form-control:focus {
            border-color: #007bff;
            box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
            outline: none;
        }

        /* Label Styles */
        .form-label {
            display: block;
            margin-bottom: 5px;
            font-weight: 500;
            color: #495057;
            font-size: 13px;
        }

        /* Button Enhancements */
        .btn {
            border-radius: 8px;
            padding: 10px 20px;
            font-weight: 500;
            text-transform: uppercase;
            font-size: 12px;
            letter-spacing: 0.5px;
            transition: all 0.3s ease;
            border: none;
            cursor: pointer;
        }
        
        .btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
        }
        
        .btn-primary {
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            color: white;
        }
        
        .btn-secondary {
            background: linear-gradient(135deg, #6c757d 0%, #495057 100%);
            color: white;
        }
        
        .btn-success {
            background: linear-gradient(135deg, #28a745 0%, #1e7e34 100%);
            color: white;
        }

        /* Summary Styles */
        .search-summary {
            background: linear-gradient(135deg, #fff 0%, #f8f9fa 100%);
            border: 1px solid #dee2e6;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 25px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }
        
        .search-summary h6 {
            color: #495057;
            font-weight: 600;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
        }
        
        .search-summary h6 i {
            margin-right: 8px;
            color: #17a2b8;
        }

        .summary-row {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            margin-bottom: 10px;
        }
        
        .summary-item {
            display: inline-flex;
            align-items: center;
            padding: 12px 18px;
            background-color: #fff;
            border-radius: 8px;
            border: 2px solid #dee2e6;
            font-weight: 500;
            min-width: 120px;
            justify-content: center;
            transition: all 0.3s ease;
        }
        
        .summary-item:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }
        
        .summary-item i {
            margin-right: 6px;
        }
        
        .summary-item.valid {
            border-color: #28a745;
            color: #28a745;
            background: linear-gradient(135deg, #fff 0%, #f8fff9 100%);
        }
        
        .summary-item.invalid {
            border-color: #dc3545;
            color: #dc3545;
            background: linear-gradient(135deg, #fff 0%, #fff8f8 100%);
        }
        
        .summary-item.total {
            border-color: #007bff;
            color: #007bff;
            font-weight: 600;
            background: linear-gradient(135deg, #fff 0%, #f8fbff 100%);
        }
        
        .summary-item.active {
            border-color: #17a2b8;
            color: #17a2b8;
            background: linear-gradient(135deg, #fff 0%, #f0fcff 100%);
        }

        /* Page Header Styles */
        .page-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            padding-bottom: 15px;
            border-bottom: 2px solid #e9ecef;
        }
        
        .page-header h3 {
            margin: 0;
            color: #495057;
            font-weight: 600;
        }

        /* Update Button Area */
        .update-section {
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            border: 1px solid #dee2e6;
            border-radius: 12px;
            padding: 20px;
            margin-top: 25px;
            text-align: center;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        /* Responsive Design */
        @media (max-width: 1200px) {
            .search-col {
                min-width: 150px;
            }
            
            .search-col-auto {
                min-width: 180px;
            }
        }
        
        @media (max-width: 992px) {
            .search-row {
                flex-direction: column;
            }
            
            .search-col, .search-col-auto {
                min-width: 100%;
            }
            
            .search-col-auto div {
                justify-content: center !important;
            }
            
            .summary-row {
                flex-direction: column;
            }
            
            .summary-item {
                min-width: 100%;
            }
            
            .page-header {
                flex-direction: column;
                gap: 15px;
                text-align: center;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="body" runat="server">
    <!-- Enhanced Page Header -->
    <div class="page-header">
        <h3><i class="fa fa-cogs" aria-hidden="true"></i> Institution Management & SMS Settings</h3>
    </div>

    <!-- Enhanced Search Filters Panel -->
    <div class="search-filters">
        <h6><i class="fa fa-search" aria-hidden="true"></i> Search & Filter Options</h6>
        
        <!-- Single Row with All Controls -->
        <div class="search-row">
            <div class="search-col">
                <label class="form-label">Search Text</label>
                <asp:TextBox ID="SearchTextBox" placeholder="🔍 Institution, Username, Phone, School ID" CssClass="form-control" runat="server"></asp:TextBox>
            </div>
            <div class="search-col">
                <label class="form-label">Validation Status</label>
                <asp:DropDownList ID="ValidationFilter" runat="server" CssClass="form-control">
                    <asp:ListItem Value="" Text="📋 All Status"></asp:ListItem>
                    <asp:ListItem Value="Valid" Text="✅ Valid Only"></asp:ListItem>
                    <asp:ListItem Value="Invalid" Text="❌ Invalid Only"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="search-col">
                <label class="form-label">Payment Active</label>
                <asp:DropDownList ID="PaymentActiveFilter" runat="server" CssClass="form-control">
                    <asp:ListItem Value="" Text="💳 All Payment Status"></asp:ListItem>
                    <asp:ListItem Value="Active" Text="🟢 Payment Active"></asp:ListItem>
                    <asp:ListItem Value="Inactive" Text="🔴 Payment Inactive"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="search-col-auto">
                <label class="form-label" style="visibility: hidden;">Actions</label>
                <div style="display: flex; gap: 10px;">
                    <asp:Button ID="FIndButton" runat="server" Text="🔍 Search" CssClass="btn btn-primary" OnClick="FIndButton_Click" />
                    <asp:Button ID="ClearButton" runat="server" Text="🗑️ Clear All" CssClass="btn btn-secondary" OnClick="ClearButton_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- Enhanced Search Results Summary -->
    <div class="search-summary" id="searchSummary" runat="server" visible="false">
        <h6><i class="fa fa-chart-bar" aria-hidden="true"></i> Search Results Summary</h6>
        <div class="summary-row">
            <div class="summary-item total">
                <i class="fa fa-database" aria-hidden="true"></i>
                <strong>Total Found: <asp:Label ID="TotalCountLabel" runat="server" Text="0"></asp:Label></strong>
            </div>
            <div class="summary-item valid">
                <i class="fa fa-check-circle" aria-hidden="true"></i>
                Valid: <asp:Label ID="ValidCountLabel" runat="server" Text="0"></asp:Label>
            </div>
            <div class="summary-item invalid">
                <i class="fa fa-times-circle" aria-hidden="true"></i>
                Invalid: <asp:Label ID="InvalidCountLabel" runat="server" Text="0"></asp:Label>
            </div>
            <div class="summary-item active">
                <i class="fa fa-credit-card" aria-hidden="true"></i>
                Payment Active: <asp:Label ID="PaymentActiveCountLabel" runat="server" Text="0"></asp:Label>
            </div>
        </div>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="SchoolGridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" DataKeyNames="SchoolID" DataSourceID="InstitutionSQL" CssClass="mGrid">
            <Columns>
                <asp:BoundField DataField="School_SN" HeaderText="SN" SortExpression="School_SN" />
                <asp:BoundField DataField="SchoolID" HeaderText="School ID" SortExpression="SchoolID" InsertVisible="False" ReadOnly="True" />
                <asp:BoundField DataField="SchoolName" HeaderText="Name" SortExpression="SchoolName" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:TemplateField HeaderText="Validation">
                    <ItemTemplate>
                        <asp:CheckBox ID="Validation_CheckBox" Checked='<%#Bind("Validation") %>' Text=" " runat="server" />
                        <input type="hidden" class="IS_Valid" value="<%#Eval("Validation") %>" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Payment Active">
                    <ItemTemplate>
                        <asp:CheckBox ID="Payment_Active_CheckBox" Checked='<%#Bind("IS_ServiceChargeActive") %>' Text=" " runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Per Student" SortExpression="Per_Student_Rate">
                    <ItemTemplate>
                        <asp:TextBox ID="Per_Student_TextBox" ToolTip="Per Student" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" runat="server" Text='<%# Bind("Per_Student_Rate") %>'></asp:TextBox>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Free SMS" SortExpression="Free_SMS">
                    <ItemTemplate>
                        <asp:TextBox ID="Free_SMS_TextBox" ToolTip="Free SMS" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" runat="server" Text='<%# Bind("Free_SMS") %>'></asp:TextBox>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Discount" SortExpression="Discount">
                    <ItemTemplate>
                        <asp:TextBox ID="Discount_TextBox" ToolTip="Discount" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" runat="server" Text='<%# Bind("Discount") %>'></asp:TextBox>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Fixed" SortExpression="Fixed">
                    <ItemTemplate>
                        <asp:TextBox ID="Fixed_TextBox" ToolTip="Fixed" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" runat="server" Text='<%# Bind("Fixed") %>'></asp:TextBox>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div style="text-align: center; padding: 20px; color: #6c757d;">
                    <i class="fa fa-search fa-3x" aria-hidden="true"></i>
                    <h4>No Records Found!</h4>
                    <p>Try adjusting your search criteria.</p>
                </div>
            </EmptyDataTemplate>
        </asp:GridView>
        <asp:SqlDataSource ID="InstitutionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Per_Student_Rate, School_SN, SchoolID, SchoolName, Date, Address, Phone, Free_SMS, Fixed, Discount, IS_ServiceChargeActive, CAST(CASE WHEN Validation = 'Valid' THEN 1 ELSE 0 END AS BIT) AS Validation, UserName FROM SchoolInfo AS Sch ORDER BY School_SN" UpdateCommand="UPDATE SchoolInfo SET Free_SMS = @Free_SMS, Discount = @Discount, Fixed = @Fixed, IS_ServiceChargeActive = @IS_ServiceChargeActive, Validation = @Validation, Per_Student_Rate=@Per_Student_Rate WHERE (SchoolID = @SchoolID)">
            <UpdateParameters>
                <asp:Parameter Name="Free_SMS" />
                <asp:Parameter Name="Discount" />
                <asp:Parameter Name="Fixed" />
                <asp:Parameter Name="SchoolID" />
                <asp:Parameter Name="IS_ServiceChargeActive" />
                <asp:Parameter Name="Validation" />
                <asp:Parameter Name="Per_Student_Rate" />
            </UpdateParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="DeviceActiveInactiveSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AttendanceSettingID FROM Attendance_Device_Setting " UpdateCommand="UPDATE  Attendance_Device_Setting SET  IsActive = @IsActive WHERE (SchoolID = @SchoolID)">
            <UpdateParameters>
                <asp:Parameter Name="IsActive" />
                <asp:Parameter Name="SchoolID" />
            </UpdateParameters>
        </asp:SqlDataSource>
    </div>

    <!-- Enhanced Update Section -->
    <div class="update-section">
        <h5><i class="fa fa-save" aria-hidden="true"></i> Save Changes</h5>
        <p style="margin: 10px 0; color: #6c757d;">Click the button below to save all modifications to institution settings.</p>
        <asp:Button ID="UpdateButton" runat="server" Text="💾 Update All Changes" CssClass="btn btn-success" OnClick="UpdateButton_Click" />
    </div>

    <script>
        $(function () {
            $('.mGrid tr').each(function () {
                if ($(this).find('.IS_Valid').val() === "False") {
                    $(this).addClass("Invaid_Ins");
                }
            });
        });

        function isNumberKey(a) { 
            a = a.which ? a.which : event.keyCode; 
            return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 
        };
    </script>
</asp:Content>
