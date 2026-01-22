<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SocietyManagement.Admin.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .dashboard-header {
            margin-bottom: 32px;
        }

        .dashboard-header h1 {
            font-size: 32px;
            font-weight: 700;
            color: #0f172a;
            margin-bottom: 8px;
        }

        .dashboard-header p {
            color: #64748b;
            font-size: 15px;
        }

        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 24px;
            margin-bottom: 32px;
        }

        .stat-card {
            background: white;
            border-radius: 16px;
            padding: 24px;
            border: 1px solid #e2e8f0;
            transition: all 0.3s;
            position: relative;
            overflow: hidden;
        }

        .stat-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            width: 4px;
            height: 100%;
            background: var(--accent-color);
        }

        .stat-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 12px 24px rgba(0, 0, 0, 0.1);
        }

        .stat-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-bottom: 16px;
        }

        .stat-title {
            font-size: 14px;
            font-weight: 600;
            color: #64748b;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .stat-icon {
            width: 48px;
            height: 48px;
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 20px;
            color: white;
            background: var(--accent-color);
        }

        .stat-value {
            font-size: 36px;
            font-weight: 700;
            color: #0f172a;
            line-height: 1;
        }

        .stat-footer {
            margin-top: 12px;
            font-size: 13px;
            color: #64748b;
        }

        /* Color variations */
        .stat-card.primary { --accent-color: #3b82f6; }
        .stat-card.success { --accent-color: #10b981; }
        .stat-card.warning { --accent-color: #f59e0b; }
        .stat-card.danger { --accent-color: #ef4444; }
        .stat-card.info { --accent-color: #06b6d4; }
        .stat-card.purple { --accent-color: #8b5cf6; }
        .stat-card.pink { --accent-color: #ec4899; }
        .stat-card.indigo { --accent-color: #6366f1; }

        /* Quick Actions */
        .quick-actions {
            background: white;
            border-radius: 16px;
            padding: 24px;
            border: 1px solid #e2e8f0;
            margin-bottom: 24px;
        }

        .section-title {
            font-size: 18px;
            font-weight: 700;
            color: #0f172a;
            margin-bottom: 20px;
        }

        .action-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 16px;
        }

        .action-btn {
            display: flex;
            align-items: center;
            gap: 12px;
            padding: 16px;
            background: #f8fafc;
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            text-decoration: none;
            color: #0f172a;
            transition: all 0.2s;
            font-weight: 500;
        }

        .action-btn:hover {
            background: #f1f5f9;
            transform: translateX(4px);
            color: #3b82f6;
        }

        .action-btn i {
            width: 40px;
            height: 40px;
            background: white;
            border-radius: 10px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #3b82f6;
            font-size: 18px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="dashboard-header">
        <h1>Dashboard</h1>
        <p>Welcome back! Here's what's happening with your society today.</p>
    </div>

    <!-- Stats Grid -->
    <div class="stats-grid">
        <div class="stat-card primary">
            <div class="stat-header">
                <span class="stat-title">Total Flats</span>
                <div class="stat-icon">
                    <i class="fas fa-home"></i>
                </div>
            </div>
            <div class="stat-value">
                <asp:Label ID="lblTotalFlats" runat="server" Text="0"></asp:Label>
            </div>
            <div class="stat-footer">
                <i class="fas fa-info-circle"></i> All registered units
            </div>
        </div>

        <div class="stat-card success">
            <div class="stat-header">
                <span class="stat-title">Total Bills</span>
                <div class="stat-icon">
                    <i class="fas fa-file-invoice"></i>
                </div>
            </div>
            <div class="stat-value">
                <asp:Label ID="lblTotalBills" runat="server" Text="0"></asp:Label>
            </div>
            <div class="stat-footer">
                <i class="fas fa-info-circle"></i> Generated this month
            </div>
        </div>

        <div class="stat-card warning">
            <div class="stat-header">
                <span class="stat-title">Total Allotment</span>
                <div class="stat-icon">
                    <i class="fas fa-users"></i>
                </div>
            </div>
            <div class="stat-value">
                <asp:Label ID="lblTotalAllotment" runat="server" Text="0"></asp:Label>
            </div>
            <div class="stat-footer">
                <i class="fas fa-info-circle"></i> Active members
            </div>
        </div>

        <div class="stat-card danger">
            <div class="stat-header">
                <span class="stat-title">In-Process</span>
                <div class="stat-icon">
                    <i class="fas fa-hourglass-half"></i>
                </div>
            </div>
            <div class="stat-value">
                <asp:Label ID="lblInProcessComplaints" runat="server" Text="0"></asp:Label>
            </div>
            <div class="stat-footer">
                <i class="fas fa-info-circle"></i> Complaints pending
            </div>
        </div>

        <div class="stat-card info">
            <div class="stat-header">
                <span class="stat-title">Total Visitors</span>
                <div class="stat-icon">
                    <i class="fas fa-user-friends"></i>
                </div>
            </div>
            <div class="stat-value">
                <asp:Label ID="lblTotalVisitors" runat="server" Text="0"></asp:Label>
            </div>
            <div class="stat-footer">
                <i class="fas fa-info-circle"></i> This month
            </div>
        </div>

        <div class="stat-card purple">
            <div class="stat-header">
                <span class="stat-title">Unresolved</span>
                <div class="stat-icon">
                    <i class="fas fa-exclamation-triangle"></i>
                </div>
            </div>
            <div class="stat-value">
                <asp:Label ID="lblUnresolvedComplaints" runat="server" Text="0"></asp:Label>
            </div>
            <div class="stat-footer">
                <i class="fas fa-info-circle"></i> Requires attention
            </div>
        </div>

        <div class="stat-card success">
            <div class="stat-header">
                <span class="stat-title">Resolved</span>
                <div class="stat-icon">
                    <i class="fas fa-check-circle"></i>
                </div>
            </div>
            <div class="stat-value">
                <asp:Label ID="lblResolvedComplaints" runat="server" Text="0"></asp:Label>
            </div>
            <div class="stat-footer">
                <i class="fas fa-info-circle"></i> Successfully closed
            </div>
        </div>

        <div class="stat-card indigo">
            <div class="stat-header">
                <span class="stat-title">Total Complaints</span>
                <div class="stat-icon">
                    <i class="fas fa-clipboard-list"></i>
                </div>
            </div>
            <div class="stat-value">
                <asp:Label ID="lblTotalComplaints" runat="server" Text="0"></asp:Label>
            </div>
            <div class="stat-footer">
                <i class="fas fa-info-circle"></i> All time
            </div>
        </div>
    </div>

    <!-- Quick Actions -->
    <div class="quick-actions">
        <h2 class="section-title">Quick Actions</h2>
        <div class="action-grid">
            <a href="~/Admin/Member/AddMember.aspx" runat="server" class="action-btn">
                <i class="fas fa-user-plus"></i>
                <span>Add Member</span>
            </a>
            <a href="~/Admin/Finance/MaintenanceBill.aspx" runat="server" class="action-btn">
                <i class="fas fa-file-invoice-dollar"></i>
                <span>Generate Bill</span>
            </a>
            <a href="~/Admin/Complaints/AddComplaints.aspx" runat="server" class="action-btn">
                <i class="fas fa-plus-circle"></i>
                <span>Add Complaint</span>
            </a>
            <a href="~/Admin/Security/VisitorsEntry.aspx" runat="server" class="action-btn">
                <i class="fas fa-sign-in-alt"></i>
                <span>Add Visitor</span>
            </a>
            <a href="~/Admin/ManageAnnouncements.aspx" runat="server" class="action-btn">
                <i class="fas fa-bullhorn"></i>
                <span>New Announcement</span>
            </a>
            <a href="~/Admin/Expenses/ManageExpenses.aspx" runat="server" class="action-btn">
                <i class="fas fa-receipt"></i>
                <span>Add Expense</span>
            </a>
        </div>
    </div>
</asp:Content>