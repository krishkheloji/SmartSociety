<%@ Page Title="My Profile" Language="C#" MasterPageFile="~/Member/Member.Master"
    AutoEventWireup="true" CodeBehind="ViewProfile.aspx.cs"
    Inherits="SocietyManagement.Member.Profile.ViewProfile" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="head">
    <style>
    /* Common Member Page Styles - Include in all member content pages */

    .page-container {
        max-width: 1200px;
        margin: 0 auto;
    }

    .page-header {
        margin-bottom: 32px;
    }

        .page-header h2,
        .page-header h3,
        .page-header h4 {
            font-size: 28px;
            font-weight: 700;
            color: #0f172a;
            margin-bottom: 8px;
            display: flex;
            align-items: center;
            gap: 12px;
        }

    .page-icon {
        width: 48px;
        height: 48px;
        background: linear-gradient(135deg, #10b981 0%, #059669 100%);
        border-radius: 12px;
        display: flex;
        align-items: center;
        justify-content: center;
        color: white;
        font-size: 20px;
    }

    /* Card Styles */
    .card,
    .form-section {
        background: white;
        border-radius: 16px;
        padding: 32px;
        border: 1px solid #e2e8f0;
        margin-bottom: 32px;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
    }

    .card-header-custom {
        font-size: 18px;
        font-weight: 700;
        color: #0f172a;
        margin-bottom: 20px;
        padding-bottom: 12px;
        border-bottom: 2px solid #e2e8f0;
    }

    /* Form Styles */
    .form-group,
    .mb-3 {
        margin-bottom: 20px;
    }

    .form-label,
    label {
        display: block;
        font-weight: 600;
        color: #0f172a;
        margin-bottom: 8px;
        font-size: 14px;
    }

    .form-control,
    .form-select,
    input[type="text"],
    input[type="email"],
    input[type="tel"],
    input[type="date"],
    input[type="datetime-local"],
    textarea,
    select {
        width: 100%;
        padding: 12px 16px;
        border: 2px solid #e2e8f0;
        border-radius: 10px;
        font-size: 14px;
        transition: all 0.2s;
        background: #f8fafc;
        color: #0f172a;
    }

        .form-control:focus,
        .form-select:focus,
        input:focus,
        textarea:focus,
        select:focus {
            outline: none;
            border-color: #10b981;
            background: white;
            box-shadow: 0 0 0 4px rgba(16, 185, 129, 0.1);
        }

    .form-control-plaintext {
        background: transparent !important;
        border: none !important;
        padding-left: 0 !important;
    }

    textarea {
        resize: vertical;
        min-height: 100px;
    }

    /* Button Styles */
    .btn {
        padding: 12px 24px;
        border-radius: 10px;
        font-weight: 600;
        font-size: 14px;
        border: none;
        cursor: pointer;
        transition: all 0.2s;
        display: inline-flex;
        align-items: center;
        gap: 8px;
    }

    .btn-primary {
        background: linear-gradient(135deg, #10b981 0%, #059669 100%);
        color: white;
    }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 16px rgba(16, 185, 129, 0.3);
        }

    .btn-success {
        background: linear-gradient(135deg, #10b981 0%, #059669 100%);
        color: white;
    }

        .btn-success:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 16px rgba(16, 185, 129, 0.3);
        }

    .btn-secondary {
        background: #f1f5f9;
        color: #64748b;
    }

        .btn-secondary:hover {
            background: #e2e8f0;
            color: #0f172a;
        }

    .btn-danger {
        background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
        color: white;
    }

        .btn-danger:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 16px rgba(239, 68, 68, 0.3);
        }

    .btn-warning {
        background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
        color: white;
    }

    .btn-info {
        background: linear-gradient(135deg, #06b6d4 0%, #0891b2 100%);
        color: white;
    }

    .btn-sm {
        padding: 8px 16px;
        font-size: 13px;
    }

    /* Table/GridView Styles */
    .table {
        width: 100%;
        background: white;
        border-radius: 12px;
        overflow: hidden;
        border: 1px solid #e2e8f0;
        margin-top: 20px;
    }

        .table thead {
            background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
            color: white;
        }

        .table th {
            padding: 16px;
            font-size: 13px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            text-align: left;
        }

        .table td {
            padding: 16px;
            border-bottom: 1px solid #e2e8f0;
            color: #0f172a;
            font-size: 14px;
        }

        .table tbody tr:hover {
            background: #f8fafc;
        }

        .table tbody tr:last-child td {
            border-bottom: none;
        }

    .table-bordered {
        border: 1px solid #e2e8f0;
    }

    .table-striped tbody tr:nth-of-type(odd) {
        background: #f8fafc;
    }

    .align-middle td {
        vertical-align: middle;
    }

    .text-center {
        text-align: center;
    }

    /* Message/Alert Styles */
    .message,
    .alert,
    .text-success,
    .text-danger,
    .text-warning,
    .text-info {
        padding: 16px 20px;
        border-radius: 10px;
        margin-bottom: 20px;
        font-size: 14px;
        font-weight: 500;
    }

        .text-success,
        .message.success {
            background: #dcfce7;
            color: #166534;
            border-left: 4px solid #10b981;
        }

        .text-danger,
        .message.error {
            background: #fee2e2;
            color: #991b1b;
            border-left: 4px solid #ef4444;
        }

    .text-warning {
        background: #fef3c7;
        color: #92400e;
        border-left: 4px solid #f59e0b;
    }

    .text-info {
        background: #dbeafe;
        color: #1e40af;
        border-left: 4px solid #3b82f6;
    }

    .text-muted {
        color: #64748b;
    }

    /* Status Badges */
    .status-paid,
    .status-resolved,
    .status-approved {
        color: #166534;
        font-weight: 600;
        padding: 4px 12px;
        background: #dcfce7;
        border-radius: 6px;
        display: inline-block;
    }

    .status-unpaid,
    .status-pending,
    .status-open {
        color: #92400e;
        font-weight: 600;
        padding: 4px 12px;
        background: #fef3c7;
        border-radius: 6px;
        display: inline-block;
    }

    .status-rejected,
    .status-closed {
        color: #991b1b;
        font-weight: 600;
        padding: 4px 12px;
        background: #fee2e2;
        border-radius: 6px;
        display: inline-block;
    }

    .status-inprogress {
        color: #1e40af;
        font-weight: 600;
        padding: 4px 12px;
        background: #dbeafe;
        border-radius: 6px;
        display: inline-block;
    }

    /* Special Boxes */
    .bill-details,
    .detail-box {
        background: white;
        border-radius: 16px;
        padding: 24px;
        border: 1px solid #e2e8f0;
        margin: 20px 0;
    }

        .bill-details td,
        .detail-box td {
            padding: 12px 8px;
        }

        .bill-details b,
        .detail-box b {
            color: #0f172a;
            font-weight: 600;
        }

    /* Comment Box */
    .comment-box {
        background: #f8fafc;
        padding: 16px;
        border-radius: 10px;
        border-left: 3px solid #10b981;
        margin-bottom: 12px;
    }

    .comment-author {
        font-weight: 600;
        color: #10b981;
        margin-bottom: 4px;
    }

    .comment-date {
        font-size: 12px;
        color: #64748b;
        margin-bottom: 8px;
    }

    .comment-text {
        color: #0f172a;
    }

    /* Announcement Card */
    .announcement-card {
        background: white;
        border-radius: 16px;
        padding: 24px;
        border: 1px solid #e2e8f0;
        margin-bottom: 20px;
        transition: all 0.3s;
        border-left: 4px solid #10b981;
    }

        .announcement-card:hover {
            transform: translateY(-4px);
            box-shadow: 0 12px 24px rgba(0, 0, 0, 0.1);
        }

        .announcement-card h3 {
            color: #10b981;
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 12px;
        }

        .announcement-card p {
            color: #0f172a;
            margin-bottom: 12px;
            line-height: 1.6;
        }

    .announcement-date {
        color: #64748b;
        font-size: 13px;
    }

    /* Activity Item */
    .activity-item {
        padding: 16px;
        background: #f8fafc;
        border-left: 3px solid #10b981;
        border-radius: 8px;
        margin-bottom: 12px;
    }

    .activity-title {
        font-weight: 600;
        color: #0f172a;
        margin-bottom: 4px;
    }

    .activity-date {
        font-size: 13px;
        color: #64748b;
    }

    /* Empty State */
    .empty-state {
        text-align: center;
        padding: 60px 20px;
        color: #64748b;
    }

        .empty-state i {
            font-size: 48px;
            margin-bottom: 16px;
            opacity: 0.5;
        }

    /* Success/Error Boxes */
    .success-box {
        max-width: 600px;
        margin: 80px auto;
        text-align: center;
        background: white;
        border-radius: 16px;
        padding: 40px;
        box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
    }

    .success-icon {
        font-size: 60px;
        color: #10b981;
        margin-bottom: 20px;
    }

    .success-text {
        font-size: 24px;
        color: #10b981;
        font-weight: 700;
        margin-bottom: 12px;
    }

    /* Responsive */
    @media (max-width: 768px) {
        .page-header h2,
        .page-header h3,
        .page-header h4 {
            font-size: 22px;
        }

        .card,
        .form-section {
            padding: 20px;
        }

        .btn {
            width: 100%;
            justify-content: center;
        }

        .table {
            font-size: 12px;
        }

            .table th,
            .table td {
                padding: 12px 8px;
            }
    }

    /* Loading Spinner */
    .spinner {
        width: 40px;
        height: 40px;
        border: 4px solid #e2e8f0;
        border-top-color: #10b981;
        border-radius: 50%;
        animation: spin 1s linear infinite;
        margin: 20px auto;
    }

    @keyframes spin {
        to {
            transform: rotate(360deg);
        }
    }
</style>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="ContentPlaceHolder1">

    <asp:HiddenField ID="hfMemberId" runat="server" />

    <h2 class="mb-4">My Profile</h2>
    <asp:Label ID="lblMessage" runat="server" CssClass="text-success fw-bold"></asp:Label>

    <!-- View Panel -->
    <asp:Panel ID="pnlView" runat="server">
        <table class="table table-bordered">
            <tr>
                <th>Username</th>
                <td><asp:Label ID="lblUsername" runat="server" /></td>
            </tr>
            <tr>
                <th>Full Name</th>
                <td><asp:TextBox ID="txtFullName" runat="server" ReadOnly="true" CssClass="form-control-plaintext" /></td>
            </tr>
            <tr>
                <th>Email</th>
                <td><asp:TextBox ID="txtEmail" runat="server" ReadOnly="true" CssClass="form-control-plaintext" /></td>
            </tr>
            <tr>
                <th>Phone</th>
                <td><asp:TextBox ID="txtPhone" runat="server" ReadOnly="true" CssClass="form-control-plaintext" /></td>
            </tr>
            <tr>
                <th>Status</th>
                <td><asp:Label ID="lblStatus" runat="server" /></td>
            </tr>
        </table>

        <asp:Button ID="btnEdit" runat="server" Text="Edit Profile"
            CssClass="btn btn-primary" OnClick="btnEdit_Click" />
    </asp:Panel>

    <!-- Edit Panel -->
    <asp:Panel ID="pnlEdit" runat="server" Visible="false">
        <div class="mb-3">
            <label>Full Name</label>
            <asp:TextBox ID="txtEditFullName" runat="server" CssClass="form-control" />
        </div>
        <div class="mb-3">
            <label>Email</label>
            <asp:TextBox ID="txtEditEmail" runat="server" CssClass="form-control" TextMode="Email" />
        </div>
        <div class="mb-3">
            <label>Phone</label>
            <asp:TextBox ID="txtEditPhone" runat="server" CssClass="form-control" />
        </div>

        <asp:Button ID="btnUpdate" runat="server" Text="Update"
            CssClass="btn btn-success me-2" OnClick="btnUpdate_Click" />
        <asp:Button ID="btnCancel" runat="server" Text="Cancel"
            CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
    </asp:Panel>

</asp:Content>
