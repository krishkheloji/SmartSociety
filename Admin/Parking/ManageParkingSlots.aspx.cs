using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Parking
{
    public partial class ManageParkingSlots : System.Web.UI.Page
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString);

        protected void Page_Load(object sender, EventArgs e)
        {
            CheckAdminAuth();
            if (!IsPostBack)
            {
                LoadSocieties();
                LoadSlots();
            }
        }
        private void CheckAdminAuth()
        {
            if (Session["Username"] == null || Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                Response.Redirect("~/Login.aspx");
            }
        }

        private void LoadSocieties()
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT SocietyId, Name FROM Societies", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            ddlSociety.DataSource = dt;
            ddlSociety.DataTextField = "Name";
            ddlSociety.DataValueField = "SocietyId";
            ddlSociety.DataBind();
        }

        private void LoadSlots()
        {
            string query = @"SELECT ps.SlotId, ps.SocietyId, s.Name AS SocietyName, ps.Identifier, ps.IsCovered
                             FROM ParkingSlots ps
                             INNER JOIN Societies s ON ps.SocietyId = s.SocietyId";
            SqlDataAdapter da = new SqlDataAdapter(query, conn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            gvSlots.DataSource = dt;
            gvSlots.DataBind();
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            SqlCommand cmd = new SqlCommand("INSERT INTO ParkingSlots (SocietyId, Identifier, IsCovered) VALUES (@SocietyId, @Identifier, @IsCovered)", conn);
            cmd.Parameters.AddWithValue("@SocietyId", ddlSociety.SelectedValue);
            cmd.Parameters.AddWithValue("@Identifier", txtIdentifier.Text);
            cmd.Parameters.AddWithValue("@IsCovered", chkIsCovered.Checked ? 1 : 0);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            txtIdentifier.Text = "";
            chkIsCovered.Checked = false;

            LoadSlots();
        }

        protected void gvSlots_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvSlots.EditIndex = e.NewEditIndex;
            LoadSlots();

            // Fill Society dropdown inside the edit row
            DropDownList ddl = (DropDownList)gvSlots.Rows[e.NewEditIndex].FindControl("ddlSocietyEdit");
            SqlDataAdapter da = new SqlDataAdapter("SELECT SocietyId, Name FROM Societies", conn);
            DataTable dt = new DataTable();
            da.Fill(dt);

            ddl.DataSource = dt;
            ddl.DataTextField = "Name";
            ddl.DataValueField = "SocietyId";
            ddl.DataBind();

            // Pre-select current society
            string currentSocietyId = ((DataRowView)gvSlots.Rows[e.NewEditIndex].DataItem)["SocietyId"].ToString();
            ddl.SelectedValue = currentSocietyId;
        }

        protected void gvSlots_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvSlots.EditIndex = -1;
            LoadSlots();
        }

        protected void gvSlots_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int slotId = Convert.ToInt32(gvSlots.DataKeys[e.RowIndex].Value);

            DropDownList ddlSocietyEdit = (DropDownList)gvSlots.Rows[e.RowIndex].FindControl("ddlSocietyEdit");
            string societyId = ddlSocietyEdit.SelectedValue;

            string identifier = ((TextBox)gvSlots.Rows[e.RowIndex].Cells[2].Controls[0]).Text;
            CheckBox chk = (CheckBox)gvSlots.Rows[e.RowIndex].Cells[3].Controls[0];
            bool isCovered = chk.Checked;

            SqlCommand cmd = new SqlCommand("UPDATE ParkingSlots SET SocietyId=@SocietyId, Identifier=@Identifier, IsCovered=@IsCovered WHERE SlotId=@SlotId", conn);
            cmd.Parameters.AddWithValue("@SocietyId", societyId);
            cmd.Parameters.AddWithValue("@Identifier", identifier);
            cmd.Parameters.AddWithValue("@IsCovered", isCovered ? 1 : 0);
            cmd.Parameters.AddWithValue("@SlotId", slotId);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            gvSlots.EditIndex = -1;
            LoadSlots();
        }

        protected void gvSlots_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int slotId = Convert.ToInt32(gvSlots.DataKeys[e.RowIndex].Value);
            SqlCommand cmd = new SqlCommand("DELETE FROM ParkingSlots WHERE SlotId=@SlotId", conn);
            cmd.Parameters.AddWithValue("@SlotId", slotId);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            LoadSlots();
        }


    }
}