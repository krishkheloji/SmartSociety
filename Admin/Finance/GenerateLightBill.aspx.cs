using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Finance
{
    public partial class GenerateLightBill : System.Web.UI.Page
    {
        string cs = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUnits();
            }
        }

        // 🔹 Load all units into dropdown
        private void LoadUnits()
        {
            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("SELECT UnitId, UnitNo FROM Units ORDER BY UnitNo", con);
                con.Open();
                ddlUnits.DataSource = cmd.ExecuteReader();
                ddlUnits.DataTextField = "UnitNo";
                ddlUnits.DataValueField = "UnitId";
                ddlUnits.DataBind();
                ddlUnits.Items.Insert(0, new ListItem("-- Select Unit --", ""));
            }
        }

        // 🔹 Load available readings for the selected unit
        protected void ddlUnits_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlReadings.Items.Clear();
            if (ddlUnits.SelectedValue != "")
            {
                using (SqlConnection con = new SqlConnection(cs))
                {
                    SqlCommand cmd = new SqlCommand("SELECT ReadingId, ReadingDate FROM MeterReadings WHERE UnitId=@U ORDER BY ReadingDate DESC", con);
                    cmd.Parameters.AddWithValue("@U", ddlUnits.SelectedValue);
                    con.Open();
                    ddlReadings.DataSource = cmd.ExecuteReader();
                    ddlReadings.DataTextField = "ReadingDate";
                    ddlReadings.DataValueField = "ReadingId";
                    ddlReadings.DataBind();
                    ddlReadings.Items.Insert(0, new ListItem("-- Select Reading --", ""));
                }
            }
        }

        // 🔹 Generate bill and send mail
        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            if (ddlUnits.SelectedValue == "" || ddlReadings.SelectedValue == "")
            {
                lblMessage.CssClass = "text-danger";
                lblMessage.Text = "⚠️ Please select both Unit and Reading Date.";
                return;
            }

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand(@"
                    SELECT U.UnitNo, MR.ReadingDate, MR.RatePerUnit, MR.ReadingValue
                    FROM MeterReadings MR
                    INNER JOIN Units U ON MR.UnitId = U.UnitId
                    WHERE MR.ReadingId = @R", con);

                cmd.Parameters.AddWithValue("@R", ddlReadings.SelectedValue);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    decimal rate = Convert.ToDecimal(dr["RatePerUnit"]);
                    decimal value = Convert.ToDecimal(dr["ReadingValue"]);
                    decimal total = rate * value;

                    lblUnitNo.Text = dr["UnitNo"].ToString();
                    lblReadingDate.Text = Convert.ToDateTime(dr["ReadingDate"]).ToString("dd-MMM-yyyy");
                    lblRate.Text = rate.ToString("0.00");
                    lblValue.Text = value.ToString("0.00");
                    lblAmount.Text = total.ToString("0.00");
                    pnlDetails.Visible = true;

                    dr.Close();

                    // Insert bill
                    SqlCommand insert = new SqlCommand(@"
                        INSERT INTO MaintenanceBills (SocietyId, UnitId, BillMonth, DueDate, TotalAmount, Status)
                        VALUES (@S, @U, @BM, DATEADD(DAY, 15, @BM), @A, 'Unpaid')", con);

                    insert.Parameters.AddWithValue("@S", 1);
                    insert.Parameters.AddWithValue("@U", ddlUnits.SelectedValue);
                    insert.Parameters.AddWithValue("@BM", Convert.ToDateTime(lblReadingDate.Text));
                    insert.Parameters.AddWithValue("@A", total);
                    insert.ExecuteNonQuery();

                    // 🔹 Get member email linked with the selected unit
                    SqlCommand emailCmd = new SqlCommand(@"
    SELECT M.Email, M.FullName
    FROM Members M
    INNER JOIN UnitOccupancies UO ON M.MemberId = UO.MemberId
    WHERE UO.UnitId = @UnitId 
      AND (UO.EndDate IS NULL OR UO.EndDate >= GETDATE())", con);

                    emailCmd.Parameters.AddWithValue("@UnitId", ddlUnits.SelectedValue);
                    SqlDataReader emailReader = emailCmd.ExecuteReader();

                    if (emailReader.Read())
                    {
                        string memberEmail = emailReader["Email"].ToString();
                        string memberName = emailReader["FullName"].ToString();
                        emailReader.Close();

                        try
                        {
                            // 🔹 Send mail to member
                            MailMessage mail = new MailMessage();
                            mail.From = new MailAddress("ajaychaugule2002@gmail.com");
                            mail.To.Add(memberEmail);
                            mail.Subject = "Light Bill Generated - Society Management";
                            mail.Body =
                                $"Dear {memberName},\n\n" +
                                $"Your light bill for Unit No. {lblUnitNo.Text} has been generated.\n\n" +
                                $"📅 Reading Date: {lblReadingDate.Text}\n" +
                                $"⚡ Reading Value: {lblValue.Text} units\n" +
                                $"💰 Rate per Unit: ₹{lblRate.Text}\n" +
                                $"💵 Total Amount: ₹{lblAmount.Text}\n\n" +
                                $"Please make your payment by the due date.\n\n" +
                                $"Regards,\nSociety Management Team";

                            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                            smtp.Port = 587;
                            smtp.Credentials = new NetworkCredential("ajaychaugule2002@gmail.com", "oxez vkdu aiky uebu");
                            smtp.EnableSsl = true;
                            smtp.Send(mail);

                            lblMessage.CssClass = "text-success";
                            lblMessage.Text = "✅ Light bill generated and mail sent successfully to the member.";
                        }
                        catch (Exception ex)
                        {
                            lblMessage.CssClass = "text-warning";
                            lblMessage.Text = "⚠️ Bill generated but failed to send email. " + ex.Message;
                        }
                    }
                    else
                    {
                        lblMessage.CssClass = "text-warning";
                        lblMessage.Text = "⚠️ Bill generated but no active member email found for this unit.";
                    }
                }
                else
                {
                    lblMessage.CssClass = "text-danger";
                    lblMessage.Text = "❌ Failed to find reading data.";
                }
            }
        }
    }
}