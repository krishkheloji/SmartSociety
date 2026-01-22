using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SocietyManagement.Member.Amenities
{
    public partial class MyBookings : System.Web.UI.Page
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
                BindBookings();
        }

        private void BindBookings()
        {
            try
            {
                using (var con = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(@"
                    SELECT 
                        ab.BookingId,
                        a.Name AS AmenityName,
                        ab.StartTime,
                        ab.EndTime,
                        ab.Status
                    FROM AmenityBookings ab
                    INNER JOIN Amenities a ON ab.AmenityId = a.AmenityId
                    INNER JOIN Users u ON ab.UserId = u.UserId
                    WHERE u.Username = @Username
                    ORDER BY ab.StartTime DESC", con))
                {
                    cmd.Parameters.AddWithValue("@Username", Session["Username"].ToString());
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        gvBookings.DataSource = dt;
                        gvBookings.DataBind();
                    }
                }

                if (gvBookings.Rows.Count == 0)
                    lblMessage.Text = "You have no bookings yet.";
                else
                    lblMessage.Text = "";
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error loading bookings: " + ex.Message;
            }
        }

        protected void gvBookings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CancelBooking")
            {
                long bookingId = Convert.ToInt64(e.CommandArgument);
                CancelBooking(bookingId);
            }
        }

        private void CancelBooking(long bookingId)
        {
            try
            {
                using (var con = new SqlConnection(ConnString))
                using (var cmd = new SqlCommand(@"
                    UPDATE AmenityBookings 
                    SET Status = 'Cancelled' 
                    WHERE BookingId = @BookingId AND Status = 'Booked'", con))
                {
                    cmd.Parameters.AddWithValue("@BookingId", bookingId);
                    con.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows > 0)
                        lblMessage.Text = "✅ Booking cancelled successfully.";
                    else
                        lblMessage.Text = "⚠ Booking cannot be cancelled (it might already be approved or cancelled).";
                }

                BindBookings();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "⚠ Error cancelling booking: " + ex.Message;
            }
        }
    }
}
