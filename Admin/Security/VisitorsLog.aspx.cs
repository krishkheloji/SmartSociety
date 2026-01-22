using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace SocietyManagement.Admin.Security
{
    public partial class VisitorsLog : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadVisitorLogs();
        }

        private void LoadVisitorLogs()
        {
            try
            {
                string query = @"
    SELECT 
        S.Name AS SocietyName,
        B.Name AS BuildingName,
        U.UnitNo,
        G.VisitorName,
        G.VehicleNo,
        G.Purpose,
        G.ApprovalStatus,
        G.CheckIn,
        G.CheckOut
    FROM GateLogs G
    LEFT JOIN Units U ON G.UnitId = U.UnitId
    LEFT JOIN Buildings B ON U.BuildingId = B.BuildingId
    LEFT JOIN Societies S ON G.SocietyId = S.SocietyId
    
      Where G.ApprovalStatus IN ('Pending', 'Approved', 'Rejected')
    ORDER BY G.CheckIn DESC";


                using (SqlCommand cmd = new SqlCommand(query, con))
                {


                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvLogs.DataSource = dt;
                    gvLogs.DataBind();
                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('Error loading visitor logs: " + ex.Message.Replace("'", "") + "');</script>");
            }
        }

    }
}