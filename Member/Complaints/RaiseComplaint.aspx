<%@ Page Title="" Language="C#" MasterPageFile="~/Member/Member.Master" AutoEventWireup="true" CodeBehind="RaiseComplaint.aspx.cs" Inherits="SocietyManagement.Member.Complaints.RaiseComplaint" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .complaint-form {
            max-width: 600px;
            margin: 40px auto;
            padding: 25px;
            background: #fff;
            border-radius: 10px;
            box-shadow: 0 3px 8px rgba(0,0,0,0.1);
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">


    <div class="complaint-form">
        <h4 class="text-center text-primary mb-4">Raise a New Complaint</h4>

        <div class="mb-3">
            <label class="form-label">Complaint Title</label>
            <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" placeholder="Enter complaint title"></asp:TextBox>
        </div>

        <div class="form-group">
    <label>Category:</label>
    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-control">
        <asp:ListItem Text="-- Select Category --" Value=""></asp:ListItem>
        <asp:ListItem Text="Plumbing" Value="Plumbing"></asp:ListItem>
        <asp:ListItem Text="Electricity" Value="Electricity"></asp:ListItem>
        <asp:ListItem Text="Security" Value="Security"></asp:ListItem>
        <asp:ListItem Text="Cleanliness" Value="Cleanliness"></asp:ListItem>
        <asp:ListItem Text="Other" Value="Other"></asp:ListItem>
    </asp:DropDownList>
</div>

        <div class="mb-3">
            <label class="form-label">Complaint Description</label>
            <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" placeholder="Describe your issue..."></asp:TextBox>
        </div>

        <div class="text-center">
            <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary px-4" Text="Submit Complaint" OnClick="btnSubmit_Click" />
        </div>
    </div>
</asp:Content>

