using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SocietyManagement.Member.Complaints
{
    public partial class ComplaintDetails : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MemberId"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["id"] != null)
                {
                    long complaintId = Convert.ToInt64(Request.QueryString["id"]);
                    LoadComplaintDetails(complaintId);
                    LoadComments(complaintId);
                }
                else
                {
                    lblMessage.Text = "Invalid complaint ID.";
                }
            }
        }

        private void LoadComplaintDetails(long complaintId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT c.Title, c.Description, c.Category, c.Status, c.CreatedAt,
                           ISNULL(m.FullName, 'Admin') AS RaisedBy
                    FROM Complaints c
                    LEFT JOIN Users u ON c.RaisedByUserId = u.UserId
                    LEFT JOIN Members m ON u.MemberId = m.MemberId
                    WHERE c.ComplaintId = @ComplaintId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ComplaintId", complaintId);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                    {
                        pnlDetails.Visible = true;
                        lblTitle.Text = rdr["Title"].ToString();
                        lblDescription.Text = rdr["Description"].ToString();
                        lblCategory.Text = rdr["Category"].ToString();
                        lblStatus.Text = rdr["Status"].ToString();
                        lblRaisedBy.Text = rdr["RaisedBy"].ToString();
                        lblCreatedAt.Text = Convert.ToDateTime(rdr["CreatedAt"]).ToString("dd-MM-yyyy HH:mm");
                    }
                    else
                    {
                        lblMessage.Text = "Complaint not found.";
                    }
                    con.Close();
                }
            }
        }

        private void LoadComments(long complaintId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT cc.Comment, cc.CreatedAt, u.Username
                    FROM ComplaintComments cc
                    INNER JOIN Users u ON cc.UserId = u.UserId
                    WHERE cc.ComplaintId = @ComplaintId
                    ORDER BY cc.CreatedAt ASC";

                using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                {
                    da.SelectCommand.Parameters.AddWithValue("@ComplaintId", complaintId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    rptComments.DataSource = dt;
                    rptComments.DataBind();
                }
            }
        }

        protected void btnAddComment_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtComment.Text))
            {
                lblMessage.Text = "Please enter a comment.";
                return;
            }

            if (Request.QueryString["id"] == null)
            {
                lblMessage.Text = "Invalid complaint.";
                return;
            }

            long complaintId = Convert.ToInt64(Request.QueryString["id"]);
            long userId = Convert.ToInt64(Session["UserId"]);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO ComplaintComments (ComplaintId, UserId, Comment)
                    VALUES (@ComplaintId, @UserId, @Comment)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ComplaintId", complaintId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Comment", txtComment.Text.Trim());

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }

            txtComment.Text = string.Empty;
            lblMessage.Text = "Comment added successfully.";
            LoadComments(complaintId);
        }
    }
}
