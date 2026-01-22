using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin
{
    public partial class ManageAnnouncements : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadAnnouncementsGrid();
            }
        }

        private void LoadAnnouncementsGrid()
        {
            string constr = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = @"SELECT AnnouncementId, Title, Content, VisibleFrom, VisibleTo
                                 FROM Announcements
                                 ORDER BY AnnouncementId DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvAnnouncements.DataSource = dt;
                gvAnnouncements.DataBind();
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string content = txtContent.Text.Trim();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                Response.Write("<script>alert('Please enter both title and content.');</script>");
                return;
            }

            if (!DateTime.TryParse(txtVisibleFrom.Text, out DateTime visibleFrom) ||
                !DateTime.TryParse(txtVisibleTo.Text, out DateTime visibleTo))
            {
                Response.Write("<script>alert('Invalid date format. Please enter valid dates.');</script>");
                return;
            }

            long societyId = 1; // You can replace with session-based SocietyId

            string constr = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;
            try
            {
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand("AddAnnouncementProc", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@SocietyId", SqlDbType.BigInt).Value = societyId;
                        cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 150).Value = title;
                        cmd.Parameters.Add("@Content", SqlDbType.NVarChar).Value = content;
                        cmd.Parameters.Add("@VisibleFrom", SqlDbType.Date).Value = visibleFrom;
                        cmd.Parameters.Add("@VisibleTo", SqlDbType.Date).Value = visibleTo;

                        con.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            Response.Write("<script>alert('Announcement added successfully');</script>");
                            LoadAnnouncementsGrid();
                            ClearForm();
                        }
                        else
                        {
                            Response.Write("<script>alert('Failed to add announcement');</script>");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write($"<script>alert('Error: {ex.Message}');</script>");
            }
        }

        private void ClearForm()
        {
            txtTitle.Text = "";
            txtContent.Text = "";
            txtVisibleFrom.Text = "";
            txtVisibleTo.Text = "";
        }
    }
}