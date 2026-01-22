using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SocietyManagement.Member.Complaints
{
    public partial class MyComplaints : System.Web.UI.Page
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
                LoadMyComplaints();
            }
        }

        private void LoadMyComplaints()
        {
            long memberId = Convert.ToInt64(Session["MemberId"]);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        c.ComplaintId,
                        c.Title,
                        c.Description,
                        c.Status,
                        c.CreatedAt
                    FROM Complaints c
                    INNER JOIN Users u ON c.RaisedByUserId = u.UserId
                    WHERE u.MemberId = @MemberId
                    ORDER BY c.ComplaintId DESC";

                using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                {
                    da.SelectCommand.Parameters.AddWithValue("@MemberId", memberId);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvComplaints.DataSource = dt;
                    gvComplaints.DataBind();
                }
            }
        }
    }
}