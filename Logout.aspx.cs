using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace SocietyManagement
{
    public partial class Logout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] != null)
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                UpdateLastLoginStatus(userId);
            }

            // ✅ Clear session
            Session.Clear();
            Session.Abandon();

            // Redirect to login page after 2 seconds (meta tag already handles this)
            Response.AddHeader("REFRESH", "2;URL=Login.aspx");
        }

        private void UpdateLastLoginStatus(int userId)
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    // Optional: Set IsActive or any flag update
                    string query = "UPDATE Users SET IsActive = 1 WHERE UserId = @UserId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Logout update failed: " + ex.Message);
            }
        }
    }
}
