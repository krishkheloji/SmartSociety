using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SocietyManagement.Member.Amenities
{
    public partial class ViewAmenities : System.Web.UI.Page
    {
        private string ConnString => ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!Session["Role"].ToString().Equals("Member", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/AccessDenied.aspx");
                return;
            }

            if (!IsPostBack)
                BindAmenities();
        }

        private void BindAmenities()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT AmenityId, Name, BookingRequired
                    FROM Amenities
                    WHERE SocietyId = (
                        SELECT TOP 1 m.SocietyId 
                        FROM Users u
                        INNER JOIN Members m ON u.MemberId = m.MemberId
                        WHERE u.Username = @Username
                    )
                    ORDER BY Name;", con))
                {
                    cmd.Parameters.AddWithValue("@Username", Session["Username"].ToString());
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvAmenities.DataSource = dt;
                        gvAmenities.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.CssClass = "text-danger";
                lblMessage.Text = "⚠ Error loading amenities: " + ex.Message;
            }
        }

        protected void gvAmenities_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "BookAmenity")
            {
                try
                {
                    string[] args = e.CommandArgument.ToString().Split('|');
                    hfAmenityId.Value = args[0];
                    lblAmenityName.Text = args[1];
                    pnlBooking.Visible = true;
                    lblMessage.Text = "";
                }
                catch
                {
                    lblMessage.Text = "⚠ Invalid amenity selection.";
                }
            }
        }

        protected void btnConfirmBooking_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            lblMessage.CssClass = "text-danger";

            if (hfAmenityId.Value == "")
            {
                lblMessage.Text = "⚠ Please select an amenity first.";
                return;
            }

            if (txtStart.Text == "" || txtEnd.Text == "")
            {
                lblMessage.Text = "⚠ Please select both start and end time.";
                return;
            }

            DateTime startTime = Convert.ToDateTime(txtStart.Text);
            DateTime endTime = Convert.ToDateTime(txtEnd.Text);

            if (endTime <= startTime)
            {
                lblMessage.Text = "⚠ End time must be after start time.";
                return;
            }

            try
            {
                long amenityId = Convert.ToInt64(hfAmenityId.Value);

                using (SqlConnection con = new SqlConnection(ConnString))
                {
                    con.Open();

                    // 1️⃣ Check if the amenity is already booked in that time range
                    string checkQuery = @"
                SELECT COUNT(*) 
                FROM AmenityBookings
                WHERE AmenityId = @AmenityId
                AND Status IN ('Pending', 'Approved')
                AND @StartTime < EndTime
                AND @EndTime > StartTime";

                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@AmenityId", amenityId);
                    checkCmd.Parameters.AddWithValue("@StartTime", startTime);
                    checkCmd.Parameters.AddWithValue("@EndTime", endTime);

                    int existingBookings = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (existingBookings > 0)
                    {
                        lblMessage.Text = "⚠ This amenity is already booked by another member during the selected time.";
                        return;
                    }

                    // 2️⃣ Insert the booking as 'Pending'
                    string insertQuery = @"
                INSERT INTO AmenityBookings (AmenityId, UserId, StartTime, EndTime, Status)
                SELECT @AmenityId, u.UserId, @StartTime, @EndTime, 'Pending'
                FROM Users u
                WHERE u.Username = @Username;";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, con);
                    insertCmd.Parameters.AddWithValue("@AmenityId", amenityId);
                    insertCmd.Parameters.AddWithValue("@Username", Session["Username"].ToString());
                    insertCmd.Parameters.AddWithValue("@StartTime", startTime);
                    insertCmd.Parameters.AddWithValue("@EndTime", endTime);

                    int rows = insertCmd.ExecuteNonQuery();

                    if (rows > 0)
                    {
                        lblMessage.CssClass = "text-success";
                        lblMessage.Text = "✅ Booking request submitted successfully (awaiting approval).";
                        pnlBooking.Visible = false;
                        txtStart.Text = "";
                        txtEnd.Text = "";
                    }
                    else
                    {
                        lblMessage.Text = "⚠ Unable to find user record.";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error submitting booking: " + ex.Message;
            }
        }

    }
}
