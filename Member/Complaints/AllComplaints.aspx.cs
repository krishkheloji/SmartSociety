using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SocietyManagement.Member.Complaints
{
    public partial class AllComplaints : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MemberId"] == null || Session["SocietyId"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadAllComplaints();
            }
        }

        private void LoadAllComplaints()
        {
            long societyId = Convert.ToInt64(Session["SocietyId"]);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        c.ComplaintId,
                        c.Title,
                        c.Category,
                        c.Status,
                        c.CreatedAt,
                        ISNULL(m.FullName, 'Admin') AS RaisedBy
                    FROM Complaints c
                    INNER JOIN Societies s ON c.SocietyId = s.SocietyId
                    LEFT JOIN Users u ON c.RaisedByUserId = u.UserId
                    LEFT JOIN Members m ON u.MemberId = m.MemberId
                    WHERE c.SocietyId = @SocietyId
                    ORDER BY c.ComplaintId DESC";

                using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                {
                    da.SelectCommand.Parameters.AddWithValue("@SocietyId", societyId);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvComplaints.DataSource = dt;
                    gvComplaints.DataBind();
                }
            }
        }
    }
}
