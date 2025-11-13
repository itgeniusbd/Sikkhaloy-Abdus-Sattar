<%@ Page Title="Manage Word of the Day" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Manage_WordOfDay.aspx.cs" Inherits="EDUCATION.COM.Profile.Manage_WordOfDay" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .word-form-card {
       background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border-radius: 10px;
   padding: 20px;
  box-shadow: 0 4px 15px rgba(0,0,0,0.2);
  }
      
 .word-form-card .form-control {
      border-radius: 6px;
       border: 2px solid #ddd;
     }
        
        .word-form-card label {
            font-weight: 600;
   color: #fff;
        }
        
  .btn-add-word {
     background: #ffd700;
            color: #333;
 font-weight: 600;
       border: none;
      padding: 10px 30px;
        }
    
        .btn-add-word:hover {
          background: #ffed4e;
      transform: scale(1.05);
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3><i class="fa fa-book"></i> Manage Word of the Day</h3>
    
    <!-- Add New Word Form -->
    <div class="card mb-4 word-form-card">
     <h5 class="mb-3"><i class="fa fa-plus-circle"></i> Add New Word</h5>
        
     <div class="row">
            <div class="col-md-6">
      <div class="form-group">
             <label>English Word *</label>
     <asp:TextBox ID="EnglishWordTextBox" runat="server" CssClass="form-control" placeholder="e.g., Achievement" MaxLength="100" />
  <asp:RequiredFieldValidator ID="rfvEnglishWord" runat="server" ControlToValidate="EnglishWordTextBox" 
             ErrorMessage="English word is required" CssClass="text-warning" Display="Dynamic" ValidationGroup="AddWord" />
         </div>
            </div>
    
            <div class="col-md-6">
           <div class="form-group">
   <label>Pronunciation</label>
         <asp:TextBox ID="PronunciationTextBox" runat="server" CssClass="form-control" placeholder="e.g., uh-CHEEV-muhnt" MaxLength="100" />
 </div>
            </div>
        </div>
  
        <div class="row">
<div class="col-md-6">
       <div class="form-group">
  <label>Bengali Meaning *</label>
               <asp:TextBox ID="BengaliMeaningTextBox" runat="server" CssClass="form-control" placeholder="e.g., ?????, ??????" MaxLength="200" />
        <asp:RequiredFieldValidator ID="rfvBengaliMeaning" runat="server" ControlToValidate="BengaliMeaningTextBox" 
         ErrorMessage="Bengali meaning is required" CssClass="text-warning" Display="Dynamic" ValidationGroup="AddWord" />
         </div>
         </div>
            
            <div class="col-md-6">
        <div class="form-group">
   <label>Part of Speech</label>
        <asp:DropDownList ID="PartOfSpeechDropDown" runat="server" CssClass="form-control">
     <asp:ListItem Value="">[ Select ]</asp:ListItem>
 <asp:ListItem Value="Noun">Noun (???????)</asp:ListItem>
    <asp:ListItem Value="Verb">Verb (???????)</asp:ListItem>
             <asp:ListItem Value="Adjective">Adjective (??????)</asp:ListItem>
               <asp:ListItem Value="Adverb">Adverb (?????????????)</asp:ListItem>
        <asp:ListItem Value="Pronoun">Pronoun (???????)</asp:ListItem>
     <asp:ListItem Value="Preposition">Preposition (????????? ??????)</asp:ListItem>
       <asp:ListItem Value="Conjunction">Conjunction (??????)</asp:ListItem>
        </asp:DropDownList>
        </div>
            </div>
        </div>
        
        <div class="form-group">
    <label>Example Sentence</label>
     <asp:TextBox ID="ExampleSentenceTextBox" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" 
   placeholder="e.g., His achievement in the field of science was remarkable." MaxLength="500" />
        </div>
        
        <div class="form-group">
  <asp:Button ID="AddWordButton" runat="server" Text="Add Word" CssClass="btn btn-add-word" 
         OnClick="AddWordButton_Click" ValidationGroup="AddWord" />
            <asp:Label ID="MessageLabel" runat="server" CssClass="ml-3" />
        </div>
    </div>
    
    <!-- Words List -->
    <div class="card">
        <div class="card-header bg-primary text-white">
      <h5 class="mb-0"><i class="fa fa-list"></i> All Words</h5>
        </div>
        <div class="card-body">
<asp:GridView ID="WordsGridView" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-hover"
      DataKeyNames="WordID" DataSourceID="WordsSQL" AllowPaging="True" PageSize="20">
      <Columns>
               <asp:BoundField DataField="WordID" HeaderText="ID" ReadOnly="True" SortExpression="WordID" />
     <asp:BoundField DataField="EnglishWord" HeaderText="English Word" SortExpression="EnglishWord" />
         <asp:BoundField DataField="BengaliMeaning" HeaderText="Bengali Meaning" SortExpression="BengaliMeaning" />
             <asp:BoundField DataField="PartOfSpeech" HeaderText="Part of Speech" SortExpression="PartOfSpeech" />
        <asp:BoundField DataField="Pronunciation" HeaderText="Pronunciation" SortExpression="Pronunciation" />
    <asp:CheckBoxField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
         <asp:TemplateField HeaderText="Actions">
         <ItemTemplate>
         <asp:LinkButton ID="DeleteButton" runat="server" CssClass="btn btn-sm btn-danger" 
    CommandName="Delete" OnClientClick="return confirm('Are you sure you want to delete this word?');">
       <i class="fa fa-trash"></i> Delete
   </asp:LinkButton>
           </ItemTemplate>
         </asp:TemplateField>
       </Columns>
                <PagerStyle CssClass="pagination" HorizontalAlign="Center" />
            </asp:GridView>
            
     <asp:SqlDataSource ID="WordsSQL" runat="server" 
     ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
         SelectCommand="SELECT [WordID], [EnglishWord], [BengaliMeaning], [PartOfSpeech], [Pronunciation], [IsActive] FROM [WordOfTheDay] ORDER BY [CreatedDate] DESC"
     DeleteCommand="DELETE FROM [WordOfTheDay] WHERE [WordID] = @WordID">
                <DeleteParameters>
   <asp:Parameter Name="WordID" Type="Int32" />
       </DeleteParameters>
     </asp:SqlDataSource>
  </div>
    </div>
</asp:Content>
