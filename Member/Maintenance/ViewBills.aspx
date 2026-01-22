<%@ Page Title="View Maintenance Bills" Language="C#" MasterPageFile="~/Member/Member.Master"
    AutoEventWireup="true" CodeBehind="ViewBills.aspx.cs" Inherits="SocietyManagement.Member.Maintenance.ViewBills" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .page-header {
            margin-bottom: 32px;
        }

        .page-header h2 {
            font-size: 28px;
            font-weight: 700;
            color: #0f172a;
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

        .bills-container {
            background: white;
            border-radius: 16px;
            padding: 24px;
            border: 1px solid #e2e8f0;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
        }

        .table {
            width: 100%;
            background: white;
            border-radius: 12px;
            overflow: hidden;
            border-collapse: separate;
            border-spacing: 0;
        }

        .table thead {
            background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
        }

        .table th {
            padding: 16px;
            color: white;
            font-weight: 600;
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .table td {
            padding: 16px;
            border-bottom: 1px solid #e2e8f0;
            color: #0f172a;
        }

        .table tbody tr:hover {
            background: #f8fafc;
        }

        .table tbody tr:last-child td {
            border-bottom: none;
        }

        .status-paid {
            color: #166534;
            font-weight: 600;
            padding: 6px 12px;
            background: #dcfce7;
            border-radius: 6px;
            display: inline-block;
        }

        .status-unpaid {
            color: #92400e;
            font-weight: 600;
            padding: 6px 12px;
            background: #fef3c7;
            border-radius: 6px;
            display: inline-block;
        }

        .btn-sm {
            padding: 8px 16px;
            font-size: 13px;
            border-radius: 8px;
            font-weight: 600;
            border: none;
            cursor: pointer;
            transition: all 0.2s;
        }

        .btn-primary {
            background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
            color: white;
        }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 12px rgba(59, 130, 246, 0.3);
        }

        .btn-success {
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
        }

        .btn-success:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 12px rgba(16, 185, 129, 0.3);
        }

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
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-header">
        <h2>
            <div class="page-icon">
                <i class="fas fa-file-invoice-dollar"></i>
            </div>
            My Maintenance Bills
        </h2>
    </div>

    <div class="bills-container">
        <asp:GridView ID="gvBills" runat="server" AutoGenerateColumns="False"
            CssClass="table"
            OnRowCommand="gvBills_RowCommand" 
            EmptyDataText="No bills found.">

            <Columns>
                <asp:BoundField DataField="BillId" HeaderText="Bill ID" />
                <asp:BoundField DataField="BillMonth" HeaderText="Month" DataFormatString="{0:MMM yyyy}" />
                <asp:BoundField DataField="Amount" HeaderText="Amount (₹)" DataFormatString="{0:N2}" />
                <asp:BoundField DataField="DueDate" HeaderText="Due Date" DataFormatString="{0:dd MMM yyyy}" />

                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <span class='<%# Eval("Status").ToString() == "Paid" ? "status-paid" : "status-unpaid" %>'>
                            <%# Eval("Status") %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Action">
                    <ItemTemplate>
                        <asp:Button ID="btnPdf" runat="server" Text="📄 Download PDF"
                            CommandName="GeneratePDF"
                            CommandArgument='<%# Eval("BillId") %>'
                            CssClass="btn-sm btn-primary"
                            Visible='<%# Eval("Status").ToString() == "Paid" %>' />

                        <asp:Button ID="btnPayNow" runat="server" Text="💳 Pay Now"
                            CommandName="PayNow"
                            CommandArgument='<%# Eval("BillId") %>'
                            CssClass="btn-sm btn-success"
                            Visible='<%# Eval("Status").ToString() != "Paid" %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>

            <EmptyDataTemplate>
                <div class="empty-state">
                    <i class="fas fa-inbox"></i>
                    <p>No bills found</p>
                </div>
            </EmptyDataTemplate>

        </asp:GridView>
    </div>
</asp:Content>