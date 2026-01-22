using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Vendors
{
    public partial class Vendors : System.Web.UI.Page
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
                LoadVendors();
            }
        }

        // Load all vendors into GridView
        private void LoadVendors()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT VendorId, Name, Phone, Email FROM Vendors ORDER BY Name";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    gvVendors.DataSource = dt;
                    gvVendors.DataBind();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading vendors: " + ex.Message, false);
            }
        }

        // Save or Update Vendor
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int vendorId = Convert.ToInt32(hfVendorId.Value);
                string name = txtVendorName.Text.Trim();
                string phone = txtPhone.Text.Trim();
                string email = txtEmail.Text.Trim();

                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    conn.Open();
                    string query;
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    if (vendorId == 0)
                    {
                        // Insert new vendor
                        query = @"INSERT INTO Vendors (Name, Phone, Email) 
                                 VALUES (@Name, @Phone, @Email)";
                    }
                    else
                    {
                        // Update existing vendor
                        query = @"UPDATE Vendors 
                                 SET Name = @Name, Phone = @Phone, Email = @Email 
                                 WHERE VendorId = @VendorId";
                        cmd.Parameters.AddWithValue("@VendorId", vendorId);
                    }

                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowMessage(vendorId == 0 ? "Vendor added successfully!" : "Vendor updated successfully!", true);
                        ClearForm();
                        LoadVendors();
                    }
                    else
                    {
                        ShowMessage("Failed to save vendor.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error: " + ex.Message, false);
            }
        }

        // Handle GridView Commands (Edit/Delete)
        protected void gvVendors_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int vendorId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditVendor")
            {
                LoadVendorForEdit(vendorId);
            }
            else if (e.CommandName == "DeleteVendor")
            {
                DeleteVendor(vendorId);
            }
        }

        // Load vendor data for editing
        private void LoadVendorForEdit(int vendorId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    string query = "SELECT * FROM Vendors WHERE VendorId = @VendorId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@VendorId", vendorId);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        hfVendorId.Value = reader["VendorId"].ToString();
                        txtVendorName.Text = reader["Name"].ToString();
                        txtPhone.Text = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "";
                        txtEmail.Text = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "";
                        btnSave.Text = "Update Vendor";
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading vendor: " + ex.Message, false);
            }
        }

        // Delete vendor
        private void DeleteVendor(int vendorId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    // Check if vendor is used in any expenses
                    string checkQuery = "SELECT COUNT(*) FROM Expenses WHERE VendorId = @VendorId";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@VendorId", vendorId);

                    conn.Open();
                    int expenseCount = (int)checkCmd.ExecuteScalar();

                    if (expenseCount > 0)
                    {
                        ShowMessage("Cannot delete vendor. This vendor is associated with " + expenseCount + " expense(s).", false);
                        return;
                    }

                    // Delete vendor
                    string query = "DELETE FROM Vendors WHERE VendorId = @VendorId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@VendorId", vendorId);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        ShowMessage("Vendor deleted successfully!", true);
                        LoadVendors();
                    }
                    else
                    {
                        ShowMessage("Failed to delete vendor.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error deleting vendor: " + ex.Message, false);
            }
        }

        // Clear form
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfVendorId.Value = "0";
            txtVendorName.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            btnSave.Text = "Save Vendor";
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