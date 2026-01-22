using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Complaints
{
    public partial class ComplaintDetails : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadComplaints();
        }

        private void LoadComplaints()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        c.ComplaintId,
                        m.FullName AS MemberName,
                        u.UnitNo AS FlatNumber,
                        c.Title,
                        c.Description,
                        c.Status,
                        c.CreatedAt
                    FROM Complaints c
                    LEFT JOIN Users us ON c.RaisedByUserId = us.UserId
                    LEFT JOIN Members m ON us.MemberId = m.MemberId
                    LEFT JOIN Units u ON c.UnitId = u.UnitId
                    ORDER BY c.ComplaintId DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvComplaints.DataSource = dt;
                gvComplaints.DataBind();
            }
        }

        protected void gvComplaints_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewComplaint")
            {
                string complaintId = e.CommandArgument.ToString();
                Response.Redirect($"ViewComplaints.aspx?id={complaintId}");
            }
        }
    }
}