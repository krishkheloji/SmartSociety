using iTextSharp.text;
using iTextSharp.text.pdf;
using SocietyManagement.Member;
using SocietyManagement.Member.Maintenance;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;

namespace SocietyManagement.Member.Maintenance
{
    public partial class ViewBills : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 🔹 1. Mark notification as read (if accessed from dropdown)
                if (Request.QueryString["nid"] != null)
                {
                    long notifId;
                    if (long.TryParse(Request.QueryString["nid"], out notifId))
                    {
                        MarkNotificationAsRead(notifId);

                        // Optional: reload without query string
                        Response.Redirect("ViewBills.aspx", false);
                        return;
                    }
                }

                // 🔹 2. Load bills
                LoadBills();
            }
        }


        private void LoadBills()
        {
            if (Session["MemberId"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            int memberId = Convert.ToInt32(Session["MemberId"]);

            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
                    SELECT mb.BillId,
                           FORMAT(mb.BillMonth, 'MMM yyyy') AS BillMonth,
                           mb.TotalAmount AS Amount,
                           mb.DueDate,
                           CASE WHEN p.PaymentId IS NOT NULL THEN 'Paid' ELSE 'Unpaid' END AS Status
                    FROM MaintenanceBills mb
                    INNER JOIN Units u ON mb.UnitId = u.UnitId
                    INNER JOIN UnitOccupancies o ON o.UnitId = u.UnitId
                    INNER JOIN Members m ON o.MemberId = m.MemberId
                    LEFT JOIN Payments p ON p.BillId = mb.BillId
                    WHERE m.MemberId = @MemberId
                      AND (p.ReferenceNo IS NULL OR p.ReferenceNo NOT LIKE 'LR-%')
                    ORDER BY mb.BillMonth ASC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@MemberId", memberId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvBills.DataSource = dt;
                gvBills.DataBind();
            }
        }

        protected void gvBills_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "GeneratePDF")
            {
                int billId = Convert.ToInt32(e.CommandArgument);
                GeneratePDF(billId);
            }
            else if (e.CommandName == "PayNow")
            {
                int billId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect("MakePayment.aspx?BillId=" + billId);
            }
        }

        private void GeneratePDF(int billId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string billQuery = @"
                    SELECT mb.BillId, s.Name AS SocietyName, s.AddressLine1, s.City, s.State,
                           m.FullName, u.UnitNo, mb.BillMonth, mb.TotalAmount, mb.DueDate,
                           CASE WHEN p.PaymentId IS NOT NULL THEN 'Paid' ELSE 'Unpaid' END AS Status
                    FROM MaintenanceBills mb
                    INNER JOIN Societies s ON mb.SocietyId = s.SocietyId
                    INNER JOIN Units u ON mb.UnitId = u.UnitId
                    INNER JOIN UnitOccupancies o ON o.UnitId = u.UnitId
                    INNER JOIN Members m ON o.MemberId = m.MemberId
                    LEFT JOIN Payments p ON p.BillId = mb.BillId
                    WHERE mb.BillId = @BillId
                      AND (p.ReferenceNo IS NULL OR p.ReferenceNo NOT LIKE 'LR-%')";

                SqlCommand cmdBill = new SqlCommand(billQuery, con);
                cmdBill.Parameters.AddWithValue("@BillId", billId);

                con.Open();
                SqlDataReader dr = cmdBill.ExecuteReader();

                if (dr.Read())
                {
                    string societyName = dr["SocietyName"].ToString();
                    string societyAddress = $"{dr["AddressLine1"]}, {dr["City"]}, {dr["State"]}";
                    string memberName = dr["FullName"].ToString();
                    string unitNo = dr["UnitNo"].ToString();
                    string billMonth = Convert.ToDateTime(dr["BillMonth"]).ToString("MMM yyyy");
                    string dueDate = Convert.ToDateTime(dr["DueDate"]).ToString("dd-MMM-yyyy");
                    string status = dr["Status"].ToString();
                    decimal totalAmount = Convert.ToDecimal(dr["TotalAmount"]);
                    dr.Close();

                    // Fetch Bill Items
                    SqlCommand cmdItems = new SqlCommand(@"
                        SELECT Description, Amount
                        FROM BillItems
                        WHERE BillId = @BillId", con);
                    cmdItems.Parameters.AddWithValue("@BillId", billId);

                    SqlDataAdapter da = new SqlDataAdapter(cmdItems);
                    DataTable dtItems = new DataTable();
                    da.Fill(dtItems);

                    // Create PDF
                    Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                    MemoryStream ms = new MemoryStream();
                    PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // Fonts
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                    // Society Header
                    Paragraph societyHeader = new Paragraph(societyName + "\n", titleFont);
                    societyHeader.Alignment = Element.ALIGN_CENTER;
                    doc.Add(societyHeader);
                    Paragraph addr = new Paragraph(societyAddress + "\n\n", normalFont);
                    addr.Alignment = Element.ALIGN_CENTER;
                    doc.Add(addr);

                    // Title
                    Paragraph title = new Paragraph("Maintenance Bill Invoice\n\n", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    doc.Add(title);

                    // Bill & Member Info
                    doc.Add(new Paragraph($"Bill ID: {billId}", normalFont));
                    doc.Add(new Paragraph($"Member Name: {memberName}", normalFont));
                    doc.Add(new Paragraph($"Unit Number: {unitNo}", normalFont));
                    doc.Add(new Paragraph($"Bill Month: {billMonth}", normalFont));
                    doc.Add(new Paragraph($"Due Date: {dueDate}", normalFont));
                    doc.Add(new Paragraph($"Status: {status}\n\n", normalFont));

                    // Bill Items Table
                    PdfPTable table = new PdfPTable(2);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 70f, 30f });

                    PdfPCell h1 = new PdfPCell(new Phrase("Description", boldFont));
                    PdfPCell h2 = new PdfPCell(new Phrase("Amount (₹)", boldFont));
                    h1.BackgroundColor = BaseColor.LIGHT_GRAY;
                    h2.BackgroundColor = BaseColor.LIGHT_GRAY;
                    table.AddCell(h1);
                    table.AddCell(h2);

                    foreach (DataRow row in dtItems.Rows)
                    {
                        table.AddCell(new Phrase(row["Description"].ToString(), normalFont));
                        table.AddCell(new Phrase(Convert.ToDecimal(row["Amount"]).ToString("0.00"), normalFont));
                    }

                    // Total Row
                    PdfPCell totalLabel = new PdfPCell(new Phrase("Total", boldFont));
                    totalLabel.HorizontalAlignment = Element.ALIGN_RIGHT;
                    table.AddCell(totalLabel);
                    PdfPCell totalValue = new PdfPCell(new Phrase("₹" + totalAmount.ToString("0.00"), boldFont));
                    table.AddCell(totalValue);

                    doc.Add(table);

                    // Footer
                    doc.Add(new Paragraph("\nGenerated on: " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"), normalFont));
                    doc.Close();

                    // Output PDF
                    byte[] bytes = ms.ToArray();
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=MaintenanceBill_" + billId + ".pdf");
                    Response.BinaryWrite(bytes);
                    Response.End();
                }
            }
        }

        private void MarkNotificationAsRead(long notificationId)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("UPDATE Notifications SET IsRead = 1 WHERE NotificationId = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", notificationId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

    }
}