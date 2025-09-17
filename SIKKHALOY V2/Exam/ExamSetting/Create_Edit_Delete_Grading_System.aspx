<%@ Page Title="Grading System" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Create_Edit_Delete_Grading_System.aspx.cs" Inherits="EDUCATION.COM.EXAM.ExamSetting.Create_Edit_Delete_Grading_System" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Create_Edit_Delete_Exam.css?v=3" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Grading System</h3>
    <button type="button" class="btn btn-primary mb-2" data-toggle="modal" data-target="#GS_Modal">Add New Grading System</button>

    <asp:Repeater ID="Grading_Repeater" runat="server" DataSourceID="GradingNameSQL">
        <ItemTemplate>
            <div class="card mb-4">
                <div class="card-header">
                    <h5 class="m-0 edit"><%#Eval("GradeName") %> <i class="fa fa-edit" style="cursor:pointer"></i> <small><asp:LinkButton ID="DeleteButton" runat="server" Text="Delete" CommandName='<%# Eval("GradeNameID") %>' OnCommand="DeleteButton_Command" OnClientClick="return confirm('Are you sure want to delete?')" /></small></h5>
                    <div class="Update" style="display:none;">
                        <asp:TextBox ID="GradeName_TextBox" CssClass="form-control GN" runat="server" Text='<%#Eval("GradeName") %>'></asp:TextBox>
                        <asp:LinkButton ID="UpdateLinkButton" runat="server" Text="Update" CommandName='<%# Eval("GradeNameID") %>' OnCommand="UpdateLinkButton_Command" />
                    </div>
                </div>

                <div class="card-body">
                    <asp:HiddenField ID="GradeNameID_HF" Value='<%# Eval("GradeNameID") %>' runat="server" />
                    <asp:GridView ID="Grading_System_GridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="GradingID" DataSourceID="GradingSystemSQL">
                        <Columns>
                            <asp:BoundField DataField="MaxPercentage" HeaderText="Max %" SortExpression="MaxPercentage" ReadOnly="True" />
                            <asp:BoundField DataField="MinPercentage" HeaderText="Min %" SortExpression="MinPercentage" ReadOnly="True" />
                            <asp:BoundField DataField="Grades" HeaderText="Grades" SortExpression="Grades" ReadOnly="True" />
                            <asp:BoundField DataField="Point" HeaderText="Point" SortExpression="Point" ReadOnly="True" />
                            <asp:TemplateField HeaderText="Comments" SortExpression="Comments">
                                <EditItemTemplate>
                                    <asp:TextBox ID="TextBox1" CssClass="form-control" runat="server" Text='<%# Bind("Comments") %>'></asp:TextBox>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("Comments") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:CommandField ShowEditButton="True" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="GradingSystemSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Exam_Grading_System] WHERE (([GradeNameID] = @GradeNameID) AND ([SchoolID] = @SchoolID))" UpdateCommand="UPDATE Exam_Grading_System SET Comments = @Comments WHERE (GradingID = @GradingID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="GradeNameID_HF" Name="GradeNameID" PropertyName="Value" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:Parameter Name="Comments" />
                            <asp:Parameter Name="GradingID" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <asp:SqlDataSource ID="GradingNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Exam_Grade_Name] WHERE ([SchoolID] = @SchoolID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="DeleteSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="IF NOT EXISTS (SELECT ExamGradeAssignID FROM Exam_Grading_Assign WHERE (GradeNameID = @GradeNameID))
BEGIN
IF NOT EXISTS (SELECT  Cumulative_SettingID FROM Exam_Cumulative_Setting WHERE (GradeNameID = @GradeNameID))
BEGIN
DELETE FROM Exam_Grading_System WHERE (GradeNameID = @GradeNameID)
DELETE FROM Exam_Grade_Name WHERE(GradeNameID = @GradeNameID)
END
END"
        SelectCommand="SELECT * FROM Exam_Name" UpdateCommand="IF NOT EXISTS (SELECT GradeName FROM Exam_Grade_Name WHERE GradeName = @GradeName AND SchoolID=@SchoolID)
UPDATE Exam_Grade_Name SET GradeName = @GradeName WHERE (GradeNameID = @GradeNameID)">
        <DeleteParameters>
            <asp:Parameter Name="GradeNameID" />
        </DeleteParameters>
        <UpdateParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:Parameter Name="GradeNameID" />
            <asp:Parameter Name="GradeName" />
        </UpdateParameters>
    </asp:SqlDataSource>

    <!-- Grading System Modal -->
    <div id="GS_Modal" class="modal fade" role="dialog">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Add New Grading System</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="Updatecard2" runat="server">
                        <ContentTemplate>
                            <div class="form-group">
                                <label>
                                    Grading For
                                    <asp:RequiredFieldValidator ControlToValidate="GradingFor_TextBox" CssClass="EroorStar" ValidationGroup="GS" ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="GradingFor_TextBox" CssClass="form-control" runat="server"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>
                                    Maximum %
                            <asp:RequiredFieldValidator ControlToValidate="Max_Marks_TextBox" CssClass="EroorStar" ValidationGroup="GS" ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="Max_Marks_TextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>
                                    Minimum %
                            <asp:RequiredFieldValidator ControlToValidate="Mini_Marks_TextBox" CssClass="EroorStar" ValidationGroup="GS" ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="Mini_Marks_TextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>
                                    Grade 
                            <asp:RequiredFieldValidator ControlToValidate="Grade_TextBox" CssClass="EroorStar" ValidationGroup="GS" ID="RequiredFieldValidator3" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="Grade_TextBox" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>
                                    Point
                            <asp:RequiredFieldValidator ControlToValidate="Point_TextBox" CssClass="EroorStar" ValidationGroup="GS" ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="Point_TextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Comment</label>
                                <asp:TextBox ID="Comment_TextBox" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <asp:Button ID="Add_Grading_Button" CssClass="btn btn-danger" ValidationGroup="GS" runat="server" Text="Add Grading System" OnClick="Add_Grading_Button_Click" />
                            </div>

                            <div class="table-responsive">
                                <asp:GridView ID="AddGrading_System_GridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid">
                                    <Columns>
                                        <asp:BoundField DataField="MaxPercentage" HeaderText="Max %" SortExpression="MaxPercentage" />
                                        <asp:BoundField DataField="MinPercentage" HeaderText="Min %" SortExpression="MinPercentage" />
                                        <asp:BoundField DataField="Grades" HeaderText="Grades" SortExpression="Grades" />
                                        <asp:BoundField DataField="Point" HeaderText="Point" SortExpression="Point" />
                                        <asp:TemplateField HeaderText="Comments" SortExpression="Comments">
                                            <ItemTemplate>
                                                <asp:Label ID="CommentsLB" runat="server" Text='<%# Bind("Comments") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField>
                                            <ItemTemplate>
                                                <asp:LinkButton ID="DeleteButton" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClick="RowDelete"></asp:LinkButton>
                                            </ItemTemplate>
                                            <ItemStyle Width="40px" />
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                                <br />
                                <%if (AddGrading_System_GridView.Rows.Count > 0)
                                    {%>
                                <asp:Button ID="Save_Grading_System_Button" runat="server" CssClass="btn btn-success" Text="Save Grading System" OnClick="Save_Grading_System_Button_Click" />
                                <asp:SqlDataSource ID="AddNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [Exam_Grade_Name] ([SchoolID], [RegistrationID], [GradeName]) VALUES (@SchoolID, @RegistrationID, @GradeName)" SelectCommand="SELECT * FROM [Exam_Grade_Name] WHERE ([SchoolID] = @SchoolID)">
                                    <InsertParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                        <asp:ControlParameter ControlID="GradingFor_TextBox" Name="GradeName" PropertyName="Text" Type="String" />
                                    </InsertParameters>
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                                <asp:SqlDataSource ID="Add_GradingSystemSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT GradingID, RegistrationID, SchoolID, EducationYearID, Grades, MaxPercentage, MinPercentage, Comments, Point, GradeNameID FROM Exam_Grading_System WHERE (SchoolID = @SchoolID)" InsertCommand="INSERT INTO Exam_Grading_System(RegistrationID, SchoolID, EducationYearID, GradeNameID,Grades, MaxPercentage, MinPercentage, Comments, Point) 
VALUES (@RegistrationID, @SchoolID, @EducationYearID,(SELECT IDENT_CURRENT ('Exam_Grade_Name')), @Grades, @MaxPercentage, @MinPercentage, @Comments, @Point)">
                                    <InsertParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                        <asp:Parameter Name="MinPercentage" />
                                        <asp:Parameter Name="MaxPercentage" />
                                        <asp:Parameter Name="Grades" />
                                        <asp:Parameter Name="Point" />
                                        <asp:Parameter Name="Comments" />
                                    </InsertParameters>
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                                <%} %>
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
                <img src="../../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script>
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };

        $(function () {
            $(".edit").on("click", function () {
                $(this).next(".Update").fadeIn(2000, "swing");
                $(this).hide();
            })
        })
    </script>
</asp:Content>
