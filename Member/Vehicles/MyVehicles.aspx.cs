using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Member.Vehicles
{
    public partial class MyVehicles : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            CheckMemberAuth();

            if (!IsPostBack)
            {
                LoadMyVehicles();
            }
        }

        private void CheckMemberAuth()
        {
            if (Session["Username"] == null || Session["Role"] == null || Session["Role"].ToString() != "Member")
            {
                Response.Redirect("~/Login.aspx");
            }
        }

        private void LoadMyVehicles()
        {
            long memberId = Convert.ToInt64(Session["MemberId"]);

            string query = @"
                SELECT v.VehicleId, v.RegistrationNo, v.Type, u.UnitNo
                FROM Vehicles v
                INNER JOIN Units u ON v.UnitId = u.UnitId
                WHERE v.MemberId = @MemberId";

            SqlDataAdapter da = new SqlDataAdapter(query, conn);
            da.SelectCommand.Parameters.AddWithValue("@MemberId", memberId);

            DataTable dt = new DataTable();
            da.Fill(dt);
            gvMyVehicles.DataSource = dt;
            gvMyVehicles.DataBind();
        }

        protected void gvMyVehicles_RowDeleting(object sender, System.Web.UI.WebControls.GridViewDeleteEventArgs e)
        {
            int vehicleId = Convert.ToInt32(gvMyVehicles.DataKeys[e.RowIndex].Value);

            SqlCommand cmd = new SqlCommand("DELETE FROM Vehicles WHERE VehicleId=@VehicleId", conn);
            cmd.Parameters.AddWithValue("@VehicleId", vehicleId);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            lblMessage.ForeColor = System.Drawing.Color.Green;
            lblMessage.Text = "Vehicle deleted successfully.";
            LoadMyVehicles();
        }


    }
}