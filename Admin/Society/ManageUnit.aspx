<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ManageUnit.aspx.cs" Inherits="SocietyManagement.Admin.Society.ManageUnit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
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

    <!-- ✅ Only one ScriptManager -->
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <div class="container">
        <h2>🏢 Manage Units</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="message" Visible="false"></asp:Label>

        <div class="form-section">
            <div class="form-row">
                <div class="form-group">
                    <label>Select Society:</label>
                    <asp:DropDownList ID="ddlSociety" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlSociety_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="rfvSociety" runat="server" ControlToValidate="ddlSociety"
                        InitialValue="0" ErrorMessage="*Please select a society" ForeColor="Red" Display="Dynamic">
                    </asp:RequiredFieldValidator>
                </div>

                <div class="form-group">
                    <label>Select Building:</label>
                    <asp:DropDownList ID="ddlBuilding" runat="server">
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="rfvBuilding" runat="server" ControlToValidate="ddlBuilding"
                        InitialValue="0" ErrorMessage="*Please select a building" ForeColor="Red" Display="Dynamic">
                    </asp:RequiredFieldValidator>
                </div>
            </div>

            <div class="form-row">
                <div class="form-group">
                    <label>Unit Number:</label>
                    <asp:TextBox ID="txtUnitNo" runat="server" placeholder="e.g., 101, A-201" MaxLength="20"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvUnitNo" runat="server" ControlToValidate="txtUnitNo"
                        ErrorMessage="*Required" ForeColor="Red" Display="Dynamic">
                    </asp:RequiredFieldValidator>
                </div>

                <div class="form-group">
                    <label>Floor Number:</label>
                    <asp:TextBox ID="txtFloorNo" runat="server" TextMode="Number" placeholder="Floor number"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvFloorNo" runat="server" ControlToValidate="txtFloorNo"
                        ErrorMessage="*Required" ForeColor="Red" Display="Dynamic">
                    </asp:RequiredFieldValidator>
                </div>
            </div>

            <div class="form-row">
                <div class="form-group">
                    <label>Carpet Area (Sq.Ft):</label>
                    <asp:TextBox ID="txtCarpetArea" runat="server" placeholder="Enter carpet area"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvCarpetArea" runat="server" ControlToValidate="txtCarpetArea"
                        ErrorMessage="*Required" ForeColor="Red" Display="Dynamic">
                    </asp:RequiredFieldValidator>
                </div>

                <div class="form-group">
                    <label>Parking Allocated:</label>
                    <div class="checkbox-group">
                        <asp:CheckBox ID="chkParking" runat="server" Text="Yes, parking is allocated" />
                    </div>
                </div>
            </div>

            <div class="form-group">
                <asp:HiddenField ID="hfUnitId" runat="server" Value="0" />
                <asp:Button ID="btnSave" runat="server" Text="Save Unit" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
            </div>
        </div>

        <div class="grid-container">
            <h3>📋 Existing Units</h3>
            <asp:GridView ID="gvUnits" runat="server" AutoGenerateColumns="False" CssClass="gridview"
                OnRowCommand="gvUnits_RowCommand" DataKeyNames="UnitId" EmptyDataText="No units found. Please add units.">
                <columns>
                    <asp:BoundField DataField="UnitId" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="UnitNo" HeaderText="Unit No" />
                    <asp:BoundField DataField="BuildingName" HeaderText="Building" />
                    <asp:BoundField DataField="SocietyName" HeaderText="Society" />
                    <asp:BoundField DataField="FloorNo" HeaderText="Floor" />
                    <asp:BoundField DataField="CarpetAreaSqft" HeaderText="Area (Sq.Ft)" />
                    <asp:BoundField DataField="IsParkingAllocated" HeaderText="Parking" />
                    <asp:TemplateField HeaderText="Actions">
                        <itemtemplate>
                            <asp:Button ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-success"
                                CommandName="EditUnit" CommandArgument='<%# Eval("UnitId") %>' CausesValidation="false" />
                            <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-danger"
                                CommandName="DeleteUnit" CommandArgument='<%# Eval("UnitId") %>'
                                OnClientClick="return confirm('Are you sure you want to delete this unit?');" CausesValidation="false" />
                        </itemtemplate>
                    </asp:TemplateField>
                </columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
