using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Web.UI.WebControls;

namespace SocietyManagement.Member.LightBill
{
    public partial class ViewLightBills : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadLightBills();
        }

        private void LoadLightBills()
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
                    SELECT 
                        M.ReadingId,
                        M.ReadingDate,
                        M.ReadingValue,
                        M.RatePerUnit,
                        (M.ReadingValue * M.RatePerUnit) AS TotalAmount,
                        CASE 
                            WHEN EXISTS (
                                SELECT 1 
                                FROM Payments P
                                WHERE P.ReferenceNo LIKE '%LR-'+CAST(M.ReadingId AS NVARCHAR)+'%'
                            )
                            THEN 'Paid'
                            ELSE 'Pending'
                        END AS Status
                    FROM MeterReadings M
                    INNER JOIN Units U ON M.UnitId = U.UnitId
                    INNER JOIN UnitOccupancies O ON O.UnitId = U.UnitId
                    INNER JOIN Members Me ON O.MemberId = Me.MemberId
                    WHERE Me.MemberId = @MemberId
                    ORDER BY M.ReadingDate DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@MemberId", memberId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvLightBills.DataSource = dt;
                gvLightBills.DataBind();
            }
        }

        protected void gvLightBills_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "PayNow")
            {
                int readingId = Convert.ToInt32(e.CommandArgument);
                Response.Redirect("PayLightBill.aspx?ReadingId=" + readingId);
            }
            else if (e.CommandName == "DownloadPDF")
            {
                int readingId = Convert.ToInt32(e.CommandArgument);
                GeneratePDF(readingId);
            }
        }

        private void GeneratePDF(int readingId)
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                string query = @"
                    SELECT 
                        M.ReadingId,
                        M.ReadingDate,
                        M.ReadingValue,
                        M.RatePerUnit,
                        (M.ReadingValue * M.RatePerUnit) AS TotalAmount,
                        U.UnitId,
                        Me.FullName
                    FROM MeterReadings M
                    INNER JOIN Units U ON M.UnitId = U.UnitId
                    INNER JOIN UnitOccupancies O ON O.UnitId = U.UnitId
                    INNER JOIN Members Me ON O.MemberId = Me.MemberId
                    WHERE M.ReadingId = @ReadingId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ReadingId", readingId);
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    Document doc = new Document();
                    MemoryStream ms = new MemoryStream();
                    PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // Header
                    Paragraph header = new Paragraph("Light Bill Receipt", new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD));
                    header.Alignment = Element.ALIGN_CENTER;
                    doc.Add(header);
                    doc.Add(new Paragraph("\n-------------------------------------------\n"));

                    // Details
                    doc.Add(new Paragraph("Reading ID: " + dr["ReadingId"].ToString()));
                    doc.Add(new Paragraph("Member Name: " + dr["FullName"].ToString()));
                    doc.Add(new Paragraph("Unit Number: " + dr["UnitId"].ToString()));
                    doc.Add(new Paragraph("Reading Date: " + Convert.ToDateTime(dr["ReadingDate"]).ToString("dd-MMM-yyyy")));
                    doc.Add(new Paragraph("Units Consumed: " + dr["ReadingValue"].ToString()));
                    doc.Add(new Paragraph("Rate per Unit: ₹" + dr["RatePerUnit"].ToString()));
                    doc.Add(new Paragraph("Total Amount: ₹" + Convert.ToDecimal(dr["TotalAmount"]).ToString("N2")));
                    doc.Add(new Paragraph("\nStatus: Paid"));
                    doc.Add(new Paragraph("Payment Reference: LR-" + dr["ReadingId"].ToString()));
                    doc.Add(new Paragraph("Generated On: " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt")));

                    doc.Add(new Paragraph("\n-------------------------------------------"));
                    doc.Add(new Paragraph("Thank you for your payment!", new Font(Font.FontFamily.HELVETICA, 12, Font.ITALIC)));

                    doc.Close();

                    byte[] bytes = ms.ToArray();
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=LightBill_" + readingId + ".pdf");
                    Response.BinaryWrite(bytes);
                    Response.End();
                }
            }
        }
    }
}
