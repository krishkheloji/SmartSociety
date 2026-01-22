using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

namespace SocietyManagement.Member.Profile
{
    public partial class ViewProfile : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadProfile();
            }
        }

        private void LoadProfile()
        {
            string username = Session["Username"]?.ToString();
            if (string.IsNullOrEmpty(username))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        M.MemberId,
                        M.FullName,
                        M.Email,
                        M.Phone,
                        M.Status,
                        U.Username
                    FROM Users U
                    LEFT JOIN Members M ON U.MemberId = M.MemberId
                    WHERE U.Username = @Username";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Username", username);
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    hfMemberId.Value = dr["MemberId"].ToString();

                    txtFullName.Text = dr["FullName"]?.ToString() ?? "";
                    txtEmail.Text = dr["Email"]?.ToString() ?? "";
                    txtPhone.Text = dr["Phone"]?.ToString() ?? "";
                    lblUsername.Text = dr["Username"]?.ToString() ?? "";
                    lblStatus.Text = dr["Status"]?.ToString() ?? "";

                    // Also prefill Edit controls for future use
                    txtEditFullName.Text = txtFullName.Text;
                    txtEditEmail.Text = txtEmail.Text;
                    txtEditPhone.Text = txtPhone.Text;
                }
                else
                {
                    lblMessage.CssClass = "text-danger fw-bold";
                    lblMessage.Text = "Profile not found.";
                }

                dr.Close();
            }
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            // Switch to Edit mode and prefill fields
            pnlView.Visible = false;
            pnlEdit.Visible = true;

            txtEditFullName.Text = txtFullName.Text;
            txtEditEmail.Text = txtEmail.Text;
            txtEditPhone.Text = txtPhone.Text;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            // Cancel editing and reload original data
            pnlEdit.Visible = false;
            pnlView.Visible = true;
            LoadProfile();
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfMemberId.Value))
            {
                lblMessage.CssClass = "text-danger fw-bold";
                lblMessage.Text = "Invalid member ID.";
                return;
            }

            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
                    UPDATE Members
                    SET FullName = @FullName,
                        Email = @Email,
                        Phone = @Phone
                    WHERE MemberId = @MemberId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FullName", txtEditFullName.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", txtEditEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@Phone", txtEditPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@MemberId", hfMemberId.Value);

                con.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    lblMessage.CssClass = "text-success fw-bold";
                    lblMessage.Text = "Profile updated successfully!";
                }
                else
                {
                    lblMessage.CssClass = "text-danger fw-bold";
                    lblMessage.Text = "Update failed.";
                }

                pnlEdit.Visible = false;
                pnlView.Visible = true;

                LoadProfile(); // Refresh data
            }
        }
    }
}
