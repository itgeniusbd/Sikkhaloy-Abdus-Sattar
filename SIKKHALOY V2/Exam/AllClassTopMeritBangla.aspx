<%@ Page Title="All Class Top Merit" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="AllClassTopMeritBangla.aspx.cs" Inherits="EDUCATION.COM.Exam.AllClassTopMeritBangla" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <style>
        body {
            font-family: 'Kalpurush', serif;
        }

        .institution-header {
            text-align: center;
            margin-bottom: 12px;
            padding: 0;
            border: none;
            background: transparent;
        }

        .institution-header .school-name {
            display: block;
            font-size: 30px;
            font-weight: bold;
            margin-bottom: 4px;
            line-height: 1.3;
        }

        .institution-header .school-address {
            display: block;
            font-size: 14px;
            color: #555;
            margin-bottom: 10px;
            line-height: 1.4;
        }

        .report-title {
            text-align: center;
            margin: 0 0 20px;
        }

        .report-title .exam-session-line {
            display: block;
            font-size: 20px;
            font-weight: bold;
            margin-bottom: 4px;
            line-height: 1.4;
        }

        .report-title .merit-list-line {
            display: block;
            font-size: 18px;
            font-weight: bold;
            line-height: 1.4;
        }

        .view-mode-box {
            background: #f8f9fa;
            border: 1px solid #dee2e6;
            border-radius: 6px;
            padding: 8px 14px;
        }

        .view-mode-box label {
            margin-right: 16px;
            font-size: 14px;
            cursor: pointer;
        }

        .merit-board {
            display: flex;
            flex-direction: column;
            gap: 20px;
        }

        .class-merit-card {
            width: 100%;
            border: 1px solid #cfd4da;
            border-radius: 8px;
            overflow: hidden;
            background: #fff;
            box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
            page-break-inside: avoid;
        }

        .class-merit-header {
            background: #4B515D;
            color: #fff;
            text-align: center;
            padding: 10px 12px;
            font-size: 20px;
            font-weight: bold;
        }

        .merit-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 16px;
            table-layout: fixed;
        }

        .merit-table th {
            background: #f1f3f5;
            color: #333;
            border: 1px solid #dee2e6;
            padding: 9px 6px;
            text-align: center;
            font-weight: bold;
            font-size: 15px;
        }

        .merit-table td {
            border: 1px solid #dee2e6;
            padding: 9px 6px;
            text-align: center;
            font-weight: 400;
            color: #444;
            font-size: 16px;
        }

        .merit-table td.name-col,
        .merit-table th.name-col,
        .merit-table td.father-col,
        .merit-table th.father-col {
            text-align: left;
            white-space: normal;
            word-wrap: break-word;
            overflow-wrap: break-word;
            word-break: break-word;
            overflow: hidden;
            vertical-align: middle;
            max-width: 0;
            line-height: 1.35;
        }

        .merit-table td.id-col,
        .merit-table th.id-col,
        .merit-table td.name-col,
        .merit-table th.name-col {
            font-weight: 800 !important;
            color: #000 !important;
            font-size: 17px !important;
        }

        .merit-table td.id-col strong,
        .merit-table td.name-col strong {
            font-weight: 800;
        }

        .merit-table col.col-merit { width: 4%; }
        .merit-table col.col-id { width: 5%; }
        .merit-table col.col-roll { width: 5%; }
        .merit-table col.col-name { width: 25%; }
        .merit-table col.col-father { width: 25%; }
        .merit-table col.col-total { width: 6%; }
        .merit-table col.col-avg { width: 5%; }
        .merit-table col.col-grade { width: 14%; }
        .merit-table col.col-point { width: 5%; }

        .pos-first td:first-child,
        .pos-second td:first-child,
        .pos-third td:first-child {
            font-weight: bold;
        }

        .pos-first td:first-child {
            background-color: #28a745 !important;
            color: #fff !important;
        }

        .pos-second td:first-child {
            background-color: #1e90ff !important;
            color: #fff !important;
        }

        .pos-third td:first-child {
            background-color: #ff8c00 !important;
            color: #fff !important;
        }

        .empty-msg {
            text-align: center;
            padding: 30px;
            color: #666;
            font-size: 16px;
        }

        @media print {
            @page {
                margin: 6mm 5mm;
                size: A4 portrait;
            }

            .NoPrint,
            #sidedrawer,
            #header,
            #footer,
            .AdminNotice {
                display: none !important;
            }

            body,
            #content-wrapper,
            #main-content,
            .container-fluid,
            .container,
            .Exam_Position,
            #ExportPanel {
                margin: 0 !important;
                padding: 0 !important;
                width: 100% !important;
                max-width: 100% !important;
            }

            .merit-print-area,
            .institution-header,
            .report-title {
                width: 100% !important;
                max-width: 100% !important;
            }

            .institution-header {
                margin-bottom: 4px;
            }

            .institution-header .school-name {
                font-size: 26px;
                margin-bottom: 2px;
                line-height: 1.2;
            }

            .institution-header .school-address {
                font-size: 15px;
                margin-bottom: 4px;
                line-height: 1.2;
            }

            .report-title {
                page-break-after: avoid;
                margin-bottom: 8px;
            }

            .report-title .exam-session-line {
                font-size: 19px;
                margin-bottom: 2px;
                line-height: 1.2;
            }

            .report-title .merit-list-line {
                font-size: 17px;
                line-height: 1.2;
            }

            .merit-board {
                display: block;
                width: 100% !important;
            }

            .class-merit-card {
                width: 100% !important;
                max-width: 100% !important;
                margin: 0 0 5px;
                padding: 0;
                box-sizing: border-box;
                border: 1px solid #333;
                border-radius: 0;
                box-shadow: none;
                page-break-inside: avoid;
                break-inside: avoid;
            }

            .class-merit-header {
                font-size: 17px;
                padding: 5px 8px;
                margin-bottom: 0;
                line-height: 1.25;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }

            .merit-table {
                width: 100% !important;
                table-layout: fixed;
                font-size: 14px;
            }

            .merit-table col.col-name {
                width: 25%;
            }

            .merit-table col.col-father {
                width: 25%;
            }

            .merit-table col.col-grade {
                width: 12%;
            }

            .merit-table td.name-col,
            .merit-table th.name-col,
            .merit-table td.father-col,
            .merit-table th.father-col {
                white-space: normal;
                word-wrap: break-word;
                overflow-wrap: break-word;
                word-break: break-word;
                overflow: hidden;
                max-width: 0;
                line-height: 1.35;
                vertical-align: middle;
            }

            .merit-table td.id-col,
            .merit-table th.id-col,
            .merit-table td.name-col,
            .merit-table th.name-col {
                font-weight: 800 !important;
                color: #000 !important;
                font-size: 15px !important;
            }

            .merit-table th {
                font-size: 14px;
                padding: 5px 4px;
                line-height: 1.25;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }

            .merit-table td {
                font-size: 14px;
                padding: 5px 4px;
                line-height: 1.25;
                font-weight: 400;
                color: #444;
            }

            .pos-first td:first-child,
            .pos-second td:first-child,
            .pos-third td:first-child {
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
        }

    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="NoPrint mb-2">
        <a href="ExmamPositionBangla.aspx">শ্রেণি ভিত্তিক মেধা তালিকা &lt;&lt;&lt;</a>
    </div>

    <div class="form-inline NoPrint mb-3">
        <div class="form-group mr-2 mb-2">
            <asp:DropDownList ID="EduYearDropDownList" runat="server" CssClass="form-control" AutoPostBack="True"
                DataSourceID="EduYearSQL" DataTextField="EducationYear" DataValueField="EducationYearID"
                OnSelectedIndexChanged="EduYearDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="EduYearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT EducationYearID, EducationYear FROM Education_Year WHERE (SchoolID = @SchoolID) ORDER BY EducationYearID DESC">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group mr-2 mb-2">
            <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" AutoPostBack="True"
                DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID"
                OnDataBound="ExamDropDownList_DataBound" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID WHERE (Exam_Name.SchoolID = @SchoolID) AND (Exam_Name.EducationYearID = @EducationYearID) ORDER BY Exam_Name.ExamID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="EduYearDropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group mr-2 mb-2 view-mode-box">
            <strong style="margin-right:8px;">মেধা দেখান:</strong>
            <asp:RadioButtonList ID="MeritViewRadioButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="True"
                OnSelectedIndexChanged="MeritViewRadioButtonList_SelectedIndexChanged">
                <asp:ListItem Value="Class" Selected="True">ক্লাশ ওয়াইজ</asp:ListItem>
                <asp:ListItem Value="Section">শাখা/গ্রুপ ওয়াইজ</asp:ListItem>
            </asp:RadioButtonList>
        </div>
        <div class="form-group mb-2">
            <button type="button" class="btn btn-primary" onclick="window.print()">
                <i class="fa fa-print" aria-hidden="true"></i> Print
            </button>
        </div>
    </div>

    <div class="institution-header">
        <asp:Label ID="SchoolNameLabel" runat="server" CssClass="school-name"></asp:Label>
        <asp:Label ID="SchoolAddressLabel" runat="server" CssClass="school-address"></asp:Label>
    </div>

    <div class="report-title">
        <asp:Label ID="ExamSessionLabel" runat="server" CssClass="exam-session-line"></asp:Label>
        <asp:Label ID="MeritListLabel" runat="server" CssClass="merit-list-line"></asp:Label>
    </div>

    <asp:Panel ID="ResultsPanel" runat="server" Visible="false" CssClass="merit-print-area">
        <div class="merit-board">
            <asp:Repeater ID="ClassMeritRepeater" runat="server" OnItemDataBound="ClassMeritRepeater_ItemDataBound">
                <ItemTemplate>
                    <div class="class-merit-card">
                        <div class="class-merit-header"><%# Eval("GroupTitle") %></div>
                        <table class="merit-table">
                            <colgroup>
                                <col class="col-merit" />
                                <col class="col-id" />
                                <col class="col-roll" />
                                <col class="col-name" />
                                <col class="col-father" />
                                <col class="col-total" />
                                <col class="col-avg" />
                                <col class="col-grade" />
                                <col class="col-point" />
                            </colgroup>
                            <thead>
                                <tr>
                                    <th>মেধা</th>
                                    <th class="id-col">আইডি</th>
                                    <th>রোল</th>
                                    <th class="name-col">নাম</th>
                                    <th class="father-col">বাবার নাম</th>
                                    <th>মোট</th>
                                    <th>গড়</th>
                                    <th>গ্রেড</th>
                                    <th>পয়েন্ট</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="StudentMeritRepeater" runat="server">
                                    <ItemTemplate>
                                        <tr class='<%# Eval("PositionCss") %>'>
                                            <td><%# Eval("MeritText") %></td>
                                            <td class="id-col"><strong><%# Eval("StudentID") %></strong></td>
                                            <td><%# Eval("RollNo") %></td>
                                            <td class="name-col"><strong><%# Eval("StudentsName") %></strong></td>
                                            <td class="father-col"><%# Eval("FathersName") %></td>
                                            <td><%# Eval("TotalMark") %></td>
                                            <td><%# Eval("Average") %></td>
                                            <td><%# Eval("Grade") %></td>
                                            <td><%# Eval("Point") %></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </asp:Panel>

    <asp:Label ID="EmptyLabel" runat="server" CssClass="empty-msg" Visible="false"
        Text="শিক্ষাবর্ষ ও পরীক্ষা নির্বাচন করুন অথবা এই পরীক্ষায় কোনো মেধা তালিকা পাওয়া যায়নি।"></asp:Label>
</asp:Content>
