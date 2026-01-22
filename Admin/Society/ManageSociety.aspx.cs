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
    public partial class ManageSociety : System.Web.UI.Page
    {
        // Get connection string from Web.config
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
            }
        }

        // Load all societies into GridView
        private void LoadSocieties()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT SocietyId, Name, AddressLine1, AddressLine2, City, State, Pincode FROM Societies ORDER BY Name";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    gvSocieties.DataSource = dt;
                    gvSocieties.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading societies: " + ex.Message, false);
            }
        }

        // Save or Update Society
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int societyId = Convert.ToInt32(hfSocietyId.Value);
                string name = txtName.Text.Trim();
                string address1 = txtAddressLine1.Text.Trim();
                string address2 = txtAddressLine2.Text.Trim();
                string city = txtCity.Text.Trim();
                string state = txtState.Text.Trim();
                string pincode = txtPincode.Text.Trim();

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    string query;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    if (societyId == 0)
                    {
                        // Insert new society
                        query = @"INSERT INTO Societies (Name, AddressLine1, AddressLine2, City, State, Pincode) 
                             VALUES (@Name, @Address1, @Address2, @City, @State, @Pincode)";
                    }
                    else
                    {
                        // Update existing society
                        query = @"UPDATE Societies 
                             SET Name = @Name, AddressLine1 = @Address1, AddressLine2 = @Address2, 
                                 City = @City, State = @State, Pincode = @Pincode, UpdatedAt = GETDATE() 
                             WHERE SocietyId = @SocietyId";
                        cmd.Parameters.AddWithValue("@SocietyId", societyId);
                    }

                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Address1", address1);
                    cmd.Parameters.AddWithValue("@Address2", string.IsNullOrEmpty(address2) ? (object)DBNull.Value : address2);
                    cmd.Parameters.AddWithValue("@City", city);
                    cmd.Parameters.AddWithValue("@State", state);
                    cmd.Parameters.AddWithValue("@Pincode", pincode);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowMessage(societyId == 0 ? "Society added successfully!" : "Society updated successfully!", true);
                        ClearForm();
                        LoadSocieties();
                    }
                    else
                    {
                        ShowMessage("Failed to save society.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error: " + ex.Message, false);
            }
        }

        // Handle GridView Commands (Edit/Delete)
        protected void gvSocieties_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int societyId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditSociety")
            {
                LoadSocietyForEdit(societyId);
            }
            else if (e.CommandName == "DeleteSociety")
            {
                DeleteSociety(societyId);
            }
        }

        // Load society data for editing
        private void LoadSocietyForEdit(int societyId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT * FROM Societies WHERE SocietyId = @SocietyId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        hfSocietyId.Value = reader["SocietyId"].ToString();
                        txtName.Text = reader["Name"].ToString();
                        txtAddressLine1.Text = reader["AddressLine1"].ToString();
                        txtAddressLine2.Text = reader["AddressLine2"].ToString();
                        txtCity.Text = reader["City"].ToString();
                        txtState.Text = reader["State"].ToString();
                        txtPincode.Text = reader["Pincode"].ToString();
                        btnSave.Text = "Update Society";
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading society: " + ex.Message, false);
            }
        }

        // Delete society
        private void DeleteSociety(int societyId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "DELETE FROM Societies WHERE SocietyId = @SocietyId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);

                    conn.Open();
                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowMessage("Society deleted successfully!", true);
                        LoadSocieties();
                    }
                    else
                    {
                        ShowMessage("Failed to delete society.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting society: " + ex.Message, false);
            }
        }

        // Clear form
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfSocietyId.Value = "0";
            txtName.Text = "";
            txtAddressLine1.Text = "";
            txtAddressLine2.Text = "";
            txtCity.Text = "";
            txtState.Text = "";
            txtPincode.Text = "";
            btnSave.Text = "Save Society";
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