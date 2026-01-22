using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Society
{
    public partial class ManageUnit : System.Web.UI.Page
    {
        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;

            if (!IsPostBack)
            {
                LoadSocieties();
                LoadUnits();
            }
        }

        // Load societies dropdown
        private void LoadSocieties()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT SocietyId, Name FROM Societies ORDER BY Name";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlSociety.DataSource = dt;
                    ddlSociety.DataTextField = "Name";
                    ddlSociety.DataValueField = "SocietyId";
                    ddlSociety.DataBind();
                    ddlSociety.Items.Insert(0, new ListItem("-- Select Society --", "0"));
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading societies: " + ex.Message, false);
            }
        }

        // Load buildings based on selected society
        protected void ddlSociety_SelectedIndexChanged(object sender, EventArgs e)
        {
            int societyId = Convert.ToInt32(ddlSociety.SelectedValue);
            if (societyId > 0)
            {
                LoadBuildings(societyId);
                LoadUnitsBySociety(societyId);
            }
            else
            {
                ddlBuilding.Items.Clear();
                ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", "0"));
                LoadUnits();
            }
        }

        private void LoadBuildings(int societyId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT BuildingId, Name FROM Buildings WHERE SocietyId=@SocietyId ORDER BY Name";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    ddlBuilding.DataSource = dt;
                    ddlBuilding.DataTextField = "Name";
                    ddlBuilding.DataValueField = "BuildingId";
                    ddlBuilding.DataBind();
                    ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", "0"));
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading buildings: " + ex.Message, false);
            }
        }

        // Load all units
        private void LoadUnits()
        {
            LoadUnitsBySociety(0);
        }

        private void LoadUnitsBySociety(int societyId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"
                        SELECT u.UnitId, u.UnitNo, u.FloorNo, u.CarpetAreaSqft, 
                               CASE WHEN u.IsParkingAllocated = 1 THEN 'Yes' ELSE 'No' END AS IsParkingAllocated,
                               b.Name AS BuildingName, s.Name AS SocietyName
                        FROM Units u
                        INNER JOIN Buildings b ON u.BuildingId = b.BuildingId
                        INNER JOIN Societies s ON b.SocietyId = s.SocietyId
                        WHERE (@SocietyId = 0 OR s.SocietyId = @SocietyId)
                        ORDER BY s.Name, b.Name, u.UnitNo";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvUnits.DataSource = dt;
                    gvUnits.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading units: " + ex.Message, false);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (ddlSociety.SelectedValue == "0" || ddlBuilding.SelectedValue == "0")
                {
                    ShowMessage("Please select society and building.", false);
                    return;
                }

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();

                    string query = hfUnitId.Value == "0"
                        ? @"INSERT INTO Units (UnitNo, FloorNo, CarpetAreaSqft, IsParkingAllocated, BuildingId)
                           VALUES (@UnitNo, @FloorNo, @CarpetAreaSqft, @IsParkingAllocated, @BuildingId)"
                        : @"UPDATE Units
                           SET UnitNo=@UnitNo, FloorNo=@FloorNo, CarpetAreaSqft=@CarpetAreaSqft,
                               IsParkingAllocated=@IsParkingAllocated, BuildingId=@BuildingId
                           WHERE UnitId=@UnitId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UnitNo", txtUnitNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@FloorNo", Convert.ToInt32(txtFloorNo.Text.Trim()));
                    cmd.Parameters.AddWithValue("@CarpetAreaSqft", Convert.ToDecimal(txtCarpetArea.Text.Trim()));
                    cmd.Parameters.AddWithValue("@IsParkingAllocated", chkParking.Checked ? 1 : 0);
                    cmd.Parameters.AddWithValue("@BuildingId", Convert.ToInt32(ddlBuilding.SelectedValue));

                    if (hfUnitId.Value != "0")
                        cmd.Parameters.AddWithValue("@UnitId", Convert.ToInt32(hfUnitId.Value));

                    cmd.ExecuteNonQuery();
                    conn.Close();

                    ShowMessage(hfUnitId.Value == "0" ? "Unit added successfully!" : "Unit updated successfully!", true);
                    ClearForm();
                    LoadUnitsBySociety(Convert.ToInt32(ddlSociety.SelectedValue));
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error saving unit: " + ex.Message, false);
            }
        }

        protected void gvUnits_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditUnit")
            {
                int unitId = Convert.ToInt32(e.CommandArgument);
                LoadUnitById(unitId);
            }
            else if (e.CommandName == "DeleteUnit")
            {
                DeleteUnit(Convert.ToInt32(e.CommandArgument));
            }
        }

        private void LoadUnitById(int unitId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT * FROM Units WHERE UnitId = @UnitId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UnitId", unitId);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];
                        hfUnitId.Value = dr["UnitId"].ToString();
                        txtUnitNo.Text = dr["UnitNo"].ToString();
                        txtFloorNo.Text = dr["FloorNo"].ToString();
                        txtCarpetArea.Text = dr["CarpetAreaSqft"].ToString();
                        chkParking.Checked = Convert.ToBoolean(dr["IsParkingAllocated"]);

                        // Load the building dropdown based on society
                        int buildingId = Convert.ToInt32(dr["BuildingId"]);
                        int societyId = GetSocietyIdByBuilding(buildingId);
                        ddlSociety.SelectedValue = societyId.ToString();
                        LoadBuildings(societyId);
                        ddlBuilding.SelectedValue = buildingId.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading unit details: " + ex.Message, false);
            }
        }

        private int GetSocietyIdByBuilding(int buildingId)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT SocietyId FROM Buildings WHERE BuildingId = @BuildingId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BuildingId", buildingId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        private void DeleteUnit(int unitId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "DELETE FROM Units WHERE UnitId = @UnitId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UnitId", unitId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();

                    ShowMessage("Unit deleted successfully!", true);
                    LoadUnitsBySociety(Convert.ToInt32(ddlSociety.SelectedValue));
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting unit: " + ex.Message, false);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfUnitId.Value = "0";
            ddlSociety.SelectedIndex = 0;
            ddlBuilding.Items.Clear();
            ddlBuilding.Items.Insert(0, new ListItem("-- Select Building --", "0"));
            txtUnitNo.Text = "";
            txtFloorNo.Text = "";
            txtCarpetArea.Text = "";
            chkParking.Checked = false;
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = "message " + (isSuccess ? "success" : "error");
        }
    }
}
