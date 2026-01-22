using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Admin.Complaints
{
    public partial class AddComplaints : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // You can add dropdowns for Category, SocietyId, or Users if needed
            }
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrEmpty(description))
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "Please enter a complaint description.";
                return;
            }

            // Example values (replace with actual session or dropdown values)
            long societyId = 1;           // e.g. from Session["SocietyId"]
            long raisedByUserId = 1;      // e.g. from Session["UserId"]
            long? unitId = null;          // Optional
            string category = "General";  // or from dropdown
            string title = "Complaint";   // optional field

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Complaints 
                                 (SocietyId, RaisedByUserId, UnitId, Category, Title, Description, Status, CreatedAt)
                                 VALUES (@SocietyId, @RaisedByUserId, @UnitId, @Category, @Title, @Description, 'Open', GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);
                    cmd.Parameters.AddWithValue("@RaisedByUserId", raisedByUserId);
                    cmd.Parameters.AddWithValue("@UnitId", (object)unitId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Category", category);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Description", description);

                    try
                    {
                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            lblMessage.ForeColor = System.Drawing.Color.Green;
                            lblMessage.Text = "Complaint added successfully!";
                            txtDescription.Text = "";
                        }
                        else
                        {
                            lblMessage.ForeColor = System.Drawing.Color.Red;
                            lblMessage.Text = "Failed to add complaint.";
                        }
                    }
                    catch (Exception ex)
                    {
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        lblMessage.Text = "Error: " + ex.Message;
                    }
                }
            }
        }
    }
}