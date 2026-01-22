using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SocietyManagement.Admin.Security
{
    public partial class VisitorsCheckout : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                LoadActiveVisitors();
        }

        private void LoadActiveVisitors()
        {
            string query = @"SELECT GateLogId, VisitorName, VehicleNo, Purpose, CheckIn
                             FROM GateLogs
                             WHERE SocietyId = @SocietyId AND CheckOut IS NULL
                             ORDER BY CheckIn DESC";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@SocietyId", Session["SocietyId"]);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            gvVisitors.DataSource = dt;
            gvVisitors.DataBind();
        }

        protected void gvVisitors_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Checkout")
            {
                string id = e.CommandArgument.ToString();
                string query = "UPDATE GateLogs SET CheckOut = GETDATE() WHERE GateLogId = @Id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                lblMessage.Text = "✅ Visitor checkout marked!";
                LoadActiveVisitors();
            }
        }
    }
}
