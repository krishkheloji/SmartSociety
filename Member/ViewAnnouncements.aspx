<%@ Page Title="" Language="C#" MasterPageFile="~/Member/Member.Master" AutoEventWireup="true" CodeBehind="ViewAnnouncements.aspx.cs" Inherits="SocietyManagement.Member.ViewAnnouncements" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
        <style>
        .announcement-container {
            max-width: 800px;
            margin: 40px auto;
            padding: 15px;
        }

        .announcement-card {
            background: #fff;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            padding: 20px;
            margin-bottom: 20px;
            transition: 0.3s;
        }

        .announcement-card:hover {
            transform: translateY(-3px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        }

        .announcement-card h3 {
            color: #0078D7;
            margin-bottom: 8px;
        }

        .announcement-card p {
            color: #333;
            margin-bottom: 10px;
        }

        .announcement-date {
            color: #666;
            font-size: 0.9em;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="announcement-container">
    <h2>📢 Current Announcements</h2>

    <asp:Repeater ID="rptAnnouncements" runat="server">
        <ItemTemplate>
            <div class="announcement-card">
                <h3><%# Eval("Title") %></h3>
                <p><%# Eval("Content") %></p>
                <p class="announcement-date">
                    Visible From: <%# Eval("VisibleFrom", "{0:dd MMM yyyy}") %> |
                    To: <%# Eval("VisibleTo", "{0:dd MMM yyyy}") %>
                </p>
            </div>
        </ItemTemplate>
    </asp:Repeater>

    <asp:Label ID="lblNoAnnouncements" runat="server" Text="No announcements available." Visible="false" ForeColor="Gray"></asp:Label>
</div>


</asp:Content>

