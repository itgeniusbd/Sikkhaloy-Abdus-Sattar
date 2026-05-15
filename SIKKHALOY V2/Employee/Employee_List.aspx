<%@ Page Title="Employee List" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Employee_List.aspx.cs" Inherits="EDUCATION.COM.Employee.Employee_List" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Emp_List.css" rel="stylesheet" />
    <style>
        .avatar-upload { position: relative; display: inline-block; }
        .avatar-upload .avatar-edit { position: absolute; right: 1px; z-index: 1; bottom: 1px; }
        .avatar-upload .avatar-edit input { display: none; }
        .avatar-upload .avatar-edit input + label { display: inline-block; width: 20px; height: 20px; padding-top: 1px; margin-bottom: 0; border-radius: 50%; background: #FFFFFF; box-shadow: 0px 1px 3px 0px rgba(0, 0, 0, 0.15); cursor: pointer; font-weight: normal; transition: all 0.2s ease-in-out; text-align: center; border: 1px solid #E6E6E6; font-size: 10px; }
        .avatar-upload .avatar-edit input + label:hover { background: #f1f1f1; border-color: #d6d6d6; }
        .avatar-upload .avatar-edit label::after { content: "\f040"; font-family: 'FontAwesome'; color: #757575; }
        .employee-img { object-fit: cover;border-radius:8px; }
        .success_message { display:none; font-size: 80%; margin:0; color: green; font-weight: bold; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Employee List</h3>
    <a class="NoPrint" href="Edit_Employee/Deactivated_Employee_List.aspx">Deactivated Employee List</a>

    <style>
        /* ── Filter Bar ── */
        .filter-bar {
            background: #fff;
            border: 1px solid #dce3ec;
            border-radius: 12px;
            padding: 12px 16px;
            margin-bottom: 14px;
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            gap: 10px;
            box-shadow: 0 2px 8px rgba(0,0,0,.06);
        }
        .filter-divider { width: 1px; height: 28px; background: #dce3ec; margin: 0 2px; }
        .filter-label { font-size: 12px; font-weight: 700; color: #888; text-transform: uppercase; letter-spacing: .5px; white-space: nowrap; }

        /* Type pill buttons */
        .type-pills { display: flex; gap: 6px; align-items: center; }
        .type-pill {
            display: inline-flex; align-items: center; gap: 6px;
            padding: 6px 14px; border-radius: 20px;
            border: 1.5px solid #d0d7e2; cursor: pointer;
            font-size: 13px; font-weight: 500; color: #555;
            background: #f8f9fb; transition: all .18s;
            line-height: 1;
        }
        .type-pill:hover { border-color: #1a6fc4; color: #1a6fc4; }
        .type-pill.active {
            background: #1a6fc4; border-color: #1a6fc4;
            color: #fff; box-shadow: 0 2px 8px rgba(26,111,196,.28);
        }

        /* Sub-filter */
        .sub-filter-wrap { display: none; align-items: center; gap: 8px; }
        .sub-filter-wrap select {
            border-radius: 8px; border: 1.5px solid #1a6fc4;
            padding: 6px 12px; font-size: 13px; color: #1a6fc4;
            font-weight: 600; background: #f0f5ff; cursor: pointer; outline: none;
        }
        .sub-filter-wrap .sub-label { font-size: 12px; color: #888; white-space: nowrap; font-weight: 600; }
        .btn-manage-sub {
            display: inline-flex; align-items: center; gap: 5px;
            border: 1.5px solid #1a6fc4; color: #1a6fc4; background: #fff;
            border-radius: 8px; padding: 5px 12px; font-size: 12px; font-weight: 600;
            text-decoration: none; white-space: nowrap; transition: all .15s;
        }
        .btn-manage-sub:hover { background: #1a6fc4; color: #fff; text-decoration: none; }

        /* Search & action row */
        .filter-actions { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
        .filter-search-wrap { position: relative; }
        .filter-search-wrap input {
            border-radius: 8px; border: 1.5px solid #d0d7e2;
            padding: 7px 12px 7px 34px; font-size: 13px; min-width: 190px; outline: none;
        }
        .filter-search-wrap input:focus { border-color: #1a6fc4; box-shadow: 0 0 0 3px rgba(26,111,196,.1); }
        .filter-search-wrap .search-icon { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); pointer-events: none; }
        .btn-find {
            display: inline-flex; align-items: center; gap: 6px;
            background: #1a6fc4; color: #fff; border: none;
            border-radius: 8px; padding: 7px 18px; font-size: 13px; font-weight: 600; cursor: pointer;
        }
        .btn-print {
            display: inline-flex; align-items: center; gap: 6px;
            background: #1e7e34; color: #fff; border: none;
            border-radius: 8px; padding: 7px 18px; font-size: 13px; font-weight: 600; cursor: pointer;
        }

        @media print {
            .filter-bar, .filter-bar * { display: none !important; }
        }
    </style>

    <div class="filter-bar NoPrint">

        <%-- Employee Type Pills --%>
        <span class="filter-label">Filter</span>
        <div class="type-pills">
            <button type="button" class="type-pill active" data-val="%">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                All
            </button>
            <button type="button" class="type-pill" data-val="Teacher">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="3" width="20" height="14" rx="2"/><path d="M8 21h8M12 17v4"/></svg>
                Teacher
            </button>
            <button type="button" class="type-pill" data-val="Staff">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 7V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v2"/><line x1="12" y1="12" x2="12" y2="16"/><line x1="10" y1="14" x2="14" y2="14"/></svg>
                Staff
            </button>
        </div>

        <div class="filter-divider"></div>

        <%-- Sub-category filter --%>
        <div class="sub-filter-wrap" id="subFilterWrap">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="#888" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3"/></svg>
            <span class="sub-label">Sub-Type</span>
            <asp:DropDownList ID="SubCategoryDropDownList" runat="server" AutoPostBack="True"
                OnSelectedIndexChanged="SubCategoryDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <a href="Manage_SubCategory.aspx" class="btn-manage-sub">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06A1.65 1.65 0 0 0 4.68 15a1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06A1.65 1.65 0 0 0 9 4.68a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06A1.65 1.65 0 0 0 19.4 9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/></svg>
                Manage
            </a>
        </div>

        <div class="filter-divider"></div>

        <%-- Hidden real RadioButtonList for postback --%>
        <asp:RadioButtonList CssClass="d-none" ID="EmpTypeRadioButtonList" runat="server" AutoPostBack="True" RepeatLayout="Flow" RepeatDirection="Horizontal" OnSelectedIndexChanged="EmpTypeRadioButtonList_SelectedIndexChanged">
            <asp:ListItem Selected="True" Value="%">All Employee</asp:ListItem>
            <asp:ListItem>Teacher</asp:ListItem>
            <asp:ListItem>Staff</asp:ListItem>
        </asp:RadioButtonList>

        <%-- Search & Buttons --%>
        <div class="filter-actions">
            <div class="filter-search-wrap">
                <svg class="search-icon" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="#aaa" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
                <asp:TextBox ID="FindTextBox" runat="server" placeholder="Search keyword..." CssClass=""></asp:TextBox>
            </div>
            <asp:Button ID="FindButton" runat="server" CssClass="btn-find" Text="Find" OnClick="FindButton_Click" />
            <button type="button" class="btn-print" onclick="window.print();">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 6 2 18 2 18 9"/><path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2"/><rect x="6" y="14" width="12" height="8"/></svg>
                Print
            </button>
        </div>
    </div>

    <script>
        (function () {
            var pills = document.querySelectorAll('.type-pill');

            function syncToHiddenRBL(val) {
                var inputs = document.querySelectorAll('[id$="EmpTypeRadioButtonList"] input[type=radio]');
                inputs.forEach(function (inp) {
                    if (inp.value === val) { inp.checked = true; inp.click(); }
                });
            }

            function setInitialState() {
                var inputs = document.querySelectorAll('[id$="EmpTypeRadioButtonList"] input[type=radio]');
                var selectedVal = '%';
                inputs.forEach(function (inp) { if (inp.checked) selectedVal = inp.value; });
                pills.forEach(function (btn) {
                    btn.classList.toggle('active', btn.dataset.val === selectedVal);
                });
                toggleSubFilter(selectedVal);
            }

            function toggleSubFilter(val) {
                var wrap = document.getElementById('subFilterWrap');
                if (val === 'Teacher' || val === 'Staff') {
                    wrap.style.display = 'flex';
                } else {
                    wrap.style.display = 'none';
                }
            }

            pills.forEach(function (btn) {
                btn.addEventListener('click', function () {
                    pills.forEach(function (b) { b.classList.remove('active'); });
                    this.classList.add('active');
                    var val = this.dataset.val;
                    toggleSubFilter(val);
                    syncToHiddenRBL(val);
                });
            });

            setInitialState();
        })();
    </script>

    <div class="alert alert-info">
<asp:Label ID="CountLabel" runat="server"></asp:Label>
    </div>

 <div class="table-responsive">
        <asp:GridView ID="EmployeeGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="EmployeeID,EmployeeType" DataSourceID="EmployeeSQL" AllowSorting="True" OnRowDataBound="EmployeeGridView_RowDataBound">
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
           <asp:TemplateField HeaderText="Emp.SubType" SortExpression="SubCategoryName">
               <ItemTemplate>
                   <asp:DropDownList ID="SubCatAssignDDL" runat="server" CssClass="form-control subcat-ddl d-print-none"
                       data-empid='<%# Eval("EmployeeID") %>'
                       data-emptype='<%# Eval("EmployeeType") %>'
                       style="font-size:12px; padding:2px 6px; min-width:130px; border-radius:6px; border:1px solid #aaa;">
                   </asp:DropDownList>
                   <span class="d-none d-print-block"><%# Eval("SubCategoryName") %></span>
               </ItemTemplate>
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
   <div class="avatar-upload">
    <div class="avatar-edit d-print-none">
              <input name="Employee_Photo" id="emp_<%# Container.DataItemIndex %>" type="file" accept="image/x-png,image/jpeg" />
  <label for="emp_<%# Container.DataItemIndex %>"></label>
       </div>
       <img alt="" src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>" class="employee-img z-depth-1 img-thumbnail" />
 <input class="EmployeeID" value="<%# Eval("EmployeeID") %>" type="hidden" />
          <input class="EmployeeType" value="<%# Eval("EmployeeType") %>" type="hidden" />
    <p class="text-center success_message">Upload Success!</p>
     </div>
         </ItemTemplate>
          <ItemStyle VerticalAlign="Middle" CssClass="Itm_Img" />
    </asp:TemplateField>
 </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
       SelectCommand="SELECT EmployeeID, ID, Bank_AccNo, EmployeeType, Permanent_Temporary, Salary, FirstName +' '+ LastName as Name, FatherName, Designation, Phone, DeviceID, SubCategoryID, SubCategoryName FROM VW_Emp_Info WHERE (SchoolID = @SchoolID) AND (Job_Status = N'Active') AND (EmployeeType LIKE @EmployeeType) AND (@SubCategoryID = 0 OR SubCategoryID = @SubCategoryID) order by ID"
            FilterExpression="ID LIKE '{0}%' or Name LIKE '{0}%' or Designation LIKE '{0}%' or Phone LIKE '{0}%'"
            UpdateCommand="IF NOT EXISTS (SELECT * FROM Employee_Info WHERE ID = @ID AND SchoolID = @SchoolID) UPDATE Employee_Info SET ID = @ID WHERE (EmployeeID = @EmployeeID)"
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
                <asp:ControlParameter ControlID="SubCategoryDropDownList" Name="SubCategoryID" PropertyName="SelectedValue" DefaultValue="0" Type="Int32" />
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
    <asp:Button ID="UploadButton" runat="server" CssClass="btn btn-primary d-print-none" OnClick="UploadButton_Click" Text="Update ID, Salary, Bank A/C & Type" />
    <%}%>

    <script src="/JS/Resize_Img/canvasResize.js"></script>
    <script type="text/javascript">
$(document).ready(function () {
    // Sub-category quick assign from grid
    $(document).on('change', '.subcat-ddl', function () {
        var empId = $(this).data('empid');
        var subCatId = $(this).val();
        var ddl = $(this);
        $.ajax({
            url: 'Employee_List.aspx/AssignSubCategory',
            data: JSON.stringify({ 'employeeID': empId, 'subCategoryID': parseInt(subCatId) }),
            dataType: 'json', type: 'POST',
            contentType: 'application/json; charset=utf-8',
            success: function () {
                ddl.css({ 'border-color': '#28a745', 'box-shadow': '0 0 4px rgba(40,167,69,.4)' });
                setTimeout(function () { ddl.css({ 'border-color': '', 'box-shadow': '' }); }, 1500);
            },
            error: function () { alert('সংরক্ষণ ব্যর্থ হয়েছে।'); }
        });
    });

            //upload image
       $('input[name=Employee_Photo]').change(function (input) {
    var file = input.target.files[0];
     var prev = $(this).closest('.avatar-upload').find('.employee-img');
            var empId = $(this).closest('.avatar-upload').find('.EmployeeID');
        var empType = $(this).closest('.avatar-upload').find('.EmployeeType');
          var success_msg = $(this).closest('.avatar-upload').find('.success_message');

  var Valid = ["image/jpg", "image/jpeg", "image/png"];

                if ($.inArray(file["type"], Valid) < 0) {
     alert('Please upload file having extensions .jpeg/.jpg/.png only');
       return false;
             }
             else {
          canvasResize(file, {
   width: 300,
               height: 330,
         quality: 70,
     callback: function (idata) {
                  $(prev).attr('src', idata);

   $.ajax({
           url: "Employee_List.aspx/UpdateEmployeeImage",
     data: JSON.stringify({ 'EmployeeID': empId.val(), 'EmployeeType': empType.val(), 'Image': idata.split(",")[1] }),
       dataType: "json",
             type: "POST",
 contentType: "application/json; charset=utf-8",
              success: function (response) {
    success_msg.fadeIn();
      setTimeout(function () { success_msg.fadeOut("slow") }, 2000);
   },
               error: function (xhr) {
     var err = JSON.parse(xhr.responseText);
               alert(err.Message || 'Error uploading image');
 }
      });
         }
       });
              }
 });
   });
    </script>
</asp:Content>
