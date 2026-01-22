using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Society
{
    public partial class ManageBuilding : System.Web.UI.Page
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
                LoadBuildings();
            }
        }

        // Load societies into dropdown
        private void LoadSocieties()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT SocietyId, Name FROM Societies ORDER BY Name";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

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

        // Load all buildings
        private void LoadBuildings()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"SELECT b.BuildingId, b.Name, b.Floors, s.Name AS SocietyName 
                               FROM Buildings b 
                               INNER JOIN Societies s ON b.SocietyId = s.SocietyId 
                               ORDER BY s.Name, b.Name";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    gvBuildings.DataSource = dt;
                    gvBuildings.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading buildings: " + ex.Message, false);
            }
        }

        // Load buildings for selected society
        protected void ddlSociety_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlSociety.SelectedValue != "0")
            {
                LoadBuildingsBySociety(Convert.ToInt32(ddlSociety.SelectedValue));
            }
            else
            {
                LoadBuildings();
            }
        }

        private void LoadBuildingsBySociety(int societyId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = @"SELECT b.BuildingId, b.Name, b.Floors, s.Name AS SocietyName 
                               FROM Buildings b 
                               INNER JOIN Societies s ON b.SocietyId = s.SocietyId 
                               WHERE b.SocietyId = @SocietyId
                               ORDER BY b.Name";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    gvBuildings.DataSource = dt;
                    gvBuildings.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading buildings: " + ex.Message, false);
            }
        }

        // Save or Update Building
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int buildingId = Convert.ToInt32(hfBuildingId.Value);
                int societyId = Convert.ToInt32(ddlSociety.SelectedValue);
                string name = txtBuildingName.Text.Trim();
                int floors = Convert.ToInt32(txtFloors.Text.Trim());

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    string query;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    if (buildingId == 0)
                    {
                        // Insert new building
                        query = @"INSERT INTO Buildings (SocietyId, Name, Floors) 
                             VALUES (@SocietyId, @Name, @Floors)";
                    }
                    else
                    {
                        // Update existing building
                        query = @"UPDATE Buildings 
                             SET SocietyId = @SocietyId, Name = @Name, Floors = @Floors 
                             WHERE BuildingId = @BuildingId";
                        cmd.Parameters.AddWithValue("@BuildingId", buildingId);
                    }

                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Floors", floors);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowMessage(buildingId == 0 ? "Building added successfully!" : "Building updated successfully!", true);
                        ClearForm();
                        LoadBuildings();
                    }
                    else
                    {
                        ShowMessage("Failed to save building.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error: " + ex.Message, false);
            }
        }

        // Handle GridView Commands
        protected void gvBuildings_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int buildingId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditBuilding")
            {
                LoadBuildingForEdit(buildingId);
            }
            else if (e.CommandName == "DeleteBuilding")
            {
                DeleteBuilding(buildingId);
            }
        }

        // Load building for editing
        private void LoadBuildingForEdit(int buildingId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT * FROM Buildings WHERE BuildingId = @BuildingId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@BuildingId", buildingId);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        hfBuildingId.Value = reader["BuildingId"].ToString();
                        ddlSociety.SelectedValue = reader["SocietyId"].ToString();
                        txtBuildingName.Text = reader["Name"].ToString();
                        txtFloors.Text = reader["Floors"].ToString();
                        btnSave.Text = "Update Building";
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading building: " + ex.Message, false);
            }
        }

        // Delete building
        private void DeleteBuilding(int buildingId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "DELETE FROM Buildings WHERE BuildingId = @BuildingId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@BuildingId", buildingId);

                    conn.Open();
                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowMessage("Building deleted successfully!", true);
                        LoadBuildings();
                    }
                    else
                    {
                        ShowMessage("Failed to delete building.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting building: " + ex.Message, false);
            }
        }

        // Clear form
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfBuildingId.Value = "0";
            ddlSociety.SelectedIndex = 0;
            txtBuildingName.Text = "";
            txtFloors.Text = "";
            btnSave.Text = "Save Building";
            lblMessage.Visible = false;
        }

        // Display messages
        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isSuccess ? "message success" : "message error";
            lblMessage.Visible = true;
        }
    }
}