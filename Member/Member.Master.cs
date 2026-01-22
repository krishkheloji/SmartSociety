using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Member
{
    public partial class Member : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                LoadNotifications();
            }
            // ✅ Check if user session exists
            if (Session["UserId"] == null || Session["Role"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            // ✅ Allow only Members
            if (Session["Role"].ToString() != "Member")
            {
                Response.Write("<script>alert('Access denied! Members only.'); window.location='~/Login.aspx';</script>");
                return;
            }

            // ✅ Optional: Display member name
            lblMemberName.Text = "Welcome, " + Session["Username"].ToString();
        }

        private void LoadNotifications()
        {
            long userId = Convert.ToInt64(Session["UserId"]);

            string query = @"SELECT TOP 5 NotificationId, Title, Message, Link, CreatedAt 
                     FROM Notifications 
                     WHERE UserId = @UserId AND IsRead = 0 
                     ORDER BY CreatedAt DESC";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                rptNotifications.DataSource = dr;
                rptNotifications.DataBind();
            }

            // Load count badge
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Notifications WHERE UserId=@UserId AND IsRead=0", con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                con.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                lblNotificationCount.InnerText = count.ToString();
            }
        }
    }
}