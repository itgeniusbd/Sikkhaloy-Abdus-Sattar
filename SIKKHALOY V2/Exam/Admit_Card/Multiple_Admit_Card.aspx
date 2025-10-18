<%@ Page Title="Admit Card" Language="C#" MasterPageFile="~/BASIC.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Multiple_Admit_Card.aspx.cs" Inherits="EDUCATION.COM.Exam.Admit_Card.Multiple_Admit_Card" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/Custom.css?v=14.3" rel="stylesheet" />
    <link href="css/skin1.css?v=6" id="DefaultCSS" rel="stylesheet" />


    <%--<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
<script src="jscolor.js"></script>--%>

    <style>
        #canvas {
            width: 500px;
            height: 250px;
            background-color: #000;
        }

        input#colorpic {
            width: 100px;
            height: 40px;
            background-color: transparent;
            float: left;
        }

        .idcardborder {
            border: 2px solid #0075d2;
        }

        /* Header Section Styling */
        .header-section {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
            margin: -15px -15px 20px -15px !important;
            padding: 15px 30px !important;
            border-radius: 0 0 10px 10px !important;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1) !important;
        }

        .page-title,
        .header-section h3,
        .header-section .page-title {
            color: white !important;
            margin: 0 !important;
            padding: 8px 0 !important;
            font-weight: 600 !important;
            text-shadow: 0 1px 2px rgba(0,0,0,0.2) !important;
            background: transparent !important;
            background-color: transparent !important;
            border: none !important;
            box-shadow: none !important;
        }

        /* Navigation Links Styling */
        .navigation-links {
            margin-bottom: 0 !important;
            display: flex !important;
            justify-content: flex-end !important;
            align-items: center !important;
            gap: 10px !important;
        }

        .nav-link-modern {
            display: inline-block !important;
            padding: 8px 15px !important;
            margin: 0 !important;
            background: rgba(255,255,255,0.2) !important;
            color: white !important;
            text-decoration: none !important;
            border-radius: 6px !important;
            font-size: 13px !important;
            font-weight: 500 !important;
            transition: all 0.3s ease !important;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1) !important;
            border: 1px solid rgba(255,255,255,0.3) !important;
        }

        .nav-link-modern:hover {
            background: rgba(255,255,255,0.3) !important;
            color: white !important;
            text-decoration: none !important;
            transform: translateY(-1px) !important;
            box-shadow: 0 4px 8px rgba(0,0,0,0.15) !important;
        }

        .nav-link-modern i {
            margin-right: 5px !important;
        }

        /* Form Layout Styling */
        .form-row {
            margin-bottom: 20px;
            padding: 15px;
            background: #f8f9fa;
            border-radius: 8px;
            border-left: 4px solid #007bff;
        }

        .configuration-panel {
            background: #fff !important;
            border: 1px solid #e9ecef !important;
            border-left: 4px solid #28a745 !important;
        }

        .control-label {
            font-weight: 600;
            color: #495057;
            margin-bottom: 5px;
            font-size: 12px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        /* Search Input Group Fix */
        .search-input-group {
            display: flex !important;
            align-items: stretch !important;
            width: 100% !important;
        }

        .search-input {
            flex: 1 !important;
            border-radius: 6px 0 0 6px !important;
            border-right: none !important;
            margin: 0 !important;
        }

        .search-btn {
            border-radius: 0 6px 6px 0 !important;
            border-left: none !important;
            min-width: 50px !important;
            display: flex !important;
            align-items: center !important;
            justify-content: center !important;
            padding: 8px 16px !important;
            margin: 0 !important;
        }

        /* Modern Button Styles */
        .modern-btn {
            position: relative;
            padding: 8px 16px;
            border: none;
            border-radius: 6px;
            font-weight: 600;
            font-size: 13px;
            cursor: pointer;
            transition: all 0.3s ease;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            text-decoration: none;
            display: inline-flex;
            align-items: center;
            gap: 6px;
            margin-bottom: 5px;
        }

        .modern-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            text-decoration: none;
        }

        .modern-btn:active {
            transform: translateY(0);
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .modern-btn:focus {
            outline: none;
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.3);
        }

        /* Button Variants */
        .btn-search {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
            color: white !important;
            border: none !important;
        }

        .btn-search:hover {
            background: linear-gradient(135deg, #5a67d8 0%, #6b46c1 100%) !important;
            color: white !important;
        }

        .btn-upload {
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%) !important;
            color: white !important;
            border: none !important;
            font-size: 11px !important;
            padding: 6px 12px !important;
        }

        .btn-upload:hover {
            background: linear-gradient(135deg, #ec77ab 0%, #ef4444 100%) !important;
            color: white !important;
        }

        .btn-print {
            background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%) !important;
            color: white !important;
            border: none !important;
        }

        .btn-print:hover {
            background: linear-gradient(135deg, #2563eb 0%, #06b6d4 100%) !important;
            color: white !important;
        }

        .btn-color {
            background: linear-gradient(135deg, #fa709a 0%, #fee140 100%) !important;
            color: white !important;
            border: none !important;
        }

        .btn-color:hover {
            background: linear-gradient(135deg, #f472b6 0%, #fbbf24 100%) !important;
            color: white !important;
        }

        .btn-reset {
            background: linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%) !important;
            color: #8b5cf6 !important;
            border: 1px solid #f59e0b !important;
        }

        .btn-reset:hover {
            background: linear-gradient(135deg, #fed7aa 0%, #f97316 100%) !important;
            color: white !important;
        }

        /* Signature Group Styling */
        .signature-group {
            display: flex;
            gap: 5px;
            align-items: stretch;
        }

        .signature-group .form-control {
            flex: 1;
        }

        .signature-group .btn {
            flex-shrink: 0;
        }

        /* Action Buttons Container */
        .action-buttons .btn {
            margin-bottom: 8px;
        }

        /* Modern Input Styles */
        .modern-input {
            border: 2px solid #e2e8f0;
            border-radius: 6px;
            padding: 8px 12px;
            font-size: 13px;
            transition: all 0.3s ease;
            background: #f8fafc;
        }

        .modern-input:focus {
            border-color: #667eea;
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
            background: white;
            outline: none;
        }

        .modern-input::placeholder {
            color: #94a3b8;
            font-weight: 400;
        }

        /* Modern Select Styles */
        .modern-select {
            border: 2px solid #e2e8f0;
            border-radius: 6px;
            padding: 8px 12px;
            font-size: 13px;
            background: #f8fafc;
            transition: all 0.3s ease;
        }

        .modern-select:focus {
            border-color: #667eea;
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
            background: white;
            outline: none;
        }

        /* Enhanced Dropdown Styles */
        .color-dropdown {
            position: relative;
        }

        .modern-dropdown {
            border: none;
            border-radius: 12px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.15);
            padding: 15px 0;
            background: white;
            min-width: 250px;
            right: 0;
            left: auto;
        }

        .modern-dropdown .dropdown-header {
            color: #4338ca !important;
            font-weight: 700 !important;
            padding: 8px 20px !important;
            background: #f8fafc !important;
            margin: 0 10px !important;
            border-radius: 6px !important;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .modern-dropdown .divider {
            margin: 8px 20px !important;
            border-color: #e2e8f0 !important;
        }

        /* Color Picker Items */
        .color-picker-item {
            padding: 8px 20px !important;
            display: flex !important;
            align-items: center !important;
            justify-content: space-between !important;
        }

        .color-picker-item .color-label {
            font-weight: 600;
            color: #374151;
            font-size: 13px;
        }

        .modern-color-picker {
            width: 45px;
            height: 30px;
            border: 2px solid #e2e8f0;
            border-radius: 6px;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .modern-color-picker:hover {
            border-color: #667eea;
            transform: scale(1.05);
        }

        /* Form Group Spacing */
        .form-group {
            margin-bottom: 15px;
        }

        /* Override Bootstrap spacing */
        .col-md-2 {
            padding-left: 8px;
            padding-right: 8px;
        }

        /* Alert styling */
        .alert-success {
            border-left: 4px solid #28a745;
            background: #d4edda;
            border-color: #c3e6cb;
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            .header-section {
                margin: -15px -15px 15px -15px !important;
                padding: 10px 15px !important;
            }
            
            .navigation-links {
                flex-direction: column !important;
                gap: 5px !important;
            }
            
            .nav-link-modern {
                width: 100% !important;
                text-align: center !important;
            }

            .form-row {
                padding: 10px;
            }
            
            .signature-group {
                flex-direction: column;
            }
            
            .modern-btn {
                font-size: 12px;
                padding: 6px 12px;
            }
        }

        /* Animation for buttons */
        @keyframes pulse {
            0% { box-shadow: 0 0 0 0 rgba(102, 126, 234, 0.7); }
            70% { box-shadow: 0 0 0 10px rgba(102, 126, 234, 0); }
            100% { box-shadow: 0 0 0 0 rgba(102, 126, 234, 0); }
        }

        .btn-search:focus {
            animation: pulse 1.5s infinite;
        }

        /* Enhanced Visual Effects */
        .modern-btn::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(255,255,255,0.1);
            border-radius: 6px;
            opacity: 0;
            transition: opacity 0.3s ease;
        }

        .modern-btn:hover::before {
            opacity: 1;
        }

        /* Loading animation for buttons */
        .modern-btn.loading {
            opacity: 0.7;
            pointer-events: none;
        }

        .modern-btn.loading::after {
            content: '';
            position: absolute;
            width: 16px;
            height: 16px;
            margin: auto;
            border: 2px solid transparent;
            border-top-color: #ffffff;
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
    </style>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <!-- Header Section with Right-aligned Navigation Links -->
    <div class="row header-section">
        <div class="col-md-6">
            <h3 class="page-title">Admit Card</h3>
        </div>
        <div class="col-md-6 text-right">
            <div class="navigation-links">
                <a class="d-print-none nav-link-modern" href="Old_AdmitCard.aspx">
                    <i class="glyphicon glyphicon-file"></i> Old Admit Card
                </a>
                <a class="d-print-none nav-link-modern" href="AdmitCard_WithoutPhoto.aspx">
                    <i class="glyphicon glyphicon-book"></i> প্রবেশপত্র বাংলায়
                </a>
                <a class="d-print-none nav-link-modern" href="Old_AdmitCardWithoutPhoto.aspx">
                    <i class="glyphicon glyphicon-list-alt"></i> প্রবেশপত্র বাংলায় পুরাতন
                </a>
            </div>
        </div>
    </div>
  
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- First Row: Filter Controls -->
    <div class="form-row NoPrint">
        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Exam Selection</label>
                <asp:DropDownList ID="ExamDropDownList" runat="server" onchange="showMe(this);" CssClass="form-control modern-select" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" AppendDataBoundItems="True" AutoPostBack="True" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
                    <asp:ListItem Value="0">[ EXAM ]</asp:ListItem>
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorStar" ErrorMessage="*" InitialValue="0" ValidationGroup="F"></asp:RequiredFieldValidator>
                <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ExamID, ExamName FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>

        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Class</label>
                <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control modern-select" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" AutoPostBack="True" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                    <asp:ListItem Value="0">[ CLASS ]</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>

        <%if (SectionDropDownList.Items.Count > 1) {%>
        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Section</label>
                <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control modern-select" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                </asp:DropDownList>
                <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID)">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>
        <%}%>

        <%if (GroupDropDownList.Items.Count > 1) { %>
        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Group</label>
                <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control modern-select" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                </asp:DropDownList>
                <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID)">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>
        <%}%>

        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Payment Status</label>
                <asp:DropDownList ID="Paid_DropDownList" CssClass="form-control modern-select" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                    <asp:ListItem Value="0">[ ALL STUDENT ]</asp:ListItem>
                    <asp:ListItem>Paid</asp:ListItem>
                    <asp:ListItem>Due</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>

        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Search by IDs</label>
                <div class="search-input-group">
                    <asp:TextBox ID="Find_ID_TextBox" runat="server" CssClass="form-control modern-input search-input" placeholder="Enter student IDs"></asp:TextBox>
                    <asp:Button ID="FindButton" runat="server" Text="🔍" CssClass="btn btn-search modern-btn search-btn" OnClick="FindButton_Click" ValidationGroup="F" />
                </div>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*" CssClass="EroorStar" ControlToValidate="Find_ID_TextBox" ValidationGroup="F"></asp:RequiredFieldValidator>
            </div>
        </div>
    </div>

    <!-- Second Row: Configuration and Actions -->
    <div class="form-row NoPrint configuration-panel">
        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Exam Name</label>
                <input id="ExamNameTextBox" class="form-control modern-input" type="text" value="" placeholder="Customize exam name" />
            </div>
        </div>

        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Teacher Signature</label>
                <div class="signature-group">
                    <input id="TeacherSign" class="form-control modern-input" type="text" value="Teacher signature" placeholder="Teacher text" />
                    <label class="btn btn-upload modern-btn btn-sm">
                        📤 Upload
                        <input id="Tfileupload" type="file" style="display: none;" />
                    </label>
                </div>
            </div>
        </div>

        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Principal Signature</label>
                <div class="signature-group">
                    <input id="PrincipalSign" class="form-control modern-input" type="text" value="Headmaster signature" placeholder="Principal text" />
                    <label class="btn btn-upload modern-btn btn-sm">
                        📤 Upload
                        <input id="Hfileupload" type="file" style="display: none;" />
                    </label>
                </div>
            </div>
        </div>

        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Cards Per Page</label>
                <select id="PrintPage" class="form-control modern-select">
                    <option value="1">📄 2 Cards</option>
                    <option value="2" selected="selected">📄 4 Cards</option>
                    <option value="3">📄 6 Cards</option>
                </select>
            </div>
        </div>

        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Issue Date</label>
                <input id="Issue_d" placeholder="Select date" autocomplete="off" type="text" class="form-control p-date modern-input" />
            </div>
        </div>

        <div class="col-md-2">
            <div class="form-group">
                <label class="control-label">Actions</label>
                <div class="action-buttons">
                    <div class="dropdown color-dropdown">
                        <button class="btn btn-color modern-btn btn-block" type="button" data-toggle="dropdown">
                            🎨 Colors
                            <span class="caret"></span>
                        </button>
                        <ul class="dropdown-menu modern-dropdown">
                            <li class="dropdown-header text-center"><strong>🎨 Background Color</strong></li>
                            <li class="divider"></li>
                            <li class="color-picker-item">
                                <span class="color-label">Card Header</span>
                                <input type="color" class="getColor modern-color-picker" />
                            </li>
                            <li class="divider"></li>
                            <li class="dropdown-header text-center"><strong>✒️ Font Color</strong></li>
                            <li class="divider"></li>
                            <li class="color-picker-item">
                                <span class="color-label">Header Text</span>
                                <input type="color" class="getfontColor modern-color-picker" />
                            </li>
                            <li class="divider"></li>
                            <li class="text-center">
                                <button type="button" class="btn btn-reset modern-btn btn-sm" id="resetColorsBtn">🔄 Reset</button>
                            </li>
                        </ul>
                    </div>
                    <button type="button" class="btn btn-print modern-btn btn-block" onclick="window.print();">
                        🖨️ Print Cards
                    </button>
                </div>
            </div>
        </div>
    </div>

    <div class="alert alert-success hidden-print">
        <asp:Label ID="TotalCardLabel" runat="server"></asp:Label>
        [Page orientation "landscape" prefer mozilla browser]
    </div>

    <div id="wrapper">
        <asp:Repeater ID="IDCardDL" runat="server">
            <ItemTemplate>
                <div class="idcardborder">
                    <div class="card-header color-output">
                        <div class="pl-1">
                            <img src='/Handeler/SchoolLogo.ashx?SLogo=<%#Eval("SchoolID") %>' />
                        </div>
                        <div>
                            <h4 class="font-weight-bold"><%# Eval("SchoolName") %></h4>
                            <p class="font-weight-bold"><%# Eval("Address") %></p>
                        </div>
                    </div>

                    <div class="Card_Title">ADMIT CARD</div>

                    <div class="student-info">
                        <div class="s-Photo">
                            <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" class="img-thumbnail rounded-circle" />
                            <strong>ID: <%# Eval("ID") %></strong>
                        </div>
                        <div class="Info">
                            <table>
                                <tr>
                                    <td>Exam</td>
                                    <td>:</td>
                                    <td class="ExamName"></td>
                                </tr>
                                <tr>
                                    <td>Session</td>
                                    <td>:</td>
                                    <td><%# Eval("EducationYear")%></td>
                                </tr>
                                <tr>
                                    <td>Name</td>
                                    <td>:</td>
                                    <td><%# Eval("StudentsName")%></td>
                                </tr>
                                <tr>
                                    <td>Class</td>
                                    <td>:</td>
                                    <td><%# Eval("Class") %></td>
                                </tr>
                                <tr>
                                    <td>Roll No</td>
                                    <td>:</td>
                                    <td><%# Eval("RollNo") %></td>
                                </tr>

                                <tr class="Group" style="display: none;">
                                    <td>Group</td>
                                    <td>:</td>
                                    <td><%# Eval("SubjectGroup") %></td>
                                </tr>
                                <tr class="Section" style="display: none;">
                                    <td>Section</td>
                                    <td>:</td>
                                    <td><%# Eval("Section") %></td>
                                </tr>
                                <tr class="Shift" style="display: none;">
                                    <td>Shift</td>
                                    <td>:</td>
                                    <td><%# Eval("Shift") %></td>
                                </tr>
                                <tr>
                                    <td>Issue</td>
                                    <td>:</td>
                                    <td class="Issue"></td>
                                </tr>
                            </table>
                        </div>
                    </div>

                    <div class="Sign">
                        <div class="pull-left">
                            <div class="SignTeacher">
                                <img class="TeacherSign" src="/Handeler/Sign_Teacher.ashx?sign=<%# Eval("SchoolID") %>" />
                            </div>
                            <label class="Teacher">Teacher</label>
                        </div>
                        <div class="text-right pull-right">
                            <div class="SignHead">
                                <img class="HeadSign" src="/Handeler/Sign_Principal.ashx?sign=<%# Eval("SchoolID") %>" />
                            </div>
                            <label class="Head">Principal</label>
                        </div>
                        <div class="clearfix"></div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <asp:SqlDataSource ID="ICardInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, SchoolInfo.SchoolID, StudentsClass.StudentID, SchoolInfo.SchoolName, Student.StudentsName, Student.FathersName, CreateClass.Class, CreateSection.Section, SchoolInfo.Address, CreateShift.Shift, StudentsClass.RollNo, Education_Year.EducationYear, CreateSubjectGroup.SubjectGroup, StudentsClass.SubjectGroupID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = 'Active') AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID)">
        <SelectParameters>
            <asp:SessionParameter DefaultValue="" Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="IDsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, SchoolInfo.SchoolID, StudentsClass.StudentID, SchoolInfo.SchoolName, Student.StudentsName, Student.FathersName, CreateClass.Class, CreateSection.Section, SchoolInfo.Address, CreateShift.Shift, StudentsClass.RollNo, Education_Year.EducationYear, CreateSubjectGroup.SubjectGroup FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = 'Active') AND (StudentsClass.SchoolID = @SchoolID) AND (Student.ID IN (SELECT id FROM dbo.In_Function_Parameter(@IDs) AS In_Function_Parameter_1))">
        <SelectParameters>
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="Find_ID_TextBox" Name="IDs" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>


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

    <%--<div class="color-picker">
        <canvas id="canvas"></canvas>
        <br />
        <input type="color" id="colorpic" />
        <div id="colorCode">#000</div>

    </div>




    <div id="canvas_1" style="width: 300px; height: 100px">
    </div>--%>




    <script src="/JS/jquery.colorpanel.js"></script>


    <script>
        //var canvas = document.getElementById("canvas_3");
        //var input = document.getElementById("colorpic");
        //var colorCode = document.getElementById("colorCode");

        //input.addEventListener("input", setColor);
        //function setColor() {
        //    canvas.style.backgroundColor = input.value;
        //    colorCode.innerHTML = input.value;
        //}
        //setColor();

        /*Sign Upload*/
        //Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (e, f) {
        $(function () {
            if ($('.Group td').eq(2).text() != "") {
                $('.Group').show();
            }
            if ($('.Section td').eq(2).text() != "") {
                $('.Section').show();
            }
            if ($('.Shift td').eq(2).text() != "") {
                $('.Shift').show();
            }

            $(".p-date").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            }).datepicker('setDate', new Date());;

            // Force apply modern button styles after page load
            setTimeout(function() {
                // Apply modern button styles
                $('.btn-search').css({
                    'background': 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    'color': 'white',
                    'border': 'none'
                });
                
                $('.btn-upload').css({
                    'background': 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
                    'color': 'white',
                    'border': 'none'
                });
                
                $('.btn-print').css({
                    'background': 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
                    'color': 'white',
                    'border': 'none'
                });
                
                $('.btn-color').css({
                    'background': 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
                    'color': 'white',
                    'border': 'none'
                });
                
                $('.btn-reset').css({
                    'background': 'linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%)',
                    'color': '#8b5cf6',
                    'border': '1px solid #f59e0b'
                });
            }, 100);

            //Issue Date
            $(".Issue").text($('#Issue_d').val());
            $('#Issue_d').on('change', function () {
                $(".Issue").text($(this).val());
            });

            // Enhanced file upload with loading states
            $("#Tfileupload").change(function (input) {
                var $btn = $(this).closest('.btn-upload');
                $btn.addClass('loading').text('📤 Uploading...');
                
                var file = input.target.files[0];
                var Valid = ["image/jpg", "image/jpeg", "image/png"];

                if ($.inArray(file["type"], Valid) < 0) {
                    alert('Please upload file having extensions .jpeg/.jpg/.png only');
                    $btn.removeClass('loading').html('📤 Upload Teacher Sign');
                    return false;
                }
                else {
                    var fileReader = new FileReader();

                    fileReader.readAsDataURL(file);
                    fileReader.onload = function (event) {
                        var image = new Image();

                        image.src = event.target.result;
                        image.onload = function () {
                            var canvas = document.createElement("canvas");
                            var context = canvas.getContext("2d");
                            canvas.width = 130;//image.width / 4;
                            canvas.height = 40; //image.height / 4;
                            context.drawImage(image, 0, 0, image.width, image.height, 0, 0, canvas.width, canvas.height);

                            $('.TeacherSign').attr("src", canvas.toDataURL());

                            //Save to server
                            $.ajax({
                                url: "Multiple_Admit_Card.aspx/Teacher_Sign",
                                data: JSON.stringify({ 'Image': canvas.toDataURL().split(",")[1] }),
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                success: function (response) {
                                    console.log(response);
                                    $btn.removeClass('loading').html('📤 Upload Teacher Sign');
                                    
                                    // Show success feedback
                                    $btn.css('background', 'linear-gradient(135deg, #10b981 0%, #065f46 100%)');
                                    setTimeout(function() {
                                        $btn.css('background', 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)');
                                    }, 2000);
                                },
                                error: function (xhr) {
                                    var err = JSON.parse(xhr.responseText);
                                    console.log(err.message);
                                    $btn.removeClass('loading').html('📤 Upload Teacher Sign');
                                }
                            });
                        }
                    }
                }
            });

            $("#Hfileupload").change(function (input) {
                var $btn = $(this).closest('.btn-upload');
                $btn.addClass('loading').text('📤 Uploading...');
                
                var file = input.target.files[0];
                var Valid = ["image/jpg", "image/jpeg", "image/png"];

                if ($.inArray(file["type"], Valid) < 0) {
                    alert('Please upload file having extensions .jpeg/.jpg/.png only');
                    $btn.removeClass('loading').html('📤 Upload Principal Sign');
                    return false;
                }
                else {
                    var fileReader = new FileReader();

                    fileReader.readAsDataURL(file);
                    fileReader.onload = function (event) {
                        var image = new Image();

                        image.src = event.target.result;
                        image.onload = function () {
                            var canvas = document.createElement("canvas");
                            var context = canvas.getContext("2d");
                            canvas.width = 130;//image.width / 4;
                            canvas.height = 40; //image.height / 4;
                            context.drawImage(image, 0, 0, image.width, image.height, 0, 0, canvas.width, canvas.height);

                            $('.HeadSign').attr("src", canvas.toDataURL());

                            //Save to server
                            $.ajax({
                                url: "Multiple_Admit_Card.aspx/Principal_Sign",
                                data: JSON.stringify({ 'Image': canvas.toDataURL().split(",")[1] }),
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                success: function (response) {
                                    console.log(response);
                                    $btn.removeClass('loading').html('📤 Upload Principal Sign');
                                    
                                    // Show success feedback
                                    $btn.css('background', 'linear-gradient(135deg, #10b981 0%, #065f46 100%)');
                                    setTimeout(function() {
                                        $btn.css('background', 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)');
                                    }, 2000);
                                },
                                error: function (xhr) {
                                    var err = JSON.parse(xhr.responseText);
                                    console.log(err.message);
                                    $btn.removeClass('loading').html('📤 Upload Principal Sign');
                                }
                            });
                        }
                    }
                }
            });

            $('.ExamName').text($('[id*=ExamDropDownList] :selected').text());

            $('#ExamNameTextBox').val($('[id*=ExamDropDownList] :selected').text());

            $(".Teacher").text($("#TeacherSign").val());
            $("#TeacherSign").on('keyup', function () {
                $(".Teacher").text($(this).val());
            });

            $("#ExamNameTextBox").on('keyup', function () {
                $(".ExamName").text($(this).val());
            });

            $(".Head").text($("#PrincipalSign").val());
            $("#PrincipalSign").on('keyup', function () {
                $(".Head").text($(this).val());
            });

            $("#wrapper").css("grid-template-columns", "repeat(" + $('#PrintPage').val() + ", 1fr)");
            $("#PrintPage").change(function () {
                $("#wrapper").css("grid-template-columns", "repeat(" + this.value + ", 1fr)");

                if (this.value == 3) {
                    $(".card-header h4").css('font-size', '1rem !important');
                }
                
                // Reapply colors after layout change
                setTimeout(function() {
                    if (typeof applySavedColors === 'function') {
                        applySavedColors();
                    }
                }, 50);
            });

            //Change Color
            $('#colorPanel').ColorPanel({
                styleSheet: '#DefaultCSS',
                animateContainer: '#wrapper',
                colors: {
                    '#00a12a': 'css/skin1.css?v=12',
                    '#ff4444': 'css/skin2.css?v=10',
                    '#4285F4': 'css/skin3.css?v=4'
                }
            });
            
            // Apply saved colors after all initialization
            setTimeout(function() {
                if (typeof applySavedColors === 'function') {
                    applySavedColors();
                }
            }, 300);
            
            // Set up MutationObserver to reapply colors when DOM changes
            if (typeof MutationObserver !== 'undefined') {
                var observer = new MutationObserver(function(mutations) {
                    var shouldReapply = false;
                    mutations.forEach(function(mutation) {
                        if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                            for (var i = 0; i < mutation.addedNodes.length; i++) {
                                var node = mutation.addedNodes[i];
                                if (node.nodeType === 1 && (node.classList.contains('color-output') || node.classList.contains('idcardborder'))) {
                                    shouldReapply = true;
                                    break;
                                }
                            }
                        }
                    });
                    
                    if (shouldReapply) {
                        setTimeout(function() {
                            if (typeof applySavedColors === 'function') {
                                applySavedColors();
                            }
                        }, 100);
                    }
                });
                
                observer.observe(document.getElementById('wrapper'), {
                    childList: true,
                    subtree: true
                });
            }
        });
    </script>

    <script type="text/javascript"> 
        function showMe(a) {
            $('#ExamNameTextBox').val($('[id*=ExamDropDownList] :selected').text());
        }

        // Background Color
        $(document).on("change", ".getColor", function () {
            //Get Color
            var color = $(".getColor").val();
            
            // Update window variable
            window.savedBgColor = color;
            
            // Save to localStorage
            try {
                localStorage.setItem('admitCard_bgColor_' + window.userColorKey, color);
            } catch(e) {
                console.log('LocalStorage not available');
            }
            
            //apply current color to div
            $(".color-output").css("background", color);
            $(".idcardborder").css("border-color", color);
            $(".headcolor").css("background", color);
            
            // Save color to session
            $.ajax({
                url: "Multiple_Admit_Card.aspx/SaveBackgroundColor",
                data: JSON.stringify({ 'color': color }),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    console.log("Background color saved: " + color);
                },
                error: function (xhr) {
                    var err = JSON.parse(xhr.responseText);
                    console.log("Error saving background color: " + err.message);
                }
            });
        });

        //  Font Color
        $(document).on("change", ".getfontColor", function () {
            //Get Color
            var color = $(".getfontColor").val();
            
            // Update window variable
            window.savedFontColor = color;
            
            // Save to localStorage
            try {
                localStorage.setItem('admitCard_fontColor_' + window.userColorKey, color);
            } catch(e) {
                console.log('LocalStorage not available');
            }
            
            //apply current color to font
            $(".color-output").css("color", color);
            
            // Save font color to session
            $.ajax({
                url: "Multiple_Admit_Card.aspx/SaveFontColor",
                data: JSON.stringify({ 'color': color }),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    console.log("Font color saved: " + color);
                },
                error: function (xhr) {
                    var err = JSON.parse(xhr.responseText);
                    console.log("Error saving font color: " + err.message);
                }
            });
        });

        // Reset Colors functionality
        $(document).on("click", "#resetColorsBtn", function (e) {
            e.preventDefault();
            
            // Update window variables
            window.savedBgColor = "#0075d2";
            window.savedFontColor = "#ffffff";
            
            // Clear localStorage
            try {
                localStorage.removeItem('admitCard_bgColor_' + window.userColorKey);
                localStorage.removeItem('admitCard_fontColor_' + window.userColorKey);
            } catch(e) {
                console.log('LocalStorage not available');
            }
            
            // Reset colors to default
            $(".getColor").val("#0075d2");
            $(".getfontColor").val("#ffffff");
            
            // Apply default colors
            $(".color-output").css("background", "#0075d2");
            $(".idcardborder").css("border-color", "#0075d2");
            $(".headcolor").css("background", "#0075d2");
            $(".color-output").css("color", "#ffffff");
            
            // Clear session colors
            $.ajax({
                url: "Multiple_Admit_Card.aspx/ResetColors",
                data: JSON.stringify({}),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    console.log("Colors reset successfully");
                    alert("Colors have been reset to default!");
                },
                error: function (xhr) {
                    var err = JSON.parse(xhr.responseText);
                    console.log("Error resetting colors: " + err.message);
                }
            });
        });
    </script>

</asp:Content>
