<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="AddMember.aspx.cs" Inherits="SocietyManagement.Admin.Member.AddMember" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Common Page Styles - Use in all content pages */

        .page-header {
            margin-bottom: 32px;
        }

            .page-header h2 {
                font-size: 28px;
                font-weight: 700;
                color: #0f172a;
                margin-bottom: 8px;
            }

            .page-header p {
                color: #64748b;
                font-size: 15px;
            }

        /* Card Styles */
        .card {
            background: white;
            border-radius: 16px;
            padding: 24px;
            border: 1px solid #e2e8f0;
            margin-bottom: 24px;
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
        .form-section {
            background: white;
            border-radius: 16px;
            padding: 24px;
            border: 1px solid #e2e8f0;
            margin-bottom: 24px;
        }

        .form-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 20px;
        }

        .form-group {
            margin-bottom: 20px;
        }

            .form-group label {
                display: block;
                font-weight: 600;
                color: #0f172a;
                margin-bottom: 8px;
                font-size: 14px;
            }

        .form-control, .form-select,
        input[type="text"],
        input[type="number"],
        input[type="email"],
        input[type="tel"],
        input[type="date"],
        input[type="month"],
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
                border-color: #3b82f6;
                background: white;
                box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
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
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
        }

            .btn-primary:hover {
                transform: translateY(-2px);
                box-shadow: 0 8px 16px rgba(59, 130, 246, 0.3);
            }

        .btn-secondary {
            background: #f1f5f9;
            color: #64748b;
        }

            .btn-secondary:hover {
                background: #e2e8f0;
                color: #0f172a;
            }

        .btn-success {
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
        }

            .btn-success:hover {
                transform: translateY(-2px);
                box-shadow: 0 8px 16px rgba(16, 185, 129, 0.3);
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

        /* GridView Styles */
        .table, .gridview {
            width: 100%;
            background: white;
            border-radius: 12px;
            overflow: hidden;
            border: 1px solid #e2e8f0;
            border-collapse: separate;
            border-spacing: 0;
        }

            .table thead, .gridview th {
                background: linear-gradient(135deg, #1e293b 0%, #334155 100%);
                color: white;
                font-weight: 600;
                text-align: left;
            }

            .table th, .gridview th {
                padding: 16px;
                font-size: 13px;
                text-transform: uppercase;
                letter-spacing: 0.5px;
            }

            .table td, .gridview td {
                padding: 16px;
                border-bottom: 1px solid #e2e8f0;
                color: #0f172a;
                font-size: 14px;
            }

            .table tbody tr:hover, .gridview tr:hover {
                background: #f8fafc;
            }

            .table tbody tr:last-child td, .gridview tr:last-child td {
                border-bottom: none;
            }

        /* Message Styles */
        .message, .alert-custom {
            padding: 16px 20px;
            border-radius: 10px;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
            gap: 12px;
            font-size: 14px;
            font-weight: 500;
        }

        .success {
            background: #dcfce7;
            color: #166534;
            border-left: 4px solid #10b981;
        }

        .error, .text-danger {
            background: #fee2e2;
            color: #991b1b;
            border-left: 4px solid #ef4444;
        }

        .warning {
            background: #fef3c7;
            color: #92400e;
            border-left: 4px solid #f59e0b;
        }

        .info {
            background: #dbeafe;
            color: #1e40af;
            border-left: 4px solid #3b82f6;
        }

        /* Checkbox Styles */
        .checkbox-group {
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .form-check-input {
            width: 20px;
            height: 20px;
            border: 2px solid #cbd5e1;
            border-radius: 6px;
            cursor: pointer;
        }

        .form-check-label {
            font-weight: 500;
            color: #0f172a;
            cursor: pointer;
        }

        /* Validator Styles */
        .validator-message {
            color: #ef4444;
            font-size: 12px;
            margin-top: 4px;
            display: block;
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

        /* Badge Styles */
        .badge {
            display: inline-block;
            padding: 6px 12px;
            border-radius: 6px;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
        }

        .badge-success {
            background: #dcfce7;
            color: #166534;
        }

        .badge-danger {
            background: #fee2e2;
            color: #991b1b;
        }

        .badge-warning {
            background: #fef3c7;
            color: #92400e;
        }

        .badge-info {
            background: #dbeafe;
            color: #1e40af;
        }

        /* Loading Spinner */
        .spinner {
            width: 40px;
            height: 40px;
            border: 4px solid #e2e8f0;
            border-top-color: #3b82f6;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin: 20px auto;
        }

        @keyframes spin {
            to {
                transform: rotate(360deg);
            }
        }

        /* Responsive */
        @media (max-width: 768px) {
            .form-row {
                grid-template-columns: 1fr;
            }

            .btn {
                width: 100%;
                justify-content: center;
            }

            .table, .gridview {
                font-size: 12px;
            }

                .table th, .gridview th,
                .table td, .gridview td {
                    padding: 12px 8px;
                }
        }
    </style>
    <title>Add Member</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container mt-4">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white">
                <h4 class="mb-0">Add Member</h4>
            </div>
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-md-4">
                        <label>Society Name</label>
                        <asp:DropDownList ID="ddlSociety" CssClass="form-select" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlSociety_SelectedIndexChanged"></asp:DropDownList>
                    </div>
                    <div class="col-md-4">
                        <label>Building Name</label>
                        <asp:DropDownList ID="ddlBuilding" CssClass="form-select" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlBuilding_SelectedIndexChanged"></asp:DropDownList>
                    </div>
                    <div class="col-md-4">
                        <label>Unit Number</label>
                        <asp:DropDownList ID="ddlUnit" CssClass="form-select" runat="server"></asp:DropDownList>
                    </div>

                    <div class="col-md-4">
                        <label>Full Name</label>
                        <asp:TextBox ID="txtFullName" CssClass="form-control" runat="server"></asp:TextBox>
                    </div>
                    <div class="col-md-4">
                        <label>Email</label>
                        <asp:TextBox ID="txtEmail" CssClass="form-control" runat="server"></asp:TextBox>
                    </div>
                    <div class="col-md-4">
                        <label>Contact</label>
                        <asp:TextBox ID="txtContact" CssClass="form-control" runat="server"></asp:TextBox>
                    </div>

                    <div class="col-md-4">
                        <label>Status</label>
                        <asp:DropDownList ID="ddlStatus" CssClass="form-select" runat="server">
                            <asp:ListItem Text="Active" Value="Active" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="Inactive"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="mt-4 text-end">
                    <asp:Button ID="btnSave" Text="Save Member" CssClass="btn btn-success" runat="server" OnClick="btnSave_Click" />
                </div>

                <asp:Label ID="lblMessage" runat="server" CssClass="text-success mt-3 d-block"></asp:Label>
            </div>
        </div>
    </div>

</asp:Content>


