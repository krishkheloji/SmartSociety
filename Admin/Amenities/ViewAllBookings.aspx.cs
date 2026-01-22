using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Amenities
{
    public partial class ViewAllBookings : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Authentication check
            if (Session["Username"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // Only admin allowed
            if (!Session["Role"].ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/AccessDenied.aspx");
                return;
            }

            if (!IsPostBack)
                BindBookings();
        }

        private long GetCurrentSocietyId()
        {
            using (var con = new SqlConnection(ConnString))
            using (var cmd = new SqlCommand(@"
                SELECT m.SocietyId
                FROM Users u
                INNER JOIN Members m ON u.MemberId = m.MemberId
                WHERE u.Username = @Username", con))
            {
                cmd.Parameters.AddWithValue("@Username", Session["Username"].ToString());
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    throw new Exception("Unable to determine society for current admin.");
                return Convert.ToInt64(result);
            }
        }

        private void BindBookings()
        {
            try
            {
                long societyId = GetCurrentSocietyId();

                using (var con = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        ab.BookingId,
                        a.Name AS AmenityName,
                        u.Username,
                        ab.StartTime,
                        ab.EndTime,
                        ab.Status
                    FROM AmenityBookings ab
                    INNER JOIN Amenities a ON ab.AmenityId = a.AmenityId
                    INNER JOIN Users u ON ab.UserId = u.UserId
                    WHERE a.SocietyId = @SocietyId
                    ORDER BY ab.StartTime DESC", con))
                {
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvAllBookings.DataSource = dt;
                        gvAllBookings.DataBind();
                    }
                }

                if (gvAllBookings.Rows.Count == 0)
                    lblMessage.Text = "ℹ️ No bookings found.";
                else
                    lblMessage.Text = "";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error loading bookings: " + ex.Message;
            }
        }

        protected void gvAllBookings_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAllBookings.PageIndex = e.NewPageIndex;
            BindBookings();
        }

        protected void btnGoToApprovals_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Admin/Amenities/ApproveBookings.aspx");
        }
    }
}
