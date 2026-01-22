using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SocietyManagement.Member.Complaints
{
    public partial class RaiseComplaint : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["MemberId"] == null)
            {
                Response.Redirect("~/Login.aspx");
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();
            string description = txtDescription.Text.Trim();
            string category = ddlCategory.SelectedValue; // assuming you have a dropdown for category
            long userId = Convert.ToInt64(Session["UserId"]);  // from Users table
            long societyId = Convert.ToInt64(Session["SocietyId"]); // from logged-in user's society
            long? unitId = Session["UnitId"] != null ? Convert.ToInt64(Session["UnitId"]) : (long?)null;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(category))
            {
                Response.Write("<script>alert('Please fill all required fields.');</script>");
                return;
            }

            string constr = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = @"
            INSERT INTO Complaints (SocietyId, RaisedByUserId, UnitId, Category, Title, Description, Status, CreatedAt)
            VALUES (@SocietyId, @RaisedByUserId, @UnitId, @Category, @Title, @Description, 'Open', GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SocietyId", societyId);
                    cmd.Parameters.AddWithValue("@RaisedByUserId", userId);
                    cmd.Parameters.AddWithValue("@Category", category);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@UnitId", (object)unitId ?? DBNull.Value);

                    con.Open();
                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        Response.Write("<script>alert('Complaint submitted successfully!');</script>");
                        txtTitle.Text = "";
                        txtDescription.Text = "";
                        ddlCategory.SelectedIndex = 0;
                    }
                    else
                    {
                        Response.Write("<script>alert('Failed to submit complaint. Please try again.');</script>");
                    }
                }
            }
        }

    }
}