<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true"
    CodeBehind="ManageSociety.aspx.cs" Inherits="SocietyManagement.Admin.Society.ManageSociety" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Use common styles from previous artifact */
        .page-header {
            margin-bottom: 32px;
        }

        .page-header h2 {
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
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 20px;
        }

        .form-section {
            background: white;
            border-radius: 16px;
            padding: 32px;
            border: 1px solid #e2e8f0;
            margin-bottom: 32px;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
        }

        .form-row {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 24px;
            margin-bottom: 24px;
        }

        .form-group label {
            display: block;
            font-weight: 600;
            color: #0f172a;
            margin-bottom: 8px;
            font-size: 14px;
        }

        input[type="text"], textarea {
            width: 100%;
            padding: 12px 16px;
            border: 2px solid #e2e8f0;
            border-radius: 10px;
            font-size: 14px;
            transition: all 0.2s;
            background: #f8fafc;
        }

        input[type="text"]:focus, textarea:focus {
            outline: none;
            border-color: #3b82f6;
            background: white;
            box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
        }

        textarea {
            resize: vertical;
            min-height: 80px;
        }

        .btn {
            padding: 12px 28px;
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
        }

        .grid-container {
            background: white;
            border-radius: 16px;
            padding: 24px;
            border: 1px solid #e2e8f0;
        }

        .grid-header {
            font-size: 20px;
            font-weight: 700;
            color: #0f172a;
            margin-bottom: 20px;
            padding-bottom: 16px;
            border-bottom: 2px solid #e2e8f0;
        }

        .gridview {
            width: 100%;
            background: white;
            border-radius: 12px;
            overflow: hidden;
            border: 1px solid #e2e8f0;
        }

        .gridview th {
            background: linear-gradient(135deg, #1e293b 0%, #334155 100%);
            color: white;
            padding: 16px;
            text-align: left;
            font-weight: 600;
            font-size: 13px;
            text-transform: uppercase;
        }

        .gridview td {
            padding: 16px;
            border-bottom: 1px solid #e2e8f0;
            color: #0f172a;
        }

        .gridview tr:hover {
            background: #f8fafc;
        }

        .btn-success {
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
            padding: 8px 16px;
            font-size: 13px;
        }

        .btn-danger {
            background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
            color: white;
            padding: 8px 16px;
            font-size: 13px;
        }

        .message {
            padding: 16px 20px;
            border-radius: 10px;
            margin-bottom: 24px;
            font-weight: 500;
        }

        .success {
            background: #dcfce7;
            color: #166534;
            border-left: 4px solid #10b981;
        }

        .error {
            background: #fee2e2;
            color: #991b1b;
            border-left: 4px solid #ef4444;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <div class="page-header">
        <h2>
            <div class="page-icon">
                <i class="fas fa-city"></i>
            </div>
            Manage Societies
        </h2>
    </div>

    <asp:Label ID="lblMessage" runat="server" CssClass="message" Visible="false"></asp:Label>

    <div class="form-section">
        <div class="form-row">
            <div class="form-group">
                <label>Society Name <span style="color: #ef4444;">*</span></label>
                <asp:TextBox ID="txtName" runat="server" placeholder="Enter society name"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvName" runat="server"
                    ControlToValidate="txtName" ErrorMessage="*Required" ForeColor="#ef4444" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <label>City <span style="color: #ef4444;">*</span></label>
                <asp:TextBox ID="txtCity" runat="server" placeholder="Enter city"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvCity" runat="server"
                    ControlToValidate="txtCity" ErrorMessage="*Required" ForeColor="#ef4444" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>Address Line 1 <span style="color: #ef4444;">*</span></label>
                <asp:TextBox ID="txtAddressLine1" runat="server" placeholder="Street address"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvAddress1" runat="server"
                    ControlToValidate="txtAddressLine1" ErrorMessage="*Required" ForeColor="#ef4444" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <label>Address Line 2</label>
                <asp:TextBox ID="txtAddressLine2" runat="server" placeholder="Apartment, suite, etc. (optional)" MaxLength="200"></asp:TextBox>
            </div>
        </div>

        <div class="form-row">
            <div class="form-group">
                <label>State <span style="color: #ef4444;">*</span></label>
                <asp:TextBox ID="txtState" runat="server" placeholder="Enter state"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvState" runat="server"
                    ControlToValidate="txtState" ErrorMessage="*Required" ForeColor="#ef4444" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <label>Pincode <span style="color: #ef4444;">*</span></label>
                <asp:TextBox ID="txtPincode" runat="server" placeholder="Enter pincode"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvPincode" runat="server"
                    ControlToValidate="txtPincode" ErrorMessage="*Required" ForeColor="#ef4444" Display="Dynamic"></asp:RequiredFieldValidator>
            </div>
        </div>

        <div class="form-group">
            <asp:HiddenField ID="hfSocietyId" runat="server" Value="0" />
            <asp:Button ID="btnSave" runat="server" Text="💾 Save Society" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="🔄 Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" CausesValidation="false" />
        </div>
    </div>

    <div class="grid-container">
        <h3 class="grid-header">📋 Existing Societies</h3>
        <asp:GridView ID="gvSocieties" runat="server" AutoGenerateColumns="False" CssClass="gridview"
            OnRowCommand="gvSocieties_RowCommand" DataKeyNames="SocietyId"
            EmptyDataText="No societies found. Please add a society.">
            <Columns>
                <asp:BoundField DataField="SocietyId" HeaderText="ID" ReadOnly="True" />
                <asp:BoundField DataField="Name" HeaderText="Society Name" />
                <asp:BoundField DataField="City" HeaderText="City" />
                <asp:BoundField DataField="State" HeaderText="State" />
                <asp:BoundField DataField="Pincode" HeaderText="Pincode" />
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <asp:Button ID="btnEdit" runat="server" Text="✏️ Edit" CssClass="btn btn-success"
                            CommandName="EditSociety" CommandArgument='<%# Eval("SocietyId") %>' CausesValidation="false" />
                        <asp:Button ID="btnDelete" runat="server" Text="🗑️ Delete" CssClass="btn btn-danger"
                            CommandName="DeleteSociety" CommandArgument='<%# Eval("SocietyId") %>'
                            OnClientClick="return confirm('Are you sure you want to delete this society?');" CausesValidation="false" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>